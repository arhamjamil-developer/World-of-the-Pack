# Card UI delivery and evidence

Working assets: `Assets/UI/WorldOfThePack`.

- 42 separate artist-generated/assembled PNGs: 14 portraits, 8 controls, 2 frame/backdrop pieces, 18 VFX frames.
- 24 prefab assets: base card, 7 inherited hero variants, responsive hand, safe-area canvas, 14 reusable UI elements.
- 7 HeroCardDefinition assets.
- Editable TextMeshPro labels using Lilita One, downloaded with its original license from Google Fonts.
- Isolated CardPortraitShowcase scene and integration components.

`generation-manifest.json` records exact image prompts and source paths. `generation-prompts.md` is the readable prompt set. `Sources` preserves source atlases. `Prepare-CardArt.ps1` performs mechanical cropping and layout export; it does not draw faces, text, or decorative UI art.

`QA/art-audit.json` lists every exported PNG's actual dimensions and sampled transparency. `QA/unity-ui-audit.json` records the completed Unity build and Play Mode pass. Seven PNG captures show iPhone and iPad portrait layouts, human/wolf examples, and confirmed deployment resetting the card to Charging.

Initial font-resource initialization and preview-background-timing issues were corrected before the final verification. The final audit reports zero captured runtime errors and `playModeVerified: true`. A clean final compilation check is separate from older Console history.

The device renders use 1170 × 2532 and 1536 × 2048 with simulated safe-area insets. These are editor validation, not physical iOS device QA, game combat integration, or a performance benchmark.

The importable Unity package is `Exports/WorldOfThePack-CardUI-v01.unitypackage`. See the asset-folder README for binding, deployment confirmation/cancellation, portrait switching, and rebuild instructions.
