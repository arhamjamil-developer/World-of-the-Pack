using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.CombatArt;

namespace WorldOfThePack.Game
{
    public enum EnemyType
    {
        Soldier,
        Hound,
        Captain,
        Rudra,
        Elsa
    }

    public class EnemyUnit : Combatant
    {
        [Header("Enemy Identity")]
        [SerializeField] private EnemyType enemyType = EnemyType.Soldier;
        [SerializeField] private float energyReward = 25f;
        [SerializeField] private float breachDamage = 15f;
        [SerializeField] private EnemyDefinition enemyDefinition;

        [Header("Telegraph Attack")]
        [SerializeField] private bool hasTelegraph = false;
        [SerializeField] private float telegraphDuration = 1.0f;
        [SerializeField] private string telegraphPrefabName = "EnemyConeSouth";

        private bool isPreparingAttack;
        private UnitAttackPresentation attackPresentation;
        private static readonly List<EnemyUnit> Living = new List<EnemyUnit>();
        public static IReadOnlyList<EnemyUnit> ActiveEnemies => Living;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => Living.Clear();
        protected override void OnEnable() { base.OnEnable(); if (!Living.Contains(this)) Living.Add(this); }
        protected override void OnDisable() { base.OnDisable(); Living.Remove(this); }

        public EnemyType Type => enemyType;
        public float EnemyScale => enemyType switch
        {
            EnemyType.Captain => 0.52f,
            EnemyType.Hound => 0.36f,
            EnemyType.Rudra => 0.50f,
            _ => 0.42f
        };

        private Vector2 entryDirection = Vector2.down;
        public void SetEntryDirection(Vector2 dir)
        {
            entryDirection = dir.normalized;
        }

        protected override void Awake()
        {
            faction = UnitFaction.Hunters;
            base.Awake();
            attackPresentation = GetComponent<UnitAttackPresentation>();

            transform.localScale = Vector3.one * EnemyScale;
            ConfigureEnemyStats();
            if (ArenaDirector.Instance != null && enemyType != EnemyType.Elsa) attackRange = .76f;
            SetupEnemyVisual();
            SetupSelectionRing();
        }

        public void ApplyWaveDifficulty(int waveIndex)
        {
            float growth = 1f + Mathf.Max(0, waveIndex - 2) * .12f;
            maxHealth *= growth; currentHealth = maxHealth;
            attackDamage *= 1f + Mathf.Max(0, waveIndex - 2) * .08f;
            UpdateHealthBarUI();
        }

        private void ConfigureEnemyStats()
        {
            if (enemyDefinition != null)
            {
                unitName = enemyDefinition.enemyName;
                maxHealth = currentHealth = enemyDefinition.maxHealth;
                moveSpeed = enemyDefinition.moveSpeed; attackDamage = enemyDefinition.attackDamage;
                attackRange = enemyDefinition.attackRange; attackInterval = enemyDefinition.attackInterval;
                energyReward = enemyDefinition.energyReward; breachDamage = enemyDefinition.breachDamage;
                hasTelegraph = enemyDefinition.hasTelegraph;
                if (hasTelegraph && enemyDefinition.telegraphAttack != null)
                {
                    telegraphDuration = enemyDefinition.telegraphAttack.telegraphDuration;
                    telegraphPrefabName = enemyDefinition.telegraphAttack.vfxName;
                }
                UpdateHealthBarUI(); return;
            }
            switch (enemyType)
            {
                case EnemyType.Hound:
                    unitName = "Hunter Hound";
                    maxHealth = 45f;
                    currentHealth = 45f;
                    moveSpeed = 2.1f;
                    attackRange = 1.1f;
                    attackDamage = 10f;
                    attackInterval = 0.85f;
                    energyReward = 20f;
                    breachDamage = 10f;
                    break;
                case EnemyType.Captain:
                    unitName = "Patrol Captain";
                    maxHealth = 220f;
                    currentHealth = 220f;
                    moveSpeed = 0.85f;
                    attackRange = 1.5f;
                    attackDamage = 35f;
                    attackInterval = 2.2f;
                    energyReward = 60f;
                    breachDamage = 30f;
                    hasTelegraph = true;
                    telegraphDuration = 1.2f;
                    telegraphPrefabName = "EnemySlamCircle";
                    break;
                case EnemyType.Rudra:
                    unitName = "Rudra the Guard";
                    maxHealth = 180f;
                    currentHealth = 180f;
                    moveSpeed = 0.95f;
                    attackRange = 1.4f;
                    attackDamage = 25f;
                    attackInterval = 1.8f;
                    energyReward = 40f;
                    breachDamage = 20f;
                    hasTelegraph = true;
                    telegraphDuration = 1.0f;
                    telegraphPrefabName = "EnemySlamCircle";
                    break;
                case EnemyType.Elsa:
                    unitName = "Queen Archer Elsa";
                    maxHealth = 90f;
                    currentHealth = 90f;
                    moveSpeed = 1.1f;
                    attackRange = 4.5f;
                    attackDamage = 18f;
                    attackInterval = 1.6f;
                    energyReward = 35f;
                    breachDamage = 15f;
                    break;
                default: // Soldier
                    enemyType = EnemyType.Soldier;
                    unitName = "Forest Patrol Soldier";
                    maxHealth = 80f;
                    currentHealth = 80f;
                    moveSpeed = 1.25f;
                    attackRange = 1.2f;
                    attackDamage = 15f;
                    attackInterval = 1.3f;
                    energyReward = 25f;
                    breachDamage = 15f;
                    break;
            }
        }

        private float attackAnimEndTime = 0f;

        private void SetupEnemyVisual()
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.color = Color.white;
            }
        }

        private void SetupSelectionRing()
        {
            if (selectionRing != null) return;
            GameObject ring = new GameObject("EnemyRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            ring.transform.localScale = new Vector3(0.38f, 0.24f, 1f);
            selectionRing = ring.AddComponent<SpriteRenderer>();
            selectionRing.sprite = GetEnemyRingSprite();
            selectionRing.color = Color.white;
            selectionRing.sortingOrder = -5;
        }

        protected override void UpdateBehavior()
        {
            if (isPreparingAttack || isDead) return;
            if (Time.time < attackAnimEndTime) return;

            // Check if breached past the formation anchor or behind the pack line (strictly above cards)
            float anchorY = FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : (Camera.main != null ? Camera.main.transform.position.y : 0f);
            float breachLimit = anchorY - 1.2f;
            if (ArenaDirector.Instance == null && transform.position.y <= breachLimit)
            {
                BreachPackLine();
                return;
            }

            currentTarget = FindNearestAlly();

            if (currentTarget != null)
            {
                float distance = Vector2.Distance(transform.position, currentTarget.Position);
                bool aligned=enemyType==EnemyType.Elsa || (Mathf.Abs(Position.x-currentTarget.Position.x)<.48f && Position.y>=currentTarget.Position.y-.1f);
                if (distance <= attackRange && aligned)
                {
                    if (Time.time < attackAnimEndTime)
                    {
                        // Maintain attack animation smoothly
                        return;
                    }
                    SetAnimationAction(0);
                    if (Time.time >= nextAttackTime)
                    {
                        if (hasTelegraph)
                        {
                            StartCoroutine(ExecuteTelegraphedAttack(currentTarget));
                        }
                        else
                        {
                            ExecuteAttack(currentTarget);
                        }
                    }
                }
                else
                {
                    MoveTowards(enemyType==EnemyType.Elsa ? currentTarget.Position : currentTarget.Position+Vector3.up*.64f);
                }
            }
            else
            {
                // Advance southward along entry direction towards formation
                Vector3 dir = new Vector3(entryDirection.x, entryDirection.y, 0f);
                MoveTowards(transform.position + dir * 2f);
            }
        }

        private void MoveTowards(Vector3 targetPos)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            Vector3 newPos = transform.position + dir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, -1.8f, 1.8f);
            transform.position = newPos;

            SetAnimationAction(1); // Walk
            float strideRate = Mathf.Clamp(moveSpeed / 1.15f, 0.4f, 2.0f);
            SetAnimationPlaybackSpeed(strideRate);
        }

        private void ExecuteAttack(Combatant target)
        {
            if (attackPresentation != null) attackPresentation.Windup(target.Position);
            nextAttackTime = Time.time + attackInterval;
            float duration = attackPresentation != null ? attackPresentation.Duration : .6f;
            attackAnimEndTime = Time.time + duration;
            SetAnimationAction(2); // Attack
            SetAnimationPlaybackSpeed(1);
            StartCoroutine(ReleaseAttack(target));
        }

        private IEnumerator ReleaseAttack(Combatant target)
        {
            yield return new WaitForSeconds(attackPresentation != null ? attackPresentation.ReleaseDelay : .3f);
            if (!IsAlive || target == null || !target.IsAlive) yield break;
            if (GameDirector.Instance == null || GameDirector.Instance.CurrentState != GameState.Playing ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) yield break;
            if (enemyType == EnemyType.Elsa && attackPresentation != null)
                attackPresentation.Shoot(this, target, attackDamage);
            else if (Vector2.Distance(Position, target.Position) <= attackRange * 1.5f)
            {
                target.TakeDamage(attackDamage, this);
                if (attackPresentation != null) attackPresentation.Impact(target.Position);
                else CombatVFXPool.SpawnEffect("CriticalBurst", target.Position + Vector3.up * .2f);
            }
        }

        private IEnumerator ExecuteTelegraphedAttack(Combatant target)
        {
            isPreparingAttack = true;
            nextAttackTime = Time.time + attackInterval + telegraphDuration;

            TelegraphIndicator.Spawn(telegraphPrefabName, transform.position, telegraphDuration, attackRange);

            yield return new WaitForSeconds(telegraphDuration);

            isPreparingAttack = false;
            if (isDead) yield break;

            if (target != null && target.IsAlive && Vector2.Distance(transform.position, target.Position) <= attackRange)
            {
                ExecuteAttack(target);
            }
        }

        public override void TakeDamage(float amount, Combatant attacker = null)
        {
            // Rudra intercepts a quarter of nearby allies' damage; intercepted damage cannot recurse.
            if (enemyType != EnemyType.Rudra && IsAlive && amount > 0)
            {
                foreach (var guardian in Living)
                {
                    if (guardian == null || guardian.enemyType != EnemyType.Rudra || !guardian.IsAlive ||
                        Vector2.Distance(Position, guardian.Position) > 1.6f) continue;
                    guardian.AbsorbGuardDamage(amount * .25f, attacker);
                    amount *= .75f; break;
                }
            }
            base.TakeDamage(amount, attacker);
        }
        private void AbsorbGuardDamage(float amount, Combatant attacker) => base.TakeDamage(amount, attacker);

        private void BreachPackLine()
        {
            if (isDead) return;
            isDead = true;

            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.DamagePack(breachDamage);
            }

            CombatVFXPool.SpawnEffect("CriticalBurst", transform.position);
            RaiseDefeated();
            Destroy(gameObject);
        }

        protected override void Die(Combatant killer)
        {
            base.Die(killer);

            CombatVFXPool.SpawnEffect("EnemyDefeat", transform.position);

            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.OnEnemyKilled(this, energyReward);
            }
        }

        private Combatant FindNearestAlly()
        {
            Combatant best = null;
            float closestDist = float.MaxValue;
            foreach (var ally in Combatant.LivingUnits)
            {
                if (!ally.IsAlive || ally.Faction != UnitFaction.Allies) continue;
                float d = Vector2.Distance(transform.position, ally.Position);
                d += Mathf.Abs(transform.position.x-ally.Position.x)*.55f;
                // Defenders intercept attackers before they reach the throne.
                if (ally is QueenEscort) d += 1.1f;
                if (d < closestDist) { closestDist = d; best = ally; }
            }
            return best;
        }

        private static Sprite cachedEnemyRingSprite;
        private static Sprite GetEnemyRingSprite()
        {
            if (cachedEnemyRingSprite != null) return cachedEnemyRingSprite;
#if UNITY_EDITOR
            var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VFX/WorldOfThePack/Art/GroundRing_EnemyOrange_HD.png");
            if (loaded != null) { cachedEnemyRingSprite = loaded; return cachedEnemyRingSprite; }
#endif
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            Color clear = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;
            float inner = radius - 6f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= radius && d >= inner) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            cachedEnemyRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return cachedEnemyRingSprite;
        }
    }
}
