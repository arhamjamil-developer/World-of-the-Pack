using UnityEngine;

namespace WorldOfThePack.UI
{
    [CreateAssetMenu(menuName = "World of the Pack/UI/Hero Card")]
    public sealed class HeroCardDefinition : ScriptableObject
    {
        public string heroId;
        public string displayName;
        public string shortName;
        public Sprite humanPortrait;
        public Sprite wolfPortrait;
        public Color accent = Color.white;
        [Min(1)] public int maximumEnergy = 100;
    }
}
