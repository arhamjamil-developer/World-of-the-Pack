using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.VFX;

namespace WorldOfThePack.Game
{
    public class CombatVFXPool : MonoBehaviour
    {
        private static CombatVFXPool instance;
        public static CombatVFXPool Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CombatVFXPool");
                    instance = go.AddComponent<CombatVFXPool>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

        [System.Serializable]
        public struct NamedPrefab
        {
            public string name;
            public GameObject prefab;
        }

        [SerializeField] private List<NamedPrefab> registeredPrefabs = new List<NamedPrefab>();

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(gameObject); return; }

            foreach (var item in registeredPrefabs)
            {
                if (item.prefab != null && !prefabCache.ContainsKey(item.name))
                {
                    prefabCache[item.name] = item.prefab;
                }
            }
        }

        public static GameObject SpawnEffect(string effectName, Vector3 position, Quaternion rotation = default)
        {
            return Instance.GetOrSpawn(effectName, position, rotation);
        }

        public GameObject GetOrSpawn(string effectName, Vector3 position, Quaternion rotation = default)
        {
            if (string.IsNullOrEmpty(effectName)) return null;

            if (rotation == default) rotation = Quaternion.identity;

            if (pool.TryGetValue(effectName, out var queue) && queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null)
                {
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.SetActive(true);

                    var flipbook = obj.GetComponent<SpriteFlipbookEffect>();
                    if (flipbook != null) flipbook.Play();

                    return obj;
                }
            }

            GameObject prefab = GetPrefab(effectName);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, position, rotation);
                instance.name = effectName;

                var flipbook = instance.GetComponent<SpriteFlipbookEffect>();
                if (flipbook != null)
                {
                    flipbook.completed.AddListener(() => ReturnToPool(effectName, instance));
                }
                else
                {
                    Destroy(instance, 1.2f);
                }
                return instance;
            }

            // Fallback subtle spark effect if prefab not found
            return CreateProceduralSpark(effectName, position);
        }

        private void ReturnToPool(string effectName, GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            if (!pool.ContainsKey(effectName))
            {
                pool[effectName] = new Queue<GameObject>();
            }
            pool[effectName].Enqueue(obj);
        }

        private GameObject GetPrefab(string effectName)
        {
            if (prefabCache.TryGetValue(effectName, out GameObject found)) return found;

            // Search in Resources or standard VFX folder
            GameObject loaded = Resources.Load<GameObject>("VFX/" + effectName);
            if (loaded != null)
            {
                prefabCache[effectName] = loaded;
                return loaded;
            }

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets(effectName + " t:Prefab");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Assets/VFX/WorldOfThePack/Prefabs/"))
                {
                    GameObject p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (p != null)
                    {
                        prefabCache[effectName] = p;
                        return p;
                    }
                }
            }
#endif
            return null;
        }

        private GameObject CreateProceduralSpark(string effectName, Vector3 position)
        {
            GameObject spark = new GameObject("Spark_" + effectName);
            spark.transform.position = position;
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = GetSparkSprite();
            sr.color = new Color(1f, 0.9f, 0.3f, 0.9f);
            sr.sortingOrder = 900;
            spark.transform.localScale = Vector3.one * 0.8f;
            Destroy(spark, 0.35f);
            return spark;
        }

        private static Sprite cachedSpark;
        private static Sprite GetSparkSprite()
        {
            if (cachedSpark != null) return cachedSpark;
            int size = 16;
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= size / 2f) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            cachedSpark = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return cachedSpark;
        }
    }
}
