# World Setting

A first-draft setting brief. **All faction and place names are `[PLACEHOLDER]`** — they exist so docs can refer to "the marines" and "the xenos" by name, and so worldbuilding has anchor points to argue with. Replace them in a single sweep before any of these names appear in player-facing content.

## Tone

Grimdark sci-fi in the Space Hulk tradition. The setting is dangerous, the void is hostile, the institutions are flawed. Missions are not glory runs — they are the dirty work of containing an outbreak no one wants to admit is happening. The squad's job is meaningful, not heroic.

Influences worth naming: *Aliens* (the squad dynamic and equipment tone), *Space Hulk* (the boarding-action format), *Warhammer 40k* (the bleak vastness), *Event Horizon* (the wrongness of derelict space), *The Expanse* (grounded engineering and corporate politics).

## Setting in one paragraph

Centuries after humanity expanded into the deep frontier, things have started coming back wrong. Derelicts drift home from the dark with their crews missing and their interiors infested by xeno organisms no catalog records. Mining colonies fall silent. Stations evacuate. The institutions that should respond — naval admiralties, corporate security, system authorities — instead deploy small boarding teams to investigate, contain, and deny ground to the threat. The teams are called **Boarding Strike** units.

## Factions

### `[PLACEHOLDER: the player faction]` — provisional name **The Vanguard**

The institution that fields the player squad. Provisional details:

- A military-corporate hybrid: chartered to operate outside normal jurisdiction, funded by the systems that pay it to keep their problems off-record.
- Boarding marines are well-equipped but under-supported. There are no reinforcements.
- The Vanguard's existence is half-secret. Missions are not in the news. Marines are professionals, not heroes.
- Provisional aesthetic: hard suits in matte greys with high-vis trim, rebreather helms, mag-grip boots, magazine-fed kinetics and short-range plasma.

### `[PLACEHOLDER: the xeno threat]` — provisional name **The Brood**

The primary antagonist. Provisional details:

- Origin unknown. Possibly a forgotten bioweapon, possibly older than humanity, possibly both.
- Multi-form: spawns several specialized phenotypes from a shared substrate.
- Spreads by infesting installations, not by space combat. Encounters always happen indoors, in tight spaces.
- The Brood is not a civilization — there are no negotiations, no factions within the Brood, no diplomacy. Only types. Two MVP types:
  - **Swarmers** (codename `husk`): clawed melee phenotypes, expendable, fast.
  - **Spitters** (codename `cyst`): ranged phenotypes, slower, secrete a toxin that wounds and slows.

### Reserved factions

Not in MVP, available for later arcs:

- **`[PLACEHOLDER: corporate scavengers]`** — humans willing to fight Vanguard teams for derelict salvage rights. Mercenary armor, hacked drones, ambush tactics.
- **`[PLACEHOLDER: rogue synthetics]`** — abandoned automation gone hostile. Tank chassis, sentry guns, hacked station systems.
- **`[PLACEHOLDER: secondary xeno strains]`** — divergent Brood phenotypes that suggest an underlying intelligence.

## Mission settings

Where scenarios take place:

- **Derelict ships.** Tight corridors, sealed bulkheads, drifting in null-g (gameplay still grid-based, no zero-g movement in MVP).
- **Abandoned stations.** Larger interior spaces, civilian areas, atmospheric systems.
- **Frontier colonies.** Underground habitats, mining shafts, atmosphere domes.
- **Wrecks half-merged with rock or other ships** — the "space hulks" of the title. Mixed corridor and cavern environments.

The MVP mission is a derelict ship interior.

## Visual identity (for reference)

The art direction is locked in [../technical/art-pipeline.md](../technical/art-pipeline.md). High-level intent:

- Low-saturation greys and metals as base, with two accent colors: a sickly bio-green for Brood organic terrain and a warning-orange for Vanguard markings.
- Hard angular shapes for ship interiors. Organic, asymmetric biomass for Brood-infested terrain.
- Light is grim — dim corridor lighting with rare emergency-red zones.

## Tone of voice (for narration / UI strings)

When in-game text appears (objectives, mission briefings, marine callouts):

- Spare. Military shorthand. No exposition dumps.
- The Vanguard refers to the threat as "contacts," "biomass," "hostiles." Never "the enemy" — too dramatic.
- Marines have callsigns, not full names, in MVP UI strings. Roster names appear in Iteration 3 once persistence exists.

## Open questions

- Final faction names. The placeholders above are usable but not final.
- The campaign's overall arc — episodic incidents or one long thread? Deferred to Iteration 3.
- Whether the player ever sees the Vanguard's command structure (briefings from a named handler) or stays purely operational. Recommend a single recurring handler voice in Iteration 4.
