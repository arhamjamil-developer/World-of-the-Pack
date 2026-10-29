# World of the Pack — visual brief v01

![Developer concept board](world-of-the-pack-visual-brief-v01.png)

Source of gameplay proposals: [startreadme.md](../../startreadme.md).

This is generated concept art, not an implemented Unity screen or a set of sliced production assets. The board is 1536 × 1024. Its gameplay inset illustrates the portrait layout; build and check the actual interface at 9:16 with responsive safe areas rather than measuring this inset as an exact specification.

## What developers should take from this image

- A shared pack condition, stamina, pursuit pressure, and shelter progress live above the action.
- The three companions travel automatically from left to right in a side-view scene.
- A patrol is announced before it acts. Players spend stamina on a readable response while the encounter continues.
- Three playable cards are visible: Pounce, Guard, and Mist Step. Their starter costs are 2, 1, and 3.
- Rally Howl is the next card preview. Moonshift's meter is a queued ability status, not a fourth action button. When Moonshift enters the hand, it requires a full meter to play.
- Loud attacks and quiet evasion have different consequences. Guard protects without removing the enemy.
- Shelter provides a safe break, a companion moment, and one upgrade choice.
- The opening has a refuge payoff before the endless journey continues.

## Art refinements before production

Preserve the cold forest, warm shelter, distinct pack silhouettes, large cards, and restrained warning red. Reduce decorative borders if they compete with card labels at phone size.

The soldier in this first illustration faces along the travel direction. In the playable encounter, turn the soldier and its warning cone toward the approaching pack so the attack direction is immediately clear. Keep the rifle attack line, wind-up animation, and timer consistent.

The glowing wolf behind the leader communicates transformation potential. Treat that as concept symbolism; reserve the strongest wolf aura for Moonshift readiness or activation so it does not imply that transformation is always active.

The three review checkmarks on the board are review topics, not evidence that those checks have passed. Treat all three as untested.

## Refinement workflow

1. Build a graybox with one patrol, auto-travel, the starter hand, a visible next-card queue, shared condition, and stamina. Test the decisions before detailed art.
2. Give each new player a brief run without coaching. Record whether they identify the threat, understand the three responses, and explain a failure.
3. Adjust one variable at a time: warning duration, stamina regeneration, card recovery, or pursuit cost. Compare observed behavior before and after.
4. Check the portrait layout on an actual phone. Prioritize readable warnings, large tap targets, safe areas, and a clear view above the cards.
5. Add Moonshift and one shelter upgrade. Check whether players deliberately save resources and whether shelter feels like relief.
6. Expand to the 10–15 minute opening only after the 3–5 minute slice produces understandable choices and voluntary retries.

Use a small review log with columns: observed problem, proposed change, version tested, and result. Avoid treating the visual board or its timing targets as proof of retention.

## First implementation boundary

One forest, three recognizable pack members, five starter abilities, a patrol, an obstacle, a tracking hound, one shelter, transformation, defeat, and restart. Follow the README for mechanics; use this board for visual direction.

Generation method: built-in image generation. The exact generation prompt is saved beside this file as `visual-brief-v01-prompt.txt`.
