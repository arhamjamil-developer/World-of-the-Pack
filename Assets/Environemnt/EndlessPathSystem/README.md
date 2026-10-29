# Endless Path System — production direction v01

## Goal

Every endless-game environment is built on one shared combat template. A player should feel a new biome immediately, without losing the reliable battlefield width, direction, or team readability established by the Sunny Forest Trail.

The new master references are in [Artwork](Artwork):

- `endless-lane-sunny-forest-master-v01.png` — baseline route.
- `endless-lane-mushroom-grove-master-v01.png` — magical variation.
- `endless-lane-river-meadow-master-v01.png` — water-adjacent variation without a crossing bottleneck.

These are **art masters and visual targets**, not single-image production backgrounds. Final Unity art must be separated into layered tiles and prop prefabs.

## Non-negotiable corridor contract

| Rule | Production value |
| --- | --- |
| Travel direction | Allies enter from the bottom and move north; enemies enter from the top and move south. |
| Lane location | Centered at 50% of the world/background width. |
| Lane width | 42% of the authored canvas, constant from top to bottom and in every biome. |
| Clean combat floor | The center 35% of the canvas has low detail, no blocking props, no dark value pools, and no decor that hides units or VFX. |
| Safe scenery bands | The outer 29% on each side contain trees, rocks, props, water, and foreground dressing. |
| Segment boundaries | Top and bottom rows must return to generic lane/edge foliage so any legal segment joins without a visual seam or a gameplay change. |
| Camera | Elevated overhead/orthographic. No horizon, vanishing point, isometric angle, or scale drift. |

## iPhone and iPad behavior

Author each segment as a wider world, not as two different levels.

```text
| iPad-only scenery | safe side scenery | 42% fixed combat lane | safe side scenery | iPad-only scenery |
|-------------------|-------------------|----------------------|-------------------|-------------------|
                                      iPhone crop
```

- The iPhone camera crops scenery first; it never narrows or rescales the combat lane.
- The iPad camera reveals additional edge scenery at the same pixels-per-unit and keeps the lane center and unit scale unchanged.
- Enemy spawns, ally formations, attack ranges, VFX placement, and card UI must use the fixed gameplay lane coordinates, never the visible screen edge.
- Important interaction props belong inside the safe side scenery bands, never in the iPad-only outer reveal.

## Biome recipe

All environments retain the peach/earth path family and friendly rounded paint treatment. Change edge materials, ambient movement, and distant landmarks — not corridor geometry.

| Set | Core visual language | Edge-only signatures | Not allowed |
| --- | --- | --- | --- |
| Sunny Forest | Warm peach path, leaf green, teal shade, gold daylight. | Daisies, ferns, coral mushrooms, boulders, broken fence. | Heavy canopy over lane, props crossing it. |
| Mushroom Grove | Same path and foliage family, lavender/coral accents. | Rounded mushroom clusters, soft flowers, distant motes. | Dense glow/fog or bright ground marks in the combat floor. |
| River Meadow | Same path, grass green, turquoise water as a side detail. | Reeds, lily pads, side stream, river stones. | A river across the lane or a narrow untested bridge. |
| Emberlane Refuge | Dusty terracotta edges, teal-painted wood, amber windows. | Crates, planters, tents, lamps, packed walkway margins. | Buildings, carts, or crowds blocking combat width. |
| Cloudstep Highlands | Meadow green, lavender stone, sky cyan, sun gold. | Standing stones, cloud edge, rope details outside lane. | Cliffs or bridges pinching the corridor. |

## Required layers per segment

1. `Ground` — tileable corridor soil plus low-detail transition into side ground.
2. `Midground edge` — bushes, grass, water/stone banks, fences; nothing overlaps lane bounds.
3. `Foreground dressing` — occasional large edge leaf or branch, restricted outside the clean lane.
4. `Ambient VFX` — fireflies, loose leaves, water glints; maximum contrast is lower than attack/telegraph VFX.
5. `Landmark` — optional authored side-only story moment. It cannot change lane dimensions.

Keep landmark segments rare: use two or three generic fillers between them. Blend from one biome to another with 2–4 neutral transition segments that preserve the same lane colour family before introducing new edge materials.

## Before art integration

- Slice/paint final assets as tiles and edge props; do not scroll the master PNG directly.
- Test every segment on a narrow iPhone portrait crop and a wide iPad portrait crop.
- Overlay live ally/enemy formations, attack VFX, orange enemy rings, blue ally rings, and the bottom card UI.
- Reject an asset if a combatant, health bar, warning cone, or spawn ring is hidden at either aspect ratio.
- Verify top/bottom joins with random sequence orders: forest → grove → meadow → forest.

