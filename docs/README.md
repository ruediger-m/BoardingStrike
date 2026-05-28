# Boarding Strike — Design Docs

Boarding Strike is a hex-based, turn-based tactical squad game with an overarching campaign. The player commands a squad of marines boarding derelict spacecraft, abandoned stations, and overrun mining colonies in a far-future grimdark setting. Combat is built around a Gloomhaven-style card-hand action economy: each turn you commit two action cards, and resting to recover them shrinks your usable hand for the rest of the mission.

This folder is the source of truth for design decisions. It is the input to implementation plans, not a record of finished work.

## How to read these docs

- **Vision and scope** live in [vision.md](vision.md). Start here for the elevator pitch and the iteration roadmap.
- **Mechanical specs** live in [design/](design/). Each doc covers one system at a level a developer or designer can build against.
- **Content sheets** for specific classes and enemies live in [design/content/](design/content/). These are templates and the first three MVP items.
- **Engineering specs** live in [technical/](technical/). These are how the design docs map onto Godot + C# + data files.
- **Implementation plans** live in [plans/](plans/). One plan per iteration. Plans cite design docs by file path.
- **Shared vocabulary** lives in [glossary.md](glossary.md). When a term is defined there, design docs use it without redefining.

## Conventions

- Docs are iteration-aware. Where a feature is deferred past the current iteration, the doc states so explicitly so readers do not assume MVP coverage.
- Placeholder names (faction names, mission names) are tagged `[PLACEHOLDER]` so they can be swept in one pass.
- Numbers (HP, damage, ranges) in content sheets are first-draft and explicitly labeled as tunable.
- File references use repo-relative paths so links resolve in any markdown viewer.

## Current iteration

[Iteration 1 — MVP Vertical Slice](plans/iteration-1-mvp.md): one handcrafted mission, four identical Boarding Marines, two enemy types (swarmer + spitter), full action/refresh loop, line of sight, doors, basic conditions. Built in Godot 4 with C#, 32×32 pixel art top-down.
