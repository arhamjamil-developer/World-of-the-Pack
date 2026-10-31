using System;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public abstract class Combatant : MonoBehaviour, IDamageable
    {
        [Header("Identity & Faction")]
        [SerializeField] protected UnitFaction faction = UnitFaction.Allies;
        [SerializeField] protected string unitName = "Combatant";

        [Header("Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth = 100f;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float attackRange = 1.2f;
        [SerializeField] protected float attackDamage = 20f;
        [SerializeField] protected float attackInterval = 1.2f;

        [Header("Visual Components")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected SpriteRenderer bodyRenderer;
        [SerializeField] protected Transform healthBarPivot;
        [SerializeField] protected Transform healthBarFill;
        [SerializeField] protected SpriteRenderer selectionRing;

        public UnitFaction Faction => faction;
        public Vector3 Position => transform.position;
        public bool IsAlive => currentHealth > 0f;
        public float HealthPercent => Mathf.Clamp01(currentHealth / Mathf.Max(1f, maxHealth));
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float AttackDamage => attackDamage;
        public string UnitName => unitName;

        public event Action<Combatant> OnDefeated;
        public event Action<float, float> OnHealthChanged;

        protected float nextAttackTime;
        protected Combatant currentTarget;
        protected bool isDead;

        protected virtual void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (bodyRenderer == null)
            {
                if (animator != null)
                {
                    Transform bodyChild = animator.transform.Find("Body");
                    if (bodyChild != null) bodyRenderer = bodyChild.GetComponent<SpriteRenderer>();
                }
                if (bodyRenderer == null)
                {
                    var renderers = GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var r in renderers)
                    {
                        if (r != selectionRing && !r.name.Contains("Ring") && !r.name.Contains("Bar") && !r.name.Contains("Fill") && !r.name.Contains("BG") && !r.name.Contains("Crown"))
                        {
                            bodyRenderer = r;
                            break;
                        }
                    }
                }
            }

            currentHealth = maxHealth;
            BuildDefaultHealthBarIfNeeded();
        }

        protected virtual void Start()
        {
            SetAnimationAction(0); // Idle
        }

        protected virtual void Update()
        {
            if (isDead) return;

            UpdateSortingOrder();
            UpdateBehavior();
        }

        protected virtual void UpdateSortingOrder()
        {
            // Standard 2D top-down perspective sorting relative to camera viewport
            if (bodyRenderer != null)
            {
                float camY = Camera.main != null ? Camera.main.transform.position.y : 0f;
                float relativeY = transform.position.y - camY;
                bodyRenderer.sortingOrder = 200 + Mathf.RoundToInt(-relativeY * 16f);
            }
        }

        protected abstract void UpdateBehavior();

        public virtual void TakeDamage(float amount, Combatant attacker = null)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            UpdateHealthBarUI();

            FlashHitFeedback();
            DamagePopupManager.ShowPopup(transform.position + Vector3.up * 0.8f, amount, faction == UnitFaction.Hunters);

            if (currentHealth <= 0f)
            {
                Die(attacker);
            }
        }

        public virtual void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            UpdateHealthBarUI();
            DamagePopupManager.ShowPopup(transform.position + Vector3.up * 0.8f, amount, false, true);
        }

        protected virtual void Die(Combatant killer)
        {
            if (isDead) return;
            isDead = true;

            SetAnimationAction(3); // Dead

            if (healthBarPivot != null) healthBarPivot.gameObject.SetActive(false);
            if (selectionRing != null) selectionRing.gameObject.SetActive(false);

            OnDefeated?.Invoke(this);
            Destroy(gameObject, 1.8f);
        }

        protected void SetAnimationAction(int actionIndex)
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                animator.SetInteger("Action", actionIndex);
            }
        }

        protected virtual void FlashHitFeedback()
        {
            if (bodyRenderer != null)
            {
                StopCoroutine(nameof(HitFlashRoutine));
                StartCoroutine(HitFlashRoutine());
            }
        }

        private System.Collections.IEnumerator HitFlashRoutine()
        {
            if (bodyRenderer != null) bodyRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            yield return new WaitForSeconds(0.08f);
            if (bodyRenderer != null) bodyRenderer.color = Color.white;
        }

        protected void BuildDefaultHealthBarIfNeeded()
        {
            if (healthBarPivot != null) return;

            GameObject hbObj = new GameObject("HealthBar_World");
            hbObj.transform.SetParent(transform, false);
            hbObj.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            healthBarPivot = hbObj.transform;

            // Background
            GameObject bgObj = new GameObject("BG");
            bgObj.transform.SetParent(hbObj.transform, false);
            SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = GetBarSprite();
            bgSr.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            bgSr.sortingOrder = 999;
            bgObj.transform.localScale = new Vector3(0.9f, 0.14f, 1f);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(hbObj.transform, false);
            SpriteRenderer fillSr = fillObj.AddComponent<SpriteRenderer>();
            fillSr.sprite = GetBarSprite();
            fillSr.color = faction == UnitFaction.Allies ? new Color(0.2f, 0.85f, 0.35f, 1f) : new Color(0.95f, 0.25f, 0.25f, 1f);
            fillSr.sortingOrder = 1000;
            fillObj.transform.localScale = new Vector3(0.86f, 0.10f, 1f);
            healthBarFill = fillObj.transform;

            UpdateHealthBarUI();
        }

        protected void UpdateHealthBarUI()
        {
            if (healthBarFill != null)
            {
                float pct = HealthPercent;
                healthBarFill.localScale = new Vector3(0.86f * pct, 0.10f, 1f);
                healthBarFill.localPosition = new Vector3(-0.43f * (1f - pct), 0f, 0f);
            }
        }

        private static Sprite cachedBarSprite;
        private static Sprite GetBarSprite()
        {
            if (cachedBarSprite != null) return cachedBarSprite;
            Texture2D tex = new Texture2D(2, 2);
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(1, 0, Color.white);
            tex.SetPixel(0, 1, Color.white);
            tex.SetPixel(1, 1, Color.white);
            tex.Apply();
            cachedBarSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return cachedBarSprite;
        }
    }
}
