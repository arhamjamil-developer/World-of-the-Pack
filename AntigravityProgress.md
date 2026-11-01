# Antigravity Progress & Verification Log

Project: `B:\World of the Pack`
Start Date: 1 November 2026

## Work Tracking Overview

- [x] Task 1: Reproduce and resolve live-unit visibility
- [x] Task 2: Fix camp and distance ownership
- [x] Task 3: Fix health synchronization and defeat explanation
- [x] Task 4: Complete generated combat integration (animations, profiles, Soldier/Rudra/Elsa prefabs)
- [x] Task 5: Remove duplicate rewards and finish complete reset/restart contract
- [x] Task 6: Pacing and encounter tuning (first minute, first camp, 5-10 minute endless run)

---

### Task 6: Pacing and Encounter Tuning
- **Reproduction:** During early playtests, pacing felt rushed: within under 60 seconds the game burned through 18 kills and 4 wave tiers, triggering camp abruptly without strategic choices or clear progression; camp options offered generic full heals without numbers or reset logic; card energy recharge was disconnected from camp bonuses; and endless waves could spawn unbounded swarms causing runaway difficulty spikes.
- **Root Cause:** `firstCampDistance` was uncalibrated to march speed, camp options in `CampManager` lacked authored gameplay modifiers with concrete numerical values, `CardDeckManager` did not consume camp recharge multipliers, and `WaveManager` lacked threat budgeting for post-boss endless waves.
- **Files Changed:** `Assets/WorldOfThePack_Game/Scripts/Camp/CampManager.cs`, `Assets/WorldOfThePack_Game/Scripts/Core/WaveManager.cs`, `Assets/WorldOfThePack_Game/Scripts/UI/CardDeckManager.cs`, `Assets/New/Scripts/PackGameplayHUD.cs`, `Assets/Scenes/SampleScene.unity`.
- **Test Performed:** Calibrated `firstCampDistance` to 16 units (64m) for ~75s arrival matching wave pacing; authored 3 distinct options (`RESTORE 25% LEADER HEALTH`, `PACK POWER • +20% ATTACK DAMAGE`, `MOON CHARGE • +30% RECHARGE RATE`) with real-time stat binding and reset contracts; scaled card recharge via `EnergyRegenMultiplier`; bounded endless waves to 4–10 enemies with composition mixing; executed multiple live Play Mode runs verifying exact arrival at 74.4s–74.6s, option selection, stat application (`PackDamageBuff=1.20`, `EnergyRegenMultiplier=1.30`), next leg countdown (120m), and clean UI (`run_camp_clean_modal.png`).
- **Observed Result:** Pack encounters lone Soldier (Wave 0), lone Elsa (Wave 1), lone Rudra (Wave 2), and Royal Escort (Wave 3) in sequence; reaches first campsite reliably at 74.6s with full pack formation visible; camp modal displays 3 clean options with numbers and no stale text; selecting Option 1 applies `PackDamageBuff=1.20`; selecting Option 2 applies `EnergyRegenMultiplier=1.30`; subsequent camp legs start countdown at 120m; 0 compilation errors and 0 runtime console errors.

---

### Task 5: Complete Reset & Restart Contract
- **Reproduction:** In-flight restart left surviving or unlisted battlefield units alive on screen, `CampManager` retained stale route distance/modifiers from the prior run, and `Time.timeScale` remained slowed at `0.4f` instead of restoring active gameplay speed `1.0f`.
- **Root Cause:** `RestartGame()` only iterated over `activeAllies`, leaving orphaned units alive, failing to call `CampManager.ResetCampState()`, and neglecting to restore `Time.timeScale`.
- **Files Changed:** `Assets/WorldOfThePack_Game/Scripts/Core/GameDirector.cs`, `Assets/WorldOfThePack_Game/Scripts/Core/FormationController.cs`, `Assets/WorldOfThePack_Game/Scripts/Camp/CampManager.cs`.
- **Test Performed:** Executed 3 consecutive in-flight retries via `RestartGame()`, logging state, health, ally count, wave, and camp distance.
- **Observed Result:** 3/3 retries cleanly reset to State=Playing, Alpha=HeroWolvy (140/140), Allies=4, Wave=1, CampDist=80.0, TimeScale=1.0 without duplicate listeners or stale objects.

---

### Task 4: Complete Generated Combat Integration
- **Reproduction:** Battlefield units fell back to placeholder or soldier combat behaviors without using generated sheets. `UnitAttackPresentation` on all hero prefabs had `_profile: null`, `rudraPrefab` and `elsaPrefab` on `WaveManager` were unassigned (`{fileID: 0}`), enemy prefabs lacked controllers (`m_Controller: {fileID: 0}` throwing hundreds of `Animator is not playing an AnimatorController` warnings per second), and waves burned through procedural soldier/hound swarms without encountering Elsa or Rudra.
- **Root Cause:** Prefab pipeline had not serialized `_profile` and `m_Controller` via `SerializedObject` upon creation, and `WaveManager` was missing authored definitions for the teach-combine-challenge encounter structure.
- **Files Changed:** `Assets/New/Combat/Editor/CombatAssetPipelineBuilder.cs`, `Assets/WorldOfThePack_Game/Scripts/Core/WaveManager.cs`, `Assets/WorldOfThePack_Game/Scripts/Combat/Combatant.cs`, `Assets/WorldOfThePack_Game/Scripts/Core/GameDirector.cs`, `Assets/Scenes/SampleScene.unity`, `Enemy_Soldier.prefab`, `Enemy_Rudra.prefab`, `Enemy_Elsa.prefab`, `Soldier_South.controller`, `Rudra_South.controller`, `Elsa_South.controller`, and 7 `*_PackUnit.prefab` assets.
- **Test Performed:** Verified all 10 `AttackPresentationProfile` assets; confirmed serialized GUIDs on all 10 unit prefabs; verified `WaveManager` inspector and scene wiring; ran Play Mode across waves 0 through 4 (Wave 0: Soldiers, Wave 1: Royal Archer Elsa, Wave 2: Iron Bodyguard Rudra, Wave 3: Royal Escort Formation, Wave 4: Ancient Thorn Guardian Boss); verified projectile spawn-to-arrival damage attribution (`spawns=36, impacts=32`) using `ElsaArrow.png`; captured live combat screenshot `run_combat_wave2_elsa.png`.
- **Observed Result:** All 3 enemy types spawn with correct sprite frames, walk/attack/death animations, and distinct profiles; Elsa fires `ElsaArrow.png` arrows dealing damage upon arrival; hero ranged and melee attacks trigger their authored VFX; death playback finishes before despawn; 0 console errors and 0 missing controller warnings.

---

### Task 3: Health Synchronization & Defeat Explanation
- **Reproduction:** Visible HUD hearts appeared depleted even though alpha leader subsequently read full HP. In addition, when Wolvy fell in combat, the defeat modal displayed generic `PACK DEFEATED` while three allies were still alive on the field.
- **Root Cause:** `HeroUnit.TakeDamage` forwarded to `GameDirector.OnAlphaHealthChanged`, but `HeroUnit` lacked an override for `Heal(float)` and `GameDirector` did not subscribe to `alphaLeader.OnHealthChanged`, leaving Lulu/camp heals disconnected from HUD hearts.
- **Files Changed:** `Assets/WorldOfThePack_Game/Scripts/Combat/HeroUnit.cs`, `Assets/WorldOfThePack_Game/Scripts/Core/GameDirector.cs`, `Assets/New/Scripts/PackGameplayHUD.cs`, `Assets/Scenes/SampleScene.unity`.
- **Test Performed:** Controlled 140/140 → damage 50 (HP drops to 76.75) → heal 30 (HP rises to 106.75) → defeat (HP 0). Inspected `GameDirector.CurrentPackHealth` and `PackGameplayHUD.modalTitle`.
- **Observed Result:** Hearts track Wolvy's authoritative health on both damage and healing; HUD label updated to `LEADER HEALTH`; defeat modal clearly reads `WOLVY FELL` and freezes enemy combat.

---

### Task 2: Camp Arrival & Distance Ownership
- **Reproduction:** `StoryCampaignDirector` was triggering `CampManager.ArriveAtCamp()` unconditionally after 3 waves (~50s) while distance-based `RemainingDistance` still had hundreds of meters remaining, resulting in `NEXT CAMP • 279m` displaying on an open camp modal; `WaveManager` also continued spawning enemies mid-wave behind the modal.
- **Root Cause:** Competing camp scheduling owners (story wave count vs run distance) and lack of modal pause synchronization.
- **Files Changed:** `Assets/WorldOfThePack_Game/Scripts/Story/StoryCampaignDirector.cs`, `Assets/WorldOfThePack_Game/Scripts/Camp/CampManager.cs`, `Assets/WorldOfThePack_Game/Scripts/Core/WaveManager.cs`, `Assets/New/Scripts/PackGameplayHUD.cs`.
- **Test Performed:** Verified distance countdown, forced camp arrival, checked `isAtCamp` idempotency, verified enemy wave loop pauses during camp, and checked HUD label.
- **Observed Result:** Single authoritative distance owner (`CampManager`). When camp arrives, `RemainingDistance = 0`, `LegProgress = 1.0`, HUD displays `CAMP REACHED`, enemies do not attack behind modal, and departure resets to the next leg.

---

### Task 1: Live-Unit Visibility & Readability
- **Reproduction:** During playtest, battlefield appeared empty or obstructed during camp and march; units in rear slots (e.g. Lulu/Tilly) were clipped near bottom card tray (`WoodTray` covering up to viewport Y 0.282), and `CampModal` had a 85% pitch-black scrim with a 520px board covering viewport Y 0.34–0.66 right over the stopped pack.
- **Root Cause:** Camera vertical framing lead and oversized centered modal with opaque backdrop completely masked pack units.
- **Files Changed:** `Assets/Scenes/SampleScene.unity`, `Assets/New/Scripts/PackGameplayHUD.cs`.
- **Test Performed:** Fresh portrait run in Play Mode; evaluated viewport coordinates of all 4 heroes (`Wolvy vp Y=0.49`, `Bobo vp Y=0.45`, `Elva vp Y=0.45`, `Lulu vp Y=0.41`); captured full-resolution Game View screenshots `run_fresh_01.png` and `run_camp_arrival.png`.
- **Observed Result:** Pack heroes march cleanly centered in battlefield with a ~13% screen height gap above the card hand; at camp, resting pack and ambient trail are clearly visible below the elevated modal panel and through the softened backdrop.

---
