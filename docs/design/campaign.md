# Campaign

Persistent state across scenarios: the squad roster, the branching scenario map, and inter-mission progression. **All material in this doc is deferred to Iteration 3.** It is captured now so the MVP code does not paint us into a corner.

## Goal

Make missions feel like episodes of an ongoing operation. Decisions in one mission affect future ones, marines accumulate identity, and the player chooses a route through a branching scenario map.

## Roster

A persistent list of marines across scenarios.

- **Roster size.** Larger than squad size. The campaign opens with ~6 marines; the player picks 4 per mission.
- **Persistence.** Marines retain identity, level, gear, modifier-deck upgrades, personal goal progress.
- **Recovery.** A marine exhausted in a scenario survives but may carry over a *fatigued* condition into the next scenario unless the player rests them.
- **Replacement.** When a marine retires (via personal goal) or is permanently lost (a future scenario stake), a new recruit becomes available.

## Branching scenario map

A node graph of scenarios.

- **Nodes** are scenarios. Each has a state: locked, available, completed, failed.
- **Edges** are choice paths. Completing a scenario unlocks edges to next nodes; the player chooses which to take.
- **Branches** can converge again. Some scenarios may be reachable from multiple parents.
- **Side scenarios** are optional nodes that grant extra resources but cost no narrative time.

The branching topology of the first campaign arc is **deferred**; we will design it when Iteration 3 begins, informed by which mission archetypes we have built by then.

## Mission archetypes

A loose taxonomy to keep variety up:

- **Sweep.** Eliminate all hostiles. The MVP mission is this archetype.
- **Extraction.** Reach a hex with a survivor and escort them to an exit.
- **Sabotage.** Reach a hex, spend N consecutive turns interacting, escape.
- **Defense.** Hold a hex / area for N rounds against spawn waves.
- **Investigate.** Open doors / loot to find a hidden objective hex.

Iteration 3 ships one of each non-sweep archetype, minimum.

## Progression rewards

Per scenario completion, the player gains:

- **XP** for each surviving marine, less for exhausted marines. Hits a level threshold to unlock card upgrades.
- **Credits / supplies.** A shared squad resource for between-mission purchases (cards, gear, modifier-deck upgrades). Exact economy deferred.
- **Map progress.** Unlocks the next scenario nodes.

## Save / load

JSON file in the user data directory. Schema is in [../technical/save-format.md](../technical/save-format.md). The file contains:

- The current campaign state (which scenarios completed, which available).
- The roster (per-marine state: level, card upgrades, gear, conditions carried over, personal goal progress).
- Shared resources.
- A timestamp and version field for compatibility.

Auto-save triggers at scenario start, scenario end, and on map-node selection. Manual save is offered from the main menu only (no in-scenario saves, to preserve the consequence weight of card choices).

## Failure handling

What happens when a scenario fails?

- **Soft fail.** Surviving marines retreat with no XP and no rewards. Exhausted marines carry *fatigued* into next mission. The scenario node remains available for retry.
- **Hard fail (campaign-ender).** Total Party Kill — campaign ends in a defeat screen offering a restart. Reserved for the most narrative scenarios; not the default.

## Out of MVP scope

Everything in this doc. The MVP has a single standalone scenario with no persistence. The data model should not invent campaign data structures yet, but the save-format spec exists ([../technical/save-format.md](../technical/save-format.md)) to anchor decisions.
