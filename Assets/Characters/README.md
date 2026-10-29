# Character sprite package — north-facing v01

Seven character folders contain separate Human, Wolf, VFX, Animations, and Prefabs assets. Artwork follows the approved friendly cartoon roster.

## Camera and sizing

- Portrait iPhone and iPad presentation. Allies face north/up the screen toward enemies; the player mainly sees their backs from an elevated camera. Do not rotate the screen or mirror the sprites to make them face north.
- One rear-facing direction is included. South/east/west sets are not included.
- All sprite cells are **256 × 256 pixels**, with **128 pixels per Unity unit**. Human and wolf idle silhouettes are normalized to approximately 160 pixels tall using the same canvas, without making the wolf form larger.
- Character pivot is `(0.5, 0.1875)` in Unity coordinates: the ground anchor is 48 pixels from the bottom. Effects use a centered pivot.
- PNG sheets have real alpha transparency. Original image-generation sources and prompts are outside runtime Assets in `Design/CharacterPipeline`.

## Included animations

Every row contains six animation frames, read left to right. Rows start at the top.

| Form | Rows | Sheet size | Animation order |
| --- | --- | --- | --- |
| Human | 4 | 1536 × 1024 | Idle, Walk, Attack, Dead |
| Wolf | 6 | 1536 × 1536 | Idle, Walk, Attack, Dead, Special01, Special02 |
| VFX | 3 | 1536 × 768 | Attack, Special01, Special02 |

The package totals **546 sliced sprites**, **70 character clips**, **21 standalone effect clips**, and **14 form prefabs** after a successful Unity build. Idle and Walk loop. Dead holds its final gentle knockdown pose. Attack and specials are one-shots; return the controller to Idle when the gameplay action completes.

## Playing a prefab

Each generated prefab has a Body SpriteRenderer and Animator. Wolf prefabs also have a separate VFX SpriteRenderer, animated alongside attacks and specials. Effects are visual only: they do not apply damage, healing, status effects, or movement.

Set the Animator integer parameter `Action`:

| Value | State |
| --- | --- |
| 0 | Idle |
| 1 | Walk |
| 2 | Attack |
| 3 | Dead |
| 4 | Special01 — wolf only |
| 5 | Special02 — wolf only |

To replay the same one-shot immediately, explicitly restart its state with `Animator.Play("Attack", 0, 0f)` or the corresponding special state. Changing the integer to the same value does not restart an active state. No movement or combat logic is attached to these art prefabs.

Wolf attack VFX begins two body-frame intervals after the action starts, then clears after its six effect frames. Its child offset is an initial visual placement to tune against actual enemy spacing. Projectile travel and effect pooling remain gameplay integration work.

## Specials by character

| Character | Special01 | Special02 |
| --- | --- | --- |
| Hero Wolvy | Paw Dash | Rally Howl |
| Princess Elva | Leaf Volley | Forest Focus |
| Fairy Lulu | Moon Hug | Bubble Blessing |
| Captain Bobo | Acorn Slam | Big Brave Guard |
| Scout Pip | Zoom Dash | Whirl Paw |
| Wizard Momo | Star Sprinkle | Moon Pop |
| Tinker Tilly | Spring Surprise | Bubble Trap |

These names describe the art gestures and effects, not implemented gameplay powers.

## Preview and rebuilding

Open `_Preview/CharacterAnimationPreview.unity` and enter Play Mode to cycle through the actions. The preview component can stop cycling and select one Action for inspection. It is an isolated art scene and is not added to the game's build scenes.

Unity menu: **World of the Pack → Character Art → Build and Verify**. This imports and slices the prepared sheets, builds clips/controllers/prefabs, creates the preview scene, renders portrait evidence, and exercises the animation states in Play Mode. It also configures portrait orientation and the iPhone-and-iPad target. Use the saved project state before running from an interactive editor: this menu opens the preview scene.

The lower-level **Rebuild Assets** command performs the asset build and captures without the Play Mode pass. Rebuilding regenerates the supplied art prefabs/controllers; put gameplay customizations on separate wrapper prefabs.

See `Design/CharacterPipeline/QA/unity-audit.json` and the Unity build log for actual verification results. Portrait preview renders verify composition at two aspect ratios; they do not prove physical-device performance, touch controls, safe-area UI, or final animation polish. Review the generated six-frame motion, costume continuity, and VFX edges in the actual combat scene before treating this first pass as final production art.
