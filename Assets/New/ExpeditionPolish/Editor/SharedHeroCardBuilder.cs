using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using WorldOfThePack.UI;
using WorldOfThePack.NewUI;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition.Editor
{
    public static class SharedHeroCardBuilder
    {
        public const string BasePath = "Assets/UI/WorldOfThePack/Prefabs/HeroDeploymentCardBase.prefab";
        private const string VariantRoot = "Assets/New/Prefabs/";
        [MenuItem("World of the Pack/Polish/Link Shared Card Prefabs")]
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Link card prefabs in Edit Mode.");
            var hud = UnityEngine.Object.FindFirstObjectByType<PackGameplayHUD>();
            if (hud == null || hud.cards == null || hud.cards.Length != 7) throw new InvalidOperationException("Expected the current seven-card hand.");
            var original = hud.cards.ToArray();
            var content = original[0].transform.parent as RectTransform;
            var viewport = content.parent as RectTransform;
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BasePath);
            if (baseAsset == null)
            {
                var template = UnityEngine.Object.Instantiate(original[0].gameObject);
                template.name = "HeroDeploymentCardBase"; template.transform.SetParent(null, false);
                if (PrefabUtility.IsPartOfPrefabInstance(template)) PrefabUtility.UnpackPrefabInstance(template, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                var rt = template.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0, .5f); rt.pivot = new Vector2(.5f, .5f);
                rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(292, 332); rt.localScale = Vector3.one;
                var card = template.GetComponent<HeroDeploymentCard>();
                card.definition = null; card.portraitView.definition = null; card.portraitView.wolfForm = false;
                card.portraitView.portrait.sprite = null; card.portraitView.portrait.enabled = true;
                card.nameLabel.text = "HERO"; card.energyLabel.text = "CHARGE";
                if (card.charmCostLabel != null) card.charmCostLabel.text = "0";
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                if (font == null) font = TMP_Settings.defaultFontAsset;
                foreach (var text in template.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font = font; text.fontStyle = FontStyles.Bold; text.enableAutoSizing = false;
                    text.overflowMode = TextOverflowModes.Ellipsis; text.alignment = TextAlignmentOptions.Center;
                }
                card.nameLabel.fontSize = 28; card.energyLabel.fontSize = 19;
                if (card.charmCostLabel != null) card.charmCostLabel.fontSize = 25;
                var copy = template.GetComponent<PackCardCopy>(); copy.actionLabel.fontSize = 24;
                // The action now uses the space freed by removal of the duplicate Moonshift button.
                SetActionRect(card.deployButton.transform as RectTransform, 252, 57);
                SetActionRect(copy.actionLabel.transform as RectTransform, 238, 46);
                NormalizeChildren(rt);
                baseAsset = PrefabUtility.SaveAsPrefabAsset(template, BasePath);
                UnityEngine.Object.DestroyImmediate(template);
            }
            var baseContents = PrefabUtility.LoadPrefabContents(BasePath);
            try { EnsureFace(baseContents); PrefabUtility.SaveAsPrefabAsset(baseContents, BasePath); }
            finally { PrefabUtility.UnloadPrefabContents(baseContents); }
            baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BasePath);
            var cards = new HeroDeploymentCard[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                var definition = original[i].Definition;
                string path = VariantRoot + "Card_" + definition.heroId + ".prefab";
                var variant = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
                variant.name = "Card_" + definition.heroId;
                var card = variant.GetComponent<HeroDeploymentCard>(); card.definition = definition;
                card.portraitView.definition = definition; card.portraitView.wolfForm = false;
                card.portraitView.portrait.sprite = definition.humanPortrait;
                card.nameLabel.text = definition.shortName;
                var role = variant.transform.Find("CardFace/CharacterRoleIcon")?.GetComponent<Image>();
                string badge = definition.heroId.Contains("Elva") || definition.heroId.Contains("Pip") ? "Leaf" : definition.heroId.Contains("Lulu") || definition.heroId.Contains("Momo") ? "Moon" : "Paw";
                if (role != null) role.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/New/Art/Badges/" + badge + ".png").OfType<Sprite>().FirstOrDefault();
                foreach (var component in variant.GetComponentsInChildren<Component>(true))
                    if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                var asset = PrefabUtility.SaveAsPrefabAsset(variant, path);
                UnityEngine.Object.DestroyImmediate(variant);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, content);
                instance.name = "Card_" + definition.heroId; instance.transform.SetSiblingIndex(i);
                cards[i] = instance.GetComponent<HeroDeploymentCard>();
            }
            hud.cards = cards;
            var deck = UnityEngine.Object.FindFirstObjectByType<CardDeckManager>();
            var deckSo = new SerializedObject(deck); var slots = deckSo.FindProperty("cardSlots"); slots.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++) slots.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            deckSo.ApplyModifiedPropertiesWithoutUndo();
            foreach (var card in original) UnityEngine.Object.DestroyImmediate(card.gameObject);
            var layout = content.GetComponent<HorizontalLayoutGroup>(); if (layout == null) layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(); layout.spacing = 14; layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fitter = content.GetComponent<ContentSizeFitter>(); if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.anchorMin = content.anchorMax = new Vector2(0, .5f); content.pivot = new Vector2(0, .5f); content.anchoredPosition = Vector2.zero;
            var responsive = viewport.GetComponent<ResponsiveCardStrip>(); if (responsive == null) responsive = viewport.gameObject.AddComponent<ResponsiveCardStrip>();
            responsive.content = content; responsive.handRoot = hud.transform.Find("SafeArea/CardHand") as RectTransform; responsive.safeArea = hud.transform.Find("SafeArea") as RectTransform;
            var pager = viewport.GetComponent<PackCardPager>(); if (pager != null) { pager.content = content; pager.stops = cards.Length - 2; }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            EditorUtility.SetDirty(hud); EditorUtility.SetDirty(deck); EditorUtility.SetDirty(responsive);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            return "One shared card base, seven variants, seven linked scene instances, responsive three-card strip.";
        }
        private static void SetActionRect(RectTransform rect, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = new Vector2(146, -291); rect.sizeDelta = new Vector2(width, height);
        }
        private static void EnsureFace(GameObject root)
        {
            var face = root.transform.Find("CardFace") as RectTransform;
            if (face == null)
            {
                var parent = root.GetComponent<RectTransform>();
                var children = root.transform.Cast<Transform>().ToArray();
                face = new GameObject("CardFace", typeof(RectTransform)).GetComponent<RectTransform>();
                face.SetParent(parent, false); face.anchorMin = face.anchorMax = face.pivot = Vector2.one * .5f;
                face.anchoredPosition = Vector2.zero; face.sizeDelta = parent.sizeDelta;
                foreach (var child in children) child.SetParent(face, false);
            }
            var element = root.GetComponent<LayoutElement>(); if (element == null) element = root.AddComponent<LayoutElement>();
            var sizer = root.GetComponent<CardFaceLayoutSizer>(); if (sizer == null) sizer = root.AddComponent<CardFaceLayoutSizer>();
            sizer.face = face; sizer.layout = element; sizer.Fit();
        }
        private static void NormalizeChildren(RectTransform parent)
        {
            foreach (RectTransform child in parent)
            {
                NormalizeChildren(child);
                Vector3[] corners = new Vector3[4]; child.GetWorldCorners(corners);
                Vector3 min = parent.InverseTransformPoint(corners[0]); Vector3 max = parent.InverseTransformPoint(corners[2]);
                var rect = parent.rect;
                child.anchorMin = new Vector2((min.x - rect.xMin) / Mathf.Max(1, rect.width), (min.y - rect.yMin) / Mathf.Max(1, rect.height));
                child.anchorMax = new Vector2((max.x - rect.xMin) / Mathf.Max(1, rect.width), (max.y - rect.yMin) / Mathf.Max(1, rect.height));
                child.offsetMin = child.offsetMax = Vector2.zero;
            }
        }
    }
}
