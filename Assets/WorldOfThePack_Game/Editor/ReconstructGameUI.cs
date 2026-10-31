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
                // 1. Find or create GameHUDCanvas
                GameObject hudCanvasGo = GameObject.Find("GameHUDCanvas");
                if (hudCanvasGo == null)
                {
                    hudCanvasGo = new GameObject("GameHUDCanvas");
                }
                hudCanvasGo.layer = LayerMask.NameToLayer("UI");

                // Destroy obsolete duplicate GameCardHandCanvas if present
                var oldHandCanvas = GameObject.Find("GameCardHandCanvas");
                if (oldHandCanvas != null)
                {
                    Object.DestroyImmediate(oldHandCanvas);
                }

                var canvas = hudCanvasGo.GetComponent<Canvas>();
                if (canvas == null) canvas = hudCanvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;
                canvas.sortingOrder = 1000;

            var scaler = hudCanvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = hudCanvasGo.AddComponent<CanvasScaler>();
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

            var raycaster = hudCanvasGo.GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = hudCanvasGo.AddComponent<GraphicRaycaster>();

            var hudController = hudCanvasGo.GetComponent<GameHUDController>();
            if (hudController == null) hudController = hudCanvasGo.AddComponent<GameHUDController>();

            // Clear old children of HUD Canvas to rebuild clean mockup layout
            var oldChildren = new List<GameObject>();
            foreach (Transform child in hudCanvasGo.transform)
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

            // Load Clean Common Sprites
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
            Sprite dotsSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyDots_Bar.png");
            Sprite moonBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/MoonshiftButton_Wood.png");
            Sprite cardFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Frames/CardFrame_HD.png");
            Sprite trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyTrack_HD.png");
            Sprite fillGreenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyFill_Green_HD.png");
            Sprite fillYellowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/EnergyFill_Yellow_HD.png");
            Sprite deployBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/DeployButton_Green.png");
            Sprite chargeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/ChargingButton_Navy.png");
            Sprite rerollBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Controls/RerollButton_Circle.png");

            // ==========================================
            // TOP HUD: WAVE, TITLE, HEARTS, BOSS, CAMP, PAUSE
            // ==========================================
            GameObject topHudRoot = CreateUIObject("TopHUDRoot", hudCanvasGo.transform);
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
            waveRt.anchoredPosition = new Vector2(10, -12);
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
            waveText.text = "WAVE 05";
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
            heartsRt.anchoredPosition = new Vector2(0, -68);
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
                // Mockup displays 4 full hearts and 1 empty heart
                hImg.sprite = (i < 4) ? heartFullSprite : heartEmptySprite;
                heartImages[i] = hImg;
            }

            // 4. TOP CENTER: BOSS HUD (BRAMBLE BRUTE)
            GameObject bossHud = CreateUIObject("BossHUDPanel", topHudRoot.transform);
            RectTransform bossRt = bossHud.GetComponent<RectTransform>();
            bossRt.anchorMin = new Vector2(0.5f, 1);
            bossRt.anchorMax = new Vector2(0.5f, 1);
            bossRt.pivot = new Vector2(0.5f, 1);
            bossRt.anchoredPosition = new Vector2(0, -96);
            bossRt.sizeDelta = new Vector2(196, 36);

            Image bossBg = bossHud.AddComponent<Image>();
            bossBg.sprite = bossBarSprite;
            bossHud.SetActive(false); // Initially hidden; dynamically appears during boss encounters

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
            pauseRt.anchoredPosition = new Vector2(-10, -10);
            pauseRt.sizeDelta = new Vector2(56, 54);

            Image pauseImg = pauseBtnGo.AddComponent<Image>();
            pauseImg.sprite = pauseBtnSprite;
            Button pauseBtn = pauseBtnGo.AddComponent<Button>();

            // 6. TOP RIGHT: NEXT CAMP PROGRESS PANEL
            GameObject campPanel = CreateUIObject("NextCampPanel", topHudRoot.transform);
            RectTransform campRt = campPanel.GetComponent<RectTransform>();
            campRt.anchorMin = new Vector2(1, 1);
            campRt.anchorMax = new Vector2(1, 1);
            campRt.pivot = new Vector2(1, 1);
            campRt.anchoredPosition = new Vector2(-8, -66);
            campRt.sizeDelta = new Vector2(172, 42);

            Image campImg = campPanel.AddComponent<Image>();
            campImg.sprite = campPanelSprite;

            // Next Camp Tent Icon
            GameObject tentGo = CreateUIObject("TentIcon", campPanel.transform);
            RectTransform tentRt = tentGo.GetComponent<RectTransform>();
            tentRt.anchorMin = new Vector2(0, 0.5f);
            tentRt.anchorMax = new Vector2(0, 0.5f);
            tentRt.pivot = new Vector2(0, 0.5f);
            tentRt.anchoredPosition = new Vector2(6, 0);
            tentRt.sizeDelta = new Vector2(30, 30);
            Image tentImg = tentGo.AddComponent<Image>();
            tentImg.sprite = tentIconSprite;

            // Next Camp Text
            GameObject campTextGo = CreateUIObject("CampText", campPanel.transform);
            RectTransform ctTxtRt = campTextGo.GetComponent<RectTransform>();
            ctTxtRt.anchorMin = new Vector2(0, 0.5f);
            ctTxtRt.anchorMax = new Vector2(1, 0.5f);
            ctTxtRt.pivot = new Vector2(0, 0.5f);
            ctTxtRt.anchoredPosition = new Vector2(18, 7);
            ctTxtRt.sizeDelta = new Vector2(-36, 16);
            TMP_Text campTxt = campTextGo.AddComponent<TextMeshProUGUI>();
            if (font != null) campTxt.font = font;
            campTxt.text = "NEXT CAMP • 260m";
            campTxt.fontSize = 13;
            campTxt.fontStyle = FontStyles.Bold;
            campTxt.alignment = TextAlignmentOptions.Center;
            campTxt.color = new Color(1f, 0.94f, 0.75f, 1f);
            SetOutlineSafely(campTxt, 0.2f, new Color32(25, 12, 6, 255));

            // ==========================================
            // LOWER CONTROLS: FORMATION, ENERGY DOTS, MOONSHIFT
            // ==========================================
            GameObject toolbarGo = CreateUIObject("LowerToolbar", hudCanvasGo.transform);
            RectTransform toolbarRt = toolbarGo.GetComponent<RectTransform>();
            toolbarRt.anchorMin = new Vector2(0, 0);
            toolbarRt.anchorMax = new Vector2(1, 0);
            toolbarRt.pivot = new Vector2(0.5f, 0);
            toolbarRt.anchoredPosition = new Vector2(0, 252);
            toolbarRt.sizeDelta = new Vector2(0, 52);

            // Formation Button
            GameObject formBtnGo = CreateUIObject("FormationButton", toolbarGo.transform);
            RectTransform formRt = formBtnGo.GetComponent<RectTransform>();
            formRt.anchorMin = new Vector2(0, 0.5f);
            formRt.anchorMax = new Vector2(0, 0.5f);
            formRt.pivot = new Vector2(0, 0.5f);
            formRt.anchoredPosition = new Vector2(10, 0);
            formRt.sizeDelta = new Vector2(165, 46);

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
            dotsRt.sizeDelta = new Vector2(130, 24);

            Image dotsImg = dotsGo.AddComponent<Image>();
            dotsImg.sprite = dotsSprite;

            // Moonshift Button
            GameObject moonBtnGo = CreateUIObject("MoonshiftButton", toolbarGo.transform);
            RectTransform moonRt = moonBtnGo.GetComponent<RectTransform>();
            moonRt.anchorMin = new Vector2(1, 0.5f);
            moonRt.anchorMax = new Vector2(1, 0.5f);
            moonRt.pivot = new Vector2(1, 0.5f);
            moonRt.anchoredPosition = new Vector2(-10, 0);
            moonRt.sizeDelta = new Vector2(172, 46);

            Image moonImg = moonBtnGo.AddComponent<Image>();
            moonImg.sprite = moonBtnSprite;
            Button moonBtn = moonBtnGo.AddComponent<Button>();

            // ==========================================
            // FOOTER: Defeats charge every card
            // ==========================================
            GameObject footerGo = CreateUIObject("FooterCaption", hudCanvasGo.transform);
            RectTransform footerRt = footerGo.GetComponent<RectTransform>();
            footerRt.anchorMin = new Vector2(0.5f, 0);
            footerRt.anchorMax = new Vector2(0.5f, 0);
            footerRt.pivot = new Vector2(0.5f, 0);
            footerRt.anchoredPosition = new Vector2(0, 6);
            footerRt.sizeDelta = new Vector2(400, 26);

            TMP_Text footerText = footerGo.AddComponent<TextMeshProUGUI>();
            if (font != null) footerText.font = font;
            footerText.text = "• Defeats charge every card •";
            footerText.fontSize = 15;
            footerText.alignment = TextAlignmentOptions.Center;
            footerText.color = new Color(0.96f, 0.88f, 0.72f, 0.95f);
            SetOutlineSafely(footerText, 0.2f, new Color32(30, 15, 8, 255));

            // ==========================================
            // 5-CARD HORIZONTAL HAND TRAY (3 Visible Prominently)
            // ==========================================
            GameObject handTrayGo = CreateUIObject("CardHandTray", hudCanvasGo.transform);
            RectTransform handRt = handTrayGo.GetComponent<RectTransform>();
            handRt.anchorMin = new Vector2(0, 0);
            handRt.anchorMax = new Vector2(1, 0);
            handRt.pivot = new Vector2(0.5f, 0);
            handRt.anchoredPosition = new Vector2(0, 32);
            handRt.sizeDelta = new Vector2(0, 214);

            // ScrollRect Setup
            ScrollRect scroll = handTrayGo.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;

            handTrayGo.AddComponent<RectMask2D>();

            ResponsiveHeroHand handComponent = handTrayGo.AddComponent<ResponsiveHeroHand>();
            handComponent.formationButton = formBtn;
            handComponent.transformationButton = moonBtn;
            handComponent.cardWidth = 174f;
            handComponent.cardHeight = 212f;
            handComponent.maximumCardWidth = 176f;
            handComponent.sidePadding = 12f;
            handComponent.gap = 10f;

            GameObject contentGo = CreateUIObject("Content", handTrayGo.transform);
            RectTransform contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 0);
            contentRt.anchorMax = new Vector2(0, 1);
            contentRt.pivot = new Vector2(0, 0.5f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(5 * 184 + 24, 0);
            scroll.content = contentRt;
            handComponent.scrollContent = contentRt;

            // 5 Hero Cards Data (Mockup: Wolvy, Elva, Lulu prominent; Bobo, Pip scrollable)
            var heroes = new (string id, string name, string roleBadge, string portraitPath, float energy, bool ready)[]
            {
                ("HeroWolvy", "WOLVY", "RoleBadge_Paw.png", "Assets/UI/WorldOfThePack/Art/Portraits/HeroWolvy_Human.png", 100f, true),
                ("PrincessElva", "ELVA", "RoleBadge_Leaf.png", "Assets/UI/WorldOfThePack/Art/Portraits/PrincessElva_Human.png", 60f, false),
                ("FairyLulu", "LULU", "RoleBadge_Moon.png", "Assets/UI/WorldOfThePack/Art/Portraits/FairyLulu_Human.png", 40f, false),
                ("CaptainBobo", "BOBO", "RoleBadge_Paw.png", "Assets/UI/WorldOfThePack/Art/Portraits/CaptainBobo_Human.png", 80f, false),
                ("ScoutPip", "PIP", "RoleBadge_Leaf.png", "Assets/UI/WorldOfThePack/Art/Portraits/ScoutPip_Human.png", 30f, false)
            };

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

                // Large Portrait
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

                // Action Button (DEPLOY or CHARGING)
                GameObject actBtnGo = CreateUIObject("ActionButton", cardGo.transform);
                RectTransform actRt = actBtnGo.GetComponent<RectTransform>();
                actRt.anchorMin = new Vector2(0.5f, 0.5f);
                actRt.anchorMax = new Vector2(0.5f, 0.5f);
                actRt.pivot = new Vector2(0.5f, 0.5f);
                actRt.anchoredPosition = new Vector2(-16, -78);
                actRt.sizeDelta = new Vector2(108, 34);

                Image actImg = actBtnGo.AddComponent<Image>();
                actImg.sprite = h.ready ? deployBtnSprite : chargeBtnSprite;
                Button actBtn = actBtnGo.AddComponent<Button>();

                // Reroll Button
                GameObject rerollGo = CreateUIObject("RerollButton", cardGo.transform);
                RectTransform rrRt = rerollGo.GetComponent<RectTransform>();
                rrRt.anchorMin = new Vector2(0.5f, 0.5f);
                rrRt.anchorMax = new Vector2(0.5f, 0.5f);
                rrRt.pivot = new Vector2(0.5f, 0.5f);
                rrRt.anchoredPosition = new Vector2(56, -78);
                rrRt.sizeDelta = new Vector2(34, 34);

                Image rrImg = rerollGo.AddComponent<Image>();
                rrImg.sprite = rerollBtnSprite;
                Button rrBtn = rerollGo.AddComponent<Button>();

                // Add HeroDeploymentCard component for drag & drop
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
                deploymentCard.definition = AssetDatabase.LoadAssetAtPath<HeroCardDefinition>("Assets/UI/WorldOfThePack/Data/" + h.id + ".asset");
                deploymentCard.SetEnergy(h.energy);

                var cpv = portraitGo.AddComponent<CardPortraitView>();
                cpv.portrait = portImg;
                cpv.definition = deploymentCard.definition;
                deploymentCard.portraitView = cpv;

                cardsArray[i] = deploymentCard;
            }

            handComponent.cards = cardsArray;

            // Wire GameHUDController properties via SerializedObject
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

            soHud.ApplyModifiedProperties();

            // Wire CardDeckManager in scene
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
            }

            // Save Scene
            EditorUtility.SetDirty(hudCanvasGo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("Reconstructed Complete Mockup UI successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Reconstruct Mockup UI Exception: " + ex);
            }
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
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
    }
}
