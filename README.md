# Boarding Strike

A hex-based, turn-based tactical squad game with a Gloomhaven-style action economy and a Space Hulk-inspired setting: marines boarding derelict ships and overrun colonies in a grimdark far future.

This repository currently holds the **Iteration 1 skeleton** — the project structure, an engine-independent gameplay solution, and a Godot presentation shell. Gameplay systems are built out per [docs/plans/iteration-1-mvp.md](docs/plans/iteration-1-mvp.md).

## Repository layout

```
BoardingStrike.sln        Solution for the engine-independent gameplay code
nuget.config              Pinned package source (reproducible restore on any machine/CI)
src/
  Core/                   Pure C#: hex math, pathfinding, FOV, RNG, data loading
  Game/                   Pure C#: rules, state, combat, AI, scenario control
  Game.Tests/             xUnit tests for Core + Game
godot/                    Godot 4 (.NET) presentation layer — references src/Core + src/Game
docs/                     Design, technical, and plan documents (start at docs/README.md)
.github/workflows/        CI: builds the solution and runs tests
```

The gameplay code (`src/`) references **no Godot types**. Only the `godot/` project depends on the engine. This keeps the rules engine testable headlessly and lets the renderer evolve (top-down → isometric → 3D) without touching gameplay. See [docs/technical/architecture.md](docs/technical/architecture.md).

## Prerequisites

- **.NET SDK 8.0+** (projects target `net8.0`; newer SDKs build it fine).
- **Godot 4.x (.NET / Mono build)** — only needed to run the presentation layer. Not required to build or test the gameplay code.

## Build & test the gameplay code

No Godot required:

```sh
dotnet test BoardingStrike.sln
```

That restores, builds `Core` + `Game` + `Game.Tests`, and runs the suite. To build only:

```sh
dotnet build BoardingStrike.sln
```

## Run the Godot presentation shell

Requires the **Godot 4 .NET edition** (not the standard build) and the **.NET 8 SDK**.

1. Open the **Godot 4 .NET** editor.
2. Import the project at `godot/project.godot`.
3. Build the C# solution with the **hammer icon** (it references `src/Core` and `src/Game`).
4. Press **Play** (F5). The scene loads the committed content, starts the Hangar Sweep mission, and renders the board: floor hexes, walls (dark), doors (orange = closed, teal = open), and unit markers (orange squares = marines, green triangles = swarmers, dark diamonds = spitters).
5. Press **F1** to toggle the hex-coordinate debug overlay.

The status line at the top shows the mission name and squad/hostile counts. (Turn playback and player input arrive in Steps 7–8; the board is currently a static render of the starting state.)

> The Godot project is intentionally **not** part of `BoardingStrike.sln`, so CI and `dotnet test` stay engine-free. The Godot editor builds `godot/BoardingStrike.Godot.csproj` itself.

## Documentation

All design and technical specs live in [docs/](docs/). Start at [docs/README.md](docs/README.md). The current build plan is [docs/plans/iteration-1-mvp.md](docs/plans/iteration-1-mvp.md).

## Status

- **Step 1 — project skeleton:** complete. Solution builds, tests pass, Godot project authored and compile-verified.
- **Step 2 — hex Core:** complete. `HexCoord` (axial + cube distance/neighbors), `HexEdge`, flat-top `HexLayout`, deterministic A\* (`HexPathfinder` over `IHexGraph`), and edge-line `LineOfSight` over `ISightMap` with the conservative corner rule.
- **Step 3 — JSON content loader:** complete. DTO records for every content category, `ContentVocabulary` of allowed enum values, `JsonContentLoader` (System.Text.Json, snake_case, comments/trailing commas) producing a validated `ContentDatabase`, and `ContentValidator` covering required fields, enum membership, ranges, and cross-references. Bad content fails with an aggregated, line-by-line error list.
- **Step 4 — Hangar Sweep content:** complete. Authored the full MVP content set under `godot/data/`: the 10 Boarding Marine cards, the `boarding_marine` class, the `husk_swarmer`/`cyst_spitter` enemies and their 16 AI cards, the two modifier decks, and the `map_derelict_alpha` map + `mission_hangar_sweep` mission. A test harness verifies reachability and door gating.
- **Step 5 — rules engine:** complete. The full turn/round loop is implemented in pure C#: a deterministic seeded RNG, the per-marine and shared-hostile modifier decks, the `ContentCatalog` mapping DTOs to domain entities, `BoardState` (live doors + occupancy), the `ActionResolver` (attacks with the modifier deck, moves, heals, conditions, doors), the `EnemyAi` executor (target priority + movement modes), and the `RoundResolver` (commit → initiative with the documented tie-breaks → execution → end-of-round tick → victory/failure), behind the public `ScenarioController` / `IMarineController`. 75 unit tests green, including a synthetic win/loss and a deterministic end-to-end run of Hangar Sweep (the squad breaches, engages, and the scenario reaches a terminal state).
- **Step 6 — Godot board view:** complete. `BoardView` renders the live map (flat-top hexes, walls, doors, unit markers) using `Core.HexLayout` as the shared geometry source; `ScenarioScene` loads content, starts the mission, frames the camera, and toggles an F1 coordinate overlay. Verified to compile against the Godot 4.6 API; visual confirmation is done in-editor.

Next up is **Step 7** (make units live: drive movement/attacks/conditions from the rules-engine event stream, add HP bars).
