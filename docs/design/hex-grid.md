# Hex Grid

The playfield. Coordinate system, neighbors, distance, movement, and walls.

## Orientation

**Flat-top** hexes. Each hex has flat top and bottom edges and pointed left/right corners. Stored internally as **axial coordinates** `(q, r)`. Cube coordinates `(x, y, z)` are derived on demand for symmetric math.

```
        +-----+
       /       \
      /  q,r    \
      \         /
       \       /
        +-----+
```

Reference: Red Blob Games "Hexagonal Grids" — we follow that convention exactly (`q` is the column-axis, `r` the row-axis; `x + y + z = 0` in cube form).

The MVP map is ~20×20 hexes. Maps are bounded rectangles in axial space.

## Neighbors

A hex has six neighbors at axial offsets:

| Direction | Δq | Δr |
|-----------|----|----|
| E         | +1 |  0 |
| NE        | +1 | −1 |
| NW        |  0 | −1 |
| W         | −1 |  0 |
| SW        | −1 | +1 |
| SE        |  0 | +1 |

(For flat-top; these are the standard axial offsets.)

> **TODO (naming consistency — non-blocking):** The direction *labels* in the
> table above (E/NE/NW/W/SW/SE) are pointy-top conventions, but the grid is
> rendered **flat-top**, where these axial offsets actually point N/S plus four
> diagonals (the E/W neighbors of a flat-top hex are its pointed corners, not
> edges). The axial *offsets* are correct and orientation-independent, so
> distance, neighbors, pathfinding, and line-of-sight are all unaffected — only
> the names are nominal. Resolve later by either (a) relabeling the directions
> for flat-top (e.g. N, NE, SE, S, SW, NW), or (b) switching the renderer to
> pointy-top. The code mirrors this note in `HexDirection` (`src/Core/Hex/`).

## Distance

Hex distance is computed in cube coordinates: `dist = (|dx| + |dy| + |dz|) / 2`. Range checks and movement cost both use this distance.

## Movement

Default movement is *walk*:

- Each entered hex costs 1 movement.
- A unit cannot enter a hex occupied by another unit.
- A unit cannot cross an edge wall (see below). Doors block movement only when closed.
- A unit cannot leave the map.

Pathfinding for AI uses A* over hex neighbors, with walls treated as blocked edges. See [../technical/architecture.md](../technical/architecture.md) for the module split.

Future movement types (reserved, not in MVP):

- **Jump N** — ignore intervening units and terrain, must land on a valid empty hex within range N.
- **Fly N** — ignore intervening terrain, no through-unit blocking.
- **Difficult terrain** — entry cost 2. Per-tile flag.
- **Hazardous terrain** — entry cost 1, deals damage on entry. Per-tile flag.

## Walls and edges

Walls live on **edges between hexes**, not on hexes themselves. An edge is identified by `(hex_a, hex_b)` where the two hexes are adjacent. Edges are undirected: `(a, b)` and `(b, a)` are the same edge.

Edge data:

- **Wall.** Solid. Blocks movement and line of sight.
- **Door.** A wall edge that can be in one of two states: open (passable, LoS through) or closed (blocking). See [line-of-sight-and-doors.md](line-of-sight-and-doors.md).
- **Open.** No wall; standard passable edge.

Map data stores only non-default edges (walls and doors). The default for every adjacency is "open."

## Map data shape

Maps are JSON files (schema in [../technical/data-model.md](../technical/data-model.md)) containing:

- Bounds in axial coordinates (min/max q, min/max r).
- Per-hex flags (in MVP just `floor` or `void`; `void` hexes are not part of the map).
- Edge overrides: list of `{a, b, kind, initial_state}` entries.
- Spawn points: list of `{kind, hex, payload}` for marines, hostiles, and scenario objects.
- Scenario metadata: id, name, victory and failure conditions.

## Coordinate display

For the player, hexes can be referenced as `q,r` in debug overlays. The end-user UI does not need to expose coordinates — they exist for designers and the dev console only.

## Rendering note

Pixel rendering of flat-top hexes uses width:height ratio of `2 : √3 ≈ 1.155`. With 32×32 tiles we will use a 32-wide, ~28-tall hex sprite and accept the minor rounding. See [../technical/art-pipeline.md](../technical/art-pipeline.md) for the exact sprite/grid math.
