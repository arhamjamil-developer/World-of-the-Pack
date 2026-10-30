using UnityEngine;

namespace WorldOfThePack.VFX
{
    [CreateAssetMenu(menuName = "World of the Pack/Combat Effect")]
    public sealed class CombatEffectDefinition : ScriptableObject
    {
        public string group;
        [TextArea] public string description;
        public Sprite[] frames;
        [Min(1)] public float framesPerSecond = 12;
        public int sortingOrder = 18;
        public bool staticMarker;
        public AnimationClip animationClip;
    }
}
