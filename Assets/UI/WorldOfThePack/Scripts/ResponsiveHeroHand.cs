using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorldOfThePack.UI
{
    [ExecuteAlways]
    public sealed class ResponsiveHeroHand : MonoBehaviour
    {
        public HeroDeploymentCard[] cards;
        public RectTransform scrollContent;
        public RectTransform toolbar;
        public RectTransform footer;
        public Button formationButton;
        public Button transformationButton;
        public TMP_Text formationLabel;
        public TMP_Text transformationLabel;
        public UnityEvent formationRequested = new UnityEvent();
        public UnityEvent transformationRequested = new UnityEvent();

        public float cardWidth = 174f;
        public float cardHeight = 212f;
        public float maximumCardWidth = 180f;
        public float sidePadding = 12f;
        public float gap = 8f;

        private void OnEnable()
        {
            if (formationButton != null) formationButton.onClick.AddListener(OnFormation);
            if (transformationButton != null) transformationButton.onClick.AddListener(OnTransform);
            Layout();
        }

        private void OnDisable()
        {
            if (formationButton != null) formationButton.onClick.RemoveListener(OnFormation);
            if (transformationButton != null) transformationButton.onClick.RemoveListener(OnTransform);
        }

        private void OnFormation() => formationRequested.Invoke();
        private void OnTransform() => transformationRequested.Invoke();
        private void LateUpdate() => Layout();

        public void Layout()
        {
            if (cards == null || cards.Length == 0) return;

            var root = (RectTransform)transform;

            if (scrollContent == null)
            {
                var sr = GetComponentInChildren<ScrollRect>();
                if (sr != null) scrollContent = sr.content;
            }

            if (scrollContent != null)
            {
                // Horizontal scrolling tray layout (3 large cards visible prominently, scroll for remaining)
                float totalWidth = sidePadding * 2f + cards.Length * cardWidth + (cards.Length - 1) * gap;
                scrollContent.sizeDelta = new Vector2(totalWidth, cardHeight);

                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] == null) continue;
                    var rect = (RectTransform)cards[i].transform;
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(cardWidth, cardHeight);
                    rect.anchoredPosition = new Vector2(sidePadding + cardWidth * 0.5f + i * (cardWidth + gap), 0f);
                }

                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardHeight + 20f);
            }
            else
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] == null) continue;
                    var rect = (RectTransform)cards[i].transform;
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(cardWidth, cardHeight);
                    rect.anchoredPosition = new Vector2(sidePadding + cardWidth * 0.5f + i * (cardWidth + gap), 0f);
                }
            }
        }

        public void DimOtherCards(HeroDeploymentCard activeCard, float dimmedAlpha)
        {
            if (cards == null) return;
            foreach (var card in cards)
            {
                if (card != null && card != activeCard)
                {
                    card.SetCardAlpha(dimmedAlpha);
                }
            }
        }

        public void RestoreAllCards()
        {
            if (cards == null) return;
            foreach (var card in cards)
            {
                if (card != null)
                {
                    card.SetCardAlpha(1.0f);
                }
            }
        }
    }
}
