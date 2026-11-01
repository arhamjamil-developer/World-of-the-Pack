# Royal escort arena conversion — 1 November 2026

## Existing project review

Reviewed the active `Assets/Scenes/SampleScene.unity`, GameDirector, WaveManager, FormationController, CinemachineCombatDirector, HeroUnit, EnemyUnit, BossUnit, CardDeckManager, PackGameplayHUD, trail choices, tasks and campaign progression. Inspected the actual CaptainBobo wolf animation sheet, CampPanel art and existing gameplay UI before producing the integrated artwork.

The existing art uses north-facing chibi characters, dark outlines, amber fur, yellow capes, painted detail and wood/gold UI. The new carried throne uses that exact wolf sheet as its generation reference. Existing combat attack profiles, animation controllers, projectiles and impact VFX are retained. Enemy target selection now uses the living combatant registry and includes the queen; bosses also target her with basic attacks.

## Implemented behavior

- GameDirector installs ArenaDirector and TutorialManager automatically. No extra scene setup is required for SampleScene.
- The queen is an allied Combatant with 240 health, carried by two wolves in an eight-frame north-facing sprite sheet: four idle frames and four walk frames. Her health drives the hearts. Wolvy remains a fighter and can fall without ending the run.
- A short march leads to a five-lane, four-rank deployment grid. Every enemy is staged before BEGIN BATTLE, on a separate five-lane, three-rank red grid. Lanes are randomized without duplicate slots; swords prefer the front, heavy guards the middle and bows the back. Bosses reserve a wider area. Blue ally tiles show open slots, green tiles show occupied slots, and pulsing tiles show recommended ranks or the tutorial target. The preview gives enemy-specific tactical advice.
- Existing defenders are placed on grid cells. Drag deployment snaps to a lane and rank; an occupied cell rejects the request without consuming charge or charms. Tapping a card or its SELECT button chooses it without spending anything. A second tap on an open ally tile deploys it. Drag placement remains supported. Selection shows charge, cost and a role-specific tactical hint for every hero. Unit home positions remain fixed during battle; fighters can chase and attack, then return to their assigned position.
- Initial currency is 24 charms and all seven cards begin charged. Every successful deployment pays its existing hero cost and resets that card's charge. Defeats recharge cards; waiting at deployment does not recharge them or advance the survival timer.
- Regular kill rewards are two charms in the first three waves and one afterward; boss kills award six. Wave completion grants `max(1, 5 - waveNumber / 3)` additional charms. Existing optional pack-task rewards continue.
- Enemies attack surviving defenders, then the queen. They no longer vanish at the breach line in arena mode. Queen death ends the run, pauses time, and uses queen-specific defeat text. The opening and restart have only Wolvy above the queen, with 24 charms and the existing card roster ready for recruitment. The two throne carriers belong to the queen artwork and are not extra battle units.
- Endless waves retain the existing mixed Soldier/Elsa/Rudra composition, with population capped at ten regular enemies. After wave three, health increases by 12% of base per wave and attack by 8%. Every fifth wave adds a boss. Subsequent bosses scale by 10% base health and 6% base damage per wave after wave five.
- Existing campaign chapter advancement and post-wave route-choice popups do not interrupt the arena loop. Camps can still be encountered while marching.
- Camera framing stays relative to the escort so a distant incoming squad cannot pull the queen behind the card hand.

## Tutorial

PlayerPrefs key: `WorldOfThePack.RoyalEscort.Tutorial.v2`. This version intentionally replays the new interactive lesson for users who finished the earlier text tutorial.

The first-run coach is action driven. A smaller callout stays above the action, while a dimmed spotlight, pulsing frame and animated finger demonstrate the current target. Its input hole permits only the demonstrated control. The tutorial uses the existing Inspector-assigned Elva card; CardDeckManager now preserves existing card definitions when initializing the hand.

1. Point at the queen and introduce Wolvy as her only starting defender.
2. Reveal the enemy formation and identify enemy health bars.
3. Highlight Elva's card and explain charge and charm price.
4. Wait for an actual card tap. It selects the card without recruiting.
5. Animate the finger from card to a glowing rear tile. Only that tile accepts the lesson's deployment, behind Wolvy.
6. Highlight the spent card's charge bar and explain separate charm and charge costs.
7. Move the callout aside and point at the real BEGIN BATTLE button.
8. Let the player watch the fight. After a defeat, point out the recharged card bar. Surviving wave one opens the completion lesson.

Finishing or skipping saves completion. Skip also immediately releases the battle button. Explanations do not hand out hidden combat immunity or refill cards. Restarting an unfinished lesson returns to the queen introduction. Existing card artwork, definition assets and prefab Inspector references are not rewritten by the coach; highlights are separate runtime UI.

Melee approach ranges were tightened to 0.76 world units for regular fighters, with opposing north/south approach positions, lane-biased target selection, shared crowd separation and soft contact shadows. Ranged kit ranges and the existing attack animations/projectiles are retained. The queen's redundant world health bar is hidden so it cannot overlap the rear deployment tiles; her hearts stay visible in the HUD.

Replay/reset command: Unity menu **World of the Pack → Royal Escort → Reset First Time Tutorial**. It deletes only the tutorial key.

## Art and import

Built-in imagegen was used. Project assets are in `Assets/Visuals/RoyalEscort/Resources/RoyalEscort/`:

- `QueenEscort.png`: 4 columns × 2 rows. `RoyalEscortSetup.ImportArt()` slices eight named sprites, 230 pixels per unit, no compression/mipmaps. Runtime uses frames 0–3 for idle and 4–7 for march.
- `TutorialPanel.png`: generated sharp rectangular wood/gold panel; Unity extracts its rectangular rim and center and applies 70-pixel nine-slice borders. The same blank artwork supplies the tutorial, deployment orders and rectangular Skip/Next/Begin Battle button backgrounds; text remains editable TMP text.

Importer menu: **World of the Pack → Royal Escort → Import Art**. Runtime resources ship in a player build and do not depend on Editor-only image lookups.

### Generation prompts

Queen reference: `Assets/Characters/CaptainBobo/Wolf/CaptainBobo_Wolf_North.png`.

> Use case stylized-concept. Reference is existing game's wolf sprite sheet. Match EXACT chibi proportions, crisp dark outlines, painted amber/brown fur, yellow capes, leather medieval costume and north-facing top-down 2D game perspective. Create a new animation sprite sheet of a queen carried on a king's wooden throne by TWO wolf pack members, one on each side holding the throne poles. Queen has pale fur, tiny crown and blue-gold cloak, seated between carriers. Both carriers match reference wolves but carry poles instead of shields. EXACT layout: 4 columns and 2 rows, 8 evenly spaced equal square cells on a transparent background, each cell contains entire trio and throne, centered, identical scale and baseline. Top row 4 subtle IDLE poses, bottom row 4 WALK poses with alternating feet, poles steady, queen seated, all facing NORTH / away from camera like reference. No text labels, no shadows on ground, no scenery, no grid lines, no cropping. Clear gaps between cells, readable at small size. Return saved PNG path.

Panel reference: `Assets/New/Art/CampPanel.png`.

> Use case ui-mockup. Create ONE production UI panel sprite for the reference fantasy wolf mobile game. Match reference hand-painted dark walnut wood and bright antique gold bevel borders. Wide rectangle with EXACT sharp square 90 degree corners, no rounded corners. Plain very dark walnut center with lots of empty area for tutorial text over it. Thin elegant gold double-line rim, restrained corner rivets. Entire rectangular panel visible with transparent margin. No lettering, no buttons, no symbols, no decoration protruding into center, no background. 3:2 rectangular image, opaque panel center with transparent outside.

The generated panel had a visible outer margin; Unity sprite slicing excludes it. Generated character frames contain small pose/shape differences typical of generated animation; inspect at target-device size before release.

## Validation

Used the official Unity CLI through com.unity.pipeline. C# compilation was verified before entering Play Mode.

Observed in Editor Play Mode:

- Eight escort sprites loaded; queen starts at 240 health.
- First-run tutorial pauses time; returning player with saved key skips it.
- Successful guided Elva deployment: 24 → 20 charms, 100 → 0 charge, exactly one extra ally.
- Occupied-cell request: charms remain 24 and charge remains 100.
- Staged enemy positions remain identical across separate runtime observations during preparation.
- Wave one completes normally with two kills and queen health 240; completion lesson appears.
- Finishing the lesson writes PlayerPrefs value 1.
- Wolvy's death leaves the game Playing.
- Removing defenders and exposing the queen to a Soldier produces damage and eventually health 0 / GameOver with the queen defeat reason.
- Restart restores queen health 240, one leader, 24 charms and Playing state.
- Wave five starts with one boss and three regular escorts.
- Procedural wave ten opens its deployment phase, starts battle and spawns scaled enemies and a scaled boss (observed boss health 975, Soldier health 147.2).
- Current portrait captures: `Coach-01-Queen.png`, `Coach-02-Select-Card.png`, `Coach-03-Place-Unit.png`, and `Coach-04-Combat.png`. Earlier screenshots are retained as historical captures.
- Final HUD check: queen damage from 240 to 140 updates the display to three full hearts and two empty hearts. Final clean Play Mode console reports zero errors and zero warnings; final compilation reports zero errors and zero warnings.
- Editor was left out of Play Mode with the tutorial preference reset for the user's next first-run playtest.

These are Editor checks, not device/build certification. Long-session balance, iPad landscape safe areas, iOS build/signing and device touch interaction still need playtesting. No claim is made that endless difficulty is fully balanced from the short runtime checks.

Pre-existing scene, prefab, animation, art deletions and other dirty worktree changes were preserved. No commit or push was performed.

## Interactive coach validation

`InteractiveCoachValidation.Run()` is an Editor-only repeatable check that uses actual UI raycasts, pointer handlers and the same grid-placement method used by touch input. It checks the one-leader opening, staged formation, input spotlight blocking the wrong card and tile, card selection without spending, exactly one deployment with the correct cost, charge acknowledgement, the real battle button, wave-one survival, card recharging and PlayerPrefs completion. Results are written to `Interactive-Coach-Checks.txt`. In the observed passing run, the queen retained 240/240 health and Wolvy retained 110/140 health. This is a short first-wave check, not long-session difficulty or device-touch certification.

Additional checks: Skip during guided selection persisted completion, re-enabled BEGIN BATTLE, and entered Battle. A staged wave-ten preview contained nine regular enemies in nine distinct slots plus one boss, with 35 total grid tiles. The preparation header now updates the main wave number before combat and suppresses the battle-only Pack Strike button so it cannot overlap the enemy preview. The selected card's role hints and front/rear placement guidance are generated without modifying its Inspector artwork or definition references.
