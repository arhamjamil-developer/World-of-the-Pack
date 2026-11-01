using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Expedition
{
    /// <summary>The inherited CardFace rectangle owns shape; the layout group owns instance root rectangles.</summary>
    [ExecuteAlways]
    public sealed class CardFaceLayoutSizer : MonoBehaviour
    {
        public RectTransform face;
        public LayoutElement layout;
        private void OnEnable() => Fit();
        private void LateUpdate() => Fit();
        public void Fit()
        {
            if (face == null || layout == null) return;
            var size = face.sizeDelta;
            if (!Mathf.Approximately(layout.preferredWidth, size.x)) layout.preferredWidth = size.x;
            if (!Mathf.Approximately(layout.preferredHeight, size.y)) layout.preferredHeight = size.y;
        }
    }
}
