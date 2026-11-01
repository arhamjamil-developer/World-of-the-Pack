using UnityEngine;

namespace WorldOfThePack.Game
{
    [ExecuteAlways]
    public class LaneOverlayRenderer : MonoBehaviour
    {
        private static LaneOverlayRenderer instance;
        public static LaneOverlayRenderer Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<LaneOverlayRenderer>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("Lane Visuals")]
        [SerializeField] private Color normalDividerColor = new Color(1f, 1f, 1f, 0.08f);
        [SerializeField] private Color activeLaneColor = new Color(0.2f, 0.85f, 0.4f, 0.18f);
        [SerializeField] private Color alphaLaneColor = new Color(1f, 0.8f, 0.2f, 0.12f);
        [SerializeField] private float overlayHeight = 16f;

        private GameObject[] laneHighlights;
        private LineRenderer[] dividers;
        private int currentHighlightedLane = -1;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this && Application.isPlaying) { Destroy(this); return; }

            BuildOverlay();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            if (Camera.main != null)
            {
                // Follow camera Y smoothly
                Vector3 p = transform.position;
                p.x = 0f;
                p.y = Camera.main.transform.position.y;
                transform.position = p;
            }
        }

        public void BuildOverlay()
        {
            // Clear existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            laneHighlights = new GameObject[LaneSystem.LaneCount];
            dividers = new LineRenderer[LaneSystem.LaneCount - 1];

            // 1. Build Lane Highlight Columns
            for (int i = 0; i < LaneSystem.LaneCount; i++)
            {
                float laneX = LaneSystem.GetLaneX(i);
                GameObject col = new GameObject($"LaneCol_{i}");
                col.transform.SetParent(transform, false);
                col.transform.localPosition = new Vector3(laneX, 0f, 0f);

                SpriteRenderer sr = col.AddComponent<SpriteRenderer>();
                sr.sprite = CreateBoxSprite();
                sr.sortingOrder = -90;
                sr.color = Color.clear;
                col.transform.localScale = new Vector3(LaneSystem.LaneSpacing * 0.95f, overlayHeight, 1f);

                laneHighlights[i] = col;
            }

            // 2. Build 4 subtle divider lines between lanes
            for (int i = 0; i < LaneSystem.LaneCount - 1; i++)
            {
                float divX = (LaneSystem.GetLaneX(i) + LaneSystem.GetLaneX(i + 1)) * 0.5f;
                GameObject div = new GameObject($"Divider_{i}");
                div.transform.SetParent(transform, false);
                div.transform.localPosition = new Vector3(divX, 0f, 0f);

                LineRenderer lr = div.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3(0f, -overlayHeight * 0.5f, 0f));
                lr.SetPosition(1, new Vector3(0f, overlayHeight * 0.5f, 0f));
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                lr.material = GetLineMaterial();
                lr.startColor = Color.clear;
                lr.endColor = Color.clear;
                lr.sortingOrder = -89;

                dividers[i] = lr;
            }
        }

        public void HighlightLane(int laneIndex, bool active)
        {
            if (laneHighlights == null || laneHighlights.Length != LaneSystem.LaneCount) return;

            if (!active)
            {
                currentHighlightedLane = -1;
                for (int i = 0; i < LaneSystem.LaneCount; i++)
                {
                    if (laneHighlights[i] != null)
                    {
                        var sr = laneHighlights[i].GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = Color.clear;
                    }
                }
                return;
            }

            currentHighlightedLane = laneIndex;
            for (int i = 0; i < LaneSystem.LaneCount; i++)
            {
                if (laneHighlights[i] != null)
                {
                    var sr = laneHighlights[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (i == laneIndex)
                        {
                            sr.color = activeLaneColor;
                        }
                        else
                        {
                            sr.color = new Color(1f, 1f, 1f, 0.02f);
                        }
                    }
                }
            }
        }

        private Sprite CreateBoxSprite()
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(0, 1, Color.white);
            tex.SetPixel(1, 0, Color.white);
            tex.SetPixel(1, 1, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }

        private Material GetLineMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            return new Material(shader);
        }
    }
}
