using System.IO;
using UnityEditor;
using UnityEngine;

namespace WorldOfThePack.Game.EditorTools
{
    public static class BiomeBossAssetGenerator
    {
        private const string FOLDER = "Assets/WorldOfThePack_Game/Data/Bosses";

        [MenuItem("World of the Pack/Generate Boss Data Assets")]
        public static void GenerateAllBosses()
        {
            if (!Directory.Exists(FOLDER)) Directory.CreateDirectory(FOLDER);

            CreateThornGuardian();
            CreateMushroomKing();
            CreateRiverGuardian();
            CreateBanditChief();
            CreateStoneTitan();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BiomeBossAssetGenerator] Successfully generated all 5 Biome Bosses!");
        }

        private static void CreateThornGuardian()
        {
            string path = $"{FOLDER}/AncientThornGuardian_Boss.asset";
            var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
            if (boss == null) { boss = ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(boss, path); }

            boss.bossId = "AncientThornGuardian";
            boss.bossName = "Ancient Thorn Guardian";
            boss.bossTitle = "Corrupted Protector of the Sunny Forest";
            boss.biome = BiomeType.SunnyForestTrail;
            boss.maxHealth = 500f;
            boss.moveSpeed = 0.8f;
            boss.basicAttackDamage = 28f;
            boss.attackInterval = 1.6f;
            boss.attackRange = 1.8f;
            boss.breachDamage = 40f;
            boss.energyReward = 120f;
            boss.tint = new Color(0.9f, 0.6f, 0.6f, 1f);
            boss.moonShardsReward = 80;
            boss.moonFragmentIndex = 1;
            boss.unlockedHeroOnDefeat = "FairyLulu";

            boss.specialAttacks.Clear();
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Bramble Slam",
                telegraphType = TelegraphType.SlamCircle,
                telegraphDuration = 1.2f,
                damage = 38f,
                radius = 2.4f,
                cooldown = 7f,
                vfxName = "EnemySlamCircle",
                isStunAttack = true,
                stunDuration = 1.2f
            });
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Thorn Sweeper",
                telegraphType = TelegraphType.ConeSouth,
                telegraphDuration = 1.4f,
                damage = 44f,
                radius = 3.2f,
                cooldown = 11f,
                vfxName = "EnemyConeSouth"
            });

            EditorUtility.SetDirty(boss);
        }

        private static void CreateMushroomKing()
        {
            string path = $"{FOLDER}/MushroomKing_Boss.asset";
            var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
            if (boss == null) { boss = ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(boss, path); }

            boss.bossId = "MushroomKing";
            boss.bossName = "The Mushroom King";
            boss.bossTitle = "Overlord of Spores";
            boss.biome = BiomeType.MushroomGrove;
            boss.maxHealth = 680f;
            boss.moveSpeed = 0.75f;
            boss.basicAttackDamage = 34f;
            boss.attackInterval = 1.7f;
            boss.attackRange = 2.0f;
            boss.breachDamage = 50f;
            boss.energyReward = 150f;
            boss.tint = new Color(0.85f, 0.65f, 0.95f, 1f);
            boss.moonShardsReward = 120;
            boss.moonFragmentIndex = 2;
            boss.unlockedHeroOnDefeat = "ScoutPip";

            boss.specialAttacks.Clear();
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Spore Detonation",
                telegraphType = TelegraphType.LandingTarget,
                telegraphDuration = 1.3f,
                damage = 48f,
                radius = 2.6f,
                cooldown = 8f,
                vfxName = "EnemyLandingTarget"
            });
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Toxic Shockwave",
                telegraphType = TelegraphType.SlamCircle,
                telegraphDuration = 1.5f,
                damage = 52f,
                radius = 3.0f,
                cooldown = 12f,
                vfxName = "EnemySlamCircle"
            });

            EditorUtility.SetDirty(boss);
        }

        private static void CreateRiverGuardian()
        {
            string path = $"{FOLDER}/RiverGuardian_Boss.asset";
            var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
            if (boss == null) { boss = ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(boss, path); }

            boss.bossId = "RiverGuardian";
            boss.bossName = "River Guardian";
            boss.bossTitle = "Serpent of the Blue Shallows";
            boss.biome = BiomeType.RiverMeadow;
            boss.maxHealth = 820f;
            boss.moveSpeed = 0.85f;
            boss.basicAttackDamage = 38f;
            boss.attackInterval = 1.5f;
            boss.attackRange = 2.2f;
            boss.breachDamage = 60f;
            boss.energyReward = 180f;
            boss.tint = new Color(0.6f, 0.8f, 1f, 1f);
            boss.moonShardsReward = 150;
            boss.moonFragmentIndex = 3;
            boss.unlockedHeroOnDefeat = "WizardMomo";

            boss.specialAttacks.Clear();
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Tidal Surge Lane",
                telegraphType = TelegraphType.ChargeLane,
                telegraphDuration = 1.4f,
                damage = 55f,
                radius = 2.0f,
                cooldown = 9f,
                vfxName = "EnemyChargeLane"
            });
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Vortex Geyser",
                telegraphType = TelegraphType.LandingTarget,
                telegraphDuration = 1.2f,
                damage = 60f,
                radius = 2.8f,
                cooldown = 13f,
                vfxName = "EnemyLandingTarget"
            });

            EditorUtility.SetDirty(boss);
        }

        private static void CreateBanditChief()
        {
            string path = $"{FOLDER}/EmberlaneBanditChief_Boss.asset";
            var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
            if (boss == null) { boss = ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(boss, path); }

            boss.bossId = "EmberlaneBanditChief";
            boss.bossName = "Bandit Chief Vane";
            boss.bossTitle = "Warmaster of Emberlane";
            boss.biome = BiomeType.EmberlaneRefuge;
            boss.maxHealth = 950f;
            boss.moveSpeed = 0.95f;
            boss.basicAttackDamage = 45f;
            boss.attackInterval = 1.35f;
            boss.attackRange = 1.7f;
            boss.breachDamage = 70f;
            boss.energyReward = 200f;
            boss.tint = new Color(1f, 0.55f, 0.45f, 1f);
            boss.moonShardsReward = 180;
            boss.moonFragmentIndex = 4;
            boss.unlockedHeroOnDefeat = "TinkerTilly";

            boss.specialAttacks.Clear();
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Fiery Ambush Cleave",
                telegraphType = TelegraphType.ConeSouth,
                telegraphDuration = 1.1f,
                damage = 62f,
                radius = 3.4f,
                cooldown = 8f,
                vfxName = "EnemyConeSouth"
            });
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Infernal Charge",
                telegraphType = TelegraphType.ChargeLane,
                telegraphDuration = 1.3f,
                damage = 68f,
                radius = 2.2f,
                cooldown = 12f,
                vfxName = "EnemyChargeLane"
            });

            EditorUtility.SetDirty(boss);
        }

        private static void CreateStoneTitan()
        {
            string path = $"{FOLDER}/CloudstepStoneTitan_Boss.asset";
            var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
            if (boss == null) { boss = ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(boss, path); }

            boss.bossId = "CloudstepStoneTitan";
            boss.bossName = "Cloudstep Stone Titan";
            boss.bossTitle = "Awakened Guardian of the Great Moon Tree";
            boss.biome = BiomeType.CloudstepHighlands;
            boss.maxHealth = 1350f;
            boss.moveSpeed = 0.65f;
            boss.basicAttackDamage = 55f;
            boss.attackInterval = 2.0f;
            boss.attackRange = 2.4f;
            boss.breachDamage = 100f;
            boss.energyReward = 300f;
            boss.tint = new Color(0.85f, 0.85f, 0.9f, 1f);
            boss.moonShardsReward = 300;
            boss.moonFragmentIndex = 5;
            boss.unlockedHeroOnDefeat = null; // Final Boss!

            boss.specialAttacks.Clear();
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Colossal Seismic Stomp",
                telegraphType = TelegraphType.SlamCircle,
                telegraphDuration = 1.6f,
                damage = 80f,
                radius = 3.6f,
                cooldown = 9f,
                vfxName = "EnemySlamCircle",
                isStunAttack = true,
                stunDuration = 2.0f
            });
            boss.specialAttacks.Add(new BossAttackTelegraph
            {
                attackName = "Falling Sky Boulders",
                telegraphType = TelegraphType.LandingTarget,
                telegraphDuration = 1.4f,
                damage = 85f,
                radius = 3.2f,
                cooldown = 13f,
                vfxName = "EnemyLandingTarget"
            });

            EditorUtility.SetDirty(boss);
        }
    }
}
