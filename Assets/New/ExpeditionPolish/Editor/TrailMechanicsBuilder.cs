using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using WorldOfThePack.Game;
using WorldOfThePack.NewUI;

namespace WorldOfThePack.Expedition.Editor
{
    public static class TrailMechanicsBuilder
    {
        private const string Root = "Assets/New/ExpeditionPolish/";
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Author trail mechanics in Edit Mode.");
            var hud = UnityEngine.Object.FindFirstObjectByType<PackGameplayHUD>();
            var game = UnityEngine.Object.FindFirstObjectByType<GameDirector>();
            if (hud == null || game == null) throw new InvalidOperationException("SampleScene gameplay owners are required.");
            CleanHud(hud);
            var sprites = ImportProps();
            var director = game.GetComponent<TrailDecisionDirector>(); if (director == null) director = game.gameObject.AddComponent<TrailDecisionDirector>();
            var old = GameObject.Find("TrailEncounters"); if (old != null) UnityEngine.Object.DestroyImmediate(old);
            var world = new GameObject("TrailEncounters");
            director.routeRoot = new GameObject("RouteChoice"); director.routeRoot.transform.SetParent(world.transform, false);
            director.rescueRoot = new GameObject("PackRescue"); director.rescueRoot.transform.SetParent(world.transform, false);
            director.springNode = Node("Spring", director.routeRoot.transform, -1.0f, sprites[0], "SPRING\nHeal pack 20%", TrailChoice.Spring, director);
            director.huntNode = Node("Hunt", director.routeRoot.transform, 1.0f, sprites[1], "HUNT\n+2 foes • +50% charms", TrailChoice.Hunt, director);
            director.rescueNode = Node("Rescue", director.rescueRoot.transform, -1.0f, sprites[3], "FREE PIP\n4 CHARMS", TrailChoice.Rescue, director);
            director.continueNode = Node("Continue", director.rescueRoot.transform, 1.0f, sprites[2], "KEEP WALKING\nSave your charms", TrailChoice.Continue, director);
            director.cageFrames = new[] { sprites[3], sprites[4], sprites[5] };
            var captive = new GameObject("Captive").AddComponent<SpriteRenderer>(); captive.transform.SetParent(director.rescueNode.transform, false);
            captive.transform.localPosition = new Vector3(0, -.2f, 0); captive.sortingOrder = 309; director.rescueNode.captive = captive;
            director.rescuedHeroSprites = new Sprite[3];
            string[] ids = { "ScoutPip", "WizardMomo", "TinkerTilly" };
            for (int i = 0; i < ids.Length; i++)
            {
                var frames = AssetDatabase.LoadAllAssetsAtPath($"Assets/Characters/{ids[i]}/Human/{ids[i]}_Human_North.png").OfType<Sprite>().ToArray();
                director.rescuedHeroSprites[i] = frames.FirstOrDefault(s => s.name.Contains("Idle")) ?? frames.FirstOrDefault();
            }
            director.rescueNode.SetCaptive(director.rescuedHeroSprites[0]);
            director.focusMarker = new GameObject("FocusFireMarker"); director.focusMarker.transform.SetParent(world.transform, false);
            var line = director.focusMarker.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true; line.positionCount = 48;
            string materialPath = Root + "Data/FocusMarker.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(material, materialPath); }
            line.sharedMaterial = material; line.startWidth = line.endWidth = .035f; line.sortingOrder = 400;
            line.startColor = line.endColor = new Color(.4f, 1, .92f, .95f);
            for (int i = 0; i < 48; i++) { float angle = i * Mathf.PI * 2 / 48; line.SetPosition(i, new Vector3(Mathf.Cos(angle) * .42f, Mathf.Sin(angle) * .28f, 0)); }
            director.routeRoot.SetActive(false); director.rescueRoot.SetActive(false); director.focusMarker.SetActive(false);
            foreach (string path in new[] { "Assets/New/Enemy/Prefabs/Enemy_Soldier.prefab", "Assets/New/Enemy/Prefabs/Enemy_Rudra.prefab", "Assets/New/Enemy/Prefabs/Enemy_Elsa.prefab", "Assets/WorldOfThePack_Game/Prefabs/Units/Boss_AncientThornGuardian.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (root.GetComponent<PackEnemyTarget>() == null) root.AddComponent<PackEnemyTarget>();
                    var collider = root.GetComponent<CircleCollider2D>(); if (collider == null) collider = root.AddComponent<CircleCollider2D>();
                    collider.isTrigger = true; collider.radius = root.GetComponent<BossUnit>() != null ? .8f : 1;
                    collider.offset = new Vector2(0, .45f);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var camera = Camera.main; if (camera != null && camera.GetComponent<Physics2DRaycaster>() == null) camera.gameObject.AddComponent<Physics2DRaycaster>();
            EditorUtility.SetDirty(director); EditorUtility.SetDirty(hud);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            return "Trail mechanics authored: focus fire, route choices, pack rescues; redundant HUD removed.";
        }
        public static void CleanHud(PackGameplayHUD hud)
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects()) if (root.name == "PortraitCardCanvas" && !root.activeSelf) UnityEngine.Object.DestroyImmediate(root);
            foreach (var card in hud.cards)
            {
                var button = card.transform.Find("Transform"); if (button != null) UnityEngine.Object.DestroyImmediate(button.gameObject);
            }
            if (hud.campOptions != null && hud.campOptions.Length > 3)
            {
                for (int i = 3; i < hud.campOptions.Length; i++)
                {
                    var button = hud.campOptions[i];
                    if (button == null) continue;
                    var text = button.transform.parent.Find(button.name + "_TMP");
                    if (text != null) UnityEngine.Object.DestroyImmediate(text.gameObject);
                    UnityEngine.Object.DestroyImmediate(button.gameObject);
                }
                hud.campOptions = hud.campOptions.Take(3).ToArray();
            }
            var atmosphere = hud.GetComponent<MoonshiftAtmosphere>();
            if (atmosphere != null && atmosphere.moonGlow != null)
            {
                UnityEngine.Object.DestroyImmediate(atmosphere.moonGlow.gameObject); atmosphere.moonGlow = null; EditorUtility.SetDirty(atmosphere);
            }
            var top = hud.transform.Find("SafeArea/TopHUD");
            bool hadLogo = top.Find("TitleLogo") != null;
            hud.berryFill = null;
            foreach (string name in new[] { "TitleLogo", "BerryTrack", "BerryFill", "CampMilestone_1", "CampMilestone_2", "CampMilestone_3" })
            { var item = top.Find(name); if (item != null) UnityEngine.Object.DestroyImmediate(item.gameObject); }
            if (hadLogo)
            {
                foreach (string name in new[] { "PackPlate", "Pack_TMP", "Heart_1", "Heart_2", "Heart_3", "Heart_4", "Heart_5" })
                { var rt = top.Find(name) as RectTransform; if (rt != null) rt.anchoredPosition += Vector2.up * 58; }
            }
            // Reusable card prefabs should not reintroduce seven duplicate Moonshift controls.
            foreach (string id in new[] { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" })
            {
                string path = "Assets/New/Prefabs/Card_" + id + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try { var repeat = root.transform.Find("Transform"); if (repeat != null) { UnityEngine.Object.DestroyImmediate(repeat.gameObject); PrefabUtility.SaveAsPrefabAsset(root, path); } }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        private static TrailChoiceNode Node(string name, Transform parent, float x, Sprite sprite, string label, TrailChoice choice, TrailDecisionDirector director)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = new Vector3(x, 0, 0);
            var node = go.AddComponent<TrailChoiceNode>(); node.director = director; node.choice = choice;
            var body = new GameObject("Art"); body.transform.SetParent(go.transform, false); node.art = body.AddComponent<SpriteRenderer>(); node.art.sortingOrder = 310; node.SetSprite(sprite);
            var collider = go.AddComponent<CircleCollider2D>(); collider.radius = .72f; collider.isTrigger = true;
            var text = new GameObject("ChoiceLabel").AddComponent<TextMeshPro>(); text.transform.SetParent(go.transform, false);
            text.transform.localPosition = new Vector3(0, -.92f, 0); text.rectTransform.sizeDelta = new Vector2(1.9f, .72f);
            text.fontSize = 2.1f; text.alignment = TextAlignmentOptions.Center; text.color = new Color(1, .96f, .8f);
            text.sortingOrder = 450; node.label = text; node.SetLabel(label); return node;
        }
        private static Sprite[] ImportProps()
        {
            string path = Root + "Art/TrailChoices.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            var factory = new SpriteDataProviderFactories(); factory.Init(); var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects(); var rects = new SpriteRect[6];
            for (int i = 0; i < 6; i++)
            {
                string name = "TrailChoice_" + i; var old = previous.FirstOrDefault(r => r.name == name);
                rects[i] = new SpriteRect { name = name, rect = new Rect((i % 3) * w / 3f, (1 - i / 3) * h / 2f, w / 3f, h / 2f),
                    pivot = Vector2.one * .5f, alignment = SpriteAlignment.Center, spriteID = old != null ? old.spriteID : GUID.Generate() };
            }
            provider.SetSpriteRects(rects); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport(); return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        }
    }
}
