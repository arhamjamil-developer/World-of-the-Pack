using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.NewUI;

namespace WorldOfThePack.Expedition.Editor
{
    public static class TaskPanelPolish
    {
        private static TMP_FontAsset _font;
        private static Sprite _frame;
        [MenuItem("World of the Pack/Royal Escort/Polish Task Sidebar")]
        public static string Build()
        {
            if(EditorApplication.isPlaying)return "Stop Play Mode first.";
            var hud=Object.FindFirstObjectByType<PackGameplayHUD>();var tasks=hud.GetComponent<PackTaskDirector>();
            var safe=hud.transform.Find("SafeArea");
            _font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/New/Fonts/Simpletix SDF.asset") ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            _frame=Resources.Load<Sprite>("PackHUD/TaskPanelCompact") ?? Resources.Load<Sprite>("RoyalEscort/TutorialPanel");
            var prior=safe.Find("TaskSidebar");if(prior!=null)Undo.DestroyObjectImmediate(prior.gameObject);
            var root=Rect("TaskSidebar",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            Undo.RegisterCreatedObjectUndo(root.gameObject,"Polish task sidebar");
            var panel=Rect("TaskJournal",root,new Vector2(0,.64f),new Vector2(0,.64f),new Vector2(64,0),new Vector2(390,324));panel.pivot=new Vector2(0,.5f);
            var outer=panel.gameObject.AddComponent<Image>();outer.sprite=_frame;outer.type=Image.Type.Sliced;outer.color=Color.white;
            var group=panel.gameObject.AddComponent<CanvasGroup>();
            var paper=Rect("Ivory reading inset",panel,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var title=Text("Objective",paper,new Vector2(24,-20),new Vector2(304,58),"Clear the trail",28,new Color(.16f,.12f,.08f),FontStyles.Bold);
            var instruction=Text("Instruction",paper,new Vector2(24,-82),new Vector2(342,60),"Defeat scouts",25,new Color(.23f,.18f,.12f),FontStyles.Bold);
            var progress=Text("Progress count",paper,new Vector2(24,-150),new Vector2(342,29),"0 / 3 enemies defeated",23,new Color(.15f,.28f,.26f),FontStyles.Bold);
            var track=Rect("Progress track",paper,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-187),new Vector2(342,9));track.pivot=new Vector2(0,.5f);
            track.gameObject.AddComponent<Image>().color=new Color(.16f,.3f,.27f,.2f);
            var fill=Rect("Progress fill",track,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<Image>();
            fill.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.fillAmount=0;
            var reward=Text("Reward",paper,new Vector2(24,-202),new Vector2(342,29),"Reward +3 charms",23,new Color(.42f,.25f,.06f),FontStyles.Bold);
            var tip=Text("Small tactical tip",paper,new Vector2(24,-244),new Vector2(342,61),"Tip: Guard ranged allies with a tank.",22,new Color(.3f,.34f,.32f),FontStyles.Bold);
            var closeRect=Rect("Close journal",panel,new Vector2(1,1),new Vector2(1,1),new Vector2(-26,-26),new Vector2(40,40));
            closeRect.gameObject.AddComponent<Image>().color=new Color(.22f,.29f,.25f,.14f);
            var close=closeRect.gameObject.AddComponent<Button>();
            var x=Text("Close label",closeRect,Vector2.zero,new Vector2(40,40),"×",30,new Color(.19f,.25f,.21f));
            x.rectTransform.anchorMin=x.rectTransform.anchorMax=x.rectTransform.pivot=new Vector2(.5f,.5f);x.rectTransform.anchoredPosition=Vector2.zero;x.alignment=TextAlignmentOptions.Center;
            var handle=Rect("Quiet left task handle",root,new Vector2(0,.64f),new Vector2(0,.64f),new Vector2(28,0),new Vector2(56,144));
            handle.gameObject.AddComponent<Image>().color=new Color(.08f,.19f,.19f,.86f);
            var rim=handle.gameObject.AddComponent<Outline>();rim.effectColor=new Color(.74f,.57f,.24f,.75f);rim.effectDistance=new Vector2(1,-1);
            var button=handle.gameObject.AddComponent<Button>();
            var label=Text("Task tab label",handle,Vector2.zero,new Vector2(100,34),"TASK",23,new Color(.96f,.88f,.66f),FontStyles.Bold);
            label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=new Vector2(.5f,.5f);label.rectTransform.anchoredPosition=new Vector2(0,12);label.rectTransform.localRotation=Quaternion.Euler(0,0,90);label.alignment=TextAlignmentOptions.Center;
            var count=Text("Tab progress",handle,Vector2.zero,new Vector2(52,25),"0/3",18,new Color(.77f,.87f,.81f));
            count.rectTransform.anchorMin=count.rectTransform.anchorMax=count.rectTransform.pivot=new Vector2(.5f,.5f);count.rectTransform.anchoredPosition=new Vector2(0,-49);count.alignment=TextAlignmentOptions.Center;
            var dot=Rect("New task dot",handle,new Vector2(1,1),new Vector2(1,1),new Vector2(-8,-8),new Vector2(7,7)).gameObject.AddComponent<Image>();dot.color=new Color(.98f,.76f,.25f);
            var drawer=hud.GetComponent<PackTaskDrawer>()??Undo.AddComponent<PackTaskDrawer>(hud.gameObject);
            var data=new SerializedObject(drawer);
            Bind(data,"_tasks",tasks);Bind(data,"_root",root.gameObject);Bind(data,"_panel",panel);Bind(data,"_panelGroup",group);Bind(data,"_handle",button);Bind(data,"_close",close);
            Bind(data,"_title",title);Bind(data,"_instruction",instruction);Bind(data,"_progress",progress);Bind(data,"_reward",reward);Bind(data,"_tip",tip);Bind(data,"_handleProgress",count);Bind(data,"_fill",fill);Bind(data,"_unreadDot",dot);
            data.ApplyModifiedProperties();EditorUtility.SetDirty(drawer);
            // Keep the legacy references intact, but hide their old full-width presentation.
            if(tasks.titleLabel!=null)tasks.titleLabel.enabled=false;if(tasks.detailLabel!=null)tasks.detailLabel.enabled=false;if(tasks.progressFill!=null)tasks.progressFill.enabled=false;
            var old=tasks.titleLabel!=null?tasks.titleLabel.transform.parent.Find("TaskPlate"):null;if(old!=null)old.GetComponent<Image>().enabled=false;
            panel.anchoredPosition=new Vector2(-410,0);group.alpha=0;group.interactable=group.blocksRaycasts=false;
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);
            return "Task sidebar authored and saved. Existing card and task catalog references preserved.";
        }
        private static void Bind(SerializedObject data,string key,Object value)=>data.FindProperty(key).objectReferenceValue=value;
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;
            r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        private static RectTransform Box(string name,Transform parent,Vector2 min,Vector2 max,Color color)
        {var r=Rect(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);r.offsetMin=min;r.offsetMax=max;r.gameObject.AddComponent<Image>().color=color;return r;}
        private static TMP_Text Text(string name,Transform parent,Vector2 position,Vector2 size,string copy,int fontSize,Color color,FontStyles style=FontStyles.Normal)
        {
            var r=Rect(name,parent,new Vector2(0,1),new Vector2(0,1),position,size);r.pivot=new Vector2(0,1);
            var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=_font;t.text=copy;t.fontSize=fontSize;t.fontStyle=style;t.color=color;t.alignment=TextAlignmentOptions.TopLeft;
            t.raycastTarget=false;t.overflowMode=TextOverflowModes.Ellipsis;t.textWrappingMode=TextWrappingModes.Normal;return t;
        }
    }
}
