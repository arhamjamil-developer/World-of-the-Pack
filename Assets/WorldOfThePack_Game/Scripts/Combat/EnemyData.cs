using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public enum EnemyRole
    {
        BasicMelee,
        FastAttacker,
        RangedAttacker,
        Tank,
        Support,
        Summoner,
        Elite,
        Boss
    }

    public enum BiomeType
    {
        SunnyForestTrail = 0,
        MushroomGrove = 1,
        RiverMeadow = 2,
        EmberlaneRefuge = 3,
        CloudstepHighlands = 4
    }

    public enum TelegraphType
    {
        None,
        ConeSouth,
        SlamCircle,
        ChargeLane,
        LandingTarget
    }

    [Serializable]
    public class BossAttackTelegraph
    {
        public string attackName;
        public TelegraphType telegraphType = TelegraphType.SlamCircle;
        public float telegraphDuration = 1.2f;
        public float damage = 40f;
        public float radius = 2.0f;
        public float cooldown = 8f;
        public string vfxName = "EnemySlamCircle";
        public bool isStunAttack = false;
        public float stunDuration = 1.5f;
    }

    [CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "World of the Pack/Combat/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId;
        public string enemyName;
        public EnemyRole role = EnemyRole.BasicMelee;
        public BiomeType biome = BiomeType.SunnyForestTrail;

        [Header("Base Stats")]
        public float maxHealth = 70f;
        public float moveSpeed = 1.2f;
        public float attackDamage = 15f;
        public float attackInterval = 1.25f;
        public float attackRange = 1.2f;
        public float breachDamage = 15f;
        public float energyReward = 20f;
        public Color tint = Color.white;

        [Header("Ranged / Spell Config")]
        public bool isRanged = false;
        public string projectileVfx = "NatureArrow";

        [Header("Telegraph Attack")]
        public bool hasTelegraph = false;
        public BossAttackTelegraph telegraphAttack;
    }

}
