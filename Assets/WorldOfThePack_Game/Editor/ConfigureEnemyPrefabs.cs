using UnityEngine;
using UnityEditor;
using WorldOfThePack.Game;

namespace WorldOfThePack.EditorTools
{
    public static class ConfigureEnemyPrefabs
    {
        [MenuItem("World of the Pack/Configure Enemy Prefabs")]
        public static void Execute()
        {
            // 1. Soldier Prefab
            string soldierPath = "Assets/WorldOfThePack_Game/Prefabs/Units/Enemy_Soldier.prefab";
            GameObject soldierGo = AssetDatabase.LoadAssetAtPath<GameObject>(soldierPath);
            if (soldierGo != null)
            {
                var sr = soldierGo.GetComponentInChildren<SpriteRenderer>();
                var soldierSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Enemies/Enemy_Soldier_South.png");
                if (sr != null && soldierSprite != null)
                {
                    sr.sprite = soldierSprite;
                    sr.color = Color.white;
                    sr.transform.localScale = Vector3.one * 1.65f;
                    EditorUtility.SetDirty(soldierGo);
                    Debug.Log("Updated Enemy_Soldier prefab with south-facing sprite!");
                }
            }

            // 2. Captain / Archer Prefab
            string captainPath = "Assets/WorldOfThePack_Game/Prefabs/Units/Enemy_Captain.prefab";
            GameObject captainGo = AssetDatabase.LoadAssetAtPath<GameObject>(captainPath);
            if (captainGo != null)
            {
                var sr = captainGo.GetComponentInChildren<SpriteRenderer>();
                var archerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Enemies/Enemy_Archer_South.png");
                if (sr != null && archerSprite != null)
                {
                    sr.sprite = archerSprite;
                    sr.color = Color.white;
                    sr.transform.localScale = Vector3.one * 1.65f;
                    EditorUtility.SetDirty(captainGo);
                    Debug.Log("Updated Enemy_Captain prefab with south-facing archer sprite!");
                }
            }

            // 3. Hound / Priest Prefab
            string houndPath = "Assets/WorldOfThePack_Game/Prefabs/Units/Enemy_Hound.prefab";
            GameObject houndGo = AssetDatabase.LoadAssetAtPath<GameObject>(houndPath);
            if (houndGo != null)
            {
                var sr = houndGo.GetComponentInChildren<SpriteRenderer>();
                var priestSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Enemies/Enemy_Priest_South.png");
                if (sr != null && priestSprite != null)
                {
                    sr.sprite = priestSprite;
                    sr.color = Color.white;
                    sr.transform.localScale = Vector3.one * 1.65f;
                    EditorUtility.SetDirty(houndGo);
                    Debug.Log("Updated Enemy_Hound prefab with south-facing priest sprite!");
                }
            }

            // 4. Boss Bramble Brute Prefab
            string bossPath = "Assets/WorldOfThePack_Game/Prefabs/Units/Boss_AncientThornGuardian.prefab";
            GameObject bossGo = AssetDatabase.LoadAssetAtPath<GameObject>(bossPath);
            if (bossGo != null)
            {
                var sr = bossGo.GetComponentInChildren<SpriteRenderer>();
                var bossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Enemies/Boss_BrambleBrute_South.png");
                if (sr != null && bossSprite != null)
                {
                    sr.sprite = bossSprite;
                    sr.color = Color.white;
                    sr.transform.localScale = Vector3.one * 2.1f;
                    EditorUtility.SetDirty(bossGo);
                    Debug.Log("Updated Boss_AncientThornGuardian prefab with Bramble Brute sprite!");
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Finished configuring enemy prefabs!");
        }
    }
}
