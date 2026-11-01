using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    public sealed class MoonshiftAtmosphere : MonoBehaviour
    {
        public WorldBiomeGenerator world;
        public Image moonGlow;
        public Image[] fireflies;
        public Color nightTint = new Color(.35f, .48f, .76f, 1);
        public float Blend { get; private set; }
        private readonly Dictionary<SpriteRenderer, Color> _terrain = new Dictionary<SpriteRenderer, Color>();
        private readonly List<SpriteRenderer> _expired = new List<SpriteRenderer>();
        private float _scan, _clock;
        private void LateUpdate()
        {
            var game = GameDirector.Instance;
            if (game == null) return;
            float target = game.IsWolfInstinctActive ? 1 : 0;
            Blend = Mathf.MoveTowards(Blend, target, Time.unscaledDeltaTime / 1.25f);
            if (game.CurrentState == GameState.Playing) _clock += Time.deltaTime;
            _scan -= Time.unscaledDeltaTime;
            if (_scan <= 0 && world != null)
            {
                _scan = .5f;
                foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                    if (sr.sortingOrder <= -40 && !_terrain.ContainsKey(sr)) _terrain.Add(sr, sr.color);
                _expired.Clear();
                foreach (var entry in _terrain) if (entry.Key == null) _expired.Add(entry.Key);
                foreach (var sr in _expired) _terrain.Remove(sr);
            }
            foreach (var entry in _terrain) if (entry.Key != null) entry.Key.color = Color.Lerp(entry.Value, entry.Value * nightTint, Blend);
            if (moonGlow != null) moonGlow.color = new Color(.55f, .72f, 1f, .35f * Blend);
            if (fireflies == null) return;
            for (int i = 0; i < fireflies.Length; i++)
            {
                var fly = fireflies[i];
                if (fly == null) continue;
                fly.color = new Color(1f, .84f, .39f, Blend * (.35f + .45f * Mathf.Sin(_clock * 1.4f + i * 2.1f)));
                var rt = fly.rectTransform;
                rt.anchoredPosition = new Vector2(Mathf.Sin(_clock * .3f + i * 2.7f) * 13f, Mathf.Cos(_clock * .45f + i) * 19f);
            }
        }
        private void OnDisable()
        {
            foreach (var entry in _terrain) if (entry.Key != null) entry.Key.color = entry.Value;
        }
    }
}
