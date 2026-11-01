using System.Collections;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public sealed class TelegraphIndicator : MonoBehaviour
    {
        private static Material ringMaterial;
        private LineRenderer outerRing, progressRing;
        public static GameObject Spawn(string name, Vector3 position, float duration, float radius = 1.2f)
        {
            var go = new GameObject("Telegraph_" + name);
            go.transform.position = position;
            var indicator = go.AddComponent<TelegraphIndicator>();
            indicator.outerRing = indicator.CreateRing("ThreatBoundary", radius, .025f);
            indicator.progressRing = indicator.CreateRing("WindupProgress", radius, .045f);
            indicator.StartCoroutine(indicator.Animate(Mathf.Max(.1f, duration)));
            return go;
        }
        private LineRenderer CreateRing(string name, float radius, float width)
        {
            if (ringMaterial == null) ringMaterial = new Material(Shader.Find("Sprites/Default"));
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = ringMaterial;
            line.useWorldSpace = false; line.loop = true; line.positionCount = 64;
            line.startWidth = line.endWidth = width; line.sortingOrder = 50;
            for (int i = 0; i < 64; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
            }
            return line;
        }
        private IEnumerator Animate(float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                if (GameDirector.Instance == null || GameDirector.Instance.CurrentState == GameState.GameOver ||
                    (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) { Destroy(gameObject); yield break; }
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                var color = Color.Lerp(new Color(1, .72f, .22f, .65f), new Color(1, .25f, .12f, .95f), t);
                outerRing.startColor = outerRing.endColor = color;
                progressRing.startColor = progressRing.endColor = color;
                progressRing.transform.localScale = Vector3.one * Mathf.Lerp(.15f, 1, t);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
