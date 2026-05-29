using BoardingStrike.Core.Hex;
using Xunit;

namespace BoardingStrike.Game.Tests.Core;

public sealed class HexEdgeTests
{
    [Fact]
    public void Edge_is_undirected()
    {
        var a = new HexCoord(0, 0);
        var b = new HexCoord(1, 0);

        Assert.Equal(new HexEdge(a, b), new HexEdge(b, a));
        Assert.Equal(new HexEdge(a, b).GetHashCode(), new HexEdge(b, a).GetHashCode());
    }

    [Fact]
    public void Edge_works_as_set_key_regardless_of_order()
    {
        var set = new HashSet<HexEdge>
        {
            new(new HexCoord(2, 2), new HexCoord(2, 3)),
        };

        Assert.Contains(new HexEdge(new HexCoord(2, 3), new HexCoord(2, 2)), set);
    }

    [Fact]
    public void Non_adjacent_hexes_cannot_form_an_edge()
    {
        Assert.Throws<ArgumentException>(() => new HexEdge(new HexCoord(0, 0), new HexCoord(2, 0)));
    }
}
