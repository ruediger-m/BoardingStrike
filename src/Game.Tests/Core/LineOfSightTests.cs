using BoardingStrike.Core.Fov;
using BoardingStrike.Core.Hex;
using Xunit;

namespace BoardingStrike.Game.Tests.Core;

public sealed class LineOfSightTests
{
    [Fact]
    public void Hex_has_sight_to_itself()
    {
        var map = TestGridMap.Rectangle(3, 3);
        Assert.True(LineOfSight.HasLineOfSight(map, new HexCoord(1, 1), new HexCoord(1, 1)));
    }

    [Fact]
    public void No_sight_to_or_from_an_off_map_hex()
    {
        var map = TestGridMap.Rectangle(3, 3);
        Assert.False(LineOfSight.HasLineOfSight(map, new HexCoord(1, 1), new HexCoord(9, 9)));
        Assert.False(LineOfSight.HasLineOfSight(map, new HexCoord(9, 9), new HexCoord(1, 1)));
    }

    [Fact]
    public void Adjacent_hexes_see_each_other_when_edge_is_open()
    {
        var map = TestGridMap.Rectangle(3, 3);
        Assert.True(LineOfSight.HasLineOfSight(map, new HexCoord(0, 0), new HexCoord(1, 0)));
    }

    [Fact]
    public void Wall_between_adjacent_hexes_blocks_sight()
    {
        var map = TestGridMap.Rectangle(3, 3).AddWall(new HexCoord(0, 0), new HexCoord(1, 0));
        Assert.False(LineOfSight.HasLineOfSight(map, new HexCoord(0, 0), new HexCoord(1, 0)));
    }

    [Fact]
    public void Clear_straight_line_has_sight()
    {
        var map = TestGridMap.Rectangle(3, 5);
        Assert.True(LineOfSight.HasLineOfSight(map, new HexCoord(0, 0), new HexCoord(0, 2)));
    }

    [Fact]
    public void Wall_across_a_straight_line_blocks_it()
    {
        // The vertical center line (0,0)->(0,2) crosses edge {(0,0),(0,1)}
        // squarely at its midpoint, so a wall there must block.
        var map = TestGridMap.Rectangle(3, 5).AddWall(new HexCoord(0, 0), new HexCoord(0, 1));
        Assert.False(LineOfSight.HasLineOfSight(map, new HexCoord(0, 0), new HexCoord(0, 2)));

        // A door further along the same line also blocks when closed (modeled as a wall edge).
        var map2 = TestGridMap.Rectangle(3, 5).AddWall(new HexCoord(0, 1), new HexCoord(0, 2));
        Assert.False(LineOfSight.HasLineOfSight(map2, new HexCoord(0, 0), new HexCoord(0, 2)));
    }

    [Fact]
    public void Open_door_does_not_block_sight()
    {
        // "Open door" == no wall edge present. Sanity that the same geometry is clear.
        var map = TestGridMap.Rectangle(3, 5);
        Assert.True(LineOfSight.HasLineOfSight(map, new HexCoord(0, 0), new HexCoord(0, 2)));
    }

    [Fact]
    public void Diagonal_sight_is_clear_when_unobstructed()
    {
        var map = TestGridMap.Rectangle(3, 3);
        Assert.True(LineOfSight.HasLineOfSight(map, new HexCoord(0, 0), new HexCoord(1, 1)));
    }

    [Fact]
    public void Cannot_see_around_a_corner_conservative_rule()
    {
        // The center line (0,0)->(1,1) passes exactly through the vertex shared
        // by (0,0), (1,0) and (0,1). Per docs/design/line-of-sight-and-doors.md,
        // a wall on EITHER edge meeting that corner blocks sight.
        var blockViaUpper = TestGridMap.Rectangle(3, 3).AddWall(new HexCoord(0, 0), new HexCoord(1, 0));
        Assert.False(LineOfSight.HasLineOfSight(blockViaUpper, new HexCoord(0, 0), new HexCoord(1, 1)));

        var blockViaLower = TestGridMap.Rectangle(3, 3).AddWall(new HexCoord(0, 0), new HexCoord(0, 1));
        Assert.False(LineOfSight.HasLineOfSight(blockViaLower, new HexCoord(0, 0), new HexCoord(1, 1)));
    }

    [Fact]
    public void Sight_is_symmetric_across_many_pairs()
    {
        var map = TestGridMap.Rectangle(6, 6)
            .AddWall(new HexCoord(2, 2), new HexCoord(2, 3))
            .AddWall(new HexCoord(3, 1), new HexCoord(3, 2));

        for (int q1 = 0; q1 < 6; q1++)
        {
            for (int r1 = 0; r1 < 6; r1++)
            {
                for (int q2 = 0; q2 < 6; q2++)
                {
                    for (int r2 = 0; r2 < 6; r2++)
                    {
                        var a = new HexCoord(q1, r1);
                        var b = new HexCoord(q2, r2);
                        Assert.Equal(
                            LineOfSight.HasLineOfSight(map, a, b),
                            LineOfSight.HasLineOfSight(map, b, a));
                    }
                }
            }
        }
    }
}
