using System.IO;
using UnityEditor;
using UnityEngine;

namespace WorldOfThePack.Game.EditorTools
{
    public static class HeroKitAssetGenerator
    {
        private const string FOLDER = "Assets/WorldOfThePack_Game/Data/HeroKits";

        [MenuItem("World of the Pack/Generate Hero Kit Data Assets")]
        public static void GenerateAllHeroKits()
        {
            if (!Directory.Exists(FOLDER))
            {
                Directory.CreateDirectory(FOLDER);
            }

            CreateWolvyKit();
            CreateElvaKit();
            CreateLuluKit();
            CreateBoboKit();
            CreatePipKit();
            CreateMomoKit();
            CreateTillyKit();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HeroKitAssetGenerator] Successfully generated all 7 Hero Kit definitions!");
        }

        private static void CreateWolvyKit()
        {
            string path = $"{FOLDER}/HeroWolvy_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "HeroWolvy";
            kit.displayName = "Hero Wolvy";
            kit.shortName = "WOLVY";
            kit.formationRole = FormationRole.Tank;
            kit.preferredLane = 2;
            kit.energyCost = 4;
            kit.maxHealth = 140f;
            kit.moveSpeed = 1.45f;
            kit.attackDamage = 26f;
            kit.attackInterval = 1.1f;
            kit.attackRange = 1.4f;
            kit.agroRadius = 3.5f;
            kit.isRanged = false;
            kit.isHealer = false;

            kit.wolfHealthMultiplier = 1.25f;
            kit.wolfDamageMultiplier = 2.0f;
            kit.wolfSpeedMultiplier = 1.35f;
            kit.wolfAttackIntervalMultiplier = 0.65f;

            kit.basicAttackVfx = "MoonlightSlash";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Paw Dash",
                abilityType = AbilityType.DashBurst,
                cooldown = 7f,
                value = 45f,
                radius = 1.8f,
                vfxName = "PawDash",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Rally Howl",
                abilityType = AbilityType.BuffMoraleSpeed,
                cooldown = 12f,
                value = 0.25f,
                radius = 3.5f,
                vfxName = "RallyHowl",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.AlphaPresence;
            kit.passiveName = "Alpha Presence";
            kit.passiveDescription = "Nearby allies gain +15% damage bonus.";
            kit.passiveMagnitude = 0.15f;

            EditorUtility.SetDirty(kit);
        }

        private static void CreateElvaKit()
        {
            string path = $"{FOLDER}/PrincessElva_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "PrincessElva";
            kit.displayName = "Princess Elva";
            kit.shortName = "ELVA";
            kit.formationRole = FormationRole.Ranger;
            kit.preferredLane = 2;
            kit.energyCost = 3;
            kit.maxHealth = 90f;
            kit.moveSpeed = 1.35f;
            kit.attackDamage = 18f;
            kit.attackInterval = 1.25f;
            kit.attackRange = 4.2f;
            kit.agroRadius = 5.0f;
            kit.isRanged = true;
            kit.isHealer = false;

            kit.wolfHealthMultiplier = 1.2f;
            kit.wolfDamageMultiplier = 2.0f;
            kit.wolfSpeedMultiplier = 1.3f;
            kit.wolfAttackIntervalMultiplier = 0.65f;

            kit.basicAttackVfx = "NatureArrow";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Leaf Volley",
                abilityType = AbilityType.AreaDamage,
                cooldown = 8f,
                value = 32f,
                radius = 2.5f,
                vfxName = "LeafVolley",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Forest Focus",
                abilityType = AbilityType.BuffMoraleSpeed,
                cooldown = 14f,
                value = 0.40f,
                duration = 6f,
                radius = 0f,
                vfxName = "ForestFocus",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.NaturePrecision;
            kit.passiveName = "Nature Precision";
            kit.passiveDescription = "+25% critical strike chance against distant foes.";
            kit.passiveMagnitude = 0.25f;

            EditorUtility.SetDirty(kit);
        }

        private static void CreateLuluKit()
        {
            string path = $"{FOLDER}/FairyLulu_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "FairyLulu";
            kit.displayName = "Fairy Lulu";
            kit.shortName = "LULU";
            kit.formationRole = FormationRole.Support;
            kit.preferredLane = 1;
            kit.energyCost = 4;
            kit.maxHealth = 85f;
            kit.moveSpeed = 1.45f;
            kit.attackDamage = 12f;
            kit.attackInterval = 1.6f;
            kit.attackRange = 3.5f;
            kit.agroRadius = 4.5f;
            kit.isRanged = true;
            kit.isHealer = true;

            kit.wolfHealthMultiplier = 1.2f;
            kit.wolfDamageMultiplier = 1.8f;
            kit.wolfSpeedMultiplier = 1.35f;
            kit.wolfAttackIntervalMultiplier = 0.65f;

            kit.basicAttackVfx = "MoonBubble";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Moon Hug",
                abilityType = AbilityType.HealAlly,
                cooldown = 9f,
                value = 50f,
                radius = 2.5f,
                vfxName = "HealingCircle",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Bubble Blessing",
                abilityType = AbilityType.ShieldTeam,
                cooldown = 16f,
                value = 35f,
                duration = 6f,
                radius = 4.0f,
                vfxName = "BlessingPop",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.KindheartedMagic;
            kit.passiveName = "Kindhearted Magic";
            kit.passiveDescription = "Aura restores 4 HP every second to wounded pack members.";
            kit.passiveMagnitude = 4.0f;

            EditorUtility.SetDirty(kit);
        }

        private static void CreateBoboKit()
        {
            string path = $"{FOLDER}/CaptainBobo_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "CaptainBobo";
            kit.displayName = "Captain Bobo";
            kit.shortName = "BOBO";
            kit.formationRole = FormationRole.Tank;
            kit.preferredLane = 3;
            kit.energyCost = 3;
            kit.maxHealth = 190f;
            kit.moveSpeed = 1.25f;
            kit.attackDamage = 22f;
            kit.attackInterval = 1.4f;
            kit.attackRange = 1.2f;
            kit.agroRadius = 3.0f;
            kit.isRanged = false;
            kit.isHealer = false;

            kit.wolfHealthMultiplier = 1.3f;
            kit.wolfDamageMultiplier = 1.9f;
            kit.wolfSpeedMultiplier = 1.25f;
            kit.wolfAttackIntervalMultiplier = 0.7f;

            kit.basicAttackVfx = "EarthImpact";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Acorn Slam",
                abilityType = AbilityType.StunSlam,
                cooldown = 8f,
                value = 30f,
                radius = 2.2f,
                vfxName = "AcornShockwave",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Big Brave Guard",
                abilityType = AbilityType.ShieldTeam,
                cooldown = 14f,
                value = 0.60f,
                duration = 5f,
                radius = 3.0f,
                vfxName = "AcornBarrier",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.BraveProtector;
            kit.passiveName = "Brave Protector";
            kit.passiveDescription = "Absorbs 25% of incoming damage for pack allies.";
            kit.passiveMagnitude = 0.25f;

            EditorUtility.SetDirty(kit);
        }

        private static void CreatePipKit()
        {
            string path = $"{FOLDER}/ScoutPip_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "ScoutPip";
            kit.displayName = "Scout Pip";
            kit.shortName = "PIP";
            kit.formationRole = FormationRole.Skirmisher;
            kit.preferredLane = 0;
            kit.energyCost = 3;
            kit.maxHealth = 95f;
            kit.moveSpeed = 1.7f;
            kit.attackDamage = 18f;
            kit.attackInterval = 0.9f;
            kit.attackRange = 1.3f;
            kit.agroRadius = 3.8f;
            kit.isRanged = false;
            kit.isHealer = false;

            kit.wolfHealthMultiplier = 1.2f;
            kit.wolfDamageMultiplier = 2.1f;
            kit.wolfSpeedMultiplier = 1.4f;
            kit.wolfAttackIntervalMultiplier = 0.6f;

            kit.basicAttackVfx = "WindStrike";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Zoom Dash",
                abilityType = AbilityType.DashBurst,
                cooldown = 6f,
                value = 40f,
                radius = 1.8f,
                vfxName = "ZoomDash",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Whirl Paw",
                abilityType = AbilityType.AreaDamage,
                cooldown = 11f,
                value = 45f,
                radius = 2.0f,
                vfxName = "Whirlwind",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.QuickPaws;
            kit.passiveName = "Quick Paws";
            kit.passiveDescription = "+30% movement speed and +20% dodge chance.";
            kit.passiveMagnitude = 0.30f;

            EditorUtility.SetDirty(kit);
        }

        private static void CreateMomoKit()
        {
            string path = $"{FOLDER}/WizardMomo_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "WizardMomo";
            kit.displayName = "Wizard Momo";
            kit.shortName = "MOMO";
            kit.formationRole = FormationRole.Mage;
            kit.preferredLane = 4;
            kit.energyCost = 4;
            kit.maxHealth = 80f;
            kit.moveSpeed = 1.3f;
            kit.attackDamage = 25f;
            kit.attackInterval = 1.5f;
            kit.attackRange = 4.0f;
            kit.agroRadius = 4.8f;
            kit.isRanged = true;
            kit.isHealer = false;

            kit.wolfHealthMultiplier = 1.2f;
            kit.wolfDamageMultiplier = 2.2f;
            kit.wolfSpeedMultiplier = 1.3f;
            kit.wolfAttackIntervalMultiplier = 0.65f;

            kit.basicAttackVfx = "StarProjectile";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Star Sprinkle",
                abilityType = AbilityType.AreaDamage,
                cooldown = 8f,
                value = 38f,
                radius = 2.4f,
                vfxName = "StarShower",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Moon Pop",
                abilityType = AbilityType.AreaDamage,
                cooldown = 13f,
                value = 55f,
                radius = 2.8f,
                vfxName = "MoonNova",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.StarChain;
            kit.passiveName = "Star Chain";
            kit.passiveDescription = "Projectiles chain to hit an additional nearby enemy.";
            kit.passiveMagnitude = 0.65f;

            EditorUtility.SetDirty(kit);
        }

        private static void CreateTillyKit()
        {
            string path = $"{FOLDER}/TinkerTilly_Kit.asset";
            var kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
            if (kit == null) { kit = ScriptableObject.CreateInstance<HeroKitDefinition>(); AssetDatabase.CreateAsset(kit, path); }

            kit.heroId = "TinkerTilly";
            kit.displayName = "Tinker Tilly";
            kit.shortName = "TILLY";
            kit.formationRole = FormationRole.Ranger;
            kit.preferredLane = 3;
            kit.energyCost = 3;
            kit.maxHealth = 105f;
            kit.moveSpeed = 1.4f;
            kit.attackDamage = 17f;
            kit.attackInterval = 1.35f;
            kit.attackRange = 3.2f;
            kit.agroRadius = 4.2f;
            kit.isRanged = true;
            kit.isHealer = false;

            kit.wolfHealthMultiplier = 1.2f;
            kit.wolfDamageMultiplier = 2.0f;
            kit.wolfSpeedMultiplier = 1.3f;
            kit.wolfAttackIntervalMultiplier = 0.7f;

            kit.basicAttackVfx = "SpringBolt";
            kit.special01 = new HeroAbilityData
            {
                abilityName = "Spring Surprise",
                abilityType = AbilityType.DeployTrap,
                cooldown = 8f,
                value = 35f,
                radius = 2.0f,
                vfxName = "BouncingTrap",
                animatorAction = 4
            };
            kit.special02 = new HeroAbilityData
            {
                abilityName = "Bubble Trap",
                abilityType = AbilityType.DeployTrap,
                cooldown = 12f,
                value = 25f,
                radius = 2.2f,
                vfxName = "BubbleDevice",
                animatorAction = 5
            };
            kit.passiveType = PassiveType.CleverContraptions;
            kit.passiveName = "Clever Contraptions";
            kit.passiveDescription = "Defeating enemies produces +1 bonus shared expedition energy.";
            kit.passiveMagnitude = 1.0f;

            EditorUtility.SetDirty(kit);
        }
    }
}
