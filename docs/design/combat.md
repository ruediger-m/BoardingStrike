# Combat

Resolution of attacks, damage, and status conditions. Movement, doors, and LoS are in [hex-grid.md](hex-grid.md) and [line-of-sight-and-doors.md](line-of-sight-and-doors.md).

## Stats

Every unit has:

- **HP** — current and max. 0 HP = exhausted/killed (see [action-system.md](action-system.md) for marine exhaustion; enemies are simply removed).
- **Movement.** Default movement per *Move N* action is just N (no per-unit movement stat in MVP). Reserve a `move_modifier` field for later iterations.
- **Initiative** is on cards, not on units (see [core-loop.md](core-loop.md)).
- **Active conditions.** A list of status conditions (see below).

Marines have additionally:

- **Hand / discard / burn pile** of action cards.
- A personal **attack modifier deck**.

Enemies have additionally:

- An **enemy type** reference, which provides their AI deck and per-type stats.

## Attack resolution

When a unit makes an attack:

1. **Eligibility.** Check the attack's range and line-of-sight requirement. Melee attacks require an adjacent target. Ranged attacks require LoS and target within range.
2. **Base damage** = the value on the action half (e.g. `Attack-ranged 3` has base damage 3).
3. **Draw a modifier.** The attacker draws the top card of their attack modifier deck and applies it to the base damage.
4. **Apply damage.** Reduce the target's HP by `max(0, modified damage)`. Damage cannot heal.
5. **Apply on-hit effects.** If the attack inflicts conditions (e.g. *poison*), apply them now, but only if at least 1 point of damage was dealt (`×0` modifier nullifies on-hit effects).
6. **Reshuffle check.** If the drawn modifier was a `×0` or `×2` (the "rolling-shuffle" cards), shuffle the modifier deck after this attack.

The defender does not draw a modifier; attacks are resolved entirely on the attacker's side. This keeps each attack to one card draw.

## Standard modifier deck

Marines start with this 20-card modifier deck. Tunable; values here are the first draft.

| Count | Effect |
|------:|--------|
| 1 | **×0** — attack does 0 damage, reshuffle after this attack |
| 1 | **×2** — attack damage doubled, reshuffle after this attack |
| 6 | **+0** — no change |
| 5 | **+1** |
| 5 | **−1** |
| 1 | **+2** |
| 1 | **−2** |

Mean modifier is approximately +0; variance is moderate, with the ×0/×2 cards creating dramatic moments.

Enemies use a simpler shared **hostile modifier deck** in MVP:

| Count | Effect |
|------:|--------|
| 1 | ×0 |
| 1 | ×2 |
| 8 | +0 |
| 5 | +1 |
| 5 | −1 |

Per-enemy-type modifier decks are deferred past MVP.

## Damage timing

Damage from a single action resolves atomically. Conditions inflicted by the attack apply after the damage step. A target killed by the damage is removed before its conditions would have triggered (no posthumous poisons).

## Status conditions (MVP)

The MVP ships three conditions. The data model supports more, defined per-card.

- **Stunned.** Affected unit cannot play / execute its next turn. On a marine, the *next initiated turn* is skipped (cards committed for that round are discarded unused). On an enemy, the affected individuals do not act on their next type-turn. Removed at the end of the round in which it was applied.
- **Wounded.** Affected unit takes 1 point of damage at the start of each of its turns. Does not stack. Removed by any heal action.
- **Immobilized.** Affected unit cannot use *Move* primitives until removed. Removed at end of the round in which it was applied.

Out of MVP scope, but reserve in the data model: **poisoned** (DoT), **muddled** (attacker draws two modifier cards, takes the lower), **invisible** (cannot be targeted), **strengthen** (attacker draws two, takes the higher).

## Friendly fire and area effects

There are no area-of-effect attacks in MVP. All attacks target a single hex.

When AoE attacks land (Iteration 2+), they target each unit in the affected hexes independently, including friendly units in the splash by default. Cards that exclude friendlies will say so explicitly.

## Healing and revives

- **Heal N.** Restores N HP and removes *wounded*. Cannot exceed max HP.
- Marines have no in-MVP revive action. An exhausted marine is out for the scenario.

## Hostile damage and death

Hostiles drop to 0 HP and are removed from the map. There is no death animation requirement beyond a sprite-fade in MVP. Hostiles do not cycle a hand or refresh.

## Decisions and open questions

- **Hostile modifier deck:** confirmed single shared deck across all enemy types. Per-type decks remain deferred indefinitely; revisit only if balance pressure demands it.
- **Per-marine modifier deck:** confirmed per-marine. Cheaper to balance, lets future iterations let the player upgrade *their* deck.
- **Critical hits:** the ×2 card serves that role. No separate crit system planned.
