using UnityEngine;

namespace WorldOfThePack.UI
{
    [ExecuteAlways]
    public sealed class PortraitSafeArea : MonoBehaviour
    {
        public bool overrideSafeArea;
        // Left, bottom, right, top, normalized to viewport dimensions.
        public Vector4 simulatedInsets;
        private Rect previous;
        private Vector2 previousScreen;
        private Vector4 previousInsets;
        private bool previousOverride;

        private void OnEnable() => Apply();
        private void Update()
        {
            if (previous != Screen.safeArea || previousScreen != new Vector2(Screen.width, Screen.height) || previousInsets != simulatedInsets || previousOverride != overrideSafeArea) Apply();
        }

        public void Apply()
        {
            var rect = (RectTransform)transform;
            Vector4 inset = simulatedInsets;
            if (!overrideSafeArea)
            {
                Rect safe = Screen.safeArea;
                inset = new Vector4(safe.xMin / Mathf.Max(1, Screen.width), safe.yMin / Mathf.Max(1, Screen.height), 1 - safe.xMax / Mathf.Max(1, Screen.width), 1 - safe.yMax / Mathf.Max(1, Screen.height));
            }
            rect.anchorMin = new Vector2(inset.x, inset.y);
            rect.anchorMax = new Vector2(1 - inset.z, 1 - inset.w);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            previous = Screen.safeArea; previousScreen = new Vector2(Screen.width, Screen.height); previousInsets = simulatedInsets; previousOverride = overrideSafeArea;
        }
    }
}
