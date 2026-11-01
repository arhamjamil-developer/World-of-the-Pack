# World of the Pack: review and tactical-adventure proposal

Date: 1 November 2026. Scene: `Assets/Scenes/SampleScene.unity`.

## Decision from the player/designer

The user selected **tactical adventure: choose routes, protect the pack, time abilities**. The mission, route mechanics, and layout below are proposals for discussion. They are not implemented gameplay.

## What was checked

Read `GEMINI.md`, `AntigravityProgress.md`, `AntigravityPlaytest.md`, `AntigravityReadit.md`, gameplay scripts and serialized scene settings. Used the official `unity` CLI with the explicit project path. `unity recompile` reported up-to-date, zero compilation errors and warnings. Started Play Mode, confirmed advancing frame/time values, inspected the actual Editor Game View with the computer-use skill, exercised an in-flight restart and a controlled camp-heal call, then exited Play Mode. No gameplay scripts, scene wiring, or pre-existing changes were edited.

This was a partial Editor review. It was not a completed 5–10-minute fresh-player usability study, an iOS build, or an iPhone/iPad device test. The first observational run did not have camp-arrival event instrumentation, so its precise first-camp timing and intervening interactions are not established. Time.time is session time and does not reset on Retry.

The Pipeline screenshot captured the battlefield without its ScreenSpaceOverlay canvas; the actual Editor window showed the HUD. Do not interpret the camera-only capture as evidence of a missing UI. Local capture: `Temp/DesignReview/initial.png`.

## Submission assessment

Antigravity marks six tasks complete. Several changes are present and useful: four readable starter heroes, serialized Soldier/Rudra/Elsa encounter references, alpha-health event binding, numeric camp options, a leader-specific defeat title, and a bounded endless spawn count. This review does not independently certify every animation, projectile, or long-run claim in that submission.

### Verified failures

| Finding | Evidence | Owning code / next acceptance check |
| --- | --- | --- |
| Old boss survives Retry | After a real boss defeat of the player, RestartGame produced four fresh heroes, zero kills, wave index 0, and a surviving BossUnit. Subsequent sample: GameOver, HP 0, kills 0, enemies 0, bosses 1. | GameDirector.RestartGame destroys HeroUnit and EnemyUnit instances; BossUnit derives directly from Combatant. Clear bosses and their pending attacks; repeat retries after a boss encounter. |
| Waves stay paused after defeat/retry | Post-retry WaveManager.isPausedForCamp remained true. Wave index stayed 0 with no normal enemies. | TriggerGameOver calls PauseForCamp; StartFirstWave / JumpToWave do not clear that flag. Verify a normal wave spawns after retry from camp and from defeat. |
| Retry inherits old route origin | Immediately after retry remaining distance was 30.50147 world units although LegDistance was 16. Later it was 24.10493 at anchor Y 7.155107. | ResetCampState samples the old anchor before RestartGame resets it to Y 0.4. Reset anchor before capturing route origin; assert remaining distance is at most the configured first-leg distance. |
| Rest heals Wolvy twice | Controlled test, combat paused: 65/140 HP became 135/140 after option 0, a 70-point heal (50% max HP). HUD/director also read 135. Label promises 25%. | CampManager.ChooseCampOption calls HealEntirePack(0.25), then heals AlphaLeader another 25%. Apply once and verify 65 becomes 100. |
| Results screen leaves the player without feedback | Actual Game View showed WOLVY FELL, Play Again and a mostly empty wooden board. | PackGameplayHUD sets title and panel state but supplies no run summary here. Show destination progress, cause, one useful suggestion and earned reward. |

### Source-backed risks requiring focused reproduction

- Camp destroys live EnemyUnit objects without the normal OnDefeated callback. WaveManager waits on remainingEnemiesInWave, decremented by that callback. Arriving during an uncleared wave can leave its enemy count stale after departure. Define camp transition as finishing, suspending, or explicitly cancelling the encounter and its accounting.
- StoryCampaignDirector and WaveManager both have boss-spawning paths. Story progress/boss flags are not reset by RestartGame. Give encounter progression one owner and test retries across chapter/boss boundaries.
- FormationController reads screen touches above a bottom-screen cutoff without checking EventSystem UI ownership. Top HUD/modal interactions may also steer/advance the pack. Verify this with real touch input.
- Independent card charging, shared energy/berries, morale-triggered automatic Wolf Instinct, and the Moonshift button coexist. Their meaning and costs need one coherent player-facing explanation.

Console check: the current Unity console reported zero errors/warnings at the sampled moment. Pipeline's retained buffer contained four earlier transport errors dated before this review; those are not new gameplay errors. One exploratory eval referenced a nonexistent property and was corrected; this was an eval compilation failure, not a project compilation failure. The first Play Mode start briefly lost the Pipeline connection during reload; status recovered on the readiness check.

## Why the current game feels directionless

At session time 37.92s the observational run had five kills, wave index 3, and 135.75 leader HP without this reviewer deploying a card. The game advances and fights on its own while the player must infer what their interventions accomplish. Automatic activity is useful between decisions, but it currently carries much of the opening without establishing a destination or a consequential choice.

The question to solve is: **What can I see coming, what can I choose, and how does that choice change the outcome?** More scenery alone will not answer it.

## Proposed first playable mission

**Lead the pack through the forest to Lantern Refuge before Wolvy falls.** Target roughly 4–6 minutes, to be tuned by actual fresh-player runs. Give the mission a finite finish, with optional replay objectives; keep endless as a later mode.

1. **Read and respond (opening).** Show the refuge goal and three journey stops. Introduce one archer with a visible wind-up. Ask the player to move Bobo into a shielding formation. Show a clear blocked-hit result.
2. **Choose a route.** Preview a sheltered path with recovery and a ranged ambush, or an exposed ridge with a tougher patrol and an upgrade reward. Both must be viable; choosing a route changes the next authored encounter, not just the background tint.
3. **Build the pack.** Offer one recruit/upgrade with a readable role and tradeoff. For the first prototype, use existing characters and attacks. Teach one extra card only after the first response is understood.
4. **Prepare at a refuge.** Heal or improve damage/recharge with exact numbers, an indication of current need, and a preview of the next threat. Avoid an always-best option.
5. **Use a timed power deliberately.** Teach Moonshift against a telegraphed dangerous volley or armored push. The player should be able to explain why they used it then.
6. **Reach the destination.** One authored final encounter, explicit mission success, a short useful result, and a visible reason to replay.

Use at least one meaningful response opportunity per encounter, then tune cadence from observation. Do not add a mandatory tap every few seconds merely to increase input count. Tactical pause/slow-time while choosing a route or placing a unit is a candidate for usability testing.

## Rules to settle together

- Keep auto basic attacks and forward travel between encounters; make route, formation and power timing the main player decisions.
- Keep Wolvy as the protected leader for the first prototype, but identify him and teach the loss rule before danger. Discuss an injury/revive alternative after the basic loop works.
- Give each card one understandable job. Candidate: recruit a unique hero once, then use that card for a charged signature command. This would replace the current repeat-deployment interpretation and needs explicit agreement before code changes.
- Prefer one readable power system. Candidate: kills charge a shared Moonshift the player activates. Automatic morale transformation and a separate free cooldown should not both compete for the same fantasy without distinct purposes.
- Resources must buy a decision. Remove or hide an energy display if the final design does not spend that resource. No final economy change is approved yet.

## Environment that supports decisions

First mission stays in one coherent sunlit forest region. Reuse existing art under `Assets/Environemnt` while authoring landmarks and encounter spaces in `Assets/Scenes` / `Assets/Visuals` according to project hierarchy.

| Space | Visual language | Gameplay purpose |
| --- | --- | --- |
| Trailhead | Signpost, distant lantern marker, open ground | Establish destination and leader |
| Archer clearing | Wide readable clearing, lookout at edge, wind-up marker | Teach shield/formation response |
| Fork | Two visible exits with threat and reward previews | Make route selection meaningful |
| Recovery path | Berry bushes, small spring, low cover | Recovery opportunity with an ambush cost |
| Ridge patrol | Stones, fallen log, armored patrol, chest beyond | Higher risk for a concrete upgrade |
| Lantern Refuge | Warm lights and recognizable safe boundary | Pay off the journey and provide respite |

Landmarks communicate actual authored rules. A log must not appear to provide cover unless it does. Keep attack tells and unit silhouettes distinct from bright foliage. Do not swap the entire biome every few waves; use chapter destinations to earn larger changes.

## iPhone and iPad

Portrait iPhone remains the complete gameplay experience. Keep objective/progress at top, threats visible ahead, and thumb-friendly commands below the battlefield. Respect safe areas and avoid interpreting HUD touches as steering.

For landscape iPad, explore a stable central portrait battlefield with side helpers: **left = journey map and next threat; right = pack health/roles and a contextual tip**. Both sides explain information available on iPhone, where it can collapse into a compact drawer or contextual hint. Do not give iPad extra battlefield visibility or exclusive tactical powers. Portrait iPad should adapt helper placement to actual available width.

`Assets/New/Ipad side` did not exist during discovery; no existing iPad-named assets were found. A new concept folder is supplied for discussion, not production integration. The landscape concept is illustrative and does not establish real device scaling, safe areas, hit targets, performance, or final mechanics.

## Smallest useful implementation after the design discussion

1. Repair reset/camp accounting and verify three retries through defeat, boss, and camp states.
2. Build one finite forest mission with the existing roster and enemies.
3. Add one shieldable archer encounter, one real route fork, and one deliberate Moonshift lesson.
4. Observe three new players: can they name the objective, identify Wolvy, predict the threat, and explain one successful decision?
5. Verify the same decisions and information on iPhone and iPad before expanding chapters or producing a large environment set.

Success measure: the player says what they are trying to do, makes a choice for a reason, sees its consequence, and wants another attempt.
