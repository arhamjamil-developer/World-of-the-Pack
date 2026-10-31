using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldOfThePack.Game;
using WorldOfThePack.UI;

namespace WorldOfThePack.EditorTools
{
    public static class ReconstructGameUI
    {
        [MenuItem("World of the Pack/UI/Reconstruct Mockup UI")]
        public static void Reconstruct()
        {
            try
            {
                // 1. Destroy old loose or obsolete duplicate canvases in scene
                var oldHandCanvas = GameObject.Find("GameCardHandCanvas");
                if (oldHandCanvas != null) Object.DestroyImmediate(oldHandCanvas);

                var oldHudCanvas = GameObject.Find("GameHUDCanvas");
                if (oldHudCanvas != null) Object.DestroyImmediate(oldHudCanvas);

                GameObject canvasGo = GameObject.Find("PortraitCardCanvas");
                if (canvasGo == null)
                {
                    canvasGo = new GameObject("PortraitCardCanvas");
                }
                canvasGo.layer = LayerMask.NameToLayer("UI");

                var canvas = canvasGo.GetComponent<Canvas>();
                if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;
                canvas.sortingOrder = 1000;

                var scaler = canvasGo.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(576, 1024);
                scaler.matchWidthOrHeight = 0.5f;

                SerializedObject soScaler = new SerializedObject(scaler);
                var refResProp = soScaler.FindProperty("m_ReferenceResolution");
                if (refResProp != null) refResProp.vector2Value = new Vector2(576, 1024);
                var matchProp = soScaler.FindProperty("m_MatchWidthOrHeight");
                if (matchProp != null) matchProp.floatValue = 0.5f;
                soScaler.ApplyModifiedProperties();
                EditorUtility.SetDirty(scaler);

                var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
                if (raycaster == null) raycaster = canvasGo.AddComponent<GraphicRaycaster>();

                var hudController = canvasGo.GetComponent<GameHUDController>();
                if (hudController == null) hudController = canvasGo.AddComponent<GameHUDController>();

                // Clear old children to rebuild pristine layout
                var oldChildren = new List<GameObject>();
                foreach (Transform child in canvasGo.transform)
                {
                    oldChildren.Add(child.gameObject);
                }
                foreach (var go in oldChildren)
                {
                    Object.DestroyImmediate(go);
                }

                // Load Font
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/LilitaOne-Dynamic.asset");
                if (font == null)
                {
                    font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                }

                // Load Common Sprites
                Sprite waveBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/WaveBadge_Clean.png");
                if (waveBadgeSprite == null) waveBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/WaveBadge_Panel.png");

                Sprite titleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Title_WorldOfThePack.png");

                Sprite heartsStripSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/PackHearts_Clean.png");
                if (heartsStripSprite == null) heartsStripSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/PackHearts_Strip.png");

                Sprite heartFullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Heart_Full.png");
                Sprite heartEmptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Heart_Empty.png");

                Sprite bossBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/BossBar_Clean.png");
                if (bossBarSprite == null) bossBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/BossBar_Full.png");

                Sprite acornIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Boss_AcornIcon.png");
                Sprite pauseBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/PauseButton_Round.png");

                Sprite campPanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/NextCamp_Clean.png");
                if (campPanelSprite == null) campPanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/NextCamp_Panel.png");

                Sprite tentIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Camp_TentIcon.png");
                Sprite formBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/FormationButton_Wood.png");
                Sprite woodCleanSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Button_Wood_Clean.png");
                Sprite greenCleanSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Button_Green_Clean.png");
                Sprite navyCleanSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/Button_Navy_Clean.png");
                Sprite dotsSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyDots_Bar.png");
                Sprite moonBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/MoonshiftButton_Wood.png");
                Sprite cardFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Frames/CardFrame_HD.png");
                Sprite trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyTrack_HD.png");
                Sprite fillGreenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyFill_Green_HD.png");
                Sprite fillYellowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyFill_Yellow_HD.png");
                Sprite deployBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/DeployButton_Green.png");
                Sprite chargeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/ChargingButton_Navy.png");

                // ==========================================
                // SAFE AREA CONTAINER
                // ==========================================
                GameObject safeAreaGo = CreateUIObject("SafeArea", canvasGo.transform);
                RectTransform safeRt = safeAreaGo.GetComponent<RectTransform>();
                safeRt.anchorMin = Vector2.zero;
                safeRt.anchorMax = Vector2.one;
                safeRt.sizeDelta = Vector2.zero;
                safeAreaGo.AddComponent<PortraitSafeArea>();

                // ==========================================
                // TOP HUD: WAVE, TITLE, HEARTS, BOSS, CAMP, PAUSE, ALLIES, DEFEATED
                // ==========================================
                GameObject topHudRoot = CreateUIObject("TopHUDRoot", safeAreaGo.transform);
                RectTransform topRt = topHudRoot.GetComponent<RectTransform>();
                topRt.anchorMin = new Vector2(0, 1);
                topRt.anchorMax = new Vector2(1, 1);
                topRt.pivot = new Vector2(0.5f, 1);
                topRt.anchoredPosition = Vector2.zero;
                topRt.sizeDelta = new Vector2(0, 150);

                // 1. TOP LEFT: WAVE BADGE
                GameObject waveBadge = CreateUIObject("WaveBadge", topHudRoot.transform);
                RectTransform waveRt = waveBadge.GetComponent<RectTransform>();
                waveRt.anchorMin = new Vector2(0, 1);
                waveRt.anchorMax = new Vector2(0, 1);
                waveRt.pivot = new Vector2(0, 1);
                waveRt.anchoredPosition = new Vector2(12, -12);
                waveRt.sizeDelta = new Vector2(135, 52);

                Image waveImg = waveBadge.AddComponent<Image>();
                waveImg.sprite = waveBadgeSprite;

                GameObject waveTextGo = CreateUIObject("WaveText", waveBadge.transform);
                RectTransform waveTextRt = waveTextGo.GetComponent<RectTransform>();
                waveTextRt.anchorMin = Vector2.zero;
                waveTextRt.anchorMax = Vector2.one;
                waveTextRt.sizeDelta = Vector2.zero;
                TMP_Text waveText = waveTextGo.AddComponent<TextMeshProUGUI>();
                if (font != null) waveText.font = font;
                waveText.text = "WAVE 01";
                waveText.fontSize = 22;
                waveText.fontStyle = FontStyles.Bold;
                waveText.alignment = TextAlignmentOptions.Center;
                waveText.color = new Color(1f, 0.94f, 0.65f, 1f);
                SetOutlineSafely(waveText, 0.25f, new Color32(35, 18, 10, 255));

                // 2. TOP CENTER: TITLE BANNER
                GameObject titleBanner = CreateUIObject("TitleBanner", topHudRoot.transform);
                RectTransform titleRt = titleBanner.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0.5f, 1);
                titleRt.anchorMax = new Vector2(0.5f, 1);
                titleRt.pivot = new Vector2(0.5f, 1);
                titleRt.anchoredPosition = new Vector2(0, -6);
                titleRt.sizeDelta = new Vector2(220, 56);

                Image titleImg = titleBanner.AddComponent<Image>();
                titleImg.sprite = titleSprite;

                // 3. TOP CENTER: PACK HEARTS (5 Hearts)
                GameObject heartsPanel = CreateUIObject("PackHeartsPanel", topHudRoot.transform);
                RectTransform heartsRt = heartsPanel.GetComponent<RectTransform>();
                heartsRt.anchorMin = new Vector2(0.5f, 1);
                heartsRt.anchorMax = new Vector2(0.5f, 1);
                heartsRt.pivot = new Vector2(0.5f, 1);
                heartsRt.anchoredPosition = new Vector2(0, -70);
                heartsRt.sizeDelta = new Vector2(148, 26);

                Image heartsBg = heartsPanel.AddComponent<Image>();
                heartsBg.sprite = heartsStripSprite;

                Image[] heartImages = new Image[5];
                for (int i = 0; i < 5; i++)
                {
                    GameObject heartGo = CreateUIObject($"Heart_{i}", heartsPanel.transform);
                    RectTransform hRt = heartGo.GetComponent<RectTransform>();
                    hRt.anchorMin = new Vector2(0.5f, 0.5f);
                    hRt.anchorMax = new Vector2(0.5f, 0.5f);
                    hRt.pivot = new Vector2(0.5f, 0.5f);
                    hRt.anchoredPosition = new Vector2(-46 + i * 23, -2);
                    hRt.sizeDelta = new Vector2(18, 16);

                    Image hImg = heartGo.AddComponent<Image>();
                    hImg.sprite = heartFullSprite;
                    heartImages[i] = hImg;
                }

                // ALLIES COUNTER (Mockup Left Counter, under Wave Badge)
                GameObject alliesGo = CreateUIObject("AlliesText", topHudRoot.transform);
                RectTransform alRt = alliesGo.GetComponent<RectTransform>();
                alRt.anchorMin = new Vector2(0, 1);
                alRt.anchorMax = new Vector2(0, 1);
                alRt.pivot = new Vector2(0, 1);
                alRt.anchoredPosition = new Vector2(14, -70);
                alRt.sizeDelta = new Vector2(120, 22);
                TMP_Text alliesTxt = alliesGo.AddComponent<TextMeshProUGUI>();
                if (font != null) alliesTxt.font = font;
                alliesTxt.text = "ALLIES 3";
                alliesTxt.fontSize = 14;
                alliesTxt.fontStyle = FontStyles.Bold;
                alliesTxt.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                SetOutlineSafely(alliesTxt, 0.2f, new Color32(25, 15, 10, 255));

                // DEFEATED COUNTER (Mockup Right Counter, under Pause Button)
                GameObject defeatedGo = CreateUIObject("DefeatedText", topHudRoot.transform);
                RectTransform defRt = defeatedGo.GetComponent<RectTransform>();
                defRt.anchorMin = new Vector2(1, 1);
                defRt.anchorMax = new Vector2(1, 1);
                defRt.pivot = new Vector2(1, 1);
                defRt.anchoredPosition = new Vector2(-14, -70);
                defRt.sizeDelta = new Vector2(120, 22);
                TMP_Text defTxt = defeatedGo.AddComponent<TextMeshProUGUI>();
                if (font != null) defTxt.font = font;
                defTxt.text = "DEFEATED 0";
                defTxt.fontSize = 14;
                defTxt.fontStyle = FontStyles.Bold;
                defTxt.alignment = TextAlignmentOptions.Right;
                defTxt.color = new Color(1f, 0.88f, 0.35f, 1f);
                SetOutlineSafely(defTxt, 0.2f, new Color32(25, 15, 10, 255));

                // 4. TOP CENTER: BOSS HUD (BRAMBLE BRUTE) - Inactive by default
                GameObject bossHud = CreateUIObject("BossHUDPanel", topHudRoot.transform);
                RectTransform bossRt = bossHud.GetComponent<RectTransform>();
                bossRt.anchorMin = new Vector2(0.5f, 1);
                bossRt.anchorMax = new Vector2(0.5f, 1);
                bossRt.pivot = new Vector2(0.5f, 1);
                bossRt.anchoredPosition = new Vector2(0, -98);
                bossRt.sizeDelta = new Vector2(196, 36);

                Image bossBg = bossHud.AddComponent<Image>();
                bossBg.sprite = bossBarSprite;
                bossHud.SetActive(false);

                // Acorn icon
                GameObject acornGo = CreateUIObject("AcornIcon", bossHud.transform);
                RectTransform acornRt = acornGo.GetComponent<RectTransform>();
                acornRt.anchorMin = new Vector2(0, 0.5f);
                acornRt.anchorMax = new Vector2(0, 0.5f);
                acornRt.pivot = new Vector2(0, 0.5f);
                acornRt.anchoredPosition = new Vector2(2, 0);
                acornRt.sizeDelta = new Vector2(32, 34);
                Image acornImg = acornGo.AddComponent<Image>();
                acornImg.sprite = acornIconSprite;

                // Boss Name Text
                GameObject bossNameGo = CreateUIObject("BossNameText", bossHud.transform);
                RectTransform bnRt = bossNameGo.GetComponent<RectTransform>();
                bnRt.anchorMin = new Vector2(0.5f, 0.5f);
                bnRt.anchorMax = new Vector2(0.5f, 0.5f);
                bnRt.pivot = new Vector2(0.5f, 0.5f);
                bnRt.anchoredPosition = new Vector2(16, 6);
                bnRt.sizeDelta = new Vector2(140, 16);
                TMP_Text bossNameTxt = bossNameGo.AddComponent<TextMeshProUGUI>();
                if (font != null) bossNameTxt.font = font;
                bossNameTxt.text = "BRAMBLE BRUTE";
                bossNameTxt.fontSize = 13;
                bossNameTxt.fontStyle = FontStyles.Bold;
                bossNameTxt.alignment = TextAlignmentOptions.Center;
                bossNameTxt.color = new Color(1f, 0.95f, 0.85f, 1f);
                SetOutlineSafely(bossNameTxt, 0.25f, new Color32(25, 12, 6, 255));

                // Boss Health Fill
                GameObject bossFillGo = CreateUIObject("BossHealthFill", bossHud.transform);
                RectTransform bfRt = bossFillGo.GetComponent<RectTransform>();
                bfRt.anchorMin = new Vector2(0.5f, 0);
                bfRt.anchorMax = new Vector2(0.5f, 0);
                bfRt.pivot = new Vector2(0.5f, 0);
                bfRt.anchoredPosition = new Vector2(16, 5);
                bfRt.sizeDelta = new Vector2(144, 9);
                Image bossFillImg = bossFillGo.AddComponent<Image>();
                bossFillImg.type = Image.Type.Filled;
                bossFillImg.fillMethod = Image.FillMethod.Horizontal;
                bossFillImg.fillAmount = 1.0f;
                bossFillImg.color = new Color(0.95f, 0.25f, 0.15f, 1f);

                // 5. TOP RIGHT: PAUSE BUTTON
                GameObject pauseBtnGo = CreateUIObject("PauseButton", topHudRoot.transform);
                RectTransform pauseRt = pauseBtnGo.GetComponent<RectTransform>();
                pauseRt.anchorMin = new Vector2(1, 1);
                pauseRt.anchorMax = new Vector2(1, 1);
                pauseRt.pivot = new Vector2(1, 1);
                pauseRt.anchoredPosition = new Vector2(-12, -12);
                pauseRt.sizeDelta = new Vector2(54, 52);

                Image pauseImg = pauseBtnGo.AddComponent<Image>();
                pauseImg.sprite = pauseBtnSprite;
                Button pauseBtn = pauseBtnGo.AddComponent<Button>();

                // 6. TOP RIGHT: NEXT CAMP PROGRESS PANEL (Cleanly below Defeated counter, no overlap!)
                GameObject campPanel = CreateUIObject("NextCampPanel", topHudRoot.transform);
                RectTransform campRt = campPanel.GetComponent<RectTransform>();
                campRt.anchorMin = new Vector2(1, 1);
                campRt.anchorMax = new Vector2(1, 1);
                campRt.pivot = new Vector2(1, 1);
                campRt.anchoredPosition = new Vector2(-12, -98);
                campRt.sizeDelta = new Vector2(156, 34);

                Image campImg = campPanel.AddComponent<Image>();
                campImg.sprite = campPanelSprite;

                // Next Camp Tent Icon
                GameObject tentGo = CreateUIObject("TentIcon", campPanel.transform);
                RectTransform tentRt = tentGo.GetComponent<RectTransform>();
                tentRt.anchorMin = new Vector2(0, 0.5f);
                tentRt.anchorMax = new Vector2(0, 0.5f);
                tentRt.pivot = new Vector2(0, 0.5f);
                tentRt.anchoredPosition = new Vector2(6, 0);
                tentRt.sizeDelta = new Vector2(26, 26);
                Image tentImg = tentGo.AddComponent<Image>();
                tentImg.sprite = tentIconSprite;

                // Next Camp Text
                GameObject campTextGo = CreateUIObject("CampText", campPanel.transform);
                RectTransform ctTxtRt = campTextGo.GetComponent<RectTransform>();
                ctTxtRt.anchorMin = new Vector2(0, 0.5f);
                ctTxtRt.anchorMax = new Vector2(1, 0.5f);
                ctTxtRt.pivot = new Vector2(0, 0.5f);
                ctTxtRt.anchoredPosition = new Vector2(16, 0);
                ctTxtRt.sizeDelta = new Vector2(-32, 16);
                TMP_Text campTxt = campTextGo.AddComponent<TextMeshProUGUI>();
                if (font != null) campTxt.font = font;
                campTxt.text = "NEXT CAMP • 260m";
                campTxt.fontSize = 12;
                campTxt.fontStyle = FontStyles.Bold;
                campTxt.alignment = TextAlignmentOptions.Center;
                campTxt.color = new Color(1f, 0.94f, 0.75f, 1f);
                SetOutlineSafely(campTxt, 0.2f, new Color32(25, 12, 6, 255));

                // ==========================================
                // DIALOGUE & STORY NOTIFICATION BANNER (Top-Middle)
                // ==========================================
                GameObject bannerGo = CreateUIObject("BannerPanel", safeAreaGo.transform);
                RectTransform bannerRt = bannerGo.GetComponent<RectTransform>();
                bannerRt.anchorMin = new Vector2(0.5f, 1);
                bannerRt.anchorMax = new Vector2(0.5f, 1);
                bannerRt.pivot = new Vector2(0.5f, 1);
                bannerRt.anchoredPosition = new Vector2(0, -145);
                bannerRt.sizeDelta = new Vector2(480, 52);

                Image bannerBg = bannerGo.AddComponent<Image>();
                bannerBg.color = new Color(0.08f, 0.12f, 0.18f, 0.92f);

                GameObject bannerTextGo = CreateUIObject("BannerText", bannerGo.transform);
                RectTransform btRt = bannerTextGo.GetComponent<RectTransform>();
                btRt.anchorMin = Vector2.zero;
                btRt.anchorMax = Vector2.one;
                btRt.sizeDelta = new Vector2(-24, -10);
                TMP_Text bannerTxt = bannerTextGo.AddComponent<TextMeshProUGUI>();
                if (font != null) bannerTxt.font = font;
                bannerTxt.text = "Wolvy: \"Pack, follow my lead!\"";
                bannerTxt.fontSize = 15;
                bannerTxt.fontStyle = FontStyles.Bold;
                bannerTxt.alignment = TextAlignmentOptions.Center;
                bannerTxt.color = new Color(1f, 0.92f, 0.55f, 1f);
                SetOutlineSafely(bannerTxt, 0.2f, new Color32(20, 10, 5, 255));
                bannerGo.SetActive(false);

                // ==========================================
                // HERO CARD HAND CONTAINER (Inside SafeArea, Y = 0, Height = 325)
                // ==========================================
                GameObject heroCardHandGo = CreateUIObject("HeroCardHand", safeAreaGo.transform);
                RectTransform handRootRt = heroCardHandGo.GetComponent<RectTransform>();
                handRootRt.anchorMin = new Vector2(0, 0);
                handRootRt.anchorMax = new Vector2(1, 0);
                handRootRt.pivot = new Vector2(0.5f, 0);
                handRootRt.anchoredPosition = Vector2.zero;
                handRootRt.sizeDelta = new Vector2(0, 325);

                // Lower Toolbar: Formation, Energy Dots, Moonshift (Y = 274, Height = 48)
                GameObject toolbarGo = CreateUIObject("Toolbar", heroCardHandGo.transform);
                RectTransform toolbarRt = toolbarGo.GetComponent<RectTransform>();
                toolbarRt.anchorMin = new Vector2(0, 0);
                toolbarRt.anchorMax = new Vector2(1, 0);
                toolbarRt.pivot = new Vector2(0.5f, 0);
                toolbarRt.anchoredPosition = new Vector2(0, 274);
                toolbarRt.sizeDelta = new Vector2(0, 48);

                // Formation Button
                GameObject formBtnGo = CreateUIObject("FormationTag", toolbarGo.transform);
                RectTransform formRt = formBtnGo.GetComponent<RectTransform>();
                formRt.anchorMin = new Vector2(0, 0.5f);
                formRt.anchorMax = new Vector2(0, 0.5f);
                formRt.pivot = new Vector2(0, 0.5f);
                formRt.anchoredPosition = new Vector2(12, 0);
                formRt.sizeDelta = new Vector2(156, 44);

                Image formImg = formBtnGo.AddComponent<Image>();
                formImg.sprite = formBtnSprite;
                Button formBtn = formBtnGo.AddComponent<Button>();

                // Deck Energy Progress Dots
                GameObject dotsGo = CreateUIObject("EnergyDots", toolbarGo.transform);
                RectTransform dotsRt = dotsGo.GetComponent<RectTransform>();
                dotsRt.anchorMin = new Vector2(0.5f, 0.5f);
                dotsRt.anchorMax = new Vector2(0.5f, 0.5f);
                dotsRt.pivot = new Vector2(0.5f, 0.5f);
                dotsRt.anchoredPosition = Vector2.zero;
                dotsRt.sizeDelta = new Vector2(120, 22);

                Image dotsImg = dotsGo.AddComponent<Image>();
                dotsImg.sprite = dotsSprite;

                // Moonshift Button
                GameObject moonBtnGo = CreateUIObject("TransformationButton", toolbarGo.transform);
                RectTransform moonRt = moonBtnGo.GetComponent<RectTransform>();
                moonRt.anchorMin = new Vector2(1, 0.5f);
                moonRt.anchorMax = new Vector2(1, 0.5f);
                moonRt.pivot = new Vector2(1, 0.5f);
                moonRt.anchoredPosition = new Vector2(-12, 0);
                moonRt.sizeDelta = new Vector2(162, 44);

                Image moonImg = moonBtnGo.AddComponent<Image>();
                moonImg.sprite = moonBtnSprite;
                Button moonBtn = moonBtnGo.AddComponent<Button>();

                // Ribbon Banner: Defeat soldiers to charge cards (Y = 246, Height = 24)
                GameObject ribbonGo = CreateUIObject("RibbonBanner", heroCardHandGo.transform);
                RectTransform ribRt = ribbonGo.GetComponent<RectTransform>();
                ribRt.anchorMin = new Vector2(0.5f, 0);
                ribRt.anchorMax = new Vector2(0.5f, 0);
                ribRt.pivot = new Vector2(0.5f, 0);
                ribRt.anchoredPosition = new Vector2(0, 246);
                ribRt.sizeDelta = new Vector2(360, 24);

                Image ribImg = ribbonGo.AddComponent<Image>();
                ribImg.color = new Color(0.12f, 0.22f, 0.26f, 0.92f);

                GameObject ribTextGo = CreateUIObject("RibbonText", ribbonGo.transform);
                RectTransform rtTxt = ribTextGo.GetComponent<RectTransform>();
                rtTxt.anchorMin = Vector2.zero;
                rtTxt.anchorMax = Vector2.one;
                rtTxt.sizeDelta = Vector2.zero;
                TMP_Text ribTxt = ribTextGo.AddComponent<TextMeshProUGUI>();
                if (font != null) ribTxt.font = font;
                ribTxt.text = "• Defeat soldiers to charge cards •";
                ribTxt.fontSize = 12;
                ribTxt.fontStyle = FontStyles.Bold;
                ribTxt.alignment = TextAlignmentOptions.Center;
                ribTxt.color = new Color(1f, 0.92f, 0.55f, 1f);
                SetOutlineSafely(ribTxt, 0.2f, new Color32(20, 10, 5, 255));

                // ==========================================
                // 7-CARD SWIPE TRAY (ScrollRect with all 7 Heroes!)
                // ==========================================
                GameObject cardScrollView = CreateUIObject("CardScrollView", heroCardHandGo.transform);
                RectTransform scrollRt = cardScrollView.GetComponent<RectTransform>();
                scrollRt.anchorMin = new Vector2(0, 0);
                scrollRt.anchorMax = new Vector2(1, 0);
                scrollRt.pivot = new Vector2(0.5f, 0);
                scrollRt.anchoredPosition = new Vector2(0, 28);
                scrollRt.sizeDelta = new Vector2(0, 214);

                ScrollRect scroll = cardScrollView.AddComponent<ScrollRect>();
                scroll.horizontal = true;
                scroll.vertical = false;
                scroll.movementType = ScrollRect.MovementType.Elastic;
                scroll.elasticity = 0.1f;
                scroll.inertia = true;
                scroll.decelerationRate = 0.135f;
                scroll.scrollSensitivity = 1.0f;

                cardScrollView.AddComponent<RectMask2D>();

                ResponsiveHeroHand handComponent = heroCardHandGo.AddComponent<ResponsiveHeroHand>();
                handComponent.formationButton = formBtn;
                handComponent.transformationButton = moonBtn;
                handComponent.cardWidth = 174f;
                handComponent.cardHeight = 212f;
                handComponent.maximumCardWidth = 176f;
                handComponent.sidePadding = 12f;
                handComponent.gap = 10f;

                // 7 Heroes Data (All 7 playable characters in canonical roster)
                var heroes = new (string id, string name, string roleBadge, string portraitPath, float energy, bool ready)[]
                {
                    ("HeroWolvy", "WOLVY", "RoleBadge_Paw.png", "Assets/UI/WorldOfThePack/Art/Portraits/HeroWolvy_Human.png", 100f, true),
                    ("PrincessElva", "ELVA", "RoleBadge_Leaf.png", "Assets/UI/WorldOfThePack/Art/Portraits/PrincessElva_Human.png", 60f, false),
                    ("FairyLulu", "LULU", "RoleBadge_Moon.png", "Assets/UI/WorldOfThePack/Art/Portraits/FairyLulu_Human.png", 40f, false),
                    ("CaptainBobo", "BOBO", "RoleBadge_Paw.png", "Assets/UI/WorldOfThePack/Art/Portraits/CaptainBobo_Human.png", 80f, false),
                    ("ScoutPip", "PIP", "RoleBadge_Leaf.png", "Assets/UI/WorldOfThePack/Art/Portraits/ScoutPip_Human.png", 30f, false),
                    ("WizardMomo", "MOMO", "RoleBadge_Moon.png", "Assets/UI/WorldOfThePack/Art/Portraits/WizardMomo_Human.png", 20f, false),
                    ("TinkerTilly", "TILLY", "RoleBadge_Leaf.png", "Assets/UI/WorldOfThePack/Art/Portraits/TinkerTilly_Human.png", 20f, false)
                };

                float contentTotalWidth = 12f * 2f + heroes.Length * 174f + (heroes.Length - 1) * 10f;
                GameObject contentGo = CreateUIObject("Content", cardScrollView.transform);
                RectTransform contentRt = contentGo.GetComponent<RectTransform>();
                contentRt.anchorMin = new Vector2(0, 0);
                contentRt.anchorMax = new Vector2(0, 1);
                contentRt.pivot = new Vector2(0, 0.5f);
                contentRt.anchoredPosition = Vector2.zero;
                contentRt.sizeDelta = new Vector2(contentTotalWidth, 0);

                scroll.content = contentRt;
                handComponent.scrollContent = contentRt;

                HeroDeploymentCard[] cardsArray = new HeroDeploymentCard[heroes.Length];

                for (int i = 0; i < heroes.Length; i++)
                {
                    var h = heroes[i];
                    GameObject cardGo = CreateUIObject($"CardSlot_{i}_{h.id}", contentGo.transform);
                    RectTransform crt = cardGo.GetComponent<RectTransform>();
                    crt.anchorMin = new Vector2(0, 0.5f);
                    crt.anchorMax = new Vector2(0, 0.5f);
                    crt.pivot = new Vector2(0.5f, 0.5f);
                    crt.anchoredPosition = new Vector2(12 + 87 + i * 184, 0);
                    crt.sizeDelta = new Vector2(174, 212);

                    // Golden Card Frame
                    Image frameImg = cardGo.AddComponent<Image>();
                    frameImg.sprite = cardFrameSprite;

                    // Large Character Portrait
                    GameObject portraitGo = CreateUIObject("Portrait", cardGo.transform);
                    RectTransform portRt = portraitGo.GetComponent<RectTransform>();
                    portRt.anchorMin = new Vector2(0.5f, 0.5f);
                    portRt.anchorMax = new Vector2(0.5f, 0.5f);
                    portRt.pivot = new Vector2(0.5f, 0.5f);
                    portRt.anchoredPosition = new Vector2(0, 36);
                    portRt.sizeDelta = new Vector2(144, 112);
                    Image portImg = portraitGo.AddComponent<Image>();
                    portImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(h.portraitPath);

                    // Role Badge on Top Left
                    GameObject roleGo = CreateUIObject("RoleBadge", cardGo.transform);
                    RectTransform roleRt = roleGo.GetComponent<RectTransform>();
                    roleRt.anchorMin = new Vector2(0.5f, 0.5f);
                    roleRt.anchorMax = new Vector2(0.5f, 0.5f);
                    roleRt.pivot = new Vector2(0.5f, 0.5f);
                    roleRt.anchoredPosition = new Vector2(-60, 78);
                    roleRt.sizeDelta = new Vector2(36, 36);
                    Image roleImg = roleGo.AddComponent<Image>();
                    roleImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/" + h.roleBadge);

                    // Nameplate
                    GameObject nameGo = CreateUIObject("Nameplate", cardGo.transform);
                    RectTransform nameRt = nameGo.GetComponent<RectTransform>();
                    nameRt.anchorMin = new Vector2(0.5f, 0.5f);
                    nameRt.anchorMax = new Vector2(0.5f, 0.5f);
                    nameRt.pivot = new Vector2(0.5f, 0.5f);
                    nameRt.anchoredPosition = new Vector2(0, -24);
                    nameRt.sizeDelta = new Vector2(144, 22);
                    TMP_Text nameTxt = nameGo.AddComponent<TextMeshProUGUI>();
                    if (font != null) nameTxt.font = font;
                    nameTxt.text = h.name;
                    nameTxt.fontSize = 17;
                    nameTxt.fontStyle = FontStyles.Bold;
                    nameTxt.alignment = TextAlignmentOptions.Center;
                    nameTxt.color = new Color32(35, 18, 10, 255);

                    // Charge Bar Track & Fill
                    GameObject chargeTrack = CreateUIObject("ChargeBarTrack", cardGo.transform);
                    RectTransform ctRt = chargeTrack.GetComponent<RectTransform>();
                    ctRt.anchorMin = new Vector2(0.5f, 0.5f);
                    ctRt.anchorMax = new Vector2(0.5f, 0.5f);
                    ctRt.pivot = new Vector2(0.5f, 0.5f);
                    ctRt.anchoredPosition = new Vector2(0, -44);
                    ctRt.sizeDelta = new Vector2(148, 16);
                    Image ctImg = chargeTrack.AddComponent<Image>();
                    ctImg.sprite = trackSprite;

                    GameObject chargeFill = CreateUIObject("ChargeBarFill", chargeTrack.transform);
                    RectTransform cfRt = chargeFill.GetComponent<RectTransform>();
                    cfRt.anchorMin = new Vector2(0, 0);
                    cfRt.anchorMax = new Vector2(1, 1);
                    cfRt.pivot = new Vector2(0, 0.5f);
                    cfRt.sizeDelta = Vector2.zero;
                    Image cfImg = chargeFill.AddComponent<Image>();
                    cfImg.type = Image.Type.Filled;
                    cfImg.fillMethod = Image.FillMethod.Horizontal;
                    cfImg.fillAmount = h.energy / 100f;
                    cfImg.sprite = h.ready ? fillGreenSprite : fillYellowSprite;

                    // Energy Label
                    GameObject energyLabelGo = CreateUIObject("EnergyLabel", chargeTrack.transform);
                    RectTransform elRt = energyLabelGo.GetComponent<RectTransform>();
                    elRt.anchorMin = Vector2.zero;
                    elRt.anchorMax = Vector2.one;
                    elRt.sizeDelta = Vector2.zero;
                    TMP_Text elTxt = energyLabelGo.AddComponent<TextMeshProUGUI>();
                    if (font != null) elTxt.font = font;
                    elTxt.text = $"{(int)h.energy} / 100";
                    elTxt.fontSize = 12;
                    elTxt.fontStyle = FontStyles.Bold;
                    elTxt.alignment = TextAlignmentOptions.Center;
                    elTxt.color = Color.white;
                    SetOutlineSafely(elTxt, 0.25f, new Color32(20, 10, 5, 255));

                    // Action Button (Centered wide: DEPLOY or CHARGING)
                    GameObject actBtnGo = CreateUIObject("ActionButton", cardGo.transform);
                    RectTransform actRt = actBtnGo.GetComponent<RectTransform>();
                    actRt.anchorMin = new Vector2(0.5f, 0.5f);
                    actRt.anchorMax = new Vector2(0.5f, 0.5f);
                    actRt.pivot = new Vector2(0.5f, 0.5f);
                    actRt.anchoredPosition = new Vector2(0, -78);
                    actRt.sizeDelta = new Vector2(148, 36);

                    Image actImg = actBtnGo.AddComponent<Image>();
                    actImg.sprite = h.ready ? deployBtnSprite : chargeBtnSprite;
                    Button actBtn = actBtnGo.AddComponent<Button>();

                    // Text Label on Button (Empty when ready because sprite has DEPLOY baked in!)
                    GameObject actTxtGo = CreateUIObject("ButtonLabel", actBtnGo.transform);
                    RectTransform actTxtRt = actTxtGo.GetComponent<RectTransform>();
                    actTxtRt.anchorMin = Vector2.zero;
                    actTxtRt.anchorMax = Vector2.one;
                    actTxtRt.sizeDelta = Vector2.zero;
                    TMP_Text actTxt = actTxtGo.AddComponent<TextMeshProUGUI>();
                    if (font != null) actTxt.font = font;
                    actTxt.text = h.ready ? "" : $"CHARGING {(int)h.energy}%";
                    actTxt.fontSize = 13;
                    actTxt.fontStyle = FontStyles.Bold;
                    actTxt.alignment = TextAlignmentOptions.Center;
                    actTxt.color = h.ready ? new Color32(25, 25, 25, 255) : new Color(0.9f, 0.95f, 1f, 1f);
                    SetOutlineSafely(actTxt, 0.15f, new Color32(10, 10, 10, 200));

                    // Add HeroDeploymentCard component
                    var deploymentCard = cardGo.AddComponent<HeroDeploymentCard>();
                    deploymentCard.deployButton = actBtn;
                    deploymentCard.actionArtwork = actImg;
                    deploymentCard.readyArtwork = deployBtnSprite;
                    deploymentCard.chargingArtwork = chargeBtnSprite;
                    deploymentCard.readyFillSprite = fillGreenSprite;
                    deploymentCard.chargingFillSprite = fillYellowSprite;
                    deploymentCard.energyFill = cfImg;
                    deploymentCard.nameLabel = nameTxt;
                    deploymentCard.energyLabel = elTxt;
                    deploymentCard.stateLabel = actTxt;
                    deploymentCard.definition = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>("Assets/UI/WorldOfThePack/Data/" + h.id + ".asset");
                    deploymentCard.SetEnergy(h.energy);

                    var cpv = portraitGo.AddComponent<CardPortraitView>();
                    cpv.portrait = portImg;
                    cpv.definition = deploymentCard.definition;
                    deploymentCard.portraitView = cpv;

                    cardsArray[i] = deploymentCard;
                }

                handComponent.cards = cardsArray;

                // Footer Caption: Full energy -> Spawn -> Recharge -> Spawn again (Y = 6, Height = 20)
                GameObject footerGo = CreateUIObject("FooterCaption", heroCardHandGo.transform);
                RectTransform footerRt = footerGo.GetComponent<RectTransform>();
                footerRt.anchorMin = new Vector2(0.5f, 0);
                footerRt.anchorMax = new Vector2(0.5f, 0);
                footerRt.pivot = new Vector2(0.5f, 0);
                footerRt.anchoredPosition = new Vector2(0, 6);
                footerRt.sizeDelta = new Vector2(440, 20);

                TMP_Text footerText = footerGo.AddComponent<TextMeshProUGUI>();
                if (font != null) footerText.font = font;
                footerText.text = "• Full energy → Spawn → Recharge → Spawn again •";
                footerText.fontSize = 12;
                footerText.alignment = TextAlignmentOptions.Center;
                footerText.color = new Color(0.96f, 0.88f, 0.72f, 0.95f);
                SetOutlineSafely(footerText, 0.2f, new Color32(30, 15, 8, 255));

                // ==========================================
                // MODALS (Pause, GameOver, Victory, Camp)
                // ==========================================
                // 1. Pause Modal
                var pauseModal = CreateModalOverlay("PauseModal", safeAreaGo.transform);
                var pauseBox = CreateDialogBox("DialogBox", pauseModal.transform, new Vector2(360, 320));
                CreateDialogHeader("Title", pauseBox.transform, "EXPEDITION PAUSED", font);

                Button resumeBtn = CreateStyledButton("ResumeButton", pauseBox.transform, "CONTINUE", new Vector2(0, 40), new Vector2(240, 44), greenCleanSprite != null ? greenCleanSprite : formBtnSprite, font);
                Button optionsBtn = CreateStyledButton("OptionsButton", pauseBox.transform, "SETTINGS", new Vector2(0, -15), new Vector2(240, 44), woodCleanSprite != null ? woodCleanSprite : formBtnSprite, font);
                Button mainMenuBtn = CreateStyledButton("MainMenuButton", pauseBox.transform, "RESTART RUN", new Vector2(0, -70), new Vector2(240, 44), navyCleanSprite != null ? navyCleanSprite : formBtnSprite, font);

                GameObject optSubPanel = CreateUIObject("OptionsSubPanel", pauseBox.transform);
                RectTransform optRt = optSubPanel.GetComponent<RectTransform>();
                optRt.anchorMin = new Vector2(0.5f, 0.5f);
                optRt.anchorMax = new Vector2(0.5f, 0.5f);
                optRt.pivot = new Vector2(0.5f, 0.5f);
                optRt.anchoredPosition = new Vector2(0, -20);
                optRt.sizeDelta = new Vector2(280, 160);
                Image optBg = optSubPanel.AddComponent<Image>();
                optBg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

                Toggle musicTog = CreateStyledToggle("MusicToggle", optSubPanel.transform, "MUSIC", new Vector2(0, 40), font);
                Toggle soundTog = CreateStyledToggle("SoundToggle", optSubPanel.transform, "SFX", new Vector2(0, 0), font);
                Toggle vibTog = CreateStyledToggle("VibrationToggle", optSubPanel.transform, "HAPTICS", new Vector2(0, -40), font);
                optSubPanel.SetActive(false);
                pauseModal.SetActive(false);

                // 2. GameOver Modal
                var gameOverModal = CreateModalOverlay("GameOverModal", safeAreaGo.transform);
                var gameOverBox = CreateDialogBox("DialogBox", gameOverModal.transform, new Vector2(360, 280));
                CreateDialogHeader("Title", gameOverBox.transform, "EXPEDITION FAILED", font, new Color(1f, 0.35f, 0.35f, 1f));

                GameObject statsGo = CreateUIObject("StatsText", gameOverBox.transform);
                RectTransform statsRt = statsGo.GetComponent<RectTransform>();
                statsRt.anchorMin = new Vector2(0.5f, 0.5f);
                statsRt.anchorMax = new Vector2(0.5f, 0.5f);
                statsRt.pivot = new Vector2(0.5f, 0.5f);
                statsRt.anchoredPosition = new Vector2(0, 20);
                statsRt.sizeDelta = new Vector2(300, 50);
                TMP_Text statsTxt = statsGo.AddComponent<TextMeshProUGUI>();
                if (font != null) statsTxt.font = font;
                statsTxt.text = "The sacred grove was overrun.\nRegroup and try again!";
                statsTxt.fontSize = 14;
                statsTxt.alignment = TextAlignmentOptions.Center;
                statsTxt.color = new Color(0.9f, 0.9f, 0.9f, 1f);

                Button retryBtn = CreateStyledButton("RetryButton", gameOverBox.transform, "RETRY EXPEDITION", new Vector2(0, -55), new Vector2(250, 46), greenCleanSprite != null ? greenCleanSprite : deployBtnSprite, font);
                gameOverModal.SetActive(false);

                // 3. Victory Modal
                var victoryModal = CreateModalOverlay("VictoryModal", safeAreaGo.transform);
                var victoryBox = CreateDialogBox("DialogBox", victoryModal.transform, new Vector2(360, 280));
                CreateDialogHeader("Title", victoryBox.transform, "GROVE LIBERATED!", font, new Color(1f, 0.88f, 0.35f, 1f));

                GameObject vicMsgGo = CreateUIObject("MessageText", victoryBox.transform);
                RectTransform vmRt = vicMsgGo.GetComponent<RectTransform>();
                vmRt.anchorMin = new Vector2(0.5f, 0.5f);
                vmRt.anchorMax = new Vector2(0.5f, 0.5f);
                vmRt.pivot = new Vector2(0.5f, 0.5f);
                vmRt.anchoredPosition = new Vector2(0, 20);
                vmRt.sizeDelta = new Vector2(300, 50);
                TMP_Text vmTxt = vicMsgGo.AddComponent<TextMeshProUGUI>();
                if (font != null) vmTxt.font = font;
                vmTxt.text = "The Moon Fragment shines bright!\nThe pack endures!";
                vmTxt.fontSize = 14;
                vmTxt.alignment = TextAlignmentOptions.Center;
                vmTxt.color = new Color(0.9f, 0.95f, 0.9f, 1f);

                Button vicContinueBtn = CreateStyledButton("ContinueButton", victoryBox.transform, "CONTINUE EXPEDITION", new Vector2(0, -55), new Vector2(250, 46), greenCleanSprite != null ? greenCleanSprite : deployBtnSprite, font);
                victoryModal.SetActive(false);

                // 4. Camp Modal
                var campModal = CreateModalOverlay("CampModal", safeAreaGo.transform);
                var campBox = CreateDialogBox("DialogBox", campModal.transform, new Vector2(360, 320));
                CreateDialogHeader("Title", campBox.transform, "SANCTUARY CAMP", font, new Color(1f, 0.92f, 0.65f, 1f));

                Button campRestBtn = CreateStyledButton("RestButton", campBox.transform, "REST & RECOVER (+50 HP)", new Vector2(0, 40), new Vector2(280, 42), greenCleanSprite != null ? greenCleanSprite : formBtnSprite, font);
                Button campBuffBtn = CreateStyledButton("BuffButton", campBox.transform, "HOWL OF VALOR (+25% ATK)", new Vector2(0, -15), new Vector2(280, 42), woodCleanSprite != null ? woodCleanSprite : formBtnSprite, font);
                Button campForestBtn = CreateStyledButton("ForestRouteButton", campBox.transform, "DEEP FOREST PATHWAY", new Vector2(0, -70), new Vector2(280, 42), navyCleanSprite != null ? navyCleanSprite : formBtnSprite, font);
                campModal.SetActive(false);

                // ==========================================
                // WIRE GameHUDController
                // ==========================================
                SerializedObject soHud = new SerializedObject(hudController);

                var waveProp = soHud.FindProperty("waveLabel");
                if (waveProp != null) waveProp.objectReferenceValue = waveText;

                var heartsProp = soHud.FindProperty("packHearts");
                if (heartsProp != null)
                {
                    heartsProp.arraySize = 5;
                    for (int i = 0; i < 5; i++)
                    {
                        heartsProp.GetArrayElementAtIndex(i).objectReferenceValue = heartImages[i];
                    }
                }

                var fullProp = soHud.FindProperty("heartFullSprite");
                if (fullProp != null) fullProp.objectReferenceValue = heartFullSprite;
                var emptyProp = soHud.FindProperty("heartEmptySprite");
                if (emptyProp != null) emptyProp.objectReferenceValue = heartEmptySprite;

                var bossPanelProp = soHud.FindProperty("bossHealthBarPanel");
                if (bossPanelProp != null) bossPanelProp.objectReferenceValue = bossHud;
                var bossNameProp = soHud.FindProperty("bossNameLabel");
                if (bossNameProp != null) bossNameProp.objectReferenceValue = bossNameTxt;
                var bossFillProp = soHud.FindProperty("bossHealthFill");
                if (bossFillProp != null) bossFillProp.objectReferenceValue = bossFillImg;

                var pauseProp = soHud.FindProperty("pauseButton");
                if (pauseProp != null) pauseProp.objectReferenceValue = pauseBtn;

                var formProp = soHud.FindProperty("formationButton");
                if (formProp != null) formProp.objectReferenceValue = formBtn;
                var moonProp = soHud.FindProperty("moonshiftButton");
                if (moonProp != null) moonProp.objectReferenceValue = moonBtn;

                var campDistProp = soHud.FindProperty("nextCampDistanceText");
                if (campDistProp != null) campDistProp.objectReferenceValue = campTxt;

                var packCountProp = soHud.FindProperty("packCountLabel");
                if (packCountProp != null) packCountProp.objectReferenceValue = alliesTxt;

                var scoreProp = soHud.FindProperty("scoreLabel");
                if (scoreProp != null) scoreProp.objectReferenceValue = defTxt;

                // Wire Banner
                var bannerPanelProp = soHud.FindProperty("bannerPanel");
                if (bannerPanelProp != null) bannerPanelProp.objectReferenceValue = bannerGo;
                var bannerTextProp = soHud.FindProperty("bannerText");
                if (bannerTextProp != null) bannerTextProp.objectReferenceValue = bannerTxt;

                // Wire Pause Modal
                var pauseModalProp = soHud.FindProperty("pauseModal");
                if (pauseModalProp != null) pauseModalProp.objectReferenceValue = pauseModal;
                var resumeProp = soHud.FindProperty("resumeButton");
                if (resumeProp != null) resumeProp.objectReferenceValue = resumeBtn;
                var optBtnProp = soHud.FindProperty("optionsButton");
                if (optBtnProp != null) optBtnProp.objectReferenceValue = optionsBtn;
                var menuBtnProp = soHud.FindProperty("mainMenuButton");
                if (menuBtnProp != null) menuBtnProp.objectReferenceValue = mainMenuBtn;
                var optPanelProp = soHud.FindProperty("optionsSubPanel");
                if (optPanelProp != null) optPanelProp.objectReferenceValue = optSubPanel;
                var musTogProp = soHud.FindProperty("musicToggle");
                if (musTogProp != null) musTogProp.objectReferenceValue = musicTog;
                var sndTogProp = soHud.FindProperty("soundToggle");
                if (sndTogProp != null) sndTogProp.objectReferenceValue = soundTog;
                var vibTogProp = soHud.FindProperty("vibrationToggle");
                if (vibTogProp != null) vibTogProp.objectReferenceValue = vibTog;

                // Wire GameOver Modal
                var goModalProp = soHud.FindProperty("gameOverModal");
                if (goModalProp != null) goModalProp.objectReferenceValue = gameOverModal;
                var goStatsProp = soHud.FindProperty("gameOverStatsText");
                if (goStatsProp != null) goStatsProp.objectReferenceValue = statsTxt;
                var retryBtnProp = soHud.FindProperty("retryButton");
                if (retryBtnProp != null) retryBtnProp.objectReferenceValue = retryBtn;

                // Wire Victory Modal
                var vicModalProp = soHud.FindProperty("victoryModal");
                if (vicModalProp != null) vicModalProp.objectReferenceValue = victoryModal;
                var vicBtnProp = soHud.FindProperty("victoryContinueButton");
                if (vicBtnProp != null) vicBtnProp.objectReferenceValue = vicContinueBtn;

                // Wire Camp Modal
                var campModalProp = soHud.FindProperty("campModal");
                if (campModalProp != null) campModalProp.objectReferenceValue = campModal;
                var campRestProp = soHud.FindProperty("campRestButton");
                if (campRestProp != null) campRestProp.objectReferenceValue = campRestBtn;
                var campBuffProp = soHud.FindProperty("campBuffButton");
                if (campBuffProp != null) campBuffProp.objectReferenceValue = campBuffBtn;
                var campForestProp = soHud.FindProperty("campRouteForestButton");
                if (campForestProp != null) campForestProp.objectReferenceValue = campForestBtn;

                soHud.ApplyModifiedProperties();

                // ==========================================
                // SAVE AS PREFAB: Assets/UI/WorldOfThePack/Prefabs/PortraitCardCanvas.prefab
                // AND CONNECT IN SCENE AS ACTIVE PREFAB INSTANCE!
                // ==========================================
                string prefabPath = "Assets/UI/WorldOfThePack/Prefabs/PortraitCardCanvas.prefab";
                GameObject connectedInstance = PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGo, prefabPath, InteractionMode.AutomatedAction);
                if (connectedInstance != null)
                {
                    canvasGo = connectedInstance;
                    handComponent = canvasGo.GetComponentInChildren<ResponsiveHeroHand>();
                    cardsArray = canvasGo.GetComponentsInChildren<HeroDeploymentCard>();
                }

                // ==========================================
                // WIRE CardDeckManager in Scene with all 7 Heroes
                // ==========================================
                var deckManager = Object.FindFirstObjectByType<CardDeckManager>();
                if (deckManager != null)
                {
                    SerializedObject soDeck = new SerializedObject(deckManager);
                    var handProp = soDeck.FindProperty("heroHand");
                    if (handProp != null) handProp.objectReferenceValue = handComponent;

                    var slotsProp = soDeck.FindProperty("cardSlots");
                    if (slotsProp != null)
                    {
                        slotsProp.arraySize = cardsArray.Length;
                        for (int i = 0; i < cardsArray.Length; i++)
                        {
                            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = cardsArray[i];
                        }
                    }

                    var defsProp = soDeck.FindProperty("heroDefinitions");
                    if (defsProp != null)
                    {
                        string[] orderedHeroes = { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" };
                        defsProp.arraySize = orderedHeroes.Length;
                        for (int i = 0; i < orderedHeroes.Length; i++)
                        {
                            defsProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>("Assets/UI/WorldOfThePack/Data/" + orderedHeroes[i] + ".asset");
                        }
                    }

                    soDeck.ApplyModifiedProperties();
                    EditorUtility.SetDirty(deckManager);
                }

                // ==========================================
                // SERIALIZE CINEMACHINE IN SCENE
                // ==========================================
                SetupCinemachineInScene();

                // Save Scene
                EditorUtility.SetDirty(canvasGo);
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
                AssetDatabase.SaveAssets();

                Debug.Log($"Successfully updated {prefabPath} and aligned SampleScene with all 7 heroes swipe!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Reconstruct Mockup UI Exception: " + ex);
            }
        }

        public static void SetupCinemachineInScene()
        {
            var mainCam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 0f, -10f);
                var brain = mainCam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
                if (brain == null) brain = mainCam.gameObject.AddComponent<Unity.Cinemachine.CinemachineBrain>();

                var expCam = mainCam.GetComponent<ExpeditionCamera>();
                if (expCam != null) expCam.enabled = false;
                EditorUtility.SetDirty(mainCam.gameObject);
            }

            var vcamGo = GameObject.Find("Cinemachine_ExpeditionCam");
            if (vcamGo == null) vcamGo = new GameObject("Cinemachine_ExpeditionCam");
            vcamGo.transform.position = new Vector3(0f, 0f, -10f);
            var vcam = vcamGo.GetComponent<Unity.Cinemachine.CinemachineCamera>();
            if (vcam == null) vcam = vcamGo.AddComponent<Unity.Cinemachine.CinemachineCamera>();

            var follow = vcamGo.GetComponent<Unity.Cinemachine.CinemachineFollow>();
            if (follow == null) follow = vcamGo.AddComponent<Unity.Cinemachine.CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 0f, -10f);

            var targetGo = GameObject.Find("Cinemachine_TrackingTarget");
            if (targetGo == null) targetGo = new GameObject("Cinemachine_TrackingTarget");

            vcam.Follow = targetGo.transform;
            var lens = vcam.Lens;
            lens.ModeOverride = Unity.Cinemachine.LensSettings.OverrideModes.Orthographic;
            lens.OrthographicSize = 6.2f;
            vcam.Lens = lens;

            EditorUtility.SetDirty(vcamGo);
            EditorUtility.SetDirty(targetGo);
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static GameObject CreateModalOverlay(string name, Transform parent)
        {
            GameObject overlay = CreateUIObject(name, parent);
            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = overlay.AddComponent<Image>();
            img.color = new Color(0.04f, 0.06f, 0.1f, 0.82f);
            return overlay;
        }

        private static GameObject CreateDialogBox(string name, Transform parent, Vector2 size)
        {
            GameObject box = CreateUIObject(name, parent);
            RectTransform rt = box.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            Image img = box.AddComponent<Image>();
            img.color = new Color(0.10f, 0.13f, 0.18f, 0.98f);

            var outline = box.AddComponent<Outline>();
            outline.effectColor = new Color(0.82f, 0.68f, 0.35f, 1f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            return box;
        }

        private static void CreateDialogHeader(string name, Transform parent, string title, TMP_FontAsset font, Color? titleColor = null)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -18);
            rt.sizeDelta = new Vector2(320, 36);

            TMP_Text txt = go.AddComponent<TextMeshProUGUI>();
            if (font != null) txt.font = font;
            txt.text = title;
            txt.fontSize = 20;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = titleColor ?? new Color(1f, 0.94f, 0.72f, 1f);
            SetOutlineSafely(txt, 0.25f, new Color32(20, 10, 5, 255));
        }

        private static Button CreateStyledButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, Sprite sprite, TMP_FontAsset font)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                if (sprite.border != Vector4.zero)
                {
                    img.type = Image.Type.Sliced;
                }
            }
            else
            {
                img.color = new Color(0.2f, 0.45f, 0.35f, 1f);
            }

            Button btn = go.AddComponent<Button>();

            GameObject textGo = CreateUIObject("Label", go.transform);
            RectTransform txtRt = textGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            TMP_Text txt = textGo.AddComponent<TextMeshProUGUI>();
            if (font != null) txt.font = font;
            txt.text = label;
            txt.fontSize = 14;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            SetOutlineSafely(txt, 0.2f, new Color32(20, 10, 5, 255));

            return btn;
        }

        private static Toggle CreateStyledToggle(string name, Transform parent, string label, Vector2 pos, TMP_FontAsset font)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(240, 32);

            Toggle toggle = go.AddComponent<Toggle>();
            toggle.isOn = true;

            GameObject bgGo = CreateUIObject("Background", go.transform);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.5f);
            bgRt.anchorMax = new Vector2(0, 0.5f);
            bgRt.pivot = new Vector2(0, 0.5f);
            bgRt.anchoredPosition = new Vector2(10, 0);
            bgRt.sizeDelta = new Vector2(24, 24);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.25f, 0.3f, 1f);

            GameObject checkGo = CreateUIObject("Checkmark", bgGo.transform);
            RectTransform chkRt = checkGo.GetComponent<RectTransform>();
            chkRt.anchorMin = new Vector2(0.5f, 0.5f);
            chkRt.anchorMax = new Vector2(0.5f, 0.5f);
            chkRt.pivot = new Vector2(0.5f, 0.5f);
            chkRt.anchoredPosition = Vector2.zero;
            chkRt.sizeDelta = new Vector2(16, 16);
            Image chkImg = checkGo.AddComponent<Image>();
            chkImg.color = new Color(0.2f, 0.9f, 0.4f, 1f);

            toggle.targetGraphic = bgImg;
            toggle.graphic = chkImg;

            GameObject lblGo = CreateUIObject("Label", go.transform);
            RectTransform lblRt = lblGo.GetComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0, 0);
            lblRt.anchorMax = new Vector2(1, 1);
            lblRt.pivot = new Vector2(0, 0.5f);
            lblRt.anchoredPosition = new Vector2(45, 0);
            lblRt.sizeDelta = new Vector2(-50, 0);
            TMP_Text lblTxt = lblGo.AddComponent<TextMeshProUGUI>();
            if (font != null) lblTxt.font = font;
            lblTxt.text = label;
            lblTxt.fontSize = 14;
            lblTxt.alignment = TextAlignmentOptions.Left;
            lblTxt.color = Color.white;

            return toggle;
        }

        private static void SetOutlineSafely(TMP_Text text, float width, Color32 color)
        {
            if (text == null) return;
            try
            {
                text.outlineColor = color;
                text.outlineWidth = width;
            }
            catch {}
        }

        [MenuItem("World of the Pack/Test/Choose Camp Option 0 (Rest)")]
        public static void TestChooseCamp0()
        {
            if (CampManager.Instance != null)
            {
                CampManager.Instance.ChooseCampOption(0);
                Debug.Log("[Test] Camp Option 0 (Rest & Recover) selected.");
            }
        }

        [MenuItem("World of the Pack/Test/Choose Camp Option 1 (Buff)")]
        public static void TestChooseCamp1()
        {
            if (CampManager.Instance != null)
            {
                CampManager.Instance.ChooseCampOption(1);
                Debug.Log("[Test] Camp Option 1 (Howl of Valor) selected.");
            }
        }
    }
}
