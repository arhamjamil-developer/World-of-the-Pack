# Expedition polish — 1 November 2026

Integrated into `Assets/Scenes/SampleScene.unity` via the official Unity CLI and `ExpeditionPolishBuilder.Build()`.

## Gameplay

- Defeated ordinary enemies award 2 Moon Charms; Rudra awards 3, a boss 8. Starting wallet: 8. Wallet cap: 9999. Run-local currency; Retry resets it.
- Card charge is awarded by combat, with camp recharge bonuses multiplying the charge award. Full charge plus sufficient Moon Charms is required. Successful deployment spends only that card's price and charge. Failed/blocked deployment spends neither.
- Prices: Pip 3; Wolvy/Elva 4; Bobo/Tilly 5; Lulu 6; Momo 7. These are tuning defaults, not a finalized economy.
- Moonshift is a deliberate pack command, ready at 50 moon power, with a 10s wolf form and at least 18s toolbar cooldown. Moon power comes from combat/passives; no automatic morale transformation. Night blends in/out over 1.25s, with warm fireflies. Both form timers pause at camp.
- `Data/PackTasks.asset` is an editable looping task catalog. Tasks award charms once, display completion feedback for 2.5s and advance. Retry clears task progress and rewards. No external scheduler/automation is used; these are in-game objectives.
- Tablet side space now uses actual woodland environment artwork: creek and bridge on the left, lantern clearing on the right. Textual side boards are retired. The central portrait battlefield retains the same visibility and controls as iPhone. Side art also transitions into Moonshift night. Device validation still required.

## Presentation

- Ordinary enemy scale 0.42, Rudra 0.50, Captain 0.52, Hound 0.36, boss 0.78. Existing hero scale remains 0.40/0.46.
- Attack sheet PPU corrected to 181 for 362px cells to match the world size of the 256px locomotion cells at 128 PPU.
- New body-only motion adds breathing, stride accents, leaning and attack lunges without moving hitboxes or world health bars.
- New eight-frame MoonSlash is bound to melee attack presentations; existing impacts are restrained and ranged projectiles reduced to readable sizes.
- New art was generated with the built-in imagegen skill/tool, saved in `Art`, sliced/imported and explicitly assigned by the Editor builder. Prompts are saved alongside this document.

## Repairs

Retry clears bosses, pause flags, projectiles, task state, stale card links and route origin. The formation resets before camp measures distance. Camp heal applies 25% once. Camp cancels wave accounting explicitly. Boss targeting includes all opposing combatants. WaveManager owns boss spawning. Defeat freezes simulation and shows run feedback. Steering excludes UI hits and the tablet side helpers.

## Tactical combat and additional art pass

- March: balanced. Guard: -30% incoming damage and -20% normal attack damage. Focus: +20% normal attack damage and +20% incoming damage. The toolbar shows the current formation and explains each change.
- Boss slams have a 1.5–1.65s positional warning, with the rendered ring matching the real damage radius. Avoid the ring or use Guard. Recovery lasts 2.5s and increases damage received by 25%.
- Hits from three distinct hero identities ready a coordinated Pack Strike. It can be commanded once during a boss recovery window. Fighters strike in sequence and Lulu heals the leader. Keeping a varied surviving pack matters.
- New guardian: 16 transparent frames, four each for idle/walk/attack/death; an authored controller, attack profile and correctly bound BossDefinition asset. The old screenshot-cropped boss art is no longer used by its prefab.
- Legacy Hound/Captain encounter slots now resolve to the animated Soldier/Rudra roster. Their old priest/archer screenshot crops are not used in normal encounters.
- New CleanCombatFX atlas is explicitly registered for shared effects and hero ability names. New warning rings contain no baked numbers or terrain. Runtime effects and pickup orbs are cleared on retry. Pickup orbs are visual only, preventing duplicate card-charge rewards.
- Original legacy artwork remains on disk for preservation; cleanup concerns active gameplay references, not deleting unrelated assets.

New generated art: `Art/TabletWoodland.png`, `Art/BrambleGuardian.png`, `Art/CleanCombatFX.png`. Their exact built-in imagegen prompts are in `Art/Environment-and-Combat-Prompts.txt`.

## Authoring

Use **World of the Pack → Polish → Apply Expedition Polish** in Edit Mode on SampleScene, or call the builder through official `unity command script eval`. This updates the existing UI/prefabs and saves the active scene; it does not rebuild the original card UI. Existing unrelated project changes must be preserved.
