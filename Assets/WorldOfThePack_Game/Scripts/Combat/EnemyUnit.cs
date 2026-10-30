using System.Collections;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public enum EnemyType
    {
        Soldier,
        Hound,
        Captain
    }

    public class EnemyUnit : Combatant
    {
        [Header("Enemy Identity")]
        [SerializeField] private EnemyType enemyType = EnemyType.Soldier;
        [SerializeField] private float energyReward = 25f;
        [SerializeField] private float breachDamage = 15f;
        [SerializeField] private float breachYThreshold = -3.8f;

        [Header("Telegraph Attack")]
        [SerializeField] private bool hasTelegraph = false;
        [SerializeField] private float telegraphDuration = 1.0f;
        [SerializeField] private string telegraphPrefabName = "EnemyConeSouth";

        private bool isPreparingAttack;

        public EnemyType Type => enemyType;

        protected override void Awake()
        {
            faction = UnitFaction.Hunters;
            base.Awake();

            ConfigureEnemyStats();
            SetupEnemyVisual();
            SetupSelectionRing();
        }

        private void ConfigureEnemyStats()
        {
            switch (enemyType)
            {
                case EnemyType.Hound:
                    unitName = "Hunter Hound";
                    maxHealth = 45f;
                    currentHealth = 45f;
                    moveSpeed = 2.2f;
                    attackRange = 1.1f;
                    attackDamage = 10f;
                    attackInterval = 0.8f;
                    energyReward = 20f;
                    breachDamage = 10f;
                    break;
                case EnemyType.Captain:
                    unitName = "Patrol Captain";
                    maxHealth = 220f;
                    currentHealth = 220f;
                    moveSpeed = 0.9f;
                    attackRange = 1.5f;
                    attackDamage = 35f;
                    attackInterval = 2.2f;
                    energyReward = 60f;
                    breachDamage = 30f;
                    hasTelegraph = true;
                    telegraphDuration = 1.2f;
                    telegraphPrefabName = "EnemySlamCircle";
                    break;
                default: // Soldier
                    enemyType = EnemyType.Soldier;
                    unitName = "Forest Patrol Soldier";
                    maxHealth = 80f;
                    currentHealth = 80f;
                    moveSpeed = 1.3f;
                    attackRange = 1.2f;
                    attackDamage = 15f;
                    attackInterval = 1.3f;
                    energyReward = 25f;
                    breachDamage = 15f;
                    break;
            }
        }

        private void SetupEnemyVisual()
        {
            // If body renderer exists, give it a distinctive cartoon hunter coloring
            if (bodyRenderer != null)
            {
                // Invert scale Y slightly or flip to face southwards toward the pack
                // If using upright sprite, facing south or distinct tint
                bodyRenderer.color = enemyType switch
                {
                    EnemyType.Captain => new Color(1f, 0.7f, 0.7f, 1f),
                    EnemyType.Hound => new Color(0.9f, 0.8f, 0.7f, 1f),
                    _ => new Color(0.95f, 0.85f, 0.85f, 1f)
                };
            }
        }

        private void SetupSelectionRing()
        {
            if (selectionRing != null) return;
            GameObject ring = new GameObject("EnemyRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            ring.transform.localScale = new Vector3(0.95f, 0.45f, 1f);
            selectionRing = ring.AddComponent<SpriteRenderer>();
            selectionRing.sprite = GetEnemyRingSprite();
            selectionRing.color = new Color(1f, 0.4f, 0.1f, 0.75f); // Team orange/red ring
            selectionRing.sortingOrder = -5;
        }

        protected override void UpdateBehavior()
        {
            if (isPreparingAttack) return;

            // Check if breached the defence line
            if (transform.position.y <= breachYThreshold)
            {
                BreachPackLine();
                return;
            }

            currentTarget = FindNearestAlly();

            if (currentTarget != null)
            {
                float distance = Vector2.Distance(transform.position, currentTarget.Position);
                if (distance <= attackRange)
                {
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
                    MoveTowards(currentTarget.Position);
                }
            }
            else
            {
                // Advance southward down the corridor
                MoveTowards(new Vector3(transform.position.x, breachYThreshold - 1f, 0f));
            }
        }

        private void MoveTowards(Vector3 targetPos)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            Vector3 newPos = transform.position + dir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, -1.8f, 1.8f);
            transform.position = newPos;

            SetAnimationAction(1); // Walk
        }

        private void ExecuteAttack(Combatant target)
        {
            nextAttackTime = Time.time + attackInterval;
            SetAnimationAction(2); // Attack
            target.TakeDamage(attackDamage, this);
            CombatVFXPool.SpawnEffect("CriticalBurst", target.Position + Vector3.up * 0.2f);
        }

        private IEnumerator ExecuteTelegraphedAttack(Combatant target)
        {
            isPreparingAttack = true;
            nextAttackTime = Time.time + attackInterval + telegraphDuration;

            // Spawn warning telegraph
            GameObject telegraph = TelegraphIndicator.Spawn(telegraphPrefabName, transform.position, telegraphDuration);

            yield return new WaitForSeconds(telegraphDuration);

            isPreparingAttack = false;
            if (isDead) yield break;

            if (target != null && target.IsAlive && Vector2.Distance(transform.position, target.Position) <= attackRange * 1.5f)
            {
                SetAnimationAction(2);
                target.TakeDamage(attackDamage, this);
                CombatVFXPool.SpawnEffect("CriticalBurst", target.Position);
            }
        }

        private void BreachPackLine()
        {
            if (isDead) return;
            isDead = true;

            // Damage shared pack health
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.DamagePack(breachDamage);
            }

            CombatVFXPool.SpawnEffect("CriticalBurst", transform.position);
            Destroy(gameObject);
        }

        protected override void Die(Combatant killer)
        {
            base.Die(killer);

            // Spawn cartoon defeat puff VFX
            CombatVFXPool.SpawnEffect("EnemyDefeat", transform.position);

            // Reward energy to player card deck
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.OnEnemyKilled(this, energyReward);
            }
        }

        private Combatant FindNearestAlly()
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            Combatant best = null;
            float closestDist = float.MaxValue;

            foreach (var ally in allies)
            {
                if (!ally.IsAlive) continue;
                float d = Vector2.Distance(transform.position, ally.Position);
                if (d < closestDist)
                {
                    closestDist = d;
                    best = ally;
                }
            }
            return best;
        }

        private static Sprite cachedEnemyRingSprite;
        private static Sprite GetEnemyRingSprite()
        {
            if (cachedEnemyRingSprite != null) return cachedEnemyRingSprite;
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
                    if (d <= radius && d >= inner)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            cachedEnemyRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return cachedEnemyRingSprite;
        }
    }
}
