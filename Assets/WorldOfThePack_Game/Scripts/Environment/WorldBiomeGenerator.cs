using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    [Serializable]
    public class BiomeSegmentConfig
    {
        public BiomeType biomeType;
        public string biomeName;
        public Sprite groundTileSprite;
        public Sprite landmarkSprite;
        public Color ambientColor = Color.white;
        public Color fogColor = new Color(0.1f, 0.2f, 0.25f, 0.3f);
    }

    public class WorldBiomeGenerator : MonoBehaviour
    {
        private static WorldBiomeGenerator instance;
        public static WorldBiomeGenerator Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<WorldBiomeGenerator>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("Biome Configurations")]
        [SerializeField] private List<BiomeSegmentConfig> biomes = new List<BiomeSegmentConfig>();
        [SerializeField] private BiomeType currentBiome = BiomeType.SunnyForestTrail;

        [Header("Tile & Segment Settings")]
        [SerializeField] private float segmentHeight = 21.72f;
        [SerializeField] private int activeSegmentCount = 4;
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private float corridorWidth = 3.6f; // 42% width combat corridor (-1.8 to +1.8)

        [Header("Landmark & Camp Prefabs")]
        [SerializeField] private Sprite lanternRefugeSprite;
        [SerializeField] private Sprite moonstoneArchSprite;
        [SerializeField] private Sprite fallenGiantSprite;
        [SerializeField] private Sprite riverBridgeSprite;

        private readonly List<GameObject> activeSegments = new List<GameObject>();
        private Camera targetCamera;
        private float lastGeneratedY = 0f;
        private int segmentsSpawned = 0;

        public BiomeType CurrentBiome => currentBiome;
        public string CurrentBiomeName => biomes.Count > (int)currentBiome ? biomes[(int)currentBiome].biomeName : "Wilderness";

        public event Action<BiomeType, string> OnBiomeChanged;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            LoadSpritesIfNeeded();
            BuildDefaultBiomes();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Start()
        {
            targetCamera = Camera.main;
            InitializeWorld();
        }

        private void LoadSpritesIfNeeded()
        {
#if UNITY_EDITOR
            if (lanternRefugeSprite == null)
                lanternRefugeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-patch-lantern-refuge-v01.png");
            if (moonstoneArchSprite == null)
                moonstoneArchSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-patch-moonstone-arch-v01.png");
            if (fallenGiantSprite == null)
                fallenGiantSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-patch-fallen-giant-v01.png");
            if (riverBridgeSprite == null)
                riverBridgeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-patch-broad-river-bridge-v01.png");
#endif
        }

        private void BuildDefaultBiomes()
        {
            if (biomes.Count >= 5) return;
            biomes.Clear();

#if UNITY_EDITOR
            // 1. Sunny Forest Trail
            biomes.Add(new BiomeSegmentConfig
            {
                biomeType = BiomeType.SunnyForestTrail,
                biomeName = "Sunny Forest Trail",
                groundTileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-sunny-forest-master-v01.png"),
                landmarkSprite = fallenGiantSprite,
                ambientColor = new Color(1f, 1f, 0.95f, 1f)
            });

            // 2. Mushroom Grove
            biomes.Add(new BiomeSegmentConfig
            {
                biomeType = BiomeType.MushroomGrove,
                biomeName = "Mushroom Grove",
                groundTileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-mushroom-grove-master-v01.png"),
                ambientColor = new Color(0.9f, 0.85f, 1f, 1f)
            });

            // 3. River Meadow
            biomes.Add(new BiomeSegmentConfig
            {
                biomeType = BiomeType.RiverMeadow,
                biomeName = "River Meadow",
                groundTileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-river-meadow-master-v01.png"),
                landmarkSprite = riverBridgeSprite,
                ambientColor = new Color(0.85f, 0.95f, 1f, 1f)
            });

            // 4. Emberlane Refuge
            biomes.Add(new BiomeSegmentConfig
            {
                biomeType = BiomeType.EmberlaneRefuge,
                biomeName = "Emberlane Refuge",
                groundTileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-autumn-orchard-master-v01.png"),
                landmarkSprite = lanternRefugeSprite,
                ambientColor = new Color(1f, 0.9f, 0.8f, 1f)
            });

            // 5. Cloudstep Highlands
            biomes.Add(new BiomeSegmentConfig
            {
                biomeType = BiomeType.CloudstepHighlands,
                biomeName = "Cloudstep Highlands",
                groundTileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-moonlit-glade-master-v01.png"),
                landmarkSprite = moonstoneArchSprite,
                ambientColor = new Color(0.8f, 0.9f, 1f, 1f)
            });
#endif
        }

        public void SetBiome(BiomeType biome)
        {
            if (currentBiome == biome) return;
            currentBiome = biome;
            OnBiomeChanged?.Invoke(currentBiome, CurrentBiomeName);
        }

        public void ResetWorld()
        {
            InitializeWorld();
        }

        private void InitializeWorld()
        {
            // Clean up any existing segments thoroughly
            foreach (var s in activeSegments)
            {
                if (s != null)
                {
                    if (Application.isPlaying) Destroy(s);
                    else DestroyImmediate(s);
                }
            }
            activeSegments.Clear();

            // Clear any lingering child objects under transform
            var toRemove = new List<GameObject>();
            foreach (Transform child in transform)
            {
                toRemove.Add(child.gameObject);
            }
            foreach (var go in toRemove)
            {
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }

            segmentsSpawned = 0;
            // Center the primary initial segment on origin (Y = 0) with a buffer segment behind
            float startY = -segmentHeight;
            lastGeneratedY = startY;

            for (int i = 0; i < activeSegmentCount; i++)
            {
                SpawnNextSegment();
            }
        }

        private void Update()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            float camY = targetCamera != null ? targetCamera.transform.position.y : 0f;

            // Generate segment ahead
            if (lastGeneratedY - camY < segmentHeight * 2.0f)
            {
                SpawnNextSegment();
            }

            // Recycle segments far below viewport
            float despawnY = camY - segmentHeight * 2.2f;
            for (int i = activeSegments.Count - 1; i >= 0; i--)
            {
                GameObject seg = activeSegments[i];
                if (seg != null && seg.transform.position.y < despawnY)
                {
                    activeSegments.RemoveAt(i);
                    if (Application.isPlaying) Destroy(seg);
                    else DestroyImmediate(seg);
                }
            }
        }

        private void SpawnNextSegment()
        {
            BiomeSegmentConfig config = GetCurrentBiomeConfig();
            Sprite sprite = config != null ? config.groundTileSprite : null;

            // Safety fallback to guarantee terrain is never blank
            if (sprite == null)
            {
                LoadSpritesIfNeeded();
                BuildDefaultBiomes();
                config = GetCurrentBiomeConfig();
                sprite = config != null ? config.groundTileSprite : null;
            }

            GameObject segmentObj = new GameObject($"BiomeSegment_{segmentsSpawned}_{currentBiome}");
            segmentObj.transform.SetParent(transform, false);
            segmentObj.transform.position = new Vector3(0f, lastGeneratedY, 0f);

            // Ground layer
            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(segmentObj.transform, false);
            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            if (terrainMaterial != null) sr.sharedMaterial = terrainMaterial;
            sr.sprite = sprite;
            sr.sortingOrder = -100;
            if (config != null) sr.color = config.ambientColor;

            // Outer scenery dressings placed strictly outside corridor (|x| > 2.0f)
            CreateOuterBorderDecorations(segmentObj.transform);

            // Occasional Landmark or Refuge
            if (segmentsSpawned > 0 && segmentsSpawned % 6 == 0 && config != null && config.landmarkSprite != null)
            {
                CreateLandmark(segmentObj.transform, config.landmarkSprite);
            }

            activeSegments.Add(segmentObj);
            lastGeneratedY += segmentHeight;
            segmentsSpawned++;
        }

        private void CreateOuterBorderDecorations(Transform parent)
        {
            // Trees and rocks along left and right borders outside combat corridor
            float[] sides = { -2.35f, 2.35f };
            foreach (float sideX in sides)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject prop = new GameObject("EdgeProp_" + i);
                    prop.transform.SetParent(parent, false);
                    prop.transform.localPosition = new Vector3(sideX + UnityEngine.Random.Range(-0.25f, 0.25f), i * (segmentHeight / 3f) - (segmentHeight * 0.4f), 0f);
                }
            }
        }

        private void CreateLandmark(Transform parent, Sprite landmarkSprite)
        {
            GameObject landmark = new GameObject("Landmark");
            landmark.transform.SetParent(parent, false);
            landmark.transform.localPosition = new Vector3(0f, 0f, 0f);

            SpriteRenderer sr = landmark.AddComponent<SpriteRenderer>();
            sr.sprite = landmarkSprite;
            sr.sortingOrder = -50;
        }

        private BiomeSegmentConfig GetCurrentBiomeConfig()
        {
            int idx = (int)currentBiome;
            if (idx >= 0 && idx < biomes.Count) return biomes[idx];
            return biomes.Count > 0 ? biomes[0] : null;
        }
    }
}
