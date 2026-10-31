using UnityEditor;
using UnityEngine;

namespace WorldOfThePack.EditorTools
{
    public static class InspectCanvas
    {
        [MenuItem("World of the Pack/Debug/Log Card Info")]
        public static void LogCardInfo()
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/WorldOfThePack/Prefabs/CardPortrait.prefab");
            if (go == null) { Debug.LogError("CardPortrait.prefab not found"); return; }
            var rt = go.GetComponent<RectTransform>();
            Debug.Log($"CARD_PREFAB: sizeDelta={rt.sizeDelta}, rect={rt.rect}, pivot={rt.pivot}");
            foreach (Transform c in go.transform)
            {
                var crt = c.GetComponent<RectTransform>();
                Debug.Log($"CHILD {c.name}: anchoredPos={crt.anchoredPosition}, sizeDelta={crt.sizeDelta}, anchorMin={crt.anchorMin}, anchorMax={crt.anchorMax}, pivot={crt.pivot}");
            }
        }
    }
}
