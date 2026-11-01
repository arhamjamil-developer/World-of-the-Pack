using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WorldOfThePack.Game;

namespace WorldOfThePack.UI
{
    [Serializable] public sealed class HeroDeployEvent : UnityEvent<HeroCardDefinition> { }
    [Serializable] public sealed class HeroDeployPositionEvent : UnityEvent<HeroCardDefinition, Vector3> { }

    public sealed class HeroDeploymentCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        [Header("Card Identity")]
        public HeroCardDefinition definition;
        public HeroCardDefinition Definition => definition;
        public CardPortraitView portraitView;

        [Header("UI Text & Labels")]
        public TMP_Text nameLabel;
        public TMP_Text energyLabel;
        public TMP_Text stateLabel;

        [Header("UI Artwork & Controls")]
        public Image energyFill;
        public Image actionArtwork;
        public Image selectionFrame;
        public Button deployButton;
        public Sprite readyArtwork;
        public Sprite chargingArtwork;
        public Sprite readyFillSprite;
        public Sprite chargingFillSprite;

        [Header("Flipbook Effects")]
        public UIFlipbookEffect chargingEffect;
        public UIFlipbookEffect readyEffect;
        public UIFlipbookEffect deployedEffect;

        [Header("Copy Overrides")]
        public string deployText = "SPAWN";
        public string chargingText = "CHARGING";
        public string pendingText = "DEPLOYING";
        public string lockedText = "LOCKED";
        public string wolfReadyText = "WOLF FORM!";
        public string displayNameOverride;

        [Header("Deployment Events")]
        public HeroDeployEvent deployRequested = new HeroDeployEvent();
        public HeroDeployPositionEvent deployAtPositionRequested = new HeroDeployPositionEvent();

        [Header("State")]
        [SerializeField] private float energy;
        [SerializeField] private bool locked;
        [SerializeField] private bool pending;

        private HeroUnit activeHero;
        private readonly List<HeroUnit> deployedHeroes = new List<HeroUnit>();
        private Vector3 originalLocalPos;
        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private CanvasGroup canvasGroup;
        private ScrollRect parentScrollRect;
        private bool isDragging = false;
        private bool isScrollingParent = false;
        private bool isWolfForm = false;
        private bool _tacticalSelected;

        public float Energy => energy;
        public TMP_Text charmCostLabel;
        public float CardCost => 100f;
        public int MaximumEnergy => 100;
        public bool IsPending => pending;
        public bool HasActiveHero => deployedHeroes.Exists(h => h != null && h.IsAlive);
        public int ActiveHeroCount => deployedHeroes.FindAll(h => h != null && h.IsAlive).Count;
        public bool HasEnoughEnergy => energy >= 99.5f;
        // Moonshift is commanded for the whole pack from the toolbar.
        public bool IsWolfSurgeReady => false;
        public int CharmPrice => GameDirector.Instance != null && definition != null ? GameDirector.Instance.CharmCost(definition.heroId) : 0;
        public bool IsDeployReady => !locked && !pending && HasEnoughEnergy && definition != null &&
            GameDirector.Instance != null && GameDirector.Instance.CurrentState == GameState.Playing &&
            !GameDirector.Instance.IsDecisionPending &&
            (TutorialManager.Instance == null || TutorialManager.Instance.AllowsCard(definition.heroId)) &&
            GameDirector.Instance.CanAfford(definition.heroId) && GameDirector.Instance.ActiveAllyCount < GameDirector.Instance.MaxAllies &&
            (CampManager.Instance == null || !CampManager.Instance.IsAtCamp);
        public bool IsReady => IsDeployReady || IsWolfSurgeReady;
        public HeroUnit ActiveHero => deployedHeroes.Find(h => h != null && h.IsAlive);

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>();
            parentScrollRect = GetComponentInParent<ScrollRect>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (deployButton != null) deployButton.onClick.AddListener(OnButtonClicked);
            Refresh();
        }

        private void OnDisable()
        {
            if (deployButton != null) deployButton.onClick.RemoveListener(OnButtonClicked);
            pending = false;
            if (isDragging) CancelDrag();
        }

        private void Update()
        {
            if (GameDirector.Instance != null && GameDirector.Instance.CurrentState == GameState.Playing)
            {
                Refresh();
            }
        }

        public void Bind(HeroCardDefinition hero, float initialEnergy, bool wolf = false)
        {
            foreach (var previous in deployedHeroes) if (previous != null) previous.OnDefeated -= OnHeroDefeated;
            deployedHeroes.Clear();
            isWolfForm = wolf;
            definition = hero;
            pending = false;
            locked = false;
            displayNameOverride = null;
            activeHero = null;

            if (deployedEffect != null) deployedEffect.Stop();
            if (portraitView != null) portraitView.Bind(hero, wolf);
            SetEnergy(initialEnergy);
        }

        public void SetDeployedHero(HeroUnit hero)
        {
            if (hero == null) return;
            deployedHeroes.RemoveAll(h => h == null);
            if (!deployedHeroes.Contains(hero))
            {
                deployedHeroes.Add(hero);
                hero.OnDefeated += OnHeroDefeated;
            }
            activeHero = hero;
            pending = false;
            Refresh();
        }

        private void OnHeroDefeated(Combatant c)
        {
            if (c is HeroUnit hu)
            {
                hu.OnDefeated -= OnHeroDefeated;
                deployedHeroes.Remove(hu);
            }
            deployedHeroes.RemoveAll(h => h == null || !h.IsAlive);
            if (deployedHeroes.Count == 0)
            {
                activeHero = null;
                SetForm(false);
            }
            else
            {
                activeHero = deployedHeroes[0];
            }
            Refresh();
        }

        public void SetEnergy(float value)
        {
            energy = float.IsNaN(value) ? 0 : Mathf.Clamp(value, 0, MaximumEnergy);
            Refresh();
        }

        public void AddEnergy(float amount)
        {
            HeroUnit liveHero = ActiveHero;
            if (liveHero != null && liveHero.IsSurgingWolf) return; // Freeze meter during 20s active wolf surge
            SetEnergy(energy + amount);
        }

        public void SetLocked(bool value) { locked = value; Refresh(); }
        public void SetForm(bool wolf)
        {
            isWolfForm = wolf;
            if (portraitView != null) portraitView.SetForm(wolf);
        }
        public void SetSelected(bool value) { _tacticalSelected=value; if (selectionFrame != null) selectionFrame.enabled = value; }
        public void SetDisplayName(string value) { displayNameOverride = value; Refresh(); }
        public void SetCardAlpha(float a) { if (canvasGroup != null) canvasGroup.alpha = a; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing)
            {
                ArenaDirector.Instance.SelectCard(this);
                return;
            }
            // Tactical lift and preview trigger
            if (IsDeployReady && !isDragging)
            {
                rectTransform.localScale = Vector3.one * 1.06f;
            }
        }

        private void OnButtonClicked()
        {
            if (GameDirector.Instance != null && GameDirector.Instance.CurrentState != GameState.Playing) return;
            if (IsDeployReady)
            {
                pending = true;
                deployRequested.Invoke(definition);
                // The successful spawn calls ConfirmDeployment; a rejected spawn retains its charge.
                Refresh();
            }
            else if (HasActiveHero && IsWolfSurgeReady)
            {
                ActivateWolfSurge();
            }
        }

        public void ActivateWolfSurge()
        {
            if (!IsWolfSurgeReady) return;

            energy = 0f;
            SetForm(true); // Switch card portrait to Wolf
            foreach (var h in deployedHeroes)
            {
                if (h != null && h.IsAlive) h.ActivateWolfSurge(20f);
            }
            Refresh();

            if (deployedEffect != null) deployedEffect.Play(false);
        }

        public bool ConfirmDeployment(HeroUnit spawnedHero = null)
        {
            pending = false;
            isDragging = false;
            energy = 0;

            if (spawnedHero != null)
            {
                SetDeployedHero(spawnedHero);
            }

            SetForm(false); // New deployments start in Human form
            Refresh();

            if (deployedEffect != null) deployedEffect.Play(false);
            return true;
        }

        public void CancelDeployment()
        {
            pending = false;
            isDragging = false;
            Refresh();
        }

        #region Physical Drag to Deploy Implementation

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (parentScrollRect == null) parentScrollRect = GetComponentInParent<ScrollRect>();

            float dx = Mathf.Abs(eventData.delta.x);
            float dy = Mathf.Abs(eventData.delta.y);

            // If drag is primarily horizontal or card cannot deploy or user drags downwards: route to ScrollRect!
            if (dx > dy || !IsDeployReady || eventData.delta.y < 0)
            {
                isScrollingParent = true;
                isDragging = false;
                if (parentScrollRect != null) parentScrollRect.OnBeginDrag(eventData);
                return;
            }

            isScrollingParent = false;
            isDragging = true;
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing) ArenaDirector.Instance.SelectCard(this);
            originalLocalPos = rectTransform.localPosition;
            rectTransform.localScale = Vector3.one * 1.08f;

            // Dim other cards in hand
            ResponsiveHeroHand hand = GetComponentInParent<ResponsiveHeroHand>();
            if (hand != null) hand.DimOtherCards(this, 0.45f);

            // Show tactical ghost preview in world
            Vector3 worldPos = GetWorldPosition(eventData.position);
            Vector3 targetSlotPos = GetTargetSlotPosition();
            bool isValid = IsValidPlacement(worldPos);

            if (CardDragGhostPreview.Instance != null)
            {
                Sprite previewSprite = GetIdlePreviewSprite();
                CardDragGhostPreview.Instance.Show(previewSprite, worldPos, targetSlotPos, isValid);
            }
        }

        private Sprite GetIdlePreviewSprite()
        {
            if (definition == null) return null;
            bool wolf = isWolfForm || (HasActiveHero && activeHero.IsSurgingWolf);
            Sprite sprite = wolf ? definition.wolfIdleSprite : definition.idleSprite;
            if (sprite != null) return sprite;

            string id = definition.heroId;
            string prefabPath = wolf
                ? $"Assets/Characters/{id}/Prefabs/{id}_Wolf_North.prefab"
                : $"Assets/WorldOfThePack_Game/Prefabs/Units/{id}_PackUnit.prefab";
#if UNITY_EDITOR
            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go != null)
            {
                var sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null) return sr.sprite;
            }
#endif
            return wolf ? definition.wolfPortrait : definition.humanPortrait;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isScrollingParent)
            {
                if (parentScrollRect != null) parentScrollRect.OnDrag(eventData);
                return;
            }

            if (!isDragging) return;

            if (rootCanvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }

            Vector3 worldPos = GetWorldPosition(eventData.position);
            int laneIndex = LaneSystem.GetNearestLaneIndex(worldPos.x);
            worldPos.x = LaneSystem.GetLaneX(laneIndex); // Snap preview to lane
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing) worldPos = ArenaDirector.Instance.SnapDeployment(worldPos);

            if (LaneOverlayRenderer.Instance != null)
            {
                LaneOverlayRenderer.Instance.HighlightLane(laneIndex, true);
            }

            Vector3 targetSlotPos = GetTargetSlotPosition();
            bool isValid = IsValidPlacement(worldPos);

            if (CardDragGhostPreview.Instance != null)
            {
                CardDragGhostPreview.Instance.UpdatePosition(worldPos, targetSlotPos, isValid);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isScrollingParent)
            {
                isScrollingParent = false;
                if (parentScrollRect != null) parentScrollRect.OnEndDrag(eventData);
                return;
            }

            if (!isDragging) return;

            isDragging = false;

            if (LaneOverlayRenderer.Instance != null)
            {
                LaneOverlayRenderer.Instance.HighlightLane(-1, false);
            }

            // Restore other cards
            ResponsiveHeroHand hand = GetComponentInParent<ResponsiveHeroHand>();
            if (hand != null) hand.RestoreAllCards();

            if (CardDragGhostPreview.Instance != null)
            {
                CardDragGhostPreview.Instance.Hide();
            }

            Vector3 worldPos = GetWorldPosition(eventData.position);
            int laneIndex = LaneSystem.GetNearestLaneIndex(worldPos.x);
            worldPos.x = LaneSystem.GetLaneX(laneIndex);
            bool isValid = IsValidPlacement(worldPos);

            if (isValid && IsDeployReady)
            {
                pending = true;
                rectTransform.localPosition = originalLocalPos;
                rectTransform.localScale = Vector3.one;
                Refresh();

                deployAtPositionRequested.Invoke(definition, worldPos);
            }
            else
            {
                // Smooth snap back
                StartCoroutine(SmoothReturnRoutine());
            }
        }

        private IEnumerator SmoothReturnRoutine()
        {
            Vector3 startPos = rectTransform.localPosition;
            Vector3 startScale = rectTransform.localScale;
            float elapsed = 0f;
            float duration = 0.18f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
                rectTransform.localPosition = Vector3.Lerp(startPos, originalLocalPos, ease);
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.one, ease);
                yield return null;
            }

            rectTransform.localPosition = originalLocalPos;
            rectTransform.localScale = Vector3.one;
            CancelDeployment();
        }

        private void CancelDrag()
        {
            isDragging = false;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
                rectTransform.localPosition = originalLocalPos;
            }
            if (CardDragGhostPreview.Instance != null)
            {
                CardDragGhostPreview.Instance.Hide();
            }
            ResponsiveHeroHand hand = GetComponentInParent<ResponsiveHeroHand>();
            if (hand != null) hand.RestoreAllCards();
        }

        private bool IsValidPlacement(Vector3 worldPos)
        {
            var arena = ArenaDirector.Instance;
            if (arena != null && arena.IsPreparing)
            {
                var cell = arena.SnapDeployment(worldPos);
                return arena.IsValidCell(worldPos) && (TutorialManager.Instance == null || TutorialManager.Instance.AllowsPlacement(definition.heroId,cell));
            }
            Camera cam = Camera.main;
            float camY = cam != null ? cam.transform.position.y : 0f;

            // Must be within horizontal corridor bounds (-1.8 to 1.8)
            // And strictly within the clear battlefield corridor (above Card UI at -2.26f and below Top HUD at +4.8f)
            bool xValid = Mathf.Abs(worldPos.x) <= 1.8f;
            bool yValid = worldPos.y >= camY - 1.6f && worldPos.y <= camY + 2.8f;

            return xValid && yValid;
        }

        private Vector3 GetTargetSlotPosition()
        {
            if (FormationController.Instance != null && definition != null)
            {
                return FormationController.Instance.GetPreferredWorldPosition(definition.heroId);
            }
            return Vector3.zero;
        }

        private Vector3 GetWorldPosition(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector3.zero;

            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
            world.z = 0f;
            world.x = Mathf.Clamp(world.x, -1.8f, 1.8f);
            return world;
        }

        #endregion

        public void Refresh()
        {
            bool showWolf = GameDirector.Instance != null && GameDirector.Instance.IsWolfInstinctActive;
            if (isWolfForm != showWolf) SetForm(showWolf);
            if (charmCostLabel != null)
            {
                charmCostLabel.text = CharmPrice.ToString();
                charmCostLabel.color = GameDirector.Instance == null || definition == null || GameDirector.Instance.CanAfford(definition.heroId)
                    ? new Color(.8f, 1f, .92f) : new Color(1f, .55f, .43f);
            }
            if (nameLabel != null)
            {
                nameLabel.text = !string.IsNullOrEmpty(displayNameOverride) ? displayNameOverride : definition == null ? "HERO" : definition.shortName;
            }

            HeroUnit liveHero = ActiveHero;

            // Energy meter display
            if (liveHero != null && liveHero.IsSurgingWolf)
            {
                if (energyLabel != null) energyLabel.text = $"{Mathf.Ceil(liveHero.WolfSurgeRemaining)}s";
                if (energyFill != null) energyFill.fillAmount = liveHero.WolfSurgePercent;
            }
            else
            {
                if (energyLabel != null) energyLabel.text = $"{Mathf.RoundToInt(energy)}% CHARGE";
                if (energyFill != null)
                {
                    energyFill.type = Image.Type.Filled;
                    energyFill.fillMethod = Image.FillMethod.Horizontal;
                    energyFill.fillAmount = Mathf.Clamp01(energy / 100f);
                    if (readyFillSprite != null && chargingFillSprite != null)
                    {
                        energyFill.sprite = HasEnoughEnergy ? readyFillSprite : chargingFillSprite;
                    }
                }
            }

            // Button and action styling
            bool ready = IsReady;
            if (deployButton != null) deployButton.interactable = ready;
            if (actionArtwork != null) actionArtwork.sprite = (ready || pending) && readyArtwork != null ? readyArtwork : chargingArtwork;

            if (stateLabel != null)
            {
                int count = ActiveHeroCount;
                if (locked) stateLabel.text = lockedText;
                else if (pending) stateLabel.text = "";
                else if (IsWolfSurgeReady) stateLabel.text = wolfReadyText;
                else if (ready)
                {
                    // DeployButton_Green artwork already has embossed 'DEPLOY' graphic baked in;
                    // show count badge if multiple active, otherwise leave clean so artwork shines through
                    stateLabel.text = count > 1 ? $"x{count}" : "";
                }
                else if (liveHero != null && liveHero.IsSurgingWolf)
                {
                    stateLabel.text = "WOLF SURGE";
                }
                else
                {
                    // ChargingButton_Navy artwork already has embossed 'CHARGING' graphic baked in;
                    // Leave clean so the embossed pill shines through, or show count badge if multi-deployed
                    stateLabel.text = count > 1 ? $"x{count}" : "";
                }
            }

            // Flipbook shimmer VFX
            if (chargingEffect != null)
            {
                if (!ready && !pending && !locked) chargingEffect.Play(true);
                else chargingEffect.Stop();
            }
            if (readyEffect != null)
            {
                if (ready) readyEffect.Play(true);
                else readyEffect.Stop();
            }

            // Golden radiant border when wolf surge is ready or surging
            if (selectionFrame != null)
            {
                selectionFrame.enabled = _tacticalSelected || IsWolfSurgeReady || (liveHero != null && liveHero.IsSurgingWolf);
                if (selectionFrame.enabled)
                {
                    selectionFrame.color = new Color(1.0f, 0.85f, 0.2f, 0.95f);
                }
            }
        }
    }
}
