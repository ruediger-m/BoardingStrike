# Glossary

Shared vocabulary used across all design docs. When a term is defined here, other docs use it without redefinition.

## Mechanics

- **Round.** One full pass through the initiative order. All living units act once per round.
- **Turn.** A single unit's action within a round. A turn is bounded by *play card(s) → execute → end*.
- **Initiative.** A numeric value that determines turn order within a round. Lower numbers act first. For player characters, initiative is set by the cards they choose to play that round (see [design/core-loop.md](design/core-loop.md)). For enemies, initiative is set by the card drawn from that enemy type's AI deck.
- **Action card** (or just **card**). A single piece of the action economy. Each card has a *top action* (typically the heavier action, e.g. an attack) and a *bottom action* (typically the lighter one, e.g. a move), plus an *initiative number*. A character plays two cards per turn and chooses one half from each.
- **Hand.** The cards a character currently has available to play.
- **Discard.** Cards a character has spent this mission but can recover with a short rest.
- **Burn pile / lost pile.** Cards that are gone for the rest of the mission. Cards can be burned voluntarily for a stronger effect or as the cost of a refresh action.
- **Refresh (short rest).** Recover all discarded cards, then randomly burn one. The hand shrinks by one card permanently for this mission.
- **Exhaustion.** A character with no cards available is *exhausted* and removed from the mission until it ends.
- **Modifier deck.** A small deck of attack-modifier cards (×0, ×2, +/-1, +/-2, plus a few special effects). Drawn once per attack to apply a multiplier or bonus/penalty to the attack's base damage. See [design/combat.md](design/combat.md).
- **Status condition.** A temporary state attached to a unit (e.g. *stunned*, *wounded*, *immobilized*, *poisoned*). Defined in [design/combat.md](design/combat.md).
- **Hex.** One tile of the playfield. The map uses **flat-top** axial hex coordinates. See [design/hex-grid.md](design/hex-grid.md).
- **Edge wall.** A blocker that sits on the *edge* between two hexes, not on either hex itself. Walls block movement and line of sight unless the edge contains a door.
- **Door.** A wall edge that can be opened or closed by an adjacent unit at a small action cost. Doors block movement and LoS when closed.
- **Line of sight (LoS).** Whether a unit can see a target hex through walls and doors. Rules in [design/line-of-sight-and-doors.md](design/line-of-sight-and-doors.md).

## Entities

- **Squad.** The player's controllable units in a mission. MVP squad size is four. Long-term squad size is 4–6 depending on mission.
- **Marine.** A player-controlled unit. The MVP has one marine class (Boarding Marine); later iterations add more.
- **Class.** The shared template for a set of marines: a card deck, base stats, and starting modifier deck.
- **Hostile.** A non-player unit. Hostiles never play action cards from a hand; they execute scripted behavior from an *enemy AI deck* keyed to their type.
- **Enemy type.** A unique behavior profile (e.g. *swarmer*, *spitter*). One AI deck per type. Multiple individuals of the same type all act according to the single card drawn for that type this round.

## Campaign

- **Scenario** (or **mission**). A single tactical encounter. Has a map, hostile spawns, victory and failure conditions, and post-mission rewards.
- **Campaign map.** The branching node graph of scenarios the player navigates between missions. Deferred to Iteration 3.
- **Roster.** The persistent list of marines available to the player across missions. Deferred to Iteration 3.

## Tech

- **Scene** (Godot). A reusable hierarchy of nodes saved as a `.tscn` file. The game's screens, units, and maps are scenes.
- **Resource** (Godot). A serializable data object (`.tres` / `.res`). We use Godot Resources sparingly — most content is JSON. See [technical/data-model.md](technical/data-model.md).
- **Content data.** JSON files under `data/` that define cards, classes, enemies, modifier decks, missions. Loaded at startup or scenario start.
