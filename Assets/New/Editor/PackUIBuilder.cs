using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WorldOfThePack.Game;
using WorldOfThePack.UI;
using WorldOfThePack.NewUI;
using Object = UnityEngine.Object;

namespace WorldOfThePack.NewUI.Editor
{
    public static class PackUIBuilder
    {
        private const string Root = "Assets/New/";
        private static TMP_FontAsset _font;
        private static PackGameplayHUD _hud;
        private static readonly string[] HeroIds = { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" };

        [MenuItem("World of the Pack/New UI/Build in SampleScene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building UI.");
            if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
                throw new InvalidOperationException("Open SampleScene before running this builder.");
            Directory.CreateDirectory(Root + "Prefabs");
            ImportArt();
            CreateDot("DotIdle", new Color(0.75f, .57f, .30f));
            CreateDot("DotSelected", new Color(0, .8f, 1));
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/LilitaOne-Dynamic.asset");
            if (_font == null) _font = TMP_Settings.defaultFontAsset;
            var previous = GameObject.Find("NewPackUICanvas");
            if (previous != null) Object.DestroyImmediate(previous);
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (canvas.isRootCanvas) canvas.gameObject.SetActive(false);

            var canvasGo = new GameObject("NewPackUICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = 5;
            var canvasNew = canvasGo.GetComponent<Canvas>();
            canvasNew.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasNew.sortingOrder = 50;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(940, 1672);
            scaler.matchWidthOrHeight = .5f;
            _hud = canvasGo.AddComponent<PackGameplayHUD>();
            var safe = Rect("SafeArea", canvasGo.transform, 0, 0, 0, 0);
            Stretch(safe);
            safe.gameObject.AddComponent<PortraitSafeArea>();
            var top = Rect("TopHUD", safe, 0, 0, 940, 270);
            top.anchorMin = top.anchorMax = new Vector2(.5f, 1);
            top.pivot = new Vector2(.5f, 1);
            ImageAt("WavePlate", top, "WavePanel", 128, 72, 230, 76);
            _hud.waveLabel = Label("Wave_TMP", top, "WAVE 01", 128, 72, 209, 55, 43);
            _hud.waveLabel.enableAutoSizing = true;
            _hud.waveLabel.fontSizeMin = 26;
            _hud.pauseButton = ButtonAt("Pause", top, "PauseButton", 885, 56, 82, 82);
            ImageAt("PackPlate", top, "PackPanel", 470, 66, 276, 68);
            Label("Pack_TMP", top, "LEADER HEALTH", 470, 45, 230, 24, 22);
            _hud.heartFull = Sprite("Heart_Full"); _hud.heartEmpty = Sprite("Heart_Empty");
            _hud.hearts = new Image[5];
            for (int i = 0; i < 5; i++) _hud.hearts[i] = ImageAt("Heart_" + (i + 1), top, "Heart_Full", 383 + i * 43, 74, 39, 35);

            ImageAt("CampPlate", top, "CampPanel", 793, 140, 278, 90);
            ImageAt("TentIcon", top, "CampTent", 689, 126, 55, 55);
            _hud.campLabel = Label("Distance_TMP", top, "NEXT CAMP • 320m", 813, 127, 206, 28, 24);
            _hud.campLabel.enableAutoSizing = true; _hud.campLabel.fontSizeMin = 17;
            ImageAt("DistanceTrack", top, "EnergyTrack", 803, 158, 218, 19);
            _hud.campFill = Fill("DistanceFill", top, "EnergyFill", 803, 158, 200, 10);

            ImageAt("BerryPlate", top, "CampPanel", 133, 156, 242, 65);
            ImageAt("BerryIcon", top, "BerryIcon", 42, 147, 53, 53);
            _hud.berryLabel = Label("Berry_TMP", top, "5 / 100", 148, 141, 155, 28, 23);

            var boss = Rect("BossHUD", top, 316, 160, 308, 54);
            _hud.bossPanel = boss.gameObject;
            ImageAt("BossPlate", boss, "CampPanel", 154, 27, 308, 54);
            ImageAt("Acorn", boss, "Boss_AcornIcon", 16, 28, 45, 56);
            _hud.bossLabel = Label("BossName_TMP", boss, "BRAMBLE BRUTE", 170, 13, 264, 25, 23);
            ImageAt("BossTrack", boss, "EnergyTrack", 171, 40, 266, 20);
            _hud.bossFill = Fill("BossFill", boss, "EnergyFill", 171, 40, 250, 11);
            _hud.bossFill.color = new Color(1, .12f, .1f);
            boss.gameObject.SetActive(false);

            var bottom = Rect("CardHand", safe, 0, 0, 940, 490);
            bottom.anchorMin = bottom.anchorMax = new Vector2(.5f, 0);
            bottom.pivot = new Vector2(.5f, 0);
            ImageAt("WoodTray", bottom, "HandTray", 470, 254, 940, 472);
            _hud.formationButton = ButtonAt("Formation", bottom, "NameRibbon", 160, 52, 280, 68);
            Label("Formation_TMP", _hud.formationButton.transform, "FORMATION", 140, 34, 250, 44, 30, true);
            _hud.moonButton = ButtonAt("Moonshift", bottom, "ChargingButton", 775, 52, 294, 68);
            ImageAt("MoonIcon", bottom, "Badges/Moon", 672, 52, 49, 49).preserveAspect = true;
            _hud.moonLabel = Label("Moonshift_TMP", bottom, "MOONSHIFT", 795, 52, 223, 42, 25);

            var viewport = Rect("CardViewport", bottom, 18, 91, 904, 338);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, .001f);
            var pager = viewport.gameObject.AddComponent<PackCardPager>();
            pager.viewport = viewport; pager.horizontal = true; pager.vertical = false;
            pager.inertia = false; pager.movementType = ScrollRect.MovementType.Clamped;
            var content = Rect("Cards", viewport, 0, 0, 904 + 4 * 306, 338);
            content.anchorMin = content.anchorMax = new Vector2(0, .5f);
            content.pivot = new Vector2(0, .5f); content.anchoredPosition = Vector2.zero;
            pager.content = content;
            pager.stops = 5;
            pager.selectedDot = Sprite("DotSelected"); pager.idleDot = Sprite("DotIdle");
            pager.dots = new Button[5];
            for (int i = 0; i < 5; i++) pager.dots[i] = ButtonAt("PageDot_" + (i + 1), bottom, "DotIdle", 400 + i * 35, 52, 27, 27);

            var cards = new HeroDeploymentCard[7];
            var definitions = new HeroCardDefinition[7];
            for (int i = 0; i < 7; i++)
            {
                definitions[i] = ConfigureDefinition(HeroIds[i]);
                cards[i] = BuildCard(content, definitions[i], i);
            }
            _hud.cards = cards;
            var deck = Object.FindFirstObjectByType<CardDeckManager>();
            if (deck == null) throw new InvalidOperationException("SampleScene requires its existing CardDeckManager.");
            var so = new SerializedObject(deck);
            so.FindProperty("heroHand").objectReferenceValue = null;
            var slots = so.FindProperty("cardSlots"); slots.arraySize = cards.Length;
            var defs = so.FindProperty("heroDefinitions"); defs.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++)
            {
                slots.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
                defs.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(deck);
            _hud.deck = deck;
            Label("Hint_TMP", bottom, "Defeats charge every card", 470, 456, 850, 35, 27);
            BuildModals(safe);
            if (Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            else
            {
                var es = Object.FindFirstObjectByType<EventSystem>();
                var legacy = es.GetComponent<StandaloneInputModule>();
                if (legacy != null) Object.DestroyImmediate(legacy);
                if (es.GetComponent<InputSystemUIInputModule>() == null) es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            // The scene owns the complete HUD; retain reusable individual cards only.
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            WorldOfThePack.Expedition.Editor.ExpeditionPolishBuilder.Build();
            Debug.Log("NEW_UI_BUILD_OK: SampleScene, 7 cards, 5 three-card window stops, editable TMP, real deployment and route distance.");
        }

        private static HeroDeploymentCard BuildCard(RectTransform content, HeroCardDefinition def, int index)
        {
            var root = Rect("Card_" + def.heroId, content, index * 306, 0, 292, 332);
            root.anchoredPosition = new Vector2(146 + index * 306, 0);
            root.anchorMin = root.anchorMax = new Vector2(0, .5f); root.pivot = new Vector2(.5f, .5f);
            root.gameObject.SetActive(false);
            var background = root.gameObject.AddComponent<Image>(); background.sprite = Sprite("CardFrame_Legacy");
            ImageAt("PortraitBackdrop", root, "PortraitBackdrop", 146, 103, 266, 185);
            var portrait = ImageAt("CharacterPortrait", root, def.heroId + "_Human", 146, 103, 260, 181);
            portrait.preserveAspect = true;
            var view = portrait.gameObject.AddComponent<CardPortraitView>(); view.portrait = portrait;
            ImageAt("GoldenFrame", root, "CardFrame_Generated", 146, 166, 292, 332);
            ImageAt("NameRibbon", root, "NameRibbon", 146, 202, 266, 42);
            var name = Label("Name_TMP", root, def.shortName.ToUpperInvariant(), 146, 202, 248, 38, 31, true);
            ImageAt("ChargeTrack", root, "EnergyTrack", 146, 241, 258, 36);
            var fill = Fill("ChargeFill_0_100", root, "EnergyFill", 146, 241, 239, 23);
            var charge = Label("Energy_TMP", root, "0 / 100", 146, 241, 244, 30, 25, true);
            var button = ButtonAt("DeployOrCharging", root, "ChargingButton", 118, 291, 204, 57);
            var action = Label("Action_TMP", root, "CHARGING", 118, 291, 190, 46, 28);
            var role = index == 1 || index == 4 ? "Badges/Leaf" : index == 2 || index == 5 ? "Badges/Moon" : "Badges/Paw";
            ImageAt("CharacterRoleIcon", root, role, 42, 42, 54, 54).preserveAspect = true;
            var card = root.gameObject.AddComponent<HeroDeploymentCard>();
            card.definition = def; card.nameLabel = name; card.energyLabel = charge;
            card.portraitView = view; card.energyFill = fill;
            card.actionArtwork = button.image; card.deployButton = button;
            card.readyArtwork = Sprite("DeployButton"); card.chargingArtwork = Sprite("ChargingButton");
            card.readyFillSprite = card.chargingFillSprite = Sprite("EnergyFill");
            var copy = root.gameObject.AddComponent<PackCardCopy>(); copy.card = card; copy.actionLabel = action;
            card.Bind(def, index == 0 ? 100 : index == 1 ? 60 : index == 2 ? 40 : 20 + index * 10);
            root.gameObject.SetActive(true);
            // SharedHeroCardBuilder authors linked variants after the full hand is built.
            return card;
        }

        private static HeroCardDefinition ConfigureDefinition(string id)
        {
            var def = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>(Root + "Data/" + id + ".asset");
            if (def == null) throw new InvalidOperationException("Missing copied hero definition: " + id);
            def.humanPortrait = Sprite(id + "_Human");
            def.wolfPortrait = Sprite(id + "_Wolf");
            EditorUtility.SetDirty(def); return def;
        }
        private static void BuildModals(Transform safe)
        {
            var pause = Modal("PauseModal", safe, "PAUSED"); _hud.pausePanel = pause.gameObject;
            _hud.resumeButton = ModalButton(pause, "Resume", "RESUME", 120);
            pause.gameObject.SetActive(false);
            var camp = Modal("CampModal", safe, "CAMP • CHOOSE YOUR REST"); _hud.campPanel = camp.gameObject;
            string[] options = { "RESTORE 25% HEALTH", "PACK POWER +20%", "CARD CHARGE +30%" };
            _hud.campOptions = new Button[3];
            for (int i = 0; i < 3; i++) _hud.campOptions[i] = ModalButton(camp, "CampOption" + i, options[i], 90 + i * 72);
            camp.gameObject.SetActive(false);
            var end = Modal("EndModal", safe, "PACK DEFEATED"); _hud.endPanel = end.gameObject;
            _hud.modalTitle = end.GetComponentInChildren<TMP_Text>();
            _hud.retryButton = ModalButton(end, "Retry", "PLAY AGAIN", 120);
            end.gameObject.SetActive(false);
        }
        private static RectTransform Modal(string name, Transform parent, string title)
        {
            var overlay = Rect(name, parent, 0, 0, 0, 0); Stretch(overlay);
            var shade = overlay.gameObject.AddComponent<Image>(); shade.color = new Color(.025f, .018f, .01f, .85f);
            var panel = Rect("Panel", overlay, 0, 0, 650, 520);
            panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f); panel.pivot = new Vector2(.5f, .5f); panel.anchoredPosition = Vector2.zero;
            ImageAt("Wood", panel, "HandTray", 325, 260, 650, 520);
            Label("Title_TMP", panel, title, 325, 40, 610, 55, 35);
            return overlay;
        }
        private static Button ModalButton(RectTransform overlay, string name, string text, float y)
        {
            var panel = overlay.Find("Panel");
            var b = ButtonAt(name, panel, "ChargingButton", 325, y, 510, 60);
            Label(name + "_TMP", panel, text, 325, y, 490, 55, 30); return b;
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.layer = 5;
            var r = (RectTransform)go.transform; r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
        }
        private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        private static Image ImageAt(string name, Transform p, string sprite, float x, float y, float w, float h)
        {
            var r = Rect(name, p, x - w / 2, y - h / 2, w, h);
            var im = r.gameObject.AddComponent<Image>(); im.sprite = Sprite(sprite); im.raycastTarget = false; return im;
        }
        private static Image Fill(string name, Transform p, string sprite, float x, float y, float w, float h)
        {
            var im = ImageAt(name, p, sprite, x, y, w, h);
            im.type = Image.Type.Filled; im.fillMethod = Image.FillMethod.Horizontal; im.fillOrigin = 0; im.fillAmount = .6f; return im;
        }
        private static Button ButtonAt(string name, Transform p, string sprite, float x, float y, float w, float h)
        {
            var im = ImageAt(name, p, sprite, x, y, w, h); im.raycastTarget = true;
            var b = im.gameObject.AddComponent<Button>(); b.targetGraphic = im;
            var colors = b.colors; colors.disabledColor = new Color(.65f, .72f, .8f); b.colors = colors; return b;
        }
        private static TMP_Text Label(string name, Transform p, string text, float x, float y, float w, float h, float size, bool dark = false)
        {
            var r = Rect(name, p, x - w / 2, y - h / 2, w, h);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.font = _font; t.text = text;
            t.fontSize = size; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
            t.color = dark ? new Color(.12f, .07f, .02f) : new Color(1, .96f, .78f);
            t.textWrappingMode = TextWrappingModes.NoWrap;
            if (!dark) { t.outlineColor = new Color32(38, 20, 9, 255); t.outlineWidth = .18f; }
            return t;
        }
        private static Sprite Sprite(string name)
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(Root + "Art/" + name + ".png").OfType<Sprite>().FirstOrDefault();
            if (sprite == null) throw new InvalidOperationException("Missing UI sprite " + name); return sprite;
        }
        private static void ImportArt()
        {
            AssetDatabase.Refresh();
            foreach (string path in Directory.GetFiles(Root + "Art", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                // Non-destructive alpha bounds: retain the original generated PNG, import one tightly fitted sprite.
                var raw = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                raw.LoadImage(File.ReadAllBytes(path));
                var pixels = raw.GetPixels32();
                int left = raw.width, bottom = raw.height, right = 0, top = 0;
                for (int y = 0; y < raw.height; y++) for (int x = 0; x < raw.width; x++)
                    if (pixels[y * raw.width + x].a > 12)
                    { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
                string name = Path.GetFileNameWithoutExtension(path);
                var factory = new SpriteDataProviderFactories(); factory.Init();
                var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
                var old = provider.GetSpriteRects();
                var id = old.Length > 0 ? old[0].spriteID : GUID.Generate();
                provider.SetSpriteRects(new[] { new SpriteRect { name = name, spriteID = id,
                    rect = new UnityEngine.Rect(left, bottom, Mathf.Max(1, right - left + 1), Mathf.Max(1, top - bottom + 1)),
                    pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center } });
                provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, id) });
                provider.Apply(); importer.SaveAndReimport(); Object.DestroyImmediate(raw);
            }
        }
        private static void CreateDot(string name, Color fill)
        {
            string path = Root + "Art/" + name + ".png";
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                Color color = distance < 25 ? fill * Mathf.Lerp(.65f, 1.2f, y / 64f) : new Color(.15f, .07f, .02f);
                color.a = Mathf.Clamp01(30 - distance); texture.SetPixel(x, y, color);
            }
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.SaveAndReimport();
        }
    }
}
