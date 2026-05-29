# Mission: The Captain's Cabin

Iteration 2 mission. Archetype: **Reach hex with cleared radius** (a constrained Sweep variant). Setting: the derelict science vessel *Helios Station* `[PLACEHOLDER]`. Recover the captain's data core from the bridge-adjacent cabin while hostiles overrun the corridors.

> Numbers are first-draft and tunable.

## Identity

- **Mission id:** `mission_captains_cabin`
- **Map id:** `map_helios_station`
- **Name (display):** The Captain's Cabin
- **Archetype:** Reach-hex-with-radius-cleared
- **Estimated optimal length:** 5–7 rounds

## Map shape

Bounds: roughly **q ∈ [0..17], r ∈ [0..17]** (~18×18). Three named zones in a sideways-T layout:

```
       q→  0    5    10   15
   r↓
   0   . . . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ . . . .         CABIN
   1   . M . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ . X .         (objective hex X)
   2   . M . . ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ . . .
   3   . M . . . . . . . . . . . . . . .         SIDE ROOM 1
   4   . M . D . . . . D . . . . . . . .
   5   . . . . . . . . . . . . . . . . .         SPINE CORRIDOR
   6   . . . . . . . . . . . . D . . . .         (N-S, doors midway)
   7   ~ ~ ~ ~ . . . . . . . . . . . . ~
   8   ~ ~ ~ ~ . . . . . . . . . . . . ~
   9   ~ ~ ~ ~ D . . . . . . . . . . . ~
  10   . . . . . . . . . . . . . . . . .         SIDE ROOM 2
  11   . . . . . . . . . . . . . . . . .
  12   . . . . . . . . . . . . . . . . .
  ...
```

### Zones

1. **Crew quarters** (W side). Marine spawn. Several small cabins separated by partition walls. Three doors leading into the spine corridor.
2. **Spine corridor** (running roughly N-S through the center of the map). Two side rooms branch off it.
3. **Captain's cabin** (NE corner). Contains the **objective hex** (the data-core terminal).

### Doors and walls

Approximately 6 doors connecting the crew quarters to the spine and the side rooms to the spine. The captain's cabin entry has one door, initially closed. The side rooms have partial walls that give spitters cover with LoS into the spine — the bottleneck of the mission.

## Spawns

### Marines (4)

Four marine slots in the westernmost crew cabin.

### Hostiles (11 total)

- **6 swarmers** distributed: 2 in the crew quarters near the spine doors, 3 in the spine corridor, 1 patrolling near the captain's cabin door.
- **3 swarmers** in the captain's cabin behind the closed door.
- **2 spitters** dug into the side rooms with LoS into the spine.

## Victory and failure

- **Victory (primary):** `reach_hex_with_radius_cleared` — a living marine ends a turn on the objective hex, AND all hostiles within 3 hexes (path distance) of the objective have been eliminated.
- **Victory (alternative):** `eliminate_all_hostiles` — kept as a fallback so players who prefer the sweep clear get full credit.
- **Failure:** `all_marines_exhausted`.

The radius-cleared requirement is what gives this mission its identity: a marine can dash to the objective hex in 3–4 rounds, but unless the squad has cleared the threats around it, the dash is just a death sentence.

## What this mission exercises

- **Spatial pressure.** The objective pulls the squad east; refreshing in the west means losing time on the goal.
- **Bypass vs. engage decisions.** Several swarmers in the spine can be skipped if the squad commits to a fast push, but bypassed hostiles will follow.
- **The new "reach hex with radius cleared" win condition.** Validates a second victory kind in the rules engine.
- **Mixed-class synergy** (this is also an Iteration 2 deliverable — see [../../plans/iteration-2-class-diversity.md](../../plans/iteration-2-class-diversity.md)). The fast advance favors a mobility-focused class; clearing the radius favors area-control cards.

## Scope impact

New rules-engine work required (delivered in Iteration 2):

- **`reach_hex_with_radius_cleared` victory condition.** The rules engine must evaluate at end-of-turn: is any marine on the objective hex AND is the count of hostiles within N hexes of the objective zero? About half a day in `Game/Scenario/VictoryEvaluator.cs`.
- **`reach_hex` and `eliminate_all_hostiles` evaluated as alternatives.** Victory conditions become an array, any of which can trigger. Small refactor.

## Tuning notes (defer to Iteration 2 playtest)

- Radius value (3 hexes). If too generous, drop to 2; if punishing, raise to 4.
- Cabin spawn (3 swarmers). If the cabin always feels like a slog, drop to 2.
- Spitter LoS lanes. If players never engage the spitters and just run past, give the spitters one extra-range AI card so they can punish flyovers.

## Reserved variants

- **Captain's Cabin — Stealth.** Add a noise mechanic; opening doors aggros nearby hostiles.
- **Captain's Cabin — Holdout.** Same map, escort the data core back to the airlock after retrieving it (turns it into an escort mission).
