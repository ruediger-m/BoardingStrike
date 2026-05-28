# Line of Sight and Doors

How sight is computed across the hex grid, and how doors work.

## Line of sight (LoS)

A unit at hex A has line of sight to hex B if and only if a straight visual line from A's center to B's center is not blocked by a wall edge or closed door edge along the way.

We use the **edge-line algorithm**:

1. Compute the straight line from the center of A to the center of B in pixel space.
2. For each adjacent hex pair along the path, check whether the segment crosses their shared edge.
3. If the crossed edge is a wall or a closed door, LoS is blocked.
4. If the line passes exactly through a corner (three or more hexes meet), LoS requires that **both** adjacent edge pairs are clear; if either is blocked, LoS is blocked. This is the conservative rule — it errs on the side of "you can't see around a corner."

LoS does not consume hexes' contents — other units (allied or hostile) do not block LoS. Only edge walls and closed doors do.

LoS is symmetric: A sees B iff B sees A.

For ranged attacks the attacker checks LoS, the target's range is the hex-distance, and the attack proceeds normally if both conditions hold.

## Field of view (FoV)

There is no fog of war in MVP. The entire map is visible to the player from the start. Fog of war is reserved for Iteration 3+ when scenarios begin including the "unknown room" mechanic.

When FoV is added, it will use the same edge-blocking model, computing each marine's visible set of hexes per round.

## Doors

A door is an edge that can be in state `open` or `closed`.

### Door interaction

A unit on either side of a door can spend the **Open / close door** primitive (typically a bottom action) to flip its state. The action targets a specific adjacent door edge.

### Door states

- **Closed.** Blocks movement and LoS. Visually rendered as a solid bar between the two hexes, distinct from a wall.
- **Open.** Does not block movement or LoS. Visually rendered as a thin marker on the edge.

### Door discovery

In MVP, all doors are visible to the player from the start. When fog of war arrives (later iteration), undiscovered doors still appear on the map outline so the player can plan around them, but discovering "what's behind" them is the gameplay.

### Reserved door variants

These are not in MVP; the data model leaves room for them:

- **Locked doors.** Require a key item or a *hack* action.
- **Vacuum-sealed doors.** Open at the cost of decompressing adjacent hexes (environmental hazard).
- **Damaged doors.** Cannot be closed once opened.
- **Reinforced doors.** Take multiple action halves to open.

### Doors in scenario flow

Doors are a primary pacing tool. A typical Space Hulk-style scenario uses doors to:

- Funnel the player into corridors where ranged attacks are strongest.
- Buy time when the swarm threatens to overrun.
- Gate access to objectives.

Door usage should be reflected in the MVP mission design even though only the basic open/closed states exist. See [content/](content/) (mission sheet to be added in a later iteration) and [../plans/iteration-1-mvp.md](../plans/iteration-1-mvp.md) for the MVP map's door placement.
