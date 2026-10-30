using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.UI
{
    public sealed class CardPortraitView : MonoBehaviour
    {
        public Image portrait;
        public HeroCardDefinition definition;
        public bool wolfForm = true;

        public void Bind(HeroCardDefinition hero, bool wolf)
        {
            definition = hero;
            SetForm(wolf);
        }

        public void SetForm(bool wolf)
        {
            wolfForm = wolf;
            if (portrait == null) return;
            portrait.sprite = definition == null ? null : wolf ? definition.wolfPortrait : definition.humanPortrait;
            portrait.enabled = portrait.sprite != null;
        }
    }
}
