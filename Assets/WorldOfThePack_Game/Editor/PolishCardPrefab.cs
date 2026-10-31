using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.EditorTools
{
    public static class PolishCardPrefab
    {
        [MenuItem("World of the Pack/UI/Polish All Card Prefabs")]
        public static void PolishAllCards()
        {
            string fontPath = "Assets/UI/WorldOfThePack/Fonts/LilitaOne-Dynamic.asset";
            string matPath = "Assets/UI/WorldOfThePack/Fonts/LilitaOne-Outlined.mat";

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            Material outlinedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (font == null || outlinedMat == null)
            {
                Debug.LogError("[PolishCardPrefab] Missing LilitaOne font or outlined material!");
                return;
            }

            string prefabsDir = "Assets/UI/WorldOfThePack/Prefabs";
            string[] files = Directory.GetFiles(prefabsDir, "*CardPortrait*.prefab");

            foreach (string file in files)
            {
                GameObject prefab = PrefabUtility.LoadPrefabContents(file);
                if (prefab == null) continue;

                PolishSinglePrefab(prefab, font, outlinedMat);

                PrefabUtility.SaveAsPrefabAsset(prefab, file);
                PrefabUtility.UnloadPrefabContents(prefab);
                Debug.Log($"[PolishCardPrefab] Polished typography for: {Path.GetFileName(file)}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void PolishSinglePrefab(GameObject root, TMP_FontAsset font, Material mat)
        {
            // 1. Hero Name
            Transform nameObj = root.transform.Find("Nameplate/HeroName");
            if (nameObj != null)
            {
                TextMeshProUGUI tmp = nameObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.font = font;
                    tmp.fontSharedMaterial = mat;
                    tmp.fontSize = 17f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = new Color(1.0f, 0.96f, 0.86f, 1f); // Warm parchment cream
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableWordWrapping = false;
                    tmp.overflowMode = TextOverflowModes.Ellipsis;
                }
            }

            // 2. Energy Text
            Transform energyObj = root.transform.Find("EnergyMeter/EditableEnergyText");
            if (energyObj != null)
            {
                TextMeshProUGUI tmp = energyObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.font = font;
                    tmp.fontSharedMaterial = mat;
                    tmp.fontSize = 13f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableWordWrapping = false;

                    // Ensure clean vertical centering inside the track
                    RectTransform rt = energyObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = new Vector2(2f, 0f);
                        rt.offsetMax = new Vector2(-2f, 0f);
                    }
                }
            }

            // 3. Deployment Button Label
            Transform btnLabelObj = root.transform.Find("DeploymentButton/EditableLabel");
            if (btnLabelObj != null)
            {
                TextMeshProUGUI tmp = btnLabelObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.font = font;
                    tmp.fontSharedMaterial = mat;
                    tmp.fontSize = 15f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableWordWrapping = false;
                }
            }
        }
    }
}
