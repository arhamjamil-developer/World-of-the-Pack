using UnityEngine;

namespace WorldOfThePack.Expedition
{
    public sealed class PackImpactPulse : MonoBehaviour
    {
        private static Material _material;
        private LineRenderer _ring;
        private Color _color;
        private float _age, _radius;
        public static void Spawn(Vector3 position, Color color, float radius)
        {
            var go = new GameObject("CleanImpactPulse"); go.transform.position = position;
            var pulse = go.AddComponent<PackImpactPulse>(); pulse._radius = radius; pulse._color = color;
            pulse._ring = go.AddComponent<LineRenderer>();
            if (_material == null) _material = new Material(Shader.Find("Sprites/Default"));
            pulse._ring.sharedMaterial = _material; pulse._ring.useWorldSpace = false;
            pulse._ring.positionCount = 64; pulse._ring.loop = true; pulse._ring.sortingOrder = 350;
        }
        private void Update()
        {
            _age += Time.deltaTime;
            float t = _age / .4f;
            if (t >= 1) { Destroy(gameObject); return; }
            float radius = Mathf.Lerp(.05f, _radius, 1 - Mathf.Pow(1 - t, 3));
            for (int i = 0; i < 64; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                _ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
            }
            _ring.startWidth = _ring.endWidth = Mathf.Lerp(.085f, .01f, t);
            var color = _color; color.a *= 1 - t;
            _ring.startColor = _ring.endColor = color;
        }
    }
}
