# World of the Pack — reusable portrait card UI

The art is split into independent PNGs and assembled in Unity as editable uGUI prefabs. Shared assets support both portrait iPhone and iPad layouts; device support does not require duplicating the artwork.

## Main deliverables

- `Prefabs/CardPortrait.prefab`: base reusable deployment card, with separate portrait window, frame, editable name, energy track/fill/text, button, repeat icon, and three UI effects.
- Seven `<Hero>_CardPortrait.prefab` variants: same base layout, different data/portraits. Each contains both portrait references and can switch human/wolf without generating a new card.
- `Prefabs/HeroCardHand.prefab`: three responsive card slots, formation control, portrait/transformation request control, and editable footer.
- `Prefabs/PortraitCardCanvas.prefab`: CanvasScaler, GraphicRaycaster, safe-area root, and the responsive hand. An EventSystem with an appropriate input module is required in the host scene; the showcase supplies one.
- `Prefabs/Elements`: fourteen reusable element prefabs, including frame, portrait window, nameplate, energy pieces, buttons, repeat icon, and Charging/Ready/Deployed effects.
- `Art/Portraits`: fourteen 512 × 512 transparent human/wolf bust PNGs covering all seven heroes.
- `Art/Frames` and `Art/Controls`: a shared 9-sliced outer frame, portrait backdrop, eight separately exported control pieces. Button artwork contains no baked labels.
- `Art/VFX`: eighteen separate 128 × 128 frames: six each for Charging, Deployed, and Ready.
- `Data`: seven editable HeroCardDefinition assets, with identity, names, portraits, accent color, and maximum energy.

## Fonts and editable copy

All labels, names, energy values, and button text are real TextMeshPro objects. No font or text is baked into the generated art. The package includes Lilita One, a dynamic TMP font asset, and shared outlined/default material treatments. The font and its SIL Open Font License were downloaded from the official Google Fonts repository; see `Fonts/OFL.txt` and the source link below.

Change a HeroCardDefinition's `displayName`, `shortName`, or portraits to change identity. Change the card's serialized `deployText`, `chargingText`, `pendingText`, and `lockedText` for copy/localization. `SetDisplayName` provides a per-instance override. Font, size, color, and material remain editable on each TMP component. The bundled font primarily targets the supplied Latin labels; add suitable font fallbacks before broader localization.

## Runtime integration

```csharp
card.Bind(heroDefinition, currentEnergy, wolf: true);
card.SetEnergy(newEnergy);       // Does not generate energy automatically.
card.SetForm(false);             // Human portrait; true selects wolf.
card.SetSelected(true);
card.deployRequested.AddListener(hero => {
    // Validate the lane, formation and population limit, then spawn a unit.
    // On success:
    card.ConfirmDeployment();
    // On rejection instead: card.CancelDeployment();
});
```

A full card displays Deploy. Below full energy it displays Charging and cannot be clicked. Clicking a ready card enters a pending state and emits one request. Energy is consumed only when `ConfirmDeployment()` succeeds, which resets it to zero and plays the Deployed effect. Repeated confirmation returns false; a pending second click cannot emit another request. Rejecting with `CancelDeployment()` preserves energy. Game code owns spawn validation, combat, kill rewards, and recovery rules.

Do not rebind a card while a deployment request is pending. Cancel/resolve its request before replacing its definition. For asynchronous spawning, keep the request tied to the original card/hero and ignore callbacks after the card is removed or rebound.

`formationRequested` and `transformationRequested` are integration events on the responsive hand. The production prefab does not change battlefield formations or transform units by itself. The showcase binds those events to visual demos; its form button changes portraits only.

## Layout and input

The CanvasScaler uses a 390 × 844 reference with height matching. The three cards stay centered, with a maximum width on tablets. The deployment button has a 45-unit hit rectangle; toolbar controls use 44-unit hit rectangles. Art frames/corners use 9-slicing, so tablet layout changes do not stretch the painted corners.

`PortraitSafeArea` uses the actual Screen.safeArea in normal operation. Simulated insets are only for review captures. The canvas/hand are bottom anchored; there are no hard-coded iPhone-specific screen positions in the runtime layout. Validate the smallest supported phone and all intended iPad multitasking/orientation policies on real devices before release.

Effects are pooled-in-place UI Images with alpha flipbooks, not scene ParticleSystems or runtime-generated graphics. Charging and Ready loop; Deployed is a short one-shot. Effects never block pointer raycasts. `reduceMotion` and `horizontalDrift` can adjust motion without changing game energy.

## Reviewing and rebuilding

Open `Scenes/CardPortraitShowcase.unity` and press Play. The top selectors choose heroes, the form control switches portrait forms, and +20 Energy demonstrates charging. Deploy confirmations are simulated here; no gameplay units are spawned. A production canvas contains no demo energy generator or selector gallery.

Editor menu: **World of the Pack → Card UI → Build and Verify**. This assembles assets and the showcase, then runs UI state checks and produces iPhone/iPad portrait captures under `Design/CardUI/QA`. **Build Assets** rebuilds without that verification pass. Save your current scene first. Rebuilding refreshes these generated UI prefabs/variants; place game-specific additions on separate wrapper prefabs.

**Verify Current Showcase** reruns the checks without rebuilding the art or prefabs. The showcase temporarily enables background playback so animation timing continues while review tools are focused; it restores the previous setting when the scene is destroyed.

Check `Design/CardUI/QA/unity-ui-audit.json` for the actual verification result. `playModeVerified: true` is required before claiming the automated pass succeeded. Portrait renders and simulated safe-area tests are not physical-device performance or touch QA.

The completed verification passed: all fourteen portrait references and form changes, editable TMP text, blocked charging/locked clicks, one pending request per click sequence, energy preservation on rejection, single energy consumption on confirmation, UI raycast targeting, deployment effect lifetime, and both portrait layouts. Seven Unity render captures are saved alongside the audit. The deployment showcase confirms a visual request only; connect it to the game's spawn service for real reinforcements.

Importable delivery archive: `Exports/WorldOfThePack-CardUI-v01.unitypackage` at the project root. The working assets already live in this project; importing that archive again here is unnecessary.

Image generation prompts and source files are in `Design/CardUI`. Mechanical export script: `Prepare-CardArt.ps1`. The UI art package does not modify the existing character animation sheets.

Font source: https://github.com/google/fonts/tree/main/ofl/lilitaone
