# Data Model

JSON schemas for content. All gameplay content (cards, classes, enemies, modifier decks, maps, missions) lives under `godot/data/` as JSON and is loaded at startup or scenario start.

Schemas below use a JSON-ish notation. Field types are illustrative.

## Conventions

- All ids are lowercase `snake_case` strings, unique within their type.
- Numbers are integers unless otherwise stated.
- Fields marked `[reserved]` are unused in MVP but part of the schema so future versions don't have to migrate.
- All files include a `version` integer at the top level. Bumped on breaking schema changes.

## Cards — `data/cards/<id>.json`

```jsonc
{
  "version": 1,
  "id": "marine_snap_shot",
  "name": "Snap Shot",
  "class_id": "boarding_marine",
  "level": 1,
  "initiative": 16,
  "top": {
    "burn": false,
    "actions": [
      { "kind": "attack_ranged", "damage": 2, "range": 4 }
    ]
  },
  "bottom": {
    "burn": false,
    "actions": [
      { "kind": "move", "distance": 2 }
    ]
  },
  "flavor": ""
}
```

### Action primitive kinds

- `move` — `{ "kind": "move", "distance": N, "type": "walk" | "jump" | "fly" }`. `type` optional, defaults to `walk`. `jump` / `fly` `[reserved]` for later.
- `attack_melee` — `{ "kind": "attack_melee", "damage": N, "on_hit_condition": "wounded" | ... }`. `on_hit_condition` optional.
- `attack_ranged` — `{ "kind": "attack_ranged", "damage": N, "range": R, "on_hit_condition": "..." }`.
- `attack_area` — `[reserved]` — `{ "kind": "attack_area", "damage": N, "range": R, "shape": "..." }`. Not in MVP.
- `heal` — `{ "kind": "heal", "amount": N, "target": "self" | "adjacent_ally" }`.
- `apply_condition` — `{ "kind": "apply_condition", "condition": "stunned" | "wounded" | "immobilized", "target": "..." }`.
- `door` — `{ "kind": "door", "operation": "open" | "close" | "toggle" }`. Targets an adjacent door edge.
- `loot` — `[reserved]`.

A card's `top.actions` and `bottom.actions` are arrays — primitives execute left-to-right.

## Classes — `data/classes/<id>.json`

```jsonc
{
  "version": 1,
  "id": "boarding_marine",
  "name": "Boarding Marine",
  "hp": 10,
  "sprite": "art/units/marine_default.png",
  "starting_card_ids": [
    "marine_snap_shot",
    "marine_carbine_burst",
    "marine_knife_work",
    "marine_overcharge_shot",
    "marine_breach_and_clear",
    "marine_frag",
    "marine_combat_stim",
    "marine_suppressing_fire",
    "marine_push_forward",
    "marine_steady_hands"
  ],
  "modifier_deck_id": "standard_marine"
}
```

## Modifier decks — `data/modifier_decks/<id>.json`

```jsonc
{
  "version": 1,
  "id": "standard_marine",
  "cards": [
    { "effect": "multiply", "value": 0, "reshuffle": true,  "count": 1 },
    { "effect": "multiply", "value": 2, "reshuffle": true,  "count": 1 },
    { "effect": "add",      "value": 0, "reshuffle": false, "count": 6 },
    { "effect": "add",      "value": 1, "reshuffle": false, "count": 5 },
    { "effect": "add",      "value": -1,"reshuffle": false, "count": 5 },
    { "effect": "add",      "value": 2, "reshuffle": false, "count": 1 },
    { "effect": "add",      "value": -2,"reshuffle": false, "count": 1 }
  ]
}
```

`effect` is `multiply` or `add`. `multiply` is applied before `add` if both ever appear on one card (only the special ×2 +1 style cards do — reserved).

## Enemies — `data/enemies/<id>.json`

```jsonc
{
  "version": 1,
  "id": "husk_swarmer",
  "name": "Swarmer",
  "sprite": "art/units/enemy_husk.png",
  "hp": 3,
  "modifier_deck_id": "standard_hostile",
  "ai_card_ids": [
    "husk_swarmer_charge",
    "husk_swarmer_pounce",
    "husk_swarmer_sprint",
    "husk_swarmer_frenzied_bite",
    "husk_swarmer_skitter",
    "husk_swarmer_tail_whip",
    "husk_swarmer_withdraw",
    "husk_swarmer_pile_on"
  ]
}
```

## AI cards — `data/ai_cards/<id>.json`

```jsonc
{
  "version": 1,
  "id": "husk_swarmer_charge",
  "name": "Charge",
  "initiative": 22,
  "movement": 4,
  "actions": [
    { "kind": "attack_melee", "damage": 2 }
  ],
  "target_priority": "closest_marine",
  "movement_mode": "approach_target"
}
```

### `target_priority` enum

- `closest_marine` (by path).
- `closest_marine_los` (by path, restricted to marines with LoS).
- `highest_hp_marine_los` (range-restricted via action range).
- `lowest_hp_marine`.
- `none` (no target needed, e.g. `Burrow`, `Withdraw`).

### `movement_mode` enum

- `approach_target` — move toward target along shortest valid path; stop at action range.
- `flee_from_closest_marine` — move maximizing distance from nearest marine.
- `reposition_for_los` — move to a hex that has LoS to a valid target, prefer ending behind cover (adjacent to a wall edge between you and the target). Used by spitters.
- `reposition_no_los` — move to a hex with no LoS from any marine (Burrow).
- `static` — movement budget unused.

The AI executor reads these to compute the actual hexes moved.

## Conditions

Conditions are not authored as standalone files; they are a fixed enum recognized by the rules engine. MVP set:

```
stunned        — skips next initiated turn; expires end of applying round
wounded        — 1 dmg per own turn start; removed by any heal action
immobilized    — cannot use move primitives; expires end of applying round
```

Reserved (not in MVP): `poisoned`, `muddled`, `invisible`, `strengthen`.

## Maps — `data/maps/<id>.json`

```jsonc
{
  "version": 1,
  "id": "map_derelict_alpha",
  "name": "Derelict Alpha",
  "bounds": { "q_min": 0, "q_max": 19, "r_min": 0, "r_max": 15 },
  "hexes": [
    { "q": 0, "r": 0, "kind": "floor" },
    { "q": 1, "r": 0, "kind": "floor" },
    // ... or use "default": "floor" and only list "void" overrides
  ],
  "default_hex_kind": "floor",
  "void_hexes": [
    { "q": 5, "r": 5 },
    { "q": 5, "r": 6 }
  ],
  "edges": [
    { "a": [0,0], "b": [1,0], "kind": "wall" },
    { "a": [3,3], "b": [3,4], "kind": "door", "initial_state": "closed" }
  ],
  "spawn_points": [
    { "kind": "marine_slot", "hex": [0,0], "slot": 1 },
    { "kind": "marine_slot", "hex": [1,0], "slot": 2 },
    { "kind": "marine_slot", "hex": [0,1], "slot": 3 },
    { "kind": "marine_slot", "hex": [1,1], "slot": 4 },
    { "kind": "enemy", "hex": [10,5], "enemy_id": "husk_swarmer" },
    { "kind": "enemy", "hex": [11,5], "enemy_id": "husk_swarmer" },
    { "kind": "enemy", "hex": [15,8], "enemy_id": "cyst_spitter" }
  ]
}
```

Marine slots are filled by the scenario's chosen squad in slot order.

## Scenarios — `data/missions/<id>.json`

```jsonc
{
  "version": 1,
  "id": "mission_hangar_sweep",
  "name": "Hangar Sweep — Derelict Alpha",
  "map_id": "map_derelict_alpha",
  "victory": { "kind": "eliminate_all_hostiles" },
  "failure": { "kind": "all_marines_exhausted" },
  "round_limit": null,
  "rng_seed_source": "scenario_id"
}
```

Reserved victory/failure kinds (extended in Iteration 2 — see [../plans/iteration-2-class-diversity.md](../plans/iteration-2-class-diversity.md)):

- `reach_hex` — a marine ends a turn on a specific hex.
- `reach_hex_with_radius_cleared` — `reach_hex` plus no hostiles within N hexes of the target.
- `hold_hex_for_n_rounds` — a marine remains on a hex through N end-of-round ticks.
- `escort_to_hex` — escort an NPC unit to a hex.
- `survive_n_rounds`, `defend_hex_for_n_rounds` — time-based defense conditions.

Iteration 2 also adds **multi-phase victory composition**: `victory` becomes an array of phases, each phase referencing one of the above primitives. The mission progresses through the phases in order; failure of an in-progress phase (e.g. the holding marine is killed during `hold_hex_for_n_rounds`) returns the mission to the prior phase. See [../plans/iteration-2-class-diversity.md](../plans/iteration-2-class-diversity.md) Step 1 for the spec.

`rng_seed_source`:
- `"scenario_id"` — deterministic from id (useful for testing).
- `"random"` — engine generates a seed at scenario start.
- A literal integer — fixed seed.

## Loading and validation

`BoardingStrike.Core.Data.JsonContentLoader`:

1. On startup, walk every JSON file under `data/`.
2. Validate the schema (required fields, enum values, id uniqueness, cross-references resolve).
3. Produce immutable Game-layer entities (`Card`, `ClassDef`, `EnemyDef`, `MapDef`, `MissionDef`).
4. Fail loud on validation errors — never silently default.

A future content-pipeline pass may compile JSON to a binary format for faster startup. Not in MVP.
