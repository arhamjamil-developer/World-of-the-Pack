using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Expedition
{
    /// <summary>Fits a three-card window while card geometry remains inherited from the shared prefab.</summary>
    [ExecuteAlways]
    public sealed class ResponsiveCardStrip : MonoBehaviour
    {
        public RectTransform content, handRoot, safeArea;
        [Min(1)] public int visibleCards = 3;
        [Min(0)] public float spacing = 14;
        [Min(1)] public float handReferenceWidth = 940;
        private RectTransform _viewport;
        private void OnEnable()
        {
            _viewport = transform as RectTransform;
            if (handRoot == null) handRoot = transform.parent as RectTransform;
            if (safeArea == null && handRoot != null) safeArea = handRoot.parent as RectTransform;
        }
        private void LateUpdate()
        {
            if (_viewport == null || content == null || content.childCount == 0) return;
            float width = 0, height = 0;
            int count = Mathf.Min(visibleCards, content.childCount);
            for (int i = 0; i < count; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (card == null) continue;
                width += card.rect.width; height = Mathf.Max(height, card.rect.height);
            }
            width += spacing * Mathf.Max(0, count - 1);
            float scale = Mathf.Min(_viewport.rect.width / Mathf.Max(1, width), _viewport.rect.height / Mathf.Max(1, height));
            var target = Vector3.one * Mathf.Max(.1f, scale);
            if ((content.localScale - target).sqrMagnitude > .000001f) content.localScale = target;
            if (handRoot != null && safeArea != null)
            {
                var fit = Vector3.one * Mathf.Min(1, safeArea.rect.width / handReferenceWidth);
                if ((handRoot.localScale - fit).sqrMagnitude > .000001f) handRoot.localScale = fit;
            }
        }
    }
}
