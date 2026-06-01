using BoardingStrike.Game.Content;
using BoardingStrike.Game.Tests.Data;

namespace BoardingStrike.Game.Tests.Engine;

/// <summary>
/// Builds tiny, fully-controlled content for engine tests: a two-hex arena with
/// one marine adjacent to one 3-HP dummy, a marine modifier deck of all +0 (so
/// damage is deterministic regardless of shuffle), and an idle enemy AI card.
/// </summary>
internal static class SyntheticContent
{
    public static ContentCatalog BuildArena()
    {
        using var dir = new TempContentDir();

        dir.Write("modifier_decks", "flat.json", """
            { "version": 1, "id": "flat", "cards": [ { "effect": "add", "value": 0, "count": 5 } ] }
            """);

        dir.Write("cards", "test_strike.json", """
            {
              "version": 1, "id": "test_strike", "name": "Strike", "class_id": "test_marine",
              "level": 1, "initiative": 10,
              "top": { "actions": [ { "kind": "attack_melee", "damage": 3 } ] },
              "bottom": { "actions": [ { "kind": "move", "distance": 1 } ] }
            }
            """);

        dir.Write("cards", "test_step.json", """
            {
              "version": 1, "id": "test_step", "name": "Step", "class_id": "test_marine",
              "level": 1, "initiative": 20,
              "top": { "actions": [ { "kind": "move", "distance": 2 } ] },
              "bottom": { "actions": [ { "kind": "move", "distance": 1 } ] }
            }
            """);

        dir.Write("classes", "test_marine.json", """
            {
              "version": 1, "id": "test_marine", "name": "Test Marine", "hp": 10,
              "starting_card_ids": [ "test_strike", "test_step" ], "modifier_deck_id": "flat"
            }
            """);

        dir.Write("ai_cards", "test_idle.json", """
            {
              "version": 1, "id": "test_idle", "name": "Idle", "initiative": 50, "movement": 0,
              "actions": [], "target_priority": "none", "movement_mode": "static"
            }
            """);

        dir.Write("enemies", "test_dummy.json", """
            {
              "version": 1, "id": "test_dummy", "name": "Dummy", "hp": 3,
              "modifier_deck_id": "flat", "ai_card_ids": [ "test_idle" ]
            }
            """);

        dir.Write("maps", "test_arena.json", """
            {
              "version": 1, "id": "test_arena", "name": "Arena",
              "bounds": { "q_min": 0, "q_max": 1, "r_min": 0, "r_max": 0 },
              "default_hex_kind": "floor",
              "spawn_points": [
                { "kind": "marine_slot", "hex": [0, 0], "slot": 1 },
                { "kind": "enemy", "hex": [1, 0], "enemy_id": "test_dummy" }
              ]
            }
            """);

        dir.Write("missions", "test_clear.json", """
            {
              "version": 1, "id": "test_clear", "name": "Clear the Arena", "map_id": "test_arena",
              "victory": { "kind": "eliminate_all_hostiles" },
              "failure": { "kind": "all_marines_exhausted" },
              "rng_seed_source": "scenario_id"
            }
            """);

        return ContentCatalog.Load(dir.Root);
    }
}
