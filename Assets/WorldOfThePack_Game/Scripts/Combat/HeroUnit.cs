using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.VFX;

namespace WorldOfThePack.Game
{
    public enum HeroState
    {
        Idle,
        Marching,
        JoiningFormation,
        DetectingEnemy,
        ChasingEnemy,
        Attacking,
        CastingSpecial,
        ReturningToFormation,
        Transforming,
        Dead
    }

    public class HeroUnit : Combatant
    {
        [Header("Hero Configuration")]
        [SerializeField] private string heroId = "HeroWolvy";
        [SerializeField] private HeroKitDefinition heroKit;
        [SerializeField] private bool isAlpha = false;
        [SerializeField] private int assignedLaneIndex = 2;
        [SerializeField] private bool isWolfForm = false;
        [SerializeField] private GameObject humanVisual;
        [SerializeField] private GameObject wolfVisual;

        [Header("Combat & Formation Settings")]
        [SerializeField] private float agroRadius = 3.5f;
        [SerializeField] private float maxChaseDistance = 2.4f;

        [Header("Wolf Surge (20s Boost)")]
        [SerializeField] private float wolfSurgeDuration = 20f;
        private float wolfSurgeTimer = 0f;
        private bool isSurgingWolf = false;

        private float baseAttackDamage = 20f;
        private float baseAttackInterval = 1.2f;
        private float baseMoveSpeed = 1.4f;
        private float baseMaxHealth = 120f;

        private float special01CooldownTimer = 0f;
        private float special02CooldownTimer = 0f;
        private float passiveTimer = 0f;
        private float attackAnimEndTime = 0f;

        private HeroState currentState = HeroState.JoiningFormation;
        private FormationSlot assignedSlot;
        private GameObject alphaCrownVisual;

        public string HeroId => heroId;
        public HeroKitDefinition Kit => heroKit;
        public bool IsAlpha => isAlpha;
        public int AssignedLaneIndex => assignedLaneIndex;
        public bool IsWolfForm => isWolfForm;
        public bool IsSurgingWolf => isSurgingWolf;
        public float WolfSurgeRemaining => Mathf.Max(0f, wolfSurgeTimer);
        public float WolfSurgePercent => Mathf.Clamp01(wolfSurgeTimer / Mathf.Max(1f, wolfSurgeDuration));
        public HeroState CurrentState => currentState;
        public bool IsEngagedInCombat => currentState == HeroState.DetectingEnemy ||
                                         currentState == HeroState.ChasingEnemy ||
                                         currentState == HeroState.Attacking ||
                                         currentState == HeroState.CastingSpecial;

        public FormationRole Role
        {
            get
            {
                if (heroKit != null) return heroKit.formationRole;
                if (heroId == "FairyLulu") return FormationRole.Support;
                if (heroId == "PrincessElva") return FormationRole.Ranger;
                if (heroId == "ScoutPip") return FormationRole.Skirmisher;
                if (heroId == "WizardMomo") return FormationRole.Mage;
                if (heroId == "TinkerTilly") return FormationRole.Ranger;
                return FormationRole.Tank;
            }
        }

        public float FormationYOffset
        {
            get
            {
                if (isAlpha) return 0.75f;
                return Role switch
                {
                    FormationRole.Tank => 0.75f,
                    FormationRole.Skirmisher => 0.20f,
                    FormationRole.Ranger => -0.45f,
                    FormationRole.Support => -0.95f,
                    FormationRole.Mage => -0.95f,
                    _ => 0.0f
                };
            }
        }

        public event Action<bool, float> OnWolfSurgeChanged;

        protected override void Awake()
        {
            faction = UnitFaction.Allies;
            base.Awake();

            LoadHeroKitIfNeeded();
            ConfigureHeroFromKit();

            baseAttackDamage = attackDamage;
            baseAttackInterval = attackInterval;
            baseMoveSpeed = moveSpeed;
            baseMaxHealth = maxHealth;

            isWolfForm = false;
            SetupFormVisuals();
            SetupSelectionRing();
        }

        protected override void Start()
        {
            base.Start();

            if (FormationController.Instance != null)
            {
                assignedSlot = FormationController.Instance.AssignHero(this);
            }

            StartCoroutine(SpawnSequenceRoutine());
        }

        private void LoadHeroKitIfNeeded()
        {
            if (heroKit == null)
            {
#if UNITY_EDITOR
                string path = $"Assets/WorldOfThePack_Game/Data/HeroKits/{heroId}_Kit.asset";
                heroKit = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(path);
#endif
            }
        }

        private void ConfigureHeroFromKit()
        {
            if (heroKit != null)
            {
                unitName = heroKit.displayName;
                maxHealth = heroKit.maxHealth;
                currentHealth = heroKit.maxHealth;
                moveSpeed = heroKit.moveSpeed;
                attackDamage = heroKit.attackDamage;
                attackInterval = heroKit.attackInterval;
                attackRange = heroKit.attackRange;
                agroRadius = heroKit.agroRadius;
                assignedLaneIndex = heroKit.preferredLane;
            }
            else
            {
                unitName = heroId;
                maxHealth = 120f;
                currentHealth = 120f;
                moveSpeed = 1.4f;
                attackDamage = 20f;
                attackInterval = 1.2f;
                attackRange = 1.4f;
                agroRadius = 3.5f;
            }
        }

        private IEnumerator SpawnSequenceRoutine()
        {
            currentState = HeroState.JoiningFormation;
            Vector3 targetScale = Vector3.one;
            transform.localScale = Vector3.zero;

            CombatVFXPool.SpawnEffect("MagicImpact", transform.position);

            float elapsed = 0f;
            float duration = 0.35f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * (1f + 0.15f * (1f - t));
                transform.localScale = targetScale * scale;
                yield return null;
            }
            transform.localScale = targetScale;

            SetAnimationAction(0); // Idle
            yield return new WaitForSeconds(0.2f);
        }

        private void SetupFormVisuals()
        {
            if (wolfVisual != null) wolfVisual.SetActive(isWolfForm);
            if (humanVisual != null) humanVisual.SetActive(!isWolfForm);

            animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Transform bodyChild = animator.transform.Find("Body");
                if (bodyChild != null) bodyRenderer = bodyChild.GetComponent<SpriteRenderer>();
                if (bodyRenderer == null) bodyRenderer = animator.GetComponentInChildren<SpriteRenderer>();
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.enabled = true;
                bodyRenderer.color = Color.white;
            }
        }

        public void SetForm(bool wolf)
        {
            if (isWolfForm == wolf) return;
            StartCoroutine(TransformRoutine(wolf));
        }

        private IEnumerator TransformRoutine(bool wolf)
        {
            HeroState prevState = currentState;
            currentState = HeroState.Transforming;
            SetAnimationAction(0); // Idle

            CombatVFXPool.SpawnEffect("MoonTransformation", transform.position + Vector3.up * 0.4f);
            yield return new WaitForSeconds(0.2f);

            isWolfForm = wolf;
            SetupFormVisuals();

            if (heroKit != null)
            {
                if (isWolfForm)
                {
                    attackDamage = baseAttackDamage * heroKit.wolfDamageMultiplier;
                    attackInterval = baseAttackInterval * heroKit.wolfAttackIntervalMultiplier;
                    moveSpeed = baseMoveSpeed * heroKit.wolfSpeedMultiplier;
                }
                else
                {
                    attackDamage = baseAttackDamage;
                    attackInterval = baseAttackInterval;
                    moveSpeed = baseMoveSpeed;
                }
            }

            yield return new WaitForSeconds(0.15f);
            if (currentState == HeroState.Transforming)
            {
                currentState = prevState == HeroState.Transforming ? HeroState.Marching : prevState;
            }
        }

        public void SetAssignedLane(int laneIndex)
        {
            assignedLaneIndex = Mathf.Clamp(laneIndex, 0, LaneSystem.LaneCount - 1);
        }

        public void SetAlpha(bool alpha)
        {
            isAlpha = alpha;
            if (alpha)
            {
                assignedLaneIndex = 2;
                SetupAlphaVisuals();
                if (selectionRing != null)
                {
                    selectionRing.color = new Color(1f, 0.85f, 0.2f, 0.95f);
                }
            }
            else if (alphaCrownVisual != null)
            {
                alphaCrownVisual.SetActive(false);
            }
        }

        public void SetDamageMultiplier(float multiplier)
        {
            float wolfMul = isSurgingWolf ? 2.0f : (isWolfForm ? 1.5f : 1.0f);
            attackDamage = baseAttackDamage * multiplier * wolfMul;
        }

        private void SetupAlphaVisuals()
        {
            if (alphaCrownVisual != null)
            {
                alphaCrownVisual.SetActive(true);
                return;
            }

            alphaCrownVisual = new GameObject("AlphaCrown");
            alphaCrownVisual.transform.SetParent(transform, false);
            alphaCrownVisual.transform.localPosition = new Vector3(0f, 1.45f, 0f);

            SpriteRenderer sr = alphaCrownVisual.AddComponent<SpriteRenderer>();
            sr.sprite = GetAlphaCrownSprite();
            sr.sortingOrder = bodyRenderer != null ? bodyRenderer.sortingOrder + 5 : 350;
            sr.color = new Color(1f, 0.88f, 0.25f, 0.95f);
            alphaCrownVisual.transform.localScale = Vector3.one * 0.45f;
        }

        protected override void UpdateSortingOrder()
        {
            base.UpdateSortingOrder();
            if (alphaCrownVisual != null && bodyRenderer != null)
            {
                var sr = alphaCrownVisual.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = bodyRenderer.sortingOrder + 5;
            }
        }

        private static Sprite cachedCrownSprite;
        private static Sprite GetAlphaCrownSprite()
        {
            if (cachedCrownSprite != null) return cachedCrownSprite;
            int w = 32, h = 24;
            Texture2D tex = new Texture2D(w, h);
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.88f, 0.22f, 1f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            for (int x = 4; x < w - 4; x++)
                for (int y = 2; y <= 6; y++) tex.SetPixel(x, y, gold);

            for (int y = 6; y < 16; y++)
            {
                for (int x = 6; x <= 9; x++) tex.SetPixel(x, y, gold);
                for (int x = 22; x <= 25; x++) tex.SetPixel(x, y, gold);
            }
            for (int y = 6; y < 22; y++)
                for (int x = 14; x <= 17; x++) tex.SetPixel(x, y, gold);

            tex.Apply();
            cachedCrownSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
            return cachedCrownSprite;
        }

        public void ActivateWolfSurge(float duration = 20f)
        {
            if (!IsAlive) return;

            wolfSurgeDuration = duration;
            wolfSurgeTimer = duration;
            isSurgingWolf = true;

            SetForm(true);

            attackDamage = baseAttackDamage * (heroKit != null ? heroKit.wolfDamageMultiplier * 1.5f : 2.5f);
            attackInterval = baseAttackInterval * 0.6f;
            moveSpeed = baseMoveSpeed * 1.35f;

            if (selectionRing != null)
            {
                selectionRing.color = new Color(1f, 0.85f, 0.2f, 0.95f);
            }

            CombatVFXPool.SpawnEffect("MoonTransformation", transform.position + Vector3.up * 0.5f);
            DamagePopupManager.ShowPopup(transform.position + Vector3.up * 1.2f, 0, false, true);

            OnWolfSurgeChanged?.Invoke(true, wolfSurgeDuration);
        }

        public void DeactivateWolfSurge()
        {
            if (!isSurgingWolf) return;

            isSurgingWolf = false;
            wolfSurgeTimer = 0f;

            SetForm(false);

            attackDamage = baseAttackDamage;
            attackInterval = baseAttackInterval;
            moveSpeed = baseMoveSpeed;

            if (selectionRing != null)
            {
                selectionRing.color = new Color(0.25f, 0.65f, 1f, 0.65f);
            }

            CombatVFXPool.SpawnEffect("MagicImpact", transform.position + Vector3.up * 0.3f);
            OnWolfSurgeChanged?.Invoke(false, 0f);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsAlive) return;

            if (special01CooldownTimer > 0f) special01CooldownTimer -= Time.deltaTime;
            if (special02CooldownTimer > 0f) special02CooldownTimer -= Time.deltaTime;

            UpdatePassiveAura();

            if (isSurgingWolf)
            {
                wolfSurgeTimer -= Time.deltaTime;
                OnWolfSurgeChanged?.Invoke(true, wolfSurgeTimer);

                if (wolfSurgeTimer <= 0f)
                {
                    DeactivateWolfSurge();
                }
            }
        }

        private void UpdatePassiveAura()
        {
            passiveTimer += Time.deltaTime;
            if (passiveTimer < 1.0f) return;
            passiveTimer = 0f;

            if (heroKit == null) return;

            switch (heroKit.passiveType)
            {
                case PassiveType.KindheartedMagic:
                    HeroUnit wounded = FindMostWoundedAlly();
                    if (wounded != null && wounded.HealthPercent < 0.95f)
                    {
                        wounded.Heal(heroKit.passiveMagnitude);
                        CombatVFXPool.SpawnEffect("HealingCircle", wounded.Position + Vector3.up * 0.2f);
                    }
                    break;

                case PassiveType.AlphaPresence:
                    ApplyAlphaPresenceBuff();
                    break;
            }
        }

        private void ApplyAlphaPresenceBuff()
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var ally in allies)
            {
                if (ally != this && ally.IsAlive && Vector2.Distance(transform.position, ally.Position) <= 3.0f)
                {
                    ally.SetDamageMultiplier(1.15f);
                }
            }
        }

        protected override void UpdateBehavior()
        {
            if (currentState == HeroState.Transforming || currentState == HeroState.Dead) return;

            Vector3 slotTarget = GetFormationSlotTarget();

            if (heroKit != null && heroKit.isHealer)
            {
                UpdateHealerState(slotTarget);
                return;
            }

            switch (currentState)
            {
                case HeroState.JoiningFormation:
                    MoveTowards(slotTarget, moveSpeed * 1.5f);
                    ApplyFlockSeparation();
                    if (Vector2.Distance(transform.position, slotTarget) < 0.2f)
                    {
                        currentState = HeroState.Marching;
                    }
                    break;

                case HeroState.Marching:
                    currentTarget = FindNearestEnemyWithinRange(agroRadius);
                    if (currentTarget != null)
                    {
                        currentState = HeroState.DetectingEnemy;
                    }
                    else
                    {
                        float distToSlot = Vector2.Distance(transform.position, slotTarget);
                        if (distToSlot > 0.08f)
                        {
                            MoveTowards(slotTarget, moveSpeed);
                            ApplyFlockSeparation();
                        }
                        else
                        {
                            transform.position = slotTarget;
                            bool formationMoving = FormationController.Instance != null && FormationController.Instance.IsMarching;
                            SetAnimationAction(formationMoving ? 1 : 0);
                            currentState = formationMoving ? HeroState.Marching : HeroState.Idle;
                        }
                    }
                    break;

                case HeroState.Idle:
                    currentTarget = FindNearestEnemyWithinRange(agroRadius);
                    if (currentTarget != null)
                    {
                        currentState = HeroState.DetectingEnemy;
                    }
                    else if (FormationController.Instance != null && FormationController.Instance.IsMarching)
                    {
                        currentState = HeroState.Marching;
                    }
                    break;

                case HeroState.DetectingEnemy:
                    if (currentTarget == null || !currentTarget.IsAlive)
                    {
                        currentState = HeroState.ReturningToFormation;
                        break;
                    }
                    currentState = HeroState.ChasingEnemy;
                    break;

                case HeroState.ChasingEnemy:
                    if (currentTarget == null || !currentTarget.IsAlive)
                    {
                        currentState = HeroState.ReturningToFormation;
                        break;
                    }

                    if (Vector2.Distance(transform.position, slotTarget) > maxChaseDistance)
                    {
                        currentState = HeroState.ReturningToFormation;
                        break;
                    }

                    float distToTarget = Vector2.Distance(transform.position, currentTarget.Position);
                    if (distToTarget <= attackRange)
                    {
                        currentState = HeroState.Attacking;
                        SetAnimationAction(0);
                    }
                    else
                    {
                        MoveTowards(currentTarget.Position, moveSpeed);
                    }
                    break;

                case HeroState.Attacking:
                    if (currentTarget == null || !currentTarget.IsAlive)
                    {
                        currentTarget = FindNearestEnemyWithinRange(agroRadius);
                        if (currentTarget != null)
                        {
                            currentState = HeroState.ChasingEnemy;
                            SetAnimationAction(1);
                        }
                        else
                        {
                            currentState = HeroState.ReturningToFormation;
                            SetAnimationAction(1);
                        }
                        break;
                    }

                    float currentDist = Vector2.Distance(transform.position, currentTarget.Position);
                    if (currentDist > attackRange * 1.35f)
                    {
                        currentState = HeroState.ChasingEnemy;
                        SetAnimationAction(1);
                    }
                    else
                    {
                        if (CanCastSpecial())
                        {
                            StartCoroutine(CastSpecialRoutine(currentTarget));
                            break;
                        }

                        if (Time.time < attackAnimEndTime)
                        {
                            // Let the attack animation complete smoothly
                            break;
                        }

                        if (Time.time >= nextAttackTime)
                        {
                            PerformAttack(currentTarget);
                        }
                        else
                        {
                            SetAnimationAction(0); // Idle between attack intervals
                        }
                    }
                    break;

                case HeroState.CastingSpecial:
                    break;

                case HeroState.ReturningToFormation:
                    MoveTowards(slotTarget, moveSpeed * 1.3f);
                    ApplyFlockSeparation();
                    if (Vector2.Distance(transform.position, slotTarget) < 0.2f)
                    {
                        currentState = HeroState.Marching;
                    }
                    else
                    {
                        Combatant threat = FindNearestEnemyWithinRange(attackRange * 1.1f);
                        if (threat != null)
                        {
                            currentTarget = threat;
                            currentState = HeroState.Attacking;
                        }
                    }
                    break;
            }
        }

        private void UpdateHealerState(Vector3 slotTarget)
        {
            HeroUnit woundedAlly = FindMostWoundedAlly();
            if (woundedAlly != null && woundedAlly.HealthPercent < 0.8f && special01CooldownTimer <= 0f)
            {
                StartCoroutine(CastHealerSpecialRoutine(woundedAlly));
                return;
            }

            float distToSlot = Vector2.Distance(transform.position, slotTarget);
            if (distToSlot > 0.1f)
            {
                MoveTowards(slotTarget, moveSpeed);
                ApplyFlockSeparation();
            }
            else
            {
                transform.position = slotTarget;
                bool isMarching = FormationController.Instance != null && FormationController.Instance.IsMarching;
                SetAnimationAction(isMarching ? 1 : 0);
            }

            Combatant threat = FindNearestEnemyWithinRange(attackRange);
            if (threat != null && Time.time >= nextAttackTime)
            {
                PerformAttack(threat);
            }
        }

        private bool CanCastSpecial()
        {
            if (heroKit == null) return false;
            if (special01CooldownTimer <= 0f && heroKit.special01 != null) return true;
            if (isWolfForm && special02CooldownTimer <= 0f && heroKit.special02 != null) return true;
            return false;
        }

        private IEnumerator CastSpecialRoutine(Combatant target)
        {
            currentState = HeroState.CastingSpecial;

            bool useSpecial02 = isWolfForm && special02CooldownTimer <= 0f && heroKit.special02 != null;
            HeroAbilityData ability = useSpecial02 ? heroKit.special02 : heroKit.special01;

            if (useSpecial02) special02CooldownTimer = ability.cooldown;
            else special01CooldownTimer = ability.cooldown;

            SetAnimationAction(ability.animatorAction);

            if (!string.IsNullOrEmpty(ability.vfxName))
            {
                CombatVFXPool.SpawnEffect(ability.vfxName, target != null ? target.Position : transform.position + Vector3.up * 0.4f);
            }

            yield return new WaitForSeconds(0.35f);

            switch (ability.abilityType)
            {
                case AbilityType.DashBurst:
                    if (target != null && target.IsAlive)
                    {
                        transform.position = target.Position + Vector3.down * 0.4f;
                        target.TakeDamage(ability.value, this);
                    }
                    break;

                case AbilityType.AreaDamage:
                    Vector3 center = target != null ? target.Position : transform.position + Vector3.up * 1.5f;
                    ApplyAreaDamage(center, ability.radius, ability.value);
                    break;

                case AbilityType.StunSlam:
                    if (target != null && target.IsAlive)
                    {
                        target.TakeDamage(ability.value, this);
                        ApplyAreaDamage(target.Position, ability.radius, ability.value * 0.5f);
                    }
                    break;

                case AbilityType.BuffMoraleSpeed:
                    if (GameDirector.Instance != null)
                    {
                        GameDirector.Instance.AddMorale(15f);
                    }
                    break;

                case AbilityType.DeployTrap:
                    CombatVFXPool.SpawnEffect(ability.vfxName, transform.position + Vector3.up * 1.2f);
                    ApplyAreaDamage(transform.position + Vector3.up * 1.2f, ability.radius, ability.value);
                    break;
            }

            yield return new WaitForSeconds(0.25f);
            SetAnimationAction(0);
            currentState = HeroState.Attacking;
        }

        private IEnumerator CastHealerSpecialRoutine(HeroUnit ally)
        {
            currentState = HeroState.CastingSpecial;
            special01CooldownTimer = heroKit != null && heroKit.special01 != null ? heroKit.special01.cooldown : 8f;

            SetAnimationAction(4);
            CombatVFXPool.SpawnEffect("HealingCircle", ally.Position);

            yield return new WaitForSeconds(0.35f);

            float heal = heroKit != null && heroKit.special01 != null ? heroKit.special01.value : 45f;
            ally.Heal(heal);

            yield return new WaitForSeconds(0.25f);
            SetAnimationAction(0);
            currentState = HeroState.Marching;
        }

        private void ApplyAreaDamage(Vector3 center, float radius, float damage)
        {
            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive && Vector2.Distance(center, enemy.Position) <= radius)
                {
                    enemy.TakeDamage(damage, this);
                }
            }
        }

        private void PerformAttack(Combatant target)
        {
            nextAttackTime = Time.time + attackInterval;
            attackAnimEndTime = Time.time + 0.45f;
            SetAnimationAction(2);

            float finalDamage = attackDamage;

            if (heroKit != null && heroKit.passiveType == PassiveType.NaturePrecision)
            {
                if (Vector2.Distance(transform.position, target.Position) >= 2.5f && UnityEngine.Random.value < heroKit.passiveMagnitude)
                {
                    finalDamage *= 2.0f;
                    DamagePopupManager.ShowPopup(target.Position + Vector3.up * 0.8f, Mathf.RoundToInt(finalDamage), true);
                }
            }

            bool isRangedAttack = heroKit != null ? heroKit.isRanged : (heroId == "PrincessElva" || heroId == "WizardMomo" || heroId == "TinkerTilly");
            string vfx = heroKit != null ? heroKit.basicAttackVfx : "MoonlightSlash";

            if (isRangedAttack)
            {
                Projectile.SpawnProjectile(transform.position + Vector3.up * 0.4f, target, finalDamage, vfx);

                if (heroKit != null && heroKit.passiveType == PassiveType.StarChain)
                {
                    Combatant secondary = FindSecondaryTarget(target, 2.5f);
                    if (secondary != null)
                    {
                        Projectile.SpawnProjectile(target.Position, secondary, finalDamage * heroKit.passiveMagnitude, vfx);
                    }
                }
            }
            else
            {
                target.TakeDamage(finalDamage, this);
                CombatVFXPool.SpawnEffect(vfx, target.Position + Vector3.up * 0.2f);
            }
        }

        private Combatant FindSecondaryTarget(Combatant primary, float range)
        {
            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                if (e.IsAlive && e != primary && Vector2.Distance(primary.Position, e.Position) <= range)
                {
                    return e;
                }
            }
            return null;
        }

        private void ApplyFlockSeparation()
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            Vector3 push = Vector3.zero;
            foreach (var ally in allies)
            {
                if (ally == this || !ally.IsAlive) continue;
                Vector3 diff = transform.position - ally.Position;
                float dist = diff.magnitude;
                if (dist < 0.45f && dist > 0.001f)
                {
                    push += (diff.normalized / dist) * 0.15f;
                }
            }
            transform.position += new Vector3(push.x, push.y * 0.5f, 0f) * Time.deltaTime;
        }

        private Vector3 GetFormationSlotTarget()
        {
            if (FormationController.Instance != null)
            {
                return FormationController.Instance.GetSlotWorldPosition(this);
            }
            return transform.position;
        }

        private void MoveTowards(Vector3 targetPos, float speed)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            Vector3 newPos = transform.position + dir * (speed * Time.deltaTime);

            newPos.x = Mathf.Clamp(newPos.x, -1.8f, 1.8f);
            newPos.z = 0f;
            transform.position = newPos;

            SetAnimationAction(1);
        }

        private Combatant FindNearestEnemyWithinRange(float range)
        {
            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            Combatant best = null;
            float closestDist = range;

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

        public override void TakeDamage(float amount, Combatant attacker = null)
        {
            if (heroKit != null && heroKit.passiveType == PassiveType.QuickPaws)
            {
                if (UnityEngine.Random.value < 0.20f)
                {
                    DamagePopupManager.ShowPopup(transform.position + Vector3.up * 0.6f, 0, false);
                    return;
                }
            }

            HeroUnit bobo = FindHero("CaptainBobo");
            if (bobo != null && bobo != this && bobo.IsAlive && Vector2.Distance(transform.position, bobo.Position) <= 2.2f)
            {
                float absorbed = amount * 0.25f;
                amount -= absorbed;
                bobo.TakeDamage(absorbed, attacker);
            }

            base.TakeDamage(amount, attacker);

            if (isAlpha && GameDirector.Instance != null)
            {
                GameDirector.Instance.OnAlphaHealthChanged(currentHealth, maxHealth);
            }
        }

        private HeroUnit FindHero(string id)
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var a in allies)
            {
                if (a.HeroId == id) return a;
            }
            return null;
        }

        protected override void Die(Combatant killer)
        {
            currentState = HeroState.Dead;
            if (FormationController.Instance != null)
            {
                FormationController.Instance.ReleaseHero(this);
            }
            if (isAlpha && GameDirector.Instance != null)
            {
                GameDirector.Instance.OnAlphaDied(this);
            }
            base.Die(killer);
        }

        private void SetupSelectionRing()
        {
            if (selectionRing != null) return;
            GameObject ring = new GameObject("AllyRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            ring.transform.localScale = new Vector3(1.15f, 0.75f, 1f);
            selectionRing = ring.AddComponent<SpriteRenderer>();
            selectionRing.sprite = GetRingSprite();
            selectionRing.color = Color.white;
            selectionRing.sortingOrder = -5;
        }

        private static Sprite cachedRingSprite;
        private static Sprite GetRingSprite()
        {
            if (cachedRingSprite != null) return cachedRingSprite;
#if UNITY_EDITOR
            var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VFX/WorldOfThePack/Art/GroundRing_AllyBlue_HD.png");
            if (loaded != null) { cachedRingSprite = loaded; return cachedRingSprite; }
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
            cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return cachedRingSprite;
        }
    }
}
