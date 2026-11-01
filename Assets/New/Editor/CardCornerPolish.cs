using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.UI;

namespace WorldOfThePack.NewUI.Editor
{
    public static class CardCornerPolish
    {
        private const string Art = "Assets/New/Art/Badges/";

        [MenuItem("World of the Pack/New UI/Polish Card Corner Badges")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Apply outside Play Mode.");
            AssetDatabase.Refresh();
            foreach (string name in new[] { "Paw", "Leaf", "Moon", "Refresh" }) ImportBadge(name);
            int count = 0;
            foreach (var card in UnityEngine.Object.FindObjectsByType<HeroDeploymentCard>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (card.transform.root.name != "NewPackUICanvas") continue;
                UpdateCard(card); count++;
            }
            UpdateToolbar(GameObject.Find("NewPackUICanvas"));
            foreach (string path in Directory.GetFiles("Assets/New/Prefabs", "*.prefab"))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var card in root.GetComponentsInChildren<HeroDeploymentCard>(true)) UpdateCard(card);
                    UpdateToolbar(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("CARD_CORNER_POLISH_OK: " + count + " scene cards; scene and reusable prefabs updated.");
        }

        private static void UpdateCard(HeroDeploymentCard card)
        {
            string id = card.definition != null ? card.definition.heroId : card.name;
            string role = id.Contains("Elva") || id.Contains("Pip") ? "Leaf" : id.Contains("Lulu") || id.Contains("Momo") ? "Moon" : "Paw";
            var icon = card.transform.Find("CharacterRoleIcon");
            if (icon != null && icon.TryGetComponent<Image>(out var image))
            {
                image.sprite = Badge(role); image.color = Color.white;
                image.preserveAspect = true; image.raycastTarget = false;
                SetRect(image.rectTransform, 42, 42, 54);
                EditorUtility.SetDirty(image);
            }
            var refresh = card.transform.Find("Transform");
            if (refresh != null && refresh.TryGetComponent<Image>(out var button))
            {
                button.sprite = Badge("Refresh"); button.preserveAspect = true;
                SetRect(button.rectTransform, 253, 291, 54);
                EditorUtility.SetDirty(button);
            }
        }
        private static void UpdateToolbar(GameObject root)
        {
            if (root == null) return;
            foreach (var image in root.GetComponentsInChildren<Image>(true))
                if (image.name == "MoonIcon") { image.sprite = Badge("Moon"); image.preserveAspect = true; }
        }
        private static void SetRect(RectTransform rect, float x, float y, float size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = Vector2.one * size;
        }
        private static Sprite Badge(string name) => AssetDatabase.LoadAllAssetsAtPath(Art + name + ".png").OfType<Sprite>().First();
        private static void ImportBadge(string name)
        {
            string path = Art + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.maxTextureSize = 512; importer.textureCompression = TextureImporterCompression.Uncompressed;
            var raw = new Texture2D(2, 2); raw.LoadImage(File.ReadAllBytes(path));
            var pixels = raw.GetPixels32(); int left = raw.width, bottom = raw.height, right = 0, top = 0;
            for (int y = 0; y < raw.height; y++) for (int x = 0; x < raw.width; x++)
                if (pixels[y * raw.width + x].a > 16)
                { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects(); var id = previous.Length > 0 ? previous[0].spriteID : GUID.Generate();
            provider.SetSpriteRects(new[] { new SpriteRect { name = name, spriteID = id,
                rect = new Rect(left, bottom, right - left + 1, top - bottom + 1), alignment = SpriteAlignment.Center, pivot = new Vector2(.5f, .5f) } });
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, id) });
            provider.Apply(); importer.SaveAndReimport(); UnityEngine.Object.DestroyImmediate(raw);
        }
    }
}
