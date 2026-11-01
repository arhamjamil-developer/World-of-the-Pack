using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using WorldOfThePack.CombatArt;
using WorldOfThePack.Game;

namespace WorldOfThePack.NewCombat.Editor
{
    public static class CombatAssetPipelineBuilder
    {
        private static readonly string[] Heroes = { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" };


        public static string WireAllPrefabsAndScene()
        {
            var log = new StringBuilder();
            try
            {
                // 1. Wire Enemies
                var enemyMap = new Dictionary<string, (string profile, string controller)>
                {
                    { "Assets/New/Enemy/Prefabs/Enemy_Soldier.prefab", ("Assets/New/Combat/Data/Soldier_AttackProfile.asset", "Assets/New/Enemy/Animations/Soldier_South.controller") },
                    { "Assets/New/Enemy/Prefabs/Enemy_Rudra.prefab", ("Assets/New/Combat/Data/Rudra_AttackProfile.asset", "Assets/New/Enemy/Animations/Rudra_South.controller") },
                    { "Assets/New/Enemy/Prefabs/Enemy_Elsa.prefab", ("Assets/New/Combat/Data/Elsa_AttackProfile.asset", "Assets/New/Enemy/Animations/Elsa_South.controller") }
                };

                foreach (var pair in enemyMap)
                {
                    var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(pair.Value.profile);
                    var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(pair.Value.controller);
                    var root = PrefabUtility.LoadPrefabContents(pair.Key);
                    if (root != null)
                    {
                        if (profile != null)
                        {
                            var uap = root.GetComponent<UnitAttackPresentation>() ?? root.AddComponent<UnitAttackPresentation>();
                            var soUap = new SerializedObject(uap);
                            soUap.FindProperty("_profile").objectReferenceValue = profile;
                            soUap.ApplyModifiedProperties();
                            EditorUtility.SetDirty(uap);
                        }

                        if (controller != null)
                        {
                            var anim = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
                            var soAnim = new SerializedObject(anim);
                            soAnim.FindProperty("m_Controller").objectReferenceValue = controller;
                            soAnim.ApplyModifiedProperties();
                            EditorUtility.SetDirty(anim);
                        }

                        PrefabUtility.SaveAsPrefabAsset(root, pair.Key);
                        PrefabUtility.UnloadPrefabContents(root);
                        log.AppendLine($"Wired {pair.Key} -> Profile={pair.Value.profile}, Controller={pair.Value.controller}");
                    }
                }

                // 2. Wire Heroes
                foreach (var hero in Heroes)
                {
                    string prefabPath = $"Assets/WorldOfThePack_Game/Prefabs/Units/{hero}_PackUnit.prefab";
                    string profilePath = $"Assets/New/Combat/Data/{hero}_AttackProfile.asset";
                    var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(profilePath);
                    var root = PrefabUtility.LoadPrefabContents(prefabPath);
                    if (root != null && profile != null)
                    {
                        var uap = root.GetComponent<UnitAttackPresentation>() ?? root.AddComponent<UnitAttackPresentation>();
                        var so = new SerializedObject(uap);
                        so.FindProperty("_profile").objectReferenceValue = profile;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(uap);
                        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                        PrefabUtility.UnloadPrefabContents(root);
                        log.AppendLine($"Wired {hero} -> {profilePath}");
                    }
                }

                // 3. Wire Scene WaveManager
                if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
                }

                var wm = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
                if (wm != null)
                {
                    var so = new SerializedObject(wm);
                    var soldier = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Soldier.prefab");
                    var rudra = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Rudra.prefab");
                    var elsa = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Elsa.prefab");

                    so.FindProperty("soldierPrefab").objectReferenceValue = soldier;
                    so.FindProperty("rudraPrefab").objectReferenceValue = rudra;
                    so.FindProperty("elsaPrefab").objectReferenceValue = elsa;
                    so.FindProperty("authoredWaves").ClearArray();
                    so.ApplyModifiedProperties();

                    EditorUtility.SetDirty(wm);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(wm.gameObject.scene);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(wm.gameObject.scene);
                    log.AppendLine("Wired WaveManager in SampleScene!");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                return log.ToString();
            }
            catch (Exception ex)
            {
                return "EX: " + ex.ToString();
            }
        }
        public static void BuildAllCombatAssets()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                EnsureFolders();
                SliceEnemySheets();
                SliceHeroAttackSheets();
                SliceProjectiles();

                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                BuildEnemyAnimationsAndControllers();
                BuildHeroAttackClips();
                BuildAttackPresentationProfiles();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                BuildEnemyPrefabs();
                WireHeroPrefabs();
                WireWaveManager();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                Debug.Log("[CombatAssetPipelineBuilder] All combat assets built and wired successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/New/Enemy/Animations");
            EnsureFolder("Assets/New/Enemy/Data");
            EnsureFolder("Assets/New/Enemy/Prefabs");
            EnsureFolder("Assets/New/Combat/Animations");
            EnsureFolder("Assets/New/Combat/Data");
            EnsureFolder("Assets/New/Combat/Prefabs");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string leaf = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        public static void SliceEnemySheets()
        {
            string[] enemies = { "Soldier", "Rudra", "Elsa" };
            string[] actions = { "Idle", "Walk", "Attack", "Dead" };

            foreach (var enemy in enemies)
            {
                string path = $"Assets/New/Enemy/Art/{enemy}_South.png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null)
                {
                    Debug.LogWarning($"[CombatPipeline] Enemy sheet missing: {path}");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 128;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;

                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
                provider.InitSpriteEditorDataProvider();

                var rects = new List<SpriteRect>();
                // 4 rows (top to bottom): Idle, Walk, Attack, Dead
                for (int row = 0; row < 4; row++)
                {
                    for (int col = 0; col < 6; col++)
                    {
                        string spriteName = $"{enemy}_South_{actions[row]}_{col:D2}";
                        var id = StableId(spriteName);
                        // row 0 is top (y = 3 * 256 = 768), row 3 is bottom (y = 0)
                        float y = (3 - row) * 256;
                        rects.Add(new SpriteRect
                        {
                            name = spriteName,
                            rect = new Rect(col * 256, y, 256, 256),
                            alignment = SpriteAlignment.Custom,
                            pivot = new Vector2(0.5f, 0.18f),
                            spriteID = id
                        });
                    }
                }

                provider.SetSpriteRects(rects.ToArray());
                var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
                provider.Apply();
                importer.SaveAndReimport();
            }
        }

        public static void SliceHeroAttackSheets()
        {
            foreach (var hero in Heroes)
            {
                string path = $"Assets/New/Combat/Art/{hero}_Attack_North.png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null)
                {
                    Debug.LogWarning($"[CombatPipeline] Hero attack sheet missing: {path}");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 128;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 4096;

                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
                provider.InitSpriteEditorDataProvider();

                var rects = new List<SpriteRect>();
                // Top row (row 0, y=362): Human Attack
                // Bottom row (row 1, y=0): Wolf Attack
                string[] forms = { "Human", "Wolf" };
                for (int row = 0; row < 2; row++)
                {
                    for (int col = 0; col < 6; col++)
                    {
                        string spriteName = $"{hero}_{forms[row]}_Attack_North_{col:D2}";
                        var id = StableId(spriteName);
                        float y = (1 - row) * 362;
                        rects.Add(new SpriteRect
                        {
                            name = spriteName,
                            rect = new Rect(col * 362, y, 362, 362),
                            alignment = SpriteAlignment.Custom,
                            pivot = new Vector2(0.5f, 0.18f),
                            spriteID = id
                        });
                    }
                }

                provider.SetSpriteRects(rects.ToArray());
                var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
                provider.Apply();
                importer.SaveAndReimport();
            }
        }

        public static void SliceProjectiles()
        {
            string[] projPaths =
            {
                "Assets/New/Enemy/Projectiles/ElsaArrow.png",
                "Assets/New/Combat/Art/Projectiles/ElvaArrow.png",
                "Assets/New/Combat/Art/Projectiles/LuluMoonOrb.png",
                "Assets/New/Combat/Art/Projectiles/MomoStarBolt.png",
                "Assets/New/Combat/Art/Projectiles/TillyGearBolt.png"
            };

            foreach (var path in projPaths)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.SaveAndReimport();
            }
        }

        public static void BuildEnemyAnimationsAndControllers()
        {
            string[] enemies = { "Soldier", "Rudra", "Elsa" };
            string[] actions = { "Idle", "Walk", "Attack", "Dead" };
            float[] fpsRates = { 6f, 8f, 10f, 8f };

            foreach (var enemy in enemies)
            {
                string sheetPath = $"Assets/New/Enemy/Art/{enemy}_South.png";
                var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToDictionary(s => s.name);

                string controllerPath = $"Assets/New/Enemy/Animations/{enemy}_South.controller";
                AssetDatabase.DeleteAsset(controllerPath);
                var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("Action", AnimatorControllerParameterType.Int);

                var rootMachine = controller.layers[0].stateMachine;
                AnimatorState idleState = null;
                AnimatorState walkState = null;
                AnimatorState attackState = null;
                AnimatorState deadState = null;

                for (int a = 0; a < actions.Length; a++)
                {
                    string actionName = actions[a];
                    bool loop = a < 2; // Idle and Walk loop
                    string clipPath = $"Assets/New/Enemy/Animations/{enemy}_{actionName}_South.anim";
                    var clip = new AnimationClip { name = $"{enemy}_{actionName}_South", frameRate = fpsRates[a] };

                    var frames = new List<Sprite>();
                    for (int f = 0; f < 6; f++)
                    {
                        string spriteName = $"{enemy}_South_{actionName}_{f:D2}";
                        if (allSprites.TryGetValue(spriteName, out var s)) frames.Add(s);
                    }

                    if (frames.Count > 0)
                    {
                        SetSpriteCurve(clip, "Body", frames.ToArray(), fpsRates[a], loop);
                        var settings = AnimationUtility.GetAnimationClipSettings(clip);
                        settings.loopTime = loop;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                        SaveClip(clip, clipPath);
                        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    }

                    var st = rootMachine.AddState(actionName);
                    st.motion = clip;

                    if (a == 0) idleState = st;
                    else if (a == 1) walkState = st;
                    else if (a == 2) attackState = st;
                    else if (a == 3) deadState = st;
                }

                rootMachine.defaultState = idleState;

                AddAnyStateTransition(rootMachine, idleState, "Action", AnimatorConditionMode.Equals, 0);
                AddAnyStateTransition(rootMachine, walkState, "Action", AnimatorConditionMode.Equals, 1);
                AddAnyStateTransition(rootMachine, attackState, "Action", AnimatorConditionMode.Equals, 2);
                AddAnyStateTransition(rootMachine, deadState, "Action", AnimatorConditionMode.Equals, 3);

                EditorUtility.SetDirty(controller);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        public static void BuildHeroAttackClips()
        {
            foreach (var hero in Heroes)
            {
                string sheetPath = $"Assets/New/Combat/Art/{hero}_Attack_North.png";
                var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToDictionary(s => s.name);

                string[] forms = { "Human", "Wolf" };
                foreach (var form in forms)
                {
                    string clipPath = $"Assets/New/Combat/Animations/{hero}_{form}_Attack_North.anim";
                    var clip = new AnimationClip { name = $"{hero}_{form}_Attack_North", frameRate = 10f };

                    var frames = new List<Sprite>();
                    for (int f = 0; f < 6; f++)
                    {
                        string spriteName = $"{hero}_{form}_Attack_North_{f:D2}";
                        if (allSprites.TryGetValue(spriteName, out var s)) frames.Add(s);
                    }

                    if (frames.Count > 0)
                    {
                        SetSpriteCurve(clip, "Body", frames.ToArray(), 10f, false);
                        var settings = AnimationUtility.GetAnimationClipSettings(clip);
                        settings.loopTime = false;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                        SaveClip(clip, clipPath);

                        // Hook into existing controller if present
                        string controllerPath = $"Assets/Characters/{hero}/Animations/{hero}_{form}.controller";
                        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                        if (controller != null)
                        {
                            var state = controller.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name == "Attack").state;
                            if (state != null)
                            {
                                state.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                                EditorUtility.SetDirty(controller);
                            }
                        }
                    }
                }
            }
        }

        public static void BuildAttackPresentationProfiles()
        {
            // 1. HeroWolvy
            CreateProfile("HeroWolvy_AttackProfile", "WolvySwordSweep", 0.6f, 0.5f, null, 0f, 0f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/HeroWolvy", "MoonlightSlash"), 1.25f, false, 1.4f, 0.25f);

            // 2. PrincessElva
            Sprite elvaArrow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/New/Combat/Art/Projectiles/ElvaArrow.png");
            CreateProfile("PrincessElva_AttackProfile", "ElvaBowShot", 0.6f, 0.5f, elvaArrow, 9.5f, 0.55f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/PrincessElva", "LeafImpact"), 1.0f, false, 0f, 0f);

            // 3. FairyLulu
            Sprite luluOrb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/New/Combat/Art/Projectiles/LuluMoonOrb.png");
            CreateProfile("FairyLulu_AttackProfile", "LuluMoonCast", 0.6f, 0.5f, luluOrb, 8.0f, 0.65f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/FairyLulu", "MoonImpact"), 1.1f, false, 0f, 0f);

            // 4. CaptainBobo
            CreateProfile("CaptainBobo_AttackProfile", "BoboShieldBash", 0.6f, 0.5f, null, 0f, 0f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/CaptainBobo", "EarthImpact"), 1.2f, false, 0f, 0f);

            // 5. ScoutPip
            CreateProfile("ScoutPip_AttackProfile", "PipDaggerStrike", 0.5f, 0.4f, null, 0f, 0f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/ScoutPip", "PawDash"), 0.9f, true, 0f, 0f);

            // 6. WizardMomo
            Sprite momoBolt = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/New/Combat/Art/Projectiles/MomoStarBolt.png");
            CreateProfile("WizardMomo_AttackProfile", "MomoStarBolt", 0.65f, 0.5f, momoBolt, 8.5f, 0.7f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/Shared", "CriticalBurst"), 1.1f, false, 0f, 0f);

            // 7. TinkerTilly
            Sprite tillyBolt = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/New/Combat/Art/Projectiles/TillyGearBolt.png");
            CreateProfile("TinkerTilly_AttackProfile", "TillyGearShot", 0.6f, 0.5f, tillyBolt, 9.0f, 0.65f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/Shared", "CriticalBurst"), 1.0f, false, 0f, 0f);

            // 8. Soldier
            CreateProfile("Soldier_AttackProfile", "SoldierSwordSlash", 0.6f, 0.5f, null, 0f, 0f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/Shared", "CriticalBurst"), 1.0f, false, 0f, 0f);

            // 9. Rudra
            CreateProfile("Rudra_AttackProfile", "RudraGauntletSlam", 0.8f, 0.6f, null, 0f, 0f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/CaptainBobo", "EarthImpact"), 1.4f, false, 1.6f, 0.25f);

            // 10. Elsa
            Sprite elsaArrow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/New/Enemy/Projectiles/ElsaArrow.png");
            CreateProfile("Elsa_AttackProfile", "ElsaRoyalArrow", 0.7f, 0.5f, elsaArrow, 8.5f, 0.6f,
                LoadVFXFrames("Assets/VFX/WorldOfThePack/Art/Shared", "CriticalBurst"), 1.0f, false, 0f, 0f);
        }

        public static void BuildEnemyPrefabs()
        {
            Material litMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/_Pipeline/Materials/Sprite-Lit-Default.mat");
            if (litMat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                litMat = new Material(shader) { name = "Sprite-Lit-Default" };
                AssetDatabase.CreateAsset(litMat, "Assets/New/Enemy/Data/Enemy-Sprite-Lit.mat");
            }

            BuildOneEnemyPrefab("Enemy_Soldier", EnemyType.Soldier, "Assets/New/Enemy/Art/Soldier_South.png",
                "Soldier_South_Idle_00", "Assets/New/Enemy/Animations/Soldier_South.controller",
                "Assets/New/Combat/Data/Soldier_AttackProfile.asset", litMat, 0.95f);

            BuildOneEnemyPrefab("Enemy_Rudra", EnemyType.Rudra, "Assets/New/Enemy/Art/Rudra_South.png",
                "Rudra_South_Idle_00", "Assets/New/Enemy/Animations/Rudra_South.controller",
                "Assets/New/Combat/Data/Rudra_AttackProfile.asset", litMat, 1.08f);

            BuildOneEnemyPrefab("Enemy_Elsa", EnemyType.Elsa, "Assets/New/Enemy/Art/Elsa_South.png",
                "Elsa_South_Idle_00", "Assets/New/Enemy/Animations/Elsa_South.controller",
                "Assets/New/Combat/Data/Elsa_AttackProfile.asset", litMat, 0.95f);
        }

        private static void BuildOneEnemyPrefab(string prefabName, EnemyType type, string sheetPath,
            string defaultSpriteName, string controllerPath, string profilePath, Material mat, float scale)
        {
            string outPath = $"Assets/New/Enemy/Prefabs/{prefabName}.prefab";
            var root = new GameObject(prefabName);
            root.transform.localScale = Vector3.one * scale;

            var col = root.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.offset = new Vector2(0f, 0.25f);
            col.isTrigger = true;

            var bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(root.transform, false);
            var sr = bodyObj.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = mat;
            sr.sortingOrder = 100;

            var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToDictionary(s => s.name);
            if (allSprites.TryGetValue(defaultSpriteName, out var s)) sr.sprite = s;

            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);

            var enemyUnit = root.AddComponent<EnemyUnit>();
            // Use SerializedObject to set private serialized fields
            var soEnemy = new SerializedObject(enemyUnit);
            soEnemy.FindProperty("enemyType").intValue = (int)type;
            soEnemy.FindProperty("animator").objectReferenceValue = animator;
            soEnemy.FindProperty("bodyRenderer").objectReferenceValue = sr;
            soEnemy.ApplyModifiedPropertiesWithoutUndo();

            var uap = root.AddComponent<UnitAttackPresentation>();
            var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(profilePath);
            var soUap = new SerializedObject(uap);
            soUap.FindProperty("_profile").objectReferenceValue = profile;
            soUap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(uap);

            PrefabUtility.SaveAsPrefabAsset(root, outPath);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log($"[CombatPipeline] Created enemy prefab: {outPath}");
        }

        public static void WireHeroPrefabs()
        {
            foreach (var hero in Heroes)
            {
                string prefabPath = $"Assets/WorldOfThePack_Game/Prefabs/Units/{hero}_PackUnit.prefab";
                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                if (root == null) continue;

                var uap = root.GetComponent<UnitAttackPresentation>();
                if (uap == null) uap = root.AddComponent<UnitAttackPresentation>();

                string profilePath = $"Assets/New/Combat/Data/{hero}_AttackProfile.asset";
                var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(profilePath);
                var soUap = new SerializedObject(uap);
                soUap.FindProperty("_profile").objectReferenceValue = profile;
                soUap.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(uap);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log($"[CombatPipeline] Wired hero prefab: {hero}");
            }
        }

        public static void WireWaveManager()
        {
            if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            }

            var rudraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Rudra.prefab");
            var elsaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Elsa.prefab");
            var soldierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Soldier.prefab");

            // Wire into open scene WaveManager
            var wm = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                var so = new SerializedObject(wm);
                if (soldierPrefab != null) so.FindProperty("soldierPrefab").objectReferenceValue = soldierPrefab;
                if (rudraPrefab != null) so.FindProperty("rudraPrefab").objectReferenceValue = rudraPrefab;
                if (elsaPrefab != null) so.FindProperty("elsaPrefab").objectReferenceValue = elsaPrefab;
                so.FindProperty("authoredWaves").ClearArray();
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(wm);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(wm.gameObject.scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(wm.gameObject.scene);
                Debug.Log("[CombatPipeline] Wired Soldier, Rudra, and Elsa into scene WaveManager!");
            }
        }

        private static void CreateProfile(string assetName, string attackName, float duration, float releaseFraction,
            Sprite projectile, float projSpeed, float projWidth, Sprite[] impactFrames, float impactSize,
            bool doubleStrike, float cleaveRadius, float cleaveFraction)
        {
            string path = $"Assets/New/Combat/Data/{assetName}.asset";
            var profile = AssetDatabase.LoadAssetAtPath<AttackPresentationProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<AttackPresentationProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.attackName = attackName;
            profile.animationDuration = duration;
            profile.releaseFraction = releaseFraction;
            profile.projectile = projectile;
            profile.projectileSpeed = projSpeed;
            profile.projectileWidth = projWidth;
            profile.impactFrames = impactFrames;
            profile.impactSize = impactSize;
            profile.doubleStrike = doubleStrike;
            profile.cleaveRadius = cleaveRadius;
            profile.cleaveFraction = cleaveFraction;

            EditorUtility.SetDirty(profile);
        }

        private static Sprite[] LoadVFXFrames(string folder, string prefix)
        {
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            var list = new List<Sprite>();
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (Path.GetFileName(p).StartsWith(prefix))
                {
                    var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    if (s != null) list.Add(s);
                }
            }
            list.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            return list.ToArray();
        }

        private static void SetSpriteCurve(AnimationClip clip, string relativePath, Sprite[] frames, float fps, bool loop)
        {
            var keys = new List<ObjectReferenceKeyframe>();
            for (int i = 0; i < frames.Length; i++)
            {
                keys.Add(new ObjectReferenceKeyframe { time = i / fps, value = frames[i] });
            }
            keys.Add(new ObjectReferenceKeyframe { time = frames.Length / fps, value = loop ? frames[0] : frames[frames.Length - 1] });
            AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = relativePath, type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys.ToArray());
        }

        private static void SaveClip(AnimationClip clip, string path)
        {
            var old = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (old == null) AssetDatabase.CreateAsset(clip, path);
            else { EditorUtility.CopySerialized(clip, old); UnityEngine.Object.DestroyImmediate(clip); }
        }

        private static void AddAnyStateTransition(AnimatorStateMachine sm, AnimatorState destination, string param, AnimatorConditionMode mode, int threshold)
        {
            var trans = sm.AddAnyStateTransition(destination);
            trans.AddCondition(mode, threshold, param);
            trans.duration = 0.05f;
            trans.canTransitionToSelf = false;
        }

        private static GUID StableId(string name)
        {
            using (var md5 = MD5.Create())
            {
                return new GUID(BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(name))).Replace("-", "").ToLowerInvariant());
            }
        }
    }
}
