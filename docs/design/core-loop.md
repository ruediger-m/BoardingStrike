# Core Loop

This doc specifies the structure of a round and a turn. The action-card economy (what cards do, refresh, exhaustion) is in [action-system.md](action-system.md). Combat resolution (hit, damage, modifier deck) is in [combat.md](combat.md).

## Goal

The player should always feel a steady squeeze on time. Every refresh shortens the mission. Every burned card narrows the options. Reading the enemy's drawn AI card and the initiative order is the puzzle.

## The Round

A round consists of:

1. **Card commit phase.** Each living squad member's controller picks two cards from their hand and places them face down. Each enemy type that has any units alive draws one card from its AI deck. Initiative is now determined.
2. **Initiative reveal.** All committed cards are revealed. Units are sorted by the initiative number of the player card with the *lower* initiative for player units, or by the AI card's initiative for enemy types. Lower acts first. Ties broken by a fixed rule (player units before enemies on ties; among players, the squad's commit order from setup; among enemies, alphabetical by type id — deterministic, no random tiebreak).
3. **Turn execution.** Each unit takes its turn in initiative order. A unit's turn cannot be interrupted by another unit's turn (overwatch/reaction-fire is deferred to Iteration 2).
4. **End of round.** Apply end-of-round status condition ticks (poison damage, condition expiry). Check victory/failure. Begin next round.

A scenario ends mid-round only on instant-win or instant-loss conditions (e.g. all marines exhausted, or scenario objective fulfilled).

## The Player Turn

When it is a marine's initiative:

1. The controller chooses which committed card supplies the *top* action and which supplies the *bottom* action. The other half of each card is discarded unused.
2. The top action is executed.
3. The bottom action is executed.
4. The order between top and bottom is the player's choice unless a card explicitly says otherwise.
5. Both played cards go to the discard pile (unless a card was *burned* — see [action-system.md](action-system.md)).
6. A marine may, instead of playing two cards, declare a **refresh** (see [action-system.md](action-system.md)). A refresh is the marine's entire turn.

A marine with cards still in hand *must* play two cards or declare a refresh; they cannot pass.

## The Enemy Turn

When it is an enemy *type*'s initiative, every living unit of that type takes a turn in the order they were spawned. Each unit executes the same drawn AI card's instructions, evaluated independently for that unit's position and target priorities.

There is no per-unit AI card — only per-type. This is intentional: it concentrates the player's reading effort on a small number of decks, not on every individual hostile.

See [enemies.md](enemies.md) for AI card grammar and target-priority rules.

## End-of-Round Tick Order

Strict order so behavior is reproducible:

1. **Poison / damage-over-time** is applied to every affected unit.
2. **Condition expiry**: any condition with `expires: end_of_round` is removed.
3. **Cooldowns** on any per-round abilities (none in MVP, but reserve the slot).
4. **Victory / failure check.**

## Scenario Start

1. Load mission data: map, hostile spawns, victory/failure conditions, environmental flags.
2. Each marine builds their hand by selecting from their class's full deck (deck-building step). MVP: each marine starts with their class's full deck in hand; the deck-build step is deferred to Iteration 2 progression.
3. Place units on starting hexes.
4. Round 1 begins.

## Scenario End

- **Victory** triggers when the scenario's victory condition is met (MVP: all hostiles defeated; later scenarios may use objective-based conditions).
- **Failure** triggers when all marines are exhausted or killed.
- A post-scenario screen shows mission stats (rounds elapsed, cards burned, hostiles killed). Rewards (XP, loot) are computed in later iterations.
