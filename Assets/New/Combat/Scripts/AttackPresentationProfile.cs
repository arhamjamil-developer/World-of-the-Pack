using UnityEngine;

namespace WorldOfThePack.CombatArt
{
    [CreateAssetMenu(menuName = "World of the Pack/Combat/Attack Presentation")]
    public sealed class AttackPresentationProfile : ScriptableObject
    {
        public string attackName;
        [Min(.1f)] public float animationDuration = .6f;
        [Range(0, 1)] public float releaseFraction = .5f;
        public Sprite projectile;
        [Min(.1f)] public float projectileSpeed = 7.5f;
        [Min(.01f)] public float projectileWidth = .55f;
        public Sprite[] impactFrames;
        [Min(.01f)] public float impactSize = 1.1f;
        public Color trailColor = Color.white;
        public bool doubleStrike;
        [Min(0)] public float cleaveRadius;
        [Range(0, 1)] public float cleaveFraction = .25f;
    }
}
