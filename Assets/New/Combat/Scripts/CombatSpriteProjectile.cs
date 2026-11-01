using UnityEngine;
using WorldOfThePack.Game;

namespace WorldOfThePack.CombatArt
{
    /// <summary>Separate authored projectile; damage lands at arrival, with source attribution.</summary>
    public sealed class CombatSpriteProjectile : MonoBehaviour
    {
        private Combatant _target, _source, _chainTarget;
        private AttackPresentationProfile _profile;
        private float _damage, _age, _chainFraction;
        public static int SpawnedCount { get; private set; }
        public static int ImpactCount { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCounts() { SpawnedCount = 0; ImpactCount = 0; }
        public static CombatSpriteProjectile Launch(Vector3 origin, Combatant target, Combatant source, float damage,
            AttackPresentationProfile profile, Combatant chainTarget = null, float chainFraction = 0)
        {
            if (target == null || !target.IsAlive || profile == null || profile.projectile == null) return null;
            var go = new GameObject("Projectile_" + profile.attackName); go.transform.position = origin;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = profile.projectile; renderer.sortingOrder = 550;
            go.transform.localScale = Vector3.one * profile.projectileWidth / Mathf.Max(.01f, profile.projectile.bounds.size.x);
            var projectile = go.AddComponent<CombatSpriteProjectile>();
            projectile._target = target; projectile._source = source; projectile._profile = profile;
            projectile._damage = damage; projectile._chainTarget = chainTarget; projectile._chainFraction = chainFraction;
            projectile.Aim(); SpawnedCount++; return projectile;
        }
        private void Aim()
        {
            Vector3 direction = _target.Position + Vector3.up * .35f - transform.position;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
        private void Update()
        {
            if (GameDirector.Instance == null || GameDirector.Instance.CurrentState != GameState.Playing ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) return;
            if (_target == null || !_target.IsAlive) { Destroy(gameObject); return; }
            _age += Time.deltaTime;
            if (_age > 5) { Destroy(gameObject); return; }
            Aim();
            Vector3 endpoint = _target.Position + Vector3.up * .35f;
            transform.position = Vector3.MoveTowards(transform.position, endpoint, _profile.projectileSpeed * Time.deltaTime);
            if ((transform.position - endpoint).sqrMagnitude > .01f) return;
            _target.TakeDamage(_damage, _source); ImpactCount++;
            AttackSpriteEffect.Play(_profile.impactFrames, endpoint, _profile.impactSize);
            if (_chainTarget != null && _chainTarget.IsAlive && _chainFraction > 0)
                Launch(endpoint, _chainTarget, _source, _damage * _chainFraction, _profile);
            Destroy(gameObject);
        }
    }
}
