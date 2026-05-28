# Class: Boarding Marine

The MVP's only class. Generalist marine with kinetic primary and short-range tools. Designed so a four-marine squad can handle both melee swarmers and ranged spitters using only this class.

> Numbers are first-draft. Expect tuning during MVP playtest. All values explicitly tunable.

## Identity

- **Id:** `boarding_marine`
- **Name:** Boarding Marine
- **Tagline:** Vanguard frontline. Mag-boots, carbine, breaching kit.
- **Lore blurb:** The generalist deployed by the Vanguard for first-response boarding actions. Hard suit, kinetic carbine, knife. Expected to handle whatever the derelict throws at them until support arrives — which it never does.

## Stats

| Stat | Value |
|------|-------|
| HP | 10 |
| Sprite | `marine_default.png` (placeholder) |
| Audio | placeholder |
| Modifier deck | standard 20-card (see [../combat.md](../combat.md)) |

## Card pool (10 level-1 cards)

The MVP marine has all 10 cards in hand at scenario start. No deck-build step.

The shorthand below describes each half. Initiative number in the header. Burn icons in **bold** mean that half burns the card.

---

### 01 — Snap Shot — init 16

- **Top:** Attack-ranged 2, range 4. *(non-burn)*
- **Bottom:** Move 2. *(non-burn)*

The reliable workhorse. Fast initiative, basic action both halves.

---

### 02 — Carbine Burst — init 28

- **Top:** Attack-ranged 3, range 4. *(non-burn)*
- **Bottom:** Move 2, then Attack-ranged 1, range 3. **(burn)**

Solid top, situational burn bottom for a kite-and-shoot.

---

### 03 — Knife Work — init 12

- **Top:** Attack-melee 3. *(non-burn)*
- **Bottom:** Move 3. *(non-burn)*

Fast melee plus the longest non-burn move on a single half.

---

### 04 — Overcharge Shot — init 35

- **Top:** Attack-ranged 5, range 4. **(burn)**
- **Bottom:** Move 2. *(non-burn)*

Burn-for-power top. The card players hold for spitters.

---

### 05 — Breach and Clear — init 24

- **Top:** Move 1, then Open/close adjacent door, then Attack-melee 2 on the first enemy adjacent after movement. **(burn)**
- **Bottom:** Open/close adjacent door. *(non-burn)*

The doorway tool. Bottom is the routine "open the door"; top is a one-shot dramatic breach.

---

### 06 — Frag — init 40

- **Top:** Attack-ranged 2 on a target hex within range 3; apply *immobilized* to the target on hit. **(burn)**
- **Bottom:** Apply *stunned* to an adjacent target. *(non-burn)*

The crowd-control card. Bottom is a reliable stun, top is a one-shot grenade pin.

---

### 07 — Combat Stim — init 09

- **Top:** Self heal 3, remove *wounded*. **(burn)**
- **Bottom:** Move 1. *(non-burn)*

The emergency self-patch. Lowest initiative card — refresh-stim-rush plays go first.

---

### 08 — Suppressing Fire — init 21

- **Top:** Attack-ranged 2, range 4. Target gets *muddled* (reserved condition, ignore in MVP — treat as *immobilized* for MVP scope). **(burn)**
- **Bottom:** Attack-ranged 1, range 4. *(non-burn)*

> **MVP tuning note:** since *muddled* is reserved past MVP, the MVP version of the top action applies *immobilized* instead of *muddled* so the card still has flavor.

---

### 09 — Push Forward — init 18

- **Top:** Move 4. *(non-burn)*
- **Bottom:** Move 2, then Attack-melee 2. **(burn)**

Mobility-focused. Top is the longest non-burn move; bottom is a charge.

---

### 10 — Steady Hands — init 33

- **Top:** Attack-ranged 4, range 5. *(non-burn)*
- **Bottom:** Self heal 1. *(non-burn)*

The sniper card. Slowest-but-non-burn ranged attack; minor heal as bottom.

---

## Design intent

- A four-marine squad with all 10 cards each starts with 40 cards in play. After ~3 refreshes per marine the squad is on a clear clock.
- The card pool gives every marine the option to fill any of three roles in a given round: attack, move, or utility (heal/door/condition).
- Bottom-half moves dominate the pool — most cards have a move on the bottom. This is intentional: the bottom half is the routine action, the top is the moment.
- Initiative spreads from 09 to 40. The fastest card (Combat Stim) is a defensive heal; the slowest (Frag) is a high-impact area effect. This pairs naturally — defensive plays go before offensive ones in the same round.
- Burn cards are tilted toward dramatic offense (Overcharge, Frag top, Breach top, Push bottom) so the "shrinking hand" pressure is felt most when the player pushes the throttle.

## Reserved for later

- Level-2+ card swap options.
- Personal goal definition.
- Class-specific gear slots.
