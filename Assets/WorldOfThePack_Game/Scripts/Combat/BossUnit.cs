using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.VFX;

namespace WorldOfThePack.Game
{
    public class BossUnit : Combatant
    {
        [Header("Boss Configuration")]
        [SerializeField] private BossDefinition bossDefinition;
        [SerializeField] private string bossId = "AncientThornGuardian";

        [Header("State")]
        private bool isEnraged = false;
        private bool isPreparingSpecial = false;
        private float nextSpecialAttackTime = 0f;
        private int currentSpecialIndex = 0;
        private float attackAnimEndTime = 0f;
        private float recoveryUntil;
        private bool strikeUsedForRecovery;
        private readonly HashSet<string> contributors = new HashSet<string>();
        private WorldOfThePack.CombatArt.UnitAttackPresentation presentation;
        public int PackContributors => contributors.Count;
        public bool IsWindingUp => isPreparingSpecial && !IsExposed;
        public bool IsExposed => IsAlive && Time.time < recoveryUntil;
        public bool CanPackStrike => IsExposed && contributors.Count >= 3 && !strikeUsedForRecovery;
        public bool TryPackStrike()
        {
            if (!CanPackStrike || GameDirector.Instance == null || GameDirector.Instance.CurrentState != GameState.Playing ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) return false;
            strikeUsedForRecovery = true;
            contributors.Clear();
            WorldOfThePack.Expedition.PackImpactPulse.Spawn(Position, new Color(.5f, .9f, 1), 1.1f);
            int order = 0;
            foreach (var unit in Combatant.LivingUnits)
                if (unit is HeroUnit hero && hero.IsAlive) hero.PerformPackStrike(this, order++);
            WorldOfThePack.NewUI.PackGameplayHUD.Instance?.ShowBanner("PACK STRIKE • Together, stronger!", 2);
            return true;
        }

        public BossDefinition Definition => bossDefinition;
        public bool IsEnraged => isEnraged;
        public void ApplyWaveDifficulty(int waveIndex)
        {
            float growth = 1f + Mathf.Max(0, waveIndex - 4) * .1f;
            maxHealth *= growth; currentHealth = maxHealth;
            attackDamage *= 1f + Mathf.Max(0, waveIndex - 4) * .06f;
            UpdateHealthBarUI();
        }

        public static event Action<BossUnit> OnBossSpawned;
        public static event Action<BossUnit> OnBossDefeated;
        public static event Action<float, float> OnBossHealthUpdated;

        protected override void Awake()
        {
            faction = UnitFaction.Hunters;
            base.Awake();
            presentation = GetComponent<WorldOfThePack.CombatArt.UnitAttackPresentation>();

            LoadBossDefinitionIfNeeded();
            ConfigureBossStats();
            if (ArenaDirector.Instance != null) attackRange = .95f;
            SetupBossVisual();
            SetupSelectionRing();
        }

        protected override void Start()
        {
            base.Start();

            // Notify systems
            OnBossSpawned?.Invoke(this);
            OnBossHealthUpdated?.Invoke(currentHealth, maxHealth);

            // Halt expedition march for the boss confrontation
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(false);
            }

            StartCoroutine(EntranceRoutine());
        }

        private void LoadBossDefinitionIfNeeded()
        {
            if (bossDefinition == null)
            {
#if UNITY_EDITOR
                string path = $"Assets/WorldOfThePack_Game/Data/Bosses/{bossId}_Boss.asset";
                bossDefinition = UnityEditor.AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
#endif
            }
        }

        private void ConfigureBossStats()
        {
            if (bossDefinition != null)
            {
                unitName = bossDefinition.bossName;
                maxHealth = bossDefinition.maxHealth;
                currentHealth = bossDefinition.maxHealth;
                moveSpeed = bossDefinition.moveSpeed;
                attackDamage = bossDefinition.basicAttackDamage;
                attackInterval = bossDefinition.attackInterval;
                attackRange = bossDefinition.attackRange;
            }
            else
            {
                unitName = "Biome Boss";
                maxHealth = 600f;
                currentHealth = 600f;
                moveSpeed = 0.8f;
                attackDamage = 30f;
                attackInterval = 1.6f;
                attackRange = 1.8f;
            }

            nextSpecialAttackTime = Time.time + 4.0f; // First special after 4 seconds
        }

        private void SetupBossVisual()
        {
            if (bodyRenderer != null && bossDefinition != null)
            {
                bodyRenderer.color = bossDefinition.tint;
            }
            transform.localScale = Vector3.one * 0.78f;
        }

        private void SetupSelectionRing()
        {
            if (selectionRing != null) return;
            GameObject ring = new GameObject("BossRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            ring.transform.localScale = new Vector3(1.35f, 0.65f, 1f);
            selectionRing = ring.AddComponent<SpriteRenderer>();
            selectionRing.sprite = GetBossRingSprite();
            selectionRing.color = new Color(1f, 0.2f, 0.2f, 0.95f); // Fiery red boss ring
            selectionRing.sortingOrder = -5;
        }

        private IEnumerator EntranceRoutine()
        {
            SetAnimationAction(0);
            CombatVFXPool.SpawnEffect("CriticalBurst", transform.position);

            yield return new WaitForSeconds(0.8f);
        }

        protected override void UpdateBehavior()
        {
            if (isPreparingSpecial || isDead || IsExposed) return;

            // Check Enrage phase
            if (!isEnraged && HealthPercent <= (bossDefinition != null ? bossDefinition.enrageHealthPercent : 0.4f))
            {
                TriggerEnrage();
            }

            // Target nearest pack hero
            currentTarget = FindNearestAlly();

            if (currentTarget != null)
            {
                float dist = Vector2.Distance(transform.position, currentTarget.Position);

                // Check special attack condition
                if (Time.time >= nextSpecialAttackTime && bossDefinition != null && bossDefinition.specialAttacks.Count > 0)
                {
                    StartCoroutine(ExecuteSpecialAttackRoutine());
                    return;
                }

                if (dist <= attackRange)
                {
                    if (Time.time < attackAnimEndTime)
                    {
                        // Maintain attack animation
                        return;
                    }
                    SetAnimationAction(0);
                    if (Time.time >= nextAttackTime)
                    {
                        ExecuteBasicAttack(currentTarget);
                    }
                }
                else
                {
                    MoveTowards(currentTarget.Position);
                }
            }
            else
            {
                // Advance southward towards pack line
                float targetY = (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : transform.position.y) - 2f;
                MoveTowards(new Vector3(transform.position.x, targetY, 0f));
            }
        }

        private void MoveTowards(Vector3 targetPos)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            Vector3 newPos = transform.position + dir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, -1.8f, 1.8f);
            
            // Keep boss in the upper-middle battlefield; do not let boss advance onto the lower card tray
            float minBossY = (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : (Camera.main != null ? Camera.main.transform.position.y : 0f)) + 1.2f;
            newPos.y = Mathf.Max(newPos.y, minBossY);

            transform.position = newPos;

            SetAnimationAction(1); // Walk
        }

        private void ExecuteBasicAttack(Combatant target)
        {
            nextAttackTime = Time.time + attackInterval;
            attackAnimEndTime = Time.time + 0.5f;
            SetAnimationAction(2); // Attack
            if (presentation != null) presentation.Windup(target.Position);
            StartCoroutine(ReleaseBasicAttack(target));
        }
        private IEnumerator ReleaseBasicAttack(Combatant target)
        {
            yield return new WaitForSeconds(.28f);
            if (!IsAlive || target == null || !target.IsAlive || GameDirector.Instance.CurrentState != GameState.Playing) yield break;
            if (Vector2.Distance(Position, target.Position) > attackRange * 1.5f) yield break;
            target.TakeDamage(attackDamage, this);
            if (presentation != null) presentation.Impact(target.Position);
        }

        private IEnumerator ExecuteSpecialAttackRoutine()
        {
            isPreparingSpecial = true;

            var attack = bossDefinition.specialAttacks[currentSpecialIndex % bossDefinition.specialAttacks.Count];
            currentSpecialIndex++;
            nextSpecialAttackTime = Time.time + attack.cooldown * (isEnraged ? 0.75f : 1.0f);

            Vector3 targetPos = currentTarget != null ? currentTarget.Position : transform.position + Vector3.down * 1.5f;

            // Spawn visual telegraph indicator
            string telegraphPrefab = attack.vfxName;
            if (string.IsNullOrEmpty(telegraphPrefab)) telegraphPrefab = "EnemySlamCircle";
            TelegraphIndicator.Spawn(telegraphPrefab, targetPos, Mathf.Max(1.3f, attack.telegraphDuration), attack.radius);

            // Windup stance
            SetAnimationAction(0);
            if (presentation != null) presentation.Windup(targetPos);
            WorldOfThePack.NewUI.PackGameplayHUD.Instance?.ShowBanner("SLAM INCOMING • Switch to GUARD or move out", 1.3f);
            yield return new WaitForSeconds(Mathf.Max(1.3f, attack.telegraphDuration));

            if (isDead || GameDirector.Instance.CurrentState != GameState.Playing || (CampManager.Instance != null && CampManager.Instance.IsAtCamp))
            {
                isPreparingSpecial = false;
                yield break;
            }

            // Execute special strike
            attackAnimEndTime = Time.time + 0.6f;
            SetAnimationAction(2);
            yield return new WaitForSeconds(.24f);
            if (!IsAlive || GameDirector.Instance.CurrentState != GameState.Playing) { isPreparingSpecial = false; yield break; }
            if (presentation != null) presentation.Impact(targetPos);
            WorldOfThePack.Expedition.PackImpactPulse.Spawn(targetPos, new Color(1, .65f, .25f), attack.radius);

            // Area damage on allies inside radius
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var ally in allies)
            {
                if (ally.IsAlive && Vector2.Distance(targetPos, ally.Position) <= attack.radius)
                {
                    ally.TakeDamage(attack.damage, this);
                }
            }

            recoveryUntil = Time.time + 2.5f;
            strikeUsedForRecovery = false;
            WorldOfThePack.NewUI.PackGameplayHUD.Instance?.ShowBanner("BOSS EXPOSED • FOCUS or use PACK STRIKE", 2.2f);
            yield return new WaitForSeconds(0.4f);
            isPreparingSpecial = false;
        }

        private void TriggerEnrage()
        {
            isEnraged = true;
            moveSpeed *= 1.25f;
            attackInterval *= (bossDefinition != null ? 1f / bossDefinition.enrageAttackSpeedMultiplier : 0.75f);

            if (bodyRenderer != null)
            {
                bodyRenderer.color = new Color(1f, 0.4f, 0.4f, 1f); // Rage red
            }

            CombatVFXPool.SpawnEffect("MoonTransformation", transform.position);
            DamagePopupManager.ShowPopup(transform.position + Vector3.up * 1.5f, 0, false, true);
        }

        public override void TakeDamage(float amount, Combatant attacker = null)
        {
            if (!IsAlive) return;
            if (attacker is HeroUnit hero) contributors.Add(hero.HeroId);
            if (IsExposed) amount *= 1.25f;
            base.TakeDamage(amount, attacker);
            OnBossHealthUpdated?.Invoke(currentHealth, maxHealth);
        }

        protected override void Die(Combatant killer)
        {
            base.Die(killer);

            // Defeat effects
            CombatVFXPool.SpawnEffect("EnemyDefeat", transform.position);
            CombatVFXPool.SpawnEffect("MoonTransformation", transform.position + Vector3.up * 0.5f);

            // Resume expedition march
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(true);
            }

            // Distribute rewards
            if (SaveManager.Instance != null && bossDefinition != null)
            {
                SaveManager.Instance.AddMoonShards(bossDefinition.moonShardsReward);
                if (!string.IsNullOrEmpty(bossDefinition.unlockedHeroOnDefeat))
                {
                    SaveManager.Instance.Data.UnlockHero(bossDefinition.unlockedHeroOnDefeat);
                }
                // Chapter progression is owned by StoryCampaignDirector.
            }

            if (GameDirector.Instance != null)
            {
                float reward = bossDefinition != null ? bossDefinition.energyReward : 150f;
                GameDirector.Instance.OnEnemyKilled(this, reward);
                GameDirector.Instance.AddMorale(40f);
            }

            OnBossDefeated?.Invoke(this);
            OnBossHealthUpdated?.Invoke(0f, maxHealth);
        }

        private Combatant FindNearestAlly()
        {
            var allies = Combatant.LivingUnits;
            Combatant best = null;
            float closestDist = float.MaxValue;

            foreach (var ally in allies)
            {
                if (!ally.IsAlive || ally.Faction != UnitFaction.Allies) continue;
                float d = Vector2.Distance(transform.position, ally.Position);
                if (d < closestDist)
                {
                    closestDist = d;
                    best = ally;
                }
            }
            return best;
        }

        private static Sprite cachedBossRingSprite;
        private static Sprite GetBossRingSprite()
        {
            if (cachedBossRingSprite != null) return cachedBossRingSprite;
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            Color clear = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;
            float inner = radius - 8f;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= radius && d >= inner) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, clear);
                }

            tex.Apply();
            cachedBossRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return cachedBossRingSprite;
        }
    }
}
