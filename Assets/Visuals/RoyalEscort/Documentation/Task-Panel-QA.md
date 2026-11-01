# Task sidebar and gameplay QA — 1 November 2026

The full-width task banner was replaced by an editable left-side task journal in SampleScene. Its default presentation is a narrow TASK handle with a small progress count and a quiet unread dot. Tapping opens a wood-and-gold frame with an opaque ivory reading inset. The objective, instruction, progress, reward and tactical tip use a readable Liberation Sans font and separate lines. All six catalog tasks fit, including the two-line BREAK THE PATROL title. The drawer closes on combat start, pause, camps, tutorials and defeat; it never opens automatically to announce a task.

The scene objects live under `NewPackUICanvas/SafeArea/TaskSidebar`. The `PackTaskDrawer` component on NewPackUICanvas exposes the editable references. Existing task catalog and card artwork/definition references are preserved. The old task labels remain bound but their old banner presentation is disabled. The builder menu is **World of the Pack → Royal Escort → Polish Task Sidebar**; it only rebuilds this sidebar.

No new generated image was needed: the panel reuses the established nine-sliced royal wood/gold artwork with a clean ivory UI inset. The text remains editable TMP text.

## Bugs found and fixed

- During deployment, shared Wolf Instinct time paused at 10 seconds while the hero's surge timer dropped to approximately 7.5. Hero cooldowns, passive effects and surge time now pause during deployment, and the HUD cooldown pauses with the shared timer. A repeat observation kept both timers at 10 seconds.
- BREAK THE PATROL required 70.28 pixels of title height in a 66-pixel title box. Its title area now has 80 pixels, with the instruction, progress and footer repositioned below it.
- The survival-task instruction still referred to keeping Wolvy alive. It now refers to the queen, matching the actual loss condition.

## Verification

Official Unity CLI compilation passed with zero errors/warnings before Play Mode. `TaskGameplayValidation.Run()` passed all 21 checks; the individual results are in `Task-Gameplay-Checks.txt`:

- Initial/retry one-leader setup and collapsed task handle.
- Drawer opening and all six task layouts without text overflow.
- First-wave ranged deployment and survival; the incomplete three-kill task pays nothing early.
- Partly charged cards cannot be selected; occupied-cell rejection preserves currency and charge.
- A real sword-and-bow wave with tank and healer support clears.
- Kill and recruitment tasks each grant exactly one reward, with six total task charms after two tasks.
- Pause freezes gameplay and closes the drawer.
- Shared buff, hero surge and HUD cooldown remain synchronized during deployment.
- Wave ten stages nine enemies at nine distinct positions plus a boss.
- Queen death clears all heart icons, hides the drawer and enters Game Over.
- Retry resets queen health, starting currency, pack and task progression.

Active defeat-screen TMP text was also checked: no overflow was reported. Runtime console after the gameplay pass reported zero errors and zero warnings.

The existing 12-check interactive first-wave tutorial validation also passed with the sidebar installed. The sidebar stayed closed during the guided lesson; the final runtime console still reported zero errors/warnings.

Previews: `Task-Handle-Collapsed.png`, `Task-Drawer-Open.png`, and `Task-Drawer-Mixed-Wave.png`.

These are Editor Play Mode checks. Device touch, iOS builds, safe areas across additional devices, and long-session balance are not certified by this pass. The pre-test tutorial preference is restored after QA and the Editor is left out of Play Mode.
