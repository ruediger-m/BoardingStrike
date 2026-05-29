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

1. Open the **Godot 4 .NET** editor.
2. Import the project at `godot/project.godot`.
3. Let Godot build the C# solution (it references `src/Core` and `src/Game`).
4. Press **Play**. The skeleton scene prints a sign-of-life line to the Output panel and shows it on screen, confirming the presentation → game → core boundary is wired:

   ```
   [BoardingStrike] Game layer online — Boarding Strike 0.1.0-dev (ping 1)
   ```

> The Godot project is intentionally **not** part of `BoardingStrike.sln`, so CI and `dotnet test` stay engine-free. The Godot editor builds `godot/BoardingStrike.Godot.csproj` itself.

## Documentation

All design and technical specs live in [docs/](docs/). Start at [docs/README.md](docs/README.md). The current build plan is [docs/plans/iteration-1-mvp.md](docs/plans/iteration-1-mvp.md).

## Status

- **Step 1 — project skeleton:** complete. Solution builds, tests pass, Godot project authored and compile-verified.
- **Step 2 — hex Core:** complete. `HexCoord` (axial + cube distance/neighbors), `HexEdge`, flat-top `HexLayout`, deterministic A\* (`HexPathfinder` over `IHexGraph`), and edge-line `LineOfSight` over `ISightMap` with the conservative corner rule. 44 unit tests green.

Next up is **Step 3** (JSON content loader: DTOs + validation for cards, classes, enemies, maps, missions).
