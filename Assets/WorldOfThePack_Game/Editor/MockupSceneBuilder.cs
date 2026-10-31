using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace WorldOfThePack.EditorTools
{
    public static class MockupSceneBuilder
    {
        [MenuItem("World of the Pack/Restore Live Playable Game")]
        public static void RestoreLiveGame()
        {
            try
            {
                // Remove static comparison root
                GameObject oldRoot = GameObject.Find("MockupBattlefieldRoot");
                if (oldRoot != null) Object.DestroyImmediate(oldRoot);

                GameObject pondOld = GameObject.Find("WaterPondProp");
                if (pondOld != null) Object.DestroyImmediate(pondOld);

                GameObject campOld = GameObject.Find("CampsiteProp");
                if (campOld != null) Object.DestroyImmediate(campOld);

                // Reconstruct the clean, beautiful Mockup UI
                ReconstructGameUI.Reconstruct();

                // Re-enable live game systems
                var gd = Object.FindFirstObjectByType<WorldOfThePack.Game.GameDirector>(FindObjectsInactive.Include);
                if (gd != null) gd.enabled = true;

                var wm = Object.FindFirstObjectByType<WorldOfThePack.Game.WaveManager>(FindObjectsInactive.Include);
                if (wm != null) wm.enabled = true;

                var fc = Object.FindFirstObjectByType<WorldOfThePack.Game.FormationController>(FindObjectsInactive.Include);
                if (fc != null) fc.enabled = true;

                var laneOverlay = Object.FindFirstObjectByType<WorldOfThePack.Game.LaneOverlayRenderer>(FindObjectsInactive.Include);
                if (laneOverlay != null) laneOverlay.gameObject.SetActive(true);

                Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
                if (cam != null)
                {
                    var corr = cam.GetComponent<WorldOfThePack.Game.CorridorCamera>();
                    if (corr != null) corr.enabled = true;
                    var exp = cam.GetComponent<WorldOfThePack.Game.ExpeditionCamera>();
                    if (exp != null) exp.enabled = true;
                }

                // Save scene
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

                Debug.Log("Live playable game successfully restored and ready for Play Mode!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("RestoreLiveGame error: " + ex);
            }
        }

        [MenuItem("World of the Pack/Reconstruct Mockup Scene")]
        public static void BuildScene()
        {
            // 0. Ensure all individual sprite assets have Single mode and Alpha transparency
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/WaveBadge_Clean.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/Title_WorldOfThePack.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/PackHearts_Clean.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/BossBar_Clean.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/Boss_AcornIcon.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/NextCamp_Clean.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/Camp_TentIcon.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/PauseButton_Round.png");
            EnsureSingleSprite("Assets/Characters/Enemies/Boss_BrambleBrute_South.png");
            EnsureSingleSprite("Assets/Characters/Enemies/Enemy_Archer_South.png");
            EnsureSingleSprite("Assets/Characters/Enemies/Enemy_Priest_South.png");
            EnsureSingleSprite("Assets/Characters/Enemies/Enemy_Soldier_South.png");
            EnsureSingleSprite("Assets/Visuals/SunnyForest_WaterPondProp.png");
            EnsureSingleSprite("Assets/Visuals/SunnyForest_CampsiteProp.png");
            EnsureSingleSprite("Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-loop-source-v01.png");
            EnsureSingleSprite("Assets/VFX/WorldOfThePack/Art/Telegraphs/EnemyConeSouth_00.png");
            EnsureSingleSprite("Assets/VFX/WorldOfThePack/Art/GroundRing_AllyBlue_HD.png");
            EnsureSingleSprite("Assets/VFX/WorldOfThePack/Art/GroundRing_EnemyOrange_HD.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/Chevron_UpArrows.png");
            EnsureSingleSprite("Assets/UI/WorldOfThePack/Art/Controls/DeployHere_Pill.png");

            // 1. Reconstruct Complete UI Canvas
            ReconstructGameUI.Reconstruct();

            // 2. Clear old MockupRoot if present
            GameObject oldRoot = GameObject.Find("MockupBattlefieldRoot");
            if (oldRoot != null)
            {
                Object.DestroyImmediate(oldRoot);
            }

            GameObject root = new GameObject("MockupBattlefieldRoot");

            // Load Font
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/LilitaOne-Dynamic.asset");
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            // Adjust Camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 0.7f, -10f);
                cam.orthographicSize = 6.2f;
                var corr = cam.GetComponent<WorldOfThePack.Game.CorridorCamera>();
                if (corr != null) corr.enabled = false;
                var exp = cam.GetComponent<WorldOfThePack.Game.ExpeditionCamera>();
                if (exp != null) exp.enabled = false;
            }

            // Disable LaneOverlayRenderer during comparison
            var laneOverlay = Object.FindFirstObjectByType<WorldOfThePack.Game.LaneOverlayRenderer>();
            if (laneOverlay != null) laneOverlay.gameObject.SetActive(false);

            // Disable Spawners/Formation during visual comparison
            var gd = Object.FindFirstObjectByType<WorldOfThePack.Game.GameDirector>();
            if (gd != null) gd.enabled = false;
            var wm = Object.FindFirstObjectByType<WorldOfThePack.Game.WaveManager>();
            if (wm != null) wm.enabled = false;
            var fc = Object.FindFirstObjectByType<WorldOfThePack.Game.FormationController>();
            if (fc != null) fc.enabled = false;

            // Remove previous active combatants in scene
            var existingCombatants = Object.FindObjectsByType<WorldOfThePack.Game.Combatant>(FindObjectsSortMode.None);
            foreach (var c in existingCombatants)
            {
                Object.DestroyImmediate(c.gameObject);
            }

            // ==========================================
            // 1. ENDLESS SUNNY FOREST TRAIL BACKGROUND
            // ==========================================
            Sprite pathSprite = LoadSprite("Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-loop-source-v01.png");
            if (pathSprite != null)
            {
                float tileH = pathSprite.rect.height / pathSprite.pixelsPerUnit;
                for (int i = 0; i < 3; i++)
                {
                    Vector3 tilePos = new Vector3(0f, -4.0f + i * tileH, 0f);
                    CreateSpriteObject($"PathTile_{i}", root.transform, pathSprite, tilePos, Vector3.one, -100);
                }
            }

            // ==========================================
            // 2. SCENIC WORLD PROPS (Pond & Campsite)
            // ==========================================
            Sprite pondSprite = LoadSprite("Assets/Visuals/SunnyForest_WaterPondProp.png");
            if (pondSprite != null)
            {
                CreateSpriteObject("Prop_WaterPond", root.transform, pondSprite, new Vector3(-2.15f, 0.45f, 0f), new Vector3(1.15f, 1.15f, 1f), -45);
            }

            Sprite campSprite = LoadSprite("Assets/Visuals/SunnyForest_CampsiteProp.png");
            if (campSprite != null)
            {
                CreateSpriteObject("Prop_Campsite", root.transform, campSprite, new Vector3(2.15f, 1.85f, 0f), new Vector3(1.15f, 1.15f, 1f), -45);
            }

            // Ground Indicator Sprites
            Sprite allyRing = LoadSprite("Assets/VFX/WorldOfThePack/Art/GroundRing_AllyBlue_HD.png");
            Sprite enemyRing = LoadSprite("Assets/VFX/WorldOfThePack/Art/GroundRing_EnemyOrange_HD.png");

            // ==========================================
            // 3. BOSS: BRAMBLE BRUTE & TELEGRAPH CONE
            // ==========================================
            Sprite bossSprite = LoadSprite("Assets/Characters/Enemies/Boss_BrambleBrute_South.png");
            GameObject bossGo = CreateSpriteObject("Boss_BrambleBrute", root.transform, bossSprite, new Vector3(0f, 2.70f, 0f), Vector3.one * 2.1f, 180);

            // Orange Telegraph Danger Cone (pointing South towards heroes)
            Sprite coneSprite = LoadSprite("Assets/VFX/WorldOfThePack/Art/Telegraphs/EnemyConeSouth_00.png");
            if (coneSprite != null)
            {
                GameObject coneGo = CreateSpriteObject("Telegraph_ConeSouth", root.transform, coneSprite, new Vector3(0f, 1.80f, 0f), Vector3.one * 1.55f, -15);
                
                // "1.2s" timer label in center of cone
                GameObject labelGo = new GameObject("TimerLabel");
                labelGo.transform.SetParent(coneGo.transform, false);
                labelGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
                var tmp = labelGo.AddComponent<TextMeshPro>();
                if (font != null) tmp.font = font;
                tmp.text = "1.2s";
                tmp.fontSize = 3.6f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.95f, 0.85f, 1f);
                tmp.outlineWidth = 0.25f;
                tmp.outlineColor = new Color32(40, 15, 5, 255);
                tmp.sortingOrder = -10;
            }

            // ==========================================
            // 4. FLANKING ENEMIES (South-Facing)
            // ==========================================
            Sprite archerSprite = LoadSprite("Assets/Characters/Enemies/Enemy_Archer_South.png");
            CreateEnemyWithVisual("Enemy_Archer", root.transform, archerSprite, new Vector3(-1.18f, 3.25f, 0f), 1.65f, enemyRing, 170, 0.85f);

            Sprite priestSprite = LoadSprite("Assets/Characters/Enemies/Enemy_Priest_South.png");
            CreateEnemyWithVisual("Enemy_Priest", root.transform, priestSprite, new Vector3(1.10f, 3.20f, 0f), 1.65f, enemyRing, 170, 0.90f);

            Sprite soldierSprite = LoadSprite("Assets/Characters/Enemies/Enemy_Soldier_South.png");
            CreateEnemyWithVisual("Enemy_Soldier_Left", root.transform, soldierSprite, new Vector3(-1.10f, 2.05f, 0f), 1.65f, enemyRing, 200, 0.65f);
            CreateEnemyWithVisual("Enemy_Soldier_Right", root.transform, soldierSprite, new Vector3(1.10f, 2.00f, 0f), 1.65f, enemyRing, 200, 0.95f);

            // Floating Damage Popup "-42" on Soldier 1
            CreateFloatingText("DamagePopup_-42", root.transform, new Vector3(-0.68f, 2.30f, 0f), "-42", new Color(1f, 0.95f, 0.35f, 1f), font, 4.2f);

            // ==========================================
            // 5. ALLIED PACK HEROES (North-Facing)
            // ==========================================
            // Wolvy (Front Left)
            Sprite wolvySprite = LoadNorthSprite("HeroWolvy", "Attack", 2);
            CreateHeroWithVisual("Hero_Wolvy", root.transform, wolvySprite, new Vector3(-0.65f, 0.85f, 0f), Vector3.one, allyRing, 230, 0.95f);
            // Golden Moon Slash VFX
            Sprite slashSprite = LoadSprite("Assets/VFX/WorldOfThePack/Art/HeroWolvy/MoonlightSlash_02.png");
            if (slashSprite != null)
            {
                CreateSpriteObject("VFX_GoldenSlash", root.transform, slashSprite, new Vector3(-0.40f, 1.25f, 0f), Vector3.one * 1.25f, 320);
            }

            // Bobo (Front Right)
            Sprite boboSprite = LoadNorthSprite("CaptainBobo", "Walk", 2);
            CreateHeroWithVisual("Hero_Bobo", root.transform, boboSprite, new Vector3(0.42f, 0.80f, 0f), Vector3.one, allyRing, 230, 0.85f);
            // Acorn Shield Bubble VFX
            Sprite shieldSprite = LoadSprite("Assets/VFX/WorldOfThePack/Art/CaptainBobo/AcornBarrier_02.png");
            if (shieldSprite != null)
            {
                CreateSpriteObject("VFX_AcornShield", root.transform, shieldSprite, new Vector3(0.42f, 0.90f, 0f), Vector3.one * 1.30f, 310);
            }
            // Floating Heal Popup "+18 ❤"
            CreateFloatingText("HealPopup_+18", root.transform, new Vector3(0.78f, 1.15f, 0f), "+18 \u2764", new Color(0.35f, 0.95f, 0.45f, 1f), font, 4.0f);

            // Scout Pip (Dashing forward right)
            Sprite pipSprite = LoadNorthSprite("ScoutPip", "Walk", 3);
            CreateHeroWithVisual("Hero_Pip", root.transform, pipSprite, new Vector3(1.22f, 0.35f, 0f), Vector3.one, allyRing, 245, 1.0f);
            // Dash afterimage VFX
            Sprite dashVfx = LoadSprite("Assets/VFX/WorldOfThePack/Art/ScoutPip/ZoomDash_02.png");
            if (dashVfx != null)
            {
                CreateSpriteObject("VFX_PipDash", root.transform, dashVfx, new Vector3(1.48f, 0.25f, 0f), Vector3.one * 1.10f, 240);
            }

            // Princess Elva (Mid Left)
            Sprite elvaSprite = LoadNorthSprite("PrincessElva", "Attack", 2);
            CreateHeroWithVisual("Hero_Elva", root.transform, elvaSprite, new Vector3(-0.85f, 0.05f, 0f), Vector3.one, allyRing, 260, 0.90f);
            // Nature Arrow VFX
            Sprite arrowVfx = LoadSprite("Assets/VFX/WorldOfThePack/Art/PrincessElva/NatureArrow_02.png");
            if (arrowVfx != null)
            {
                GameObject arrGo = CreateSpriteObject("VFX_NatureArrow", root.transform, arrowVfx, new Vector3(-0.55f, 0.45f, 0f), Vector3.one * 1.10f, 300);
                arrGo.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            }

            // Wizard Momo (Center)
            Sprite momoSprite = LoadNorthSprite("WizardMomo", "Attack", 2);
            CreateHeroWithVisual("Hero_Momo", root.transform, momoSprite, new Vector3(0.02f, 0.00f, 0f), Vector3.one, allyRing, 260, 1.0f);
            // Star Shower VFX
            Sprite starVfx = LoadSprite("Assets/VFX/WorldOfThePack/Art/WizardMomo/StarShower_02.png");
            if (starVfx != null)
            {
                CreateSpriteObject("VFX_MomoStars", root.transform, starVfx, new Vector3(0.02f, 0.00f, 0f), Vector3.one * 1.25f, 255);
            }

            // Fairy Lulu (Back Left)
            Sprite luluSprite = LoadNorthSprite("FairyLulu", "Walk", 2);
            CreateHeroWithVisual("Hero_Lulu", root.transform, luluSprite, new Vector3(-0.95f, -0.75f, 0f), Vector3.one, allyRing, 275, 1.0f);
            // Healing Circle VFX
            Sprite healCircle = LoadSprite("Assets/VFX/WorldOfThePack/Art/FairyLulu/HealingCircle_02.png");
            if (healCircle != null)
            {
                CreateSpriteObject("VFX_LuluHeal", root.transform, healCircle, new Vector3(-0.95f, -0.75f, 0f), Vector3.one * 1.15f, 270);
            }

            // Tinker Tilly (Back Right)
            Sprite tillySprite = LoadNorthSprite("TinkerTilly", "Walk", 2);
            CreateHeroWithVisual("Hero_Tilly", root.transform, tillySprite, new Vector3(0.95f, -0.70f, 0f), Vector3.one, allyRing, 275, 1.0f);
            // Deployed Turret Prop
            Sprite turretSprite = LoadSprite("Assets/VFX/WorldOfThePack/Art/TinkerTilly/BubbleDevice_02.png");
            if (turretSprite != null)
            {
                CreateSpriteObject("Prop_Turret", root.transform, turretSprite, new Vector3(0.62f, -0.75f, 0f), Vector3.one * 1.10f, 276);
            }

            // ==========================================
            // 6. DEPLOY HERE RETICLE INDICATOR
            // ==========================================
            GameObject deployReticle = new GameObject("DeployHere_Reticle");
            deployReticle.transform.SetParent(root.transform, false);
            deployReticle.transform.position = new Vector3(0f, -1.65f, 0f);

            // Glowing blue ground ring
            if (allyRing != null)
            {
                CreateSpriteObject("ReticleRing", deployReticle.transform, allyRing, Vector3.zero, new Vector3(1.45f, 0.95f, 1f), -20);
            }

            // Translucent Wolvy Ghost Silhouette
            Sprite wolvyIdle = LoadNorthSprite("HeroWolvy", "Idle", 0);
            if (wolvyIdle != null)
            {
                GameObject ghostGo = CreateSpriteObject("GhostSilhouette", deployReticle.transform, wolvyIdle, new Vector3(0f, 0.20f, 0f), Vector3.one, 280);
                var gSr = ghostGo.GetComponent<SpriteRenderer>();
                gSr.color = new Color(0.75f, 0.90f, 1.0f, 0.65f);
            }

            // Upward Chevron Arrows
            Sprite chevSprite = LoadSprite("Assets/UI/WorldOfThePack/Art/Controls/Chevron_UpArrows.png");
            if (chevSprite != null)
            {
                CreateSpriteObject("ChevronArrows", deployReticle.transform, chevSprite, new Vector3(0f, 0.72f, 0f), Vector3.one * 1.25f, 285);
            }

            // "DEPLOY HERE" Pill Badge
            Sprite pillSprite = LoadSprite("Assets/UI/WorldOfThePack/Art/Controls/DeployHere_Pill.png");
            if (pillSprite != null)
            {
                CreateSpriteObject("DeployHere_Pill", deployReticle.transform, pillSprite, new Vector3(0f, -0.28f, 0f), Vector3.one * 1.15f, 290);
            }

            // Save Scene
            EditorUtility.SetDirty(root);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("Mockup Comparison Scene successfully constructed with clean assets!");
        }

        private static void EnsureSingleSprite(string path)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                bool changed = false;
                if (ti.textureType != TextureImporterType.Sprite)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (ti.spriteImportMode != SpriteImportMode.Single)
                {
                    ti.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (!ti.alphaIsTransparency)
                {
                    ti.alphaIsTransparency = true;
                    changed = true;
                }
                if (changed)
                {
                    ti.SaveAndReimport();
                }
            }
        }

        private static Sprite LoadSprite(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadNorthSprite(string heroId, string action = "Walk", int frame = 0)
        {
            string path = $"Assets/Characters/{heroId}/Human/{heroId}_Human_North.png";
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            string targetName = $"{heroId}_Human_{action}_North_{frame:D2}";
            foreach (var a in assets)
            {
                if (a is Sprite s && s.name == targetName) return s;
            }
            foreach (var a in assets)
            {
                if (a is Sprite s && s.name.Contains(action)) return s;
            }
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return null;
        }

        private static GameObject CreateSpriteObject(string name, Transform parent, Sprite sprite, Vector3 pos, Vector3 scale, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        private static GameObject CreateHeroWithVisual(string name, Transform parent, Sprite sprite, Vector3 pos, Vector3 scale, Sprite ringSprite, int sortingOrder, float healthPercent)
        {
            GameObject hero = new GameObject(name);
            hero.transform.SetParent(parent, false);
            hero.transform.position = pos;
            hero.transform.localScale = scale;

            // Body
            GameObject body = new GameObject("Body");
            body.transform.SetParent(hero.transform, false);
            SpriteRenderer bSr = body.AddComponent<SpriteRenderer>();
            bSr.sprite = sprite;
            bSr.color = Color.white;
            bSr.sortingOrder = sortingOrder;

            // Ground Ring
            if (ringSprite != null)
            {
                GameObject ring = new GameObject("AllyRing");
                ring.transform.SetParent(hero.transform, false);
                ring.transform.localPosition = new Vector3(0f, -0.20f, 0f);
                ring.transform.localScale = new Vector3(1.15f, 0.75f, 1f);
                SpriteRenderer rSr = ring.AddComponent<SpriteRenderer>();
                rSr.sprite = ringSprite;
                rSr.color = Color.white;
                rSr.sortingOrder = -20;
            }

            // Health Bar
            CreateUnitHealthBar(hero.transform, healthPercent, isAllied: true);

            return hero;
        }

        private static GameObject CreateEnemyWithVisual(string name, Transform parent, Sprite sprite, Vector3 pos, float scale, Sprite ringSprite, int sortingOrder, float healthPercent)
        {
            GameObject enemy = new GameObject(name);
            enemy.transform.SetParent(parent, false);
            enemy.transform.position = pos;
            enemy.transform.localScale = Vector3.one * scale;

            // Body
            GameObject body = new GameObject("Body");
            body.transform.SetParent(enemy.transform, false);
            SpriteRenderer bSr = body.AddComponent<SpriteRenderer>();
            bSr.sprite = sprite;
            bSr.color = Color.white;
            bSr.sortingOrder = sortingOrder;

            // Orange Ground Ring
            if (ringSprite != null)
            {
                GameObject ring = new GameObject("EnemyRing");
                ring.transform.SetParent(enemy.transform, false);
                ring.transform.localPosition = new Vector3(0f, -0.25f, 0f);
                ring.transform.localScale = new Vector3(1.15f, 0.75f, 1f);
                SpriteRenderer rSr = ring.AddComponent<SpriteRenderer>();
                rSr.sprite = ringSprite;
                rSr.color = Color.white;
                rSr.sortingOrder = -20;
            }

            // Health Bar
            CreateUnitHealthBar(enemy.transform, healthPercent, isAllied: false);

            return enemy;
        }

        private static void CreateUnitHealthBar(Transform parent, float percent, bool isAllied)
        {
            GameObject hb = new GameObject("HealthBar");
            hb.transform.SetParent(parent, false);
            hb.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            // BG
            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(hb.transform, false);
            SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = GetBoxSprite();
            bgSr.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
            bgSr.sortingOrder = 998;
            bg.transform.localScale = new Vector3(0.65f, 0.08f, 1f);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(hb.transform, false);
            fill.transform.localPosition = new Vector3(-0.325f * (1f - percent), 0f, 0f);
            SpriteRenderer fSr = fill.AddComponent<SpriteRenderer>();
            fSr.sprite = GetBoxSprite();
            fSr.color = isAllied ? new Color(0.2f, 0.88f, 0.35f, 1f) : new Color(0.95f, 0.25f, 0.20f, 1f);
            fSr.sortingOrder = 999;
            fill.transform.localScale = new Vector3(0.63f * percent, 0.06f, 1f);
        }

        private static void CreateFloatingText(string name, Transform parent, Vector3 pos, string text, Color color, TMP_FontAsset font, float fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var tmp = go.AddComponent<TextMeshPro>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color32(20, 10, 5, 255);
            tmp.sortingOrder = 1500;
        }

        private static Sprite cachedBoxSprite;
        private static Sprite GetBoxSprite()
        {
            if (cachedBoxSprite != null) return cachedBoxSprite;
            Texture2D tex = new Texture2D(4, 4);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            cachedBoxSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return cachedBoxSprite;
        }

        public static string InspectMockupRoot()
        {
            var root = GameObject.Find("MockupBattlefieldRoot");
            if (root == null) return "Root NOT FOUND";
            var sb = new System.Text.StringBuilder();
            foreach (Transform child in root.transform)
            {
                var srs = child.GetComponentsInChildren<SpriteRenderer>(true);
                sb.Append(child.name + " (pos=" + child.position + "): ");
                foreach (var sr in srs)
                {
                    sb.Append("[" + sr.gameObject.name + ": sp=" + (sr.sprite != null ? sr.sprite.name : "NULL") + " order=" + sr.sortingOrder + " layer=" + sr.gameObject.layer + "] ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string InspectCards()
        {
            var sb = new System.Text.StringBuilder();
            var deck = Object.FindFirstObjectByType<WorldOfThePack.Game.CardDeckManager>();
            sb.AppendLine("DeckManager found: " + (deck != null));
            var cards = Object.FindObjectsByType<WorldOfThePack.UI.HeroDeploymentCard>(FindObjectsSortMode.None);
            sb.AppendLine("Cards found count: " + cards.Length);
            foreach (var c in cards)
            {
                sb.AppendLine(c.gameObject.name + ": energy=" + c.Energy + ", isReady=" + c.IsReady + ", label=" + (c.energyLabel != null ? c.energyLabel.text : "null") + ", actionSprite=" + (c.actionArtwork != null && c.actionArtwork.sprite != null ? c.actionArtwork.sprite.name : "null"));
            }
            return sb.ToString();
        }

        public static void DeploySlot(int index)
        {
            var cards = Object.FindObjectsByType<WorldOfThePack.UI.HeroDeploymentCard>(FindObjectsSortMode.None);
            foreach (var c in cards)
            {
                if (c.gameObject.name.StartsWith("CardSlot_" + index))
                {
                    c.deployRequested.Invoke(c.definition);
                    Debug.Log("Invoked deployment for card slot " + index);
                    return;
                }
            }
        }

        public static void TriggerWave5Boss()
        {
            var wm = Object.FindFirstObjectByType<WorldOfThePack.Game.WaveManager>();
            if (wm != null)
            {
                wm.JumpToWave(4);
                Debug.Log("Jumped to Wave 05 Boss Encounter!");
            }
        }

        public static void CycleFormation()
        {
            var fc = Object.FindFirstObjectByType<WorldOfThePack.Game.FormationController>();
            if (fc != null) fc.CycleFormation();
        }

        public static void ToggleMoonshift()
        {
            var deck = Object.FindFirstObjectByType<WorldOfThePack.Game.CardDeckManager>();
            if (deck != null) deck.ToggleAllForms();
        }

        public static string ListRoots()
        {
            var sb = new System.Text.StringBuilder();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            foreach (var r in roots)
            {
                sb.AppendLine(r.name + " (active=" + r.activeSelf + ")");
            }
            return sb.ToString();
        }
    }
}
