using System;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public enum AbilityType
    {
        TargetDamage,
        AreaDamage,
        HealAlly,
        ShieldTeam,
        DashBurst,
        StunSlam,
        BuffMoraleSpeed,
        DeployTrap
    }

    [Serializable]
    public class HeroAbilityData
    {
        public string abilityName;
        public AbilityType abilityType;
        public float cooldown = 8f;
        public float value = 35f;
        public float radius = 2.0f;
        public float duration = 4.0f;
        public string vfxName;
        public int animatorAction = 4; // 4 = Special01, 5 = Special02
    }

    public enum PassiveType
    {
        AlphaPresence,      // Wolvy: Nearby allies +15% damage
        NaturePrecision,    // Elva: +25% Crit chance on distant targets
        KindheartedMagic,   // Lulu: Constant ambient healing
        BraveProtector,     // Bobo: Intercepts attacks for nearby allies & damage reduction
        QuickPaws,          // Pip: +30% movement speed and 20% dodge
        StarChain,          // Momo: Attacks bounce to a second target
        CleverContraptions  // Tilly: Drops brass gears granting energy
    }

    [CreateAssetMenu(fileName = "NewHeroKit", menuName = "World of the Pack/Combat/Hero Kit Definition")]
    public class HeroKitDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string heroId;
        public string displayName;
        public string shortName;
        public FormationRole formationRole;
        public int preferredLane = 2;
        public int energyCost = 3;

        [Header("Base Human Stats")]
        public float maxHealth = 120f;
        public float moveSpeed = 1.4f;
        public float attackDamage = 20f;
        public float attackInterval = 1.2f;
        public float attackRange = 1.4f;
        public float agroRadius = 3.5f;
        public bool isRanged = false;
        public bool isHealer = false;

        [Header("Wolf Form Modifiers")]
        public float wolfHealthMultiplier = 1.25f;
        public float wolfDamageMultiplier = 2.0f;
        public float wolfSpeedMultiplier = 1.35f;
        public float wolfAttackIntervalMultiplier = 0.65f;

        [Header("Abilities")]
        public string basicAttackVfx = "MoonlightSlash";
        public HeroAbilityData special01;
        public HeroAbilityData special02;

        [Header("Passive")]
        public PassiveType passiveType;
        public string passiveName;
        public string passiveDescription;
        public float passiveMagnitude = 0.15f;
    }
}
