using BoardingStrike.Core.Data;
using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Pathfinding;

namespace BoardingStrike.Game.Tests.Data;

/// <summary>
/// A lightweight, test-only adapter that turns a <see cref="MapData"/> DTO into
/// an <see cref="IHexGraph"/> so the pathfinder can reason about a map before
/// the real Game-layer board exists (Step 5). Door state is configurable via
/// <see cref="ClosedDoors"/> so tests can check both all-doors-open connectivity
/// and per-door gating. Wall edges always block.
/// </summary>
internal sealed class MapDataGraph : IHexGraph
{
    private readonly HashSet<HexCoord> _floor;
    private readonly HashSet<HexEdge> _walls;

    private MapDataGraph(
        HashSet<HexCoord> floor,
        HashSet<HexEdge> walls,
        HashSet<HexEdge> doors,
        IReadOnlyList<HexCoord> marineSpawns,
        IReadOnlyList<(HexCoord Hex, string EnemyId)> enemySpawns)
    {
        _floor = floor;
        _walls = walls;
        Doors = doors;
        MarineSpawns = marineSpawns;
        EnemySpawns = enemySpawns;
    }

    /// <summary>All door edges on the map.</summary>
    public IReadOnlySet<HexEdge> Doors { get; }

    /// <summary>Doors currently treated as closed (blocking). Mutated by tests.</summary>
    public HashSet<HexEdge> ClosedDoors { get; } = [];

    public IReadOnlyList<HexCoord> MarineSpawns { get; }

    public IReadOnlyList<(HexCoord Hex, string EnemyId)> EnemySpawns { get; }

    public IReadOnlyCollection<HexCoord> FloorCells => _floor;

    public static MapDataGraph Build(MapData map)
    {
        // Resolve every cell's kind: default, then explicit hex overrides, then void overrides.
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
        var doors = new HashSet<HexEdge>();
        foreach (EdgeData e in map.Edges ?? [])
        {
            var edge = new HexEdge(new HexCoord(e.A![0], e.A[1]), new HexCoord(e.B![0], e.B[1]));
            switch (e.Kind)
            {
                case "wall":
                    walls.Add(edge);
                    break;
                case "door":
                    doors.Add(edge);
                    break;
            }
        }

        var marineSpawns = new List<(int Slot, HexCoord Hex)>();
        var enemySpawns = new List<(HexCoord, string)>();
        foreach (SpawnPointData s in map.SpawnPoints ?? [])
        {
            var hex = new HexCoord(s.Hex![0], s.Hex[1]);
            switch (s.Kind)
            {
                case "marine_slot":
                    marineSpawns.Add((s.Slot ?? 0, hex));
                    break;
                case "enemy":
                    enemySpawns.Add((hex, s.EnemyId!));
                    break;
            }
        }

        var orderedMarines = marineSpawns.OrderBy(m => m.Slot).Select(m => m.Hex).ToList();
        return new MapDataGraph(floor, walls, doors, orderedMarines, enemySpawns);
    }

    /// <summary>Finds a door edge by its two endpoints (order-independent).</summary>
    public HexEdge Door(HexCoord a, HexCoord b)
    {
        var edge = new HexEdge(a, b);
        if (!Doors.Contains(edge))
        {
            throw new ArgumentException($"No door edge {edge} on this map.");
        }

        return edge;
    }

    public bool Contains(HexCoord hex) => _floor.Contains(hex);

    public bool IsBlocked(HexCoord hex) => false;

    public bool BlocksMovement(HexCoord from, HexCoord to)
    {
        var edge = new HexEdge(from, to);
        return _walls.Contains(edge) || ClosedDoors.Contains(edge);
    }

    public double EnterCost(HexCoord hex) => 1.0;
}
