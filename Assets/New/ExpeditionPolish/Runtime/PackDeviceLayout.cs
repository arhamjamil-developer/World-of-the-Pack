using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    public sealed class PackDeviceLayout : MonoBehaviour
    {
        public CanvasScaler scaler;
        public RectTransform canvasRect, leftHelper, rightHelper;
        public TMP_Text journeyLabel, packLabel;
        public RawImage leftEnvironment, rightEnvironment;
        private Camera _camera;
        private int _width, _height;
        private void Start() { _camera = Camera.main; ApplyLayout(); }
        private void ApplyLayout()
        {
            _width = Screen.width; _height = Screen.height;
            float aspect = _width / Mathf.Max(1f, _height);
            bool tabletWidth = aspect > .70f;
            if (scaler != null) scaler.matchWidthOrHeight = tabletWidth ? 1 : 0;
            float viewport = tabletWidth ? Mathf.Min(1, (9f / 16f) / aspect) : 1;
            if (_camera != null) _camera.rect = new Rect((1 - viewport) * .5f, 0, viewport, 1);
            if (leftHelper != null) leftHelper.gameObject.SetActive(false);
            if (rightHelper != null) rightHelper.gameObject.SetActive(false);
            LayoutEnvironment(leftEnvironment, 0, (1 - viewport) * .5f);
            LayoutEnvironment(rightEnvironment, (1 + viewport) * .5f, 1);
        }
        private void LayoutEnvironment(RawImage image, float min, float max)
        {
            if (image == null) return;
            image.gameObject.SetActive(max - min > .01f);
            var rt = image.rectTransform;
            rt.anchorMin = new Vector2(min, 0); rt.anchorMax = new Vector2(max, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        private void Update()
        {
            if (Screen.width != _width || Screen.height != _height) ApplyLayout();
            TintEnvironment(leftEnvironment, .25f);
            TintEnvironment(rightEnvironment, .75f);
        }
        private void TintEnvironment(RawImage image, float center)
        {
            if (image == null || image.texture == null || !image.gameObject.activeInHierarchy) return;
            float sourceAspect = image.texture.width / (float)image.texture.height;
            float desiredAspect = image.rectTransform.rect.width / Mathf.Max(1, image.rectTransform.rect.height);
            float width = Mathf.Min(.5f, desiredAspect / sourceAspect);
            float height = Mathf.Min(1, (.5f * sourceAspect) / Mathf.Max(.01f, desiredAspect));
            image.uvRect = new Rect(center - width * .5f, (1 - height) * .5f, width, height);
            var game = GameDirector.Instance;
            float night = game != null && game.IsWolfInstinctActive ? 1 : 0;
            image.color = Color.Lerp(image.color, night > 0 ? new Color(.35f, .48f, .76f) : Color.white, Time.unscaledDeltaTime * 3);
        }
        private void OnDisable() { if (_camera != null) _camera.rect = new Rect(0, 0, 1, 1); }
    }
}
