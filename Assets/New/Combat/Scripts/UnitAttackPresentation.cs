using UnityEngine;
using WorldOfThePack.Game;

namespace WorldOfThePack.CombatArt
{
    public sealed class UnitAttackPresentation : MonoBehaviour
    {
        [SerializeField] private AttackPresentationProfile _profile;
        [SerializeField] private Sprite[] _slashFrames;
        private WorldOfThePack.Expedition.PackUnitMotion _motion;
        private void Awake() => _motion = GetComponent<WorldOfThePack.Expedition.PackUnitMotion>();
        public void Windup(Vector3 target) { if (_motion != null) _motion.Strike(target); }
        public AttackPresentationProfile Profile => _profile;
        public float Duration => _profile != null ? _profile.animationDuration : .6f;
        public float ReleaseDelay => Duration * (_profile != null ? _profile.releaseFraction : .5f);
        public void Configure(AttackPresentationProfile profile) => _profile = profile;
        public void Impact(Vector3 position)
        {
            if (_profile == null) return;
            Vector3 direction = (position - transform.position).normalized;
            if (_profile.projectile == null)
                AttackSpriteEffect.Play(_slashFrames, transform.position + direction * .35f + Vector3.up * .15f, .72f,
                    Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 45);
            AttackSpriteEffect.Play(_profile.impactFrames, position + Vector3.up * .18f, _profile.impactSize * .5f);
        }
        public CombatSpriteProjectile Shoot(Combatant source, Combatant target, float damage, Combatant chainTarget = null, float chainFraction = 0)
        {
            return CombatSpriteProjectile.Launch(source.Position + Vector3.up * .45f, target, source, damage, _profile, chainTarget, chainFraction);
        }
    }
}
