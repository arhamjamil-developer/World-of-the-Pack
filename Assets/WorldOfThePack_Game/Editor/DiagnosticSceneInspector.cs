using UnityEngine;
using UnityEditor;
using WorldOfThePack.Game;

namespace WorldOfThePack.EditorTools
{
    public static class DiagnosticSceneInspector
    {
        public static string Inspect()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== DIAGNOSTIC SCENE INSPECTOR ===");
            var heroes = Object.FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            sb.AppendLine($"Found {heroes.Length} HeroUnits in scene.");
            foreach (var h in heroes)
            {
                var srs = h.GetComponentsInChildren<SpriteRenderer>(true);
                sb.AppendLine($"Hero: {h.gameObject.name}, activeSelf: {h.gameObject.activeSelf}, pos: {h.transform.position}, scale: {h.transform.localScale}, child SpriteRenderers: {srs.Length}");
                foreach (var sr in srs)
                {
                    sb.AppendLine($"   -> SR: {sr.gameObject.name}, activeInHierarchy: {sr.gameObject.activeInHierarchy}, enabled: {sr.enabled}, sprite: {(sr.sprite != null ? sr.sprite.name : "NULL")}, color: {sr.color}, sortingOrder: {sr.sortingOrder}, layer: {sr.gameObject.layer} ({LayerMask.LayerToName(sr.gameObject.layer)})");
                }
            }

            var enemies = Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            sb.AppendLine($"Found {enemies.Length} EnemyUnits in scene.");
            foreach (var e in enemies)
            {
                var srs = e.GetComponentsInChildren<SpriteRenderer>(true);
                sb.AppendLine($"Enemy: {e.gameObject.name}, activeSelf: {e.gameObject.activeSelf}, pos: {e.transform.position}, scale: {e.transform.localScale}");
                foreach (var sr in srs)
                {
                    sb.AppendLine($"   -> SR: {sr.gameObject.name}, activeInHierarchy: {sr.gameObject.activeInHierarchy}, enabled: {sr.enabled}, sprite: {(sr.sprite != null ? sr.sprite.name : "NULL")}, color: {sr.color}, sortingOrder: {sr.sortingOrder}, layer: {sr.gameObject.layer} ({LayerMask.LayerToName(sr.gameObject.layer)})");
                }
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                sb.AppendLine($"Camera: pos: {cam.transform.position}, orthographicSize: {cam.orthographicSize}, cullingMask: {cam.cullingMask:X}, clearFlags: {cam.clearFlags}");
            }
            return sb.ToString();
        }
    }
}
