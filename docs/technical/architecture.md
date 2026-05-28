# Architecture

How the design docs map onto Godot 4 + C#. Aimed at someone about to open the project and start coding.

## Engine and language

- **Engine.** Godot 4.x (current stable when work begins).
- **Language.** C# for all gameplay code. Use the `.NET 8` SDK profile Godot expects.
- **Native code (C++).** Deferred. The architecture below names the modules that are *candidates* for a future C++ port via GDExtension: hex math, A* pathfinding, FOV. C# implementations should keep clean interfaces so a port stays surgical.
- **Editor.** Godot editor for scenes / nodes; any C# IDE for code (VS Code with Godot's C# tooling, JetBrains Rider, or Visual Studio).

## High-level separation of concerns

Three layers that talk in one direction only.

```
+--------------------------------------------+
|              Presentation layer            |   Godot scenes, sprites, UI, audio
|     (depends on Game; never the reverse)   |
+--------------------------------------------+
                     |
                     | events / view models
                     v
+--------------------------------------------+
|                Game layer                  |   Rules, state, commands, AI
|         (depends on Core; pure C#)         |
+--------------------------------------------+
                     |
                     | calls
                     v
+--------------------------------------------+
|                Core layer                  |   Hex math, A*, FOV, RNG, data loading
|       (no Godot dependencies; pure C#)     |
+--------------------------------------------+
```

The Core and Game layers are pure C# — they reference no Godot types. The Presentation layer is the only layer that touches `Godot.*`. This makes the rules engine independently testable and would let a future renderer (3D, isometric) swap in without disturbing gameplay code.

## Namespaces

- `BoardingStrike.Core` — primitives (hex coords, RNG, A*, FOV, JSON loading helpers).
- `BoardingStrike.Game` — domain model and rules (cards, units, board, scenario, AI, combat resolution).
- `BoardingStrike.Game.Commands` — discrete actions that mutate state (`PlayCardCommand`, `EnemyTurnCommand`, etc.). Commands return events the presentation layer subscribes to.
- `BoardingStrike.Game.Events` — read-only descriptions of what happened (`UnitMovedEvent`, `AttackResolvedEvent`, `ConditionAppliedEvent`).
- `BoardingStrike.Data` — JSON DTOs and the loader that produces Game-layer entities from them.
- `BoardingStrike.Presentation` — Godot scenes/nodes, UI controllers, sprite animators, sound players.

## Project layout

```
BoardingStrike.sln
src/
  Core/                       # pure C# library — no Godot refs
    Hex/                      # axial coords, neighbors, distance, line-cross
    Pathfinding/              # A* on hex grid with edge-wall awareness
    Fov/                      # LoS edge-line algorithm; FoV reserved
    Rng/                      # deterministic RNG with seed
    Data/                     # JSON DTO records, loader
  Game/                       # pure C# library
    Cards/                    # action cards, hands, decks, refresh, burn
    Combat/                   # attack resolution, modifier deck, conditions
    Units/                    # marines, hostiles, stats, conditions
    Board/                    # board state: hexes, edges, units, doors
    Ai/                       # enemy AI decks, target priority, behavior
    Scenario/                 # mission state, victory/failure, round/turn loop
    Commands/                 # mutation entry points
    Events/                   # event records emitted on state changes
  Game.Tests/                 # xUnit tests for Game and Core
godot/                        # Godot project root (one folder up so .sln stays clean)
  project.godot
  scenes/
    Main.tscn
    Scenario.tscn
    Hud/
    Unit/
    Card/
  scripts/                    # C# scripts that subclass Godot.Node
    Presentation/
  art/
    tiles/
    units/
    ui/
  data/                       # JSON content (cards, classes, enemies, maps)
    cards/
    classes/
    enemies/
    maps/
    modifier_decks/
docs/                         # this folder
```

Godot's `.csproj` references the `Core` and `Game` projects. Game logic *never* references Godot.

## Determinism

The rules engine must be reproducible given identical inputs.

- **One RNG, seeded per scenario.** Hold the seed on the scenario; persist it in save data. All randomness (modifier deck draws, AI deck draws, refresh burns) consumes from this RNG in a defined order.
- **No hash-order iteration.** Use deterministic collection types (sorted dictionaries, lists indexed by id).
- **No wall-clock or `DateTime.Now`** in the rules engine.
- **Tests assert event sequences**, not visual outcomes.

## Command/event flow

A turn proceeds via commands:

```
Player picks 2 cards
        |
        v
ScenarioController.SubmitPlayerCommits(...)
        |
        v
RoundResolver runs:
  - DrawEnemyAiCards()    -> AiCardsDrawnEvent
  - ResolveInitiative()    -> InitiativeOrderEvent
  - For each unit in order:
      ExecuteTurn()        -> per-turn events
  - EndOfRoundTick()       -> condition expiry events
  - VictoryCheck()         -> ScenarioEndedEvent (maybe)
```

Events are appended to a buffer. The Presentation layer reads the buffer and animates each event in order. Animation is purely cosmetic — state has already moved.

This pattern lets us:

- Save/load mid-scenario without coupling to animations.
- Run rules tests headlessly with no renderer.
- Eventually support replays.

## Module responsibilities

### Core / Hex

`HexCoord` (axial `q,r`), neighbors, distance, line crossing for LoS. Pure value types. Candidate for future C++ port.

### Core / Pathfinding

A* over a `IHexGraph` interface. The Game-layer board implements `IHexGraph` by exposing per-edge cost and walkability (respecting walls/doors and other units). Candidate for future C++ port.

### Core / FOV / LoS

Edge-line LoS query: `HasLineOfSight(board, from, to)`. The MVP exposes only LoS; FoV (per-unit visible hex set) is reserved. Candidate for future C++ port.

### Core / Data

DTO record types (`CardData`, `ClassData`, `EnemyData`, `MapData`, `ModifierDeckData`) + a `JsonContentLoader` that loads `data/` at startup and validates schemas. Schemas live in [data-model.md](data-model.md).

### Game / Cards

`ActionCard` (immutable), `Hand`, `Discard`, `BurnPile`, refresh logic, exhaustion check.

### Game / Combat

`AttackResolver` (range check, LoS check, modifier draw, damage application, condition application), `ModifierDeck`, `ConditionStore` on units.

### Game / Ai

`EnemyAiDeck`, `AiCard`, `TurnExecutor` for enemies. Reads target priority and executes movement + action against current board state.

### Game / Scenario

`ScenarioState` (the root rules state), `RoundResolver`, `ScenarioController` (the public API the presentation layer calls).

### Presentation

Godot nodes:

- `ScenarioScene` — owns a `ScenarioController` instance, listens to its event stream, drives the visualization.
- `BoardView` — renders the hex grid, walls, doors. Single tilemap-like node with per-edge overlays.
- `UnitView` — sprite + HP bar + condition icons. One per unit.
- `CardHandHud` — bottom-screen card hand for the active marine.
- `RoundHud` — initiative ladder, round counter, refresh prompt.
- `TooltipLayer` — hover info for hexes, units, cards.

The Presentation layer subscribes to Game events and animates them in sequence with a small per-event delay (e.g. 0.15s per move tween, 0.4s per attack swing). The player cannot input during the animation queue.

## Input

- **Mouse-first.** Click a card to preview it; click a target to confirm.
- **Hover** shows reachable hexes for a Move primitive and LoS / range overlays for an attack primitive.
- **Keyboard shortcuts** (1–9 for cards, R for refresh, space to confirm) added but not required for MVP.

## Saving

Save format is in [save-format.md](save-format.md). The MVP does not implement save/load — there is one scenario and the game ends with it.

## Testing strategy

- **Unit tests** in `Game.Tests` cover: hex math, pathfinding, LoS, combat resolution (including modifier-deck math with a stubbed RNG), card lifecycle, enemy AI decisions.
- **Scenario fixtures** — JSON scenarios with scripted RNG seeds, run end-to-end through the rules engine, asserting the resulting event stream. Built up over time; the first one ships with the MVP mission.
- **No UI tests in MVP.**

## What this architecture buys us

- Replace pixel renderer with isometric or 3D later — only Presentation changes.
- Swap C# modules for C++ (hex, A*, FOV) without touching gameplay code.
- Add an AI replay/test harness with no Godot dependency.
- Test rules headlessly in CI.
