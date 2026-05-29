# Iteration 2 — Class Diversity and Mission Variety

Second iteration. Goal: prove the game holds up beyond a single class and a single mission archetype. Adds two new playable classes, three new enemy types, the overwatch / reaction-fire mechanic, and **two new missions** that exercise victory conditions beyond Sweep.

This is a larger iteration than the MVP. Sequence the work so the action-economy gains real validation against varied classes and varied mission shapes.

## Definition of done

The iteration ships when:

1. The player can pick a 4-marine squad from a pool that includes at least 3 distinct classes (Boarding Marine + 2 new).
2. The two new classes each ship 10 level-1 cards, distinct enough that mixed-class squads play noticeably differently from same-class squads.
3. Three new enemy types are in play with full AI decks.
4. Overwatch / reaction-fire is implemented as an action primitive available to at least one class.
5. The new mission [Captain's Cabin](../design/content/mission-captains-cabin.md) is playable end-to-end.
6. The new mission [Containment](../design/content/mission-containment.md) is playable end-to-end.
7. The original [Hangar Sweep](../design/content/mission-hangar-sweep.md) mission still passes its regression test against the refactored rules engine.
8. A simple mission-select screen lets the player choose which of the three available missions to play.
9. End-to-end automated scenario tests cover all three missions in CI.

The iteration ships *without*:

- Persistent campaign roster (Iteration 3).
- Save/load (Iteration 3).
- Between-mission progression (Iteration 3).
- Full art pass (Iteration 4+).
- Audio (Iteration 4+).

## Iteration scope at a glance

This is roughly 3× the new content of Iteration 1 plus several rules-engine extensions. The bullet list below is a sizing reality check — do not let it expand without an explicit scope conversation.

- 2 new classes × 10 cards = 20 new action cards.
- 3 new enemy types × ~8 AI cards each = 24 new AI cards.
- 2 new missions × ~12 hostiles + map authoring + tuning.
- Overwatch primitive (rules + UI).
- New victory conditions: `reach_hex_with_radius_cleared`, `hold_hex_for_n_rounds`, multi-phase composition.
- Reinforcement spawn triggers.
- Mission-select UI.

If at the start of the iteration this list looks like ~6 weeks of work and we have less, drop **Containment** first (the most expensive single deliverable) and ship it as Iteration 2.5. Captain's Cabin and class diversity are the irreducible core of "prove the game scales past the MVP."

## Build order

Numbered steps; sub-bullets are parallelizable within a step.

### Step 1 — Refactor extensions

Before adding content, harden the rules engine for the new mechanics.

- **Victory condition composition.** Refactor `VictoryEvaluator` to accept an array of conditions joined by `any` or `all`, with phased progression.
- **Reinforcement spawn system.** Add `reinforcement_triggers` to map data; engine fires triggers in the end-of-round tick.
- **Interact-and-hold victory primitive (`hold_hex_for_n_rounds`).** Track an "occupancy timer" per relevant hex; reset on displacement.
- **Overwatch primitive.** A unit may declare overwatch as part of a card half; the unit fires on the first hostile to enter LoS in a defined arc / range during the rest of the round. Resolution order documented in an extension to [../design/combat.md](../design/combat.md).
- Tests for each new mechanic with seeded RNG.

**Exit criterion:** all new rules-engine pieces pass unit tests; Hangar Sweep's existing regression test still passes.

### Step 2 — Two new classes

- Design and author the two new classes. Suggested archetypes:
  - **Breacher** — heavy melee, high HP, short range, signature *hack* and *charge* mechanics. Built to crack tight doorways.
  - **Tech-Specialist** — low HP, ranged, control-focused. Carries overwatch on multiple cards, deploys deployable sensors / turret tokens (reserved primitive).
- Author 10 level-1 cards per class.
- Add a class-select / squad-build screen (placeholder UI in MVP-style, polished later).
- Playtest at least 6 squad compositions (e.g., 4 Marines, 4 Breachers, 4 Tech, 2 Marine + 2 Breacher, etc.) on the Hangar Sweep map.

**Exit criterion:** mixed-class squads finish Hangar Sweep at roughly the same difficulty as a pure-Marine squad. No class trivializes the mission.

### Step 3 — Three new enemy types

- **Reaver** (suggested name): swarmer variant with a self-destruct AI card. Forces marines to spread out.
- **Hunter** (suggested name): spitter variant with a longer-range tracking shot. Counters camping.
- **Brute** (suggested name): high-HP melee that takes 2–3 cards to drop. Tank archetype.
- Each ships ~8 AI cards.
- Document each in `design/content/enemy-<name>.md`.

**Exit criterion:** all three enemy types pass a sandbox test scenario.

### Step 4 — Mission: Captain's Cabin

- Author the map per [mission-captains-cabin.md](../design/content/mission-captains-cabin.md).
- Wire up the `reach_hex_with_radius_cleared` victory condition (built in Step 1).
- Tune spawn counts and the radius value per playtest.
- Add to mission-select.

**Exit criterion:** human player can finish Captain's Cabin in 5–7 rounds with a competent squad.

### Step 5 — Mission: Containment

- Author the map per [mission-containment.md](../design/content/mission-containment.md).
- Wire up the multi-phase victory state machine and reinforcement triggers.
- Tune reinforcement cadence and phase 2 duration per playtest. This is the highest-risk mission for difficulty tuning — schedule at least two tuning passes.
- Add to mission-select.

**Exit criterion:** human player can finish Containment in 8–10 rounds; failure rate is non-trivial (lose-state must feel achievable, otherwise the action-economy clock is invisible).

### Step 6 — Mission-select UI and polish

- Minimal mission-select screen: list of 3 missions, click to launch.
- Result screen after each mission with the same stats as MVP plus per-class notes for the post-game review.
- Crash sweep across all three missions and all class combos.
- Regression test suite: each mission has at least one scripted end-to-end test in CI.

**Exit criterion:** all definition-of-done items above check out.

## Risks and mitigations

- **Risk:** Multi-phase victory + reinforcements is more work than estimated.
  **Mitigation:** Step 1 lands these in isolation, tested independently, before content authoring. If Step 1 overruns, defer Containment to Iteration 2.5.
- **Risk:** Class balance is bad and one class becomes the obvious pick.
  **Mitigation:** Step 2 includes the 6-composition playtest. Tune cards iteratively, not as a one-shot.
- **Risk:** Overwatch interactions with the round structure create edge cases.
  **Mitigation:** Document overwatch resolution rules as a doc extension in Step 1 *before* writing code. The doc is the test plan.
- **Risk:** Iteration scope balloons.
  **Mitigation:** The "drop Containment first" escape hatch is named in the iteration scope section. Use it.

## Hand-off

When the iteration completes:

1. Update [../vision.md](../vision.md) with Iteration 2's actual scope landed.
2. Open `plans/iteration-3-campaign.md` with the campaign-layer scope.
3. Capture playtest notes in `docs/playtest/iteration-2.md`.
