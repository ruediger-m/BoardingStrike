using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Pathfinding;
using Xunit;

namespace BoardingStrike.Game.Tests.Core;

public sealed class HexPathfinderTests
{
    [Fact]
    public void Path_to_self_is_single_hex_zero_cost()
    {
        var map = TestGridMap.Rectangle(5, 5);
        HexPath? path = HexPathfinder.FindPath(map, new HexCoord(2, 2), new HexCoord(2, 2));

        Assert.NotNull(path);
        Assert.Equal([new HexCoord(2, 2)], path!.Hexes);
        Assert.Equal(0.0, path.Cost);
    }

    [Fact]
    public void Open_grid_path_has_length_equal_to_hex_distance()
    {
        var map = TestGridMap.Rectangle(10, 10);
        var start = new HexCoord(0, 0);
        var goal = new HexCoord(5, 2);

        HexPath? path = HexPathfinder.FindPath(map, start, goal);

        Assert.NotNull(path);
        Assert.Equal(start, path!.Hexes[0]);
        Assert.Equal(goal, path.Hexes[^1]);
        Assert.Equal(start.DistanceTo(goal), path.StepCount);
        Assert.Equal(start.DistanceTo(goal), path.Cost);

        // Every consecutive pair is a single adjacency step.
        for (int i = 1; i < path.Hexes.Count; i++)
        {
            Assert.True(path.Hexes[i - 1].IsAdjacentTo(path.Hexes[i]));
        }
    }

    [Fact]
    public void Returns_null_when_goal_is_blocked()
    {
        var map = TestGridMap.Rectangle(5, 5).BlockHex(new HexCoord(4, 4));
        Assert.Null(HexPathfinder.FindPath(map, new HexCoord(0, 0), new HexCoord(4, 4)));
    }

    [Fact]
    public void Returns_null_when_goal_is_off_map()
    {
        var map = TestGridMap.Rectangle(5, 5);
        Assert.Null(HexPathfinder.FindPath(map, new HexCoord(0, 0), new HexCoord(9, 9)));
    }

    [Fact]
    public void Returns_null_when_goal_is_walled_off_completely()
    {
        // Isolate (1,0) by walling every edge into it.
        var target = new HexCoord(1, 0);
        var map = TestGridMap.Rectangle(6, 6);
        foreach (HexCoord neighbor in target.Neighbors())
        {
            map.AddWall(target, neighbor);
        }

        Assert.Null(HexPathfinder.FindPath(map, new HexCoord(0, 0), target));
    }

    [Fact]
    public void Wall_forces_a_detour_with_higher_cost()
    {
        var start = new HexCoord(0, 0);
        var goal = new HexCoord(1, 0);

        var open = TestGridMap.Rectangle(5, 5);
        HexPath? direct = HexPathfinder.FindPath(open, start, goal);
        Assert.NotNull(direct);
        Assert.Equal(1.0, direct!.Cost);

        var walled = TestGridMap.Rectangle(5, 5).AddWall(start, goal);
        HexPath? detour = HexPathfinder.FindPath(walled, start, goal);
        Assert.NotNull(detour);
        Assert.True(detour!.Cost > direct.Cost);
        // The two hexes still share two common neighbors, so routing around a
        // single walled edge costs just one extra step (cost 2, via e.g. (1,-1)).
        Assert.Equal(2.0, detour.Cost);
        Assert.DoesNotContain(goal, detour.Hexes.Take(detour.Hexes.Count - 1));
    }

    [Fact]
    public void Solves_a_unique_corridor_path()
    {
        // A 1-wide corridor with a single forced route, so the optimal path is
        // unique and we can assert it exactly (also pins determinism).
        var map = new TestGridMap();
        HexCoord[] corridor =
        [
            new(0, 0), new(1, 0), new(1, 1), new(1, 2), new(2, 2), new(3, 2),
        ];
        foreach (HexCoord cell in corridor)
        {
            map.AddCell(cell);
        }

        HexPath? path = HexPathfinder.FindPath(map, corridor[0], corridor[^1]);

        Assert.NotNull(path);
        Assert.Equal(corridor, path!.Hexes);
        Assert.Equal(corridor.Length - 1, path.StepCount);
    }

    [Fact]
    public void Higher_enter_cost_is_avoided_when_cheaper_route_exists()
    {
        // Make the direct neighbor very expensive; expect the path to route around it.
        var start = new HexCoord(0, 0);
        var goal = new HexCoord(2, 0);
        var expensive = new HexCoord(1, 0);

        var map = TestGridMap.Rectangle(5, 5).SetEnterCost(expensive, 100.0);
        HexPath? path = HexPathfinder.FindPath(map, start, goal);

        Assert.NotNull(path);
        Assert.DoesNotContain(expensive, path!.Hexes);
        Assert.True(path.Cost < 100.0);
    }
}
