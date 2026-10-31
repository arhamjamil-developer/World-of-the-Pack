# World of the Pack — Official Game Design Document (GDD)
**Version:** 1.0.0 (Production Release)  
**Target Platform:** Mobile (iOS / Android)  
**Aspect Ratio:** 9:16 Portrait (1080×1920 reference, 576×1024 test target)  
**Engine & Pipeline:** Unity 6 (`6000.3.13f1`), URP 2D (`17.3.0`), Unity Input System (`1.19.0`), Cinemachine (`com.unity.cinemachine`)  
**Project Path:** `B:\World of the Pack`

---

## 1. Executive Summary & Vision Statement

### 1.1 High Concept
*World of the Pack* is an atmospheric, top-down 2D card-tactics action game built specifically for one-thumb portrait mobile play. The player leads a sacred fellowship of seven lupine shapeshifters—champions disguised in human forms who awaken into fearsome primal wolves when the astral moon crests. As corrupt human hunters and mechanised forest wardens encroach upon the Ancient Canopy, the player must orchestrate unit deployment, formation marching, and tactical lane defense to survive endless escalating waves.

### 1.2 Core Pillars
1. **Fluid One-Handed Tactics:** Zero clunky card-cost mana systems. Every hero card charges independently through continuous passive flow and combat triage (defeating enemies directly siphons energy back into the deck).
2. **Dynamic Shapeshifter Combat:** Deploying a hero summons their tactical human form into the 5-lane march corridor; surging their card a second time unleashes their ferocious Wolf Form, transforming mundane skirmishes into ferocious lunar blitzes.
3. **Strict Above-Deck Battlefield Framing:** The tactical UI is cleanly compartmentalised. Cards, formation toggles, and moonshift meters anchor the lower viewport, while Cinemachine dynamically calculates the clash midpoint between the advancing pack and incoming enemy squads—guaranteeing 100% action readability above the card deck at all times.
4. **Endless Expedition Rhythm:** Seamlessly shifts between tactical corridor skirmishes, pincer wave ambushes, and restorative forest campsites where heroes recuperate, forge runes, and adjust formation ranks.

```
       +---------------------------------------------+
       | [WAVE 01]       WORLD OF THE PACK       [||] | <- Top HUD
       | Allies: 4         [<3 <3 <3 <3 <3]  Camp 260m |
       +---------------------------------------------+
       |                                             |
       |             [Enemy Squad North]             |
       |             (Soldiers & Hounds)             |
       |                      v                      |
       |                 \  CLASH  /                 | <- Cinemachine Dynamic
       |                  \ ZONE  /                  |    Combat Framing
       |                      ^                      |
       |             [Allied Pack Vanguard]          |
       |           (Wolvy, Bobo, Elva, Lulu)         |
       |                                             |
       +---------------------------------------------+
       | [FORMATION]     * . . . . . .    [MOONSHIFT] | <- Control Ribbon
       +---------------------------------------------+
       | [ WOLVY ]   [ ELVA ]    [ LULU ]   [ BOBO ]  | <- 7-Card Persistent
       | 0/100 Bar   0/100 Bar   0/100 Bar  0/100 Bar |    Hand Tray
       | [CHARGING]  [CHARGING]  [CHARGING] [CHARGING]|
       +---------------------------------------------+
```

---

## 2. World Lore & Narrative Setting

### 2.1 The Briarwood Blight
Deep within the untamed continent of Sylvanna lies the **Ancient Canopy**, anchored by the **Great Moon Tree**—a kilometer-tall colossal cedar that absorbs astral energy from the Twin Moons. For generations, the Lupine Guardians lived in equilibrium with the forest. However, the Grand Industrial Baronies from the northern valleys discovered that crystallised sap (known as *Moon Amber*) could power relentless alchemical siege engines.

Driven by greed, the Baronies dispatched the **Ironthorne Hunters**—a mechanized mercenary faction armed with barbed spears, iron traps, and trained hounds—to strip the Moon Tree of its heart. The resulting spiritual tear caused the *Briarwood Blight*, turning passive woodland beasts into frenzied abominations and poisoning the groundwater.

### 2.2 Alpha Wolvy's Sacred Oath
Wolvy, youngest prince of the pack line, witnessed his elders fall while protecting the Moon Tree's inner sanctuary. With their dying breaths, the elders sealed the pack's astral wolf spirits inside sacred runic talismans. To survive among the invading legions, Wolvy and six surviving companions donned human disguise. 

Their quest: march northward against the hunter incursions, reclaim stolen moonshards, and reach the summit of the Great Moon Tree to break the blight once and for all.

---

## 3. The Seven Lead Heroes (The Pack Fellowship)

Every character possesses dual forms:
* **Human Form:** Balanced skirmisher with tactical abilities, defensive tools, or precision ranged weaponry.
* **Wolf Surge Form:** Triggered when the hero's card reaches 100% energy while already deployed on the field. The hero sheds their disguise, doubling speed and critical damage while draining enemy life essence for 20 seconds.

```
+---------------+--------------+----------------+---------------+-----------------------+
| Hero          | Human Role   | Wolf Form Name | Attack Range  | Signature Ability     |
+---------------+--------------+----------------+---------------+-----------------------+
| Wolvy         | Vanguard     | Alpha Direwolf | Melee (1.1m)  | Pack Howl (ATK Buff)  |
| Princess Elva | Marksman     | Verdant Prowler| Ranged (3.6m) | Piercing Thistle Arrow|
| Fairy Lulu    | Shaman/Healer| Celestial Glim | Ranged (3.2m) | Lunar Mending Aura    |
| Captain Bobo  | Iron Bastion | Ironhide Behem | Melee (1.0m)  | Aegis Shield Wall     |
| Scout Pip     | Infiltrator  | Shadow Stalker | Flank (1.4m)  | Dagger Bleed Ambush   |
| Wizard Momo   | Area Mage    | Astral Howler  | Magic (3.8m)  | Arcane Nova Vortex    |
| Tinker Tilly  | Alchemist    | Gearfang Gnaw  | Mid-R (2.8m)  | Bouncing Cog Bombs    |
+---------------+--------------+----------------+---------------+-----------------------+
```

### 3.1 Wolvy — The Pack Leader
* **Role:** Frontline Vanguard / Offensive Buffer
* **Human Kit:** Wields the Twin Dagger Blades. High attack frequency with light horizontal cleave.
  * *Base Stats:* HP: 120 | Move Speed: 1.35 m/s | ATK: 18 | ATK Speed: 0.9s
  * *Passive — Alpha Presence:* Allies within 1.5m deal +15% increased physical damage.
* **Wolf Surge — Alpha Direwolf:**
  * *Surge Stats:* HP: +50 Temp Shield | Move Speed: 2.2 m/s | ATK: 36 | Life Steal: 25%
  * *Active Roar:* Emits a radial shockwave that stuns frontline enemies for 1.2s.

### 3.2 Princess Elva — The Verdant Ranger
* **Role:** Backline Precision DPS / Anti-Air & Anti-Sniper
* **Human Kit:** Longbow of the Weeping Willow. Pierces armor and prioritizes high-threat enemies.
  * *Base Stats:* HP: 75 | Move Speed: 1.20 m/s | ATK: 24 | ATK Speed: 1.4s | Range: 3.6m
  * *Passive — Hawkeye:* Critical hit chance increases by +5% for every meter of distance to target (up to +25%).
* **Wolf Surge — Verdant Prowler:**
  * *Surge Stats:* HP: +30 Temp Shield | Move Speed: 2.4 m/s | Rapid Volley: shoots 3 arrows in a spread.

### 3.3 Fairy Lulu — The Astral Shaman
* **Role:** Backline Sustained Support / Crowd Control
* **Human Kit:** Moondust Staff. Fires glowing orbs that damage enemies or heal damaged pack members.
  * *Base Stats:* HP: 70 | Move Speed: 1.25 m/s | Heal: 14 HP/burst | Interval: 1.6s
  * *Passive — Moon Blossom:* When any ally drops below 30% HP, instantly shields them for 35 HP (20s cooldown).
* **Wolf Surge — Celestial Glimmer:**
  * *Surge Stats:* Becomes an ethereal radiant wolf. Continuous healing aura (8 HP/sec to all pack members within 2.5m).

### 3.4 Captain Bobo — The Iron Bastion
* **Role:** Frontline Tank / Formation Anchor
* **Human Kit:** Oak & Iron Tower Shield and Mace. Intercepts incoming projectiles and body-blocks enemy rushes.
  * *Base Stats:* HP: 210 | Move Speed: 1.05 m/s | ATK: 14 | Armor: 30% flat damage reduction
  * *Passive — Steadfast:* Immune to knockback; stops charging hounds dead in their tracks.
* **Wolf Surge — Ironhide Behemoth:**
  * *Surge Stats:* HP: +100 Temp Shield | Armor: 50% flat reduction | Taunts all enemies within 3.0m.

### 3.5 Scout Pip — The Shadow Skirmisher
* **Role:** Flank Infiltrator / High Mobility Assassin
* **Human Kit:** Dual Obsidian Needles. Teleports behind enemy backlines to assassinate priests and archers.
  * *Base Stats:* HP: 85 | Move Speed: 1.65 m/s | ATK: 22 | ATK Speed: 0.75s
  * *Passive — Shadow Step:* Gains 40% evasion while moving; dodged attacks trigger an immediate counter-slash.
* **Wolf Surge — Shadow Stalker:**
  * *Surge Stats:* Permanent stealth for first 3 seconds of surge; first strike deals 300% critical damage.

### 3.6 Wizard Momo — The Arcane Arcanist
* **Role:** Mid-range Area Denial / Spellcaster
* **Human Kit:** Starlight Tome. Channels orbital celestial vortexes that pull enemies into tight clusters.
  * *Base Stats:* HP: 80 | Move Speed: 1.15 m/s | ATK: 28 (AoE) | ATK Speed: 2.1s
  * *Passive — Overload:* Defeating 3 enemies with spells refunds 20 energy immediately to Momo's card.
* **Wolf Surge — Astral Howler:**
  * *Surge Stats:* Summons falling star meteors every 2.5 seconds that stagger enemy squadrons.

### 3.7 Tinker Tilly — The Gearwright Alchemist
* **Role:** Mid-range Trapster / Turret Engineer
* **Human Kit:** Spring-loaded acorn catapult. Drops explosive caltrops and deployable wooden barricades.
  * *Base Stats:* HP: 90 | Move Speed: 1.20 m/s | ATK: 16 (Splatter) | ATK Speed: 1.2s
  * *Passive — Scrap Harvest:* Defeated mechanized enemies drop scrap, increasing deploy energy charge rate by +10%.
* **Wolf Surge — Gearfang Gnawer:**
  * *Surge Stats:* Claws shred enemy mechanical armor by 50% for 6 seconds.

---

## 4. Card Hand, Deck Flow & Deployment Economy

```
   [SWIPE CARDS HORIZONTALLY] <--------------------> [7 HERO CARDS TRAY]
   
             +--------------------+
             |  LANE 1 | 2 | 3 | 4|
             +--------------------+
                      ^
                      |   [DRAG UP TO SPAWN]
                      |
             +--------------------+
             |      PORTRAIT      |
             |      [ WOLVY ]     |
             |      =========     |
             |       0 / 100      | <- Energy Bar (0-100)
             |   +------------+   |
             |   |  CHARGING  |   | <- Navy Pill -> Turns Green DEPLOY at 100%
             |   +------------+   |
             +--------------------+
```

### 4.1 Zero-Cost Independent Charging Economy
Traditional collectible card games force players to wait for a shared, monolithic mana pool. In *World of the Pack*, **every hero card is an independent battery**:
* **Max Energy:** 100 per card.
* **Passive Trickle:** Every card charges at a base rate of **+2.5 energy per second**.
* **Combat Siphon:** Defeating enemy soldiers grants **+20 to +25 energy** distributed directly across all active and reserve cards. Elite captains grant **+60 energy**, and bosses award a full instant +100% refill to all cards.

### 4.2 Multi-Deployment & Meter Emptying
* When a card reaches 100%, its button transitions into the radiant green embossed **DEPLOY** state.
* **Tap or Drag:** The player can either tap the button to deploy the hero into their default formation slot, or drag the card vertically into any of the 5 lanes.
* **Instant Empty:** Deploying the hero immediately resets the card's energy to **0%**, disabling the button into the navy **CHARGING** state.
* **Multi-Unit Deployment:** The player can deploy multiple copies of any hero over time. When a hero is on the field and their card recharges back to 100%, the button displays **x1 [DEPLOY]**, allowing the player to summon a duplicate vanguard or trigger **WOLF SURGE**.

### 4.3 Physical Touch Controls & Responsive Hand
* **Horizontal Swipe:** Dragging horizontally scrolls smoothly through the 7-card carousel (`ResponsiveHeroHand`), snapping cards to centered anchors with haptic ticks.
* **Vertical Drag-to-Spawn:** Dragging a card upward above the deck threshold (delta Y > 30px) activates the **Card Drag Ghost Preview** (`CardDragGhostPreview`), casting an ethereal silhouette onto the battlefield with snapping lane indicators (`LaneOverlayRenderer`).
* **Release Placement:** Releasing inside the valid battlefield corridor instantiates the character with a snappy bounce-punch animation; releasing below snaps the card back into the tray with smooth elastic easing.

---

## 5. 5-Lane Battlefield Corridor & March Navigation

### 5.1 Lane Geometry
The battlefield is divided into five standardized vertical lanes:
* **Lane 0 (Far Left):** $X = -1.44\,\text{m}$
* **Lane 1 (Mid Left):** $X = -0.72\,\text{m}$
* **Lane 2 (Center):** $X = 0.00\,\text{m}$ (The Leader's Path)
* **Lane 3 (Mid Right):** $X = +0.72\,\text{m}$
* **Lane 4 (Far Right):** $X = +1.44\,\text{m}$

Horizontal world bounds are clamped between $-1.80\,\text{m}$ and $+1.80\,\text{m}$ to ensure characters never clip into side foliage.

### 5.2 Advance-Only Touch March
To maintain cinematic forward momentum and prevent disorienting retreats:
* Touching and holding the battlefield screen commands Alpha Wolvy and the vanguard to march forward along the corridor at full expedition speed ($1.35\,\text{m/s}$).
* Releasing touch causes the pack to assume a grounded defensive stance, bracing for incoming melee charges.
* Units never step backward behind the formation anchor line ($Y_{\text{anchor}} - 0.85\,\text{m}$), preventing cards at the bottom of the screen from obscuring any combatant.

---

## 6. Enemy Squad & Wave Archetypes

Enemy squadrons spawn from the northern thickets and advance southward.

```
       [AMBUSH FLANK LEFT]        [OVERHEAD NORTH]        [AMBUSH FLANK RIGHT]
       (-1.44, Y + 3.4)           (0.00, Y + 4.2)         (+1.44, Y + 3.4)
              \                          |                         /
               \                         v                        /
                +------------------------------------------------+
                |              BATTLEFIELD CORRIDOR              |
```

### 6.1 Hunter Soldier (Infantry)
* **Visual:** Brown leather tunic, iron kettle helm, spear and light wooden buckler.
* **Role:** Frontline wall. Advances at steady pace ($1.25\,\text{m/s}$) in phalanx rows of 3 to 5 units.
* **Stats:** HP: 80 | ATK: 15 | Attack Interval: 1.3s | Energy Reward: 25

### 6.2 Hunter Hound (Flanker)
* **Visual:** Barbed collar, feral spiked muzzle, rapid four-legged gait.
* **Role:** High-speed flank ambusher. Spawns via `FlankLeft` or `FlankRight` at 35-degree angles, targeting vulnerable backline spellcasters (Lulu, Elva).
* **Stats:** HP: 45 | ATK: 10 | Move Speed: 2.10 m/s | Energy Reward: 20

### 6.3 Patrol Captain (Heavy Commander)
* **Visual:** Full iron plate armor, great-axe, plumed helmet.
* **Role:** Miniboss / Heavy disrupter. Features a telegraphed 1.2-second ground-slam attack (`EnemySlamCircle`) that damages all allies in an area.
* **Stats:** HP: 220 | ATK: 35 | Move Speed: 0.85 m/s | Energy Reward: 60

### 6.4 Ancient Thorn Guardian / Bramble Brute (Colossus Boss)
* **Visual:** Towering twisted treant infused with dark corrupted iron machinery.
* **Role:** Appears at Wave 5 and every 5th wave thereafter. Features multiple attack phases:
  1. *Thorn Barrage:* Shoots arcs of corrupted quills down 3 lanes.
  2. *Root Snare:* Roots the allied vanguard in place for 2 seconds.
  3. *Cataclysmic Stomp:* Massive screen shake dealing 60 damage across all lanes unless interrupted by a Wolf Surge attack.

---

## 7. Cinemachine Dynamic Combat Framing & Camera Directing

### 7.1 Orthographic Projection & Safe Area Constraints
* **Virtual Camera:** `Cinemachine_ExpeditionCam` driven by `CinemachineBrain`.
* **Lens Size:** Base orthographic size is dynamically calculated to guarantee a minimum horizontal corridor width of $5.5\,\text{m}$ regardless of whether the player is on a 9:16 iPhone, a 19.5:9 flagship, or a 3:4 iPad tablet.
$$\text{OrthographicSize} = \max\left(6.20, \frac{5.5 \times 0.5}{\text{AspectRatio}}\right)$$

### 7.2 Real-time Combat Clash Tracking
The `CinemachineCombatDirector` tracks three critical real-time variables every frame:
1. `CombatClashDistance`: The exact distance between the leading ally vanguard and the nearest incoming enemy:
   $$D_{\text{clash}} = \max\left(0, Y_{\text{nearest\_enemy}} - Y_{\text{lead\_ally}}\right)$$
2. `ClashCenterY`: The vertical midpoint of the battle:
   $$Y_{\text{clash\_center}} = \frac{Y_{\text{lead\_ally}} + Y_{\text{nearest\_enemy}}}{2}$$
3. `HasActiveCombatClash`: Boolean state flag set to `true` whenever active enemies and allies coexist on the field.

### 7.3 Midpoint Dynamic Lock & Above-Deck Clamping
* When $D_{\text{clash}} < 4.5\,\text{m}$, the director activates **Encounter Lock**, smoothly centering the camera on $Y_{\text{clash\_center}} - 0.20\,\text{m}$.
* **Corridor Floor Invariant:** To satisfy the strict mandate that no hero or enemy can ever be hidden behind the bottom cards, the camera tracking target position is clamped:
  $$Y_{\text{desired}} \ge Y_{\text{anchor}} + 0.85\,\text{m}$$
  Since cards occupy viewport space up to $Y \approx -2.26\,\text{m}$, this guarantee keeps all characters positioned safely in the central corridor with $\ge 1.41\,\text{m}$ of visual clearance above the top edge of the card hand.

---

## 8. Campsites, Runes & Endless Expedition Progression

```
+-------------------------------------------------------------------------+
| [REST & RECOVER]          CAMPSITE MILESTONE (250m)       [FORGE RUNES] |
|                                                                         |
|                (o)               Campfire Props                         |
|               /|\                 Healing Broth                         |
|               / \                                                       |
|                                                                         |
| [Upgrade: +15% Wolf Duration]  [Upgrade: +10 Max Pack HP] [Leave Camp] |
+-------------------------------------------------------------------------+
```

### 8.1 Campsite Structure
Every 250 to 500 meters of northward progress, the wave spawner pauses (`WaveManager.PauseForCamp()`), and the pack arrives at an atmospheric woodland clearing with a campfire and a sparkling restorative spring:
* **Full Resuscitation:** Any fallen heroes are revived to full health.
* **Energy Restocking:** All 7 hero cards receive an instantaneous +50 energy boost.
* **Rune Blessing Selection:** The player chooses one of three randomly drawn ancestral runes:
  * *Lunar Tide:* Increases passive card charge rate by +20%.
  * *Iron Claws:* Increases Wolf Surge critical damage multiplier by +50%.
  * *Bramble Ward:* The pack gains a persistent barrier that absorbs 40 damage per wave.

### 8.2 Biome Progression
As the expedition distance increases, the environment procedurally transitions across 4 distinct visual biomes:
1. **Sunny Forest (0m – 1000m):** Lush emerald grass, blooming lavender flowers, red fly-agaric mushrooms, winding earthen trail.
2. **Whispering Canopy (1000m – 2500m):** Ancient bioluminescent moss, deep jade shadows, falling autumn leaves, decaying hollow logs.
3. **Cloudstep Highlands (2500m – 4500m):** Misty granite cliff paths, windblown pines, altitude fog volumes, sudden hound ambushes.
4. **Emberlane Refuge (4500m+):** Charred timber, glowing ash particles, burnt iron barricades, high-intensity hunter artillery.

---

## 9. Visual Style Guide, Asset Pipelines & UI Hierarchy

### 9.1 Art Direction
* **Style:** High-detail 2D hand-painted anime/chibi proportions (256×256 hero sprites, clean line work, vivid color saturation).
* **Lighting:** URP 2D forward renderer with a subtle warm directional light, soft ambient rim, and glowing character selection rings (Cyan for Allies, Orange for Enemies).
* **Global Volume Post-Processing:**
  * *Bloom:* Soft lunar glow on staves, daggers, and wolf eyes (Threshold: 0.9, Intensity: 0.85).
  * *Vignette:* Subtle darkened corners to draw focus into the center corridor (Intensity: 0.28).
  * *Color Adjustments:* +8% Contrast and +10% Saturation for rich fairytale vibrancy.

### 9.2 UI Sorting Layers
To eliminate all clipping, overlap, and Z-fighting issues, the scene follows this strict rendering order:
1. `Background` (Sorting Order -100): Procedural terrain chunks, grass tiles, dirt paths.
2. `GroundProps` (Sorting Order -50): Wildflowers, rocks, fallen acorns, campsite fires.
3. `TargetRings` (Sorting Order -5): Blue pack rings and orange enemy targeting decals.
4. `Characters` (Sorting Order 0–20): Dynamic sorting order based on $Y$-position (`-(int)(transform.position.y * 100)`).
5. `CombatVFX` (Sorting Order 25–40): Slash arcs, magic missiles, floating combat numbers.
6. `CanvasWorldSpace` (Sorting Order 50): Unit health bars, speech bubbles, telegraph cones.
7. `CanvasScreenSpaceOverlay` (Sorting Order 100+):
   * *Top HUD:* Wave counter, Pack hearts, Camp indicator, Pause button.
   * *Control Ribbon:* Formation toggle button, 7-card dot indicators, Moonshift button.
   * *Card Deck Canvas:* 7-card horizontal scroll view with clean embossed buttons (`DeployButton_Green`, `ChargingButton_Navy`).
   * *Drag Ghost Layer:* High-priority overlay rendering dragging card previews.

---

## 10. Technical Architecture & Coding Standards

* **Singletons:** Built with non-destructive fallback pattern:
  ```csharp
  if (instance == null) instance = this;
  else if (instance != this) { Destroy(this); return; }
  ```
  Guarantees zero duplicate managers across scene reloads without destroying whole game objects or child hierarchies.
* **Component References:** Null-safe caching in `Awake()`, with robust `TryGetComponent` and `ActiveHero` null guards preventing runtime `NullReferenceExceptions`.
* **Clean Card Styling:** Strict separation of text layers from embossed art textures. Text labels remain blank when baked artwork handles primary messaging, preventing doubled or misaligned typography.

---
*Authored by the Google Deepmind Antigravity Team for the World of the Pack Development Framework.*
