using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldOfThePack.Game;
using WorldOfThePack.NewUI;
using WorldOfThePack.CombatArt;

namespace WorldOfThePack.Expedition.Editor
{
    public static class ExpeditionPolishBuilder
    {
        private const string Root = "Assets/New/ExpeditionPolish/";
        private static TMP_FontAsset _font;
        private static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/New/Art/" + name + ".png");
        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(.5f, .5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size; return rt;
        }
        private static Image Image(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Sprite sprite, Color color)
        {
            var image = Rect(name, parent, anchor, pos, size).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.color = color; image.raycastTarget = false; return image;
        }
        private static TMP_Text Text(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, string text, float fontSize)
        {
            var label = Rect(name, parent, anchor, pos, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = _font; label.fontSize = fontSize; label.text = text;
            label.color = new Color(1, .94f, .79f); label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Center; return label;
        }
        private static void ImportIcon(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing generated art " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 512; importer.SaveAndReimport();
        }
        private static Sprite[] ImportSlash()
        {
            string path = Root + "Art/MoonSlash.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects();
            var rects = new SpriteRect[8];
            for (int i = 0; i < 8; i++)
            {
                string name = "MoonSlash_" + i;
                var old = previous.FirstOrDefault(r => r.name == name);
                rects[i] = new SpriteRect { name = name, rect = new Rect((i % 4) * width / 4f, (1 - i / 4) * height / 2f, width / 4f, height / 2f),
                    pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center, spriteID = old != null ? old.spriteID : GUID.Generate() };
            }
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        }
        [MenuItem("World of the Pack/Polish/Apply Expedition Polish")]
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/SampleScene.unity") throw new InvalidOperationException("Open SampleScene first.");
            AssetDatabase.Refresh();
            ImportIcon(Root + "Art/MoonCharm.png");
            var charm = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/MoonCharm.png");
            var slash = ImportSlash();
            BuildBoss(slash);
            BuildCleanEffects();
            var terrainShader = Shader.Find("WorldOfThePack/SeamBlendTerrain");
            var terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Data/TerrainSeamBlend.mat");
            if (terrainMaterial == null) { terrainMaterial = new Material(terrainShader); AssetDatabase.CreateAsset(terrainMaterial, Root + "Data/TerrainSeamBlend.mat"); }
            terrainMaterial.shader = terrainShader;
            var world = UnityEngine.Object.FindFirstObjectByType<WorldBiomeGenerator>();
            if (world != null) { var worldSo = new SerializedObject(world); worldSo.FindProperty("terrainMaterial").objectReferenceValue = terrainMaterial; worldSo.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(world); }
            string[] heroes = { "HeroWolvy", "CaptainBobo", "PrincessElva", "FairyLulu", "ScoutPip", "WizardMomo", "TinkerTilly" };
            foreach (string hero in heroes)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/New/Combat/Art/" + hero + "_Attack_North.png");
                // 362px attack cells must occupy the same world footprint as the 256px locomotion cells.
                importer.spritePixelsPerUnit = 181; importer.SaveAndReimport();
                PolishPrefab("Assets/WorldOfThePack_Game/Prefabs/Units/" + hero + "_PackUnit.prefab", slash, hero);
            }
            foreach (string enemy in new[] { "Soldier", "Rudra", "Elsa" })
                PolishPrefab("Assets/New/Enemy/Prefabs/Enemy_" + enemy + ".prefab", slash, null);
            foreach (string enemy in new[] { "Hound", "Captain" })
                PolishPrefab("Assets/WorldOfThePack_Game/Prefabs/Units/Enemy_" + enemy + ".prefab", slash, null);
            PolishPrefab("Assets/WorldOfThePack_Game/Prefabs/Units/Boss_AncientThornGuardian.prefab", slash, null);
            foreach (string guid in AssetDatabase.FindAssets("t:AttackPresentationProfile", new[] { "Assets/New/Combat/Data" }))
            {
                var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if (profile.projectile != null) profile.projectileWidth = profile.attackName.Contains("Gear") ? .25f : .18f;
                EditorUtility.SetDirty(profile);
            }
            var hud = UnityEngine.Object.FindFirstObjectByType<PackGameplayHUD>();
            if (hud == null) throw new InvalidOperationException("Gameplay HUD not found.");
            var waveManager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                var waveSo = new SerializedObject(waveManager);
                waveSo.FindProperty("houndPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Soldier.prefab");
                waveSo.FindProperty("captainPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Rudra.prefab");
                waveSo.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(waveManager);
            }
            _font = hud.waveLabel.font;
            var safe = hud.transform.Find("SafeArea");
            var prior = safe.Find("ExpeditionPolishUI"); if (prior != null) UnityEngine.Object.DestroyImmediate(prior.gameObject);
            var ui = Rect("ExpeditionPolishUI", safe, Vector2.one * .5f, Vector2.zero, Vector2.zero);
            ui.anchorMin = Vector2.zero; ui.anchorMax = Vector2.one; ui.sizeDelta = Vector2.zero;
            // Non-interactive task board below the top HUD, leaving the combat corridor clear.
            Image("TaskPlate", ui, new Vector2(.5f, 1), new Vector2(0, -270), new Vector2(860, 112), Art("CampPanel"), new Color(.48f, .65f, .62f, .95f));
            var title = Text("TaskTitle", ui, new Vector2(.5f, 1), new Vector2(0, -247), new Vector2(795, 35), "PACK TASK", 29);
            hud.noticeLabel = Text("ExpeditionNotice", ui, new Vector2(.5f, 1), new Vector2(0, -355), new Vector2(840, 45), "", 24);
            hud.noticeLabel.enableAutoSizing = true; hud.noticeLabel.fontSizeMin = 18;
            hud.noticeLabel.gameObject.SetActive(false);
            var detail = Text("TaskDetail", ui, new Vector2(.5f, 1), new Vector2(0, -284), new Vector2(790, 36), "Protect the leader", 24);
            var progress = Image("TaskProgress", ui, new Vector2(.5f, 1), new Vector2(0, -312), new Vector2(730, 5), Art("EnergyFill"), new Color(.48f, .95f, .8f));
            progress.type = UnityEngine.UI.Image.Type.Filled; progress.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            var tasks = hud.GetComponent<PackTaskDirector>() ?? hud.gameObject.AddComponent<PackTaskDirector>();
            hud.packStrikeButton = Image("PackStrike", ui, new Vector2(.5f, 1), new Vector2(0, -415), new Vector2(630, 65), Art("Button_Navy_Clean"), Color.white).gameObject.AddComponent<Button>();
            hud.packStrikeButton.image.raycastTarget = true;
            hud.packStrikeLabel = Text("PackStrikeLabel", hud.packStrikeButton.transform, Vector2.one * .5f, Vector2.zero, new Vector2(600, 55), "PACK STRIKE", 27);
            hud.packStrikeButton.gameObject.SetActive(false);
            tasks.titleLabel = title; tasks.detailLabel = detail; tasks.progressFill = progress;
            var catalog = AssetDatabase.LoadAssetAtPath<PackTaskCatalog>(Root + "Data/PackTasks.asset");
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PackTaskCatalog>();
                System.IO.Directory.CreateDirectory(Root + "Data"); AssetDatabase.CreateAsset(catalog, Root + "Data/PackTasks.asset");
                catalog.tasks = new[] {
                    new PackTask { title="CLEAR THE TRAIL", hint="Defeat scouts • Reward: 3 charms", kind=PackTaskKind.Defeat, target=3, reward=3 },
                    new PackTask { title="GROW THE PACK", hint="Deploy a charged card • Reward: 3 charms", kind=PackTaskKind.Deploy, target=1, reward=3 },
                    new PackTask { title="CALL THE MOON", hint="Use Moonshift at 50 moon power • Reward: 4 charms", kind=PackTaskKind.Moonshift, target=1, reward=4 },
                    new PackTask { title="HOLD TOGETHER", hint="Keep the queen safe for 25s • Reward: 3 charms", kind=PackTaskKind.Travel, target=25, reward=3 },
                    new PackTask { title="FIND A REFUGE", hint="Reach the next camp • Reward: 5 charms", kind=PackTaskKind.Camp, target=1, reward=5 },
                    new PackTask { title="BREAK THE PATROL", hint="Defeat five enemies • Reward: 4 charms", kind=PackTaskKind.Defeat, target=5, reward=4 }
                }; EditorUtility.SetDirty(catalog);
            }
            tasks.catalog = catalog;
            var resultParent = hud.endPanel.transform.Find("Panel");
            var oldResult = resultParent.Find("RunSummary"); if (oldResult != null) UnityEngine.Object.DestroyImmediate(oldResult.gameObject);
            tasks.resultLabel = Text("RunSummary", resultParent, new Vector2(.5f, .5f), new Vector2(0, -50), new Vector2(540, 290), "", 28);
            tasks.resultLabel.alignment = TextAlignmentOptions.Center;
            var icon = safe.Find("TopHUD/BerryIcon")?.GetComponent<Image>(); if (icon != null) { icon.sprite = charm; icon.preserveAspect = true; }
            var hint = safe.Find("CardHand/Hint_TMP")?.GetComponent<TMP_Text>();
            if (hint != null) { hint.text = "DEFEAT → CHARGE + CHARMS → DEPLOY"; hint.fontSize = 23; }
            foreach (var card in hud.cards)
            {
                var humanPortrait = Art(card.Definition.heroId + "_Human");
                if (humanPortrait != null) { card.Definition.humanPortrait = humanPortrait; EditorUtility.SetDirty(card.Definition); }
                var oldPrice = card.transform.Find("CharmPrice");
                if (oldPrice != null) UnityEngine.Object.DestroyImmediate(oldPrice.gameObject);
                var price = Rect("CharmPrice", card.transform, new Vector2(.79f, .86f), Vector2.zero, new Vector2(104, 43));
                var backing = price.gameObject.AddComponent<Image>(); backing.color = new Color(.035f, .12f, .15f, .96f); backing.raycastTarget = false;
                Image("CharmIcon", price, new Vector2(.25f, .5f), Vector2.zero, new Vector2(38, 38), charm, Color.white).preserveAspect = true;
                card.charmCostLabel = Text("Price", price, new Vector2(.7f, .5f), Vector2.zero, new Vector2(45, 40), "4", 30);
                EditorUtility.SetDirty(card);
                card.energyLabel.fontSize = 20;
                card.energyLabel.enableAutoSizing = true; card.energyLabel.fontSizeMin = 16;
                var copy = card.GetComponent<PackCardCopy>(); if (copy != null) copy.charging = "CHARGE / CHARMS";
            }
            var atmosphere = hud.GetComponent<MoonshiftAtmosphere>() ?? hud.gameObject.AddComponent<MoonshiftAtmosphere>();
            atmosphere.world = UnityEngine.Object.FindFirstObjectByType<WorldBiomeGenerator>();
            var dot = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            atmosphere.moonGlow = Image("MoonHalo", ui, new Vector2(.5f, .78f), Vector2.zero, new Vector2(180, 180), dot, Color.clear);
            atmosphere.moonGlow.gameObject.SetActive(false);
            atmosphere.fireflies = new Image[16];
            for (int i = 0; i < 16; i++)
            {
                float side = i % 2 == 0 ? .22f : .78f;
                atmosphere.fireflies[i] = Image("Firefly_" + i, ui, new Vector2(side, .34f + (i / 2) * .055f), Vector2.zero, new Vector2(7, 7), dot, Color.clear);
            }
            var device = hud.GetComponent<PackDeviceLayout>() ?? hud.gameObject.AddComponent<PackDeviceLayout>();
            device.scaler = hud.GetComponent<CanvasScaler>(); device.canvasRect = hud.GetComponent<RectTransform>();
            device.leftHelper = device.rightHelper = null;
            device.journeyLabel = device.packLabel = null;
            var oldScenery = hud.transform.Find("TabletScenery");
            if (oldScenery != null) UnityEngine.Object.DestroyImmediate(oldScenery.gameObject);
            var scenery = Rect("TabletScenery", hud.transform, Vector2.one * .5f, Vector2.zero, Vector2.zero);
            scenery.anchorMin = Vector2.zero; scenery.anchorMax = Vector2.one; scenery.offsetMin = scenery.offsetMax = Vector2.zero;
            scenery.SetAsFirstSibling();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/TabletWoodland.png");
            device.leftEnvironment = Rect("CreekWoodland", scenery, Vector2.zero, Vector2.zero, Vector2.zero).gameObject.AddComponent<RawImage>();
            device.rightEnvironment = Rect("LanternWoodland", scenery, Vector2.zero, Vector2.zero, Vector2.zero).gameObject.AddComponent<RawImage>();
            foreach (var image in new[] { device.leftEnvironment, device.rightEnvironment }) { image.texture = texture; image.raycastTarget = false; }
            TrailMechanicsBuilder.Build();
            RectangularCardHolderBuilder.Build();
            EditorUtility.SetDirty(hud); EditorUtility.SetDirty(tasks); EditorUtility.SetDirty(atmosphere); EditorUtility.SetDirty(device);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Expedition polish authored: charm wallet, priced cards, task catalog, motion, slash, Moonshift atmosphere and tablet helpers.";
        }
        private static RectTransform Helper(string name, Transform parent, float x)
        {
            var rt = Rect(name, parent, new Vector2(x, .54f), Vector2.zero, new Vector2(420, 980));
            var bg = rt.gameObject.AddComponent<Image>(); bg.sprite = Art("HandTray"); bg.color = new Color(.52f, .65f, .63f); bg.raycastTarget = false; return rt;
        }
        private static void BuildBoss(Sprite[] slash)
        {
            string path = Root + "Art/BrambleGuardian.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("New guardian sheet missing");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 190; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects();
            string[] actions = { "Idle", "Walk", "Attack", "Death" };
            var rects = new SpriteRect[16];
            for (int i = 0; i < 16; i++)
            {
                string name = "Guardian_" + actions[i / 4] + "_" + (i % 4);
                var old = previous.FirstOrDefault(r => r.name == name);
                rects[i] = new SpriteRect { name = name, rect = new Rect((i % 4) * w / 4f, (3 - i / 4) * h / 4f, w / 4f, h / 4f),
                    pivot = new Vector2(.5f, .16f), alignment = SpriteAlignment.Custom, spriteID = old != null ? old.spriteID : GUID.Generate() };
            }
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            System.IO.Directory.CreateDirectory(Root + "Animations"); System.IO.Directory.CreateDirectory(Root + "Data");
            string controllerPath = Root + "Animations/BrambleGuardian.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) { controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath); controller.AddParameter("Action", AnimatorControllerParameterType.Int); }
            var machine = controller.layers[0].stateMachine;
            for (int row = 0; row < 4; row++)
            {
                var frames = sprites.Where(s => s.name.StartsWith("Guardian_" + actions[row] + "_")).OrderBy(s => s.name).ToArray();
                string clipPath = Root + "Animations/Guardian_" + actions[row] + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
                clip.frameRate = 8;
                var keys = new ObjectReferenceKeyframe[5];
                for (int i = 0; i < 5; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 8f, value = frames[Mathf.Min(i, 3)] };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Body", typeof(SpriteRenderer), "m_Sprite"), keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = row < 2; AnimationUtility.SetAnimationClipSettings(clip, settings);
                var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == actions[row]);
                if (state == null)
                {
                    state = machine.AddState(actions[row]);
                    var transition = machine.AddAnyStateTransition(state); transition.hasExitTime = false; transition.duration = .035f;
                    transition.canTransitionToSelf = false; transition.AddCondition(AnimatorConditionMode.Equals, row, "Action");
                }
                state.motion = clip; if (row == 0) machine.defaultState = state;
                EditorUtility.SetDirty(clip);
            }
            var definition = AssetDatabase.LoadAssetAtPath<BossDefinition>(Root + "Data/BrambleGuardian.asset");
            if (definition == null) { definition = ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(definition, Root + "Data/BrambleGuardian.asset"); }
            definition.bossId = "AncientThornGuardian"; definition.bossName = "Bramble Guardian"; definition.bossTitle = "The Forest Keeper";
            definition.maxHealth = 650; definition.moveSpeed = .65f; definition.basicAttackDamage = 18; definition.attackInterval = 2;
            definition.attackRange = 1.4f; definition.tint = Color.white; definition.energyReward = 120;
            definition.moonShardsReward = 80; definition.moonFragmentIndex = 1;
            definition.specialAttacks = new System.Collections.Generic.List<BossAttackTelegraph> {
                new BossAttackTelegraph { attackName="Root Slam", radius=.95f, damage=32, telegraphDuration=1.5f, cooldown=6 },
                new BossAttackTelegraph { attackName="Bramble Sweep", radius=1.25f, damage=28, telegraphDuration=1.65f, cooldown=8 }
            };
            var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(Root + "Data/BossAttack.asset");
            if (profile == null) { profile = ScriptableObject.CreateInstance<AttackPresentationProfile>(); AssetDatabase.CreateAsset(profile, Root + "Data/BossAttack.asset"); }
            profile.attackName = "BrambleSlam"; profile.animationDuration = .6f; profile.releaseFraction = .45f; profile.impactFrames = new Sprite[0];
            string prefabPath = "Assets/WorldOfThePack_Game/Prefabs/Units/Boss_AncientThornGuardian.prefab";
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var body = root.transform.Find("Body"); if (body == null) { body = new GameObject("Body").transform; body.SetParent(root.transform, false); }
                body.localPosition = Vector3.zero; body.localRotation = Quaternion.identity; body.localScale = Vector3.one;
                var sr = body.GetComponent<SpriteRenderer>(); if (sr == null) sr = body.gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = sprites.First(s => s.name == "Guardian_Idle_0"); sr.sortingOrder = 210;
                var oldRenderer = root.GetComponent<SpriteRenderer>(); if (oldRenderer != null) { sr.sharedMaterial = oldRenderer.sharedMaterial; UnityEngine.Object.DestroyImmediate(oldRenderer); }
                var animator = root.GetComponent<Animator>(); if (animator == null) animator = root.AddComponent<Animator>(); animator.runtimeAnimatorController = controller;
                var boss = root.GetComponent<BossUnit>(); var so = new SerializedObject(boss);
                so.FindProperty("bodyRenderer").objectReferenceValue = sr; so.FindProperty("animator").objectReferenceValue = animator;
                so.FindProperty("bossDefinition").objectReferenceValue = definition; so.ApplyModifiedPropertiesWithoutUndo();
                var presentation = root.GetComponent<UnitAttackPresentation>(); if (presentation == null) presentation = root.AddComponent<UnitAttackPresentation>(); presentation.Configure(profile);
                var ps = new SerializedObject(presentation); var f = ps.FindProperty("_slashFrames"); f.arraySize = slash.Length;
                for (int i = 0; i < slash.Length; i++) f.GetArrayElementAtIndex(i).objectReferenceValue = slash[i]; ps.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            EditorUtility.SetDirty(definition); EditorUtility.SetDirty(profile); EditorUtility.SetDirty(controller);
        }
        private static void BuildCleanEffects()
        {
            string path = Root + "Art/CleanCombatFX.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects(); var rects = new SpriteRect[16];
            for (int i = 0; i < 16; i++)
            {
                string name = "CleanFX_" + (i / 4) + "_" + (i % 4); var old = previous.FirstOrDefault(r => r.name == name);
                rects[i] = new SpriteRect { name=name, rect=new Rect(i%4*w/4f,(3-i/4)*h/4f,w/4f,h/4f),
                    pivot=Vector2.one*.5f, alignment=SpriteAlignment.Center, spriteID=old!=null?old.spriteID:GUID.Generate() };
            }
            provider.SetSpriteRects(rects); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            string[] names = { "EnemySpawn", "EnemyDefeat", "MagicImpact", "CriticalBurst", "MoonTransformation", "HealingCircle", "MoonlightSlash", "LeafVolley", "PawDash", "StarShower", "GearTrap", "ForestFocus", "RallyHowl", "AcornShockwave", "LeafImpact", "StardustImpact", "ForestHeal", "MoonMend", "MoonNova", "BouncingTrap", "BubbleDevice", "ZoomDash", "Whirlwind", "BlessingPop", "AcornBarrier", "EarthImpact", "WindStrike", "NatureArrow", "MoonBubble", "StarProjectile", "SpringBolt" };
            System.IO.Directory.CreateDirectory(Root + "Prefabs");
            var pool = UnityEngine.Object.FindFirstObjectByType<CombatVFXPool>();
            if (pool == null) pool = new GameObject("CombatPresentation").AddComponent<CombatVFXPool>();
            var so = new SerializedObject(pool); var list = so.FindProperty("registeredPrefabs"); list.arraySize = names.Length;
            for (int i=0;i<names.Length;i++)
            {
                string name=names[i]; int row=name.Contains("Heal")||name.Contains("Mend")?2:name.Contains("Moon")||name.Contains("Star")||name.Contains("Magic")?1:name.Contains("Enemy")?3:0;
                string dataPath=Root+"Data/FX_"+name+".asset";
                var definition=AssetDatabase.LoadAssetAtPath<WorldOfThePack.VFX.CombatEffectDefinition>(dataPath);
                if(definition==null){definition=ScriptableObject.CreateInstance<WorldOfThePack.VFX.CombatEffectDefinition>();AssetDatabase.CreateAsset(definition,dataPath);}
                definition.frames=sprites.Where(s=>s.name.StartsWith("CleanFX_"+row+"_")).OrderBy(s=>s.name).ToArray(); definition.framesPerSecond=16; definition.sortingOrder=350; definition.staticMarker=false;
                var go=new GameObject(name); var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=definition.frames[0];renderer.sortingOrder=350;
                float width=name.Contains("Transformation")?1.05f:row==2?.9f:.6f;
                go.transform.localScale=Vector3.one*width/Mathf.Max(.01f,definition.frames[0].bounds.size.x);
                var flip=go.AddComponent<WorldOfThePack.VFX.SpriteFlipbookEffect>();flip.definition=definition;flip.loop=false;flip.deactivateOnComplete=true;
                string prefabPath=Root+"Prefabs/FX_"+name+".prefab"; var prefab=PrefabUtility.SaveAsPrefabAsset(go,prefabPath);UnityEngine.Object.DestroyImmediate(go);
                list.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue=name;list.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue=prefab;
                EditorUtility.SetDirty(definition);
            }
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(pool);
            foreach(string guid in AssetDatabase.FindAssets("t:AttackPresentationProfile",new[]{"Assets/New/Combat/Data"}))
            {
                var profile=AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(AssetDatabase.GUIDToAssetPath(guid));
                int row=profile.attackName.Contains("Momo")||profile.attackName.Contains("Lulu")?1:0;
                profile.impactFrames=sprites.Where(s=>s.name.StartsWith("CleanFX_"+row+"_")).OrderBy(s=>s.name).ToArray();
                profile.impactSize=.8f;EditorUtility.SetDirty(profile);
            }
        }
        private static void PolishPrefab(string path, Sprite[] slash, string hero)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<PackUnitMotion>() == null) root.AddComponent<PackUnitMotion>();
                var presentation = root.GetComponent<UnitAttackPresentation>();
                if (presentation != null)
                {
                    var so = new SerializedObject(presentation); var frames = so.FindProperty("_slashFrames"); frames.arraySize = slash.Length;
                    for (int i = 0; i < slash.Length; i++) frames.GetArrayElementAtIndex(i).objectReferenceValue = slash[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                if (hero != null)
                {
                    var unit = root.GetComponent<HeroUnit>(); var so = new SerializedObject(unit);
                    so.FindProperty("heroKit").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HeroKitDefinition>("Assets/WorldOfThePack_Game/Data/HeroKits/" + hero + "_Kit.asset");
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
