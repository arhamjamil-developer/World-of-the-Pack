using System;
using UnityEngine;
using System.Collections.Generic;

namespace WorldOfThePack.Game
{
    public abstract class Combatant : MonoBehaviour, IDamageable
    {
        private static readonly List<Combatant> livingUnits = new List<Combatant>();
        public static IReadOnlyList<Combatant> LivingUnits => livingUnits;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetLiving() => livingUnits.Clear();
        protected virtual void OnEnable() { if (!livingUnits.Contains(this)) livingUnits.Add(this); }
        protected virtual void OnDisable() => livingUnits.Remove(this);
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
        public bool IsAlive => !isDead && currentHealth > 0f;
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
        private SpriteRenderer _contactShadow;
        private static Sprite _shadowSprite;

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
            BuildContactShadow();
        }

        protected virtual void Update()
        {
            if (isDead) return;
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing) return;
            if (GameDirector.Instance != null && GameDirector.Instance.CurrentState != GameState.Playing) return;
            if (GameDirector.Instance != null && GameDirector.Instance.IsDecisionPending) return;
            if (CampManager.Instance != null && CampManager.Instance.IsAtCamp) return;

            UpdateSortingOrder();
            UpdateBehavior();
            ResolveCrowding();
        }

        protected virtual void UpdateSortingOrder()
        {
            // Standard 2D top-down perspective sorting relative to camera viewport
            if (bodyRenderer != null)
            {
                float camY = Camera.main != null ? Camera.main.transform.position.y : 0f;
                float relativeY = transform.position.y - camY;
                bodyRenderer.sortingOrder = 200 + Mathf.RoundToInt(-relativeY * 16f);
                if (_contactShadow != null) _contactShadow.sortingOrder = bodyRenderer.sortingOrder-2;
            }
        }

        protected abstract void UpdateBehavior();
        private void ResolveCrowding()
        {
            if (ArenaDirector.Instance == null || this is QueenEscort) return;
            Vector3 push=Vector3.zero;
            foreach(var unit in livingUnits)
            {
                if(unit==this || !unit.IsAlive)continue;
                Vector3 delta=Position-unit.Position; float distance=delta.magnitude;
                float spacing=unit is QueenEscort?.85f:unit.Faction==Faction?.43f:.53f;
                if(distance>=spacing)continue;
                if(distance<.001f)delta=GetInstanceID()>unit.GetInstanceID()?Vector3.right:Vector3.left;
                push+=delta.normalized*(spacing-distance);
            }
            transform.position+=Vector3.ClampMagnitude(push,.4f)*Time.deltaTime*3;
        }
        private void BuildContactShadow()
        {
            if (ArenaDirector.Instance == null) return;
            if (_shadowSprite == null)
            {
                const int size=32;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float distance=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;
                    texture.SetPixel(x,y,new Color(.035f,.025f,.02f,Mathf.Clamp01(1-distance)*.5f));
                }
                texture.Apply();_shadowSprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),size);
            }
            var shadow=new GameObject("Contact shadow");shadow.transform.SetParent(transform,false);
            shadow.transform.localPosition=Vector3.down*.1f;
            float sizeMultiplier=this is QueenEscort?2.2f:1;
            shadow.transform.localScale=new Vector3(.66f/Mathf.Max(.1f,transform.lossyScale.x),.22f/Mathf.Max(.1f,transform.lossyScale.y),1)*sizeMultiplier;
            _contactShadow=shadow.AddComponent<SpriteRenderer>();_contactShadow.sprite=_shadowSprite;
        }

        public virtual void TakeDamage(float amount, Combatant attacker = null)
        {
            if (isDead) return;
            var formation = FormationController.Instance;
            if (faction == UnitFaction.Allies && formation != null)
                amount *= formation.CurrentFormationIndex == 1 ? .7f : formation.CurrentFormationIndex == 2 ? 1.2f : 1f;

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

            float previous = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            UpdateHealthBarUI();
            float restored = currentHealth - previous;
            if (restored >= 5) DamagePopupManager.ShowPopup(transform.position + Vector3.up * 0.6f, restored, false, true);
        }

        protected virtual void Die(Combatant killer)
        {
            if (isDead) return;
            isDead = true;

            SetAnimationAction(3); // Dead
            SetAnimationPlaybackSpeed(1f);

            if (healthBarPivot != null) healthBarPivot.gameObject.SetActive(false);
            if (selectionRing != null) selectionRing.gameObject.SetActive(false);

            RaiseDefeated();
            Destroy(gameObject, 1.8f);
        }

        protected void RaiseDefeated()
        {
            OnDefeated?.Invoke(this);
        }

        protected void SetAnimationAction(int actionIndex)
        {
            if (animator != null && animator.gameObject.activeInHierarchy && animator.runtimeAnimatorController != null)
            {
                animator.SetInteger("Action", actionIndex);
                if (actionIndex == 0) // Idle
                {
                    animator.speed = 1.0f;
                }
            }
        }

        public void SetAnimationPlaybackSpeed(float speedMultiplier)
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                animator.speed = Mathf.Clamp(speedMultiplier, 0.25f, 3.0f);
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
