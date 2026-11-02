using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    [ExecuteAlways]
    public sealed class PackDeviceLayout : MonoBehaviour
    {
        public CanvasScaler scaler;
        public RectTransform canvasRect, leftHelper, rightHelper;
        public TMP_Text journeyLabel, packLabel;
        public RawImage leftEnvironment, rightEnvironment;
        private Camera _camera;
        private int _width, _height;

        private void Awake() { _camera = Camera.main; ApplyLayout(); }
        private void Start() { _camera = Camera.main; ApplyLayout(); }

        public void ApplyLayout()
        {
            if (_camera == null) _camera = Camera.main;
            _width = Screen.width; _height = Screen.height;
            float aspect = _width / Mathf.Max(1f, _height);
            bool tabletWidth = aspect > .70f;
            if (scaler != null) scaler.matchWidthOrHeight = tabletWidth ? 1 : 0;
            float viewport = tabletWidth ? Mathf.Min(1, (9f / 16f) / aspect) : 1;
            if (_camera != null) _camera.rect = new Rect((1 - viewport) * .5f, 0, viewport, 1);
            if (leftHelper != null) leftHelper.gameObject.SetActive(false);
            if (rightHelper != null) rightHelper.gameObject.SetActive(false);

            // Ensure TabletScenery parent container is active on tablets
            if (leftEnvironment != null && leftEnvironment.transform.parent != null)
            {
                leftEnvironment.transform.parent.gameObject.SetActive(tabletWidth);
            }

            // 80px canvas overlap into camera viewport completely conceals camera scissor line
            // allowing organic alpha feathering to dissolve seamlessly into 3D forest ground
            float overlap = tabletWidth ? 80f : 0f;
            LayoutEnvironment(leftEnvironment, 0, (1 - viewport) * .5f, isLeft: true, overlap: overlap);
            LayoutEnvironment(rightEnvironment, (1 + viewport) * .5f, 1, isLeft: false, overlap: overlap);
        }

        private void LayoutEnvironment(RawImage image, float min, float max, bool isLeft, float overlap)
        {
            if (image == null) return;
            bool active = max - min > .01f;
            image.gameObject.SetActive(active);
            if (!active) return;

            var rt = image.rectTransform;
            rt.anchorMin = new Vector2(min, 0); 
            rt.anchorMax = new Vector2(max, 1);
            if (isLeft)
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = new Vector2(overlap, 0);
            }
            else
            {
                rt.offsetMin = new Vector2(-overlap, 0);
                rt.offsetMax = Vector2.zero;
            }
        }

        private void Update()
        {
            if (Screen.width != _width || Screen.height != _height) ApplyLayout();

            // Synchronize vertical UV scroll with camera movement
            // WorldBiomeGenerator segment height is 21.72f.
            // As the wolves run north, the side woodland canopy scrolls down at the exact
            // same velocity as the central 3D ground, eliminating static velocity shear.
            float scrollY = 0f;
            if (_camera != null)
            {
                scrollY = (_camera.transform.position.y / 21.72f);
            }

            TintEnvironment(leftEnvironment, isLeft: true, scrollY: scrollY);
            TintEnvironment(rightEnvironment, isLeft: false, scrollY: scrollY);
        }

        private void TintEnvironment(RawImage image, bool isLeft, float scrollY)
        {
            if (image == null || image.texture == null || !image.gameObject.activeInHierarchy) return;
            // Clean 1:1 UV mapping scrolling in sync with camera Y
            image.uvRect = isLeft 
                ? new Rect(0f, scrollY, 0.5f, 1f) 
                : new Rect(0.5f, scrollY, 0.5f, 1f);

            var game = GameDirector.Instance;
            float night = game != null && game.IsWolfInstinctActive ? 1 : 0;
            image.color = Color.Lerp(image.color, night > 0 ? new Color(.35f, .48f, .76f) : Color.white, Time.unscaledDeltaTime * 3);
        }

        private void OnDisable() { if (_camera != null) _camera.rect = new Rect(0, 0, 1, 1); }
    }
}
