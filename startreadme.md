# World of the Pack

Initial game concept and design starting point.

## Current direction — 30 October 2026

The latest gameplay reference is [top-down cartoon gameplay v02](Assets/Visuals/pack-topdown-cartoon-gameplay-v02.png).

Three matching horizontal design boards now explore [main menu and game flow](Assets/Visuals/board-01-main-menu-game-flow.png), [Story levels](Assets/Visuals/board-02-story-level-journey.png), and [Endless mode](Assets/Visuals/board-03-endless-mode-loop.png). See the [developer flow and refinement guide](Design/Concepts/three-game-boards-guide.md) for proposed transitions, shared rules, and implementation order. These expand the visual proposal; their progression and reward values still require playtesting.

The following decisions supersede the earlier side-view, dark presentation, and ability-card proposals below:

- Portrait gameplay uses a top-down camera. Allied groups advance from the bottom toward the top, while enemy soldiers approach from the opposite direction.
- The presentation is colorful, cartoonish, and kid-friendly, with rounded characters, readable teams, playful impacts, and no graphic violence.
- Persistent unit cards spawn additional allied units. Each card has its own energy meter; defeating soldiers charges those meters. A full card can spawn another copy, resetting only its own energy. Cards can be recharged and used repeatedly.
- Begin with an allied starter group so kill-based energy generation can start. Exact energy rewards and unit limits still need design and playtesting.
- Guardian, Ranger, and Helper in v02 are provisional role labels. The requested roster has expanded to seven lead characters: Hero Wolvy, Princess Elva, Fairy Lulu, Captain Bobo, Scout Pip, Wizard Momo, and Tinker Tilly. See the [paired human/wolf concept lineup](Assets/Visuals/seven-heroes-human-wolf-concepts-v01.png) and [roster design brief](Design/Concepts/seven-heroes-roster.md). These are first-pass identity concepts; detailed character sheets are the next art stage.
- North-facing animation assets are now being produced in [Assets/Characters](Assets/Characters/README.md): four human actions, six wolf actions, separate attack/special VFX, equal 256px frame canvases, and a shared ground pivot. This package targets portrait iPhone/iPad composition. Consult its Unity QA audit for completed import/playback checks; assets and preview scenes do not establish that the full game is implemented.
- This image is a visual target, not evidence of implemented or tested gameplay. Earlier side-view images and the detailed original pitch below are historical exploration where they conflict with this update.

Visual review note: the illustration shows seven existing allies plus an arriving summon, while the HUD still reads seven. In implementation, update the ally count atomically when the new unit becomes active. Test and tune the camera toward a true overhead view; the concept uses some character tilt for readability.

Status: proposed design, ready for discussion and visual exploration. Nothing in this document has been implemented or playtested. All numbers are starting hypotheses for a prototype.

## 1. Name and corrected concept

**World of the Pack** is grammatically correct. It suggests a world experienced through the bonds, rules, and survival of a wolf pack. Keep this name if that is the heart of the game.

Use **portrait**, **wolf form**, **abilities**, **atmospheric**, and **real-time card tactics** when describing the concept.

### Polished version of the original idea

World of the Pack is an atmospheric, portrait-oriented, endless 2D survival game built around real-time card tactics. Some humans have the ability to transform into wolves. The world knows they exist, and soldiers have been sent to hunt them down. You lead a small pack fleeing persecution. To survive, you must protect your people, manage your abilities, and decide when to hide, fight, or transform.

The upper panel shows your pack's condition and the approaching danger. Ability cards sit at the bottom of the screen. The journey continues while you make decisions, so hesitation and repeated mistakes can bring the run to an end. Along the way, story encounters reveal who your pack members are and what they are fighting for.

### Short pitch

**Lead a hunted pack through an endless night. Read the danger, play your cards, and decide what you will risk to keep your family alive.**

## 2. The emotional core

The strongest version of this idea is about responsibility for a small, recognizable family. Running farther matters because someone is counting on you.

Target player feelings:

- **Attachment:** "I know who I am protecting."
- **Tension:** "I can see danger coming, and I have a difficult choice."
- **Power:** "My transformation changes the situation immediately."
- **Mastery:** "I survived because I understood the threat and timed my cards."
- **Curiosity:** "I want to reach the next refuge and learn what happens."

The first 10–15 minutes should test whether these feelings actually emerge. A written design cannot guarantee retention.

## 3. Story foundation

### The premise

A minority of humans can take wolf form. A military authority considers them a threat and sends patrols to capture or kill them. You are the leader of a displaced pack trying to reach a rumored sanctuary beyond the occupied forest.

The pack calls itself a clan in its traditions; in moment-to-moment dialogue, its members call one another the pack.

### Suggested opening

An abandoned shelter burns behind three fleeing figures. Searchlights sweep the trees. One member is injured, and another is too young to fight.

Opening text, shown in short beats:

> Some of us are born with a second shape.
>
> When the world discovered ours, it sent soldiers.
>
> Tonight, our home is gone.
>
> But our pack still lives.

The playable instruction follows immediately: **Keep them together. Reach the treeline.**

### Give the player someone to care about

Start with the leader and two named companions. Working names:

- **Mara:** an injured scout who knows a route toward sanctuary.
- **Ash:** a young pack member carrying the last keepsake from home.

Use tiny interactions during travel: Ash falls behind, Mara warns about a patrol, the leader checks that everyone is still there. Save longer conversations for shelter stops.

For the first prototype, companions do not die individually. Warnings and injury presentation communicate rising danger; the shared pack condition determines defeat. Individual deaths and branching grief would add substantial narrative and balancing work later.

## 4. Proposed playable core

### Format

- Portrait screen, with 9:16 as the initial composition target.
- A side-view 2D scene in the middle of the screen.
- The pack travels automatically from left to right; scenery scrolls past.
- The player controls tactical responses through cards rather than steering or jumping.
- One clearly signaled encounter is active at a time in the introductory version.
- Threat countdowns, attack wind-ups, card effects, and recovery occur in real time.

### The repeating loop

1. **Read:** identify an approaching patrol, obstacle, or pursuit threat.
2. **Decide:** compare the available cards, stamina, and time remaining.
3. **Act:** play one card or a short combination.
4. **Understand:** see the effect and a brief explanation of the consequence.
5. **Recover:** travel through a quieter stretch before the next threat.
6. **Adapt:** reach a shelter, choose an upgrade, and continue into a harder chapter.

The main question is: **Which response protects the pack now without leaving us helpless at the next encounter?**

### Example encounter

A rifle patrol blocks the trail. Its aim indicator fills over several seconds.

- **Mist Step** takes the pack around it quietly, but leaves less stamina for the next threat.
- **Pounce** removes it quickly, but the noise raises pursuit pressure.
- **Guard** absorbs the opening volley, allowing the pack to get past with a smaller cost to its condition if the other cards are unavailable.

These are different costs and consequences, rather than a single mandatory answer. On-screen symbols and animations must communicate which responses are effective before the deadline.

## 5. Cards and resources

### Keep the first version readable

Use a fixed five-card loadout with three cards visible in hand. Show the next card as a small preview. Playing a card sends it to the back of a visible, deterministic queue and fills its slot with the next card. A card's recovery timer continues while it cycles; it cannot be played again until ready.

Give the player this known starter loadout before introducing deck customization. Predictable access makes early failures easier to understand and reduces the chance that the opening feels unfair.

| Card | Immediate effect | Cost or tradeoff | Starter role |
| --- | --- | --- | --- |
| Pounce | Interrupt and remove one exposed enemy | 2 stamina; adds pursuit | Direct attack |
| Guard | Protect the pack against the next impact for a short window | 1 stamina; does not remove the threat | Forgiving defense |
| Mist Step | Slip past one ordinary patrol or obstacle | 3 stamina; ineffective against a marked tracking threat | Quiet evasion |
| Rally Howl | Recover 2 stamina over a short period | Long recovery; adds pursuit; restores no pack health | Resource recovery |
| Moonshift | Transform briefly, interrupt a threat, and strengthen the next Pounce or Guard | Requires a full moon meter | Signature power |

Use one large card label, one cost, and one primary effect icon. Put detailed explanations in an optional inspection view that pauses the run. Basic play must not require reading paragraphs under time pressure.

### Three visible systems

- **Pack condition:** one shared health bar, initially tested as five segments. Repeated unprotected hits end the run. Damage has a brief protection window so one failed response cannot cause several immediate losses.
- **Stamina:** initially five points, with slow passive recovery. Ordinary cards spend it; a long recovery on Rally Howl prevents an unlimited recovery loop.
- **Pursuit:** a three-stage pressure indicator. Loud actions raise it; quiet travel and some shelter choices lower it. Higher pursuit changes the next encounter mix, with advance warning.

The moon meter belongs beside Moonshift, rather than becoming another large HUD bar. Charge it through surviving encounters; do not make taking damage the best way to earn transformation.

### When there is no playable card

Every ordinary threat has a default consequence the player can survive at healthy condition. An unopposed attack costs condition, then the pack moves past it. No card-state deadlocks or unavoidable repeated hits.

Do not add hunger, thirst, equipment durability, individual companion health, or several currencies to the first prototype. The existing decisions need room to become interesting.

## 6. Making continuous play tense and fair

The world moves during encounters, but tension needs rhythm. Alternate short danger sequences with quiet travel and safe shelter decisions.

- Begin with roughly 6–8 seconds of visible warning before an ordinary threat resolves; tune through playtesting.
- Use silhouette, sound, and a clear timer together. Never rely on color alone.
- Introduce each enemy's behavior alone before mixing it into sequences.
- Give an immediate cue such as "Patrol bypassed — pursuit unchanged" or "Shot blocked — stamina low."
- Escalate with new combinations and tighter resource choices before simply making everything faster.
- Offer a pause button; opening tutorials, card inspection, and shelter choices pauses encounter timers.
- Start with modest encounter intensity. Reserve very short response windows for clearly introduced advanced threats.

One mistake should usually create a recoverable problem. Repeated mistakes, or an intentional gamble at low condition, can end the run. Defeat should leave the player able to explain what happened.

## 7. First 10–15 minutes: proposed experience

These are approximate beats, not forced delays or a promise that every run lasts this long.

| Time | Experience | What the player learns or feels |
| --- | --- | --- |
| 0:00–0:30 | Brief escape opening; automatic travel starts | Understand the danger and who needs protection |
| 0:30–2:00 | A safe demonstration of Guard, then a simple Pounce encounter | Learn timing with room to recover |
| 2:00–4:00 | Introduce stamina and Mist Step; show loud versus quiet consequences | Discover that different solutions affect the next encounter |
| 4:00–5:00 | First shelter; a short companion exchange and one upgrade choice | Feel relief and make the run their own |
| 5:00–7:00 | Introduce pursuit hounds and Rally Howl; show the hounds' tracking warning | Read a new threat instead of repeating one answer |
| 7:00–9:00 | First Moonshift opportunity, followed by a short two-encounter sequence | Feel powerful, then use that power deliberately |
| 9:00–11:00 | A wounded stranger encounter at a safe stop | Choose between helping and conserving supplies; see an immediate response |
| 11:00–13:00 | A telegraphed hunter captain sequence using already learned rules | Demonstrate mastery without a surprise mechanic |
| 13:00–15:00 | Reach a meaningful refuge, resolve the opening escape, reveal a clue to sanctuary | Receive closure and a reason to continue |

At the first refuge, offer **Continue the journey** and **Rest here**. Rest saves the active run at that checkpoint. Endless play should support a satisfying short session as well as a long one.

The opening story has a destination. Reaching it opens the continuing journey: other pack members, occupied routes, and later sanctuary clues. Do not keep the first promise permanently out of reach just because the mode is endless.

## 8. Reasons to keep playing

### Within a run

- A nearby, visible goal: next shelter, next rescue, or next chapter.
- A small upgrade choice every few encounters.
- Companions react to the player's choices.
- Tactical combinations emerge, such as Moonshift followed by Pounce, or Guard followed by resource recovery.
- Biome and encounter changes reveal progress through the world.

Example first-shelter choices: Guard protects for longer, Mist Step costs one less stamina with a longer recovery, or Pounce generates less pursuit. Each option changes a decision rather than only increasing a damage number.

### Between runs

Eventually unlock alternate cards, companion stories, and starting styles. Favor new options over permanent stat upgrades that make early runs deliberately weak. The first run should already feel complete enough to judge.

On defeat, show the cause, one useful learning cue, and the chapter reached. Offer a fast restart. Preserve discovered story entries, while resetting run resources and run-only upgrades.

### Endless variety

Build encounters from authored patterns, then vary their order within difficulty rules. Avoid unrestricted random combinations. After the opening arc, rotate forest, abandoned village, mountain pass, and occupied road chapters, each with readable threats and shelter moments.

## 9. Portrait UI and atmosphere

### Starting composition

- **Top, approximately 15%:** pack condition, stamina, pursuit, and a compact next-shelter indicator.
- **Middle, approximately 55%:** the pack, environmental storytelling, approaching threat, and its readable warning.
- **Bottom, approximately 30%:** three large ability cards, the next-card preview, and Moonshift readiness integrated with its card.

These proportions are a layout hypothesis. Check safe areas, thumb reach, text size, and whether a distant threat remains readable on a real phone.

### Art direction

Layered 2D silhouettes, cold moonlight, drifting fog, muted blue-green forest tones, and small warm lights around the pack. Patrol lanterns and searchlights create sharp shapes in the darkness. Use restrained red for immediate danger.

Give pack members distinct silhouettes and small human gestures. Wolf form should feel like an expression of the same character. Transformation needs an unmistakable shape change, a brief audio cue, and a clear gameplay payoff.

Use quiet footsteps, breathing, distant horns, and branches moving in the wind. Let pursuit change the soundscape. Keep warnings audible and readable even if the player turns off music.

## 10. Prototype scope and evidence

### First playable slice

One forest route, the leader and two companions, the five starter cards, a patrol, an obstacle, a tracking hound, one shelter upgrade, Moonshift, a defeat screen, and restart. Aim first for a coherent 3–5 minute loop before expanding into the full opening arc.

The slice must demonstrate automatic travel, readable warning windows, card cycling, resource tradeoffs, immediate feedback, fair damage, and one meaningful recovery after a mistake.

### Then expand

Once the short loop is enjoyable, add the 10–15 minute story sequence, the hunter captain, the first refuge, checkpoint saving, and rule-based endless encounter sequencing.

### What to observe in playtests

- Can a new player explain the objective and card costs without verbal coaching?
- Can they predict what happens if a warning expires?
- Do they notice their companions and care about reaching shelter?
- Do they vary their responses, or does one card solve nearly everything?
- Does defeat feel traceable to decisions rather than hand order or unclear warnings?
- Do they voluntarily continue or restart, and what reason do they give?

Record where players hesitate, take damage, quit, pause, or restart. Use observed behavior to revise the design. Compilation and attractive mockups alone cannot establish that the game feels good.

## 11. Next step: visual concepts

Create visuals after reviewing this gameplay direction. Suggested deliverables:

1. **Portrait gameplay concept, 1080 × 1920:** the top HUD, moonlit forest escape, three named cards, a readable approaching patrol, and next-shelter progress.
2. **Opening story frame:** the pack fleeing a burning shelter beneath distant searchlights.
3. **Shelter screen concept:** a quiet clearing, companion interaction, and three upgrade choices.

Keep card names, resource icons, character silhouettes, and colors consistent across all three. Label these as concept art until the composition has been built and checked in Unity.

## 12. Working decisions to revisit

The title, transformation premise, portrait orientation, atmospheric 2D presentation, endless journey, upper information panel, and bottom ability cards come from the original concept.

Automatic side-view travel, the named companions, five-card starter loadout, resource values, opening timeline, shelters, and checkpoint behavior are design proposals in this document. They can change after discussion and prototype evidence.

The next design decision is whether the fantasy of **leading and protecting a moving pack** feels right. If it does, prototype the card loop before expanding the lore or producing a large asset set.
