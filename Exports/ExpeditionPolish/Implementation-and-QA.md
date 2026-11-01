# World of the Pack — integrated polish and verification

1 November 2026. Active scene: `Assets/Scenes/SampleScene.unity`.

## Implemented

- Actual generated creek/bridge and lantern-grove environment imagery fills tablet side space. The old side boards and text are removed from the authored layout. Central gameplay retains its portrait viewport. Sides and terrain transition to Moonshift night with warm fireflies.
- A new 16-frame guardian sheet replaces the cropped screenshot boss. Idle, walk, attack and death clips are bound to an authored AnimatorController. The inherited 2.1× body-child scale was reset; the resulting rendered cell bounds are approximately 1.29 world units.
- New melee slash and four-family combat-effects atlases are imported and wired. Clean effect prefabs are registered in the scene for shared effects and hero ability names. Warning rings have no baked text, numbers or ground. Legacy priest/archer screenshot crops no longer serve as Hound/Captain encounter visuals.
- Movement accents, attack wind-up/lunge, restrained projectiles/impacts, smaller floating numbers, a separate notification line and explicit card price badges improve readability.
- Defeats award Moon Charms and card charge. Card deployment spends its displayed price and only its own charge. Pickup orbs are visual feedback and no longer award charge a second time.
- An editable looping task catalog gives the pack short-term goals and one-time rewards per completed task.
- March is balanced; Guard reduces incoming damage by 30% and normal attack by 20%; Focus raises normal attack and incoming damage by 20%.
- The guardian telegraphs the actual slam radius, then exposes itself for 2.5s. Three distinct hero contributors ready Pack Strike, usable once in that recovery window. Fighters strike in sequence; Lulu supports with healing. The exposed boss takes 25% more damage.
- Retry clears bosses, projectiles, telegraphs, pickup orbs, pooled effects, task state and stale card links. Camp distance starts from the reset formation anchor. Healing applies once. Camera reset forces a cut to the new pack position. Chapter progression has one owner.
- A small terrain shader blends matching top/bottom edge samples to soften visible repetition boundaries in existing biome artwork.

## Verified in the Editor

| Check | Observed result |
| --- | --- |
| Final `unity recompile` | Up to date / no compile errors or warnings |
| Terrain shader | `ShaderUtil.ShaderHasError` false |
| Game View progression | Frame/time counters advanced in Play Mode |
| Landscape tablet | 1536×1024 capture; environment imagery present, side boards absent |
| Portrait | 1080×1920 Game View observed; same wallet, task board and controls |
| Card transaction | Pip: wallet 10 → 7, own charge → 0; repeating the uncharged action spent nothing |
| Combat reward | One actual Soldier defeat: wallet 8 → 10; Elva charge 60 → 85 |
| Duplicate reward regression | Elva remained at 85 after pickup orbs arrived |
| Camp heal | 61.75 → 96.75 of 140 HP: exactly 35 restored; repeated choice outside camp did not add a buff |
| Formation damage | Identical 10-point hit on Bobo: March 10, Guard 7, Focus 12 |
| Pack Strike | UI button activated during a naturally reached boss recovery window; could not repeat in that window |
| Combo observation | Boss 232.9 → 13.4 HP across the one-second strike sample; this includes ongoing combat, not isolated skill-only damage |
| Three boss/defeat retries | 3/3 restored four heroes, 140 leader HP, 8 charms, first-camp distance 16 at reset and no surviving boss |
| Transient cleanup | After the next frames: zero old bosses, telegraphs and pickup orbs; route countdown then resumed normally |
| Active-art scan | No active SpriteRenderer referenced the old `Assets/Characters/Enemies` screenshot crops, `Telegraph_Orange…`, or `VFX_Golden…` assets in the sampled scene |
| Console | Current sampled Editor console: zero runtime errors/warnings; retained Pipeline transport history predates the tests |

Evidence: `Cleanup-QA.txt`, `iPad-Guardian-Day.png`, `iPad-Moonshift-Night.png`; additional local captures and the Pack Strike sample log are under `Temp/DesignReview`.

## Limits and follow-up

These are targeted Editor checks and controlled gameplay scenarios, not a full fresh-player usability study or iOS-device/build validation. Some tests used direct enemy damage, generated encounter setup, added moon power or button UnityEvents. The coordinated strike test waited for the boss's natural exposure window. Balance values remain tuning defaults.

Boss testing exercised the normal save/reward path and advanced the local Editor test-save progression. No prior save snapshot was available, and no destructive save reset or guessed rollback was performed.

Follow-up implementation adds three tactical mechanics: six-second focus targeting, a Spring/Hunt route decision after every fourth-wave cycle's second encounter, and optional packmate rescues after its third encounter. Spring heals 20% max health; Hunt adds two foes and 50% charm rewards for two encounters. Rescue costs four charms, needs room in the pack, and preserves the rescued hero card's charge. Choices pause combat/marching and reject duplicate input. Generated route props and a three-stage opening cage are authored in the scene. These are encounter choices along the existing trail; spatially branching map lanes remain a possible later design extension.

Generated art and exact prompts: `Assets/New/ExpeditionPolish/Art`. All images used the built-in imagegen tool and were copied into the project. Original unrelated assets and dirty changes were preserved. Legacy artwork remains on disk where it is no longer referenced by the active gameplay paths.

## Shared rectangular card hand follow-up

- Generated `RectangularCardHolder.png` supplies straight wooden edges and square gold corner plates. Sprite import trims transparent margins and provides a 112px nine-slice border. Source bitmap remains intact.
- `Assets/UI/WorldOfThePack/Prefabs/CardHandHolder.prefab` contains the editable **Wolves vs Monster** title, centered toolbar labels, page controls, and nested hero variants. The scene uses its connected instance.
- All seven `Assets/New/Prefabs/Card_*.prefab` assets are variants of `Assets/UI/WorldOfThePack/Prefabs/HeroDeploymentCardBase.prefab`. Edit the base's **CardFace** dimensions and TMP styles to change every card. Normalized child anchors and CardFaceLayoutSizer preserve inheritance while HorizontalLayoutGroup lays out the strip. Hero identity/portrait/role are variant overrides.
- The strip fits three cards inside its viewport and pages across all seven. A solid white sprite makes the filled charge bars correctly show fractional charge. Phone CanvasScaler fits width to prevent clipping on tall aspect ratios; wider tablets retain side environment imagery.
- Removed the unused 140-transform old portrait canvas, seven duplicate Moonshift controls, fake berry track, static milestone dots, redundant gameplay logo, excess camp options, and unreferenced old full-HUD prefab. Native dependency audit removed 20 unused art files; recovery copies and manifest are in this export folder. Assets still referenced by other scenes/libraries were preserved.
- Shared-card inheritance test changed base font size to 31 and CardFace size to 300x340: all seven variants inherited both; original values restored. EventSystem clicks successfully deployed a nested card and selected page five. See `Shared-Card-QA.txt`.
- Controlled Play Mode checks passed Spring's exact heal, duplicate rejection, Hunt duration/multiplier and actual wave-routine enemy count, rescue spending/recruitment/charge preservation, repeated-rescue guard, unaffordable rescue/free Continue, and reset cleanup. See `Trail-Mechanics-QA.txt`. Focus targeting also passed a Physics2D/EventSystem click and ranged-hero target selection check. These controlled checks do not replace a full natural run or difficulty balancing.
- Final Game View captures: `Rectangular-Phone.png` at 1170x2532 and `Rectangular-iPad.png` at 1536x2048. Device hardware, iOS builds and every notch/safe-area shape remain untested. Sampled console had zero errors/warnings. New mechanics test-save snapshot was restored after testing; earlier boss-test progression noted above is preserved.
