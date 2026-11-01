using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    [CreateAssetMenu(fileName = "NewBossDefinition", menuName = "World of the Pack/Combat/Boss Definition")]
    public class BossDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string bossId;
        public string bossName;
        public string bossTitle;
        public BiomeType biome;

        [Header("Boss Stats")]
        public float maxHealth = 650f;
        public float moveSpeed = 0.75f;
        public float basicAttackDamage = 35f;
        public float attackInterval = 1.8f;
        public float attackRange = 1.8f;
        public float breachDamage = 50f;
        public float energyReward = 150f;
        public Color tint = new Color(1f, 0.65f, 0.65f, 1f);

        [Header("Boss Special Attacks")]
        public List<BossAttackTelegraph> specialAttacks = new List<BossAttackTelegraph>();

        [Header("Phase Progression")]
        public float enrageHealthPercent = 0.4f;
        public float enrageAttackSpeedMultiplier = 1.35f;

        [Header("Victory Rewards")]
        public int moonShardsReward = 100;
        public int moonFragmentIndex = 1;
        public string unlockedHeroOnDefeat;
    }
}
