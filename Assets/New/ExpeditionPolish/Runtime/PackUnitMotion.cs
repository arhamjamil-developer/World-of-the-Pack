using UnityEngine;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    /// <summary>Small body-only motion; never moves hitboxes, selection rings or health bars.</summary>
    public sealed class PackUnitMotion : MonoBehaviour
    {
        private Transform[] _bodies;
        private Vector3[] _origins;
        private Quaternion[] _rotations;
        private Vector3 _lastPosition;
        private Combatant _unit;
        private float _phase, _strike;
        private Vector3 _strikeDirection;
        private void Awake()
        {
            _unit = GetComponent<Combatant>();
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            var bodies = new System.Collections.Generic.List<Transform>();
            foreach (var sr in renderers) if (sr.name == "Body") bodies.Add(sr.transform);
            _bodies = bodies.ToArray();
            _origins = new Vector3[_bodies.Length];
            _rotations = new Quaternion[_bodies.Length];
            for (int i = 0; i < _bodies.Length; i++) { _origins[i] = _bodies[i].localPosition; _rotations[i] = _bodies[i].localRotation; }
            _phase = Mathf.Abs(transform.GetInstanceID() % 17) * .37f;
            _lastPosition = transform.position;
        }
        public void Strike(Vector3 target)
        {
            _strikeDirection = Vector3.ClampMagnitude(target - transform.position, 1);
            _strike = .32f;
        }
        private void LateUpdate()
        {
            var delta = transform.position - _lastPosition;
            _lastPosition = transform.position;
            if (_unit == null || !_unit.IsAlive || GameDirector.Instance == null || GameDirector.Instance.CurrentState != GameState.Playing ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) return;
            float speed = delta.magnitude / Mathf.Max(.001f, Time.deltaTime);
            _phase += Time.deltaTime * Mathf.Lerp(2f, 11f, Mathf.Clamp01(speed));
            _strike = Mathf.Max(0, _strike - Time.deltaTime);
            float pulse = Mathf.Sin(Mathf.Clamp01(_strike / .32f) * Mathf.PI);
            float bob = Mathf.Sin(_phase) * (speed > .08f ? .055f : .012f);
            float lean = Mathf.Clamp(-delta.x * 100, -5, 5);
            for (int i = 0; i < _bodies.Length; i++)
            {
                if (_bodies[i] == null) continue;
                _bodies[i].localPosition = _origins[i] + Vector3.up * Mathf.Abs(bob) + _strikeDirection * (.12f * pulse);
                _bodies[i].localRotation = _rotations[i] * Quaternion.Euler(0, 0, lean + Mathf.Sin(_phase) * (speed > .08f ? 1.5f : .25f));
            }
        }
    }
}
