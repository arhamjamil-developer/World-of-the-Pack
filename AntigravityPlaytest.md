# Antigravity: concrete playtest problems and implementation instructions

Project: `B:\World of the Pack`. Read this before continuing `AntigravityReadit.md`.

## Evidence and limits

I entered the current `Assets/Scenes/SampleScene.unity` through the official Unity CLI, observed its portrait Game View, and read live game state. Compilation was up to date with no reported errors. This was a partial Editor playtest, not a completed controlled 5–10 minute human run or an iOS-device test. Desktop user input interrupted the attempted camp-button click; do not claim that click succeeded or that every interaction was tested. Later changes in camp state may include user interaction.

Observed snapshots:

- At approximately **58.47 game seconds**: Playing, wave index reported as **wave 4**, **18 kills**, four allies, zero currently spawned enemies.
- The nearby visual snapshot showed a camp-choice modal, a mushroom environment, **NEXT CAMP approximately 278m**, and no readable pack in the main battlefield. The screenshot and subsequent state query were not simultaneous; the visible badge still showed wave 3 in the screenshot.
- While at camp, the camera was approximately `(0, 11.76, -10)` and the formation anchor `(0, 10.91, 0)`.
- Live hero coordinates projected inside the Game View: Wolvy around viewport `(0.50, 0.49)`, Bobo `(0.42, 0.45)`, Elva `(0.58, 0.44)`, Lulu `(0.50, 0.40)`. Those are central positions, not behind the bottom hand.
- A subsequent query found Wolvy at **140/140 HP**, Bobo **183/190**, Elva **90/90**, Lulu **85/85**. The preceding screenshot appeared to show only one red heart, so healing synchronization needs reproduction and repair.
- Later the live run was in **GameOver**, at wave 5, **29 kills**, three surviving allies, alpha HP **0/140**, and `Time.timeScale = 0.4`. The later sample had game clock approximately 206 seconds; this is not the measured time of death or wall-clock session length.
- Surviving hero Body renderers had active human visuals, enabled renderers, non-null animation sprites, scale `(0.4, 0.4, 0.4)`, and sorting orders approximately 242–258. Merely moving the camera or adding another spawn will not explain away the earlier invisible-battlefield observation.

## Repair 1 — make the live pack visible and readable

**Priority: P0.** The main view looked like an empty road while combat statistics advanced. A player cannot make tactical decisions without seeing units, attacks, and health.

Relevant files:

- `Assets/WorldOfThePack_Game/Scripts/Combat/Combatant.cs`
- `Assets/WorldOfThePack_Game/Scripts/Combat/HeroUnit.cs`
- `Assets/WorldOfThePack_Game/Scripts/Core/CinemachineCombatDirector.cs`
- `Assets/WorldOfThePack_Game/Scripts/Environment/WorldBiomeGenerator.cs`
- `Assets/WorldOfThePack_Game/Prefabs/Units/*_PackUnit.prefab`
- `Assets/Characters/{HeroID}/Prefabs/` and `/Animations/`
- `Assets/Scenes/SampleScene.unity`

**Do this:** Run a fresh portrait test and pause when the pack is near screen center. Inspect actual active sprite alpha/rect, SpriteRenderer color/material/shader, sorting layer/order, SortingGroup overrides, camera culling mask, world Z, parent scale, and overlapping environment/overlay renderers. Temporarily isolate render layers in Play Mode to identify the culprit; do not save debug removals. Compare Scene View and Game View at the same frame. If it is only a transient capture issue, prove that with consecutive live captures and record the correction.

**Do not:** Assume all units are offscreen; the measured viewport coordinates contradict that diagnosis for the sampled frame. Do not enlarge every unit or alter camera zoom randomly until something appears.

**Pass:** A fresh run visibly shows four starter allies, incoming enemies, attacks, and health indicators. They remain visible before/during/after camp and transformation, with a clear gap above the card hand. Provide same-frame scene/state and portrait screenshots.

## Repair 2 — camp arrival and distance must describe the same event

**Priority: P0.** The player was presented with a camp choice while the HUD still advertised hundreds of meters to the next camp. This makes the route feel arbitrary.

Relevant files:

- `Assets/WorldOfThePack_Game/Scripts/Camp/CampManager.cs`
- `Assets/WorldOfThePack_Game/Scripts/Story/StoryCampaignDirector.cs`
- `Assets/WorldOfThePack_Game/Scripts/Core/WaveManager.cs`
- `Assets/New/Scripts/PackGameplayHUD.cs`

**Do this:** Choose one camp-scheduling owner. Currently distance-based camp arrival and story wave-count camp arrival coexist. Use the same run-distance value for spawning, arrival, progress fill, and the label. During arrival show `CAMP REACHED`, then show the new leg's remaining distance only after departure. Make `ArriveAtCamp` idempotent. Ensure a pending spawn loop cannot resume enemies behind an open modal.

**Pass:** Log one camp ID and one arrival event. Remaining distance reaches zero at that arrival; one selection departs once, and distance resets to the next leg. No spontaneous second camp or mismatch after three retries.

## Repair 3 — health display must update on healing, not just damage

**Priority: P1, source-backed synchronization defect.** The visible hearts appeared depleted even though the alpha subsequently read full HP. Reproduce with an intentional damage/heal cycle rather than relying on two asynchronous observations.

`HeroUnit.TakeDamage()` forwards alpha changes through `GameDirector.OnAlphaHealthChanged()`. `Combatant.Heal()` raises its own health event, but HeroUnit has no matching Heal override in the inspected code. `PackGameplayHUD` subscribes to the GameDirector health event. This leaves a concrete path for healing to restore the unit without restoring the hearts.

**Do this:** Bind the director to the current alpha's authoritative `OnHealthChanged` event for both damage and healing; unsubscribe on replacement/death. Prefer one health-event route to several manually maintained copies. Also initialize the HUD after starter spawning.

**Pass:** Test 140/140 → damaged → healed → dead. Five hearts reflect the same health ratio throughout, including passive Lulu healing and camp healing. Replacing/restarting the alpha must not leave duplicate or stale subscriptions.

## Repair 4 — make defeat understandable and avoid a false pack-wipe message

**Priority: P1.** GameOver occurred with three allies still alive because Wolvy, the alpha, died. This can be a valid rule, but the player needs to know that rule and have a chance to respond.

Relevant files: `GameDirector.cs`, `HeroUnit.cs`, `PackGameplayHUD.cs` at the paths above.

**Do this:** Decide explicitly whether the five hearts represent alpha health or shared pack health. Recommended short-term choice: keep alpha-loss defeat, call the display `LEADER HEALTH`, make Wolvy visually identifiable, and show a restrained low-health warning. The result should say `WOLVY FELL` with the actual reason, not imply every ally died. Longer term, shared pack lives are a separate design change; do not mix both interpretations.

**Pass:** A player can point out the protected leader before the first difficult encounter. Defeat explains which event caused the run to end. Results include survival time, distance, wave, kills, reward, and Retry. Remaining enemies, coroutines and projectiles cannot keep running meaningful combat behind results. Current 0.4 time scale alone is not a complete end-state policy.

## Repair 5 — stop racing through content before players understand it

**Priority: P1, design finding.** The run had reached the fourth-wave index with 18 kills in under one game minute without deliberate tactical input from this test. A camp and biome change had already appeared. This does not prove the game is too hard; it shows that major beats arrive before there is evidence the player understands them.

**Do this:** Use the first minute to teach one deployment, one clear enemy tell, and why a formation/Moonshift choice helps. Delay major biome/chapter changes until a meaningful milestone. Tune waves by encounter duration and actual player decisions rather than burning through a short authored list. First camp target: approximately 75–100 active seconds, subject to measured travel and combat pacing.

**Pass:** In fresh-player tests, the player deploys intentionally before the first camp, identifies at least one enemy type, and can explain why they used an ability. Measure first death and decisions taken; do not claim a 5–10 minute game merely because the wave loop is infinite.

## Repair 6 — finish the generated combat assets before inventing more enemies

**Priority: P1.** Existing on-disk generated sheets and the partial integration are fully mapped in `AntigravityReadit.md`, section 8. The prior wiring inspection found no `UnitAttackPresentation` instances and null Rudra/Elsa prefab slots. Recheck current state before implementation.

Build these exact outputs:

1. Soldier, Rudra, Elsa clips/controllers/definitions/prefabs under `Assets/New/Enemy/` using the three 6×4 sheets.
2. Seven human/wolf attack clip pairs under `Assets/New/Combat/Animations/`, using 6×2 sheets with **362×362 cells**, not the original 256 grid.
3. Authored `AttackPresentationProfile` assets under `Assets/New/Combat/Data/`, assigned to the unit prefabs.
4. Normal encounter spawns that actually use Soldier/Rudra/Elsa; Elsa shoots `Assets/New/Enemy/Projectiles/ElsaArrow.png`.
5. Correctly timed arrow/orb/star/gear release and impact using the already generated projectile files.

**Pass:** A normal run shows all three enemy types and distinct hero attacks. An arrow causes exactly one impact when it arrives. A dying archer cannot create a new arrow after death. Death playback finishes before despawn. No Editor-only asset lookup is required in the player build.

## Concrete design ideas: implement these as small, testable systems

### A. Introduce enemies through a teach → combine → challenge sequence

- First: a lone Soldier with a visible sword windup.
- Then: an unguarded Elsa with a bow draw and an easy-to-read arrow.
- Then: Rudra alone, with an obvious heavy-slam warning.
- Combine: Rudra guarding Elsa, with a subtle amber tether/guard icon explaining reduced damage.
- Challenge: guarded Elsa plus one flanking Soldier. Leave a viable response window; do not fill every lane with simultaneous warnings.

Each introduction must be visible in the actual battlefield. Do not show a tutorial card while its subject is hidden behind foliage or UI.

### B. Make the first camp offer three clear improvements

Offer one healing option and two build choices, with actual numbers. Example starting choices: `Restore 25% leader health`, `Wolvy cleaves one nearby target for 25% damage`, `Elva's next three deployments start with +15 charge`. These are proposed tuning values, not existing behavior. Avoid permanent free full heals every few seconds. Show the chosen modifier and ensure it resets on Retry.

### C. Give players short goals inside the longer run

Show one compact next goal: `Reach the refuge`, `Survive the archer escort`, or `Defeat Rudra`. Reward the goal with a clear sound and a brief safe beat. At approximately 2–3 minutes, an elite encounter tests the current build. At roughly 5–7 minutes, a larger milestone gives a strong payoff; endless continues after it.

### D. Make roles visible through actions

Wolvy sweeps; Bobo blocks/bash-hits; Elva draws and shoots; Lulu heals a visibly injured ally; Pip darts in with a short combo; Momo chains a star after the first impact; Tilly recoils from a mechanical shot. Preserve readable silhouettes and give each action a sound family. Use a small controlled amount of movement, impact pause and recoil; never shake the entire screen on every hit.

### E. Make poor decisions recoverable

Warn about low leader health before fatal pressure, leave a short deployment opportunity, and provide a sustain option at the next camp. Avoid invisible breaches, attacks during a modal, enemies spawning on the leader, or damage before the visible windup. Difficulty should test choices, not punish missing information.

## Work order and reporting rules

**Do not implement all ideas at once.** Work in this order:

1. Reproduce and resolve live-unit visibility.
2. Fix camp/distance ownership.
3. Fix health synchronization and defeat explanation.
4. Complete the generated combat integration.
5. Remove duplicate rewards and finish complete reset behavior from the main handoff.
6. Tune the first minute, then the first camp, then a full 5–10 minute run.

For each task, write these five lines in `AntigravityProgress.md`: **reproduction, root cause, files changed, test performed, observed result**. If the cause is uncertain, say so and perform the smallest diagnostic that distinguishes the possibilities. Do not mark a task fixed because its code looks plausible. Compile through the official Unity CLI before Play Mode, test the actual behavior, and then continue to the next task.

Keep working until the owner tells you to stop or the agreed acceptance criteria are satisfied. If a genuine external blocker appears, document it and continue independent work. Do not spend the next pass regenerating UI, adding cosmetic systems, or rewriting the project while these basic player-facing problems remain.
