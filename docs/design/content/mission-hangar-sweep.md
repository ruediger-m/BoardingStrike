# Mission: Hangar Sweep

The MVP mission. Archetype: **Sweep** (eliminate all hostiles). Setting: the wrecked frigate *Derelict Alpha*, drifting in unmarked space. The squad breaches at the airlock, works east through service corridors, fights through the hangar bay, and finishes in the bridge.

> Numbers are first-draft and tunable. Map coordinates are illustrative — final hex placement is fixed in Step 4 of the [iteration-1-mvp plan](../../plans/iteration-1-mvp.md).

## Identity

- **Mission id:** `mission_hangar_sweep`
- **Map id:** `map_derelict_alpha`
- **Name (display):** Hangar Sweep — Derelict Alpha
- **Archetype:** Sweep
- **Estimated optimal length:** 6–8 rounds

## Map shape

Bounds: roughly **q ∈ [0..19], r ∈ [0..15]** (~20×16 flat-top hexes). Four named zones, gated by doors. Top-down sketch (north up; `M` = marine spawn slot, `s` = swarmer, `S` = spitter, `=` = wall edge, `D` = door edge, `.` = floor, `~` = void/non-map):

```
       q→  0    5    10   15
   r↓
   0   . M M . . . . . . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~        AIRLOCK
   1   . M M . . . . . . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~        ANTECHAMBER
   2   . . . . . . . . . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~        (4×4-ish)
   3   . . . . D = = = = = ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
   4   ~ ~ ~ ~ . . . . . . . . . . . . . . . .        SERVICE
   5   ~ ~ . D . . . . . . . . . . . . . . . .        CORRIDOR
   6   ~ ~ . . . . . . . . . . . . D . . . . .        (3 rows wide)
   7   ~ ~ ~ ~ = = = = = = D = = = = = = = = =
   8   ~ ~ ~ ~ . . s . . . . . . S . . . . . .        HANGAR
   9   ~ ~ ~ ~ . . . . s . . . . . . . . . . .        BAY
  10   ~ ~ ~ ~ . . . . . . . . . . . . . . . .        (catwalk hex
  11   ~ ~ ~ ~ . . s . . . s . . . . . . . . .         at ~q=13,r=8)
  12   ~ ~ ~ ~ . . . . . . . . . . . . . . . .
  13   ~ ~ ~ ~ = = = = = = = = = = = D = = = =
  14   ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ . . s s . .        BRIDGE
  15   ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ . S . . . .        (5×4-ish)
```

The sketch is approximate — actual hexes follow the flat-top neighbor pattern, so columns step horizontally and rows vertically. Closet positions, exact door hexes, and the catwalk hex are pinned during map authoring in Step 4 of the build plan.

### Zones

1. **Airlock antechamber** (NW, ~4×4). Marine spawn. Open to the service corridor through one door on its east wall.
2. **Service corridor** (3 rows wide, running east from the airlock to the hangar). Two side closets:
   - **Closet A** opens off the corridor's south wall, near the airlock. Closed door at start.
   - **Closet B** opens off the corridor's south wall, ~midway. Closed door at start.
3. **Hangar bay** (large central area, ~10×8). Mostly open floor. A **raised catwalk** hex along the eastern wall (at ~q=13, r=8) gives a long line of sight back down the corridor — this is where the visible spitter sits.
4. **Bridge** (E side, ~5×4). Sealed behind a door on its west wall. Contains the final swarmers and the second spitter.

### Doors (5 total)

| # | Connects                        | Initial state |
|---|----------------------------------|---------------|
| 1 | Airlock ↔ Service corridor       | closed        |
| 2 | Service corridor ↔ Closet A      | closed        |
| 3 | Service corridor ↔ Closet B      | closed        |
| 4 | Service corridor ↔ Hangar bay    | open          |
| 5 | Hangar bay ↔ Bridge              | closed        |

The first door (airlock) being closed gives a beat-zero "stack up before breach" feel. Door 4 being open keeps the early-game flowing without forcing the player to spend a card on routine door work in the first round.

### Walls

Every hex edge between two adjacent zones that is not listed as a door is a solid wall. The hangar's interior is otherwise wall-free except for two short edge walls flanking the catwalk hex — these give the spitter cover from ranged attacks coming up the corridor (forces a marine to actually enter the hangar bay to land a clean shot).

## Spawns

### Marines (player spawn slots, 1–4)

Four marine slots in the airlock antechamber. Suggested hexes: `(1,0)`, `(2,0)`, `(1,1)`, `(2,1)`.

### Hostiles (12 total)

Visible at start:

- **4 swarmers** in the hangar bay, scattered roughly: `(6,8)`, `(8,9)`, `(6,11)`, `(10,11)`.
- **1 spitter** on the catwalk: `(13,8)`.

Revealed on door interactions:

- **2 swarmers** in Closet A (revealed when door 2 opens).
- **2 swarmers** in Closet B (revealed when door 3 opens).
- **2 swarmers** in the bridge: `(15,14)`, `(16,14)` (revealed when door 5 opens or visually if the player gains LoS through the open door).
- **1 spitter** in the bridge: `(15,15)`.

The closet swarmers serve as ambush units — opening a closet door for the first time triggers a "what's behind door 2" beat. They are placed on the map at scenario start (no dynamic spawn logic needed) but only visible when LoS reveals them. Since MVP has no fog of war, "revealed" in MVP just means "present from the start, behind a closed door" — they cannot path out until the door is opened.

## Victory and failure

- **Victory:** `eliminate_all_hostiles` — every hostile entity removed.
- **Failure:** `all_marines_exhausted` — every marine either at 0 HP or out of cards/discard.
- **Round limit:** none. The shrinking-hand pressure is the only clock.

## What this mission exercises

- **Door-as-pacing-tool.** Five doors, all meaningful. The player will spend bottom-half actions on door work; this validates that the bottom-half economy is interesting.
- **Long-corridor LoS.** The catwalk spitter has clean LoS down the service corridor. Marines must either advance under fire, blow a burn card to one-shot the spitter, or hug the corridor's north wall to break LoS.
- **Open-floor combat.** The hangar bay punishes stacking — swarmers can flank, the spitter can hit anyone. Forces the squad to spread out.
- **Action-card depth.** Optimal play uses all 10 cards across the squad. Players will refresh at least twice; the squad's "average" character will burn 2–3 cards by the bridge.
- **All three MVP conditions.** Wounded comes from spitters, immobilized from Frag and Slow Spit, stunned from Frag bottom. Players will see each at least once.

## Tuning notes (revisit after Step 8 playtest)

- If the catwalk spitter is too oppressive, replace one of its early-round AI cards with *Burrow* or reduce its damage modifier coverage. Alternatively, give it less cover (remove one of the flanking edge walls).
- If the closet ambushes are too cheap, place a second spitter on the catwalk or add a third closet.
- If players steamroll: reduce starting hand from 10 cards to 9 (the standard Gloomhaven approach for harder scenarios).
- If players never refresh: the mission is too short. Add 2 more swarmers in the hangar or extend the service corridor.

## Reserved variants (later iterations)

- **Hangar Sweep — Hard.** All doors locked at start; add a *hack* primitive to a future class card.
- **Hangar Sweep — Recovery.** Same map, add a survivor NPC to escort from the bridge back to the airlock. Escort archetype trial.

## Authoring checklist (for Step 4 of the build plan)

- [ ] Define all hexes (~280 floor hexes) and void hexes in `map_derelict_alpha.json`.
- [ ] Define edge walls between every adjacent zone except for the 5 doors above.
- [ ] Define the 5 doors with initial states.
- [ ] Define the 4 marine spawn slots.
- [ ] Define the 12 hostile spawns.
- [ ] Define `mission_hangar_sweep.json` referencing the map, with `victory = eliminate_all_hostiles` and `failure = all_marines_exhausted`.
- [ ] Validate the file loads cleanly with `JsonContentLoader`.
- [ ] Sanity-check that A* can path from every marine spawn to every hostile spawn (modulo doors).
