using BoardingStrike.Core.Data;
using Xunit;

namespace BoardingStrike.Game.Tests.Data;

public sealed class JsonContentLoaderTests
{
    // A coherent, fully cross-referential content set used by the happy-path tests.
    private static TempContentDir BuildValidContent()
    {
        var dir = new TempContentDir();
        dir.Write("modifier_decks", "standard_marine.json", """
            {
              "version": 1,
              "id": "standard_marine",
              "cards": [
                { "effect": "multiply", "value": 0, "reshuffle": true, "count": 1 },
                { "effect": "add", "value": 0, "count": 5 }
              ]
            }
            """);
        dir.Write("cards", "test_shot.json", """
            {
              "version": 1,
              "id": "test_shot",
              "name": "Test Shot",
              "class_id": "test_marine",
              "level": 1,
              "initiative": 20,
              "top": { "burn": false, "actions": [ { "kind": "attack_ranged", "damage": 2, "range": 4 } ] },
              "bottom": { "burn": false, "actions": [ { "kind": "move", "distance": 2 } ] }
            }
            """);
        dir.Write("classes", "test_marine.json", """
            {
              "version": 1,
              "id": "test_marine",
              "name": "Test Marine",
              "hp": 10,
              "starting_card_ids": [ "test_shot" ],
              "modifier_deck_id": "standard_marine"
            }
            """);
        dir.Write("ai_cards", "test_charge.json", """
            {
              "version": 1,
              "id": "test_charge",
              "name": "Charge",
              "initiative": 22,
              "movement": 4,
              "actions": [ { "kind": "attack_melee", "damage": 2 } ],
              "target_priority": "closest_marine",
              "movement_mode": "approach_target"
            }
            """);
        dir.Write("enemies", "test_husk.json", """
            {
              "version": 1,
              "id": "test_husk",
              "name": "Husk",
              "hp": 3,
              "modifier_deck_id": "standard_marine",
              "ai_card_ids": [ "test_charge" ]
            }
            """);
        dir.Write("maps", "test_map.json", """
            {
              "version": 1,
              "id": "test_map",
              "name": "Test Map",
              "bounds": { "q_min": 0, "q_max": 5, "r_min": 0, "r_max": 5 },
              "default_hex_kind": "floor",
              "edges": [
                { "a": [0, 0], "b": [1, 0], "kind": "door", "initial_state": "closed" }
              ],
              "spawn_points": [
                { "kind": "marine_slot", "hex": [0, 0], "slot": 1 },
                { "kind": "enemy", "hex": [3, 3], "enemy_id": "test_husk" }
              ]
            }
            """);
        dir.Write("missions", "test_mission.json", """
            {
              "version": 1,
              "id": "test_mission",
              "name": "Test Mission",
              "map_id": "test_map",
              "victory": { "kind": "eliminate_all_hostiles" },
              "failure": { "kind": "all_marines_exhausted" },
              "round_limit": null,
              "rng_seed_source": "scenario_id"
            }
            """);
        return dir;
    }

    [Fact]
    public void Valid_content_loads_all_categories()
    {
        using TempContentDir dir = BuildValidContent();

        ContentDatabase db = JsonContentLoader.Load(dir.Root);

        Assert.Single(db.Cards);
        Assert.Single(db.Classes);
        Assert.Single(db.ModifierDecks);
        Assert.Single(db.Enemies);
        Assert.Single(db.AiCards);
        Assert.Single(db.Maps);
        Assert.Single(db.Missions);
        Assert.Equal(7, db.TotalCount);

        Assert.True(db.Cards.ContainsKey("test_shot"));
        Assert.Equal("Husk", db.Enemies["test_husk"].Name);
        Assert.Equal(4, db.Cards["test_shot"].Top!.Actions![0].Range);
    }

    [Fact]
    public void TryLoad_reports_success_for_valid_content()
    {
        using TempContentDir dir = BuildValidContent();

        bool ok = JsonContentLoader.TryLoad(dir.Root, out ContentDatabase? db, out IReadOnlyList<string> errors);

        Assert.True(ok);
        Assert.Empty(errors);
        Assert.NotNull(db);
    }

    [Fact]
    public void Missing_root_directory_is_an_error()
    {
        bool ok = JsonContentLoader.TryLoad(
            Path.Combine(Path.GetTempPath(), "does_not_exist_" + Guid.NewGuid().ToString("N")),
            out _,
            out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("does not exist", StringComparison.Ordinal));
    }

    [Fact]
    public void Duplicate_id_is_reported()
    {
        using var dir = new TempContentDir();
        string deck = """
            { "version": 1, "id": "dup_deck", "cards": [ { "effect": "add", "value": 0, "count": 1 } ] }
            """;
        dir.Write("modifier_decks", "a.json", deck);
        dir.Write("modifier_decks", "b.json", deck);

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("duplicate id 'dup_deck'", StringComparison.Ordinal));
    }

    [Fact]
    public void Missing_required_field_is_reported()
    {
        using var dir = new TempContentDir();
        // Card with no initiative (and otherwise valid structure).
        dir.Write("classes", "c.json", """
            { "version": 1, "id": "c", "hp": 10, "starting_card_ids": ["k"], "modifier_deck_id": "d" }
            """);
        dir.Write("modifier_decks", "d.json", """
            { "version": 1, "id": "d", "cards": [ { "effect": "add", "value": 0, "count": 1 } ] }
            """);
        dir.Write("cards", "k.json", """
            {
              "version": 1, "id": "k", "class_id": "c",
              "top": { "actions": [ { "kind": "move", "distance": 1 } ] },
              "bottom": { "actions": [ { "kind": "move", "distance": 1 } ] }
            }
            """);

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("initiative", StringComparison.Ordinal) && e.Contains("missing", StringComparison.Ordinal));
    }

    [Fact]
    public void Unknown_action_kind_is_reported()
    {
        using var dir = new TempContentDir();
        dir.Write("ai_cards", "bad.json", """
            {
              "version": 1, "id": "bad", "initiative": 10, "movement": 2,
              "target_priority": "closest_marine", "movement_mode": "approach_target",
              "actions": [ { "kind": "teleport", "distance": 3 } ]
            }
            """);

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("unknown action kind 'teleport'", StringComparison.Ordinal));
    }

    [Fact]
    public void Dangling_cross_reference_is_reported()
    {
        using var dir = new TempContentDir();
        dir.Write("modifier_decks", "d.json", """
            { "version": 1, "id": "d", "cards": [ { "effect": "add", "value": 0, "count": 1 } ] }
            """);
        // Class references a card that does not exist.
        dir.Write("classes", "c.json", """
            { "version": 1, "id": "c", "hp": 10, "starting_card_ids": ["missing_card"], "modifier_deck_id": "d" }
            """);

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("unknown card 'missing_card'", StringComparison.Ordinal));
    }

    [Fact]
    public void Mission_referencing_missing_map_is_reported()
    {
        using var dir = new TempContentDir();
        dir.Write("missions", "m.json", """
            {
              "version": 1, "id": "m", "map_id": "ghost_map",
              "victory": { "kind": "eliminate_all_hostiles" },
              "failure": { "kind": "all_marines_exhausted" }
            }
            """);

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("unknown map 'ghost_map'", StringComparison.Ordinal));
    }

    [Fact]
    public void Malformed_json_is_reported_with_file_name()
    {
        using var dir = new TempContentDir();
        dir.Write("cards", "broken.json", "{ this is not valid json ");

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("failed to parse 'broken.json'", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_throws_aggregate_exception_listing_every_error()
    {
        using var dir = new TempContentDir();
        // Two independent problems should both surface.
        dir.Write("modifier_decks", "d.json", """
            { "version": 1, "id": "d", "cards": [ { "effect": "add", "value": 0, "count": 0 } ] }
            """);
        dir.Write("missions", "m.json", """
            {
              "version": 1, "id": "m", "map_id": "nope",
              "victory": { "kind": "eliminate_all_hostiles" },
              "failure": { "kind": "all_marines_exhausted" }
            }
            """);

        ContentValidationException ex = Assert.Throws<ContentValidationException>(() => JsonContentLoader.Load(dir.Root));

        Assert.True(ex.Errors.Count >= 2);
        Assert.Contains(ex.Errors, e => e.Contains("count", StringComparison.Ordinal));
        Assert.Contains(ex.Errors, e => e.Contains("unknown map 'nope'", StringComparison.Ordinal));
    }

    [Fact]
    public void Comments_and_trailing_commas_are_tolerated()
    {
        using var dir = new TempContentDir();
        dir.Write("modifier_decks", "d.json", """
            {
              // author note: a deck with a trailing comma and comments
              "version": 1,
              "id": "d",
              "cards": [
                { "effect": "add", "value": 0, "count": 3 },
              ],
            }
            """);

        bool ok = JsonContentLoader.TryLoad(dir.Root, out _, out IReadOnlyList<string> errors);

        Assert.True(ok);
        Assert.Empty(errors);
    }
}
