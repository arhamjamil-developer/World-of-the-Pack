using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace WorldOfThePack.Expedition
{
    public sealed class TrailChoiceNode : MonoBehaviour, IPointerClickHandler
    {
        public TrailDecisionDirector director;
        public TrailChoice choice;
        public SpriteRenderer art, captive;
        public TMP_Text label;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) director.Choose(choice);
        }
        public void SetLabel(string text) { if (label != null) label.text = text; }
        public void SetSprite(Sprite sprite)
        {
            if (art == null || sprite == null) return;
            art.sprite = sprite; art.transform.localScale = Vector3.one * 1.15f / Mathf.Max(.01f, sprite.bounds.size.x);
        }
        public void SetCaptive(Sprite sprite)
        {
            if (captive == null) return;
            captive.sprite = sprite;
            if (sprite != null) captive.transform.localScale = Vector3.one * .56f / Mathf.Max(.01f, sprite.bounds.size.x);
        }
    }
}
