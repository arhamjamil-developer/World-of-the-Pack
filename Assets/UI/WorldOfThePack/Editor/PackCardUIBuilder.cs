using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using WorldOfThePack.UI;

namespace WorldOfThePack.UI.Editor
{
    [InitializeOnLoad]
    public static class PackCardUIBuilder
    {
        public const string Root = "Assets/UI/WorldOfThePack";
        private const string Pending = "PackCardUI.Verify";
        private static readonly string[] Ids = { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" };
        private static readonly string[] Names = { "Hero Wolvy", "Princess Elva", "Fairy Lulu", "Captain Bobo", "Scout Pip", "Wizard Momo", "Tinker Tilly" };
        private static readonly string[] Short = { "WOLVY", "ELVA", "LULU", "BOBO", "PIP", "MOMO", "TILLY" };
        private static TMP_FontAsset font;
        private static Material outlined;
        private static int phase;
        private static double next;
        private static readonly List<string> Errors = new List<string>();
        private static readonly Color Ink = new Color(.22f, .11f, .055f);
        private static string Project => Directory.GetParent(Application.dataPath).FullName;
        private static string QA => Path.Combine(Project, "Design/CardUI/QA");
        private static string ReportPath => Path.Combine(QA, "unity-ui-audit.json");
        [Serializable] private sealed class Report
        {
            public int standaloneArtAssets, portraits, heroDefinitions, heroVariants, elementPrefabs;
            public bool assetsBuilt, allFourteenPortraits, chargingDisabled, readyClick, pendingNoSpend, rejectPreservesEnergy, confirmConsumesOnce, duplicateClickBlocked, dynamicText, portraitSwitch, raycastHit, deployedVfxPlayed, deployedVfxStopped, demoDeploymentConfirmed, iphoneLayout, ipadLayout, playModeVerified;
            public string fontName;
            public string[] runtimeErrors;
            public string note;
        }
        private static Report report;

        static PackCardUIBuilder()
        {
            EditorApplication.update += VerifyUpdate;
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Pending, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) { phase = 0; next = EditorApplication.timeSinceStartup + 1; }
                if (state == PlayModeStateChange.EnteredEditMode) { SessionState.SetBool(Pending, false); Debug.Log("PACK_CARD_UI_VERIFICATION_FINISHED"); }
            };
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (SessionState.GetBool(Pending, false) && EditorApplication.isPlaying && (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)) Errors.Add(message + "\n" + trace);
            };
        }

        [MenuItem("World of the Pack/Card UI/Build and Verify")]
        public static void BuildAndVerify()
        {
            Build();
            VerifyCurrentShowcase();
        }

        [MenuItem("World of the Pack/Card UI/Verify Current Showcase")]
        public static void VerifyCurrentShowcase()
        {
            if (UnityEngine.Object.FindAnyObjectByType<CardUIShowcase>() == null) throw new InvalidOperationException("Open CardPortraitShowcase before running this verification.");
            Errors.Clear();
            SessionState.SetBool(Pending, true);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("World of the Pack/Card UI/Build Assets")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Build the card UI in Edit Mode.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save your current scene before building the isolated UI showcase.");
            Directory.CreateDirectory(QA);
            report = new Report();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length == 0)
            {
                string package = Directory.GetFiles(Path.Combine(Project, "Library/PackageCache"), "TMP Essential Resources.unitypackage", SearchOption.AllDirectories).First();
                AssetDatabase.ImportPackage(package, false);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            ImportArt();
            BuildFont();
            var definitions = BuildDefinitions();
            BuildElementPrefabs(definitions[0]);
            GameObject master = MakeCard(definitions[0]);
            string masterPath = Root + "/Prefabs/CardPortrait.prefab";
            PrefabUtility.SaveAsPrefabAsset(master, masterPath); UnityEngine.Object.DestroyImmediate(master);
            for (int i = 0; i < definitions.Length; i++)
            {
                var variant = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(masterPath));
                variant.name = Ids[i] + "_CardPortrait";
                variant.GetComponent<HeroDeploymentCard>().Bind(definitions[i], 100, true);
                PrefabUtility.SaveAsPrefabAsset(variant, Root + "/Prefabs/" + variant.name + ".prefab");
                UnityEngine.Object.DestroyImmediate(variant); report.heroVariants++;
            }
            var hand = MakeHand(definitions);
            PrefabUtility.SaveAsPrefabAsset(hand, Root + "/Prefabs/HeroCardHand.prefab"); UnityEngine.Object.DestroyImmediate(hand);
            var canvas = MakeCanvas();
            var safe = NewRect("SafeArea", canvas.transform); Stretch(safe);
            safe.gameObject.AddComponent<PortraitSafeArea>();
            var savedHand = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/HeroCardHand.prefab"));
            savedHand.transform.SetParent(safe, false); AnchorHand((RectTransform)savedHand.transform);
            PrefabUtility.SaveAsPrefabAsset(canvas, Root + "/Prefabs/PortraitCardCanvas.prefab"); UnityEngine.Object.DestroyImmediate(canvas);
            MakeShowcase(definitions);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            report.assetsBuilt = report.portraits == 14 && report.heroVariants == 7 && report.standaloneArtAssets == 42;
            report.note = "Rendered iPhone/iPad aspect and simulated-safe-area checks; no physical-device QA or combat spawning is claimed. Font labels are editable TMP text. Shared assets are assembled by this Editor builder, not drawn at runtime.";
            SaveReport(); AssetDatabase.SaveAssets();
            Debug.Log("PACK_CARD_UI_BUILD_OK");
        }

        private static void ImportArt()
        {
            foreach (string absolute in Directory.GetFiles(Path.Combine(Project, Root, "Art"), "*.png", SearchOption.AllDirectories))
            {
                string path = absolute.Substring(Project.Length + 1).Replace('\\', '/');
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                var textureSettings = new TextureImporterSettings(); importer.ReadTextureSettings(textureSettings); textureSettings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(textureSettings);
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.isReadable = false;
                importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
                string name = Path.GetFileNameWithoutExtension(path);
                importer.spriteBorder = name == "CardFrame" ? new Vector4(100, 128, 100, 128) : path.Contains("/Controls/") && name != "RepeatIcon" ? new Vector4(64, 42, 64, 42) : Vector4.zero;
                importer.SaveAndReimport(); report.standaloneArtAssets++;
                if (path.Contains("/Portraits/")) report.portraits++;
            }
        }

        private static void BuildFont()
        {
            string path = Root + "/Fonts/LilitaOne-Dynamic.asset";
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LilitaOne-Regular.ttf");
                if (source == null) source = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                font = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
                font.name = "Lilita One Dynamic";
                font.TryAddCharacters(string.Concat(Enumerable.Range(32, 95).Select(x => (char)x)) + "•…", out string missing);
                AssetDatabase.CreateAsset(font, path);
                font.material.name = "LilitaOne SDF"; AssetDatabase.AddObjectToAsset(font.material, font);
                foreach (var texture in font.atlasTextures) { texture.name = "LilitaOne Atlas"; AssetDatabase.AddObjectToAsset(texture, font); }
            }
            report.fontName = font.name;
            string materialPath = Root + "/Fonts/LilitaOne-Outlined.mat";
            outlined = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (outlined == null) { outlined = new Material(font.material); AssetDatabase.CreateAsset(outlined, materialPath); }
            outlined.CopyPropertiesFromMaterial(font.material);
            outlined.SetFloat("_OutlineWidth", .15f); outlined.SetColor("_OutlineColor", Ink); outlined.SetFloat("_FaceDilate", .035f);
        }

        private static HeroCardDefinition[] BuildDefinitions()
        {
            var colors = new[] { "#49A4F6", "#63BE53", "#6FD9ED", "#EAA549", "#4DDBC8", "#AA7CEB", "#F9BD42" };
            var result = new List<HeroCardDefinition>();
            for (int i = 0; i < Ids.Length; i++)
            {
                string path = Root + "/Data/" + Ids[i] + ".asset";
                var data = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>(path);
                if (data == null) { data = ScriptableObject.CreateInstance<HeroCardDefinition>(); AssetDatabase.CreateAsset(data, path); }
                data.heroId = Ids[i]; data.displayName = Names[i]; data.shortName = Short[i]; data.maximumEnergy = 100;
                data.humanPortrait = Sprite("Portraits/" + Ids[i] + "_Human"); data.wolfPortrait = Sprite("Portraits/" + Ids[i] + "_Wolf");
                ColorUtility.TryParseHtmlString(colors[i], out data.accent); EditorUtility.SetDirty(data); result.Add(data); report.heroDefinitions++;
            }
            return result.ToArray();
        }

        private static void BuildElementPrefabs(HeroCardDefinition hero)
        {
            foreach (string name in new[] { "Nameplate", "EnergyTrack", "EnergyFill", "DeployButton", "ChargingButton", "FormationTag", "RepeatIcon", "MoonshiftButton" })
            {
                var root = NewRect(name, null); root.sizeDelta = name == "RepeatIcon" ? new Vector2(24, 24) : new Vector2(144, 44);
                var image = root.gameObject.AddComponent<Image>(); Style(image, Sprite("Controls/" + name), name != "RepeatIcon", name != "RepeatIcon" ? 6 : 1);
                if (name.Contains("Button") || name == "FormationTag")
                {
                    var button = root.gameObject.AddComponent<Button>(); SetButtonColors(button);
                    var label = Label("EditableLabel", root, name == "DeployButton" ? "DEPLOY" : name == "ChargingButton" ? "CHARGING" : name == "FormationTag" ? "FORMATION" : "MOONSHIFT", 16, name == "FormationTag" ? Ink : Color.white, name != "FormationTag");
                    Stretch((RectTransform)label.transform, 10);
                }
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, Root + "/Prefabs/Elements/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(root.gameObject); report.elementPrefabs++;
            }
            var frame = NewRect("CardFrame", null); frame.sizeDelta = new Vector2(116, 212); var frameImage = frame.gameObject.AddComponent<Image>(); Style(frameImage, Sprite("Frames/CardFrame"), true, 12); frameImage.fillCenter = false;
            PrefabUtility.SaveAsPrefabAsset(frame.gameObject, Root + "/Prefabs/Elements/CardFrame.prefab"); UnityEngine.Object.DestroyImmediate(frame.gameObject); report.elementPrefabs++;
            var portrait = NewRect("PortraitWindow", null); portrait.sizeDelta = new Vector2(128, 128); var background = portrait.gameObject.AddComponent<Image>(); Style(background, Sprite("Frames/PortraitBackdrop"));
            var face = Image("Portrait", portrait, hero.wolfPortrait); Stretch((RectTransform)face.transform, 3); face.preserveAspect = true;
            var view = portrait.gameObject.AddComponent<CardPortraitView>(); view.portrait = face; view.definition = hero; view.wolfForm = true;
            PrefabUtility.SaveAsPrefabAsset(portrait.gameObject, Root + "/Prefabs/Elements/PortraitWindow.prefab"); UnityEngine.Object.DestroyImmediate(portrait.gameObject); report.elementPrefabs++;
            foreach (string effect in new[] { "Charging", "Deployed", "Ready" })
            {
                var fx = MakeEffect(effect + "VFX", null, effect, new Vector2(80, 80));
                PrefabUtility.SaveAsPrefabAsset(fx.gameObject, Root + "/Prefabs/Elements/" + effect + "VFX.prefab"); UnityEngine.Object.DestroyImmediate(fx.gameObject); report.elementPrefabs++;
            }
            var meter = NewRect("EnergyMeter", null); meter.sizeDelta = new Vector2(116, 20); BuildEnergy(meter, out _, out _, out _);
            PrefabUtility.SaveAsPrefabAsset(meter.gameObject, Root + "/Prefabs/Elements/EnergyMeter.prefab"); UnityEngine.Object.DestroyImmediate(meter.gameObject); report.elementPrefabs++;
        }

        private static GameObject MakeCard(HeroCardDefinition hero)
        {
            var root = NewRect("CardPortrait", null); root.sizeDelta = new Vector2(116, 212);
            var baseImage = Image("CreamBacking", root, Sprite("Controls/Nameplate"), true, 10); Stretch((RectTransform)baseImage.transform, 3);
            var window = NewRect("PortraitWindow", root); window.anchorMin = new Vector2(0, 0); window.anchorMax = new Vector2(1, 1); window.offsetMin = new Vector2(7, 103); window.offsetMax = new Vector2(-7, -7);
            var back = window.gameObject.AddComponent<Image>(); Style(back, Sprite("Frames/PortraitBackdrop")); window.gameObject.AddComponent<RectMask2D>();
            var portrait = Image("Portrait", window, hero.wolfPortrait); Stretch((RectTransform)portrait.transform); portrait.preserveAspect = true;
            var view = window.gameObject.AddComponent<CardPortraitView>(); view.portrait = portrait; view.definition = hero;
            var nameplate = Image("Nameplate", root, Sprite("Controls/Nameplate"), true, 10); BottomStretch((RectTransform)nameplate.transform, 5, 79, 25);
            var nameLabel = Label("HeroName", nameplate.transform, hero.shortName, 16, Ink); Stretch((RectTransform)nameLabel.transform, 4);
            var energy = NewRect("EnergyMeter", root); BottomStretch(energy, 7, 56, 19); BuildEnergy(energy, out Image fill, out TMP_Text energyText, out UIFlipbookEffect charging);
            var action = Button("DeploymentButton", root, "DEPLOY", Sprite("Controls/DeployButton"), Color.white, true, out TMP_Text stateText); BottomStretch((RectTransform)action.transform, 6, 7, 45);
            ((RectTransform)stateText.transform).offsetMax = new Vector2(-19, -3); ((RectTransform)stateText.transform).offsetMin = new Vector2(5, 3);
            var repeat = Image("RepeatIcon", action.transform, Sprite("Controls/RepeatIcon")); Fixed((RectTransform)repeat.transform, new Vector2(1, .5f), new Vector2(-12, 0), new Vector2(15, 15)); repeat.preserveAspect = true;
            var border = Image("SharedOuterFrame", root, Sprite("Frames/CardFrame"), true, 12); Stretch((RectTransform)border.transform); border.fillCenter = false;
            var selected = Image("SelectionFrame", root, Sprite("Frames/CardFrame"), true, 12); Stretch((RectTransform)selected.transform, -1); selected.fillCenter = false; selected.color = new Color(1, .9f, .45f, .65f); selected.enabled = false;
            var ready = MakeEffect("ReadyVFX", root, "Ready", root.sizeDelta); Stretch((RectTransform)ready.transform, 3); ready.image.preserveAspect = false;
            var deployed = MakeEffect("DeployedVFX", root, "Deployed", new Vector2(80, 80)); Fixed((RectTransform)deployed.transform, new Vector2(.5f, .7f), Vector2.zero, new Vector2(80, 80));
            var card = root.gameObject.AddComponent<HeroDeploymentCard>();
            card.portraitView = view; card.nameLabel = nameLabel; card.energyLabel = energyText; card.stateLabel = stateText; card.energyFill = fill; card.actionArtwork = action.GetComponent<Image>(); card.selectionFrame = selected; card.deployButton = action;
            card.readyArtwork = Sprite("Controls/DeployButton"); card.chargingArtwork = Sprite("Controls/ChargingButton"); card.chargingEffect = charging; card.readyEffect = ready; card.deployedEffect = deployed;
            card.Bind(hero, 100, true);
            return root.gameObject;
        }

        private static void BuildEnergy(RectTransform root, out Image fill, out TMP_Text text, out UIFlipbookEffect charging)
        {
            var track = Image("Track", root, Sprite("Controls/EnergyTrack"), true, 12); Stretch((RectTransform)track.transform);
            var clip = NewRect("FillClip", root); Stretch(clip, 3); clip.gameObject.AddComponent<RectMask2D>();
            fill = Image("Fill", clip, Sprite("Controls/EnergyFill")); Stretch((RectTransform)fill.transform); fill.type = UnityEngine.UI.Image.Type.Filled; fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 1;
            charging = MakeEffect("ChargingVFX", root, "Charging", new Vector2(44, 18)); Fixed((RectTransform)charging.transform, new Vector2(.5f, .5f), Vector2.zero, new Vector2(44, 18)); charging.horizontalDrift = 28;
            text = Label("EditableEnergyText", root, "100/100", 11, Color.white, true); Stretch((RectTransform)text.transform, 1);
        }

        private static GameObject MakeHand(HeroCardDefinition[] definitions)
        {
            var root = NewRect("HeroCardHand", null); root.sizeDelta = new Vector2(390, 300);
            var toolbar = NewRect("Toolbar", root);
            var formation = Button("FormationTag", toolbar, "FORMATION", Sprite("Controls/FormationTag"), Ink, false, out TMP_Text formationLabel); Fixed((RectTransform)formation.transform, new Vector2(0, .5f), new Vector2(70, 0), new Vector2(140, 44));
            var transform = Button("TransformationButton", toolbar, "SHOW HUMAN", Sprite("Controls/MoonshiftButton"), Color.white, true, out TMP_Text transformLabel); Fixed((RectTransform)transform.transform, new Vector2(1, .5f), new Vector2(-70, 0), new Vector2(140, 44));
            var cards = new List<HeroDeploymentCard>();
            for (int i = 0; i < 3; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/CardPortrait.prefab")); instance.transform.SetParent(root, false);
                instance.name = "CardSlot_" + i;
                var card = instance.GetComponent<HeroDeploymentCard>(); card.Bind(definitions[i], i == 0 ? 100 : i == 1 ? 60 : 40, true); card.SetSelected(i == 0); cards.Add(card);
            }
            var footer = Label("EditableFooter", root, "Defeats charge every card", 11, new Color(1, .96f, .8f), true);
            var hand = root.gameObject.AddComponent<ResponsiveHeroHand>(); hand.cards = cards.ToArray(); hand.toolbar = toolbar; hand.footer = (RectTransform)footer.transform; hand.formationButton = formation; hand.transformationButton = transform; hand.formationLabel = formationLabel; hand.transformationLabel = transformLabel;
            hand.Layout(); return root.gameObject;
        }

        private static GameObject MakeCanvas()
        {
            var root = new GameObject("PortraitCardCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.pixelPerfect = true;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(390, 844); scaler.matchWidthOrHeight = 1;
            return root;
        }

        private static void MakeShowcase(HeroCardDefinition[] definitions)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("UI Preview Camera").AddComponent<Camera>(); camera.tag = "MainCamera"; camera.orthographic = true; camera.transform.position = new Vector3(0, 0, -10); camera.backgroundColor = new Color(.06f, .17f, .18f); camera.clearFlags = CameraClearFlags.SolidColor;
            var canvasRoot = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/PortraitCardCanvas.prefab"));
            var canvas = canvasRoot.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            var backdrop = NewRect("ShowcaseForestBackground", canvasRoot.transform); Stretch(backdrop); backdrop.SetAsFirstSibling(); var raw = backdrop.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false; raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-stage-master-v01.png");
            var fit = backdrop.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = raw.texture.width / (float)raw.texture.height;
            var safe = canvasRoot.GetComponentInChildren<PortraitSafeArea>();
            var hand = canvasRoot.GetComponentInChildren<ResponsiveHeroHand>();
            var title = Image("ShowcaseTitlePlate", safe.transform, Sprite("Controls/FormationTag"), true, 6); Fixed((RectTransform)title.transform, new Vector2(.5f, 1), new Vector2(0, -27), new Vector2(340, 44));
            var titleText = Label("EditableTitle", title.transform, "WORLD OF THE PACK", 22, Ink); Stretch((RectTransform)titleText.transform, 9);
            var status = Label("DemoStatus", safe.transform, "Select a hero and test the card", 12, Color.white, true); Fixed((RectTransform)status.transform, new Vector2(.5f, 1), new Vector2(0, -69), new Vector2(360, 32));
            var showcase = new GameObject("CardUIShowcase").AddComponent<CardUIShowcase>(); showcase.hand = hand; showcase.roster = definitions; showcase.status = status;
            var gallery = NewRect("HeroSelectorGrid", safe.transform); Fixed(gallery, new Vector2(.5f, 1), new Vector2(0, -172), new Vector2(330, 180));
            var grid = gallery.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(76, 84); grid.spacing = new Vector2(8, 10); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4; grid.childAlignment = TextAnchor.UpperCenter;
            for (int i = 0; i < definitions.Length; i++)
            {
                var selector = NewRect("Select_" + Ids[i], gallery); var bg = selector.gameObject.AddComponent<Image>(); Style(bg, Sprite("Frames/PortraitBackdrop")); bg.raycastTarget = true; var button = selector.gameObject.AddComponent<Button>(); SetButtonColors(button);
                var portrait = Image("WolfPortrait", selector, definitions[i].wolfPortrait); Stretch((RectTransform)portrait.transform, 3); portrait.preserveAspect = true;
                var plate = Image("Nameplate", selector, Sprite("Controls/Nameplate"), true, 10); BottomStretch((RectTransform)plate.transform, 0, 0, 20); var label = Label("Name", plate.transform, definitions[i].shortName, 11, Ink); Stretch((RectTransform)label.transform, 2);
                var border = Image("Frame", selector, Sprite("Frames/CardFrame"), true, 14); Stretch((RectTransform)border.transform); border.fillCenter = false;
                UnityEventTools.AddIntPersistentListener(button.onClick, showcase.SelectHero, i);
            }
            var demoEnergy = Button("DemoEnergy", safe.transform, "+20 ENERGY", Sprite("Controls/FormationTag"), Ink, false, out _); Fixed((RectTransform)demoEnergy.transform, new Vector2(.5f, 1), new Vector2(0, -302), new Vector2(150, 44)); UnityEventTools.AddPersistentListener(demoEnergy.onClick, showcase.AddDemoEnergy);
            var info = Label("ShowcaseOnlyLabel", safe.transform, "UI showcase - deployment is simulated", 11, Color.white, true); Fixed((RectTransform)info.transform, new Vector2(.5f, 1), new Vector2(0, -337), new Vector2(350, 22));
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            SetGameView(1170, 2532, "Pack iPhone Portrait");
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Root + "/Scenes/CardPortraitShowcase.unity");
        }

        private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/" + name + ".png");
        private static RectTransform NewRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); if (parent != null) rect.SetParent(parent, false); return rect;
        }
        private static Image Image(string name, Transform parent, Sprite sprite, bool sliced = false, float multiplier = 1)
        {
            var rect = NewRect(name, parent); var image = rect.gameObject.AddComponent<Image>(); Style(image, sprite, sliced, multiplier); return image;
        }
        private static void Style(Image image, Sprite sprite, bool sliced = false, float multiplier = 1)
        {
            image.sprite = sprite; image.raycastTarget = false; image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple; image.pixelsPerUnitMultiplier = multiplier;
        }
        private static Button Button(string name, Transform parent, string text, Sprite sprite, Color color, bool outline, out TMP_Text label)
        {
            var image = Image(name, parent, sprite, true, 6); image.raycastTarget = true; var button = image.gameObject.AddComponent<Button>(); SetButtonColors(button);
            label = Label("EditableLabel", image.transform, text, 16, color, outline); Stretch((RectTransform)label.transform, 8); return button;
        }
        private static void SetButtonColors(Button button)
        {
            var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1, .98f, .88f); colors.pressedColor = new Color(.83f, .87f, .9f); colors.disabledColor = Color.white; colors.fadeDuration = .08f; button.colors = colors;
        }
        private static TMP_Text Label(string name, Transform parent, string text, float size, Color color, bool outline = false)
        {
            var rect = NewRect(name, parent); var label = rect.gameObject.AddComponent<TextMeshProUGUI>(); label.font = font; label.fontSharedMaterial = outline ? outlined : font.material; label.text = text; label.color = color; label.fontSize = size; label.enableAutoSizing = true; label.fontSizeMax = size; label.fontSizeMin = size * .7f; label.alignment = TextAlignmentOptions.Center; label.textWrappingMode = TextWrappingModes.NoWrap; label.overflowMode = TextOverflowModes.Ellipsis; label.raycastTarget = false; label.margin = Vector4.zero; return label;
        }
        private static UIFlipbookEffect MakeEffect(string name, Transform parent, string sequence, Vector2 size)
        {
            var image = Image(name, parent, Sprite("VFX/" + sequence + "_00")); ((RectTransform)image.transform).sizeDelta = size; image.preserveAspect = true; image.enabled = false;
            var fx = image.gameObject.AddComponent<UIFlipbookEffect>(); fx.image = image; fx.frames = Enumerable.Range(0, 6).Select(i => Sprite("VFX/" + sequence + "_" + i.ToString("00"))).ToArray(); fx.framesPerSecond = sequence == "Ready" ? 8 : 12; return fx;
        }
        private static void Stretch(RectTransform rect, float inset = 0) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(inset, inset); rect.offsetMax = new Vector2(-inset, -inset); }
        private static void Fixed(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size) { rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
        private static void BottomStretch(RectTransform rect, float side, float bottom, float height) { rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(.5f, 0); rect.offsetMin = new Vector2(side, bottom); rect.offsetMax = new Vector2(-side, bottom + height); }
        private static void AnchorHand(RectTransform rect) { rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(.5f, 0); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(0, 300); }

        private static void SetGameView(int width, int height, string title)
        {
            var assembly = typeof(EditorWindow).Assembly; var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType); var sizes = singleton.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null);
            var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType"); var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, "Standalone") });
            var texts = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null); int index = Array.FindIndex(texts, t => t.Contains(title));
            if (index < 0)
            {
                var sizeType = assembly.GetType("UnityEditor.GameViewSize"); var mode = Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution");
                var entry = Activator.CreateInstance(sizeType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { mode, (object)width, height, title }, null);
                group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { entry }); index = ((string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null)).Length - 1;
            }
            var view = EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView")); view.GetType().GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, index); view.Repaint();
        }

        private static void VerifyUpdate()
        {
            if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying || EditorApplication.timeSinceStartup < next) return;
            var demo = UnityEngine.Object.FindAnyObjectByType<CardUIShowcase>(); if (demo == null) return;
            try
            {
                if (report == null) report = JsonUtility.FromJson<Report>(File.ReadAllText(ReportPath));
                switch (phase)
                {
                    case 0: RuntimeChecks(demo); PrepareCapture(demo, false, true, 0); break;
                    case 1: Capture(demo, "iPhone_Wolf_ReadyCharging.png", false); PrepareCapture(demo, false, false, 0); break;
                    case 2: Capture(demo, "iPhone_Human_ReadyCharging.png", false); PrepareCapture(demo, true, true, 3); break;
                    case 3: Capture(demo, "iPad_Wolf_ReadyCharging.png", true); PrepareCapture(demo, true, false, 3); break;
                    case 4: Capture(demo, "iPad_Human_ReadyCharging.png", true); PrepareCapture(demo, true, true, 6); break;
                    case 5: Capture(demo, "iPad_Tilly_Wolf.png", true); PrepareCapture(demo, true, false, 6); break;
                    case 6: Capture(demo, "iPad_Tilly_Human.png", true); PrepareCapture(demo, false, true, 0); break;
                    case 7: Click(demo.hand.cards[0].deployButton); break;
                    case 8: report.demoDeploymentConfirmed = demo.hand.cards[0].Energy == 0 && !demo.hand.cards[0].IsPending; Capture(demo, "iPhone_Deployment_Confirmed.png", false); break;
                    case 9:
                        report.deployedVfxStopped = !demo.hand.cards[0].deployedEffect.IsPlaying;
                        report.runtimeErrors = Errors.ToArray();
                        report.playModeVerified = report.assetsBuilt && report.allFourteenPortraits && report.chargingDisabled && report.readyClick && report.pendingNoSpend && report.rejectPreservesEnergy && report.confirmConsumesOnce && report.duplicateClickBlocked && report.dynamicText && report.portraitSwitch && report.raycastHit && report.deployedVfxPlayed && report.deployedVfxStopped && report.demoDeploymentConfirmed && report.iphoneLayout && report.ipadLayout && Errors.Count == 0;
                        SaveReport(); Debug.Log(report.playModeVerified ? "PACK_CARD_UI_PLAYMODE_OK" : "PACK_CARD_UI_PLAYMODE_REVIEW_REQUIRED"); EditorApplication.ExitPlaymode(); phase++; return;
                }
                phase++; next = EditorApplication.timeSinceStartup + (phase == 8 ? .43 : .65);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception); File.WriteAllText(Path.Combine(QA, "verification-failure.txt"), exception.ToString()); SessionState.SetBool(Pending, false); EditorApplication.ExitPlaymode();
            }
        }

        private static void RuntimeChecks(CardUIShowcase demo)
        {
            demo.autoConfirmForDemo = false; var card = demo.hand.cards[0]; int requests = 0; card.deployRequested.AddListener(_ => requests++);
            card.SetEnergy(99.9f); Click(card.deployButton); report.chargingDisabled = !card.IsReady && !card.deployButton.interactable && requests == 0 && card.energyLabel.text == "99/100";
            card.SetEnergy(100); Click(card.deployButton); report.readyClick = requests == 1 && card.IsPending; report.pendingNoSpend = card.Energy == 100;
            Click(card.deployButton); report.duplicateClickBlocked = requests == 1;
            card.CancelDeployment(); report.rejectPreservesEnergy = card.Energy == 100 && card.IsReady;
            Click(card.deployButton); int effects = card.deployedEffect.PlayCount; bool confirmed = card.ConfirmDeployment(); report.confirmConsumesOnce = confirmed && card.Energy == 0 && !card.ConfirmDeployment(); report.deployedVfxPlayed = card.deployedEffect.IsPlaying && card.deployedEffect.PlayCount == effects + 1;
            card.SetLocked(true); card.SetEnergy(100); Click(card.deployButton); report.chargingDisabled &= !card.deployButton.interactable && requests == 2; card.SetLocked(false);
            bool portraits = true, switches = true;
            foreach (var definition in demo.roster)
            {
                portraits &= definition.humanPortrait != null && definition.wolfPortrait != null;
                card.Bind(definition, 100, false); switches &= card.portraitView.portrait.sprite == definition.humanPortrait; card.SetForm(true); switches &= card.portraitView.portrait.sprite == definition.wolfPortrait;
            }
            report.allFourteenPortraits = portraits && demo.roster.Length == 7; report.portraitSwitch = switches;
            card.SetDisplayName("CUSTOM NAME"); report.dynamicText = card.nameLabel is TextMeshProUGUI && card.nameLabel.text == "CUSTOM NAME" && card.nameLabel.font != null;
            demo.SelectHero(0); Canvas.ForceUpdateCanvases();
            var canvas = demo.hand.GetComponentInParent<Canvas>(); var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, card.deployButton.transform.position) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits); report.raycastHit = hits.Any(h => h.gameObject == card.deployButton.gameObject || h.gameObject.transform.IsChildOf(card.deployButton.transform));
            demo.autoConfirmForDemo = true;
        }

        private static void Click(Button button)
        {
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static void PrepareCapture(CardUIShowcase demo, bool ipad, bool wolf, int hero)
        {
            SetGameView(ipad ? 1536 : 1170, ipad ? 2048 : 2532, ipad ? "Pack iPad Portrait" : "Pack iPhone Portrait");
            if (demo.wolf != wolf) demo.ToggleForm(); demo.SelectHero(hero);
            var safe = demo.hand.GetComponentInParent<PortraitSafeArea>(); safe.overrideSafeArea = true; safe.simulatedInsets = ipad ? new Vector4(0, .025f, 0, .025f) : new Vector4(0, .035f, 0, .047f); safe.Apply();
        }

        private static void Capture(CardUIShowcase demo, string file, bool ipad)
        {
            int width = ipad ? 1536 : 1170, height = ipad ? 2048 : 2532;
            var canvas = demo.hand.GetComponentInParent<Canvas>(); var camera = canvas.worldCamera; var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(width, height, 24); camera.targetTexture = target;
            var scaler = canvas.GetComponent<CanvasScaler>(); bool wasEnabled = scaler.enabled; scaler.enabled = false; canvas.scaleFactor = height / 844f;
            Canvas.ForceUpdateCanvases(); demo.hand.GetComponentInParent<PortraitSafeArea>().Apply(); demo.hand.Layout(); Canvas.ForceUpdateCanvases();
            bool layout = LayoutFits(demo.hand, canvas, width, height); if (ipad) report.ipadLayout |= layout; else report.iphoneLayout |= layout;
            camera.Render(); RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGBA32, false); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes(Path.Combine(QA, file), image.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(image);
            camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); scaler.enabled = wasEnabled; SaveReport();
        }

        private static bool LayoutFits(ResponsiveHeroHand hand, Canvas canvas, int width, int height)
        {
            var safe = hand.GetComponentInParent<PortraitSafeArea>(); Vector4 inset = safe.simulatedInsets; var minimum = new Vector2(inset.x * width, inset.y * height); var maximum = new Vector2((1 - inset.z) * width, (1 - inset.w) * height);
            foreach (var card in hand.cards)
            {
                var corners = new Vector3[4]; ((RectTransform)card.transform).GetWorldCorners(corners);
                foreach (var corner in corners) { var point = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corner); if (point.x < minimum.x - 2 || point.x > maximum.x + 2 || point.y < minimum.y - 2 || point.y > maximum.y + 2) return false; }
                if (((RectTransform)card.deployButton.transform).rect.height < 44) return false;
            }
            return true;
        }

        private static void SaveReport() => File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
    }
}
