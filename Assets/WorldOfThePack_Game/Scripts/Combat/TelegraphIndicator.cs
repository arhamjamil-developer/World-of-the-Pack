using System.Collections;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class TelegraphIndicator : MonoBehaviour
    {
        public static GameObject Spawn(string prefabName, Vector3 position, float duration)
        {
            GameObject prefab = Resources.Load<GameObject>("Telegraphs/" + prefabName);
            GameObject instance = null;

            if (prefab != null)
            {
                instance = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // Fallback procedural warning circle if prefab not in Resources
                instance = new GameObject("Telegraph_" + prefabName);
                instance.transform.position = position;
                SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                sr.sprite = GetCircleSprite();
                sr.color = new Color(1f, 0.2f, 0.2f, 0.4f);
                sr.sortingOrder = -10;
                instance.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            }

            TelegraphIndicator ti = instance.AddComponent<TelegraphIndicator>();
            ti.StartCoroutine(ti.FadeAndDestroy(duration));
            return instance;
        }

        private IEnumerator FadeAndDestroy(float duration)
        {
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                // Subtle pulse effect
                transform.localScale = initialScale * (1f + 0.1f * Mathf.Sin(progress * Mathf.PI * 4f));
                yield return null;
            }

            Destroy(gameObject);
        }

        private static Sprite cachedCircle;
        private static Sprite GetCircleSprite()
        {
            if (cachedCircle != null) return cachedCircle;
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= size / 2f) tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((size / 2f - d) / 4f)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            cachedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return cachedCircle;
        }
    }
}
