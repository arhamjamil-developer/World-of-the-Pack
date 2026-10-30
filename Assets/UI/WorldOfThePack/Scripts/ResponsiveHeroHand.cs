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
        public RectTransform toolbar;
        public RectTransform footer;
        public Button formationButton;
        public Button transformationButton;
        public TMP_Text formationLabel;
        public TMP_Text transformationLabel;
        public UnityEvent formationRequested = new UnityEvent();
        public UnityEvent transformationRequested = new UnityEvent();
        public float maximumCardWidth = 136;
        public float sidePadding = 8;
        public float gap = 5;

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
            float width = Mathf.Min(maximumCardWidth, (root.rect.width - sidePadding * 2 - gap * (cards.Length - 1)) / cards.Length);
            width = Mathf.Max(80, width);
            float height = Mathf.Max(210, width * 1.83f);
            float total = width * cards.Length + gap * (cards.Length - 1);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                var rect = (RectTransform)cards[i].transform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.pivot = new Vector2(.5f, 0);
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = new Vector2(-total * .5f + width * .5f + i * (width + gap), 24);
            }
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + 82);
            if (toolbar != null) { toolbar.anchorMin = toolbar.anchorMax = new Vector2(.5f, 0); toolbar.sizeDelta = new Vector2(total, 48); toolbar.anchoredPosition = new Vector2(0, height + 54); }
            if (footer != null) { footer.anchorMin = footer.anchorMax = new Vector2(.5f, 0); footer.sizeDelta = new Vector2(total, 20); footer.anchoredPosition = new Vector2(0, 12); }
        }
    }
}
