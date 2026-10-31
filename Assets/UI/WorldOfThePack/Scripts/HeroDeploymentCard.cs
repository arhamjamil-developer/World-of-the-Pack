using System;
using System.Collections;
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
        private Vector3 originalLocalPos;
        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private CanvasGroup canvasGroup;
        private bool isDragging = false;

        public float Energy => energy;
        public float CardCost => 100f;
        public int MaximumEnergy => 100;
        public bool IsPending => pending;
        public bool HasActiveHero => activeHero != null && activeHero.IsAlive;
        public bool HasEnoughEnergy => energy >= 99.5f;
        public bool IsWolfSurgeReady => HasActiveHero && !locked && !pending && (HasEnoughEnergy || (GameDirector.Instance != null && GameDirector.Instance.IsWolfInstinctActive));
        public bool IsDeployReady => !locked && !pending && HasEnoughEnergy;
        public bool IsReady => IsDeployReady || IsWolfSurgeReady;
        public HeroUnit ActiveHero => activeHero;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>();
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
            if (activeHero != null)
            {
                activeHero.OnDefeated -= OnHeroDefeated;
            }

            activeHero = hero;
            if (activeHero != null)
            {
                activeHero.OnDefeated += OnHeroDefeated;
            }
            Refresh();
        }

        private void OnHeroDefeated(Combatant c)
        {
            activeHero = null;
            SetForm(false);
            Refresh();
        }

        public void SetEnergy(float value)
        {
            energy = float.IsNaN(value) ? 0 : Mathf.Clamp(value, 0, MaximumEnergy);
            Refresh();
        }

        public void AddEnergy(float amount)
        {
            if (HasActiveHero && activeHero.IsSurgingWolf) return; // Freeze meter during 20s active wolf surge
            SetEnergy(energy + amount);
        }

        public void SetLocked(bool value) { locked = value; Refresh(); }
        public void SetForm(bool wolf) { if (portraitView != null) portraitView.SetForm(wolf); }
        public void SetSelected(bool value) { if (selectionFrame != null) selectionFrame.enabled = value; }
        public void SetDisplayName(string value) { displayNameOverride = value; Refresh(); }
        public void SetCardAlpha(float a) { if (canvasGroup != null) canvasGroup.alpha = a; }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Tactical lift and preview trigger
            if (IsDeployReady && !isDragging)
            {
                rectTransform.localScale = Vector3.one * 1.06f;
            }
        }

        private void OnButtonClicked()
        {
            if (HasActiveHero)
            {
                if (IsWolfSurgeReady)
                {
                    ActivateWolfSurge();
                }
            }
            else
            {
                if (IsDeployReady)
                {
                    pending = true;
                    Refresh();
                    deployRequested.Invoke(definition);
                }
            }
        }

        public void ActivateWolfSurge()
        {
            if (!IsWolfSurgeReady) return;

            energy = 0f;
            SetForm(true); // Switch card portrait to Wolf
            activeHero.ActivateWolfSurge(20f);
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
            if (!IsDeployReady) return;

            isDragging = true;
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
                Sprite previewSprite = definition != null ? definition.humanPortrait : null;
                CardDragGhostPreview.Instance.Show(previewSprite, worldPos, targetSlotPos, isValid);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            if (rootCanvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }

            Vector3 worldPos = GetWorldPosition(eventData.position);
            int laneIndex = LaneSystem.GetNearestLaneIndex(worldPos.x);
            worldPos.x = LaneSystem.GetLaneX(laneIndex); // Snap preview to lane

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
            Camera cam = Camera.main;
            float camY = cam != null ? cam.transform.position.y : 0f;

            // Must be within horizontal corridor bounds (-1.8 to 1.8)
            // And within playable vertical bounds (between bottom safe lane and upper screen)
            bool xValid = Mathf.Abs(worldPos.x) <= 1.8f;
            bool yValid = worldPos.y >= camY - 3.8f && worldPos.y <= camY + 2.2f;

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
            if (nameLabel != null)
            {
                nameLabel.text = !string.IsNullOrEmpty(displayNameOverride) ? displayNameOverride : definition == null ? "HERO" : definition.shortName;
            }

            // Energy meter display
            if (HasActiveHero && activeHero.IsSurgingWolf)
            {
                if (energyLabel != null) energyLabel.text = $"{Mathf.Ceil(activeHero.WolfSurgeRemaining)}s";
                if (energyFill != null) energyFill.fillAmount = activeHero.WolfSurgePercent;
            }
            else
            {
                if (energyLabel != null) energyLabel.text = $"{Mathf.RoundToInt(energy)} / 100";
                if (energyFill != null)
                {
                    energyFill.type = Image.Type.Filled;
                    energyFill.fillMethod = Image.FillMethod.Horizontal;
                    energyFill.fillAmount = Mathf.Clamp01(energy / 100f);
                    if (readyFillSprite != null && chargingFillSprite != null)
                    {
                        energyFill.sprite = IsReady ? readyFillSprite : chargingFillSprite;
                    }
                }
            }

            // Button and action styling
            bool ready = IsReady;
            if (deployButton != null) deployButton.interactable = ready;
            if (actionArtwork != null) actionArtwork.sprite = (ready || pending) && readyArtwork != null ? readyArtwork : chargingArtwork;

            if (stateLabel != null)
            {
                if (locked) stateLabel.text = lockedText;
                else if (pending) stateLabel.text = pendingText;
                else if (HasActiveHero)
                {
                    if (activeHero.IsSurgingWolf) stateLabel.text = $"WOLF {Mathf.Ceil(activeHero.WolfSurgeRemaining)}s";
                    else if (IsWolfSurgeReady) stateLabel.text = wolfReadyText;
                    else stateLabel.text = chargingText;
                }
                else
                {
                    stateLabel.text = ready ? deployText : chargingText;
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
                selectionFrame.enabled = IsWolfSurgeReady || (HasActiveHero && activeHero.IsSurgingWolf);
                if (selectionFrame.enabled)
                {
                    selectionFrame.color = new Color(1.0f, 0.85f, 0.2f, 0.95f);
                }
            }
        }
    }
}
