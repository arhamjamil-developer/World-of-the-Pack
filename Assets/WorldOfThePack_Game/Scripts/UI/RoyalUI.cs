using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public static class RoyalUI
    {
        public static void Place(RectTransform rect,Vector2 min,Vector2 max)
        { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
        public static GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
            Place(go.GetComponent<RectTransform>(),min,max);
            var image=go.GetComponent<Image>(); image.color=new Color(.12f,.065f,.025f,.97f);
            var sprite=Resources.Load<Sprite>("RoyalEscort/TutorialPanel");
            if(sprite!=null) { image.sprite=sprite; image.type=Image.Type.Sliced; image.color=Color.white; }
            var outline=go.AddComponent<Outline>(); outline.effectColor=new Color(.95f,.68f,.2f); outline.effectDistance=new Vector2(3,-3);
            return go;
        }
        public static TMP_Text Label(Transform parent,string name,Vector2 min,Vector2 max,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(parent,false);
            Place(go.GetComponent<RectTransform>(),min,max); var text=go.GetComponent<TextMeshProUGUI>();
            text.fontSize=size; text.color=new Color(1,.94f,.76f); text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false;
            return text;
        }
        public static Button Button(Transform parent,string title,Vector2 min,Vector2 max,UnityAction action)
        {
            var go=Panel(parent,title,min,max); go.GetComponent<Image>().color=new Color(.35f,.46f,.27f);
            var button=go.AddComponent<Button>(); button.onClick.AddListener(action);
            Label(go.transform,"Label",new Vector2(.03f,.05f),new Vector2(.97f,.95f),28).text=title;
            return button;
        }
    }
}
