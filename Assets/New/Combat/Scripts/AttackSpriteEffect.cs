using UnityEngine;

namespace WorldOfThePack.CombatArt
{
    public sealed class AttackSpriteEffect : MonoBehaviour
    {
        private Sprite[] _frames;
        private SpriteRenderer _renderer;
        private float _elapsed;
        public static void Play(Sprite[] frames, Vector3 position, float size, float angle = 0)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null) return;
            var go = new GameObject("AttackImpact_" + frames[0].name);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.sortingOrder = 600;
            go.transform.localScale = Vector3.one * size / Mathf.Max(.01f, frames[0].bounds.size.x);
            var effect = go.AddComponent<AttackSpriteEffect>(); effect._frames = frames; effect._renderer = sr;
        }
        private void Update()
        {
            _elapsed += Time.deltaTime;
            int index = Mathf.FloorToInt(_elapsed * 22);
            if (index >= _frames.Length) { Destroy(gameObject); return; }
            _renderer.sprite = _frames[index];
        }
    }
}
