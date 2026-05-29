# Vision

## Pitch

Boarding Strike is a turn-based tactical squad game inspired by **Gloomhaven** (action-economy and campaign progression) and **Space Hulk** (claustrophobic boarding actions against hostile xenos in derelict installations). The player leads a small squad of marines through a branching campaign of scenarios set in lost ships, abandoned stations, and overrun colonies on the frontier of human space.

The signature design hook is the action economy: each turn, every character commits two cards from their personal hand. Each card has a *top* and *bottom* action — you pick one of each. Cards are spent until the hand is empty, then a refresh action restores them — minus one card, permanently burned for the rest of the mission. The player is on a fuse: powerful plays today shorten the mission's remaining time.

## Pillars

1. **Time pressure as a resource.** The player is always asking "can I afford to refresh yet?" The shrinking hand is the heart of the tension.
2. **Tactical legibility.** Every enemy action is determined by a visible deck. No hidden AI. The puzzle is in reading the board, not in guessing the opponent.
3. **Squad identity over hero power.** Marines are interchangeable in lore but unique in build. Class diversity and gear matter more than individual stats.
4. **Grimdark, but not nihilistic.** The setting is bleak and the missions are dangerous, but the squad's job is meaningful: contain outbreaks, recover survivors, deny ground to the xeno threat.

## What this game is not

- Not a real-time tactics game. There is never a clock outside the player's turn.
- Not a base-building or strategy-layer game in the X-COM mold. The campaign layer is a branching scenario map, not a strategic simulation.
- Not an open-world game. Missions are discrete scenarios on a hex grid.
- Not a roguelike. Squad members are persistent and named; deaths cost real campaign progress.

## Iteration roadmap

Each iteration is a self-contained vertical slice with its own plan. Later iterations expand scope; the core action-economy loop is locked from Iteration 1.

- **Iteration 1 — MVP Vertical Slice.** One handcrafted mission ([Hangar Sweep](design/content/mission-hangar-sweep.md)), one class (Boarding Marine, four identical squad members), two enemy types (swarmer + spitter), full action/refresh loop, line of sight, doors, three status conditions, deterministic + modifier-deck combat resolution, 32×32 pixel top-down. No campaign layer yet; the mission stands alone. See [plans/iteration-1-mvp.md](plans/iteration-1-mvp.md).
- **Iteration 2 — Class Diversity and Mission Variety.** Two additional classes with distinct decks (Breacher and Tech-Specialist), three more enemy types, overwatch/reaction-fire mechanic, mission-select screen, and **two new missions**: [Captain's Cabin](design/content/mission-captains-cabin.md) (reach-hex-with-radius-cleared archetype) and [Containment](design/content/mission-containment.md) (multi-phase interact-and-hold with reinforcements). See [plans/iteration-2-class-diversity.md](plans/iteration-2-class-diversity.md).
- **Iteration 3 — Campaign Layer.** Persistent roster, branching scenario map, between-mission upgrades, save/load.
- **Iteration 4 — Content & Polish.** More missions, more enemies, environmental hazards (vacuum, fire, radiation), narrative beats between missions, audio first pass.
- **Iteration 5+ — Presentation evolution.** Move to isometric, then optionally full 3D. UI polish. Modding hooks.

The roadmap is a sketch, not a contract. Each iteration's scope is finalized in its plan doc at the start of that iteration.

## Tech stack at a glance

- **Engine:** Godot 4 (with the C# bindings).
- **Language:** C# for all gameplay code. Native C++ via GDExtension is deferred until profiling proves it necessary, with hex math / pathfinding / FOV called out as the likely candidates.
- **Art:** 32×32 pixel tiles, limited palette, top-down (Into-the-Breach-adjacent). Open-source pipeline only.
- **Data:** Cards, enemies, maps, and missions are defined in JSON files under `data/` and loaded at runtime. Code does not hardcode content.
- **Saves:** JSON files in the user data directory, human-readable. Save format is specified now but not implemented until the campaign layer arrives.

See [technical/architecture.md](technical/architecture.md) for the full breakdown.

## Open questions

These are knowingly deferred and revisited later:

- Multiplayer / hot-seat — not in scope for any current iteration, but the data model should not preclude it.
- Modding — desirable, falls out naturally from the JSON content pipeline. No formal mod API yet.
- Audio — handled by Godot's mixer, not specified beyond placeholders until Iteration 4+.
- Narrative scope and tone of voice — `[PLACEHOLDER]` faction names in the world doc; full setting bible deferred.
