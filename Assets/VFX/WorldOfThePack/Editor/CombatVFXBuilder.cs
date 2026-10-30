using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.U2D;

namespace WorldOfThePack.VFX.Editor
{
    [InitializeOnLoad]
    public static class CombatVFXBuilder
    {
        public const string Root = "Assets/VFX/WorldOfThePack";
        private const string Pending = "Pack.CombatVFX.Verify";
        [Serializable] private sealed class Item { public string group, name, description; public int frames, fps, sortingOrder; }
        [Serializable] private sealed class Items { public Item[] items; }
        [Serializable] private sealed class Audit
        {
            public int sprites, definitions, prefabs, animationClips;
            public bool shaderSupported, shaderHasNoErrors, allFramesAssigned, enteredPlayMode, frameAdvanced, pausePreservesFrame, completedOnce, completedEffectCannotResume, pooledReplayStartsAtZero, pooledReplayCompleted, staticMarkerPersists, iphoneCapture, ipadCapture, forestAlphaCapture, feedbackCapture;
            public string[] runtimeErrors;
            public string firstCompletionState, replayCompletionState;
            public string note = "Editor Play Mode and offscreen portrait captures; physical-device performance has not been measured. Six-frame generated sequences require final animation polish against combat timings. No damage, navigation or energy systems were changed.";
        }
        private static Audit audit;
        private static readonly List<string> Errors = new List<string>();
        private static double next;
        private static double completionDeadline;
        private static int phase, pausedFrame;
        private static SpriteFlipbookEffect probe;
        private static string QA => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Design/CombatVFX/QA");
        static CombatVFXBuilder()
        {
            EditorApplication.update += VerifyUpdate;
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Pending, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) { phase = 0; next = EditorApplication.timeSinceStartup + 1; }
                if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool(Pending, false);
            };
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (SessionState.GetBool(Pending, false) && EditorApplication.isPlaying && (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)) Errors.Add(message + "\n" + trace);
            };
        }
        [MenuItem("World of the Pack/Combat VFX/Build Assets")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Build in Edit Mode.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the active scene before creating the separate VFX showcase.");
            Directory.CreateDirectory(QA);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var items = JsonUtility.FromJson<Items>("{\"items\":" + File.ReadAllText(Root + "/Data/sequence-manifest.json") + "}").items;
            foreach (string path in Directory.GetFiles(Root + "/Art", "*.png", SearchOption.AllDirectories))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            Shader shader = Shader.Find("WorldOfThePack/SpriteFeedback");
            if (!shader) throw new InvalidOperationException("Sprite feedback shader missing.");
            var alpha = MaterialAsset("SoftAlpha", shader);
            var additive = MaterialAsset("SoftAdditive", shader); additive.SetFloat("_DstBlend", 1); EditorUtility.SetDirty(additive);
            var ground = MaterialAsset("GroundWarning", shader); ground.SetFloat("_Opacity", .72f); EditorUtility.SetDirty(ground);
            MaterialAsset("CharacterFeedback", shader);
            var prefabs = new List<GameObject>();
            foreach (var item in items)
            {
                Directory.CreateDirectory(Root + "/Data/" + item.group);
                Directory.CreateDirectory(Root + "/Prefabs/" + item.group);
                Directory.CreateDirectory(Root + "/Animations/" + item.group);
                string stem = item.group + "/" + item.name;
                var data = AssetDatabase.LoadAssetAtPath<CombatEffectDefinition>(Root + "/Data/" + stem + ".asset");
                if (!data) { data = ScriptableObject.CreateInstance<CombatEffectDefinition>(); AssetDatabase.CreateAsset(data, Root + "/Data/" + stem + ".asset"); }
                data.group = item.group; data.description = item.description; data.framesPerSecond = item.fps; data.sortingOrder = item.sortingOrder; data.staticMarker = item.frames == 1;
                data.frames = Enumerable.Range(0, item.frames).Select(i => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/" + stem + "_" + i.ToString("00") + ".png")).ToArray();
                if (!data.staticMarker)
                {
                    string clipPath = Root + "/Animations/" + stem + ".anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (!clip) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
                    clip.frameRate = item.fps;
                    var keys = new List<ObjectReferenceKeyframe>();
                    for (int i = 0; i < data.frames.Length; i++) keys.Add(new ObjectReferenceKeyframe { time = (float)i / item.fps, value = data.frames[i] });
                    keys.Add(new ObjectReferenceKeyframe { time = (float)item.frames / item.fps, value = null });
                    AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys.ToArray());
                    data.animationClip = clip;
                }
                EditorUtility.SetDirty(data);
                var go = new GameObject(item.name);
                var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = data.frames[0]; renderer.sortingOrder = item.sortingOrder; renderer.sharedMaterial = item.group == "Telegraphs" ? ground : alpha;
                var fx = go.AddComponent<SpriteFlipbookEffect>(); fx.definition = data; fx.deactivateOnComplete = !data.staticMarker;
                if (item.name == "EnemyConeSouth") go.transform.rotation = Quaternion.Euler(0, 0, 180);
                prefabs.Add(PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/" + stem + ".prefab"));
                UnityEngine.Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();
            BuildAtlas();
            CreateShowcase(prefabs);
            Debug.Log("PACK_COMBAT_VFX_BUILT: " + prefabs.Count + " effect prefabs");
        }
        private static Material MaterialAsset(string name, Shader shader)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            mat.shader = shader; mat.enableInstancing = true; return mat;
        }
        private static void CreateShowcase(List<GameObject> prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Portrait Camera").AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 12.2f; camera.transform.position = new Vector3(0, 0, -10); camera.backgroundColor = new Color(.055f,.11f,.085f); camera.clearFlags = CameraClearFlags.SolidColor;
            var showcase = new GameObject("Combat VFX Catalogue - isolated art preview").AddComponent<CombatVFXShowcase>();
            var list = new List<SpriteFlipbookEffect>();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Dynamic.asset");
            Label("WORLD OF THE PACK", new Vector3(0,11.2f,0), 4.5f, font, new Color(1,.83f,.4f), 10);
            Label("COMBAT VFX / ART PREVIEW", new Vector3(0,10.55f,0), 2.5f, font, Color.white, 10);
            for (int i = 0; i < prefabs.Count; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i]);
                go.transform.position = new Vector3((i % 4 - 1.5f) * 2.35f, 9.05f - (i / 4) * 2.03f, 0);
                go.transform.localScale = Vector3.one * .94f;
                list.Add(go.GetComponent<SpriteFlipbookEffect>());
                Label(go.name, go.transform.position + new Vector3(0,-.88f,0), 1.55f, font, new Color(.87f,.93f,.88f));
            }
            showcase.effects = list.ToArray();
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/CombatVFXShowcase.unity");
        }
        private static void Label(string text, Vector3 position, float size, TMP_FontAsset font, Color color, float width = 2.3f)
        {
            var label = new GameObject(text + " Label").AddComponent<TextMeshPro>();
            label.text = text; label.font = font; label.fontSize = size; label.alignment = TextAlignmentOptions.Center; label.color = color; label.transform.position = position; label.rectTransform.sizeDelta = new Vector2(width, .7f); label.overflowMode = TextOverflowModes.Overflow; label.GetComponent<MeshRenderer>().sortingOrder = 100;
        }
        [MenuItem("World of the Pack/Combat VFX/Verify Showcase")]
        public static void Verify()
        {
            if (!UnityEngine.Object.FindAnyObjectByType<CombatVFXShowcase>()) throw new InvalidOperationException("Open CombatVFXShowcase first.");
            Errors.Clear(); SessionState.SetBool(Pending, true); EditorApplication.isPlaying = true;
        }
        private static void VerifyUpdate()
        {
            if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying) return;
            EditorApplication.QueuePlayerLoopUpdate();
            if (EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
            try
            {
                var showcase = UnityEngine.Object.FindAnyObjectByType<CombatVFXShowcase>();
                if (!showcase) return;
                if (phase == 0)
                {
                    var defs = AssetDatabase.FindAssets("t:CombatEffectDefinition", new[] { Root }).Select(g => AssetDatabase.LoadAssetAtPath<CombatEffectDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
                    var shader = Shader.Find("WorldOfThePack/SpriteFeedback");
                    audit = new Audit { sprites = defs.Sum(d => d.frames.Length), definitions = defs.Length, prefabs = Directory.GetFiles(Root + "/Prefabs", "*.prefab", SearchOption.AllDirectories).Length, animationClips = defs.Count(d => d.animationClip), shaderSupported = shader.isSupported, shaderHasNoErrors = !ShaderUtil.ShaderHasError(shader), allFramesAssigned = defs.All(d => d.frames.All(s => s)), enteredPlayMode = true };
                    showcase.HoldFrame(2);
                    Capture("combat-vfx-iphone.png", 1170, 2532); audit.iphoneCapture = true;
                    Capture("combat-vfx-ipad.png", 1536, 2048); audit.ipadCapture = true;
                    CaptureForestAndFeedback(showcase); audit.forestAlphaCapture = true; audit.feedbackCapture = true;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/HeroWolvy/MoonlightSlash.prefab");
                    probe = UnityEngine.Object.Instantiate(prefab).GetComponent<SpriteFlipbookEffect>(); probe.transform.position = Vector3.one * 50; probe.deactivateOnComplete = false;
                    // Slow only the disposable test definition so a stalled editor cannot finish before the pause assertion.
                    probe.definition = UnityEngine.Object.Instantiate(probe.definition); probe.definition.framesPerSecond = 4; probe.Play();
                    next = EditorApplication.timeSinceStartup + .1; completionDeadline = EditorApplication.timeSinceStartup + 20; phase++;
                }
                else if (phase == 1)
                {
                    if (probe.CurrentFrame == 0 && EditorApplication.timeSinceStartup < completionDeadline) return;
                    audit.frameAdvanced = probe.CurrentFrame > 0; probe.Pause(); pausedFrame = probe.CurrentFrame; next = EditorApplication.timeSinceStartup + .2; phase++;
                }
                else if (phase == 2)
                {
                    audit.pausePreservesFrame = probe.CurrentFrame == pausedFrame; probe.Resume(); next = EditorApplication.timeSinceStartup + .1; completionDeadline = EditorApplication.timeSinceStartup + 20; phase++;
                }
                else if (phase == 3)
                {
                    if (probe.IsPlaying && EditorApplication.timeSinceStartup < completionDeadline) return;
                    audit.completedOnce = probe.CompletedCount == 1 && !probe.IsPlaying && !probe.GetComponent<SpriteRenderer>().enabled;
                    audit.firstCompletionState = "completed=" + probe.CompletedCount + ", playing=" + probe.IsPlaying + ", renderer=" + probe.GetComponent<SpriteRenderer>().enabled + ", frame=" + probe.CurrentFrame;
                    probe.Resume(); audit.completedEffectCannotResume = !probe.IsPlaying && probe.CompletedCount == 1;
                    probe.gameObject.SetActive(false); probe.gameObject.SetActive(true); audit.pooledReplayStartsAtZero = probe.CurrentFrame == 0 && probe.IsPlaying;
                    probe.deactivateOnComplete = true; next = EditorApplication.timeSinceStartup + .1; completionDeadline = EditorApplication.timeSinceStartup + 20; phase++;
                }
                else
                {
                    if (probe.IsPlaying && EditorApplication.timeSinceStartup < completionDeadline) return;
                    audit.pooledReplayCompleted = probe.CompletedCount == 2 && !probe.gameObject.activeSelf;
                    audit.replayCompletionState = "completed=" + probe.CompletedCount + ", playing=" + probe.IsPlaying + ", active=" + probe.gameObject.activeSelf + ", frame=" + probe.CurrentFrame;
                    audit.staticMarkerPersists = showcase.effects.Where(e => e.definition.staticMarker).All(e => e.gameObject.activeSelf && e.GetComponent<SpriteRenderer>().enabled && !e.IsPlaying);
                    audit.runtimeErrors = Errors.ToArray(); Directory.CreateDirectory(QA); File.WriteAllText(Path.Combine(QA, "unity-vfx-audit.json"), JsonUtility.ToJson(audit, true));
                    SessionState.SetBool(Pending, false); EditorApplication.isPlaying = false; Debug.Log("PACK_COMBAT_VFX_VERIFICATION_FINISHED");
                }
            }
            catch (Exception e) { Debug.LogException(e); SessionState.SetBool(Pending, false); EditorApplication.isPlaying = false; }
        }
        private static void Capture(string name, int width, int height)
        {
            var camera = Camera.main ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();
            var previous = RenderTexture.active; var target = RenderTexture.GetTemporary(width, height, 24); var old = camera.targetTexture;
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGBA32, false); image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply(); File.WriteAllBytes(Path.Combine(QA,name),image.EncodeToPNG());
            camera.targetTexture = old; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(image);
        }
        private static void CaptureForestAndFeedback(CombatVFXShowcase showcase)
        {
            var camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-stage-master-v01.png");
            var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "Temporary forest alpha inspection surface";
            background.transform.position = new Vector3(0,0,3);
            background.transform.localScale = new Vector3(17,51,1);
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); material.SetTexture("_BaseMap", texture);
            background.GetComponent<MeshRenderer>().sharedMaterial = material;
            Capture("forest-alpha-inspection-iphone.png",1170,2532);
            foreach (var effect in showcase.effects) effect.gameObject.SetActive(false);
            var originalLabels = UnityEngine.Object.FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None);
            foreach (var label in originalLabels) label.gameObject.SetActive(false);
            camera.orthographicSize = 4.5f;
            var portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/WorldOfThePack/Art/Portraits/HeroWolvy_Wolf.png");
            if (!portrait) portrait = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/HeroWolvy/MoonlightSlash_02.png");
            var feedback = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/CharacterFeedback.mat");
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Dynamic.asset");
            var samples = new List<GameObject>();
            string[] names = { "NORMAL", "HIT FLASH", "DISSOLVE" };
            for (int i = 0; i < 3; i++)
            {
                var sample = new GameObject(names[i]); samples.Add(sample); sample.transform.position = new Vector3((i-1)*3,0,0); var renderer = sample.AddComponent<SpriteRenderer>(); renderer.sprite = portrait; renderer.sharedMaterial = feedback; renderer.sortingOrder = 30;
                var block = new MaterialPropertyBlock(); block.SetFloat("_FlashAmount", i == 1 ? .75f : 0); block.SetColor("_FlashColor",new Color(1,1,.7f)); block.SetFloat("_Dissolve",i == 2 ? .55f : 0); renderer.SetPropertyBlock(block);
                Label(names[i], sample.transform.position + new Vector3(0,-1.6f,0), 3, font, Color.white);
            }
            Capture("sprite-feedback-inspection.png",1536,1024);
            foreach (var sample in samples) UnityEngine.Object.Destroy(sample);
            foreach (var label in UnityEngine.Object.FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(label.gameObject);
            foreach (var label in originalLabels) label.gameObject.SetActive(true);
            UnityEngine.Object.Destroy(background); UnityEngine.Object.Destroy(material);
            camera.orthographicSize = 12.2f;
            showcase.HoldFrame(2);
        }
        [MenuItem("World of the Pack/Combat VFX/Export Package")]
        public static void Export()
        {
            Directory.CreateDirectory("Exports");
            AssetDatabase.ExportPackage(new[] { Root, "Assets/UI/WorldOfThePack/Fonts", "Assets/UI/WorldOfThePack/Art/Portraits/HeroWolvy_Wolf.png", "Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-stage-master-v01.png" }, "Exports/WorldOfThePack-CombatVFX-v01.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log("PACK_COMBAT_VFX_EXPORTED");
        }
        [MenuItem("World of the Pack/Combat VFX/Build Sprite Atlas")]
        public static void BuildAtlas()
        {
            string path = Root + "/Art/CombatVFX.spriteatlas";
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (!atlas) { atlas = new SpriteAtlas(); AssetDatabase.CreateAsset(atlas,path); }
            var old = atlas.GetPackables(); if (old.Length > 0) atlas.Remove(old);
            // Pack explicit textures so the atlas asset does not recursively pack its own folder.
            atlas.Add(Directory.GetFiles(Root + "/Art","*.png",SearchOption.AllDirectories).Select(p => AssetDatabase.LoadAssetAtPath<Texture2D>(p.Replace('\\','/'))).Cast<UnityEngine.Object>().ToArray());
            atlas.SetPackingSettings(new SpriteAtlasPackingSettings { padding = 4, enableRotation = false, enableTightPacking = false });
            atlas.SetTextureSettings(new SpriteAtlasTextureSettings { generateMipMaps = false, readable = false, sRGB = true, filterMode = FilterMode.Bilinear });
            var platform = atlas.GetPlatformSettings("DefaultTexturePlatform"); platform.maxTextureSize = 2048; platform.textureCompression = TextureImporterCompression.CompressedHQ; atlas.SetPlatformSettings(platform);
            atlas.SetIncludeInBuild(true); EditorUtility.SetDirty(atlas); AssetDatabase.SaveAssets();
        }
        [MenuItem("World of the Pack/Combat VFX/Rebuild Showcase Only")]
        public static void RebuildShowcase()
        {
            if (EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene and leave Play Mode before rebuilding the preview.");
            var items = JsonUtility.FromJson<Items>("{\"items\":" + File.ReadAllText(Root + "/Data/sequence-manifest.json") + "}").items;
            CreateShowcase(items.Select(i => AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/" + i.group + "/" + i.name + ".prefab")).ToList());
        }
    }
}
