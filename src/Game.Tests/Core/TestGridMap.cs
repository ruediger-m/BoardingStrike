using BoardingStrike.Core.Fov;
using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Pathfinding;

namespace BoardingStrike.Game.Tests.Core;

/// <summary>
/// A small in-memory hex map for exercising the Core algorithms. Implements both
/// the movement graph and the sight map. Wall edges block movement and sight;
/// "blocked" hexes are impassable (e.g. occupied). A door is modeled simply as a
/// wall edge when closed and an absent edge when open.
/// </summary>
internal sealed class TestGridMap : IHexGraph, ISightMap
{
    private readonly HashSet<HexCoord> _cells = [];
    private readonly HashSet<HexCoord> _blockedHexes = [];
    private readonly HashSet<HexEdge> _wallEdges = [];
    private readonly Dictionary<HexCoord, double> _enterCosts = [];

    /// <summary>Builds a rectangular block of hexes: q in [0,width), r in [0,height).</summary>
    public static TestGridMap Rectangle(int width, int height)
    {
        var map = new TestGridMap();
        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                map._cells.Add(new HexCoord(q, r));
            }
        }

        return map;
    }

    public TestGridMap AddCell(HexCoord hex)
    {
        _cells.Add(hex);
        return this;
    }

    public TestGridMap BlockHex(HexCoord hex)
    {
        _blockedHexes.Add(hex);
        return this;
    }

    public TestGridMap AddWall(HexCoord a, HexCoord b)
    {
        _wallEdges.Add(new HexEdge(a, b));
        return this;
    }

    public TestGridMap SetEnterCost(HexCoord hex, double cost)
    {
        _enterCosts[hex] = cost;
        return this;
    }

    // IHexGraph + ISightMap
    public bool Contains(HexCoord hex) => _cells.Contains(hex);

    public bool IsBlocked(HexCoord hex) => _blockedHexes.Contains(hex);

    public bool BlocksMovement(HexCoord from, HexCoord to) => _wallEdges.Contains(new HexEdge(from, to));

    public bool BlocksSight(HexCoord a, HexCoord b) => _wallEdges.Contains(new HexEdge(a, b));

    public double EnterCost(HexCoord hex) => _enterCosts.TryGetValue(hex, out double cost) ? cost : 1.0;
}
