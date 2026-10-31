using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.EditorTools
{
    public static class InspectSceneUI
    {
        [MenuItem("World of the Pack/Debug/Dump HUD Hierarchy")]
        public static string Dump()
        {
            var hud = GameObject.Find("GameHUDCanvas");
            if (hud == null) return "GameHUDCanvas not found in scene!";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Dumping GameHUDCanvas:");
            PrintHierarchy(hud.transform, "", sb);
            return sb.ToString();
        }

        public static string CheckBackground()
        {
            var cam = Camera.main;
            var c0 = GameObject.Find("CardSlot_0_HeroWolvy");
            var tray = GameObject.Find("CardHandTray");
            var canvas = GameObject.Find("GameHUDCanvas").GetComponent<Canvas>();
            var scaler = GameObject.Find("GameHUDCanvas").GetComponent<CanvasScaler>();
            var cr = canvas.GetComponent<RectTransform>();
            return $"Screen: ({Screen.width}, {Screen.height}), Cam: ortho={cam.orthographicSize}, CanvasRect: {cr.rect.size}, Scaler: mode={scaler.uiScaleMode}, refRes={scaler.referenceResolution}, match={scaler.matchWidthOrHeight}";
        }

        public static string CheckGameplay()
        {
            var heroes = Object.FindObjectsByType<WorldOfThePack.Game.HeroUnit>(FindObjectsSortMode.None);
            var enemies = Object.FindObjectsByType<WorldOfThePack.Game.EnemyUnit>(FindObjectsSortMode.None);
            var boss = Object.FindObjectsByType<WorldOfThePack.Game.BossUnit>(FindObjectsSortMode.None);
            var gd = Object.FindFirstObjectByType<WorldOfThePack.Game.GameDirector>();
            var wm = Object.FindFirstObjectByType<WorldOfThePack.Game.WaveManager>();
            var fc = Object.FindFirstObjectByType<WorldOfThePack.Game.FormationController>();
            return $"Heroes: {heroes.Length}, Enemies: {enemies.Length}, Bosses: {boss.Length}, GameDirector: {gd != null}, WaveManager: {wm != null}, FormationController: {fc != null}";
        }

        public static string CheckCanvasStatus()
        {
            var go = GameObject.Find("GameHUDCanvas");
            if (go == null) return "Canvas GO null";
            var c = go.GetComponent<Canvas>();
            var cg = go.GetComponent<CanvasGroup>();
            var cam = Camera.main;
            string camMask = cam != null ? cam.cullingMask.ToString() : "no cam";
            int uiLayer = LayerMask.NameToLayer("UI");
            bool hasUI = cam != null && ((cam.cullingMask & (1 << uiLayer)) != 0);
            return $"active: {go.activeInHierarchy}, enabled: {(c != null && c.enabled)}, renderMode: {(c != null ? c.renderMode.ToString() : "null")}, order: {(c != null ? c.sortingOrder : 0)}, alpha: {(cg != null ? cg.alpha : 1f)}, camMask: {camMask}, hasUILayer: {hasUI}";
        }

        private static void PrintHierarchy(Transform t, string indent, System.Text.StringBuilder sb)
        {
            string comps = "";
            foreach (var c in t.GetComponents<Component>())
            {
                if (c != null) comps += c.GetType().Name + ", ";
            }
            sb.AppendLine($"{indent}- {t.name} [{comps}]");
            foreach (Transform child in t)
            {
                PrintHierarchy(child, indent + "  ", sb);
            }
        }
    }
}
