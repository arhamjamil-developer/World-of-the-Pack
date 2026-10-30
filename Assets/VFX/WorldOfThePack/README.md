# World of the Pack — Combat VFX v01

This additive art pack contains 210 transparent 256×256 sprites, 40 effect prefabs and definitions, and 34 six-frame animation clips. Existing character animations, card UI, combat logic and approved environments are preserved.

## Contents

| Hero | Four distinct effects | Base playback |
|---|---|---|
| Hero Wolvy | MoonlightSlash, MoonImpact, PawDash, RallyHowl | 12 fps |
| Princess Elva | NatureArrow, LeafImpact, LeafVolley, ForestFocus | 14 fps |
| Fairy Lulu | MoonBubble, BlessingPop, HealingCircle, ProtectiveBubble | 10 fps |
| Captain Bobo | BouncingAcorn, EarthImpact, AcornShockwave, AcornBarrier | 10 fps |
| Scout Pip | WindStrike, QuickImpact, ZoomDash, Whirlwind | 18 fps |
| Wizard Momo | StarProjectile, CosmicPop, StarShower, MoonNova | 12 fps |
| Tinker Tilly | SpringBolt, BrassImpact, BouncingTrap, BubbleDevice | 12 fps |
| Shared | MarchDust, EnemySpawn, EnemyDefeat, CriticalBurst, MoonTransformation, ComboLink | 12 fps |
| Telegraphs | EnemySlamCircle, EnemyConeSouth, EnemyChargeLane, EnemyLandingTarget, DeploymentValid, DeploymentInvalid | Static |

## Unity use

1. Place or pool a prefab from `Prefabs/<Hero>/`. Set its world position to the appropriate attack origin or impact point. Allies attack north (+Y); enemy cone prefab is rotated to face south. Charge chevrons point south.
2. Enable the object to replay from frame zero. The default one-shot hides and deactivates itself after its final frame. `completed` is an optional presentation callback. Avoid applying damage through this callback: gameplay timing should remain authoritative.
3. Use `CombatEffectDefinition.framesPerSecond` to tune presentation to attack events. `Pause`, `Resume`, `Play`, and `ShowFrame` are available. The runtime uses scaled game time so normal game pauses also pause effects.
4. Static telegraphs remain visible until their owner hides them. Animate their scale/opacity and warning duration from the gameplay controller. Their artwork does not implement collision geometry or damage.
5. `Animations/` clips are optional SpriteRenderer alternatives for your own Animator. Do not drive a SpriteRenderer with both an Animator and the flipbook component.

`SoftAlpha` is the default readable blend. `SoftAdditive` is optional for a small bright core, not opaque gadgets or full-screen effects. `GroundWarning` uses reduced opacity. `CharacterFeedback` plus `SpriteFeedbackPlayer` supports per-renderer hit flashes and a local-space dissolve through a reused MaterialPropertyBlock. Shader assets are saved in the project; no runtime shader generation is required. Dissolve is a lightweight pixel breakup, not a replacement for human/wolf body animation.

## Mobile layout and budget

These are world sprites, not UI textures. Use the same assets on iPhone and iPad at 128 PPU; adjust orthographic camera and safe-area UI independently. Keep gameplay inside the canonical 42% corridor. Large specials may extend over scenery, but warning shapes and enemy silhouettes must remain readable. Keep important attack origins above the card hand.

Limit concurrent large translucent effects, pool repeated effects, and prefer normal alpha for legibility. The shader uses one unlit pass, no grab pass, no screen sampling, no dynamic lights and no runtime material allocation in the feedback component. `Art/CombatVFX.spriteatlas` packs the frames with four-pixel padding, rotation disabled and 2048-pixel pages. FullRect quads preserve stable sprite positions but can create overdraw; compression and draw-call tuning still require device profiling. This pack is not a physical-device performance certification.

## Preview and regeneration

Open `Scenes/CombatVFXShowcase.unity`. It is a separate catalogue, deliberately showing all effects together; it is not an intended combat density. Its background-play setting exists only during the preview and is restored afterwards.

Editor menus: **World of the Pack → Combat VFX → Build Assets**, **Verify Showcase**, **Export Package**. Building saves and opens the isolated showcase and refuses to replace an unsaved active scene. Keep the `Fonts` license with the exported preview font. Labels are live TextMeshPro, not baked into effect art.

Source sheets, original prompts, cleanup instructions, mechanical slicing script and QA evidence are under `Design/CombatVFX/`. `generation-manifest-v02.json` selects the cleaned sheets and reviewed row boundaries; the original manifest retains first-pass provenance. The individual sprites have at least 24-pixel padding and preserve relative positions within each animation row. The generated six-frame timing is an art starting point: review directional alignment and cadence at actual deployment scale before shipping. Tall or wide effects may need gameplay-specific world scaling. These effects do not add navigation, combat abilities, transformation state, healing, enemy AI, damage-number code or card-energy rules.
