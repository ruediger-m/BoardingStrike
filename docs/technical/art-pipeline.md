# Art Pipeline

How pixel art is authored and consumed for the MVP. Constraints first, process second.

## Style targets

- **Tile size:** 32×32 pixels.
- **Camera:** Top-down. No perspective. North-up.
- **Palette:** Limited. Approx 24 colors total, organized as:
  - 6 greys for ship interior structure.
  - 4 metallics / accents for marine gear (matte grey, hi-vis orange, dim cyan light, dark blue).
  - 4 organic colors for Brood (sickly green primary, dull purple secondary, bruise yellow accent, blood red).
  - 4 warning / UI colors (amber, red alert, lime debug, white text).
  - ~6 neutrals (black, near-black, mid-grey, near-white, off-white, and one warm shadow tone).
  Locked once chosen; new art picks from the existing palette only.
- **Inspiration anchors:** *Into the Breach* (clarity, readability at small scale), *Heat Signature* and *Subset's other work* (UI craft), classic SNES top-down tactical games (the unfussy silhouette discipline).

## Hex sprite math

Flat-top hex with 32×32 sprite bounding box. The visible hex inscribes a regular hexagon roughly 32 wide × 28 tall (`32 / 1.155 ≈ 27.7`, round to 28). The remaining vertical space is transparent and used for sprite overlap on the row above.

- **Hex width on screen:** 32 px.
- **Hex height on screen:** 28 px.
- **Column step (q increment):** 24 px horizontal (3/4 of hex width), 0 px vertical.
- **Row step (r increment):** depends on parity — implement once in `Core/Hex/HexRender.cs`; this doc records the constants.

Sprites are drawn anchored at the hex center; transparent pixels in the corners are fine.

## Asset categories and filenames

### Tiles (`art/tiles/`)

- `floor_ship_metal.png` — base ship floor.
- `floor_biomass.png` — Brood-corrupted floor (decorative overlay, no gameplay effect in MVP).
- `floor_void.png` — null tile background.
- Wall and door edge sprites stored separately (see below).

### Edge overlays (`art/edges/`)

Edges are drawn on top of the tile layer. One sprite per edge direction × kind.

- `wall_<dir>.png` — for each of 6 edge directions (`e`, `ne`, `nw`, `w`, `sw`, `se`).
- `door_open_<dir>.png`
- `door_closed_<dir>.png`

This means 18 wall/door sprites total. Authoring them as flips and rotations of a base wall sprite is fine.

### Units (`art/units/`)

- `marine_default.png` — 32×32 single-frame placeholder for the MVP marine. Animation frames added later.
- `enemy_husk.png` — swarmer.
- `enemy_cyst.png` — spitter.
- Each unit sprite is 32×32 and centered on the hex.

### UI (`art/ui/`)

- Card frames (front face), card back, refresh button, HP icon, condition icons (`stunned.png`, `wounded.png`, `immobilized.png`), modifier-deck cards.

### Effects (`art/fx/`)

- Hit flash, ranged-shot tracer (a short line sprite), heal pulse, condition-apply puff. All single-frame or 2–3 frame for MVP.

## Animation budget

MVP animation is minimal:

- Units do not have walk cycles. Movement is a position tween over the path (linear, ~0.1s per hex).
- Attacks show a brief sprite tint flash on the attacker plus an effect sprite on the target. No swing or recoil animation.
- Doors have a simple open/closed pair, no in-between frames.
- Status conditions show a small icon attached to the unit; no animation.

Frame counts grow in later iterations.

## Authoring rules

1. **Palette-locked.** Every new pixel uses an existing palette color. Maintain `art/palette.png` as the canonical reference.
2. **No anti-aliasing.** Hard pixels only.
3. **Silhouette before color.** A sprite should be readable as a black silhouette before any interior detail is added.
4. **Visual contrast between factions.** Marines must read at a glance against Brood: marines lean cool greys + orange trim, Brood lean sickly green + dull purple.
5. **Consistent shadow direction.** Light from the upper-left. One-pixel dark accent on bottom-right edges where a form casts on itself.
6. **No text in sprites.** Strings come from UI, not art.

## File format and import

- PNG only. No PSD or PSB in version control.
- Import settings (Godot side): nearest-neighbor filter, mipmaps off, sRGB.
- Atlas: not in MVP. Each sprite is its own file. Atlas pass is an Iteration 4+ optimization.

## Placeholders

Until art lands, use solid-color stand-ins:

- Marine: orange square.
- Swarmer: green triangle.
- Spitter: dark green diamond.
- Wall: thick black line on the edge.
- Closed door: thick red line on the edge.
- Open door: thin red line on the edge.

The placeholder set is committed to the repo so the engine renders something visible from day one of Iteration 1.

## Sourcing

- Open-source / CC0 base assets are acceptable as starting points (e.g. OpenGameArt, Kenney). Any sourced asset is recolored to the project palette before use, with attribution noted in `art/CREDITS.md`.
- All custom art is original work, owned by the project.

## What lives in this doc vs Godot

This doc records *what* the art is. The Godot project records *how* it's imported and assembled (scene tree, animation players, tilemap configuration). When in doubt: design intent here, engine configuration in Godot.
