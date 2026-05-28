# Action System

This doc specifies how player characters spend, recover, and burn cards. Round and turn structure is in [core-loop.md](core-loop.md).

## Card anatomy

Every action card has:

- **Id.** Unique slug, e.g. `marine_overcharge_shot`.
- **Name.** Display name.
- **Class.** Which class can include it.
- **Level.** 1 for starter cards; higher tiers gated behind class progression (deferred past MVP).
- **Initiative.** A number 01–99 (lower acts earlier in the round). Each half of the card does not have its own initiative — the whole card has one.
- **Top action.** A block of action effects, typically the heavier action.
- **Bottom action.** A block of action effects, typically the lighter action.
- **Top burn?** Boolean. Whether the top action burns the card when used.
- **Bottom burn?** Boolean. Same, for the bottom action.

Default design rule: more powerful halves are gated behind burning. Most cards have one non-burn half and one burn half, so the player decides whether to spend the card now or keep it cycling.

The data schema is in [../technical/data-model.md](../technical/data-model.md).

## Action types (MVP)

The MVP card pool uses these action primitives. Cards combine them in their top/bottom blocks.

- **Move N.** Move up to N hexes. Default movement type is *walk* — costs 1 movement per hex entered, blocked by walls and other units.
- **Attack-melee N (range 1).** Make a melee attack with base damage N against an adjacent hex.
- **Attack-ranged N (range R).** Make a ranged attack with base damage N at any hex within range R that the attacker has line of sight to.
- **Heal N (self or adjacent).** Restore N HP and remove the *wounded* condition.
- **Apply condition X.** Inflict a status condition on the target. See [combat.md](combat.md).
- **Open / close door.** Manipulate an adjacent door edge.
- **Loot.** Interact with a scenario object on the current hex. Reserved; not in MVP.

A single action half can chain multiple primitives, e.g. *Move 2, then Attack-melee 3*.

## A turn in detail

A marine's turn is described in [core-loop.md](core-loop.md). The key invariant is: a marine plays *one top action* and *one bottom action* from the two cards they committed this round. The unused halves are discarded with the card.

A card cannot be "saved for later" — once committed, both halves resolve or are discarded.

## Refresh (short rest)

When a marine has at least one card in their discard pile and no cards in hand, or simply chooses to refresh proactively, they may declare a refresh as their entire turn. The refresh:

1. Returns all cards from the discard pile to the hand.
2. Randomly **burns one card** from the recovered set, removing it from play for the rest of the scenario.

A marine can declare a refresh at any initiative slot. The marine still acts on the round when they refresh — refreshing is their turn.

A *long rest* (refresh with player-chosen burn instead of random) is deferred to Iteration 2.

## Burning cards

A card is burned in one of three ways:

1. **Forced burn from refresh.** The random card removed during refresh.
2. **Voluntary burn.** Some action effects say "burn this card." The player chooses to spend the card permanently for a stronger effect (e.g. an "overcharge shot" top action that does +3 damage but burns).
3. **Cost burn.** Some effects require burning a card from hand as a cost (e.g. a defensive bottom action that triggers when hit, burning a card to halve the damage). Out of MVP scope.

Burned cards return to the player's deck at scenario end. Card progression (gaining new cards, upgrading levels) is deferred past MVP.

## Exhaustion

A marine is *exhausted* when:

- They cannot play two cards on a turn (because hand has fewer than two cards) **and** they cannot refresh (because discard is empty), **or**
- They are reduced to 0 HP.

An exhausted marine is removed from the map. Their tokens, gear, and remaining cards are set aside for scenario end. They are not dead — they are out of this mission. Across iterations 1–2, exhaustion has no campaign cost; from Iteration 3 onward an exhausted marine may carry a recovery penalty into the next scenario.

If all marines in the squad are exhausted, the scenario fails.

## MVP starting hand

Each Boarding Marine starts the scenario with their class's full deck of 10 cards in hand. No pre-mission deck-building, no card selection. See [content/class-boarding-marine.md](content/class-boarding-marine.md) for the card list.

This means a marine can play at most five rounds before forcing a refresh, and each refresh shortens the remaining play by one round on average. The MVP mission should be tuned so an optimal player can finish in ~6–8 rounds.

## Design intent and tradeoffs

- The two-cards-per-turn structure forces tempo decisions: a marine cannot use a single card's "best" half multiple times.
- Initiative-on-the-card couples *what you do* with *when you do it*. A faster (lower-init) action is often the weaker one, creating a real choice.
- The shrinking hand is a clock. The player should always be aware that aggressive play costs future turns.
- Top/bottom halves create deck-building puzzles in later iterations: a hand of all-heavy-tops gives raw power but no mobility.

## Out of MVP scope

- Pre-mission deck-build (selecting a subset of class cards). Deferred.
- Long rest with chosen burn. Deferred.
- Reaction triggers (overwatch, on-hit defensive cards). Deferred to Iteration 2.
- Card-level progression and upgrades. Deferred to Iteration 3+.
- Equipment cards / item cards. Deferred.
