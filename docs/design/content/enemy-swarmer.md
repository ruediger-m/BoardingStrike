# Enemy: Swarmer (Husk)

Melee rusher. The Brood's basic combatant. Spawns in numbers. Designed to make the player commit to LoS-aware positioning so the swarm doesn't reach the back rank.

> Numbers tunable. The MVP mission ships ~10–12 swarmers across the map; tune after the first end-to-end playtest.

## Identity

- **Id:** `husk_swarmer`
- **Name:** Husk / Swarmer
- **Sprite:** `enemy_husk.png` (placeholder)
- **Tactical role:** Melee rusher.

## Stats

| Stat | Value |
|------|-------|
| HP | 3 |
| Base attack | 2 (melee, range 1) |
| Move per move-primitive | 1 hex |
| Modifier deck | shared hostile deck (see [../combat.md](../combat.md)) |

A swarmer dies to one solid carbine shot. The threat is volume.

## AI deck (8 cards)

Each card has an initiative number, a movement budget, an action, and a target priority. Default target priority is *closest marine by movement path*.

---

### 01 — Charge — init 22

- Movement 4.
- Then Attack-melee 2.
- Notes: standard rush-and-bite.

---

### 02 — Pounce — init 17

- Movement 5.
- Then Attack-melee 1 if adjacent.
- Notes: fast initiative, longest move; weaker hit.

---

### 03 — Sprint — init 14

- Movement 6.
- No attack.
- Notes: pure repositioning card. Closes ground for the next round.

---

### 04 — Frenzied Bite — init 32

- Movement 2.
- Then Attack-melee 3 if adjacent. Apply *wounded* on hit.
- Notes: slower, harder hit, lingering damage.

---

### 05 — Skitter — init 25

- Movement 3.
- Then Attack-melee 2.
- Notes: middle-of-the-road. Most common feel.

---

### 06 — Tail Whip — init 28

- Movement 1.
- Then Attack-melee 2. Apply *immobilized* on hit.
- Notes: holds a marine in place. Painful when a second swarmer is already adjacent.

---

### 07 — Withdraw — init 11

- Movement 4 directly away from closest marine.
- No attack.
- Notes: rare. Used by wounded swarmers (HP ≤ 1). For MVP simplicity, every swarmer in the type executes this card regardless of HP — the unusual behavior is part of the puzzle. Re-evaluate after playtest.

---

### 08 — Pile On — init 30

- Movement 3.
- Then Attack-melee 2.
- Then second Attack-melee 1 against same target if still alive.
- Notes: the punishing card. Two hits per swarmer.

---

## Target priority

Unless a card says otherwise, swarmers target the **closest marine by movement path** (i.e. respecting walls and doors). Tiebreaks:

1. Lowest current marine HP.
2. Lowest marine entity id.

If no path to any marine exists (e.g. all doors closed), swarmers move toward the nearest door / movable obstacle and pass attack.

## Spawn / encounter feel

- Swarmers spawn behind closed doors, around corners, or from "biomass nests" (terrain decorations in MVP).
- A typical MVP wave is 2–4 swarmers visible plus 2 more revealed when a door is opened.
- The player should always be able to one-shot a swarmer with a base 3 attack — which is most ranged tops in the marine deck — but should rarely be able to kill more than two in a single round without burning cards.

## Reserved variants

- **Lurker** — swarmer that hides behind walls until a marine ends turn adjacent.
- **Brute swarmer** — HP 6, attack 3, slower. Iteration 2.

## Open questions

- Whether the swarmer modifier deck should be slightly more swingy than the shared hostile deck (e.g. extra ×0 and ×2). Defer to playtest.
- Whether the *Withdraw* card should remain in MVP. It only makes sense if swarmers can be wounded but alive at the start of an enemy turn, which can happen in MVP. Reasonable to keep.
