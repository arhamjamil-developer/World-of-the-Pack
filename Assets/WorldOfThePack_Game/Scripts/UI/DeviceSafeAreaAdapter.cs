using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    [ExecuteAlways]
    public class DeviceSafeAreaAdapter : MonoBehaviour
    {
        [Header("Canvas Reference")]
        [SerializeField] private CanvasScaler canvasScaler;

        [Header("Anchored Sub-Panels")]
        [SerializeField] private RectTransform topHUDPanel;
        [SerializeField] private RectTransform bottomCardsPanel;

        [Header("Safe Margins")]
        [SerializeField] private float topPadding = 12f;
        [SerializeField] private float bottomPadding = 10f;

        private void Awake()
        {
            if (canvasScaler == null) canvasScaler = GetComponent<CanvasScaler>();
            ApplyAdapter();
        }

        private void Update()
        {
#if UNITY_EDITOR
            ApplyAdapter();
#endif
        }

        public void ApplyAdapter()
        {
            if (canvasScaler != null)
            {
                canvasScaler.referenceResolution = new Vector2(576, 1024);
                float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
                // iPad aspect is around 0.75 (3:4) or wider; iPhone is around 0.46 - 0.56 (9:16)
                if (aspect > 0.65f)
                {
                    // Tablet / iPad: Match height to prevent UI overflowing vertically
                    canvasScaler.matchWidthOrHeight = 0.5f;
                }
                else
                {
                    // Smartphone / iPhone: Match width to ensure readable UI
                    canvasScaler.matchWidthOrHeight = 0.0f;
                }
            }

            Rect safeArea = Screen.safeArea;
            float topInset = (Screen.height - safeArea.yMax) / Mathf.Max(1, Screen.height);
            float bottomInset = safeArea.yMin / Mathf.Max(1, Screen.height);

            if (topHUDPanel != null)
            {
                topHUDPanel.offsetMax = new Vector2(0f, -topInset * Screen.height - topPadding);
            }

            if (bottomCardsPanel != null)
            {
                bottomCardsPanel.offsetMin = new Vector2(0f, bottomInset * Screen.height + bottomPadding);
            }
        }
    }
}
