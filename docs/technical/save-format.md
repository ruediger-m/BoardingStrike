# Save Format

JSON saves for the campaign layer. **Not implemented in MVP.** Specified here so the rest of the architecture won't paint us into a corner.

## Location

Saves live under the user data directory provided by Godot (`OS.GetUserDataDir()` / `user://` in Godot terms). One slot per file:

```
<user_data>/saves/
  campaign_001.json
  campaign_002.json
```

## Top-level structure

```jsonc
{
  "save_version": 1,
  "game_version": "0.x.y",
  "created_at": "2026-05-28T10:15:00Z",
  "updated_at": "2026-05-28T11:00:00Z",
  "slot_name": "Operation Cold Star",
  "campaign": { /* CampaignState */ },
  "roster":   [ /* MarineState */ ],
  "resources": { /* shared resources */ }
}
```

`save_version` is bumped on any breaking schema change. The loader refuses to read a save with a `save_version` higher than it knows; older saves go through a migration step.

## CampaignState

```jsonc
{
  "campaign_id": "act_01_outbreak",
  "node_states": {
    "node_start": "completed",
    "node_alpha": "available",
    "node_beta":  "locked",
    // status enum: locked | available | completed | failed
  },
  "current_node": "node_alpha",
  "decisions": {
    // free-form key/value record of player choices that affect future nodes
  },
  "rng_seed_root": 1734018273
}
```

The `rng_seed_root` is the master seed; scenario seeds are derived from it deterministically, so a campaign reloaded from save plays out the same sequence of random events.

## MarineState

```jsonc
{
  "id": "m_001",
  "name": "Cpl. Halden",
  "class_id": "boarding_marine",
  "level": 1,
  "xp": 0,
  "card_loadout_ids": [ /* the cards currently in this marine's deck */ ],
  "modifier_deck": [
    /* explicit list of modifier-deck card definitions for this marine
       so the player can see (and later upgrade) it */
  ],
  "perks": [],
  "gear": [],
  "personal_goal": {
    "id": "goal_lost_brother",
    "progress": 0
  },
  "conditions_carried_over": [],
  "exhaustion_recovery": 0
}
```

A marine's per-mission state (current HP, current hand) is **not** saved between missions — that resets each scenario. Only durable progression is persisted.

## Resources

```jsonc
{
  "credits": 0,
  "supplies": 0,
  "unlocked_card_ids": [],
  "unlocked_class_ids": ["boarding_marine"]
}
```

Shared squad resources used between missions.

## In-scenario state (not in save)

We do **not** save mid-scenario state in the MVP or in early iterations:

- Saving mid-mission would interact poorly with the modifier deck and the card-burn economy (save-scumming).
- The auto-save points are: scenario start, scenario end, campaign-map navigation.
- Manual save is offered from the main menu / campaign map only.

If mid-scenario save is added later, it would need to be a "suspend" only — single slot, deleted on resume to prevent reloading.

## Validation

The loader:

1. Reads JSON.
2. Checks `save_version` against the current loader.
3. Validates referenced ids exist (cards, classes, missions, goals).
4. Returns a typed `LoadedSave` value or a structured error.

The loader never silently drops unknown fields — they trigger a warning so future fields don't get lost in older clients.

## Backups

On save, the previous version of the file is renamed to `<slot>.bak`. Only the immediately previous version is kept. Tooling for slot copy / export is not in scope.

## Implementation notes

- The save model lives in `BoardingStrike.Game/Save/` (alongside the rest of Game-layer code).
- Serialization uses `System.Text.Json` with explicit converters where needed — no Newtonsoft dependency.
- Tests should round-trip every save schema (`Save → JSON → Save` with equality).
- A debug command (`F8` in dev builds) writes the current rules-state to a dump file for inspection — different from the user-save format and not subject to backward compatibility.

## Out of scope

- Cloud saves.
- Cross-platform save migration (the project targets desktop only for the foreseeable future).
- Encryption / tampering protection. Saves are deliberately human-readable; cheating one's own save game is not a threat model.
