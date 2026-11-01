# Compact task art and fold-away gameplay HUD

## Delivered assets

Built-in imagegen produced two project-bound sprites, saved under `Assets/New/Art/Resources/PackHUD/`:

- `TaskPanelCompact.png`: a clean parchment-center task frame with a slim walnut/gold rim.
- `HUDToggleBanner.png`: a shallow dark-teal banner with a narrow gold/wood border. Top and bottom toggle labels remain editable TMP text.

The importer trims transparent margins into named sprites and adds nine-slice borders. Source PNGs remain intact. The same slim banner artwork is used on the compact encounter panel so its text does not sit across a heavy decorative rim.

The TMP font asset is `Assets/New/Fonts/Simpletix SDF.asset`, using the user's existing `Assets/New/FONT/simpletix/simpletix.otf`. It has a Liberation Sans fallback for unsupported glyphs. So Simple was inspected but not selected: its supplied font omitted visible digits, and its Read Me explicitly restricts it to personal use. This task does not establish commercial licensing for the user's other font files.

## Layout and behavior

- Task panel changed from 420×450 to 390×324 reference units: approximately 33% less area. Copy is shorter, title case is used, and reward/progress remain separate.
- Top and bottom banner buttons independently collapse the status HUD and card hand. Top collapse also hides the encounter-order panel.
- Show controls remain at the safe-area screen edges after collapse; they do not disappear with their panels.
- A compact pause button remains available while the upper HUD is hidden.
- Hidden groups stop intercepting pointer input. Hiding the card hand also cancels a selected-but-unplaced card without spending its charge or charms.
- The interactive tutorial forces required panels open. Starting a new deployment encounter reopens both panels so the player can prepare.
- Header positions use stable baselines; repeated hide/show does not accumulate offsets. Camp status is placed above the encounter-order panel.
- Existing hero-card artwork and definition assignments are preserved.

Editor builder: `HUDSpacePolish.Build()` / task sidebar builder. Scene objects are under `NewPackUICanvas/SafeArea/HUDVisibilityControls`, `TopHUD`, `CardHand`, and `TaskSidebar`.

## Generation prompts

Reference for both images: `Assets/Visuals/RoyalEscort/Resources/RoyalEscort/TutorialPanel.png`, used for visual style only.

Task panel prompt:

> Use case: ui-mockup. Produce one production-ready 2D mobile game TASK PANEL sprite for World of the Pack. Reference is STYLE ONLY: hand-painted warm walnut and antique gold fantasy UI. New design must be much simpler and thinner: a compact horizontal rectangle in a strictly straight-on orthographic UI view, sharp square corners, slender walnut outer rim with restrained gold edging, tiny unobtrusive rivets only at corners, and a large perfectly clean light ivory parchment center for readable dynamic text. 4:3 aspect ratio. Center is opaque flat cream with extremely faint paper texture, no lines or decoration inside. Frame takes at most 5% of width per edge. Minimal decorative weight; fits a small slide-out task journal over a colorful forest game. Entire panel inside frame with generous genuinely transparent margin. NO words, NO text, NO icons, NO characters, NO shadows outside the silhouette, NO perspective tilt, NO other assets. Return saved PNG path.

Toggle banner prompt:

> Use case: ui-mockup. ONE isolated slim horizontal HUD toggle banner sprite for World of the Pack mobile fantasy game. Reference STYLE ONLY: painted walnut and antique-gold forest-game UI. Straight-on orthographic UI view, perfectly rectangular, sharp corners, restrained narrow gold rim, dark desaturated teal center, subtly beveled walnut backing. Very wide and shallow: 5:1 banner silhouette, occupies center of a 3:1 image with generous genuine transparent margin above and below. Minimal visual weight; this is a tiny Show/Hide handle at top or bottom of gameplay. The center must be perfectly clean dark flat teal for dynamic cream text. No large ornaments, no leaves, no handles sticking out, no round gold knobs, no highlights across the text area. NO baked text, NO arrows, NO symbols, NO additional objects. Keep full sprite inside image. Return saved PNG path.

## Story and store copy

`Design/StoryAndStore/Story-and-AppStore-Proposal.md` contains a proposed queen-escort story, current-feature App Store description, and a separate future feature plan. It distinguishes arena features that exist from unimplemented story chapters, blessings, bonds and memories. No store listing was published.

## Verification

See `HUD-Space-Checks.txt`, `Task-Gameplay-Checks.txt`, and `Interactive-Coach-Checks.txt` for individual Editor checks and results. The full-view captures are `HUD-Full-v3.png`, `HUD-Both-Hidden-v3.png`, `HUD-Upper-Hidden-v3.png`, and `HUD-Compact-Task-v3.png`.

Final regression results: 11 HUD visibility/recovery checks passed, the 21-check task/gameplay pass passed with all six compact task texts fitting, and the 12-check guided first-battle pass passed. The task/gameplay pass includes two live waves, reward accounting, invalid placements, pause/resume, deployment timers, a procedural boss formation, queen defeat and retry. Compilation was checked with the official Unity CLI before Play Mode.

Editor verification does not certify physical-device touch, additional device safe areas, release builds, or long-session balance.
