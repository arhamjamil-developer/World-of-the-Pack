# ANTIGRAVITY: Build World of the Pack into a lively, polished endless game

> **READ FIRST: direct runtime observations and the immediate repair order are in [AntigravityPlaytest.md](./AntigravityPlaytest.md). Those findings take priority over adding more features. Reproduce each problem, make one repair, and verify its acceptance test before continuing.**

**Owner's instruction:** Do the hard design, implementation, integration, balancing, debugging, and testing work. Do not stop after producing a plan, a few scripts, or a visually convincing screenshot. Keep working through the backlog until the owner explicitly tells you to stop, or every agreed completion criterion is met. If an external dependency genuinely blocks one task, record the evidence and continue all independent tasks. Ask only for information or access that is actually necessary. Never claim device testing, signing, upload, or approval that you have not performed.

This is a **game design and coding handoff**, not a request to rebuild the UI. Existing artwork and UI should support the playable game. The owner wants a game that feels alive and gives players a reason to keep playing for at least 5–10 minutes per run. That duration is a design target to measure with playtests, not something a document or formula can guarantee.

Audit date: **1 November 2026, Asia/Karachi**. Findings below are from the current source, disk assets, and Unity Editor inspection. Recheck current files before changing them because another collaborator may have continued work.

## 1. Project location and operating rules

- Project root: **`B:\World of the Pack`**. Every `Assets/...`, `Design/...`, and `ProjectSettings/...` path below is relative to this exact root.
- Read `B:\World of the Pack\GEMINI.md` and applicable `AGENTS.md` instructions first.
- Unity **6000.3.13f1**, URP 2D **17.3.0**, Input System **1.19.0**, uGUI and TextMesh Pro. Portrait mobile game targeting iOS/iPadOS.
- Use the **official Unity CLI with `com.unity.pipeline`**, explicitly targeting this project. Do not substitute the third-party MCP package merely because it is installed.
- Example: `unity status --project-path "B:\World of the Pack"`.
- After writing C#, run `unity recompile --project-path "B:\World of the Pack"` before Play Mode.
- Editor commands must include `--caller plugin --skill unity-cli` and `--project-path "B:\World of the Pack"`.
- Preserve unrelated dirty changes. Do not reset the working tree, regenerate the entire game, overwrite original character sheets, or rename existing asset hierarchies casually.
- Author reusable assets/prefabs and serialize runtime references. Editor-only `AssetDatabase` lookups are tooling, not a player-build content-loading solution.

## 2. What this game should feel like

The player leads a small woodland pack north along a beautiful forest corridor. The pack marches and fights automatically, while the player makes frequent, understandable tactical choices: where to reinforce, which card to spend, when to Moonshift, which formation to use, and which camp upgrade or route to choose.

The fantasy is **protect my pack, combine their strengths, survive the next encounter, and make this run different from the last**. Automatic combat must produce readable consequences. It must not become a screen full of identical units, generic sparks, arbitrary deaths, and meters filling themselves.

Keep the existing sunny forest art direction. Protect the battlefield's readable space between the HUD and the hand. Use anticipation, attacks, projectile travel, impacts, recovery, reactions, and quiet moments to make it lively. Bigger damage numbers alone are not a combat system.

Initial release scope: a complete, replayable **Sunny Forest endless expedition**, all seven heroes with distinct attacks, and the Soldier/Rudra/Elsa enemy trio. Existing campaign/biome content can remain, but do not let five unfinished chapters prevent one excellent endless mode from shipping. Hide unsupported player-facing options until they work. Do not add multiplayer, accounts, monetization SDKs, or a backend as prerequisites.

## 3. Current implementation status: be honest about the starting point

### Present foundations

- `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/SampleScene.unity` are enabled in `ProjectSettings/EditorBuildSettings.asset`.
- Game state, formation movement, seven hero kits, autonomous combat, wave generation, boss/camp systems, card charging/deployment, save data, and menu controllers exist.
- Existing human/wolf animation libraries, hero portraits, individual combat effects, forest tiles, landmarks, and a large art direction reference are available.
- New UI assets and a scene-authored `NewPackUICanvas` are saved in SampleScene. The older `PortraitCardCanvas` is inactive as a backup. Do not reactivate both.
- The last completed `unity recompile` after the combat edits reported zero errors. A subsequent Editor console snapshot had zero current errors and eight warnings. This is compilation evidence, **not** evidence of a functioning iOS build or a finished game.
- The log buffer contains older scene/import errors. The current saved SampleScene had no duplicate serialized object IDs in the disk audit. Investigate fresh failures; distinguish historical logs from current console ground truth.

### Interrupted work that you must finish

- Generated enemy sheets and hero attack sheets have been copied into the project. **The new combat animation clips, profiles, Animator overrides, and playable enemy prefabs have not been built or connected.**
- The live Edit Mode scene inspection found **zero `UnitAttackPresentation` components**, and `WaveManager.rudraPrefab` / `elsaPrefab` were null. The new code therefore does not yet deliver the new attacks in gameplay.
- `Assets/New/Combat/Data`, `Animations`, `Prefabs`, and corresponding enemy folders were created as destinations; their existence is not proof of implemented content.
- The captured `Assets/New/Documentation/Gameplay-Preview.png` is a landscape layout capture with no live units. It shows distortion and is **not approved portrait visual QA or a successful gameplay test**.
- A prior Intro-state observation occurred while the Editor was subsequently confirmed to be in Edit Mode. Do not invent a proven startup bug from that observation. Start a fresh controlled Play Mode run and verify startup properly.

## 4. Highest-priority code problems to resolve

| Priority | Observed issue | Required result / owning path |
|---|---|---|
| P0 | Menu writes `PlayerPrefs["GameMode"]`, but the inspected gameplay scripts have no corresponding read. | Introduce one explicit run configuration and initialize Story/Endless correctly. Own this in `Scripts/Core/GameDirector.cs`, `Scripts/UI/MainMenuController.cs`, and `Scripts/Story/StoryCampaignDirector.cs` under `Assets/WorldOfThePack_Game`. Test launch from MainMenu and direct SampleScene play. |
| P0 | `StoryCampaignDirector` stores an endless flag but its inspected wave-completion handler still contains chapter camp/boss logic without an endless guard. `WaveManager` also spawns bosses every fifth wave. | One encounter director owns scheduling. Endless never ends because campaign content finished, and each boss is spawned exactly once. |
| P0 | Camps are triggered by both traveled distance in `CampManager` and wave count in `StoryCampaignDirector`. | Use one authoritative camp schedule. HUD distance and actual camp arrival must agree. Arrival must be idempotent. |
| P0 | `EnemyUnit.Die()` calls `GameDirector.OnEnemyKilled()`, which distributes card energy; `WaveManager.OnEnemyDefeated()` also distributes +20 card energy. Breach uses the same defeated event. | One reward transaction per real kill. Breaches/despawns do not grant kill rewards. Reward ownership must be explicit and tested. |
| P0 | Rudra/Elsa enum and behavior code were added, but prefabs/definitions are missing. The existing default stats branch resets unknown types to Soldier if `enemyDefinition` is absent. | Create and serialize their actual definitions and prefabs before enabling those spawns. Preserve old enum numeric values. No invisible/procedural fallback enemy in release. |
| P0 | Hero/prefab/biome/VFX fallback loading often sits inside `#if UNITY_EDITOR`. | Explicit catalogs or serialized references survive an actual player build. Validate every hero, enemy, boss, biome, projectile, and effect in the build. |
| P1 | `WaveManager` invokes `OnAllWavesCompleted` after the authored count while its loop continues; legacy HUD interprets that as victory. Rest waits also occur in two places. | Endless wave milestones are not victory. One inter-wave rest duration, no accidental double downtime, no campaign popup in endless. |
| P1 | Upgrade levels are saved by the menu, but the source search found no gameplay use of `GetUpgradeLevel`. `RecordExpeditionRun` also had no caller in the searched scripts. | Purchased upgrades alter actual stats; results/rewards and best distance save once per run. No fake upgrade buttons. |
| P1 | Tutorial contains ten reading steps, including a shared deployment-energy explanation that conflicts with current free shared-energy deployment methods. No normal caller of `CheckAndStartTutorial` was found. | Build a short action-based onboarding and choose one consistent economy. Explain what the player actually does. |
| P1 | Restart resets some systems but does not visibly reset every camp timer, story counter, cooldown, active projectile, and run modifier. | One complete run-reset contract. Retry three times without inherited camps, extra wave coroutines, stale buffs, or doubled listeners. |
| P1 | Old gameplay calls find `GameHUDController`; the active new HUD is `PackGameplayHUD`. | Route banners, camp, boss, result, tutorial, and pause events to the active presentation through a clear interface/event layer. No silent missing feedback. |
| P1 | Different frame rates, attack states, movement and special casts can interrupt each other. Existing human controllers do not necessarily have wolf special states. | Finish attack timing/animation integration and check both forms. Dead units cannot complete delayed attacks. Spell release and damage must match the visible frame. |
| P1 | Per-frame unit searches and repeated effect/projectile creation exist across combat/camera code. | Profile first, then use cached registries and bounded pools where useful. Keep object count and memory stable over a 15-minute soak. |

Inspect `HeroDeploymentCard.cs` as well: a recent change preserves charge on rejected button deployment and blocks button deployment outside Playing. Validate drag deployment, pause, rejection at population cap, and all click paths too. Fix the complete behavior rather than assuming that small change covers every path.

## 5. Design the 5–10 minute run

Use **active gameplay time**, excluding pause and camp selection. Difficulty should rise through composition and pressure, not unlimited enemy counts. The player should make a meaningful decision approximately every 8–15 seconds and see a larger change every 45–90 seconds. These are starting tuning goals.

| Run time | Player experience | Content / pacing target |
|---|---|---|
| 0:00–0:20 | Understand the pack and get a successful action immediately. | Start with Wolvy/Bobo/Elva, one ready reinforcement, and a gentle Soldier encounter. Teach one deployment through doing it. |
| 0:20–1:00 | Notice roles, charge, and the first choice. | Soldier pairs, an isolated Elsa introduction with a readable bow windup, one Moonshift prompt after the player has a reason to use it. |
| 1:00–2:00 | Establish a run build. | First camp around 75–100 active seconds. Choose one of three meaningful upgrades. Introduce Rudra guarding a Soldier or Elsa. |
| 2:00–3:00 | First satisfying test of the build. | Mixed encounters; a mini-boss/elite event around 150–180 seconds. Give a short recovery beat and clear reward. |
| 3:00–5:00 | A second build decision changes tactics. | Second camp and a route modifier. Alternate melee pressure, protected archers, and a small flank; do not stack every danger together. |
| 5:00–7:30 | The first run feels substantial. | A major encounter with a readable phase change, then a strong upgrade/recovery reward. A reasonably attentive new player should be able to reach this band after onboarding. |
| 7:30–10:00 | Skilled play pushes the personal best. | More demanding compositions and an additional elite modifier, with recoverable mistakes and visible next milestones. |
| 10:00+ | True endless continuation. | Recombine known patterns and scale within caps. No forced ending at ten minutes and no impossible instant-death switch. |

Author encounter patterns as data: patrol, shield wall, archer escort, bodyguard pair, staggered flank, elite challenge, recovery. Each has a spawn budget, spacing, warning, cooldown, and counterplay. Begin with at most roughly 10–12 active ordinary enemies and the scene's current 10-ally cap, then tune from measured readability/performance. These are design defaults, not platform requirements.

Use a bounded threat budget. A possible first curve is `baseBudget * (1 + 0.08 * activeMinutes)` with separately clamped health/damage modifiers. Tune against actual kill time, pack health, deployment frequency, and survival. Do not combine exponential stats, unlimited population, faster attacks, and higher spawn rate simultaneously.

Combat encounters should usually resolve in 12–25 seconds, with 3–6 second breathing spaces and occasional longer events. Do not force wave completion to wait forever for an offscreen enemy. Recover or resolve stuck agents explicitly without awarding exploit rewards.

The next camp meter must use the same authoritative distance as camp spawning. If encounters are timed, pace camp placement against expected travel; do not show a distance countdown that secretly triggers from an unrelated wave number.

## 6. Make each hero worth choosing

Preserve the seven named identities and existing human/wolf forms. Every basic attack needs a visible anticipation, a release/contact, and a recovery. Add a small hit reaction and intentional sound. Normalize sprite scale and foot position so attack transitions do not make a unit grow, shrink, or jump.

| Hero ID | Identity and basic attack | Tactical purpose / upgrade direction |
|---|---|---|
| `HeroWolvy` | Blue-caped sword leader; crescent sweep with restrained nearby cleave. | Holds the front. Upgrades can widen cleave or reward staying near allies. Alpha survival must be clearly communicated. |
| `PrincessElva` | Green-hooded ranger; bow draw, release, separate nature arrow. | Reliable ranged pressure. Choices between precision/crit and a limited volley. |
| `FairyLulu` | Blue-white moon healer; crescent staff cast, separate moon orb, distinct healing effect. | Sustain and protection. Healing targets must be injured allies; no permanent full-health cast spam or interruption every frame. |
| `CaptainBobo` | Gold-caped shield guardian; planted shield bash and earth/acorn impact. | Protects nearby allies. Distinguish protection from simply having more health. |
| `ScoutPip` | Teal-scarfed skirmisher; fast dagger lunge / two-hit rhythm. | Removes exposed targets. Keep total combo damage intentional; do not accidentally double the original damage. |
| `WizardMomo` | Purple star wizard; wand cast and star bolt with a readable chained impact. | Answers groups. Secondary bolts begin after the first hit, with bounded bounce count and range. |
| `TinkerTilly` | Goggles and brass machinery; spring launcher recoil and a separate gear bolt. | Mechanical damage and run utility. Any trap must be visible and have an actual lifespan and effect. |

Existing kit definitions are in `Assets/WorldOfThePack_Game/Data/HeroKits/{HeroID}_Kit.asset`. Existing combat logic is `Assets/WorldOfThePack_Game/Scripts/Combat/HeroUnit.cs`. Do not replace seven heroes with seven recolors of the same projectile.

Choose one consistent Moonshift model: recommended initial design is a **timed empowered form**, with a visible duration and cooldown, distinct from independent card charge. Avoid the current conceptual overlap between a free global form toggle, morale instinct, and per-card wolf surge. Define their interaction in code and tutorial before balancing.

Recommended economy: independent **0–100 charge per card** is the reinforcement cost. Kills add one defined charge reward and passive trickle prevents a dead hand. Shared berries become a separately explained camp/run resource, or remove their gameplay role until implemented. Do not show berries as a deployment cost while every deployment ignores them. Start tuning from the existing 100/60/40 card distribution; adjust passive recharge and kill rewards so choices remain frequent without trivial duplicate-unit flooding.

## 7. Enemy design and integration

- **Soldier:** readable baseline melee threat. Walk south, brace shield, wind up, sword contact, recover, recoil and collapse on death. Must be beatable without spending every resource.
- **Rudra:** muscular bodyguard with bronze gauntlets and crimson sash. Intercepts a bounded portion of damage for nearby enemy allies, making the player care about target priority. New code currently intercepts 25% within 1.6 world units. Treat these as tuning values. Show the protected relationship subtly; avoid overlapping opaque shield bubbles. His heavy slam needs a warning and an escape/counterplay window. Protection cannot recurse between guardians.
- **Elsa:** original auburn-haired woodland queen archer with crown, teal/burgundy clothing, and golden bow. She is a distinct enemy from Princess Elva. Use bow draw → release → separate traveling royal arrow → impact. An arrow must not apply damage at spawn and again on impact. Cancel an unlaunched shot if the archer dies; a launched arrow can finish its flight under an explicit rule.
- Keep legacy `Hound` and `Captain` enum values stable while deciding whether to retain them in the release roster. Do not leave old waves spawning static placeholder enemies while the new trio appears only in a showcase.
- Add enemies to normal authored encounters and endless composition. A prefab sitting in a folder does not satisfy integration.
- Enemy deaths must complete visibly before despawn. Breach, kill, scripted despawn, pool return, and boss defeat need distinct outcomes. Counters cannot decrement or reward twice.

## 8. Exact new artwork paths and what remains to do

All following files exist under `B:\World of the Pack\`. These were generated with the built-in image generation tool. **Do not regenerate the finished sheets before trying to import and preview them.**

### Enemy sheets

- `Assets/New/Enemy/Art/Soldier_South.png`
- `Assets/New/Enemy/Art/Rudra_South.png`
- `Assets/New/Enemy/Art/Elsa_South.png`
- Each is **1536×1024**, **6 columns × 4 rows**, **256×256 per cell**. Rows from top: **Idle, Walk, Attack, Dead**, six frames each. South-facing enemies. Preserve cell registration; do not independently trim every animation frame.
- `Assets/New/Enemy/Projectiles/ElsaArrow.png` is a **2172×724** isolated arrow image. Import one alpha-bounded sprite. Its art points right; rotate it toward flight direction.
- Destination folders already exist: `Assets/New/Enemy/Animations`, `Assets/New/Enemy/Prefabs`, `Assets/New/Enemy/Data`. Create actual definitions, clips/controllers, and Soldier/Rudra/Elsa prefabs there. These are pending outputs, not completed assets.

### Hero attack sheets

`Assets/New/Combat/Art/{HeroID}_Attack_North.png` exists for **all seven IDs in section 6**.

- Every sheet is **2172×724**, **6 columns × 2 rows**, **362×362 per cell**.
- Top row: six **human** attack frames. Bottom row: six **wolf** attack frames. All face north, matching existing march direction.
- Do **not** slice these using the old 256-pixel hero grid. Read actual dimensions.
- Original identity/idle/walk/death sources remain at `Assets/Characters/{HeroID}/Human/{HeroID}_Human_North.png` and `/Wolf/{HeroID}_Wolf_North.png`.
- Existing animation/controller libraries: `Assets/Characters/{HeroID}/Animations/`.
- Existing form prefabs: `Assets/Characters/{HeroID}/Prefabs/`.
- Existing gameplay prefabs: `Assets/WorldOfThePack_Game/Prefabs/Units/{HeroID}_PackUnit.prefab`.
- Prefer new attack clips and Animator overrides under `Assets/New/Combat/Animations/`, then new/updated unit prefabs with serialized references. Preserve original libraries and inspect both forms.
- Begin with a six-frame 0.6-second attack and release on frame four, then tune. Existing new profiles expose duration and release fraction; synchronize them with the actual clip.

### Separate hero projectile art

- `Assets/New/Combat/Art/Projectiles/ElvaArrow.png` — 2172×724.
- `Assets/New/Combat/Art/Projectiles/LuluMoonOrb.png` — 1324×1188.
- `Assets/New/Combat/Art/Projectiles/MomoStarBolt.png` — 1402×1122.
- `Assets/New/Combat/Art/Projectiles/TillyGearBolt.png` — 1536×1024.
- These are individual projectile images, not animation grids. Use alpha bounds, appropriate on-screen scale, correct orientation and bounded lifetime. Do not render the entire padded canvas as an oversized projectile.

### New combat code already written, but not integrated

- `Assets/New/Combat/Scripts/AttackPresentationProfile.cs` — authored duration, release timing, projectile art/speed/size, impact frames, and melee variations.
- `Assets/New/Combat/Scripts/UnitAttackPresentation.cs` — per-unit profile reference and projectile/impact helpers.
- `Assets/New/Combat/Scripts/CombatSpriteProjectile.cs` — traveling projectile, impact-time damage/source attribution, optional bounded secondary target.
- `Assets/New/Combat/Scripts/AttackSpriteEffect.cs` — separate impact flipbook playback.
- Existing `HeroUnit.cs`, `EnemyUnit.cs`, `Combatant.cs`, and `WaveManager.cs` have partial integration edits. Audit and finish them. Do not assume compiling means profiles are assigned.
- `Assets/New/Combat/Data/`, `Animations/`, `Prefabs/`, and `Editor/` are intended destinations for your remaining authored content and integration tooling.

### Remaining sprite checks / actual art gaps

The read-only audit at `Assets/New/Documentation/SpriteAudit.json` checked **15 new PNGs**, **156 nonempty body-animation cells plus five projectile images**, and transparent pixels in every file. It does not prove seamless animation, consistent scale/pivots, or correct gameplay.

Check each sheet at game scale: silhouette, foot registration, frame clipping, identity consistency, alpha halos, attack direction, first/last-frame continuity, and weapon release. Elsa's attack poses turn sideways; test whether aiming readability needs a presentation adjustment rather than rotating the whole character unnaturally. Generated alpha may contain faint edge pixels; inspect against both light and dark environments.

Prioritize missing presentation work in this order:

1. **Integration first:** all 156 existing new body frames, five projectiles, existing impact flipbooks, and death playback. Much of the missing view is wiring rather than missing art.
2. **Small reactions:** hit flash, short recoil, grounded contact shadow, spawn arrival, death fade. A separate hit-reaction sheet is optional if tasteful code-based reactions work with the current sprites.
3. **Rudra protection and slam readability:** existing circle telegraphs and earth impacts can be reused; a bespoke protection indicator is only needed if those fail visually.
4. **Boss coherence:** existing boss definitions outnumber verified fully animated boss art. Finish one forest boss with idle/move/attack/death and phase feedback; do not ship a placeholder hero sprite as a boss.
5. **Run decisions/results:** camp-upgrade icons, selected/unselected states, reward currency, personal-best feedback. Reuse established visual language and keep text editable.
6. **Store assets:** approved app icon and real-device gameplay screenshots after the game is playable. A mockup is not a gameplay screenshot.

Existing separate VFX frames are at `Assets/VFX/WorldOfThePack/Art/{HeroID}/`, including MoonlightSlash, EarthImpact, MoonBubble, NatureArrow/LeafImpact, WindStrike/QuickImpact, star effects, and SpringBolt/BrassImpact. Shared and telegraph effects are in sibling `Shared/` and `Telegraphs/` directories. Inspect and reuse them before requesting more generation.

Prompts/provenance for this generated work: `Assets/New/Documentation/GenerationPrompts.md`. Main visual reference: `Assets/Visuals/ArtDirection/sunny-forest-integrated-gameplay-v03.png`. Existing design references also live under `Design/ArtDirection/` and `Assets/Visuals/board-01-main-menu-game-flow.png`, `board-02-story-level-journey.png`, and `board-03-endless-mode-loop.png`.

## 9. Give the run choices and a reason to replay

- Camps offer **one of three** clearly different upgrades, not five unexplained buttons. Examples: Sword Arc, Ranger Focus, Moonwell Healing, Shield Bond, Quick Paws, Star Chain, Clockwork Charge. Describe the exact benefit and show the chosen upgrade in a compact run summary.
- Seed the choice generator so bugs can be reproduced. Avoid offering useless upgrades for unavailable heroes. Cap stacks and show caps.
- Route choices should change encounter composition and reward/risk: safer path with more recovery, archer-heavy path with greater rewards, elite path with an explicit warning. Do not call a tint change a new route mechanic.
- Add two or three run objectives: survive a milestone, deploy three different heroes, defeat a guarded archer. Keep them optional and reward normal play rather than farming menus.
- End-of-run screen shows active survival time, distance, waves, kills, elite/boss defeats, earned shards, best record, and one clear Retry action. Results save once. Retrying should take seconds.
- Keep persistent upgrades modest: understandable capped improvements rather than mandatory grinding. Existing menu purchases must actually modify gameplay, explain cost/level, and persist. Grant rewards from completed run events; do not require a store or ad to reach the intended first-run duration.
- Unlock Endless after a short introduction, or expose it immediately for this release. Requiring five unverified chapters conflicts with delivering the owner's endless game. Preserve campaign save data when changing unlock rules.

## 10. Liveliness is animation, feedback, sound, and pacing

Implement a coherent combat-feedback pass: subtle idle breathing, walk speed matched to movement, anticipation before hits, quick contact flash/recoil, distinct projectile silhouettes, restrained impact effects, short death holds, floating numbers only when readable, and mild optional camera shake for heavy impacts.

The audit did not find project audio clips in `Assets` or a complete game audio controller in the searched gameplay source. Add a small practical audio system: forest ambience, calm/combat music transitions, sword/shield/bow/magic/mechanical signatures, hit/death cues, card-ready/deploy/denied cues, camp/reward cues, and menu clicks. Use owned/licensed audio and document provenance. Settings must truly control music, SFX, master volume and haptics, persist, and restore on startup. Avoid layering a new sound for every damage tick at full volume; cap voices and add small pitch variation.

Keep camera ownership singular. `CinemachineCombatDirector` deliberately disables `ExpeditionCamera`; do not enable competing camera drivers. Compute a safe combat region from the camera and the actual HUD/card bounds, particularly on iPad. A unit behind the cards is not acceptable combat readability.

## 11. Main path map for implementation

All paths resolve under **`B:\World of the Pack`**:

| Responsibility | Existing owning location |
|---|---|
| Run lifecycle, spawn catalog, energy, pause, restart, defeat | `Assets/WorldOfThePack_Game/Scripts/Core/GameDirector.cs` |
| Waves, enemy prefab slots, spawn accounting | `Assets/WorldOfThePack_Game/Scripts/Core/WaveManager.cs` |
| Movement, lane assignment, formation changes | `Assets/WorldOfThePack_Game/Scripts/Core/FormationController.cs`, `LaneSystem.cs` |
| Camera / battlefield framing | `Assets/WorldOfThePack_Game/Scripts/Core/CinemachineCombatDirector.cs` |
| Hero, enemy, boss, health, damage | `Assets/WorldOfThePack_Game/Scripts/Combat/` |
| Hero attack/stats data | `Assets/WorldOfThePack_Game/Data/HeroKits/` |
| Existing boss data / gameplay unit prefabs | `Assets/WorldOfThePack_Game/Data/Bosses/`, `Assets/WorldOfThePack_Game/Prefabs/Units/` |
| Card hand energy and deployment hookups | `Assets/WorldOfThePack_Game/Scripts/UI/CardDeckManager.cs` |
| Card state, pointer drag, deploy acceptance | `Assets/UI/WorldOfThePack/Scripts/HeroDeploymentCard.cs` |
| Active new HUD and paging | `Assets/New/Scripts/PackGameplayHUD.cs`, `PackCardPager.cs`, `PackCardCopy.cs` |
| New UI authoring builder | `Assets/New/Editor/PackUIBuilder.cs`; rebuild only when necessary, since it changes the scene hierarchy and deck references |
| New UI prefab / seven card prefabs / definitions | `Assets/New/Prefabs/`, `Assets/New/Data/` |
| Legacy HUD still referenced by old gameplay code | `Assets/WorldOfThePack_Game/Scripts/UI/GameHUDController.cs` |
| Camp arrival, route modifiers, camp state | `Assets/WorldOfThePack_Game/Scripts/Camp/CampManager.cs` |
| Competing chapter progression | `Assets/WorldOfThePack_Game/Scripts/Story/StoryCampaignDirector.cs` |
| Save model and persistence | `Assets/WorldOfThePack_Game/Scripts/Progression/GameSaveData.cs`, `SaveManager.cs` |
| Menu routing and upgrade purchases | `Assets/WorldOfThePack_Game/Scripts/UI/MainMenuController.cs` |
| Onboarding | `Assets/WorldOfThePack_Game/Scripts/Tutorial/TutorialDirector.cs` |
| World and tile recycling | `Assets/WorldOfThePack_Game/Scripts/Environment/WorldBiomeGenerator.cs`, `EndlessPathScroller.cs` |
| Existing environment art | `Assets/Environemnt/EndlessPathSystem/Artwork/` and `Assets/Environemnt/SunnyForestTrail/Artwork/` — preserve the existing `Environemnt` spelling |
| Existing animation authoring reference | `Assets/Characters/_Pipeline/Editor/PackCharacterAssetBuilder.cs`; reuse patterns but do not rerun its entire destructive rebuild blindly |
| Old scene rebuild tools | `Assets/WorldOfThePack_Game/Editor/GameSceneBuilder.cs`, `MockupSceneBuilder.cs`, `ReconstructGameUI.cs`; inspect before running because they can replace current wiring |
| Build configuration | `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `Packages/manifest.json` |

Suggested new code/data destinations, **not claims of existing implementations**: `Assets/New/Endless/Runtime/`, `Assets/New/Endless/Data/`, `Assets/New/Endless/Editor/`, `Assets/New/Audio/`, `Assets/New/Tests/EditMode/`, and `Assets/New/Tests/PlayMode/`. Keep gameplay systems decoupled from UI and make content tunable through ScriptableObjects.

## 12. Persistence, mobile behavior, and release hardening

Create one run state containing mode, seed, active elapsed time, distance, wave, score, selected upgrades, route, and reward-finalization flag. Separate it from persistent unlocks/settings. Validate save values, version migrations, missing lists, and corrupt data without silently losing all legitimate progress. Use a safe backup/atomic strategy if moving away from PlayerPrefs.

Handle app backgrounding and focus loss: pause gameplay, preserve settings/progress at deliberate points, resume into an explicit pause state rather than allowing unseen combat, and restore audio correctly. Do not award elapsed survival time while the app is suspended. Test interruptions during deployment, projectile flight, camp, and defeat.

Profile a real device. Set a measured frame-rate target, compress/atlas art appropriately for iOS, avoid shipping all source-sized generated textures uncompressed, prevent repeated material creation, bound particles and pooled objects, and verify memory does not climb with every wave. Do not optimize by removing essential attack/death frames or making telegraphs unreadable.

The existing UI must support editable wave numbers **01–1000**, card names, 0–100 rectangular charge fills, actual pack health, camp distance/progress, pause/resume, formation, Moonshift, clickable dots, and horizontal swiping without accidental deployment. Five window dots currently represent seven cards with three visible; verify the owner-facing behavior instead of adding decorative dots that do nothing.

## 13. App Store readiness: evidence required, not a label

Current project settings still show `DefaultCompany`, only a default standalone application identifier in the inspected block, an empty Apple team ID, and empty iOS icon texture slots. Confirm the actual iOS build profile and configure the approved bundle ID, company, version/build, icon, supported devices/orientations, minimum OS, IL2CPP/ARM64, and signing. Do not invent the owner's team, certificates, or bundle identity.

Prepare the game so it can be exported from Windows, then validate/archive/sign on a suitable Mac with Xcode and the owner's authorized provisioning. Produce a TestFlight build and device evidence when access exists. If Mac/signing access is unavailable, finish all local work and provide exact remaining steps; do not call the game submission-ready.

Apple's submission page opened during this audit states the iOS/iPadOS 26 SDK-or-later requirement effective April 28, 2026. Recheck the current requirement before exporting/submitting; do not confuse SDK version with deployment target. Source: [Apple submission requirements](https://developer.apple.com/app-store/submitting/).

Check crash-free complete flows, working support/privacy URLs, accurate age rating and privacy disclosures, SDK privacy manifests/required-reason API declarations as applicable, appropriate permissions, asset/audio rights, real gameplay screenshots, and complete review notes. Do not add tracking/ATT prompts without an actual tracking feature. Do not claim Apple approval from passing local tests. References: [App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/), [App privacy details](https://developer.apple.com/app-store/app-privacy-details/), [Privacy manifests](https://developer.apple.com/documentation/bundleresources/adding-a-privacy-manifest-to-your-app-or-third-party-sdk).

## 14. Execute in this order and keep going

1. **Establish a reproducible baseline:** preserve the current work, compile, start SampleScene, record actual run state/scene wiring, and verify one real deployment and kill. Use fresh logs. Investigate the Device Simulator graphics warning separately from actual target-device behavior.
2. **Finish generated combat integration:** import/slice all sheets; author clips, controllers/overrides, profiles and prefabs; serialize all catalogs; connect the three enemies and all seven hero attacks to actual encounters. Verify human/wolf forms and death cleanup.
3. **Repair lifecycle and economy:** mode selection, single kill rewards, correct failed deployments, camp/boss ownership, bounded enemy accounting, complete retry, and pause/background handling.
4. **Build the endless run:** authored encounter catalog, seeded pacing/threat director, run upgrades, camp choices/routes, visible milestones, and fair difficulty escalation.
5. **Close the loop:** action onboarding, results, real persistent upgrades/unlocks, reward saving, record chasing, and immediate retry.
6. **Make it lively:** readable reactions, actual sound, hero/enemy identity, camera framing, effect restraint, and consistent animation scale/timing.
7. **Test and balance:** multiple 5–10 minute human-played runs, seeded automated regressions, portrait/iPad checks, player-build reference validation, and a 15-minute stability soak.
8. **Prepare release evidence:** actual iOS export, Mac/Xcode/device/TestFlight work when available, store assets/metadata, and an honest remaining-blocker list.

Maintain `B:\World of the Pack\AntigravityProgress.md` with completed work, changed files, commands/results, measured run duration, active blockers, and the next concrete task. This is a requested future progress file. Mark stages done only when their acceptance checks pass. Do not repeatedly write plans instead of implementing them.

## 15. Definition of done

- [ ] A fresh player can launch from MainMenu, understand one action quickly, and play an uninterrupted, understandable endless run.
- [ ] Endless continues beyond all authored waves and beyond ten minutes; no accidental campaign victory, duplicated boss/camp, or stuck wave.
- [ ] All seven heroes and Soldier/Rudra/Elsa appear in normal gameplay with distinct correctly timed attacks. Elsa and Elva fire separate arrows; mage/mechanical projectiles are visually distinct.
- [ ] Actual damage happens at strike/arrival, once per intended hit; dead units cannot release delayed attacks; kill rewards cannot double or come from breach/despawn.
- [ ] Charge, pause, swipe/dots, drag/click deployment, formation, Moonshift, distance and health display actual game state. Rejected deployments preserve energy.
- [ ] Three consecutive retries reset all run state without duplicated listeners, projectiles, camps, waves or buffs.
- [ ] Run choices meaningfully change tactics; results and purchases affect real saved progression exactly once.
- [ ] Fresh/returning saves, corrupt-save recovery, app interruption, audio settings and background/resume behavior are verified.
- [ ] Several observed runs support the 5–10 minute engagement/survival target; record what players did and where they disengaged. Do not infer retention from a single automated idle run.
- [ ] A 15-minute soak has bounded population/object count, stable memory, and no fresh exceptions. Measure device frame times rather than claiming performance from the Editor.
- [ ] Actual portrait phone and iPad layouts show readable combat, no covered units/controls, no distorted art, and no unintended input conflicts.
- [ ] A player build loads all content without Editor-only fallback dependencies.
- [ ] Required iOS/device/signing/store work has real evidence, or remaining external blockers are stated precisely.

**Antigravity: own the hard work. Implement, integrate, play, measure, fix, and repeat. Keep moving through the next actionable task until the owner tells you to stop or the agreed work is complete. Preserve uncertainty where evidence is missing. A generated sprite, compiling script, or attractive screenshot alone is not the finished game.**
