using BoardingStrike.Core.Data;
using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Pathfinding;
using Xunit;

namespace BoardingStrike.Game.Tests.Data;

/// <summary>
/// Validates the committed Hangar Sweep map (Step 4 exit criterion): the content
/// loads, the spawns sit on floor, every hostile is reachable from every marine
/// spawn with doors open, and each gated room is sealed when its door is closed
/// (which also proves there are no accidental diagonal "leaks" past a door).
/// </summary>
public sealed class HangarSweepMapTests
{
    private static MapDataGraph LoadHangarSweepGraph()
    {
        ContentDatabase db = JsonContentLoader.Load(RepoLocator.GodotDataDir());
        MissionData mission = db.Missions["mission_hangar_sweep"];
        MapData map = db.Maps[mission.MapId!];
        return MapDataGraph.Build(map);
    }

    [Fact]
    public void Hangar_sweep_has_four_marine_slots_and_twelve_hostiles()
    {
        MapDataGraph graph = LoadHangarSweepGraph();

        Assert.Equal(4, graph.MarineSpawns.Count);
        Assert.Equal(12, graph.EnemySpawns.Count);
        Assert.Equal(10, graph.EnemySpawns.Count(e => e.EnemyId == "husk_swarmer"));
        Assert.Equal(2, graph.EnemySpawns.Count(e => e.EnemyId == "cyst_spitter"));
    }

    [Fact]
    public void All_spawns_sit_on_floor()
    {
        MapDataGraph graph = LoadHangarSweepGraph();

        Assert.All(graph.MarineSpawns, hex => Assert.True(graph.Contains(hex), $"marine spawn {hex} is not floor"));
        Assert.All(graph.EnemySpawns, e => Assert.True(graph.Contains(e.Hex), $"enemy spawn {e.Hex} is not floor"));
    }

    [Fact]
    public void With_doors_open_every_hostile_is_reachable_from_every_marine_spawn()
    {
        MapDataGraph graph = LoadHangarSweepGraph();
        graph.ClosedDoors.Clear(); // all doors open

        foreach (HexCoord marine in graph.MarineSpawns)
        {
            foreach ((HexCoord enemy, string id) in graph.EnemySpawns)
            {
                HexPath? path = HexPathfinder.FindPath(graph, marine, enemy);
                Assert.True(path is not null, $"no path from marine {marine} to {id} at {enemy} (doors open)");
            }
        }
    }

    [Theory]
    // door endpoints (the corridor/hangar side, the room side), and a hostile inside the gated room
    [InlineData(5, 2, 5, 3, 5, 3)]     // Closet A
    [InlineData(8, 2, 8, 3, 8, 4)]     // Closet B
    [InlineData(12, 5, 12, 6, 13, 9)]  // Bridge (spitter)
    public void Closed_door_makes_its_room_unreachable(int dq1, int dr1, int dq2, int dr2, int hq, int hr)
    {
        MapDataGraph graph = LoadHangarSweepGraph();
        graph.ClosedDoors.Clear();
        graph.ClosedDoors.Add(graph.Door(new HexCoord(dq1, dr1), new HexCoord(dq2, dr2)));

        var hostile = new HexCoord(hq, hr);
        HexCoord marine = graph.MarineSpawns[0];

        // Sanity: with the door open it *is* reachable...
        Assert.NotNull(HexPathfinder.FindPath(graph_WithAllOpen(), marine, hostile));
        // ...but closing only this door seals the room (no diagonal leak).
        Assert.Null(HexPathfinder.FindPath(graph, marine, hostile));

        static MapDataGraph graph_WithAllOpen()
        {
            MapDataGraph g = LoadHangarSweepGraph();
            g.ClosedDoors.Clear();
            return g;
        }
    }
}
