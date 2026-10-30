using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Game;
using WorldOfThePack.UI;
using WorldOfThePack.VFX;

namespace WorldOfThePack.EditorTools
{
    public static class GameSceneBuilder
    {
        private const string PrefabsRoot = "Assets/WorldOfThePack_Game/Prefabs";
        private const string UnitsPath = PrefabsRoot + "/Units";
        private const string UIPath = PrefabsRoot + "/UI";

        [MenuItem("World of the Pack/Game/Build Complete Game and Setup Scene")]
        public static void BuildCompleteGame()
        {
            Directory.CreateDirectory(UnitsPath);
            Directory.CreateDirectory(UIPath);

            BuildUnitPrefabs();
            GameObject hudPrefab = BuildHUDCanvasPrefab();
            GameObject handPrefab = BuildCardHandCanvasPrefab();

            SetupSampleScene(hudPrefab, handPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WorldOfThePack] Game Prefabs built & SampleScene successfully configured!");
        }

        private static void BuildUnitPrefabs()
        {
            string[] heroes = { "HeroWolvy", "PrincessElva", "CaptainBobo", "FairyLulu", "ScoutPip", "WizardMomo", "TinkerTilly" };

            foreach (string hero in heroes)
            {
                string wolfPath = $"Assets/Characters/{hero}/Prefabs/{hero}_Wolf_North.prefab";
                string humanPath = $"Assets/Characters/{hero}/Prefabs/{hero}_Human_North.prefab";

                GameObject wolfSrc = AssetDatabase.LoadAssetAtPath<GameObject>(wolfPath);
                GameObject humanSrc = AssetDatabase.LoadAssetAtPath<GameObject>(humanPath);

                GameObject root = new GameObject(hero + "_PackUnit");

                GameObject wolfChild = wolfSrc != null ? PrefabUtility.InstantiatePrefab(wolfSrc, root.transform) as GameObject : new GameObject("Wolf");
                wolfChild.name = "WolfVisual";
                wolfChild.transform.localPosition = Vector3.zero;

                GameObject humanChild = humanSrc != null ? PrefabUtility.InstantiatePrefab(humanSrc, root.transform) as GameObject : new GameObject("Human");
                humanChild.name = "HumanVisual";
                humanChild.transform.localPosition = Vector3.zero;
                humanChild.SetActive(false);

                HeroUnit unit = root.AddComponent<HeroUnit>();

                // Configure serialized fields via SerializedObject for clean persistence
                SerializedObject so = new SerializedObject(unit);
                so.FindProperty("heroId").stringValue = hero;
                so.FindProperty("wolfVisual").objectReferenceValue = wolfChild;
                so.FindProperty("humanVisual").objectReferenceValue = humanChild;
                so.FindProperty("isWolfForm").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();

                string savePath = $"{UnitsPath}/{hero}_PackUnit.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, savePath);
                Object.DestroyImmediate(root);
            }

            // Build Enemy Prefabs
            BuildEnemyPrefab(EnemyType.Soldier, "Assets/Characters/CaptainBobo/Wolf/CaptainBobo_Wolf_North.png", new Color(0.95f, 0.45f, 0.45f), 1.0f);
            BuildEnemyPrefab(EnemyType.Hound, "Assets/Characters/ScoutPip/Wolf/ScoutPip_Wolf_North.png", new Color(0.85f, 0.65f, 0.35f), 0.9f);
            BuildEnemyPrefab(EnemyType.Captain, "Assets/Characters/CaptainBobo/Wolf/CaptainBobo_Wolf_North.png", new Color(1.0f, 0.3f, 0.3f), 1.35f);
        }

        private static void BuildEnemyPrefab(EnemyType type, string spriteSheetPath, Color tint, float scale)
        {
            GameObject root = new GameObject("Enemy_" + type);
            GameObject body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
            var allSprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
            foreach (var a in allSprites)
            {
                if (a is Sprite s) { sr.sprite = s; break; }
            }
            sr.color = tint;

            EnemyUnit enemy = root.AddComponent<EnemyUnit>();
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("enemyType").enumValueIndex = (int)type;
            so.ApplyModifiedPropertiesWithoutUndo();

            string savePath = $"{UnitsPath}/Enemy_{type}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, savePath);
            Object.DestroyImmediate(root);
        }

        private static GameObject BuildHUDCanvasPrefab()
        {
            GameObject canvasObj = new GameObject("GameHUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            DeviceSafeAreaAdapter safeAdapter = canvasObj.AddComponent<DeviceSafeAreaAdapter>();

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Dynamic.asset");

            // Top HUD Panel with translucent header backdrop
            GameObject topPanel = new GameObject("TopHUDPanel");
            topPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform topRt = topPanel.AddComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(0f, 210f);
            topRt.anchoredPosition = Vector2.zero;

            Image topBg = topPanel.AddComponent<Image>();
            topBg.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);

            // Pack Health Background & Fill
            GameObject hpBg = new GameObject("PackHP_BG");
            hpBg.transform.SetParent(topPanel.transform, false);
            RectTransform hpBgRt = hpBg.AddComponent<RectTransform>();
            hpBgRt.anchorMin = new Vector2(0.08f, 0.65f);
            hpBgRt.anchorMax = new Vector2(0.68f, 0.92f);
            hpBgRt.offsetMin = hpBgRt.offsetMax = Vector2.zero;
            Image hpBgImg = hpBg.AddComponent<Image>();
            hpBgImg.color = new Color(0.12f, 0.15f, 0.18f, 0.9f);

            GameObject hpFill = new GameObject("PackHP_Fill");
            hpFill.transform.SetParent(hpBg.transform, false);
            RectTransform hpFillRt = hpFill.AddComponent<RectTransform>();
            hpFillRt.anchorMin = Vector2.zero;
            hpFillRt.anchorMax = Vector2.one;
            hpFillRt.offsetMin = hpFillRt.offsetMax = Vector2.zero;
            Image hpFillImg = hpFill.AddComponent<Image>();
            hpFillImg.type = Image.Type.Filled;
            hpFillImg.fillMethod = Image.FillMethod.Horizontal;
            hpFillImg.color = new Color(0.25f, 0.85f, 0.45f, 1f);

            GameObject hpTextObj = new GameObject("PackHP_Text");
            hpTextObj.transform.SetParent(hpBg.transform, false);
            RectTransform hpTextRt = hpTextObj.AddComponent<RectTransform>();
            hpTextRt.anchorMin = Vector2.zero;
            hpTextRt.anchorMax = Vector2.one;
            hpTextRt.offsetMin = hpTextRt.offsetMax = Vector2.zero;
            TextMeshProUGUI hpTmp = hpTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) hpTmp.font = font;
            hpTmp.alignment = TextAlignmentOptions.Center;
            hpTmp.fontSize = 28;
            hpTmp.text = "PACK HEALTH: 100/100";
            hpTmp.color = Color.white;

            // Pause Button
            GameObject pauseBtnObj = new GameObject("PauseButton");
            pauseBtnObj.transform.SetParent(topPanel.transform, false);
            RectTransform pauseRt = pauseBtnObj.AddComponent<RectTransform>();
            pauseRt.anchorMin = new Vector2(0.78f, 0.65f);
            pauseRt.anchorMax = new Vector2(0.92f, 0.92f);
            pauseRt.offsetMin = pauseRt.offsetMax = Vector2.zero;
            Image pauseImg = pauseBtnObj.AddComponent<Image>();
            pauseImg.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
            Button pauseBtn = pauseBtnObj.AddComponent<Button>();

            GameObject pauseTextObj = new GameObject("Text");
            pauseTextObj.transform.SetParent(pauseBtnObj.transform, false);
            RectTransform ptRt = pauseTextObj.AddComponent<RectTransform>();
            ptRt.anchorMin = Vector2.zero; ptRt.anchorMax = Vector2.one; ptRt.offsetMin = ptRt.offsetMax = Vector2.zero;
            TextMeshProUGUI pauseTmp = pauseTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) pauseTmp.font = font;
            pauseTmp.alignment = TextAlignmentOptions.Center;
            pauseTmp.fontSize = 26;
            pauseTmp.text = "||";
            pauseTmp.color = Color.white;

            // Sub info row: Wave text, Pack units count, Score
            GameObject waveObj = new GameObject("WaveText");
            waveObj.transform.SetParent(topPanel.transform, false);
            RectTransform waveRt = waveObj.AddComponent<RectTransform>();
            waveRt.anchorMin = new Vector2(0.08f, 0.2f);
            waveRt.anchorMax = new Vector2(0.48f, 0.55f);
            waveRt.offsetMin = waveRt.offsetMax = Vector2.zero;
            TextMeshProUGUI waveTmp = waveObj.AddComponent<TextMeshProUGUI>();
            if (font != null) waveTmp.font = font;
            waveTmp.alignment = TextAlignmentOptions.Left;
            waveTmp.fontSize = 26;
            waveTmp.text = "WAVE 1: FOREST PATROL";
            waveTmp.color = new Color(1f, 0.92f, 0.5f);

            GameObject countObj = new GameObject("PackCountText");
            countObj.transform.SetParent(topPanel.transform, false);
            RectTransform countRt = countObj.AddComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.52f, 0.2f);
            countRt.anchorMax = new Vector2(0.72f, 0.55f);
            countRt.offsetMin = countRt.offsetMax = Vector2.zero;
            TextMeshProUGUI countTmp = countObj.AddComponent<TextMeshProUGUI>();
            if (font != null) countTmp.font = font;
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.fontSize = 24;
            countTmp.text = "PACK: 3/10";
            countTmp.color = new Color(0.6f, 0.9f, 1f);

            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(topPanel.transform, false);
            RectTransform scoreRt = scoreObj.AddComponent<RectTransform>();
            scoreRt.anchorMin = new Vector2(0.74f, 0.2f);
            scoreRt.anchorMax = new Vector2(0.92f, 0.55f);
            scoreRt.offsetMin = scoreRt.offsetMax = Vector2.zero;
            TextMeshProUGUI scoreTmp = scoreObj.AddComponent<TextMeshProUGUI>();
            if (font != null) scoreTmp.font = font;
            scoreTmp.alignment = TextAlignmentOptions.Right;
            scoreTmp.fontSize = 24;
            scoreTmp.text = "SCORE: 0";
            scoreTmp.color = Color.white;

            // Center Banner Panel
            GameObject bannerObj = new GameObject("BannerPanel");
            bannerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bannerRt = bannerObj.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.1f, 0.55f);
            bannerRt.anchorMax = new Vector2(0.9f, 0.68f);
            bannerRt.offsetMin = bannerRt.offsetMax = Vector2.zero;
            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.08f, 0.1f, 0.15f, 0.85f);

            GameObject bannerTxtObj = new GameObject("BannerText");
            bannerTxtObj.transform.SetParent(bannerObj.transform, false);
            RectTransform bTxtRt = bannerTxtObj.AddComponent<RectTransform>();
            bTxtRt.anchorMin = Vector2.zero; bTxtRt.anchorMax = Vector2.one; bTxtRt.offsetMin = bTxtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI bannerTmp = bannerTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) bannerTmp.font = font;
            bannerTmp.alignment = TextAlignmentOptions.Center;
            bannerTmp.fontSize = 42;
            bannerTmp.text = "WAVE 1\nFOREST PATROL";
            bannerTmp.color = new Color(1f, 0.85f, 0.2f);
            bannerObj.SetActive(false);

            // Game Over Modal
            GameObject goModal = BuildSimpleModal(canvasObj.transform, "GameOverModal", "PACK SCATTERED", "RETRY ESCAPE", font);
            goModal.SetActive(false);

            // Victory Modal
            GameObject vicModal = BuildSimpleModal(canvasObj.transform, "VictoryModal", "SANCTUARY REACHED!", "PLAY AGAIN", font);
            vicModal.SetActive(false);

            // Wire GameHUDController
            GameHUDController hud = canvasObj.AddComponent<GameHUDController>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("packHealthFill").objectReferenceValue = hpFillImg;
            so.FindProperty("packHealthLabel").objectReferenceValue = hpTmp;
            so.FindProperty("waveLabel").objectReferenceValue = waveTmp;
            so.FindProperty("packCountLabel").objectReferenceValue = countTmp;
            so.FindProperty("scoreLabel").objectReferenceValue = scoreTmp;
            so.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            so.FindProperty("bannerPanel").objectReferenceValue = bannerObj;
            so.FindProperty("bannerText").objectReferenceValue = bannerTmp;
            so.FindProperty("gameOverModal").objectReferenceValue = goModal;
            so.FindProperty("retryButton").objectReferenceValue = goModal.GetComponentInChildren<Button>();
            so.FindProperty("victoryModal").objectReferenceValue = vicModal;
            so.FindProperty("victoryContinueButton").objectReferenceValue = vicModal.GetComponentInChildren<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // Wire Safe Area Adapter
            SerializedObject soAdapter = new SerializedObject(safeAdapter);
            soAdapter.FindProperty("topHUDPanel").objectReferenceValue = topRt;
            soAdapter.ApplyModifiedPropertiesWithoutUndo();

            string savePath = $"{UIPath}/GameHUDCanvas.prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(canvasObj, savePath);
            Object.DestroyImmediate(canvasObj);
            return saved;
        }

        private static GameObject BuildSimpleModal(Transform parent, string name, string title, string btnText, TMP_FontAsset font)
        {
            GameObject modal = new GameObject(name);
            modal.transform.SetParent(parent, false);
            RectTransform rt = modal.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.12f, 0.35f);
            rt.anchorMax = new Vector2(0.88f, 0.65f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            Image bg = modal.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.18f, 0.96f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(modal.transform, false);
            RectTransform trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.05f, 0.55f);
            trt.anchorMax = new Vector2(0.95f, 0.9f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTmp.font = font;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontSize = 38;
            titleTmp.text = title;
            titleTmp.color = new Color(1f, 0.9f, 0.3f);

            GameObject btnObj = new GameObject("ActionButton");
            btnObj.transform.SetParent(modal.transform, false);
            RectTransform brt = btnObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.2f, 0.15f);
            brt.anchorMax = new Vector2(0.8f, 0.45f);
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.25f, 0.7f, 0.35f, 1f);
            btnObj.AddComponent<Button>();

            GameObject bTxtObj = new GameObject("Text");
            bTxtObj.transform.SetParent(btnObj.transform, false);
            RectTransform btrt = bTxtObj.AddComponent<RectTransform>();
            btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.offsetMin = btrt.offsetMax = Vector2.zero;
            TextMeshProUGUI bTmp = bTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) bTmp.font = font;
            bTmp.alignment = TextAlignmentOptions.Center;
            bTmp.fontSize = 28;
            bTmp.text = btnText;
            bTmp.color = Color.white;

            return modal;
        }

        private static GameObject BuildCardHandCanvasPrefab()
        {
            string handSrcPath = "Assets/UI/WorldOfThePack/Prefabs/PortraitCardCanvas.prefab";
            GameObject handSrc = AssetDatabase.LoadAssetAtPath<GameObject>(handSrcPath);

            GameObject root;
            if (handSrc != null)
            {
                root = PrefabUtility.InstantiatePrefab(handSrc) as GameObject;
                root.name = "GameCardHandCanvas";
            }
            else
            {
                root = new GameObject("GameCardHandCanvas");
                Canvas c = root.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler cs = root.AddComponent<CanvasScaler>();
                cs.referenceResolution = new Vector2(390, 844);
                root.AddComponent<GraphicRaycaster>();
            }

            // Ensure DeviceSafeAreaAdapter is present
            if (root.GetComponent<DeviceSafeAreaAdapter>() == null)
            {
                var adapter = root.AddComponent<DeviceSafeAreaAdapter>();
                var hand = root.GetComponentInChildren<ResponsiveHeroHand>();
                if (hand != null)
                {
                    SerializedObject so = new SerializedObject(adapter);
                    so.FindProperty("bottomCardsPanel").objectReferenceValue = hand.GetComponent<RectTransform>();
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            string savePath = $"{UIPath}/GameCardHandCanvas.prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, savePath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static void SetupSampleScene(GameObject hudPrefab, GameObject handPrefab)
        {
            var activeScene = EditorSceneManager.GetActiveScene();

            // Clear legacy objects in SampleScene except Camera & Light
            foreach (var rootGo in activeScene.GetRootGameObjects())
            {
                if (rootGo.name != "Main Camera" && rootGo.name != "Global Light 2D")
                {
                    Object.DestroyImmediate(rootGo);
                }
            }

            // Configure Camera
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj == null)
            {
                camObj = new GameObject("Main Camera");
                camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }
            camObj.tag = "MainCamera";
            Camera cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.18f, 0.28f, 0.22f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            if (camObj.GetComponent<CorridorCamera>() == null)
            {
                camObj.AddComponent<CorridorCamera>();
            }

            // Configure Global 2D Light
            GameObject lightObj = GameObject.Find("Global Light 2D");
            if (lightObj != null)
            {
                var light2d = lightObj.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
                if (light2d != null)
                {
                    light2d.color = new Color(1.0f, 0.98f, 0.92f);
                    light2d.intensity = 1.15f;
                }
            }

            // Setup Scrolling Forest Background
            GameObject envObj = new GameObject("Environment_Scroller");
            envObj.AddComponent<EndlessPathScroller>();

            // Setup Core Systems & Managers
            GameObject systemsObj = new GameObject("GameSystems");
            GameDirector director = systemsObj.AddComponent<GameDirector>();
            WaveManager waveManager = systemsObj.AddComponent<WaveManager>();
            CardDeckManager deckManager = systemsObj.AddComponent<CardDeckManager>();
            CombatVFXPool vfxPool = systemsObj.AddComponent<CombatVFXPool>();
            DamagePopupManager popupMgr = systemsObj.AddComponent<DamagePopupManager>();

            // Wire WaveManager Prefabs
            SerializedObject soWave = new SerializedObject(waveManager);
            soWave.FindProperty("soldierPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Enemy_Soldier.prefab");
            soWave.FindProperty("houndPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Enemy_Hound.prefab");
            soWave.FindProperty("captainPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Enemy_Captain.prefab");
            soWave.ApplyModifiedPropertiesWithoutUndo();

            // Setup UI Canvases
            if (hudPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(hudPrefab);
            }

            if (handPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(handPrefab);
            }

            // Setup EventSystem if missing
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
    }
}
