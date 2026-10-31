using System.Collections.Generic;
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

            // Polish card prefabs typography first
            PolishCardPrefab.PolishAllCards();

            BuildUnitPrefabs();
            BuildBossPrefab();

            GameObject hudPrefab = BuildHUDCanvasPrefab();
            GameObject handPrefab = BuildCardHandCanvasPrefab();

            SetupSampleScene(hudPrefab, handPrefab);
            BuildMainMenuScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WorldOfThePack] Complete Game Architecture, Cinematics, Biomes, Tutorial, MainMenu, and SampleScene successfully built!");
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
                wolfChild.SetActive(false);

                GameObject humanChild = humanSrc != null ? PrefabUtility.InstantiatePrefab(humanSrc, root.transform) as GameObject : new GameObject("Human");
                humanChild.name = "HumanVisual";
                humanChild.transform.localPosition = Vector3.zero;
                humanChild.SetActive(true);

                HeroUnit unit = root.AddComponent<HeroUnit>();

                string kitPath = $"Assets/WorldOfThePack_Game/Data/HeroKits/{hero}_Kit.asset";
                HeroKitDefinition kit = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>(kitPath);

                SerializedObject so = new SerializedObject(unit);
                so.FindProperty("heroId").stringValue = hero;
                so.FindProperty("heroKit").objectReferenceValue = kit;
                so.FindProperty("wolfVisual").objectReferenceValue = wolfChild;
                so.FindProperty("humanVisual").objectReferenceValue = humanChild;
                so.FindProperty("isWolfForm").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();

                string savePath = $"{UnitsPath}/{hero}_PackUnit.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, savePath);
                Object.DestroyImmediate(root);
            }

            // Build Standard Enemy Prefabs
            BuildEnemyPrefab(EnemyType.Soldier, "Assets/Characters/CaptainBobo/Wolf/CaptainBobo_Wolf_North.png", new Color(0.95f, 0.45f, 0.45f), 1.0f);
            BuildEnemyPrefab(EnemyType.Hound, "Assets/Characters/ScoutPip/Wolf/ScoutPip_Wolf_North.png", new Color(0.85f, 0.65f, 0.35f), 0.9f);
            BuildEnemyPrefab(EnemyType.Captain, "Assets/Characters/CaptainBobo/Wolf/CaptainBobo_Wolf_North.png", new Color(1.0f, 0.3f, 0.3f), 1.35f);
        }

        private static void BuildBossPrefab()
        {
            GameObject root = new GameObject("Boss_AncientThornGuardian");
            GameObject body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = Vector3.one * 1.45f;

            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
            var allSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/CaptainBobo/Wolf/CaptainBobo_Wolf_North.png");
            foreach (var a in allSprites)
            {
                if (a is Sprite s) { sr.sprite = s; break; }
            }
            sr.color = new Color(0.95f, 0.45f, 0.45f);

            BossUnit boss = root.AddComponent<BossUnit>();
            string bossAssetPath = "Assets/WorldOfThePack_Game/Data/Bosses/AncientThornGuardian_Boss.asset";
            BossDefinition bossDef = AssetDatabase.LoadAssetAtPath<BossDefinition>(bossAssetPath);

            SerializedObject so = new SerializedObject(boss);
            so.FindProperty("bossId").stringValue = "AncientThornGuardian";
            so.FindProperty("bossDefinition").objectReferenceValue = bossDef;
            so.ApplyModifiedPropertiesWithoutUndo();

            string savePath = $"{UnitsPath}/Boss_AncientThornGuardian.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, savePath);
            Object.DestroyImmediate(root);
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
            Material outlinedMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Outlined.mat");

            // Top HUD Panel
            GameObject topPanel = new GameObject("TopHUDPanel");
            topPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform topRt = topPanel.AddComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(0f, 260f);
            topRt.anchoredPosition = Vector2.zero;

            Image topBg = topPanel.AddComponent<Image>();
            topBg.color = new Color(0.08f, 0.12f, 0.18f, 0.94f);

            // Row 1 Left: Alpha Health
            GameObject hpBg = new GameObject("AlphaHP_BG");
            hpBg.transform.SetParent(topPanel.transform, false);
            RectTransform hpBgRt = hpBg.AddComponent<RectTransform>();
            hpBgRt.anchorMin = new Vector2(0.04f, 0.70f);
            hpBgRt.anchorMax = new Vector2(0.48f, 0.94f);
            hpBgRt.offsetMin = hpBgRt.offsetMax = Vector2.zero;
            Image hpBgImg = hpBg.AddComponent<Image>();
            hpBgImg.color = new Color(0.12f, 0.15f, 0.18f, 0.95f);

            GameObject hpFill = new GameObject("AlphaHP_Fill");
            hpFill.transform.SetParent(hpBg.transform, false);
            RectTransform hpFillRt = hpFill.AddComponent<RectTransform>();
            hpFillRt.anchorMin = Vector2.zero; hpFillRt.anchorMax = Vector2.one; hpFillRt.offsetMin = hpFillRt.offsetMax = Vector2.zero;
            Image hpFillImg = hpFill.AddComponent<Image>();
            hpFillImg.type = Image.Type.Filled;
            hpFillImg.fillMethod = Image.FillMethod.Horizontal;
            hpFillImg.color = new Color(0.25f, 0.85f, 0.45f, 1f);

            GameObject hpTextObj = new GameObject("AlphaHP_Text");
            hpTextObj.transform.SetParent(hpBg.transform, false);
            RectTransform hpTextRt = hpTextObj.AddComponent<RectTransform>();
            hpTextRt.anchorMin = Vector2.zero; hpTextRt.anchorMax = Vector2.one; hpTextRt.offsetMin = hpTextRt.offsetMax = Vector2.zero;
            TextMeshProUGUI hpTmp = hpTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { hpTmp.font = font; if (outlinedMat != null) hpTmp.fontSharedMaterial = outlinedMat; }
            hpTmp.alignment = TextAlignmentOptions.Center;
            hpTmp.fontSize = 24;
            hpTmp.text = "ALPHA: 130/130";
            hpTmp.color = Color.white;

            // Row 1 Center: Shared Energy Pool
            GameObject energyBg = new GameObject("SharedEnergy_BG");
            energyBg.transform.SetParent(topPanel.transform, false);
            RectTransform energyBgRt = energyBg.AddComponent<RectTransform>();
            energyBgRt.anchorMin = new Vector2(0.50f, 0.70f);
            energyBgRt.anchorMax = new Vector2(0.80f, 0.94f);
            energyBgRt.offsetMin = energyBgRt.offsetMax = Vector2.zero;
            Image energyBgImg = energyBg.AddComponent<Image>();
            energyBgImg.color = new Color(0.1f, 0.14f, 0.2f, 0.95f);

            GameObject energyFill = new GameObject("SharedEnergy_Fill");
            energyFill.transform.SetParent(energyBg.transform, false);
            RectTransform energyFillRt = energyFill.AddComponent<RectTransform>();
            energyFillRt.anchorMin = Vector2.zero; energyFillRt.anchorMax = Vector2.one; energyFillRt.offsetMin = energyFillRt.offsetMax = Vector2.zero;
            Image energyFillImg = energyFill.AddComponent<Image>();
            energyFillImg.type = Image.Type.Filled;
            energyFillImg.fillMethod = Image.FillMethod.Horizontal;
            energyFillImg.color = new Color(0.2f, 0.75f, 1f, 1f);

            GameObject energyTextObj = new GameObject("SharedEnergy_Text");
            energyTextObj.transform.SetParent(energyBg.transform, false);
            RectTransform energyTextRt = energyTextObj.AddComponent<RectTransform>();
            energyTextRt.anchorMin = Vector2.zero; energyTextRt.anchorMax = Vector2.one; energyTextRt.offsetMin = energyTextRt.offsetMax = Vector2.zero;
            TextMeshProUGUI energyTmp = energyTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { energyTmp.font = font; if (outlinedMat != null) energyTmp.fontSharedMaterial = outlinedMat; }
            energyTmp.alignment = TextAlignmentOptions.Center;
            energyTmp.fontSize = 24;
            energyTmp.text = "ENERGY: 5.0/10";
            energyTmp.color = Color.white;

            // Row 1 Right: Pause Button
            GameObject pauseBtnObj = new GameObject("PauseButton");
            pauseBtnObj.transform.SetParent(topPanel.transform, false);
            RectTransform pauseRt = pauseBtnObj.AddComponent<RectTransform>();
            pauseRt.anchorMin = new Vector2(0.83f, 0.70f);
            pauseRt.anchorMax = new Vector2(0.96f, 0.94f);
            pauseRt.offsetMin = pauseRt.offsetMax = Vector2.zero;
            Image pauseImg = pauseBtnObj.AddComponent<Image>();
            pauseImg.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
            Button pauseBtn = pauseBtnObj.AddComponent<Button>();

            GameObject pauseTextObj = new GameObject("Text");
            pauseTextObj.transform.SetParent(pauseBtnObj.transform, false);
            RectTransform ptRt = pauseTextObj.AddComponent<RectTransform>();
            ptRt.anchorMin = Vector2.zero; ptRt.anchorMax = Vector2.one; ptRt.offsetMin = ptRt.offsetMax = Vector2.zero;
            TextMeshProUGUI pauseTmp = pauseTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { pauseTmp.font = font; if (outlinedMat != null) pauseTmp.fontSharedMaterial = outlinedMat; }
            pauseTmp.alignment = TextAlignmentOptions.Center;
            pauseTmp.fontSize = 28;
            pauseTmp.text = "||";
            pauseTmp.color = Color.white;

            // Row 2: Pack Morale & Wolf Instinct
            GameObject moraleBg = new GameObject("Morale_BG");
            moraleBg.transform.SetParent(topPanel.transform, false);
            RectTransform moraleBgRt = moraleBg.AddComponent<RectTransform>();
            moraleBgRt.anchorMin = new Vector2(0.04f, 0.40f);
            moraleBgRt.anchorMax = new Vector2(0.96f, 0.64f);
            moraleBgRt.offsetMin = moraleBgRt.offsetMax = Vector2.zero;
            Image moraleBgImg = moraleBg.AddComponent<Image>();
            moraleBgImg.color = new Color(0.15f, 0.12f, 0.08f, 0.95f);

            GameObject moraleFill = new GameObject("Morale_Fill");
            moraleFill.transform.SetParent(moraleBg.transform, false);
            RectTransform moraleFillRt = moraleFill.AddComponent<RectTransform>();
            moraleFillRt.anchorMin = Vector2.zero; moraleFillRt.anchorMax = Vector2.one; moraleFillRt.offsetMin = moraleFillRt.offsetMax = Vector2.zero;
            Image moraleFillImg = moraleFill.AddComponent<Image>();
            moraleFillImg.type = Image.Type.Filled;
            moraleFillImg.fillMethod = Image.FillMethod.Horizontal;
            moraleFillImg.color = new Color(1f, 0.82f, 0.2f, 1f);

            GameObject moraleTextObj = new GameObject("Morale_Text");
            moraleTextObj.transform.SetParent(moraleBg.transform, false);
            RectTransform moraleTextRt = moraleTextObj.AddComponent<RectTransform>();
            moraleTextRt.anchorMin = Vector2.zero; moraleTextRt.anchorMax = Vector2.one; moraleTextRt.offsetMin = moraleTextRt.offsetMax = Vector2.zero;
            TextMeshProUGUI moraleTmp = moraleTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { moraleTmp.font = font; if (outlinedMat != null) moraleTmp.fontSharedMaterial = outlinedMat; }
            moraleTmp.alignment = TextAlignmentOptions.Center;
            moraleTmp.fontSize = 24;
            moraleTmp.text = "PACK MORALE: 0%";
            moraleTmp.color = Color.white;

            // Row 3: Wave Text, Allies Count, Score
            GameObject waveObj = new GameObject("WaveText");
            waveObj.transform.SetParent(topPanel.transform, false);
            RectTransform waveRt = waveObj.AddComponent<RectTransform>();
            waveRt.anchorMin = new Vector2(0.04f, 0.08f); waveRt.anchorMax = new Vector2(0.44f, 0.34f);
            waveRt.offsetMin = waveRt.offsetMax = Vector2.zero;
            TextMeshProUGUI waveTmp = waveObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { waveTmp.font = font; if (outlinedMat != null) waveTmp.fontSharedMaterial = outlinedMat; }
            waveTmp.alignment = TextAlignmentOptions.Left;
            waveTmp.fontSize = 24;
            waveTmp.text = "WAVE 01 - DEFEATED 0";
            waveTmp.color = new Color(1f, 0.92f, 0.5f);

            GameObject countObj = new GameObject("PackCountText");
            countObj.transform.SetParent(topPanel.transform, false);
            RectTransform countRt = countObj.AddComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.46f, 0.08f); countRt.anchorMax = new Vector2(0.70f, 0.34f);
            countRt.offsetMin = countRt.offsetMax = Vector2.zero;
            TextMeshProUGUI countTmp = countObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { countTmp.font = font; if (outlinedMat != null) countTmp.fontSharedMaterial = outlinedMat; }
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.fontSize = 22;
            countTmp.text = "ALLIES: 3/10";
            countTmp.color = new Color(0.6f, 0.9f, 1f);

            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(topPanel.transform, false);
            RectTransform scoreRt = scoreObj.AddComponent<RectTransform>();
            scoreRt.anchorMin = new Vector2(0.72f, 0.08f); scoreRt.anchorMax = new Vector2(0.96f, 0.34f);
            scoreRt.offsetMin = scoreRt.offsetMax = Vector2.zero;
            TextMeshProUGUI scoreTmp = scoreObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { scoreTmp.font = font; if (outlinedMat != null) scoreTmp.fontSharedMaterial = outlinedMat; }
            scoreTmp.alignment = TextAlignmentOptions.Right;
            scoreTmp.fontSize = 22;
            scoreTmp.text = "SCORE: 0";
            scoreTmp.color = Color.white;

            // Center Banner Panel
            GameObject bannerObj = new GameObject("BannerPanel");
            bannerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bannerRt = bannerObj.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.08f, 0.55f);
            bannerRt.anchorMax = new Vector2(0.92f, 0.68f);
            bannerRt.offsetMin = bannerRt.offsetMax = Vector2.zero;
            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.08f, 0.1f, 0.15f, 0.92f);

            GameObject bannerTxtObj = new GameObject("BannerText");
            bannerTxtObj.transform.SetParent(bannerObj.transform, false);
            RectTransform bTxtRt = bannerTxtObj.AddComponent<RectTransform>();
            bTxtRt.anchorMin = Vector2.zero; bTxtRt.anchorMax = Vector2.one; bTxtRt.offsetMin = bTxtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI bannerTmp = bannerTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { bannerTmp.font = font; if (outlinedMat != null) bannerTmp.fontSharedMaterial = outlinedMat; }
            bannerTmp.alignment = TextAlignmentOptions.Center;
            bannerTmp.fontSize = 38;
            bannerTmp.text = "THE ADVENTURE BEGINS!";
            bannerTmp.color = new Color(1f, 0.88f, 0.25f);
            bannerObj.SetActive(false);

            // Boss Health Bar Panel
            GameObject bossBarPanel = BuildBossHealthBar(canvasObj.transform, font, outlinedMat, out Image bossFill, out TMP_Text bName, out TMP_Text bTitle, out TMP_Text bPct);

            // Modals: Pause, GameOver, Victory, CampModal
            GameObject pauseModal = BuildPauseModal(canvasObj.transform, font, outlinedMat);
            pauseModal.SetActive(false);

            GameObject goModal = BuildSimpleModal(canvasObj.transform, "GameOverModal", "THE ALPHA HAS FALLEN", "RETRY EXPEDITION", font, outlinedMat);
            goModal.SetActive(false);

            GameObject vicModal = BuildSimpleModal(canvasObj.transform, "VictoryModal", "SANCTUARY REACHED!", "PLAY AGAIN", font, outlinedMat);
            vicModal.SetActive(false);

            GameObject campModal = BuildCampModal(canvasObj.transform, font, outlinedMat);
            campModal.SetActive(false);

            // Wire GameHUDController
            GameHUDController hud = canvasObj.AddComponent<GameHUDController>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("packHealthFill").objectReferenceValue = hpFillImg;
            so.FindProperty("packHealthLabel").objectReferenceValue = hpTmp;
            so.FindProperty("sharedEnergyFill").objectReferenceValue = energyFillImg;
            so.FindProperty("sharedEnergyLabel").objectReferenceValue = energyTmp;
            so.FindProperty("packMoraleFill").objectReferenceValue = moraleFillImg;
            so.FindProperty("packMoraleLabel").objectReferenceValue = moraleTmp;
            so.FindProperty("waveLabel").objectReferenceValue = waveTmp;
            so.FindProperty("packCountLabel").objectReferenceValue = countTmp;
            so.FindProperty("scoreLabel").objectReferenceValue = scoreTmp;
            so.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            so.FindProperty("bannerPanel").objectReferenceValue = bannerObj;
            so.FindProperty("bannerText").objectReferenceValue = bannerTmp;

            // Boss wiring
            so.FindProperty("bossHealthBarPanel").objectReferenceValue = bossBarPanel;
            so.FindProperty("bossNameLabel").objectReferenceValue = bName;
            so.FindProperty("bossTitleLabel").objectReferenceValue = bTitle;
            so.FindProperty("bossHealthFill").objectReferenceValue = bossFill;
            so.FindProperty("bossHealthPercentLabel").objectReferenceValue = bPct;

            // Modals wiring
            so.FindProperty("pauseModal").objectReferenceValue = pauseModal;
            so.FindProperty("resumeButton").objectReferenceValue = pauseModal.transform.Find("ResumeButton")?.GetComponent<Button>();
            so.FindProperty("optionsButton").objectReferenceValue = pauseModal.transform.Find("OptionsButton")?.GetComponent<Button>();
            so.FindProperty("mainMenuButton").objectReferenceValue = pauseModal.transform.Find("MainMenuButton")?.GetComponent<Button>();
            so.FindProperty("optionsSubPanel").objectReferenceValue = pauseModal.transform.Find("SettingsPanel")?.gameObject;
            so.FindProperty("gameOverModal").objectReferenceValue = goModal;
            so.FindProperty("retryButton").objectReferenceValue = goModal.GetComponentInChildren<Button>();
            so.FindProperty("victoryModal").objectReferenceValue = vicModal;
            so.FindProperty("victoryContinueButton").objectReferenceValue = vicModal.GetComponentInChildren<Button>();

            // CampModal wiring
            so.FindProperty("campModal").objectReferenceValue = campModal;
            so.FindProperty("campRestButton").objectReferenceValue = campModal.transform.Find("CampRestButton")?.GetComponent<Button>();
            so.FindProperty("campBuffButton").objectReferenceValue = campModal.transform.Find("CampBuffButton")?.GetComponent<Button>();
            so.FindProperty("campRouteForestButton").objectReferenceValue = campModal.transform.Find("CampRouteForest")?.GetComponent<Button>();
            so.FindProperty("campRouteCursedButton").objectReferenceValue = campModal.transform.Find("CampRouteCursed")?.GetComponent<Button>();
            so.FindProperty("campRouteMoonButton").objectReferenceValue = campModal.transform.Find("CampRouteMoon")?.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // Safe Area Adapter
            SerializedObject soAdapter = new SerializedObject(safeAdapter);
            soAdapter.FindProperty("topHUDPanel").objectReferenceValue = topRt;
            soAdapter.ApplyModifiedPropertiesWithoutUndo();

            string savePath = $"{UIPath}/GameHUDCanvas.prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(canvasObj, savePath);
            Object.DestroyImmediate(canvasObj);
            return saved;
        }

        private static GameObject BuildBossHealthBar(Transform parent, TMP_FontAsset font, Material mat, out Image fill, out TMP_Text nameTxt, out TMP_Text titleTxt, out TMP_Text pctTxt)
        {
            GameObject bossBarObj = new GameObject("BossHealthBarPanel");
            bossBarObj.transform.SetParent(parent, false);
            RectTransform rt = bossBarObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.12f, 0.74f);
            rt.anchorMax = new Vector2(0.88f, 0.85f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            Image bg = bossBarObj.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.06f, 0.08f, 0.96f);

            GameObject nameObj = new GameObject("BossName");
            nameObj.transform.SetParent(bossBarObj.transform, false);
            RectTransform nrt = nameObj.AddComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0.04f, 0.52f); nrt.anchorMax = new Vector2(0.55f, 0.95f);
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { nameTxt.font = font; if (mat != null) nameTxt.fontSharedMaterial = mat; }
            nameTxt.alignment = TextAlignmentOptions.Left;
            nameTxt.fontSize = 24;
            nameTxt.text = "ANCIENT THORN GUARDIAN";
            nameTxt.color = new Color(1f, 0.45f, 0.45f);

            GameObject titleObj = new GameObject("BossTitle");
            titleObj.transform.SetParent(bossBarObj.transform, false);
            RectTransform trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.52f, 0.52f); trt.anchorMax = new Vector2(0.96f, 0.95f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { titleTxt.font = font; if (mat != null) titleTxt.fontSharedMaterial = mat; }
            titleTxt.alignment = TextAlignmentOptions.Right;
            titleTxt.fontSize = 18;
            titleTxt.text = "Corrupted Protector";
            titleTxt.color = new Color(0.85f, 0.85f, 0.85f);

            GameObject track = new GameObject("Track");
            track.transform.SetParent(bossBarObj.transform, false);
            RectTransform trtTrack = track.AddComponent<RectTransform>();
            trtTrack.anchorMin = new Vector2(0.04f, 0.10f); trtTrack.anchorMax = new Vector2(0.96f, 0.46f);
            trtTrack.offsetMin = trtTrack.offsetMax = Vector2.zero;
            Image trackImg = track.AddComponent<Image>();
            trackImg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(track.transform, false);
            RectTransform frt = fillObj.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = frt.offsetMax = Vector2.zero;
            fill = fillObj.AddComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.color = new Color(0.95f, 0.2f, 0.25f, 1f);

            GameObject pctObj = new GameObject("PercentText");
            pctObj.transform.SetParent(track.transform, false);
            RectTransform prt = pctObj.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = prt.offsetMax = Vector2.zero;
            pctTxt = pctObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { pctTxt.font = font; if (mat != null) pctTxt.fontSharedMaterial = mat; }
            pctTxt.alignment = TextAlignmentOptions.Center;
            pctTxt.fontSize = 20;
            pctTxt.text = "100%";
            pctTxt.color = Color.white;

            bossBarObj.SetActive(false);
            return bossBarObj;
        }

        private static GameObject BuildPauseModal(Transform parent, TMP_FontAsset font, Material mat)
        {
            GameObject modal = new GameObject("PauseModal");
            modal.transform.SetParent(parent, false);
            RectTransform rt = modal.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.2f);
            rt.anchorMax = new Vector2(0.9f, 0.8f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            Image bg = modal.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.16f, 0.22f, 0.98f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(modal.transform, false);
            RectTransform trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.1f, 0.82f);
            trt.anchorMax = new Vector2(0.9f, 0.96f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { titleTmp.font = font; if (mat != null) titleTmp.fontSharedMaterial = mat; }
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontSize = 42;
            titleTmp.text = "PAUSED";
            titleTmp.color = new Color(1f, 0.9f, 0.35f);

            CreateModalButton(modal.transform, "ResumeButton", "▶  RESUME", new Vector2(0.15f, 0.64f), new Vector2(0.85f, 0.78f), new Color(0.25f, 0.75f, 0.35f), font, mat);
            CreateModalButton(modal.transform, "OptionsButton", "⚙  OPTIONS", new Vector2(0.15f, 0.48f), new Vector2(0.85f, 0.62f), new Color(0.45f, 0.35f, 0.25f), font, mat);
            CreateModalButton(modal.transform, "MainMenuButton", "🏠  MAIN MENU", new Vector2(0.15f, 0.32f), new Vector2(0.85f, 0.46f), new Color(0.35f, 0.40f, 0.50f), font, mat);

            GameObject settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(modal.transform, false);
            RectTransform srt = settingsPanel.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.1f, 0.04f);
            srt.anchorMax = new Vector2(0.9f, 0.28f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            Image sBg = settingsPanel.AddComponent<Image>();
            sBg.color = new Color(0.08f, 0.1f, 0.14f, 0.9f);

            GameObject sHeader = new GameObject("Header");
            sHeader.transform.SetParent(settingsPanel.transform, false);
            RectTransform shrt = sHeader.AddComponent<RectTransform>();
            shrt.anchorMin = new Vector2(0.05f, 0.65f);
            shrt.anchorMax = new Vector2(0.95f, 0.95f);
            shrt.offsetMin = shrt.offsetMax = Vector2.zero;
            TextMeshProUGUI shTmp = sHeader.AddComponent<TextMeshProUGUI>();
            if (font != null) { shTmp.font = font; if (mat != null) shTmp.fontSharedMaterial = mat; }
            shTmp.alignment = TextAlignmentOptions.Center;
            shTmp.fontSize = 22;
            shTmp.text = "AUDIO & HAPTICS";
            shTmp.color = new Color(0.85f, 0.85f, 0.85f);

            return modal;
        }

        private static GameObject CreateModalButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color color, TMP_FontAsset font, Material mat)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;

            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { tmp.font = font; if (mat != null) tmp.fontSharedMaterial = mat; }
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 28;
            tmp.text = label;
            tmp.color = Color.white;

            return btnObj;
        }

        private static GameObject BuildSimpleModal(Transform parent, string name, string title, string btnText, TMP_FontAsset font, Material mat)
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
            if (font != null) { titleTmp.font = font; if (mat != null) titleTmp.fontSharedMaterial = mat; }
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
            if (font != null) { bTmp.font = font; if (mat != null) bTmp.fontSharedMaterial = mat; }
            bTmp.alignment = TextAlignmentOptions.Center;
            bTmp.fontSize = 28;
            bTmp.text = btnText;
            bTmp.color = Color.white;

            return modal;
        }

        private static GameObject BuildCampModal(Transform parent, TMP_FontAsset font, Material mat)
        {
            GameObject modal = new GameObject("CampModal");
            modal.transform.SetParent(parent, false);
            RectTransform rt = modal.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.15f);
            rt.anchorMax = new Vector2(0.92f, 0.85f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            Image bg = modal.AddComponent<Image>();
            bg.color = new Color(0.09f, 0.13f, 0.20f, 0.98f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(modal.transform, false);
            RectTransform trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.05f, 0.88f);
            trt.anchorMax = new Vector2(0.95f, 0.97f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { titleTmp.font = font; if (mat != null) titleTmp.fontSharedMaterial = mat; }
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontSize = 36;
            titleTmp.text = "LANTERN REFUGE - CAMPSITE";
            titleTmp.color = new Color(1f, 0.88f, 0.35f);

            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(modal.transform, false);
            RectTransform srt = subObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.05f, 0.80f);
            srt.anchorMax = new Vector2(0.95f, 0.87f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { subTmp.font = font; if (mat != null) subTmp.fontSharedMaterial = mat; }
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.fontSize = 20;
            subTmp.text = "The expedition pauses to heal and choose the road ahead:";
            subTmp.color = new Color(0.8f, 0.9f, 1f);

            CreateModalButton(modal.transform, "CampRestButton", "HEARTH & REST (+50% Health)", new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.77f), new Color(0.2f, 0.65f, 0.35f), font, mat);
            CreateModalButton(modal.transform, "CampBuffButton", "WOLF BLESSING (+25% Pack Damage)", new Vector2(0.1f, 0.51f), new Vector2(0.9f, 0.63f), new Color(0.75f, 0.45f, 0.15f), font, mat);

            GameObject rHead = new GameObject("RouteHeader");
            rHead.transform.SetParent(modal.transform, false);
            RectTransform rhrt = rHead.AddComponent<RectTransform>();
            rhrt.anchorMin = new Vector2(0.1f, 0.42f);
            rhrt.anchorMax = new Vector2(0.9f, 0.48f);
            rhrt.offsetMin = rhrt.offsetMax = Vector2.zero;
            TextMeshProUGUI rhTmp = rHead.AddComponent<TextMeshProUGUI>();
            if (font != null) { rhTmp.font = font; if (mat != null) rhTmp.fontSharedMaterial = mat; }
            rhTmp.alignment = TextAlignmentOptions.Center;
            rhTmp.fontSize = 22;
            rhTmp.text = "--- CHOOSE NEXT ROUTE ---";
            rhTmp.color = new Color(1f, 0.95f, 0.6f);

            CreateModalButton(modal.transform, "CampRouteForest", "SUNLIT FOREST (+50% Energy Regen)", new Vector2(0.1f, 0.29f), new Vector2(0.9f, 0.40f), new Color(0.18f, 0.5f, 0.35f), font, mat);
            CreateModalButton(modal.transform, "CampRouteCursed", "SHADOW MIRE (+75% Score Multiplier)", new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.27f), new Color(0.55f, 0.2f, 0.25f), font, mat);
            CreateModalButton(modal.transform, "CampRouteMoon", "MOONLIT GLADE (Wolf Frenzy Surge)", new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.14f), new Color(0.25f, 0.35f, 0.7f), font, mat);

            return modal;
        }

        private static GameObject BuildCardHandCanvasPrefab()
        {
            GameObject canvasObj = new GameObject("GameCardHandCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.matchWidthOrHeight = 0.0f;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(canvasObj.transform, false);
            RectTransform safeRt = safeArea.AddComponent<RectTransform>();
            safeRt.anchorMin = Vector2.zero; safeRt.anchorMax = Vector2.one; safeRt.offsetMin = safeRt.offsetMax = Vector2.zero;

            GameObject handObj = new GameObject("HeroCardHand");
            handObj.transform.SetParent(safeArea.transform, false);
            RectTransform handRt = handObj.AddComponent<RectTransform>();
            handRt.anchorMin = new Vector2(0f, 0f);
            handRt.anchorMax = new Vector2(1f, 0f);
            handRt.pivot = new Vector2(0.5f, 0f);
            handRt.anchoredPosition = new Vector2(0f, 8f);
            handRt.sizeDelta = new Vector2(0f, 280f);

            GameObject scrollObj = new GameObject("CardsScrollView");
            scrollObj.transform.SetParent(handObj.transform, false);
            RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(4f, 4f);
            scrollRt.offsetMax = new Vector2(-4f, -4f);

            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.15f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.85f;

            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform vpRt = viewportObj.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.offsetMin = vpRt.offsetMax = Vector2.zero;
            viewportObj.AddComponent<RectMask2D>();

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRt = contentObj.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 0f);
            contentRt.anchorMax = new Vector2(0f, 1f);
            contentRt.pivot = new Vector2(0f, 0.5f);
            contentRt.anchoredPosition = Vector2.zero;

            scrollRect.viewport = vpRt;
            scrollRect.content = contentRt;

            // Instantiate all 7 Hero Cards
            string[] heroes = { "HeroWolvy", "PrincessElva", "CaptainBobo", "FairyLulu", "ScoutPip", "WizardMomo", "TinkerTilly" };
            List<HeroDeploymentCard> cardList = new List<HeroDeploymentCard>();

            for (int i = 0; i < heroes.Length; i++)
            {
                string hero = heroes[i];
                string prefabPath = $"Assets/UI/WorldOfThePack/Prefabs/{hero}_CardPortrait.prefab";
                GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (cardPrefab == null)
                {
                    cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/WorldOfThePack/Prefabs/CardPortrait.prefab");
                }

                GameObject cardInstance = cardPrefab != null ? PrefabUtility.InstantiatePrefab(cardPrefab, contentRt) as GameObject : new GameObject("CardSlot_" + i);
                cardInstance.name = "CardSlot_" + i + "_" + hero;

                HeroDeploymentCard depCard = cardInstance.GetComponent<HeroDeploymentCard>();
                if (depCard != null)
                {
                    string dataPath = $"Assets/UI/WorldOfThePack/Data/{hero}.asset";
                    HeroCardDefinition def = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>(dataPath);
                    if (def != null)
                    {
                        depCard.definition = def;
                        float initEnergy = (i == 0) ? 100f : (20f + i * 10f);
                        depCard.Bind(def, initEnergy, wolf: false);
                    }
                    cardList.Add(depCard);
                }
            }

            ResponsiveHeroHand heroHand = handObj.AddComponent<ResponsiveHeroHand>();
            heroHand.cards = cardList.ToArray();
            heroHand.scrollContent = contentRt;
            heroHand.cardWidth = 118f;
            heroHand.cardHeight = 216f;
            heroHand.sidePadding = 12f;
            heroHand.gap = 8f;
            heroHand.Layout();

            string savePath = $"{UIPath}/GameCardHandCanvas.prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(canvasObj, savePath);
            Object.DestroyImmediate(canvasObj);
            return saved;
        }

        private static void SetupSampleScene(GameObject hudPrefab, GameObject handPrefab)
        {
            var activeScene = EditorSceneManager.GetActiveScene();

            foreach (var rootGo in activeScene.GetRootGameObjects())
            {
                if (rootGo.name != "Main Camera" && rootGo.name != "Global Light 2D")
                {
                    Object.DestroyImmediate(rootGo);
                }
            }

            // Configure Camera with ExpeditionCamera
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
            cam.orthographicSize = 5.2f;
            cam.backgroundColor = new Color(0.18f, 0.28f, 0.22f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            CorridorCamera legacyCam = camObj.GetComponent<CorridorCamera>();
            if (legacyCam != null) Object.DestroyImmediate(legacyCam);

            if (camObj.GetComponent<ExpeditionCamera>() == null)
            {
                camObj.AddComponent<ExpeditionCamera>();
            }

            // Global 2D Light
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

            // World Biome Generator
            GameObject envObj = new GameObject("Environment_BiomeGenerator");
            envObj.AddComponent<WorldBiomeGenerator>();

            // 5-Lane Tactical Overlay
            GameObject laneOverlayObj = new GameObject("LaneOverlay");
            laneOverlayObj.AddComponent<LaneOverlayRenderer>();

            // Setup Core Systems & Directors
            GameObject systemsObj = new GameObject("GameSystems");
            systemsObj.AddComponent<SaveManager>();
            FormationController formationCtrl = systemsObj.AddComponent<FormationController>();
            ExpeditionIntroController introCtrl = systemsObj.AddComponent<ExpeditionIntroController>();
            GameDirector director = systemsObj.AddComponent<GameDirector>();
            WaveManager waveManager = systemsObj.AddComponent<WaveManager>();
            CampManager campManager = systemsObj.AddComponent<CampManager>();
            CardDeckManager deckManager = systemsObj.AddComponent<CardDeckManager>();
            StoryCampaignDirector campaignDirector = systemsObj.AddComponent<StoryCampaignDirector>();
            TutorialDirector tutorialDirector = systemsObj.AddComponent<TutorialDirector>();
            CinematicDirector cinematicDirector = systemsObj.AddComponent<CinematicDirector>();
            CombatVFXPool vfxPool = systemsObj.AddComponent<CombatVFXPool>();
            DamagePopupManager popupMgr = systemsObj.AddComponent<DamagePopupManager>();

            // Wire StoryCampaignDirector Boss Prefab
            SerializedObject soStory = new SerializedObject(campaignDirector);
            soStory.FindProperty("bossPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Boss_AncientThornGuardian.prefab");
            soStory.ApplyModifiedPropertiesWithoutUndo();

            // Wire GameDirector Hero Prefabs
            string[] heroNames = { "HeroWolvy", "PrincessElva", "CaptainBobo", "FairyLulu", "ScoutPip", "WizardMomo", "TinkerTilly" };
            SerializedObject soDirector = new SerializedObject(director);
            SerializedProperty heroPrefabsProp = soDirector.FindProperty("heroPrefabs");
            heroPrefabsProp.arraySize = heroNames.Length;
            for (int i = 0; i < heroNames.Length; i++)
            {
                var elem = heroPrefabsProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("heroId").stringValue = heroNames[i];
                elem.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/{heroNames[i]}_PackUnit.prefab");
            }
            soDirector.ApplyModifiedPropertiesWithoutUndo();

            // Card Drag Ghost Preview
            GameObject ghostPreviewObj = new GameObject("CardDragGhostPreview");
            ghostPreviewObj.AddComponent<CardDragGhostPreview>();

            // Wire WaveManager Prefabs
            SerializedObject soWave = new SerializedObject(waveManager);
            soWave.FindProperty("soldierPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Enemy_Soldier.prefab");
            soWave.FindProperty("houndPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Enemy_Hound.prefab");
            soWave.FindProperty("captainPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{UnitsPath}/Enemy_Captain.prefab");
            soWave.ApplyModifiedPropertiesWithoutUndo();

            // Instantiate UI Canvases
            GameObject hudInstance = null;
            if (hudPrefab != null)
            {
                hudInstance = PrefabUtility.InstantiatePrefab(hudPrefab) as GameObject;
            }

            GameObject handInstance = null;
            if (handPrefab != null)
            {
                handInstance = PrefabUtility.InstantiatePrefab(handPrefab) as GameObject;
            }

            // Wire CardDeckManager
            SerializedObject soDeck = new SerializedObject(deckManager);
            if (handInstance != null)
            {
                soDeck.FindProperty("heroHand").objectReferenceValue = handInstance.GetComponentInChildren<ResponsiveHeroHand>();
            }
            SerializedProperty heroDefsProp = soDeck.FindProperty("heroDefinitions");
            heroDefsProp.arraySize = heroNames.Length;
            for (int i = 0; i < heroNames.Length; i++)
            {
                heroDefsProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>($"Assets/UI/WorldOfThePack/Data/{heroNames[i]}.asset");
            }
            soDeck.ApplyModifiedPropertiesWithoutUndo();

            // Wire Intro Controller UI
            if (hudInstance != null)
            {
                SerializedObject soIntro = new SerializedObject(introCtrl);
                Transform bannerPanel = hudInstance.transform.Find("BannerPanel");
                if (bannerPanel != null)
                {
                    soIntro.FindProperty("bannerPanel").objectReferenceValue = bannerPanel.gameObject;
                    soIntro.FindProperty("bannerText").objectReferenceValue = bannerPanel.Find("BannerText")?.GetComponent<TMP_Text>();
                }
                Transform skipBtn = hudInstance.transform.Find("TopHUDPanel/SkipIntroButton");
                if (skipBtn != null)
                {
                    soIntro.FindProperty("skipButtonObj").objectReferenceValue = skipBtn.gameObject;
                }
                soIntro.ApplyModifiedPropertiesWithoutUndo();
            }

            // EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }

        public static void BuildMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.0f;
            cam.backgroundColor = new Color(0.12f, 0.22f, 0.16f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.AddComponent<AudioListener>();

            GameObject canvasObj = new GameObject("MainMenuCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Dynamic.asset");
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Outlined.mat");

            // Home Panel
            GameObject homePanel = new GameObject("HomePanel");
            homePanel.transform.SetParent(canvasObj.transform, false);
            RectTransform hrt = homePanel.AddComponent<RectTransform>();
            hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one; hrt.offsetMin = hrt.offsetMax = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(homePanel.transform, false);
            RectTransform trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.1f, 0.72f);
            trt.anchorMax = new Vector2(0.9f, 0.88f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            TextMeshProUGUI ttmp = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { ttmp.font = font; if (mat != null) ttmp.fontSharedMaterial = mat; }
            ttmp.alignment = TextAlignmentOptions.Center;
            ttmp.fontSize = 62;
            ttmp.text = "WORLD OF THE PACK";
            ttmp.color = new Color(1f, 0.92f, 0.35f);

            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(homePanel.transform, false);
            RectTransform srt = subObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.1f, 0.66f);
            srt.anchorMax = new Vector2(0.9f, 0.72f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            TextMeshProUGUI stmp = subObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { stmp.font = font; if (mat != null) stmp.fontSharedMaterial = mat; }
            stmp.alignment = TextAlignmentOptions.Center;
            stmp.fontSize = 28;
            stmp.text = "A new adventure for your pack";
            stmp.color = new Color(0.9f, 0.95f, 1f);

            // Home Buttons
            GameObject playBtn = CreateModalButton(homePanel.transform, "PlayButton", "PLAY CAMPAIGN", new Vector2(0.2f, 0.46f), new Vector2(0.8f, 0.56f), new Color(0.25f, 0.78f, 0.35f), font, mat);
            GameObject endlessBtn = CreateModalButton(homePanel.transform, "EndlessButton", "ENDLESS EXPEDITION", new Vector2(0.2f, 0.34f), new Vector2(0.8f, 0.44f), new Color(0.35f, 0.55f, 0.85f), font, mat);
            GameObject upgradesBtn = CreateModalButton(homePanel.transform, "UpgradesButton", "CAMP UPGRADES", new Vector2(0.2f, 0.22f), new Vector2(0.8f, 0.32f), new Color(0.75f, 0.52f, 0.18f), font, mat);
            GameObject optBtn = CreateModalButton(homePanel.transform, "OptionsButton", "SETTINGS", new Vector2(0.2f, 0.10f), new Vector2(0.8f, 0.20f), new Color(0.35f, 0.38f, 0.44f), font, mat);

            // Currency Label
            GameObject currencyObj = new GameObject("MoonShardsLabel");
            currencyObj.transform.SetParent(homePanel.transform, false);
            RectTransform crt = currencyObj.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.6f, 0.92f); crt.anchorMax = new Vector2(0.95f, 0.98f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            TextMeshProUGUI ctmp = currencyObj.AddComponent<TextMeshProUGUI>();
            if (font != null) { ctmp.font = font; if (mat != null) ctmp.fontSharedMaterial = mat; }
            ctmp.alignment = TextAlignmentOptions.Right;
            ctmp.fontSize = 28;
            ctmp.text = "150 SHARDS";
            ctmp.color = new Color(0.7f, 0.9f, 1f);

            // Mode Selection Panel
            GameObject modePanel = new GameObject("ModePanel");
            modePanel.transform.SetParent(canvasObj.transform, false);
            RectTransform mrt = modePanel.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero; mrt.anchorMax = Vector2.one; mrt.offsetMin = mrt.offsetMax = Vector2.zero;
            modePanel.SetActive(false);

            GameObject modeTitle = new GameObject("Title");
            modeTitle.transform.SetParent(modePanel.transform, false);
            RectTransform mtRt = modeTitle.AddComponent<RectTransform>();
            mtRt.anchorMin = new Vector2(0.1f, 0.82f); mtRt.anchorMax = new Vector2(0.9f, 0.94f); mtRt.offsetMin = mtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI mttmp = modeTitle.AddComponent<TextMeshProUGUI>();
            if (font != null) { mttmp.font = font; if (mat != null) mttmp.fontSharedMaterial = mat; }
            mttmp.alignment = TextAlignmentOptions.Center;
            mttmp.fontSize = 46;
            mttmp.text = "CHOOSE EXPEDITION";
            mttmp.color = new Color(1f, 0.92f, 0.35f);

            GameObject storyBtn = CreateModalButton(modePanel.transform, "CampaignStartButton", "STORY CAMPAIGN\n<size=22>Recover the Moon Fragments</size>", new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.68f), new Color(0.35f, 0.55f, 0.75f), font, mat);
            GameObject endlessStartBtn = CreateModalButton(modePanel.transform, "EndlessStartButton", "ENDLESS MARCH\n<size=22>March north into infinite lands</size>", new Vector2(0.15f, 0.32f), new Vector2(0.85f, 0.48f), new Color(0.65f, 0.45f, 0.25f), font, mat);
            GameObject backBtn = CreateModalButton(modePanel.transform, "BackButton", "BACK", new Vector2(0.3f, 0.16f), new Vector2(0.7f, 0.24f), new Color(0.28f, 0.32f, 0.38f), font, mat);

            // Upgrades Panel
            GameObject upgradesPanel = new GameObject("UpgradesPanel");
            upgradesPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform upRt = upgradesPanel.AddComponent<RectTransform>();
            upRt.anchorMin = Vector2.zero; upRt.anchorMax = Vector2.one; upRt.offsetMin = upRt.offsetMax = Vector2.zero;
            upgradesPanel.SetActive(false);

            GameObject upTitle = new GameObject("UpgradesTitle");
            upTitle.transform.SetParent(upgradesPanel.transform, false);
            RectTransform uptRt = upTitle.AddComponent<RectTransform>();
            uptRt.anchorMin = new Vector2(0.1f, 0.82f); uptRt.anchorMax = new Vector2(0.9f, 0.94f); uptRt.offsetMin = uptRt.offsetMax = Vector2.zero;
            TextMeshProUGUI uptTmp = upTitle.AddComponent<TextMeshProUGUI>();
            if (font != null) { uptTmp.font = font; if (mat != null) uptTmp.fontSharedMaterial = mat; }
            uptTmp.alignment = TextAlignmentOptions.Center;
            uptTmp.fontSize = 46;
            uptTmp.text = "CAMP UPGRADES";
            uptTmp.color = new Color(1f, 0.88f, 0.35f);

            GameObject upHealthBtn = CreateModalButton(upgradesPanel.transform, "UpgradeHealthButton", "PACK VITALITY (+25 Max HP) - 50 SHARDS", new Vector2(0.15f, 0.60f), new Vector2(0.85f, 0.72f), new Color(0.2f, 0.6f, 0.35f), font, mat);
            GameObject upEnergyBtn = CreateModalButton(upgradesPanel.transform, "UpgradeEnergyButton", "MOONLIGHT RECOVERY (+20% Regen) - 60 SHARDS", new Vector2(0.15f, 0.45f), new Vector2(0.85f, 0.57f), new Color(0.25f, 0.5f, 0.75f), font, mat);
            GameObject upMoraleBtn = CreateModalButton(upgradesPanel.transform, "UpgradeMoraleButton", "WOLF INSTINCT SURGE (+15% Power) - 75 SHARDS", new Vector2(0.15f, 0.30f), new Vector2(0.85f, 0.42f), new Color(0.75f, 0.45f, 0.15f), font, mat);
            GameObject upBackBtn = CreateModalButton(upgradesPanel.transform, "UpgradesBackButton", "BACK", new Vector2(0.3f, 0.14f), new Vector2(0.7f, 0.22f), new Color(0.28f, 0.32f, 0.38f), font, mat);

            // Options Panel
            GameObject optionsPanel = new GameObject("OptionsPanel");
            optionsPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform optRt = optionsPanel.AddComponent<RectTransform>();
            optRt.anchorMin = Vector2.zero; optRt.anchorMax = Vector2.one; optRt.offsetMin = optRt.offsetMax = Vector2.zero;
            optionsPanel.SetActive(false);

            GameObject optTitle = new GameObject("OptionsTitle");
            optTitle.transform.SetParent(optionsPanel.transform, false);
            RectTransform opttRt = optTitle.AddComponent<RectTransform>();
            opttRt.anchorMin = new Vector2(0.1f, 0.82f); opttRt.anchorMax = new Vector2(0.9f, 0.94f); opttRt.offsetMin = opttRt.offsetMax = Vector2.zero;
            TextMeshProUGUI opttTmp = optTitle.AddComponent<TextMeshProUGUI>();
            if (font != null) { opttTmp.font = font; if (mat != null) opttTmp.fontSharedMaterial = mat; }
            opttTmp.alignment = TextAlignmentOptions.Center;
            opttTmp.fontSize = 46;
            opttTmp.text = "SETTINGS";
            opttTmp.color = new Color(1f, 0.92f, 0.35f);

            GameObject replayIntroBtn = CreateModalButton(optionsPanel.transform, "ReplayIntroButton", "REPLAY OPENING CINEMATIC", new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.67f), new Color(0.35f, 0.45f, 0.65f), font, mat);
            GameObject resetSaveBtn = CreateModalButton(optionsPanel.transform, "ResetProgressButton", "RESET PROGRESS", new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.50f), new Color(0.65f, 0.25f, 0.25f), font, mat);
            GameObject optBackBtn = CreateModalButton(optionsPanel.transform, "OptionsBackButton", "BACK", new Vector2(0.3f, 0.16f), new Vector2(0.7f, 0.24f), new Color(0.28f, 0.32f, 0.38f), font, mat);

            // Controller Wiring
            MainMenuController mmc = canvasObj.AddComponent<MainMenuController>();
            SerializedObject soMmc = new SerializedObject(mmc);
            soMmc.FindProperty("homePanel").objectReferenceValue = homePanel;
            soMmc.FindProperty("modePanel").objectReferenceValue = modePanel;
            soMmc.FindProperty("upgradesPanel").objectReferenceValue = upgradesPanel;
            soMmc.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;

            soMmc.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
            soMmc.FindProperty("endlessButton").objectReferenceValue = endlessBtn.GetComponent<Button>();
            soMmc.FindProperty("upgradesButton").objectReferenceValue = upgradesBtn.GetComponent<Button>();
            soMmc.FindProperty("optionsButton").objectReferenceValue = optBtn.GetComponent<Button>();
            soMmc.FindProperty("moonShardsLabel").objectReferenceValue = ctmp;

            soMmc.FindProperty("campaignStartButton").objectReferenceValue = storyBtn.GetComponent<Button>();
            soMmc.FindProperty("endlessStartButton").objectReferenceValue = endlessStartBtn.GetComponent<Button>();

            soMmc.FindProperty("upgradeHealthButton").objectReferenceValue = upHealthBtn.GetComponent<Button>();
            soMmc.FindProperty("upgradeEnergyButton").objectReferenceValue = upEnergyBtn.GetComponent<Button>();
            soMmc.FindProperty("upgradeMoraleButton").objectReferenceValue = upMoraleBtn.GetComponent<Button>();
            soMmc.FindProperty("upgradesBackButton").objectReferenceValue = upBackBtn.GetComponent<Button>();

            soMmc.FindProperty("replayIntroButton").objectReferenceValue = replayIntroBtn.GetComponent<Button>();
            soMmc.FindProperty("resetProgressButton").objectReferenceValue = resetSaveBtn.GetComponent<Button>();
            soMmc.FindProperty("optionsBackButton").objectReferenceValue = optBackBtn.GetComponent<Button>();

            soMmc.ApplyModifiedPropertiesWithoutUndo();

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
