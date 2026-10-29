# Three game design boards

These horizontal boards extend the approved top-down cartoon art direction. They are proposed screen flows, not implemented Unity UI, final character assets, or validated game balance.

## 01 — Main menu and game flow

[Open board](../../Assets/Visuals/board-01-main-menu-game-flow.png)

Home offers Play, My Pack, and Options. Play opens Story Levels or Endless. Both use the same top-down squad combat and repeatable summon cards. Pausing freezes the encounter; Resume returns to it. Options controls music, sound, and vibration. Results follows a finished run; it is a separate state from Pause, even though the compact board places the navigation strip below the pause panel.

My Pack is a future roster entry point. The requested roster now has seven leads with paired human/wolf concept designs; see `seven-heroes-roster.md`. These earlier boards preserve the three provisional role designs.

## 02 — Story level journey

[Open board](../../Assets/Visuals/board-02-story-level-journey.png)

Level map → selected mission → pack preparation → battle → refuge/completion → next level or map. The current proposal introduces short authored objectives and terrain changes while keeping card controls familiar. Completed stages unlock the next route node. Failure should offer Retry and Level Map.

Preparation uses role previews rather than functional summon buttons. The board reuses battle-style card widgets there to communicate the art direction; implement spawning only during active battle. Guardian, Ranger, and Helper are provisional role labels; see the newer seven-character roster brief.

Choose a refuge reward before proceeding. Its exact scope needs a decision: in this proposal it lasts through the current chapter journey and resets on starting a new journey. Checkpoint saving must preserve that journey state. Avoid describing rewards as permanent until that system is designed.

## 03 — Endless adventure

[Open board](../../Assets/Visuals/board-03-endless-mode-loop.png)

Start with an allied group → defeat approaching patrols → earn per-card energy → spawn more copies → survive waves → take shelter upgrades → continue → see results → retry or return home.

Proposed shelter example: every ten waves. This is a tuning hypothesis. Rest & Save stores the current run; Continue resumes it. Starting a fresh run clears run-only upgrades and meters, while keeping the saved personal best. Reaching zero shared pack condition ends the run.

## Shared gameplay rules

- Portrait viewport; overhead battlefield; allies advance from bottom to top and enemies approach from above.
- Defeating soldiers charges each card independently. Cards below full energy stay disabled and read Charging.
- A full card spawns another unit and resets only its own meter. The card remains reusable.
- Start with enough allies to earn the first kill. Establish a population cap and a clear recovery rule before balancing endless waves.
- Friendly cartoon silhouettes, blue ally rings, orange enemy rings, readable health/energy, and playful defeat effects remain consistent across environments.
- Update active-unit counts when units activate or leave the field. Derive all displayed counters from game state.

## Recommended build and refinement order

1. Build the overhead battle with starter allies, one enemy type, three summon cards, independent energy, and repeat spawning. Verify direction, spawn placement, and count updates.
2. Add pause, defeat, and restart. Check that the player can explain why a card is disabled and why a run ended.
3. Add the main menu and mode selector. Reuse battle code for both modes; change objectives and wave rules through data.
4. Build one Story stage and one refuge reward. Then build a short Endless run with one shelter. Test pacing before producing many levels.
5. Add checkpoint saves and settings. Check transitions and restore behavior, including leaving a paused run.
6. Refine the seven proposed lead characters and their matching human/wolf concepts in this established style, then produce consistent sheets and verify them at gameplay size.

Tune energy reward, energy threshold, population cap, enemy health, spawn recovery, shelter cadence, and upgrade values through configuration rather than baking them into art. Test one change at a time. Terrain variation should preserve readable team colors, path direction, and card accessibility.

Generation method: built-in image generation with the approved gameplay image as reference. Exact prompts and the targeted energy-button correction are saved in `three-game-boards-prompts.md`.
