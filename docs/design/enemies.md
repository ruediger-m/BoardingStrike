# Enemies

The framework for enemy types and the AI deck that drives them. MVP enemies (swarmer, spitter) are in [content/enemy-swarmer.md](content/enemy-swarmer.md) and [content/enemy-spitter.md](content/enemy-spitter.md).

## What an enemy type is

An enemy type defines a unique behavior profile that may be instantiated multiple times in a scenario. All instances of one type share:

- **Stats.** HP, base attack damage(s), movement budget per action, ranges, etc.
- **AI deck.** A small deck (6–10 cards) of action scripts. One card is drawn per round and dictates all of that type's units' behavior that round.
- **Modifier deck.** MVP: shared hostile modifier deck (see [combat.md](combat.md)).

A *hostile* in a scenario is an instance of an enemy type with its own HP, position, and active conditions.

## How an enemy turn resolves

See [core-loop.md](core-loop.md) for the round structure. When it is an enemy type's initiative slot:

1. The drawn AI card is already revealed (it was drawn during the commit phase).
2. Each living hostile of this type acts in spawn order.
3. Each hostile reads the AI card top-to-bottom and executes its instructions against its own position and target priorities.

There is no per-hostile branching. All swarmers do the same thing this round; they just do it from different positions.

## AI card grammar

An AI card has:

- **Id, name, initiative number.**
- **Movement.** How far the unit can move this turn. May be 0 (does not move) or `N`.
- **Action.** One or more action primitives executed after movement (or interleaved with it; the card specifies order).
- **Target priority.** Whom this unit prefers to attack/move toward. Default ranking, overridable per card:
  1. Closest enemy (marine) by movement path, then by hex distance as tiebreak.
  2. Enemy with lowest current HP.
  3. Lowest unit id as final tiebreak (deterministic).
- **On-hit conditions.** Conditions inflicted, if any.
- **Notes.** Designer-visible text explaining intent. Not shown in game.

Example pseudocode card:

```
id: swarmer_charge
name: Charge
initiative: 22
movement: 4
action: attack-melee 2
target: closest_marine
notes: classic rush-and-bite
```

This says: each swarmer moves up to 4 hexes toward the closest marine; if it ends adjacent, it attacks for base 2.

## Determinism

All enemy behavior is deterministic given:

- The drawn AI card.
- The current board state.
- The (declared) target priority and tiebreak rules.

A future replay/test pass should be able to reproduce identical enemy turns given identical inputs. The only randomness in the enemy turn is the modifier-deck draw for each attack.

## Spawning

Enemy spawn is part of scenario data:

- **Initial spawns** are listed in the map's spawn-points block.
- **Reinforcement spawns** trigger on round number or event (e.g. door opened, hex entered). Reserved in the data model, not used in MVP beyond a single trigger if needed for pacing.

## Adding a new enemy type (process)

1. Write a content sheet in `content/enemy-<name>.md` using the swarmer or spitter as a template.
2. Define base stats and the AI deck (6–10 cards typical).
3. Identify the type's **tactical role** (rusher, sniper, controller, tank, summoner) and ensure no redundancy with existing types in the same scenario.
4. Add a JSON data file under `data/enemies/<id>.json`.

## MVP scope

Two enemy types only:

- **Swarmer** — melee rusher. Numerous, fragile, fast. See [content/enemy-swarmer.md](content/enemy-swarmer.md).
- **Spitter** — ranged controller. Fewer, slower, applies conditions. See [content/enemy-spitter.md](content/enemy-spitter.md).

Together they exercise the squad's full toolkit: swarmers force melee and movement decisions, spitters force LoS and cover decisions.
