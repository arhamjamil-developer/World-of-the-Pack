using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using WorldOfThePack.Expedition.Editor;
using WorldOfThePack.NewUI;

namespace WorldOfThePack.NewUI.Editor
{
    public static class HUDSpacePolish
    {
        public const string FontPath="Assets/New/Fonts/Simpletix SDF.asset";
        public static string Build()
        {
            if(EditorApplication.isPlaying)return "Stop Play Mode first.";
            AssetDatabase.Refresh();EnsureFont();
            Import("Assets/New/Art/Resources/PackHUD/TaskPanelCompact.png",70);
            Import("Assets/New/Art/Resources/PackHUD/TaskMiniBanner_v2.png",70);
            Import("Assets/New/Art/Resources/PackHUD/HUDToggleBanner.png",30);
            TaskPanelPolish.Build();
            var hud=Object.FindFirstObjectByType<PackGameplayHUD>();var safe=hud.transform.Find("SafeArea");
            var prior=safe.Find("HUDVisibilityControls");if(prior!=null)Undo.DestroyObjectImmediate(prior.gameObject);
            var root=new GameObject("HUDVisibilityControls",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rootRect=(RectTransform)root.transform;rootRect.anchorMin=Vector2.zero;rootRect.anchorMax=Vector2.one;rootRect.sizeDelta=Vector2.zero;
            root.transform.SetSiblingIndex(2);
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);var art=Resources.Load<Sprite>("PackHUD/HUDToggleBanner");
            var upper=Button(root.transform,"Upper HUD handle",new Vector2(.5f,1),new Vector2(0,-28),new Vector2(64,56),"×",font,art);
            var lower=Button(root.transform,"Lower card handle",new Vector2(.5f,0),new Vector2(0,524),new Vector2(64,56),"×",font,art);
            var pause=Button(root.transform,"Always available pause",new Vector2(.5f,1),new Vector2(68,-28),new Vector2(52,56),"II",font,art);
            var top=(RectTransform)safe.Find("TopHUD");var bottom=(RectTransform)safe.Find("CardHand");
            top.anchoredPosition=bottom.anchoredPosition=Vector2.zero;
            ArrangeTop(hud,top,font);
            if(!top.TryGetComponent<CanvasGroup>(out var topGroup))topGroup=Undo.AddComponent<CanvasGroup>(top.gameObject);
            if(!bottom.TryGetComponent<CanvasGroup>(out var bottomGroup))bottomGroup=Undo.AddComponent<CanvasGroup>(bottom.gameObject);
            var control=hud.GetComponent<GameplayHUDVisibility>()??Undo.AddComponent<GameplayHUDVisibility>(hud.gameObject);
            var data=new SerializedObject(control);
            Bind(data,"_top",top);Bind(data,"_bottom",bottom);Bind(data,"_lowerHandle",(RectTransform)lower.transform);
            Bind(data,"_topGroup",topGroup);Bind(data,"_bottomGroup",bottomGroup);Bind(data,"_upperButton",upper);Bind(data,"_lowerButton",lower);Bind(data,"_pause",pause);
            Bind(data,"_upperLabel",upper.GetComponentInChildren<TMP_Text>());Bind(data,"_lowerLabel",lower.GetComponentInChildren<TMP_Text>());Bind(data,"_controls",root);
            data.ApplyModifiedProperties();EditorUtility.SetDirty(control);
            var oldTitle=bottom.Find("WolvesVsMonster_Title");if(oldTitle!=null)oldTitle.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);
            return "Compact task art, Simpletix font and reversible HUD handles installed.";
        }
        private static void EnsureFont()
        {
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if(font==null)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/New/FONT/simpletix/simpletix.otf");
                font=TMP_FontAsset.CreateFontAsset(source,48,5,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
                font.name="Simpletix SDF";AssetDatabase.CreateAsset(font,FontPath);
                AssetDatabase.AddObjectToAsset(font.material,font);
                foreach(var texture in font.atlasTextures){texture.name="Simpletix Atlas";AssetDatabase.AddObjectToAsset(texture,font);}
            }
            if(font.fallbackFontAssetTable==null)font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>();
            var fallback=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if(!font.fallbackFontAssetTable.Contains(fallback))font.fallbackFontAssetTable.Add(fallback);
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;!?+-/()'^v%×•");
            EditorUtility.SetDirty(font);AssetDatabase.SaveAssets();
        }
        private static void Import(string path,float border)
        {
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)return;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=true;importer.maxTextureSize=2048;
            bool banner=path.EndsWith("HUDToggleBanner.png");
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=banner?400:300;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();
            int left=texture.width,right=0,bottom=texture.height,top=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                if(pixels[y*texture.width+x].a>16){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
            var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var rect=new SpriteRect{name=System.IO.Path.GetFileNameWithoutExtension(path),spriteID=GUID.Generate(),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),
                rect=new Rect(left,bottom,right-left+1,top-bottom+1),border=banner?new Vector4(100,40,100,40):Vector4.one*border};
            provider.SetSpriteRects(new[]{rect});provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(rect.name,rect.spriteID)});
            provider.Apply();importer.isReadable=false;importer.SaveAndReimport();
        }
        private static void Bind(SerializedObject data,string name,Object value)=>data.FindProperty(name).objectReferenceValue=value;
        private static void ArrangeTop(PackGameplayHUD hud,RectTransform top,TMP_FontAsset font)
        {
            // Normalize the status cluster once so repeated hide/show never accumulates offsets.
            Place(top.Find("WavePlate") as RectTransform,top,128,72,230,76);
            Place(hud.waveLabel.rectTransform,top,128,72,209,55);Style(hud.waveLabel,font,36);
            Place((RectTransform)hud.pauseButton.transform,top,885,56,82,82);
            Place(top.Find("PackPlate") as RectTransform,top,470,66,276,68);
            var health=top.Find("Pack_TMP")?.GetComponent<TMP_Text>();
            if(health!=null){health.enabled=true;health.text="Queen health";Place(health.rectTransform,top,470,45,230,24);Style(health,font,19);}
            Place(top.Find("CampPlate") as RectTransform,top,793,140,278,90);
            Place(top.Find("TentIcon") as RectTransform,top,689,126,45,45);
            Place(hud.campLabel.rectTransform,top,813,127,206,28);Style(hud.campLabel,font,22);
            Place(top.Find("DistanceTrack") as RectTransform,top,803,158,218,19);
            Place(hud.campFill.rectTransform,top,803,158,200,10);
            Place(top.Find("BerryPlate") as RectTransform,top,133,156,242,65);
            Place(hud.berryLabel.rectTransform,top,148,141,190,28);Style(hud.berryLabel,font,23);
        }
        private static void Place(RectTransform rect,Transform parent,float x,float y,float width,float height)
        {
            if(rect==null)return;rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(width,height);rect.localScale=Vector3.one;
        }
        private static void Style(TMP_Text text,TMP_FontAsset font,int size)
        {text.font=font;text.fontStyle=FontStyles.Bold;text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=size-3;text.fontSizeMax=size;}
        private static Button Button(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size,string label,TMP_FontAsset font,Sprite art)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=size;rect.anchoredPosition=position;
            var image=go.GetComponent<Image>();image.sprite=art;image.type=Image.Type.Sliced;image.color=Color.white;
            var text=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));text.transform.SetParent(go.transform,false);
            var r=(RectTransform)text.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(10,3);r.offsetMax=new Vector2(-10,-3);
            var tmp=text.GetComponent<TextMeshProUGUI>();tmp.font=font;tmp.fontSize=label=="II"?20:42;tmp.fontStyle=FontStyles.Bold;tmp.text=label;tmp.color=label=="×"?new Color(.79f,.24f,.2f):new Color(1,.95f,.8f);tmp.alignment=TextAlignmentOptions.Center;tmp.raycastTarget=false;
            return go.GetComponent<Button>();
        }
    }
}
