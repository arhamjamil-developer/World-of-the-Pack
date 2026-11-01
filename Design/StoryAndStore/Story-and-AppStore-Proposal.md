# World of the Pack: The Last Moonward

## What the current game supports

The implemented loop is a queen escort: Wolvy begins as the only defender, enemies form on a grid, and the player recruits allies using independently charged cards and Moon Charms. Seven authored hero kits cover melee, a high-health tank, ranged attacks, healing and magic. Defeats refill cards and build Moonshift power. The queen's death ends the run. Waves continue procedurally, with a boss every fifth wave. An interactive tutorial teaches the first deployment, and short pack tasks award charms.

This analysis uses the current GameDirector, WaveManager, ArenaDirector, HeroKit assets, Task catalog and tutorial, rather than treating the older design document as proof of shipped features. The older GDD supplies the Great Moon Tree, Moon Amber, Briarwood Blight and Ironthorne Hunters. Five chapter/biome names exist in StoryCampaignDirector, but campaign wave progression is currently bypassed in arena mode. Branching story chapters, queen blessings, bond bonuses and collectible story fragments below are proposals, not current store claims.

## Proposed story

For generations, the Great Moon Tree watched over Sylvanna. Its silver roots bound the forest together, and its light gave seven guardians the gift of two forms: a familiar face by day, a wolf's spirit when the moon called.

Then the Ironthorne Hunters cut into the tree's heart. They carried away its Moon Amber, leaving the Briarwood Blight to creep through the roots. The paths shifted. Gentle creatures turned hostile. The forest began to forget its own name.

The queen survived the fall of the sanctuary, but only barely. Within her remained the last Rootsong—the living memory that could wake the Moon Tree again. Two loyal wolves lifted her onto an old royal seat made from the tree's unbroken wood. It became a travelling sanctuary, carried above the poisoned ground.

Wolvy was the first to step onto the road beside her.

He could not defend every path alone. With each Moon Charm, another guardian could answer his call: Elva with her bow, Bobo with his shield, Lulu with her healing light, Pip with his speed, Momo with his magic, and Tilly with her clever devices. Every companion strengthened the pack. Every hasty decision could leave the queen exposed.

Now the Hunters gather in the clearings ahead. Wolvy must read their formation, choose who answers the next call, and decide when to release the wolves within his companions.

The queen does not ask them to save a crown. She asks them to carry the forest's last hope one clearing farther.

**Narrative status:** this is a proposed framing for the existing escort mechanics. Rootsong dialogue, an opening cinematic, and a story ending have not been added by this UI task.

## How to make the story playable

### 1. Give each region a small, complete journey

Use the existing region names: Sunny Forest Trail, Mushroom Grove, River Meadow, Emberlane Refuge and Cloudstep Highlands. Build one short chapter from three to five encounters, one camp choice and a named boss. A chapter should reveal a small part of why the queen matters, then leave a clear next destination. Keep Endless Escort as a separate mode after the first chapter, so a new player can finish something before being asked to survive indefinitely.

Implementation: put encounter order, biome, enemy pool, camp and dialogue into authored chapter assets. Let one progression owner select the next arena; remove the current overlap between campaign observers and endless wave spawning. Save chapter completion separately from the current run. This is the strongest next feature because the visual regions and core arena already exist.

### 2. Show enemy intent before the fight

Add readable role badges and a short intent preview: a bow threatens a protected lane, a heavy guard shields nearby enemies, and a runner tries to slip past a weak front. A slow attack gets a visible windup indicator. Give the player enough information to explain why Elva belongs behind Bobo or why Momo helps against a cluster.

Implementation: add intent and target-rule data to enemy definitions, render a small icon over the relevant enemy tile, then make targeting follow the displayed rule. A tooltip must describe actual behavior. Prioritize this before adding more enemy artwork or hero cards; it makes the existing choices more meaningful.

### 3. Turn the queen into a source of choices

At a refuge, the queen can spend her remaining strength on one blessing: recover some health, prepare a card, or improve the next encounter's charm reward. Choosing strength now should carry an understandable cost later. Keep these as run choices so they complement card tactics rather than replace them with permanent stat grinding.

Implementation: extend the existing camp decision flow with three clearly authored blessings, an explanation of the tradeoff and a visible run effect. Do not promise queen powers in store copy until they can be used and verified in the actual arena loop.

### 4. Make relationships visible

Introduce a few named pack bonds, not every possible pair at once. Bobo and Elva can reward protected ranged positioning; Wolvy and Lulu can reward holding a wounded front; Pip and Tilly can reward a controlled flank. Show the activation condition and bonus before deployment.

Implementation: define a small set of pairing rules, evaluate them against positions/units, and show one discreet bond line or icon. Test that the bonus does not erase the value of other compositions. This creates a stronger collection identity without immediately needing a larger roster.

### 5. Unlock memories, not just numbers

Boss victories and task milestones can reveal illustrated memories of the queen, the seven guardians and the Moon Tree. Each entry should also teach something useful about an enemy or hero. This makes short runs contribute to a larger story even when the escort falls.

Implementation: use a persistent codex of unlocked entries with one short illustrated page per milestone. Keep narrative unlocks separate from combat power so repeated failure still feels worthwhile without making early waves trivial.

### Suggested order

1. Enemy intent and clearer counterplay.
2. One complete story chapter with a camp and boss.
3. Queen blessings integrated into that chapter.
4. A few pack bonds.
5. Memory codex, then additional chapters and challenge contracts.

A useful next production milestone is **one polished ten-minute chapter with an ending**, alongside the existing endless mode. Add the remaining chapter content after its pacing, readability and replay value work on a device.

## App Store description: current-feature draft

**One queen. Seven guardians. Every card matters.**

Lead Wolvy and his companions through a threatened forest in World of the Pack, a fantasy card-tactics adventure where the queen's survival rests on your decisions.

Begin with a single defender. When enemies gather, study their formation and build your own response. Place sturdy allies at the front, protect your ranged attackers, and bring healing when the pack needs it. Your companions fight automatically—but choosing who joins them, where they stand and when to reinforce is your responsibility.

Earn Moon Charms, recharge your hero cards through victories, and call on Moonshift to unleash the pack's wolf forms. Spend too quickly and you may have no answer to the next threat. Wait too long and the enemy may reach the queen.

- Recruit seven distinct guardians with different combat roles.
- Plan deployments on a five-lane battlefield.
- Balance card charge, charm costs and reinforcements.
- Face mixed enemy formations and recurring boss encounters.
- Complete pack tasks for extra charms.
- Learn through a guided first battle.
- Take in the forest with independently collapsible HUD and card panels.
- Push farther through an endless escort journey.

Protect the queen. Build your pack. Carry hope one wave farther.

**Copy status:** a draft describing the current local implementation, not evidence of App Store release readiness. It deliberately does not claim playable five-region story chapters, multiplayer, daily events, offline certification, cloud saves, cosmetic collections or the proposed bond/blessing systems. Verify the release build and the final feature set before publishing.

## Future store language, only after those features ship

“Journey through five story regions, discover the queen's forgotten memories, and shape each expedition with blessings and pack bonds.”

That sentence should replace the endless-only framing only after the chapters, memories, blessings and bonds are implemented and tested.
