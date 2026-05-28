# Enemy: Spitter (Cyst)

Ranged controller. Slow, fewer in number than swarmers, applies conditions. Designed to force the player to use line of sight and doors instead of camping in open corridors.

> Numbers tunable. The MVP mission ships 2–3 spitters; tune after the first end-to-end playtest.

## Identity

- **Id:** `cyst_spitter`
- **Name:** Cyst / Spitter
- **Sprite:** `enemy_cyst.png` (placeholder)
- **Tactical role:** Ranged controller.

## Stats

| Stat | Value |
|------|-------|
| HP | 5 |
| Base attack | 2 (ranged, range 5) |
| Move per move-primitive | 1 hex |
| Modifier deck | shared hostile deck (see [../combat.md](../combat.md)) |

A spitter takes two solid hits to drop. The threat is positioning and conditions.

## AI deck (8 cards)

Default target priority: **marine with the highest HP, with LoS, at any range**. (Spitters target marines who can still take a hit; rationale: their toxin lingers, so they prefer healthy targets to wound rather than finishing low-HP marines.) If no marine has LoS, the spitter moves to acquire LoS.

---

### 01 — Spit — init 26

- Movement 2.
- Attack-ranged 2, range 5, LoS required. Apply *wounded* on hit.
- Notes: standard ranged shot. The bread-and-butter.

---

### 02 — Aimed Spit — init 38

- Movement 1.
- Attack-ranged 3, range 5, LoS required. Apply *wounded* on hit.
- Notes: slower, hits harder.

---

### 03 — Acid Volley — init 31

- Movement 0.
- Attack-ranged 2, range 5, LoS required. Apply *wounded*. Then a second Attack-ranged 1 against another marine in LoS if any exists.
- Notes: the multi-target card; the moment two spitters volley in the same round is when the player feels overwhelmed.

---

### 04 — Reposition — init 19

- Movement 4 to a hex that has LoS to at least one marine, prefer cover behind an edge wall.
- No attack.
- Notes: keeps the spitter from being kited around a corner indefinitely.

---

### 05 — Slow Spit — init 29

- Movement 2.
- Attack-ranged 2, range 5, LoS required. Apply *immobilized* on hit.
- Notes: the lockdown card. Forces the marine to refresh or burn.

---

### 06 — Burrow — init 13

- Movement 3 to a hex without LoS to any marine (hide).
- No attack.
- Notes: the spitter denies the player a target until next round. Frustrating in a good way — the player has to advance to flush them out.

---

### 07 — Inhale — init 41

- Movement 0.
- The spitter does nothing this turn; on the next round in which the spitter draws a card, that attack does +2 damage.
- Notes: the telegraph card. Players see "Inhale" and know the next round will be ugly.
- **MVP implementation note:** Track this with a one-round buff token on each affected spitter individual. Buff consumed on the next attack the spitter performs.

---

### 08 — Acid Mist — init 35

- Movement 1.
- All marines within range 3 in LoS take 1 damage and gain *wounded*.
- Notes: the AoE card. Forces marines to spread out.
- **MVP implementation note:** This is the only AoE in the MVP. Define the effect as iterating LoS+range against each marine and applying independently — no special area code.

---

## Target priority

Default per-card rule: **highest-HP marine with LoS, within range 5**. Tiebreaks:

1. Closest by hex distance.
2. Lowest marine entity id.

If no marine has LoS, the spitter falls back to *Reposition* behavior even if a different card was drawn — i.e., it spends its movement to acquire LoS first, then executes whatever portion of the card it can.

Special-case rule for *Burrow* (init 13): runs as written regardless of LoS state, since the card's purpose is to leave LoS.

## Spawn / encounter feel

- Spitters lurk in rooms with LoS to the corridor the marines will use. The MVP mission positions one spitter in the first large room and one or two more in a back chamber.
- A typical "spitter encounter" is the player committing two cards: one to flush LoS (move out of the corridor), one to land a kill before the spitter can volley.
- The presence of two simultaneous spitters should feel dangerous — if they both draw Acid Volley, the squad will eat 6+ damage and pick up *wounded* twice.

## Reserved variants

- **Bursting Cyst** — explodes on death, AoE damage. Iteration 2.
- **Sniper Cyst** — range 7, no movement, single high-damage shot. Iteration 2.

## Decisions and open questions

- **Target priority:** confirmed *highest-HP marine with LoS* for the MVP. **Flagged for playtest** — if a single tanky marine gets focus-fired into uselessness, fall back to a closest-then-highest hybrid. Re-evaluate in Iteration 1's playtest pass (MVP step 8).
- **Inhale (init 41):** kept for MVP. The telegraph beat is worth the small implementation cost. Tracked via a `pending_modifiers` list on the unit.
