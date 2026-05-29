using BoardingStrike.Core.Hex;
using Xunit;

namespace BoardingStrike.Game.Tests.Core;

public sealed class HexCoordTests
{
    [Fact]
    public void Cube_coordinates_sum_to_zero()
    {
        var hex = new HexCoord(3, -5);
        Assert.Equal(0, hex.Q + hex.R + hex.S);
    }

    [Fact]
    public void Distance_to_self_is_zero()
    {
        var hex = new HexCoord(2, 7);
        Assert.Equal(0, hex.DistanceTo(hex));
    }

    [Theory]
    [InlineData(0, 0, 1, 0, 1)]   // one step E
    [InlineData(0, 0, 0, 1, 1)]   // one step SE
    [InlineData(0, 0, -1, 1, 1)]  // one step SW
    [InlineData(0, 0, 2, 0, 2)]
    [InlineData(0, 0, -2, 1, 2)]  // |dq|=2,|dr|=1,|ds|=1 -> (2+1+1)/2 = 2
    [InlineData(0, 0, 3, -1, 3)]
    public void Distance_matches_cube_formula(int aq, int ar, int bq, int br, int expected)
    {
        var a = new HexCoord(aq, ar);
        var b = new HexCoord(bq, br);
        Assert.Equal(expected, a.DistanceTo(b));
    }

    [Fact]
    public void Distance_is_symmetric()
    {
        var a = new HexCoord(1, 2);
        var b = new HexCoord(-4, 3);
        Assert.Equal(a.DistanceTo(b), b.DistanceTo(a));
    }

    [Fact]
    public void Neighbors_are_six_distinct_hexes_each_one_step_away()
    {
        var origin = new HexCoord(0, 0);
        var neighbors = origin.Neighbors().ToList();

        Assert.Equal(6, neighbors.Count);
        Assert.Equal(6, neighbors.Distinct().Count());
        Assert.All(neighbors, n => Assert.Equal(1, origin.DistanceTo(n)));
        Assert.All(neighbors, n => Assert.True(origin.IsAdjacentTo(n)));
    }

    [Theory]
    [InlineData(HexDirection.E, 1, 0)]
    [InlineData(HexDirection.NE, 1, -1)]
    [InlineData(HexDirection.NW, 0, -1)]
    [InlineData(HexDirection.W, -1, 0)]
    [InlineData(HexDirection.SW, -1, 1)]
    [InlineData(HexDirection.SE, 0, 1)]
    public void Neighbor_offsets_match_design_doc_table(HexDirection direction, int dq, int dr)
    {
        var origin = new HexCoord(0, 0);
        Assert.Equal(new HexCoord(dq, dr), origin.Neighbor(direction));
    }

    [Fact]
    public void Tuple_converts_implicitly()
    {
        HexCoord hex = (4, -2);
        Assert.Equal(new HexCoord(4, -2), hex);
    }

    [Fact]
    public void Addition_and_subtraction_compose()
    {
        var a = new HexCoord(2, 3);
        var b = new HexCoord(-1, 5);
        Assert.Equal(a, (a + b) - b);
    }
}
