using BoardingStrike.Core.Data;
using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Rng;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Combat;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Scenario;

/// <summary>
/// The root mutable rules state for a running scenario: the board, the squad,
/// the hostiles (with their per-type AI decks), the shared hostile modifier
/// deck, and the seeded RNG. Built from a <see cref="ContentCatalog"/> and a
/// mission id.
/// </summary>
public sealed class ScenarioState
{
    private ScenarioState(MissionDef mission, BoardState board, IRandomSource rng)
    {
        Mission = mission;
        Board = board;
        Rng = rng;
    }

    public MissionDef Mission { get; }

    public BoardState Board { get; }

    public IRandomSource Rng { get; }

    /// <summary>Marines in commit (spawn-slot) order.</summary>
    public List<Marine> Marines { get; } = [];

    /// <summary>Hostiles in spawn order.</summary>
    public List<Hostile> Hostiles { get; } = [];

    /// <summary>AI draw pile per enemy type id.</summary>
    public Dictionary<string, AiDeckPile> AiDecks { get; } = [];

    public int Round { get; set; }

    public ScenarioStatus Status { get; set; } = ScenarioStatus.InProgress;

    public bool AnyHostileAlive => Hostiles.Any(h => h.IsAlive);

    public bool AnyMarineActive => Marines.Any(m => m.IsActive);

    /// <summary>Enemy type ids that still have a living member, sorted for deterministic turn order.</summary>
    public IEnumerable<string> LivingEnemyTypeIds =>
        Hostiles.Where(h => h.IsAlive).Select(h => h.Enemy.Id).Distinct().OrderBy(id => id, StringComparer.Ordinal);

    public static ScenarioState Build(ContentCatalog catalog, string missionId, ulong? seedOverride = null, string marineClassId = "boarding_marine")
    {
        ArgumentNullException.ThrowIfNull(catalog);
        MissionDef mission = catalog.Missions[missionId];
        MapData map = catalog.Maps[mission.MapId];
        BoardState board = BoardState.FromMap(map);
        IRandomSource rng = CreateRng(mission, seedOverride);

        var state = new ScenarioState(mission, board, rng);
        ClassDef squadClass = catalog.Classes[marineClassId];

        // A single shared hostile modifier deck (docs/design/combat.md).
        ModifierDeck? hostileModifiers = null;

        int spawnIndex = 0;
        foreach (SpawnPointData spawn in map.SpawnPoints ?? [])
        {
            var hex = new HexCoord(spawn.Hex![0], spawn.Hex[1]);
            switch (spawn.Kind)
            {
                case "marine_slot":
                {
                    int slot = spawn.Slot ?? state.Marines.Count + 1;
                    var marine = new Marine($"marine_{slot}", $"Marine {slot}", squadClass, hex, squadClass.Modifiers.CreateDeck(rng));
                    state.Marines.Add(marine);
                    board.PlaceUnit(marine);
                    break;
                }

                case "enemy":
                {
                    EnemyDef def = catalog.Enemies[spawn.EnemyId!];
                    hostileModifiers ??= def.Modifiers.CreateDeck(rng);
                    var hostile = new Hostile($"hostile_{spawnIndex}", def, hex, hostileModifiers, spawnIndex);
                    spawnIndex++;
                    state.Hostiles.Add(hostile);
                    board.PlaceUnit(hostile);
                    if (!state.AiDecks.ContainsKey(def.Id))
                    {
                        state.AiDecks[def.Id] = new AiDeckPile(def.AiDeck, rng);
                    }

                    break;
                }
            }
        }

        // Marines act in slot order.
        state.Marines.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
        return state;
    }

    private static IRandomSource CreateRng(MissionDef mission, ulong? seedOverride)
    {
        if (seedOverride is ulong seed)
        {
            return new DeterministicRng(seed);
        }

        return mission.RngSeedSource switch
        {
            null or "scenario_id" => DeterministicRng.FromString(mission.Id),
            "random" => new DeterministicRng((ulong)Guid.NewGuid().GetHashCode()),
            string s when long.TryParse(s, out long literal) => new DeterministicRng(unchecked((ulong)literal)),
            _ => DeterministicRng.FromString(mission.Id),
        };
    }
}
