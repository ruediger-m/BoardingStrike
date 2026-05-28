# Characters

The framework for marine classes. The MVP class (Boarding Marine) is in [content/class-boarding-marine.md](content/class-boarding-marine.md).

## What a class is

A class is a *template* shared by all marines of that type. It specifies:

- **Identity.** Id, display name, short description, lore blurb.
- **Base stats.** HP at level 1, sprite reference, audio cues (placeholder in MVP).
- **Starting deck.** 10 action cards at level 1 (all "level-1" cards from the class's card pool). The deck is fixed for MVP — no selection step.
- **Starting modifier deck.** Standard 20-card modifier deck (see [combat.md](combat.md)).
- **Progression hooks.** Reserved fields for level-up rewards. Not used in MVP.

A class does **not** carry per-marine state. Two marines of the same class share the class template but have their own HP, hand, discard, and modifier-deck pile during a scenario.

## Marines vs. classes

A *marine* is an instance of a class with:

- A unique name (placeholder names in MVP, e.g. "Marine 1" — proper naming hooks for Iteration 3 roster).
- Current HP.
- Current hand / discard / burn piles.
- Current modifier deck and modifier discard.
- Active status conditions.

## Stats

| Stat | MVP default | Notes |
|------|-------------|-------|
| HP | per class | Boarding Marine: 10 |
| Visual sprite | per class | 32×32 sheet |
| Audio cue set | placeholder | Iteration 4 |

There is no separate "speed" stat — movement comes from card actions.

## Progression (deferred)

Class progression is **not** in the MVP. The framework reserves these elements for later iterations:

- **Experience and levels.** Earned per scenario; unlock higher-level card options.
- **Card upgrades.** At level-up, a marine may swap a level-1 card for one of two new level-N cards.
- **Perks.** Passive bonuses unlocked at certain levels.
- **Personal goal.** A long-arc condition that, when fulfilled, retires the marine and unlocks a new class slot. Inspired by Gloomhaven personal quests.
- **Gear.** Equipment slots and item cards.

These appear in Iteration 3 (campaign layer) at the earliest.

## Adding a new class (process)

When designing a new class for a later iteration:

1. Write a content sheet in `content/class-<name>.md` using [class-boarding-marine.md](content/class-boarding-marine.md) as the template.
2. Define 10 level-1 cards.
3. Define how the class differs from existing classes in *role* (ranged, melee, support, control) — no two classes should occupy the same role tightly.
4. Add a JSON data file under `data/classes/<id>.json`.
5. Playtest in a sandbox scenario with mixed-class squads before the next campaign integration.

## MVP scope

One class only: the **Boarding Marine**. See [content/class-boarding-marine.md](content/class-boarding-marine.md).

The squad is four Boarding Marines, no class mixing. The two-card system still creates variety per marine (each player picks different cards each round), but role specialization is absent in the MVP. Iteration 2 adds at least two more classes.
