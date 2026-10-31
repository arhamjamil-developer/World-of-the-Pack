using UnityEngine;

namespace WorldOfThePack.UI
{
    public class CardDragGhostPreview : MonoBehaviour
    {
        private static CardDragGhostPreview instance;
        public static CardDragGhostPreview Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<CardDragGhostPreview>();
                return instance;
            }
            private set => instance = value;
        }

        [SerializeField] private SpriteRenderer ghostRenderer;
        [SerializeField] private SpriteRenderer reticleRenderer;
        [SerializeField] private SpriteRenderer chevronRenderer;
        [SerializeField] private SpriteRenderer pillRenderer;
        [SerializeField] private LineRenderer pathLineRenderer;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            BuildPreviewObjectsIfNeeded();
            Hide();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            if (chevronRenderer != null && chevronRenderer.gameObject.activeInHierarchy)
            {
                float bob = Mathf.Sin(Time.time * 5.5f) * 0.08f;
                chevronRenderer.transform.localPosition = new Vector3(0f, 1.42f + bob, 0f);
            }
        }

        private void BuildPreviewObjectsIfNeeded()
        {
            if (ghostRenderer == null)
            {
                GameObject ghostGo = new GameObject("GhostSprite");
                ghostGo.transform.SetParent(transform, false);
                ghostRenderer = ghostGo.AddComponent<SpriteRenderer>();
                ghostRenderer.sortingOrder = 80;
            }

            if (reticleRenderer == null)
            {
                GameObject reticleGo = new GameObject("LandingReticle");
                reticleGo.transform.SetParent(transform, false);
                reticleGo.transform.localPosition = new Vector3(0f, -0.4f, 0f);
                reticleGo.transform.localScale = new Vector3(1.4f, 1.0f, 1f);
                reticleRenderer = reticleGo.AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
                var indSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/DeployIndicator_Hologram_HD.png");
                reticleRenderer.sprite = indSprite != null ? indSprite : GetReticleSprite();
#else
                reticleRenderer.sprite = GetReticleSprite();
#endif
                reticleRenderer.sortingOrder = 75;
            }

            if (chevronRenderer == null)
            {
                GameObject chevronGo = new GameObject("ChevronArrows");
                chevronGo.transform.SetParent(transform, false);
                chevronGo.transform.localPosition = new Vector3(0f, 1.42f, 0f);
                chevronGo.transform.localScale = Vector3.one * 1.35f;
                chevronRenderer = chevronGo.AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
                var chevSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Chevron_UpArrows.png");
                chevronRenderer.sprite = chevSprite;
#endif
                chevronRenderer.sortingOrder = 85;
            }

            if (pillRenderer == null)
            {
                GameObject pillGo = new GameObject("DeployPill");
                pillGo.transform.SetParent(transform, false);
                pillGo.transform.localPosition = new Vector3(0f, -0.92f, 0f);
                pillGo.transform.localScale = Vector3.one * 0.85f;
                pillRenderer = pillGo.AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
                var pillSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/DeployHere_Pill.png");
                pillRenderer.sprite = pillSprite;
#endif
                pillRenderer.sortingOrder = 86;
            }

            if (pathLineRenderer == null)
            {
                GameObject lineGo = new GameObject("PathLine");
                lineGo.transform.SetParent(transform, false);
                pathLineRenderer = lineGo.AddComponent<LineRenderer>();
                pathLineRenderer.useWorldSpace = true;
                pathLineRenderer.startWidth = 0.08f;
                pathLineRenderer.endWidth = 0.04f;
                pathLineRenderer.positionCount = 2;
                pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                pathLineRenderer.startColor = new Color(0.3f, 0.95f, 0.5f, 0.7f);
                pathLineRenderer.endColor = new Color(0.3f, 0.95f, 0.5f, 0.15f);
                pathLineRenderer.sortingOrder = 70;
            }
        }

        public void Show(Sprite heroSprite, Vector3 worldPos, Vector3 targetSlotPos, bool isValid)
        {
            BuildPreviewObjectsIfNeeded();
            gameObject.SetActive(true);
            transform.position = worldPos;

            if (ghostRenderer != null)
            {
                ghostRenderer.sprite = heroSprite;
                ghostRenderer.transform.localScale = Vector3.one * 0.44f;
                ghostRenderer.transform.localPosition = new Vector3(0f, 0.22f, 0f);
                ghostRenderer.color = isValid ? new Color(0.85f, 0.95f, 1.0f, 0.85f) : new Color(1f, 0.35f, 0.35f, 0.65f);
            }

            if (reticleRenderer != null)
            {
                reticleRenderer.color = isValid ? Color.white : new Color(0.95f, 0.25f, 0.25f, 0.85f);
            }

            if (chevronRenderer != null)
            {
                chevronRenderer.gameObject.SetActive(isValid);
            }

            if (pillRenderer != null)
            {
                pillRenderer.gameObject.SetActive(isValid);
            }

            if (pathLineRenderer != null)
            {
                pathLineRenderer.enabled = isValid;
                if (isValid)
                {
                    pathLineRenderer.SetPosition(0, worldPos);
                    pathLineRenderer.SetPosition(1, targetSlotPos);
                }
            }
        }

        public void UpdatePosition(Vector3 worldPos, Vector3 targetSlotPos, bool isValid)
        {
            transform.position = worldPos;

            if (ghostRenderer != null)
            {
                ghostRenderer.color = isValid ? new Color(0.75f, 0.9f, 1.0f, 0.70f) : new Color(1f, 0.35f, 0.35f, 0.65f);
            }

            if (reticleRenderer != null)
            {
                reticleRenderer.color = isValid ? Color.white : new Color(0.95f, 0.25f, 0.25f, 0.85f);
            }

            if (chevronRenderer != null)
            {
                chevronRenderer.gameObject.SetActive(isValid);
            }

            if (pillRenderer != null)
            {
                pillRenderer.gameObject.SetActive(isValid);
            }

            if (pathLineRenderer != null)
            {
                pathLineRenderer.enabled = isValid;
                if (isValid)
                {
                    pathLineRenderer.SetPosition(0, worldPos);
                    pathLineRenderer.SetPosition(1, targetSlotPos);
                }
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (pathLineRenderer != null) pathLineRenderer.enabled = false;
        }

        private static Sprite cachedReticleSprite;
        private static Sprite GetReticleSprite()
        {
            if (cachedReticleSprite != null) return cachedReticleSprite;
            int size = 128;
            Texture2D tex = new Texture2D(size, size);
            Color clear = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 4f;
            float inner = radius - 10f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= radius && d >= inner) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            cachedReticleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return cachedReticleSprite;
        }
    }
}
