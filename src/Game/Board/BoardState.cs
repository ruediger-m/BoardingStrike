using BoardingStrike.Core.Data;
using BoardingStrike.Core.Fov;
using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Pathfinding;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Board;

/// <summary>
/// The live playfield: floor cells, wall edges, mutable doors, and unit
/// occupancy. Implements the Core pathfinding/sight interfaces so A* and
/// line-of-sight operate directly on the current state (closed doors and other
/// units block movement; closed doors and walls block sight; units do not).
/// </summary>
public sealed class BoardState : IHexGraph, ISightMap
{
    private readonly HashSet<HexCoord> _floor;
    private readonly HashSet<HexEdge> _walls;
    private readonly Dictionary<HexEdge, bool> _doorOpen; // value = is open
    private readonly Dictionary<HexCoord, Unit> _occupancy = [];

    private BoardState(HashSet<HexCoord> floor, HashSet<HexEdge> walls, Dictionary<HexEdge, bool> doorOpen)
    {
        _floor = floor;
        _walls = walls;
        _doorOpen = doorOpen;
    }

    public IReadOnlyCollection<HexCoord> FloorCells => _floor;

    public IReadOnlyCollection<HexEdge> Walls => _walls;

    public IReadOnlyCollection<HexEdge> Doors => _doorOpen.Keys;

    /// <summary>Builds the board geometry (cells, walls, doors) from map content.</summary>
    public static BoardState FromMap(MapData map)
    {
        ArgumentNullException.ThrowIfNull(map);

        string defaultKind = map.DefaultHexKind ?? "floor";
        var kinds = new Dictionary<HexCoord, string>();

        BoundsData? b = map.Bounds;
        if (b is { QMin: not null, QMax: not null, RMin: not null, RMax: not null })
        {
            for (int q = b.QMin.Value; q <= b.QMax.Value; q++)
            {
                for (int r = b.RMin.Value; r <= b.RMax.Value; r++)
                {
                    kinds[new HexCoord(q, r)] = defaultKind;
                }
            }
        }

        foreach (HexData h in map.Hexes ?? [])
        {
            kinds[new HexCoord(h.Q ?? 0, h.R ?? 0)] = h.Kind ?? defaultKind;
        }

        foreach (CoordData v in map.VoidHexes ?? [])
        {
            kinds[new HexCoord(v.Q ?? 0, v.R ?? 0)] = "void";
        }

        var floor = new HashSet<HexCoord>(kinds.Where(kv => kv.Value == "floor").Select(kv => kv.Key));

        var walls = new HashSet<HexEdge>();
        var doors = new Dictionary<HexEdge, bool>();
        foreach (EdgeData e in map.Edges ?? [])
        {
            var edge = new HexEdge(new HexCoord(e.A![0], e.A[1]), new HexCoord(e.B![0], e.B[1]));
            switch (e.Kind)
            {
                case "wall":
                    walls.Add(edge);
                    break;
                case "door":
                    doors[edge] = e.InitialState == "open";
                    break;
            }
        }

        return new BoardState(floor, walls, doors);
    }

    // ----- units -----

    public void PlaceUnit(Unit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _occupancy[unit.Position] = unit;
    }

    public void MoveUnit(Unit unit, HexCoord to)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _occupancy.Remove(unit.Position);
        unit.Position = to;
        _occupancy[to] = unit;
    }

    public void RemoveUnit(Unit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (_occupancy.TryGetValue(unit.Position, out Unit? occupant) && ReferenceEquals(occupant, unit))
        {
            _occupancy.Remove(unit.Position);
        }
    }

    public Unit? UnitAt(HexCoord hex) => _occupancy.GetValueOrDefault(hex);

    public bool IsOccupied(HexCoord hex) => _occupancy.ContainsKey(hex);

    // ----- doors -----

    public bool HasDoor(HexCoord a, HexCoord b) => _doorOpen.ContainsKey(new HexEdge(a, b));

    public bool IsDoorOpen(HexCoord a, HexCoord b) => _doorOpen.TryGetValue(new HexEdge(a, b), out bool open) && open;

    public void SetDoor(HexEdge edge, bool open)
    {
        if (!_doorOpen.ContainsKey(edge))
        {
            throw new ArgumentException($"No door on edge {edge}.", nameof(edge));
        }

        _doorOpen[edge] = open;
    }

    /// <summary>Toggles a door and returns its new open state.</summary>
    public bool ToggleDoor(HexEdge edge)
    {
        if (!_doorOpen.TryGetValue(edge, out bool open))
        {
            throw new ArgumentException($"No door on edge {edge}.", nameof(edge));
        }

        _doorOpen[edge] = !open;
        return !open;
    }

    private bool EdgeBlocks(HexEdge edge) =>
        _walls.Contains(edge) || (_doorOpen.TryGetValue(edge, out bool open) && !open);

    // ----- IHexGraph -----

    public bool Contains(HexCoord hex) => _floor.Contains(hex);

    public bool IsBlocked(HexCoord hex) => _occupancy.ContainsKey(hex);

    public bool BlocksMovement(HexCoord from, HexCoord to) => EdgeBlocks(new HexEdge(from, to));

    public double EnterCost(HexCoord hex) => 1.0;

    // ----- ISightMap -----

    public bool BlocksSight(HexCoord a, HexCoord b) => EdgeBlocks(new HexEdge(a, b));
}
