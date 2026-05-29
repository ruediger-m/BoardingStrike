# Iteration 1 — MVP Vertical Slice

The first iteration's implementation plan. Goal: a playable single-mission vertical slice that exercises the full action/refresh loop with four Boarding Marines against swarmers and spitters on the handcrafted **Hangar Sweep** mission.

**Mission:** [Hangar Sweep — Derelict Alpha](../design/content/mission-hangar-sweep.md). Archetype: Sweep. Eliminate all 12 hostiles (10 swarmers + 2 spitters) on a ~20×16 derelict-ship map across four zones (airlock, service corridor, hangar bay, bridge).

This plan cites design and technical docs by path. When the plan and a design doc disagree, the design doc wins and this plan gets updated.

## Definition of done

The iteration ships when:

1. Launching the game drops directly into the Hangar Sweep mission (no main menu yet — keystroke to start).
2. Four Boarding Marines spawn on the map. The player can pick which marine to play each turn from a turn-order indicator.
3. The player commits two cards per round per marine. Initiative is computed correctly. Marines and enemy types act in the right order.
4. All ten Boarding Marine cards execute correctly, including the burn-card cases.
5. Refresh works: discard returns to hand, one card randomly burned.
6. Exhaustion correctly removes marines from play; scenario fails when the squad is wiped.
7. Swarmers and spitters draw from their AI decks; their behavior matches [content/enemy-swarmer.md](../design/content/enemy-swarmer.md) and [content/enemy-spitter.md](../design/content/enemy-spitter.md).
8. Combat resolution uses the modifier deck. Reshuffle on ×0 / ×2 works. On-hit conditions apply (stunned, wounded, immobilized).
9. Doors can be opened/closed; closed doors block LoS and movement.
10. Victory triggers on all hostiles defeated; failure on all marines exhausted.
11. Hangar Sweep can be played to completion (win or fail) without crashes or rule-bugs that block flow.
12. At least one end-to-end automated scenario test runs in CI against the rules engine.

The iteration ships *without*:

- Main menu, settings, save/load, audio, music.
- Multi-class squad, deck-building, between-mission progression.
- Animation polish, particle effects, screen shake.
- Final art (placeholders are acceptable; some pass-1 art is a stretch goal).
- Overwatch / reaction fire.
- Tutorial.

## Build order

Steps are sequenced so each unblock the next. Inside a step, sub-bullets may be parallelizable.

### Step 1 — Project skeleton

- Create the solution at the repo root.
- Create the three projects per [../technical/architecture.md](../technical/architecture.md): `BoardingStrike.Core`, `BoardingStrike.Game`, `BoardingStrike.Game.Tests`.
- Create the Godot project under `godot/` with `.NET` enabled, referencing `Core` and `Game`.
- Wire up a single C# script on the root scene that calls into a stub `ScenarioController` from `Game` to confirm the boundary works.
- Add CI that builds the solution and runs `Game.Tests` on push.

**Exit criterion:** `dotnet build` succeeds; Godot opens; one passing test exists.

### Step 2 — Hex Core

- Implement `HexCoord` (axial), neighbors, distance, line-cross routine for LoS.
- Implement A* in `Core/Pathfinding` with `IHexGraph` interface and edge-walkability hooks.
- Implement `HasLineOfSight(board, from, to)` using the edge-line algorithm.
- Unit tests for: distance correctness, neighbor enumeration, A* over a simple maze, LoS through walls and doors.

**Exit criterion:** all `Core` tests pass. No Godot reference.

### Step 3 — JSON content loader

- Define DTO records for cards, classes, enemies, AI cards, modifier decks, maps, missions per [../technical/data-model.md](../technical/data-model.md).
- Implement `JsonContentLoader` that walks `godot/data/`, validates, and produces immutable Game-layer entities.
- Write minimal sample content (one card, one class, one enemy) and a test that loads and validates.

**Exit criterion:** loader test passes; bad content fails with a useful error message.

### Step 4 — Authoring the MVP content data

Author the JSON files for the MVP. Live alongside Step 5 — designers/authors can pin down numbers while engineering proceeds.

- 10 Boarding Marine cards per [../design/content/class-boarding-marine.md](../design/content/class-boarding-marine.md).
- 1 class file (`boarding_marine`).
- 2 enemy files (`husk_swarmer`, `cyst_spitter`) and their 8+8 AI cards per [../design/content/enemy-swarmer.md](../design/content/enemy-swarmer.md) and [../design/content/enemy-spitter.md](../design/content/enemy-spitter.md).
- 2 modifier decks (`standard_marine`, `standard_hostile`).
- 1 map JSON (`map_derelict_alpha`) — the Hangar Sweep map per [../design/content/mission-hangar-sweep.md](../design/content/mission-hangar-sweep.md). Four zones (airlock, service corridor, hangar bay, bridge), 5 doors, walls between zones, 4 marine spawn slots, 12 hostile spawns.
- 1 mission JSON (`mission_hangar_sweep`) referencing the map, with `victory = eliminate_all_hostiles` and `failure = all_marines_exhausted`.

Follow the authoring checklist at the bottom of [mission-hangar-sweep.md](../design/content/mission-hangar-sweep.md).

**Exit criterion:** content loads cleanly; A* paths exist from every marine spawn to every hostile spawn (respecting doors).

### Step 5 — Board state & rules engine

- `BoardState` holds hexes, edges, doors, units, conditions.
- `Unit` (marine, hostile) with HP, position, conditions, hand/discard/burn for marines.
- `ScenarioState` composes `BoardState` and the round/turn structures.
- `RoundResolver` implementing the round flow per [../design/core-loop.md](../design/core-loop.md).
- `AttackResolver` and `ModifierDeck` per [../design/combat.md](../design/combat.md).
- `ConditionStore` for stunned, wounded, immobilized.
- Refresh & exhaustion per [../design/action-system.md](../design/action-system.md).
- Enemy AI executor per [../design/enemies.md](../design/enemies.md), driving from AI card primitives.
- Tests: scenario fixtures (seeded RNG) that run a few rounds and assert event sequences.

**Exit criterion:** the rules engine can play Hangar Sweep end-to-end via a scripted test driver. No UI yet.

### Step 6 — Godot presentation: board view

- `BoardView` Godot node renders the hex grid using placeholder solid-color tiles.
- Edge overlay layer renders walls and doors.
- Camera centered on the map; no scrolling or zoom needed for the MVP map size.
- Coordinate-debug overlay toggle (`F1`).

**Exit criterion:** running the project shows the MVP map.

### Step 7 — Godot presentation: units

- `UnitView` per marine and per hostile, anchored on their hex.
- HP bar and condition icons (placeholder squares).
- Tween-based movement following emitted `UnitMovedEvent` paths.
- Attack flash and tracer effect on `AttackResolvedEvent`.

**Exit criterion:** the Hangar Sweep scenario plays out visually when scripted.

### Step 8 — Card hand UI and input

- `CardHandHud` shows the active marine's hand at the bottom of the screen.
- Click a card to select for top or bottom half; second click sets the alternative half.
- Once two cards are committed for a marine, prompt the player to confirm or change.
- "Refresh" button when applicable.
- Initiative ladder shows the round's draw order once committed.

**Exit criterion:** a human player can drive Hangar Sweep to victory or failure.

### Step 9 — Polish and shipping

- One end-to-end automated scenario test added to CI: scripted player and enemy moves, asserted event stream.
- Quick-fail crash sweep: open and close doors in every order, drag every marine to exhaustion, kill every hostile type.
- README at repo root with a 30-second "how to run" snippet.
- Build artifact: a `dotnet publish`-able Godot export for Windows.

**Exit criterion:** all definition-of-done items above check out.

## Open questions to resolve during MVP

- Final hex-by-hex layout of the Hangar Sweep map. The zone sketch in [mission-hangar-sweep.md](../design/content/mission-hangar-sweep.md) is authoritative on shape; exact hex coordinates land in Step 4.
- Tuning of HP/damage numbers and the per-mission tuning notes in [mission-hangar-sweep.md](../design/content/mission-hangar-sweep.md). Expect at least one pass after Step 8.
- Whether to ship pass-1 unit sprites or stay on placeholders. Stretch goal — does not gate the iteration.
- **Spitter targeting (highest-HP marine with LoS)** — flagged for explicit playtest scrutiny in Step 8 per [../design/content/enemy-spitter.md](../design/content/enemy-spitter.md).

## Risks and mitigations

- **Risk:** Action system is the gameplay heart and may not feel right.
  **Mitigation:** Build the simplest full version first (Step 5), playtest immediately on placeholder UI in Step 8 before polishing.
- **Risk:** Enemy AI determinism bugs.
  **Mitigation:** Comprehensive scenario fixtures with seeded RNG in Step 5. Bugs caught headlessly are cheaper than bugs caught in-engine.
- **Risk:** Godot C# friction (build pipeline, hot reload).
  **Mitigation:** Step 1 stands up the boundary early. If friction is high, retreat to keeping presentation thin and gameplay in pure-C# projects (already the architecture).
- **Risk:** Scope creep into Iteration 2 territory (a second class, overwatch).
  **Mitigation:** Definition of done is the contract. Anything outside it is queued for Iteration 2.

## Hand-off

When the iteration completes:

1. Mark Hangar Sweep and the Boarding Marine content as v1 in the docs (any tuning changes during the iteration are captured back into the design docs).
2. Move to [plans/iteration-2-class-diversity.md](iteration-2-class-diversity.md) — already drafted, includes the Captain's Cabin and Containment missions.
3. Capture playtest notes in a new `docs/playtest/iteration-1.md` for retrospective reference.
