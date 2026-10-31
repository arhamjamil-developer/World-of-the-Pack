using System.Collections;
using UnityEngine;
using WorldOfThePack.Game;

namespace WorldOfThePack.VFX
{
    public class EnergyOrbVFX : MonoBehaviour
    {
        private static Sprite cachedOrbSprite;

        public static void Spawn(Vector3 startPos, Vector3 endPos, float energyAmount, float travelTime = 0.65f)
        {
            GameObject orbObj = new GameObject("EnergyOrb");
            orbObj.transform.position = startPos;

            SpriteRenderer sr = orbObj.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrbSprite();
            sr.color = new Color(1f, 0.9f, 0.2f, 1f); // Bright radiant yellow
            sr.sortingOrder = 120;
            orbObj.transform.localScale = Vector3.one * 0.45f;

            EnergyOrbVFX runner = orbObj.AddComponent<EnergyOrbVFX>();
            runner.StartCoroutine(runner.TravelRoutine(startPos, endPos, energyAmount, travelTime));
        }

        private IEnumerator TravelRoutine(Vector3 start, Vector3 end, float energy, float duration)
        {
            // Control point for a pleasing arc
            Vector3 midPoint = (start + end) * 0.5f + Vector3.right * Random.Range(-0.8f, 0.8f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Quadratic Bezier curve
                float u = 1f - t;
                Vector3 currentPos = u * u * start + 2f * u * t * midPoint + t * t * end;
                transform.position = currentPos;

                // Pulsing sparkle scale
                float scale = 0.45f + 0.15f * Mathf.Sin(t * Mathf.PI * 4f);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            // Impact pop
            CombatVFXPool.SpawnEffect("MagicImpact", end);

            // Grant energy to cards
            CardDeckManager deck = FindFirstObjectByType<CardDeckManager>();
            if (deck != null)
            {
                deck.DistributeEnergy(energy);
            }

            Destroy(gameObject);
        }

        private static Sprite GetOrbSprite()
        {
            if (cachedOrbSprite != null) return cachedOrbSprite;
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= radius)
                    {
                        float alpha = 1f - Mathf.Pow(d / radius, 1.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            cachedOrbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return cachedOrbSprite;
        }
    }
}
