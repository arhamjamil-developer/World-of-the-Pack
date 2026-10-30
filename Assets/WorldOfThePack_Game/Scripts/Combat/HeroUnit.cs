using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class HeroUnit : Combatant
    {
        [Header("Hero Configuration")]
        [SerializeField] private string heroId = "HeroWolvy";
        [SerializeField] private bool isWolfForm = true;
        [SerializeField] private GameObject humanVisual;
        [SerializeField] private GameObject wolfVisual;

        [Header("Hero Roles & Abilities")]
        [SerializeField] private bool isRanged = false;
        [SerializeField] private bool isHealer = false;
        [SerializeField] private float healAmount = 15f;
        [SerializeField] private string attackVfxName = "MoonlightSlash";

        public string HeroId => heroId;
        public bool IsWolfForm => isWolfForm;

        protected override void Awake()
        {
            faction = UnitFaction.Allies;
            base.Awake();

            ConfigureHeroDefaults();
            SetupFormVisuals();
            SetupSelectionRing();
        }

        private void ConfigureHeroDefaults()
        {
            switch (heroId)
            {
                case "PrincessElva":
                    unitName = "Princess Elva";
                    attackRange = 4.5f;
                    moveSpeed = 1.4f;
                    attackDamage = 18f;
                    attackInterval = 1.3f;
                    isRanged = true;
                    attackVfxName = "NatureArrow";
                    break;
                case "CaptainBobo":
                    unitName = "Captain Bobo";
                    maxHealth = 180f;
                    currentHealth = 180f;
                    attackRange = 1.2f;
                    moveSpeed = 1.2f;
                    attackDamage = 22f;
                    attackInterval = 1.5f;
                    attackVfxName = "EarthImpact";
                    break;
                case "FairyLulu":
                    unitName = "Fairy Lulu";
                    attackRange = 3.5f;
                    moveSpeed = 1.5f;
                    attackDamage = 12f;
                    attackInterval = 1.8f;
                    isHealer = true;
                    attackVfxName = "HealingCircle";
                    break;
                case "ScoutPip":
                    unitName = "Scout Pip";
                    attackRange = 1.3f;
                    moveSpeed = 2.4f;
                    attackDamage = 16f;
                    attackInterval = 0.9f;
                    attackVfxName = "WindStrike";
                    break;
                case "WizardMomo":
                    unitName = "Wizard Momo";
                    attackRange = 4.0f;
                    moveSpeed = 1.3f;
                    attackDamage = 24f;
                    attackInterval = 1.6f;
                    isRanged = true;
                    attackVfxName = "StarShower";
                    break;
                case "TinkerTilly":
                    unitName = "Tinker Tilly";
                    attackRange = 3.0f;
                    moveSpeed = 1.6f;
                    attackDamage = 17f;
                    attackInterval = 1.4f;
                    attackVfxName = "BrassImpact";
                    break;
                default: // HeroWolvy
                    heroId = "HeroWolvy";
                    unitName = "Hero Wolvy";
                    maxHealth = 120f;
                    currentHealth = 120f;
                    attackRange = 1.4f;
                    moveSpeed = 1.8f;
                    attackDamage = 25f;
                    attackInterval = 1.1f;
                    attackVfxName = "MoonlightSlash";
                    break;
            }
        }

        private void SetupFormVisuals()
        {
            if (wolfVisual != null) wolfVisual.SetActive(isWolfForm);
            if (humanVisual != null) humanVisual.SetActive(!isWolfForm);

            // Re-bind animator to active visual
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Transform bodyChild = animator.transform.Find("Body");
                if (bodyChild != null) bodyRenderer = bodyChild.GetComponent<SpriteRenderer>();
            }
        }

        public void SetForm(bool wolf)
        {
            if (isWolfForm == wolf) return;
            isWolfForm = wolf;
            SetupFormVisuals();
            CombatVFXPool.SpawnEffect("MoonTransformation", transform.position);
        }

        private void SetupSelectionRing()
        {
            if (selectionRing != null) return;
            GameObject ring = new GameObject("AllyRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            ring.transform.localScale = new Vector3(0.9f, 0.45f, 1f);
            selectionRing = ring.AddComponent<SpriteRenderer>();
            selectionRing.sprite = GetRingSprite();
            selectionRing.color = new Color(0.25f, 0.65f, 1f, 0.65f); // Team blue ring
            selectionRing.sortingOrder = -5;
        }

        protected override void UpdateBehavior()
        {
            if (isHealer)
            {
                UpdateHealerBehavior();
                return;
            }

            // Find nearest enemy in front
            currentTarget = FindNearestEnemy();

            if (currentTarget != null)
            {
                float distance = Vector2.Distance(transform.position, currentTarget.Position);
                if (distance <= attackRange)
                {
                    // In attack range
                    SetAnimationAction(0); // Idle/Stand ground
                    if (Time.time >= nextAttackTime)
                    {
                        PerformAttack(currentTarget);
                    }
                }
                else
                {
                    // Move towards target
                    MoveTowards(currentTarget.Position);
                }
            }
            else
            {
                // Advance slowly northward if lane is clear
                if (transform.position.y < 0.5f)
                {
                    MoveTowards(new Vector3(transform.position.x, 0.5f, 0f));
                }
                else
                {
                    SetAnimationAction(0); // Hold formation
                }
            }
        }

        private void UpdateHealerBehavior()
        {
            // Find most wounded ally
            HeroUnit woundedAlly = FindMostWoundedAlly();
            if (woundedAlly != null && woundedAlly.HealthPercent < 0.85f && Time.time >= nextAttackTime)
            {
                SetAnimationAction(4); // Special heal pose
                CombatVFXPool.SpawnEffect("HealingCircle", woundedAlly.Position);
                woundedAlly.Heal(healAmount);
                nextAttackTime = Time.time + attackInterval;
                return;
            }

            // Fallback to basic attack against nearest enemy
            currentTarget = FindNearestEnemy();
            if (currentTarget != null && Vector2.Distance(transform.position, currentTarget.Position) <= attackRange)
            {
                if (Time.time >= nextAttackTime) PerformAttack(currentTarget);
            }
            else
            {
                if (transform.position.y < 0f) MoveTowards(new Vector3(transform.position.x, 0f, 0f));
                else SetAnimationAction(0);
            }
        }

        private void MoveTowards(Vector3 targetPos)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            // Restrict horizontal corridor bounds (-1.8 to 1.8)
            Vector3 newPos = transform.position + dir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, -1.8f, 1.8f);
            transform.position = newPos;

            SetAnimationAction(1); // Walk
        }

        private void PerformAttack(Combatant target)
        {
            nextAttackTime = Time.time + attackInterval;
            SetAnimationAction(2); // Attack

            if (isRanged)
            {
                // Spawn projectile or direct ranged burst
                Projectile.SpawnProjectile(transform.position + Vector3.up * 0.4f, target, attackDamage, attackVfxName);
            }
            else
            {
                // Melee strike
                target.TakeDamage(attackDamage, this);
                CombatVFXPool.SpawnEffect(attackVfxName, target.Position + Vector3.up * 0.2f);
            }
        }

        private Combatant FindNearestEnemy()
        {
            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            Combatant best = null;
            float closestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                float d = Vector2.Distance(transform.position, enemy.Position);
                if (d < closestDist)
                {
                    closestDist = d;
                    best = enemy;
                }
            }
            return best;
        }

        private HeroUnit FindMostWoundedAlly()
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            HeroUnit best = null;
            float lowestPct = 1f;

            foreach (var ally in allies)
            {
                if (!ally.IsAlive || ally == this) continue;
                if (ally.HealthPercent < lowestPct)
                {
                    lowestPct = ally.HealthPercent;
                    best = ally;
                }
            }
            return best;
        }

        private static Sprite cachedRingSprite;
        private static Sprite GetRingSprite()
        {
            if (cachedRingSprite != null) return cachedRingSprite;
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
            cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return cachedRingSprite;
        }
    }
}
