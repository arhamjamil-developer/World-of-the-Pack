using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering;
using WorldOfThePack.CharacterArt;

[InitializeOnLoad]
public static class PackCharacterAssetBuilder
{
    private const string Root = "Assets/Characters";
    private const string Pending = "PackCharacterArt.VerificationPending";
    private static readonly string[] Heroes = { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" };
    private static readonly string[] Actions = { "Idle", "Walk", "Attack", "Dead", "Special01", "Special02" };
    private static readonly float[] Rates = { 6, 8, 12, 8, 10, 10 };
    private static readonly HashSet<string> Observed = new HashSet<string>();
    private static readonly HashSet<string> ObservedFrames = new HashSet<string>();
    private static readonly List<string> RuntimeErrors = new List<string>();
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
    private static string QA => Path.Combine(ProjectRoot, "Design/CharacterPipeline/QA");

    [Serializable] private sealed class Audit { public int sheets, sprites, bodyClips, effectClips, prefabs; public bool importVerified, playModeVerified; public string[] observedActions, runtimeErrors; public int observedFrames; public string note; }
    private static Audit audit;

    static PackCharacterAssetBuilder()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += VerifyUpdate;
        Application.logMessageReceived += OnLog;
    }

    [MenuItem("World of the Pack/Character Art/Build and Verify")]
    public static void BuildAndVerify()
    {
        try
        {
            Build();
            SessionState.SetBool(Pending, true);
            SessionState.SetBool(Pending + ".started", false);
            SessionState.SetBool(Pending + ".finishing", false);
            SessionState.SetBool(Pending + ".batch", Application.isBatchMode);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Directory.CreateDirectory(QA);
            File.WriteAllText(Path.Combine(QA, "failure.txt"), exception.ToString());
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
    }

    [MenuItem("World of the Pack/Character Art/Rebuild Assets")]
    public static void Build()
    {
        Directory.CreateDirectory(QA);
        audit = new Audit();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var prefabs = new List<GameObject>();
        foreach (string hero in Heroes)
        {
            var effects = ImportSheet(hero, "VFX", 3);
            for (int row = 0; row < 3; row++)
            {
                var clip = new AnimationClip { name = hero + "_VFX_" + (row == 0 ? "Attack" : Actions[row + 3]), frameRate = 12 };
                SetSpriteCurve(clip, "", effects.Skip(row * 6).Take(6).ToArray(), 12, false, true);
                SaveClip(clip, Root + "/" + hero + "/Animations/" + clip.name + ".anim");
                audit.effectClips++;
            }
            foreach (string form in new[] { "Human", "Wolf" })
            {
                int count = form == "Human" ? 4 : 6;
                var sprites = ImportSheet(hero, form, count);
                string animationFolder = Root + "/" + hero + "/Animations";
                string controllerPath = animationFolder + "/" + hero + "_" + form + ".controller";
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null) AssetDatabase.DeleteAsset(controllerPath);
                var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("Action", AnimatorControllerParameterType.Int);
                var machine = controller.layers[0].stateMachine;
                for (int row = 0; row < count; row++)
                {
                    bool loop = row < 2;
                    var clip = new AnimationClip { name = hero + "_" + form + "_" + Actions[row] + "_North", frameRate = Rates[row] };
                    SetSpriteCurve(clip, "Body", sprites.Skip(row * 6).Take(6).ToArray(), Rates[row], loop, false);
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = loop;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    if (form == "Wolf")
                    {
                        int effectRow = row == 2 ? 0 : row == 4 ? 1 : row == 5 ? 2 : -1;
                        var keys = new List<ObjectReferenceKeyframe> { new ObjectReferenceKeyframe { time = 0, value = null } };
                        if (effectRow >= 0)
                        {
                            for (int frame = 0; frame < 6; frame++) keys.Add(new ObjectReferenceKeyframe { time = (frame + 2) / Rates[row], value = effects[effectRow * 6 + frame] });
                            keys.Add(new ObjectReferenceKeyframe { time = 8 / Rates[row], value = null });
                        }
                        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = "VFX", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys.ToArray());
                    }
                    SaveClip(clip, animationFolder + "/" + clip.name + ".anim");
                    var state = machine.AddState(Actions[row]);
                    state.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationFolder + "/" + clip.name + ".anim");
                    if (row == 0) machine.defaultState = state;
                    var transition = machine.AddAnyStateTransition(state);
                    transition.hasExitTime = false;
                    transition.duration = 0;
                    transition.canTransitionToSelf = false;
                    transition.AddCondition(AnimatorConditionMode.Equals, row, "Action");
                    audit.bodyClips++;
                }
                var actor = new GameObject(hero + "_" + form);
                var body = new GameObject("Body").AddComponent<SpriteRenderer>();
                body.transform.SetParent(actor.transform, false);
                body.sprite = sprites[0];
                body.sortingOrder = 10;
                if (form == "Wolf")
                {
                    var effect = new GameObject("VFX").AddComponent<SpriteRenderer>();
                    effect.transform.SetParent(actor.transform, false);
                    effect.transform.localPosition = new Vector3(0, 1.15f, 0);
                    effect.sortingOrder = 11;
                }
                actor.AddComponent<Animator>().runtimeAnimatorController = controller;
                string path = Root + "/" + hero + "/Prefabs/" + actor.name + "_North.prefab";
                PrefabUtility.SaveAsPrefabAsset(actor, path);
                UnityEngine.Object.DestroyImmediate(actor);
                prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                audit.prefabs++;
            }
        }
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.defaultScreenWidth = 1170;
        PlayerSettings.defaultScreenHeight = 2532;
        CreatePreview(prefabs);
        audit.importVerified = audit.sheets == 21 && audit.sprites == 546 && audit.bodyClips == 70 && audit.effectClips == 21 && audit.prefabs == 14;
        if (!audit.importVerified) throw new InvalidOperationException("Asset inventory did not match the requested roster.");
        audit.note = "One north-facing art set. Editor renders are aspect-ratio checks, not physical iPhone/iPad validation. Generated animation timing and anatomy still require visual art review.";
        WriteAudit();
        AssetDatabase.SaveAssets();
        Debug.Log("PACK_CHARACTER_BUILD_OK: " + JsonUtility.ToJson(audit));
    }

    private static Sprite[] ImportSheet(string hero, string form, int rows)
    {
        string path = Root + "/" + hero + "/" + form + "/" + hero + "_" + form + "_North.png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new FileNotFoundException(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 128;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.isReadable = false;
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
        var rects = new List<SpriteRect>();
        for (int row = 0; row < rows; row++) for (int col = 0; col < 6; col++)
        {
            string action = form == "VFX" ? (row == 0 ? "Attack" : Actions[row + 3]) : Actions[row];
            string name = hero + "_" + form + "_" + action + "_North_" + col.ToString("00");
            var id = old.TryGetValue(name, out var existing) ? existing : StableId(name);
            rects.Add(new SpriteRect { name = name, rect = new Rect(col * 256, (rows - 1 - row) * 256, 256, 256), alignment = SpriteAlignment.Custom, pivot = form == "VFX" ? new Vector2(.5f, .5f) : new Vector2(.5f, .1875f), spriteID = id });
        }
        provider.SetSpriteRects(rects.ToArray());
        var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var map = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s => s.name);
        var sorted = rects.Select(r => map[r.name]).ToArray();
        if (sorted.Length != rows * 6 || sorted.Any(s => s.rect.width != 256 || s.rect.height != 256)) throw new InvalidOperationException("Invalid slicing: " + path);
        var raw = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        raw.LoadImage(File.ReadAllBytes(Path.Combine(ProjectRoot, path)));
        if (raw.width != 1536 || raw.height != rows * 256 || !raw.GetPixels32().Any(p => p.a == 0)) throw new InvalidOperationException("Invalid size or alpha: " + path);
        UnityEngine.Object.DestroyImmediate(raw);
        audit.sheets++; audit.sprites += sorted.Length;
        return sorted;
    }

    private static GUID StableId(string name)
    {
        using (var md5 = MD5.Create()) return new GUID(BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(name))).Replace("-", "").ToLowerInvariant());
    }

    private static void SetSpriteCurve(AnimationClip clip, string path, Sprite[] frames, float fps, bool loop, bool clear)
    {
        var keys = new List<ObjectReferenceKeyframe>();
        for (int i = 0; i < frames.Length; i++) keys.Add(new ObjectReferenceKeyframe { time = i / fps, value = frames[i] });
        keys.Add(new ObjectReferenceKeyframe { time = frames.Length / fps, value = clear ? null : loop ? frames[0] : frames[frames.Length - 1] });
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = path, type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys.ToArray());
    }

    private static void SaveClip(AnimationClip clip, string path)
    {
        var old = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (old == null) AssetDatabase.CreateAsset(clip, path);
        else { EditorUtility.CopySerialized(clip, old); UnityEngine.Object.DestroyImmediate(clip); }
    }

    private static void CreatePreview(List<GameObject> prefabs)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Character Art Camera").AddComponent<Camera>();
        camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 9.5f;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.13f, .27f, .20f);
        Text("NORTH-FACING CHARACTER ART", new Vector3(0, 8.5f), .30f);
        Text("HUMAN                      WOLF", new Vector3(0, 7.8f), .24f);
        var actors = new List<Animator>();
        for (int i = 0; i < Heroes.Length; i++)
        {
            float y = 5.6f - i * 1.85f;
            Text(Heroes[i], new Vector3(0, y + 1.45f), .19f);
            for (int form = 0; form < 2; form++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i * 2 + form]);
                instance.transform.position = new Vector3(form == 0 ? -1.5f : 1.5f, y, 0);
                actors.Add(instance.GetComponent<Animator>());
            }
        }
        Text("Art preview only | Same 256px cells / 128 PPU", new Vector3(0, -8), .20f);
        var preview = new GameObject("Animation Review").AddComponent<PackCharacterAnimationPreview>();
        preview.actors = actors.ToArray();
        Directory.CreateDirectory(Path.Combine(ProjectRoot, Root, "_Preview"));
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Root + "/_Preview/CharacterAnimationPreview.unity");
        // A temporary built-in render path makes the offline evidence independent of URP lighting.
        var originalGraphics = GraphicsSettings.defaultRenderPipeline;
        var originalQuality = QualitySettings.renderPipeline;
        try
        {
            GraphicsSettings.defaultRenderPipeline = null; QualitySettings.renderPipeline = null;
            foreach (string action in Actions)
            {
                foreach (var actor in actors)
                {
                    string form = actor.name.EndsWith("_Human") ? "Human" : "Wolf";
                    string hero = actor.name.Substring(0, actor.name.LastIndexOf('_'));
                    string available = form == "Human" && action.StartsWith("Special") ? "Idle" : action;
                    string path = Root + "/" + hero + "/Animations/" + hero + "_" + form + "_" + available + "_North.anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    clip.SampleAnimation(actor.gameObject, available == "Dead" ? clip.length : clip.length * .55f);
                }
                Render(camera, 1170, 2532, Path.Combine(QA, "iPhone_Portrait_" + action + ".png"));
                Render(camera, 1536, 2048, Path.Combine(QA, "iPad_Portrait_" + action + ".png"));
            }
        }
        finally { GraphicsSettings.defaultRenderPipeline = originalGraphics; QualitySettings.renderPipeline = originalQuality; }
        foreach (var actor in actors) actor.Rebind();
    }

    private static void Text(string value, Vector3 position, float size)
    {
        var text = new GameObject(value).AddComponent<TextMesh>();
        text.text = value; text.fontSize = 64; text.characterSize = size; text.anchor = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        text.color = new Color(.96f, .94f, .79f); text.transform.position = position;
    }

    private static void Render(Camera camera, int width, int height, string path)
    {
        var previous = RenderTexture.active;
        var render = RenderTexture.GetTemporary(width, height, 24);
        camera.targetTexture = render; camera.Render(); RenderTexture.active = render;
        var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
        File.WriteAllBytes(path, image.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(image); camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(render);
    }

    private static void OnLog(string message, string trace, LogType type)
    {
        if (SessionState.GetBool(Pending, false) && EditorApplication.isPlaying && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) RuntimeErrors.Add(message);
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Pending, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SessionState.SetBool(Pending + ".started", true);
            SessionState.SetFloat(Pending + ".time", (float)EditorApplication.timeSinceStartup);
        }
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Pending + ".started", false))
        {
            SessionState.SetBool(Pending, false);
            bool pass = SessionState.GetBool(Pending + ".pass", false);
            Debug.Log(pass ? "PACK_CHARACTER_PLAYMODE_OK" : "PACK_CHARACTER_PLAYMODE_FAILED");
            if (SessionState.GetBool(Pending + ".batch", false)) EditorApplication.Exit(pass ? 0 : 1);
        }
    }

    private static void VerifyUpdate()
    {
        if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying || !SessionState.GetBool(Pending + ".started", false) || SessionState.GetBool(Pending + ".finishing", false)) return;
        foreach (var renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (renderer.name != "Body" || renderer.sprite == null) continue;
            string sprite = renderer.sprite.name; ObservedFrames.Add(sprite);
            if (renderer.transform.parent.name.EndsWith("_Wolf")) foreach (string action in Actions) if (sprite.Contains("_" + action + "_")) Observed.Add(action);
        }
        if (EditorApplication.timeSinceStartup - SessionState.GetFloat(Pending + ".time", 0) < 12) return;
        string path = Path.Combine(QA, "unity-audit.json");
        audit = JsonUtility.FromJson<Audit>(File.ReadAllText(path));
        audit.observedActions = Observed.OrderBy(x => x).ToArray(); audit.observedFrames = ObservedFrames.Count; audit.runtimeErrors = RuntimeErrors.ToArray();
        audit.playModeVerified = Observed.Count == 6 && RuntimeErrors.Count == 0 && ObservedFrames.Count > 100;
        SessionState.SetBool(Pending + ".pass", audit.playModeVerified); SessionState.SetBool(Pending + ".finishing", true); WriteAudit(); EditorApplication.ExitPlaymode();
    }

    private static void WriteAudit() => File.WriteAllText(Path.Combine(QA, "unity-audit.json"), JsonUtility.ToJson(audit, true));
}
