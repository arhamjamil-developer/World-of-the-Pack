using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldOfThePack.UI;
using WorldOfThePack.NewUI;

namespace WorldOfThePack.Expedition.Editor
{
    public static class RectangularCardHolderBuilder
    {
        public const string HolderPath = "Assets/UI/WorldOfThePack/Prefabs/CardHandHolder.prefab";
        const string ArtPath = "Assets/New/ExpeditionPolish/Art/RectangularCardHolder.png";
        [MenuItem("World of the Pack/Polish/Rectangular Card Holder")]
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Build in Edit Mode.");
            AssetDatabase.ImportAsset(ArtPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(ArtPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 400; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().FirstOrDefault();
            // Trim the generated transparent margins without changing the original bitmap.
            var rect = new SpriteRect { name="RectangularWood", rect=new Rect(width*.01f,height*.13f,width*.98f,height*.76f),
                pivot=Vector2.one*.5f, alignment=SpriteAlignment.Center, border=new Vector4(112,112,112,112), spriteID=old != null ? old.spriteID : GUID.Generate() };
            provider.SetSpriteRects(new[]{rect});
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(rect.name,rect.spriteID)});
            provider.Apply(); importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().First();
            const string fillPath="Assets/New/ExpeditionPolish/Art/RectangularChargeFill.png";
            if(!System.IO.File.Exists(fillPath))
            {
                var solid=new Texture2D(2,2,TextureFormat.RGBA32,false); solid.SetPixels(new[]{Color.white,Color.white,Color.white,Color.white}); solid.Apply();
                System.IO.File.WriteAllBytes(fillPath,solid.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(solid); AssetDatabase.ImportAsset(fillPath);
            }
            var fillImporter=(TextureImporter)AssetImporter.GetAtPath(fillPath); fillImporter.textureType=TextureImporterType.Sprite; fillImporter.spriteImportMode=SpriteImportMode.Single; fillImporter.mipmapEnabled=false; fillImporter.SaveAndReimport();
            var fillSprite=AssetDatabase.LoadAssetAtPath<Sprite>(fillPath);
            SharedHeroCardBuilder.Build();
            var root = PrefabUtility.LoadPrefabContents(SharedHeroCardBuilder.BasePath);
            try
            {
                var card = root.GetComponent<HeroDeploymentCard>();
                var background = root.GetComponent<Image>(); if(background != null) Style(background,sprite);
                var face = root.transform.Find("CardFace");
                var frame = face.Find("GoldenFrame")?.GetComponent<Image>();
                if(frame != null) { Style(frame,sprite); frame.fillCenter=false; }
                foreach(var image in face.GetComponentsInChildren<Image>(true))
                    if(image.name == "NameRibbon") Style(image,sprite);
                card.readyArtwork = card.chargingArtwork = sprite;
                card.readyFillSprite = card.chargingFillSprite = fillSprite;
                if(card.energyFill != null) { card.energyFill.sprite=fillSprite; card.energyFill.color=new Color(.35f,.85f,.6f); }
                Style(card.deployButton.image,sprite);
                var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                foreach(var text in root.GetComponentsInChildren<TMP_Text>(true)) { text.font=font; text.color=Color.white; text.margin=Vector4.zero; }
                Stretch((RectTransform)face.Find("CharacterPortrait"),new Vector2(.05f,.45f),new Vector2(.95f,.95f));
                Stretch((RectTransform)face.Find("PortraitBackdrop"),new Vector2(.05f,.45f),new Vector2(.95f,.95f));
                Stretch((RectTransform)face.Find("NameRibbon"),new Vector2(.05f,.335f),new Vector2(.95f,.46f));
                Stretch(card.nameLabel.rectTransform,new Vector2(.10f,.35f),new Vector2(.90f,.45f));
                card.nameLabel.fontSize=24; card.energyLabel.fontSize=19;
                Stretch(card.energyLabel.rectTransform,new Vector2(.08f,.255f),new Vector2(.92f,.33f));
                var track=face.Find("ChargeTrack")?.GetComponent<Image>();
                if(track!=null) { track.sprite=null; track.color=new Color(.08f,.13f,.14f); Stretch(track.rectTransform,new Vector2(.08f,.22f),new Vector2(.92f,.245f)); }
                Stretch(card.energyFill.rectTransform,new Vector2(.08f,.22f),new Vector2(.92f,.245f));
                var copy = root.GetComponent<PackCardCopy>(); copy.actionLabel.fontSize=24;
                PrefabUtility.SaveAsPrefabAsset(root,SharedHeroCardBuilder.BasePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            SharedHeroCardBuilder.Build();
            var hud = UnityEngine.Object.FindFirstObjectByType<PackGameplayHUD>();
            var hand = (RectTransform)hud.transform.Find("SafeArea/CardHand");
            if(PrefabUtility.IsAnyPrefabInstanceRoot(hand.gameObject))
                PrefabUtility.UnpackPrefabInstance(hand.gameObject,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
            hand.anchorMin=hand.anchorMax=new Vector2(.5f,0); hand.pivot=new Vector2(.5f,0);
            hand.anchoredPosition=Vector2.zero; hand.sizeDelta=new Vector2(940,500);
            var tray=(RectTransform)hand.Find("WoodTray"); Stretch(tray,Vector2.zero,Vector2.one); Style(tray.GetComponent<Image>(),sprite); tray.SetAsFirstSibling();
            var title=hand.Find("WolvesVsMonster_Title")?.GetComponent<TMP_Text>();
            if(title==null) { var go=new GameObject("WolvesVsMonster_Title",typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(hand,false); title=go.GetComponent<TMP_Text>(); }
            title.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"); title.text="Wolves vs Monster"; title.fontSize=30; title.fontStyle=FontStyles.Bold;
            title.alignment=TextAlignmentOptions.Center; title.color=new Color(1,.9f,.62f); title.raycastTarget=false;
            Top(title.rectTransform,470,32,750,38);
            Top(hud.formationButton.GetComponent<RectTransform>(),158,80,260,54); Style(hud.formationButton.image,sprite);
            var formationText=hud.formationButton.GetComponentInChildren<TMP_Text>(); Stretch(formationText.rectTransform,new Vector2(.03f,.05f),new Vector2(.97f,.95f)); formationText.fontSize=22; formationText.font=title.font; formationText.color=Color.white; formationText.alignment=TextAlignmentOptions.Center;
            Top(hud.moonButton.GetComponent<RectTransform>(),776,80,280,54); Style(hud.moonButton.image,sprite);
            hud.moonLabel.transform.SetParent(hud.moonButton.transform,false); Stretch(hud.moonLabel.rectTransform,new Vector2(.18f,.05f),new Vector2(.98f,.95f)); hud.moonLabel.fontSize=22; hud.moonLabel.font=title.font;
            hud.moonLabel.alignment=TextAlignmentOptions.Center;
            var icon=hand.Find("MoonIcon"); if(icon!=null) { icon.SetParent(hud.moonButton.transform,false); var ir=(RectTransform)icon; ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(.5f,.5f); ir.anchoredPosition=new Vector2(-111,0); ir.sizeDelta=Vector2.one*32; }
            var viewport=(RectTransform)hand.Find("CardViewport");
            viewport.anchorMin=new Vector2(0,1); viewport.anchorMax=new Vector2(1,1); viewport.pivot=new Vector2(.5f,1); viewport.anchoredPosition=new Vector2(0,-118); viewport.sizeDelta=new Vector2(-48,334);
            var pager=viewport.GetComponent<PackCardPager>();
            for(int i=0;i<pager.dots.Length;i++) Top(pager.dots[i].GetComponent<RectTransform>(),400+i*35,80,24,24);
            var hint=hand.Find("Hint_TMP")?.GetComponent<TMP_Text>();
            if(hint!=null) { hint.text="Defeat enemies  •  Earn charms  •  Deploy"; hint.font=title.font; hint.fontSize=18; Top(hint.rectTransform,470,475,850,26); }
            var responsive=viewport.GetComponent<ResponsiveCardStrip>(); responsive.safeArea=null;
            PrefabUtility.SaveAsPrefabAssetAndConnect(hand.gameObject,HolderPath,InteractionMode.AutomatedAction);
            responsive.safeArea=hand.parent as RectTransform; PrefabUtility.RecordPrefabInstancePropertyModifications(responsive);
            EditorUtility.SetDirty(hud); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            return "Rectangular sliced border, editable Wolves vs Monster title, linked holder and seven card variants.";
        }
        static void Style(Image image,Sprite sprite) { if(image==null)return; image.sprite=sprite; image.type=Image.Type.Sliced; image.color=Color.white; image.pixelsPerUnitMultiplier=1; }
        static void Top(RectTransform rt,float x,float y,float w,float h) { rt.anchorMin=rt.anchorMax=new Vector2(0,1); rt.pivot=Vector2.one*.5f; rt.anchoredPosition=new Vector2(x,-y); rt.sizeDelta=new Vector2(w,h); }
        static void Stretch(RectTransform rt,Vector2 min,Vector2 max) { rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=rt.offsetMax=Vector2.zero; }
    }
}
