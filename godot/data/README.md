# Game content (`godot/data/`)

All gameplay content is authored here as JSON and loaded at runtime by
`BoardingStrike.Core.Data.JsonContentLoader`. Schemas are documented in
[../../docs/technical/data-model.md](../../docs/technical/data-model.md).

## Layout

One subfolder per content category; one file per item, named `<id>.json`:

```
data/
  cards/            player action cards
  classes/          class templates
  modifier_decks/   attack-modifier decks
  enemies/          enemy types
  ai_cards/         enemy AI behavior cards
  maps/             playfield maps
  missions/         scenario definitions
```

A missing subfolder simply means "no content of that kind yet" — it is not an
error. Cross-references between items (e.g. a class's `starting_card_ids`, a
mission's `map_id`) are validated on load; bad content fails loudly with a list
of every problem.

## Current state

Seeded in Step 3 (loader): the two attack-modifier decks `standard_marine` and
`standard_hostile` (final values, per
[../../docs/design/combat.md](../../docs/design/combat.md)).

The full MVP content set — the 10 Boarding Marine cards, the `boarding_marine`
class, the swarmer/spitter enemies and their AI cards, and the Hangar Sweep map
and mission — is authored in **Step 4** of
[../../docs/plans/iteration-1-mvp.md](../../docs/plans/iteration-1-mvp.md).

> The automated test `CommittedContentValidates` loads this folder and fails CI
> if any committed content is invalid, so this directory always stays loadable.
