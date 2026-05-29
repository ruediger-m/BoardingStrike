# Mission: Containment

Iteration 2 mission. Archetype: **Multi-phase interact + extract under reinforcements**. Setting: `[PLACEHOLDER]` research station whose central bulkhead has ruptured, leaking biomass into the station core. The squad must reach the breach, hold position to weld it shut, then withdraw — while reinforcements pour in until the seal completes.

This is the iteration's marquee mission. It puts maximum pressure on the action-economy clock and validates several systems at once.

> Numbers are first-draft and tunable.

## Identity

- **Mission id:** `mission_containment`
- **Map id:** `map_research_station_core`
- **Name (display):** Containment
- **Archetype:** Interact-and-hold + reach-hex (multi-phase)
- **Estimated optimal length:** 8–10 rounds

## Map shape

Bounds: roughly **q ∈ [0..19], r ∈ [0..19]** (~20×20). Symmetric layout, near-square:

```
       q→  0    5    10   15
   r↓
   0   ~ ~ ~ ~ . . . . . . . . . . ~ ~ ~ ~ ~ ~       N CORRIDOR
   1   ~ ~ ~ ~ . . . . . . . . . . ~ ~ ~ ~ ~ ~       (reinforcement
   2   ~ ~ ~ ~ . . D . . . . . . . ~ ~ ~ ~ ~ ~        spawn line)
   3   ~ ~ ~ ~ . . . . . . . . . . ~ ~ ~ ~ ~ ~
   4   . . . . . . . . . . . . . . . . . . . .       E CORRIDOR
   5   . . . . . . . . . . . . . . . . D . . .       (spitter cover)
   6   . . . . = = = = = = = = = = . . . . . .
   7   . . . . = . . . . . . . . = . . . . . .
   8   . . . . = . . . B . . . . = . . . . . .       CENTRAL LAB
   9   . . . . = . . . . . . . . = . . . . . .       (B = breach hex)
  10   . . . . = . . . . . . . . = . . . . . .
  11   . . . . = . . . . . . . . = . . . . . .
  12   . . . . = . . . . . . . . = . . . . . .
  13   . . . . = = = = = D = = = = . . . . . .
  14   . . . . . . . . . . . . . . . . . . . .       S CORRIDOR
  15   . . . D . . . . . . . . . . . . . . . .
  16   . M M . . . . . . . . . . . . . . . . .       AIRLOCK
  17   . M M . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~       (spawn + extract)
  18   . X . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~       (X = extraction)
  19   . . . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
```

### Zones

1. **Extraction airlock** (SW corner). Marine spawn AND extraction hex (objective phase 3).
2. **Central lab** (middle, walled off, ~8×8 interior). Contains the **breach hex** (`B`). One door on the N wall, one door on the S wall. Walls around the lab create choke geometry — every entry is contested.
3. **North corridor**. Open lane along the top; this is the reinforcement spawn corridor.
4. **East corridor**. Spitter cover; LoS lanes into the lab via a door on its W wall.
5. **South corridor**. Connects the airlock to the lab. Clear ground at start.

### Doors

- Lab N door: closed at start.
- Lab S door: closed at start.
- E corridor entry to the lab: closed at start.
- Airlock door: open at start.

## Spawns

### Marines (4)

Four marine slots in the extraction airlock.

### Initial hostiles (5)

- **3 swarmers** distributed in the south corridor near the lab.
- **2 spitters** dug into the east corridor with LoS into the lab through the E door (once opened).

### Reinforcements

**Trigger:** every end-of-round before phase 2 completes (i.e. before the breach is sealed). **Spawn:** 2 swarmers from the north corridor's eastern end, at hexes (12,0) and (13,0). They path south into the lab on their next turn.

Reinforcements **stop** once the breach is sealed (end of phase 2). This is the mission's incentive structure: every refresh, every burn card, every delay puts more swarmers on the board.

## Victory and failure

### Victory — three phases, in order

1. **Phase 1 — Reach.** Any living marine ends a turn on the breach hex.
2. **Phase 2 — Seal.** A marine remains on the breach hex through one full end-of-round tick. If the marine is killed, stunned, or pushed off the hex during the round, phase 2 fails and the squad must restart phase 1 (a different marine, or the same one re-entering).
3. **Phase 3 — Extract.** Any living marine ends a turn on the extraction hex (SW corner).

### Failure

- `all_marines_exhausted`.
- (No round limit. The reinforcement pressure is the only timer beyond the card economy.)

## What this mission exercises

- **The action-economy clock at maximum pressure.** Every round the squad delays, the board gets harder. Refreshing has real campaign-equivalent cost.
- **Multi-phase victory state machine.** Three independent conditions, each gating the next.
- **Reinforcement spawn triggers.** A new rules-engine system, reusable for any future mission.
- **Interact-and-hold mechanic.** A marine "occupying" a hex for a duration is a new mechanic distinct from "reaching" or "attacking" it.
- **Squad coordination under fire.** Mixed-class squads (Iteration 2 deliverable) shine here — a Breacher to crack the lab door, a Tech-Specialist to seal the breach faster, a Boarding Marine to hold the line.

## Scope impact

New rules-engine work required (delivered in Iteration 2):

- **Reinforcement spawn system.** Map data gains a `reinforcement_triggers` array of `{trigger_kind, spawn_hexes, enemy_id, count}`. Engine fires triggers in the end-of-round tick. Roughly 1 day.
- **Multi-phase victory state machine.** Mission data gains a `victory_phases` array; the engine tracks which phase is active and evaluates only that phase's condition each turn. Roughly 1 day.
- **Interact-and-hold primitive.** A `hold_hex_for_n_rounds` victory condition that requires a marine to *remain* on a hex through a configurable number of end-of-round ticks. Half a day.
- **Multi-victory composition.** Phases reference existing victory primitives (`reach_hex`, `hold_hex_for_n_rounds`); composition is a refactor, ~half a day.

Total: ~3 days of new rules-engine work for this mission alone. Worth doing because it unlocks an entire archetype.

## Tuning notes (defer to Iteration 2 playtest)

- Reinforcement count per round. Start at 2 swarmers per round; drop to 1 if the mission is unwinnable.
- Phase 2 duration. Start at 1 full round on the breach; if too easy, extend to 2.
- Initial hostile count (5). Could go as low as 4 to make the early game less front-loaded.
- Reinforcement direction. Single-corridor spawn is intentionally exploitable — players can block it with a sacrificial marine. Acceptable. If it trivializes the mission, add a second spawn line from the east.

## Reserved variants

- **Containment — Evac.** Add 1–2 NPC scientists in the lab who must also reach the airlock.
- **Containment — Cascade.** Three breach hexes, must be sealed in sequence.
- **Containment — No Quarter.** Reinforcements never stop; victory is purely on extraction with the squad as intact as possible (scoring variant).
