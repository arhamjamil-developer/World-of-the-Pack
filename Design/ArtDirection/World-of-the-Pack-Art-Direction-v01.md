# World of the Pack — integrated art direction v01

First deliverable: [Sunny Forest integrated gameplay mockup](../../Assets/Visuals/ArtDirection/sunny-forest-integrated-gameplay-v03.png).

This is a proposed visual target, not a captured Unity game, playable prototype, sliced UI kit, or finished shader/VFX implementation. Existing approved character designs and animation files remain unchanged. Expand into the other reference screens after establishing this first integrated direction.

## 1. Visual rules to preserve

- Portrait iPhone and iPad; elevated top-down representation. Allies show their backs as they physically travel north; enemies approach south. Character proportions, recognizable accessories, and paired human/wolf identities follow the approved seven-hero lineup.
- Existing animation cells remain 256 × 256, with the shared scale, ground pivot, and 128 PPU contract described in `Assets/Characters/README.md`. Paint effects in separate cells/layers rather than enlarging a character's frame for an ultimate.
- The corridor is exactly 42% of the canonical authored field width: normalized X = 0.29 to 0.71. The generated mockup approximates the composition; implement and verify the geometry numerically rather than tracing its painted edges.
- Keep a stable gameplay corridor across devices. Additional iPad width reveals decorative scenery without changing combat distances or enemy counts. Adapt UI with safe areas; do not crop card controls to preserve a fixed screenshot.
- Warm hand-painted sand, round green/teal trees, cream UI panels, wood frames, brass accents, blue allied markers, and orange enemy markers form the shared visual language. Environmental texture is quieter than characters and attack warnings.
- All seven heroes need different shape and timing signatures. Color reinforces identity; it cannot be the only difference. Reserve saturated red/orange ground shapes for enemy danger.

## 2. First mockup: how to read it

The scene combines a single forest battle, a valid deployment preview, a transformation effect finishing on Momo, an enemy elite warning, and the persistent card interface. It is a staged art-reference moment; the eventual game should not force all seven effects to peak together.

The Bramble Brute is an elite. Its upper bar establishes a reusable visual language for tougher enemies, not a claim that the full boss encounter is already designed or implemented. The leaf slinger and bell healer demonstrate different silhouettes and targeting priorities.

Wolvy's golden crescent is the primary effect; Elva's narrow arrow and Lulu's healing are secondary. Pip's short cyan afterimage, Momo's small cosmic accents, Bobo's ground response, and Tilly's spring gadget stay restrained. The translucent eighth silhouette is a deployment preview, not an active unit.

The shown Wolvy card is ready at 100/100. Elva and Lulu are charging. A successful deploy resets only the selected card; defeating enemies charges each card according to its configured reward rules. The Moonshift control is on cooldown while a prior transformation finishes. Formation is a separate compact control.

## 3. Seven combat identities

All ability behavior below is a design specification to prototype, not implemented gameplay. Timing ranges are initial hypotheses. Preserve current animation drawings and tune clips/animation events before commissioning extra frames.

| Hero | Movement and basic attack | Passive identity | Wolf specials and effect language | Tactical reason to deploy |
| --- | --- | --- | --- | --- |
| Hero Wolvy | Confident planted steps; a deliberate wind-up, one broad northward golden crescent, brief recovery. Target roughly 0.7–0.9 seconds per attack cycle. | Nearby allies resist brief interruption while his rally is active; give the buff a defined radius and duration. | Paw Dash is a compact golden paw thrust with a short dust trail; Rally Howl is an expanding gold paw-ring and small shield marks. Gold moonlight is the main signature; blue cape stays costume color. | Reliable frontline pressure and keeping an advancing group together. |
| Princess Elva | Small precise steps; pause to aim, a thin emerald leaf arrow, visible string recoil. Roughly 0.8–1.1 seconds per aimed shot. | Consecutive shots against one target build a clearly indicated precision mark; changing target resets it. | Leaf Volley releases a readable fan of leaf arrows; Forest Focus uses a leaf-shaped reticle and a restrained spiral around the bow. Never use Momo's star shapes or Pip's speed ribbon. | Remove an exposed ranged/healing threat without flooding the lane with effects. |
| Fairy Lulu | Gentle short steps; supports first, with a small pastel crescent pulse only when no ally needs help. Roughly 1.0–1.3 seconds per support cast. | Prioritize the lowest health percentage ally within range, with a small moon marker that explains her choice. No overhealing resource exploit. | Moon Hug sends heart/crescent healing sparkles; Bubble Blessing is a thin transparent pastel bubble with a visible health rim and soft pop on expiry. | Sustain an injured formation; her lower damage creates a real tradeoff. |
| Captain Bobo | Heaviest steps and strongest anticipation; shield brace, earth-powered paw/ground impact, slow settle. Roughly 1.1–1.5 seconds per heavy cycle. | Front-facing block window shown by the acorn shield turning outward; protection has a limit and readable cooldown. | Acorn Slam uses chunky bouncing acorns, a low earth ripple, and small pebbles; Big Brave Guard raises a warm amber barrier. No giant rock cloud hiding the next warning. | Absorb pressure and interrupt clustered threats; trades speed for protection. |
| Scout Pip | Fast short bursts and brief stops; a quick cyan wind jab with very short recovery. Roughly 0.4–0.6 seconds per strike, lower damage per hit. | A brief movement window after a successful evade; show scarf stretch and one short afterimage. Do not make him invulnerable or let him run out of the readable corridor. | Zoom Dash uses a narrow cyan tapered trail; Whirl Paw is a small circular wind curl with a clear center. Use one or two pooled afterimages, not a chain of full actors. | React to exposed fast enemies and reposition quickly, at the cost of durability. |
| Wizard Momo | Measured steps; wind-up with hat bob, one distinct five-point purple star, star-shaped impact. Roughly 1.2–1.6 seconds per cast. | Repeated casts build a small constellation indicator for the next enhanced cast, capped and consumed visibly. | Star Sprinkle is a compact staggered shower with ground markers; Moon Pop is a purple crescent core that bursts into large readable stars. Avoid generic fireballs and continuous glitter. | Area pressure against grouped enemies; needs protection during his longer cast. |
| Tinker Tilly | Bouncy movement with backpack follow-through; crank, spring release, mechanical recoil. Roughly 0.9–1.2 seconds per gadget cycle. | A deployed device receives one limited repair/rearm when its condition is met; show a brass wrench spark and enforce a per-device limit. | Spring Surprise creates a bouncing spring trap with clear landing shadow; Bubble Trap uses turquoise bubbles and brass gear fragments. Shapes are mechanical, not Lulu's hearts or Momo's stars. | Control a route and slow a rush while other heroes finish the threat. |

Human attacks retain the same readable role and matching props. Wolf form enhances the role through its specials; do not invent fourteen unrelated character classes. Transformation requirements, duration, and resource costs remain gameplay tuning decisions. Preserve repeated summons of a unit type.

### Compatible combinations

- Bobo holds a group inside Momo's marked star-shower area. A small acorn/star link cue explains the combination; no extra full-screen explosion.
- Elva's precision mark gives Pip a clear priority target. A leaf reticle briefly gains a cyan directional tick when Pip commits.
- Lulu protects Wolvy before his dash. The pastel bubble compresses into a thin gold-edged rim while protection lasts.
- Tilly slows enemies long enough for Elva to line up a volley. Show the trap state and leaf impacts separately.

Each combination needs a visible setup and result. Do not add invisible damage multipliers that the player cannot infer.

## 4. Enemy families and biomes

Create separate readable silhouettes for fast attackers, armored tanks, ranged units, healers, summoners, elites, and bosses. New enemies must remain friendly cartoon adversaries: patrols, playful fantasy creatures, and enchanted constructs. Hostility should be clear through team markers and behavior rather than making every creature look frightening.

The names below are proposed production briefs, not generated enemy assets.

| Biome | Fast / armored / ranged | Healer / summoner / elite | Boss identity and attack grammar |
| --- | --- | --- | --- |
| Sunny Forest | Acorn Runner: low plume and quick charge. Barkguard: broad bark shield. Leaf Slinger: forked slingshot silhouette. | Bell Keeper: raised bell and short healing tether. Sprout Caller: seed satchel and limited sprout summons. Bramble Brute: rounded wood fists and acorn shoulders. | Ancient Oakheart: broad oak crown and root feet; alternate a marked root lane, acorn landing circles, and an interruptible rally. |
| Mushroom Grove | Cap Hopper: springy tall cap. Puffguard: wide layered mushroom armor. Spore Lobber: cupped basket and arcing soft puff. | Glowcap Nurse: lantern cap and healing ring. Patch Gardener: planter backpack with capped seedling summons. Mossback Bully: low broad mossy dome. | Grand Puffcap: giant rounded cap, compressing anticipation before concentric puff rings and safe wedge gaps. |
| River Meadow | Reed Skipper: narrow reed crest and skipping steps. Shellwarden: rounded shell shield. Pebble Skimmer: low sling and bouncing stones. | Lily Mender: lily parasol. Ripple Caller: round water jar spawning limited bubble constructs. Brook Brute: smooth stone shoulders and waterwheel motif. | Dam Warden: broad wood-and-stone construct; marked horizontal water pushes, visible stone lanes, and exposed recovery windows. |
| Emberlane Refuge approaches | Cinder Scout: small orange scarf and fast sidestep. Kiln Guard: squat rounded kiln armor. Spark Slinger: brass pan and soft ember arc. | Hearth Tender: warm lantern. Coal Toymaker: wind-up ember toys with a strict summon cap. Brickback: stacked rounded bricks and heavy mitts. | Hearth Colossus: friendly-looking rounded furnace construct outside the safe camp; clearly marked steam cones, delayed warm ground circles, then cooling breaks. |
| Cloudstep Highlands | Gust Runner: long windsock pennant. Cloudshield: broad padded kite shield. Kite Archer: unmistakable kite/bow outline. | Chime Mender: hanging bells. Nimbus Caller: cloud drum and small bounded cloud summons. Crag Pouncer: compact stone body with spring legs. | Aerie Keeper: round cloud mantle and wing-shaped arms; forecast gust lanes, staggered feather markers, and a calm center during phase changes. |

A physical refuge interior is safe; enemies on its surrounding approach do not attack through merchant or upgrade screens. Summoned enemies must not create an unlimited energy farm. Define whether they grant reduced or zero card energy and make that rule visible.

### Warning and defeat rules

- Give warnings a visible rim, simple pattern, footprint matching actual damage, and a timer/anticipation animation. Use filled/hatched shapes as well as color.
- Fast charge: a narrow directional lane. Lob: landing circle and descending shadow. Slam: expanding ground ring. Cone: bounded sector. Boss phases reuse this vocabulary, with combinations introduced gradually.
- Keep warnings below actors but above quiet ground. Friendly effects must not erase them.
- Defeated enemies lose collision and combat immediately, then play a short wobble/puff or harmless star burst. Count the defeat and card energy once, independently of pooled VFX lifetime.
- Boss phase changes use a short silhouette pose, readable bar segment, and restrained particle transition. Telegraph any gameplay pause or invulnerability explicitly.

## 5. Mobile VFX and shader production system

Author and save graphs, shaders, materials, clips, and prefabs in the Editor. Do not generate or compile shader source/graphs at runtime. Runtime code may drive saved materials, animation events, particle instances, and pooled effect objects.

Suggested families:

| Family | Saved asset / treatment | Readability and cost rules |
| --- | --- | --- |
| Character hit response | URP 2D sprite-compatible hit-flash material with a bounded color mix; optional tiny local squash on a visual child | Preserve alpha and ground pivot; never scale the collider or move the root to fake impact. One brief flash, not persistent bloom. |
| Transformation | One masked dissolve material with a noise texture, narrow rim, and hero-specific emblem particles; two forms share the same ground anchor | Use a tight local dissolve; swap the correct sprite set at a defined point. The illustrated Momo glow is a visual reference, not an existing dissolve implementation. |
| Personal glow / buffs | Small emissive-like painted halo sprites or a restrained sprite rim treatment, parameterized per hero | Use MaterialPropertyBlock-compatible parameters where practical. No unique material allocation for every hit or summon. |
| Projectiles and trails | Atlas-backed sprite flipbooks, small pooled trail meshes/particles, separate impact prefab | Leaf arrows, stars, acorns, spring devices, and moon crescents need different geometry and timing. Do not solve differentiation with tint alone. |
| Transparent protection | Thin rim bubble sprite/shader with sparse highlights and a slight local pulse | Avoid full-screen refraction, repeated scene-color grabs, or opaque fill hiding characters. |
| Ground attacks | Separate low-cost ring/cone/stripe sprites with smooth fill progression; matching warning and impact assets | Keep exact radii and timing tied to gameplay data; decouple decorative particles from damage application. |
| Damage / healing numbers | Pooled text with small, short motion; distinct symbols and restrained colors | Aggregate rapid repeated damage; prioritize critical hits and meaningful healing. Do not produce a number for every tiny particle. |
| Status effects | Small root/leaf snare, cyan speed ticks, warm block glint, purple constellation mark | Each status has an icon, start cue, and expiry cue; cap simultaneous decorative layers. |
| Ground dust / footsteps | Small pooled dust puffs triggered at selected foot contacts | Root movement remains real northward movement. Puffs should not mask lower legs or path warnings. |
| Environment motion | Saved foliage-wind graph with vertex/UV weighting and anchored base; simple pond UV animation and sparse ambient motes | Sway edge scenery lightly. No screen-wide moving canopy over combat; no physical simulation for every leaf. |
| Spawn / rewards | Small paw footprint and controlled arrival ring; short reward rays at a safe stop | Preview stays translucent; invalid positions use a shape/icon change. Spawn count updates at actual unit activation. |

Use regular ParticleSystem and sprite flipbooks as the mobile baseline. VFX Graph is optional only after confirming the target device/API support and measuring its benefit; do not require it for the first playable loop. Choose the project's URP 2D sprite target correctly when authoring Shader Graphs.

Set budgets from measured device performance, not attractive screenshots. Start with one large effect per combat focal event, a small bounded pool per effect family, and a capped number of visible combat texts. Profile overdraw, batching, allocations, and frame time with the busiest planned formation on target hardware.

## 6. Feedback timing and motion

Every attack has anticipation, a precise gameplay hit moment, follow-through, and recovery. Character attack art, projectile release, hit flash, sound, and damage must reference that moment; a pretty effect is not a substitute for correct combat timing.

Preserve current six-frame animations as the first pass. Retiming and light child-only squash can improve feel. Request new frames if a signature action cannot be made legible; do not distort the approved anatomy or invent rotations from a single north-facing sprite set.

Use very short, modest hit reactions. Large hero actions can receive a tiny optional camera impulse; avoid persistent shake and provide a motion-reduction option. Boss warning shapes and unit aim directions must remain stable during feedback.

## 7. Environment continuity

Maintain one biome palette within a stage, near-constant prop scale, stable lighting, and the 42% open corridor. Trees, flowers, stones, ponds, fences, and camp props belong at the shoulders. Alternate familiar motifs instead of swapping the whole backdrop every wave.

Use authored modular sections with explicit entrance/exit connectors, deterministic seeds, and edge-prop sockets. Validate joins and collision footprints. Generated loop artwork is a visual source; its filename does not establish pixel-perfect seamless tiling.

Transition between major biomes through authored lead-in segments: forest flowers gain mushrooms, meadow reeds introduce a river, lanterns lead toward a refuge, and pale stones precede highland grasses. Stage these over several view lengths. Do not abruptly change path width, camera zoom, or enemy scale.

## 8. UI and physical-world screens

Keep the shared wood/cream/brass language and large text/tap targets. Three active card slots remain the on-screen hand; roster navigation exposes the seven types without squeezing seven full cards into the bottom bar. Cards show a portrait, a name, their own energy meter, a ready/charging state, and repeatability. Never display a ready action below its energy threshold.

Formation previews show the resulting positions before committing. Transformation controls expose eligibility and cooldown. Health bars appear when relevant; boss/elite bars have a distinct label and phase markers. Keep decorative accents away from warning footprints and card costs.

Physical camps and merchants sit beside the route with a safe interaction boundary. Entering a camp stops combat before presenting conversation, upgrades, merchant inventory, or rewards. A saved run restores formation, wave, pack condition, energies, selected upgrades, and the world seed/position consistently.

For reference production, merchant and upgrade screens should explain one meaningful choice at a time; inventory, prices, currencies, and balance remain gameplay decisions rather than implied features of this mockup.

## 9. Reference production order after this first image

1. Sunny Forest normal march and deployment close-up: root movement, valid/invalid previews, energy spending, formation preview.
2. Transformation and seven signature action references: preserve the approved forms; separate body poses, projectile, trail, impact, ground mark, and status icon.
3. Sunny Forest enemy role sheet and Oakheart boss encounter: silhouettes, warning footprints, attack/recovery, defeat, and phase transitions.
4. Physical camp, merchant, upgrade, reward, and progression references using the same interface materials and character scale.
5. Mushroom Grove, River Meadow, Emberlane Refuge, and Cloudstep Highlands environment/enemy/boss references with gradual transition sections.

Each production sheet should state exact dimensions, frame count, pivot, PPU, row order, direction, and alpha requirements. Deliver body sprites, projectiles, VFX, UI components, and environment props separately. A flattened integrated mockup remains a reference, not a production sprite atlas.

## 10. Review gates

- Confirm all seven identities from rear silhouettes and small card portraits, without names or color alone.
- Verify actor artwork, enemies, and warnings fit the actual 42% corridor at both phone and tablet portrait layouts.
- Play effects at real speed. Can a player identify who attacked, what landed, what is dangerous next, and why a card is unavailable?
- Exercise physical northward travel, no-energy recovery, repeated summoning, population limits, transformation, death/pooling, camp safety, and save/restore.
- Verify Unity compilation, Console, Play Mode, captures, and measured mobile performance before describing implementation as complete.

Exact image-generation prompts, including the selected layout/anatomy correction, are saved in `sunny-forest-integrated-v03-prompts.md`. Built-in image generation was used; no existing gameplay, shader, sprite sheet, or scene was replaced by this art-direction pass.
