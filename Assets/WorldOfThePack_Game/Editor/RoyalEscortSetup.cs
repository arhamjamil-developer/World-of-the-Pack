using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace WorldOfThePack.Game.Editor
{
    public static class RoyalEscortSetup
    {
        [MenuItem("World of the Pack/Royal Escort/Import Art")]
        public static string ImportArt()
        {
            const string path="Assets/Visuals/RoyalEscort/Resources/RoyalEscort/QueenEscort.png";
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=2048; importer.spritePixelsPerUnit=230;
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var rects=new SpriteRect[8];
            float width=texture.width/4f, height=texture.height/2f;
            for(int i=0;i<8;i++) rects[i]=new SpriteRect {
                name=$"QueenEscort_{i:D2}", spriteID=GUID.Generate(), alignment=SpriteAlignment.Center,
                pivot=new Vector2(.5f,.5f), rect=new Rect((i%4)*width,(1-i/4)*height,width,height)
            };
            provider.SetSpriteRects(rects);
            var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            const string panel="Assets/Visuals/RoyalEscort/Resources/RoyalEscort/TutorialPanel.png";
            if(AssetImporter.GetAtPath(panel) is TextureImporter panelImporter)
            {
                panelImporter.textureType=TextureImporterType.Sprite; panelImporter.spriteImportMode=SpriteImportMode.Multiple;
                panelImporter.alphaIsTransparency=true; panelImporter.mipmapEnabled=false; panelImporter.SaveAndReimport();
                var panelProvider=factory.GetSpriteEditorDataProviderFromObject(panelImporter); panelProvider.InitSpriteEditorDataProvider();
                var panelRect=new SpriteRect { name="TutorialPanel",spriteID=GUID.Generate(),alignment=SpriteAlignment.Center,
                    pivot=new Vector2(.5f,.5f),rect=new Rect(32,136,1472,768),border=new Vector4(70,70,70,70) };
                panelProvider.SetSpriteRects(new[]{panelRect});
                panelProvider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(panelRect.name,panelRect.spriteID)});
                panelProvider.Apply(); panelImporter.SaveAndReimport();
            }
            return $"Royal escort imported: {AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count()} frames";
        }
        [MenuItem("World of the Pack/Royal Escort/Reset First Time Tutorial")]
        public static void ResetTutorial() => TutorialManager.ResetTutorial();
    }
}
