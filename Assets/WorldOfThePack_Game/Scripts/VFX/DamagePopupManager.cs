using System.Collections;
using TMPro;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class DamagePopupManager : MonoBehaviour
    {
        private static DamagePopupManager instance;
        public static DamagePopupManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("DamagePopupManager");
                    instance = go.AddComponent<DamagePopupManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
        public void ClearPopups()
        {
            StopAllCoroutines();
            foreach (Transform child in transform) Destroy(child.gameObject);
        }

        public static void ShowPopup(Vector3 worldPos, float amount, bool isEnemyHit, bool isHeal = false)
        {
            if (amount <= 0) return;
            Instance.CreatePopup(worldPos, amount, isEnemyHit, isHeal);
        }

        private void CreatePopup(Vector3 worldPos, float amount, bool isEnemyHit, bool isHeal)
        {
            GameObject go = new GameObject("DamagePopup");
            go.transform.SetParent(transform, true);
            go.transform.position = worldPos + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0f, 0.2f), 0f);

            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = isHeal ? 2.2f : isEnemyHit ? 2.5f : 2.2f;
            tmp.sortingOrder = 1500;

            if (isHeal)
            {
                tmp.text = "+" + Mathf.RoundToInt(amount);
                tmp.color = new Color(0.3f, 1f, 0.4f, 1f);
            }
            else if (isEnemyHit)
            {
                tmp.text = Mathf.RoundToInt(amount).ToString();
                tmp.color = new Color(1f, 0.95f, 0.3f, 1f); // Vibrant yellow/orange hit
            }
            else
            {
                tmp.text = "-" + Mathf.RoundToInt(amount);
                tmp.color = new Color(1f, 0.35f, 0.35f, 1f); // Red pack hit
            }

            StartCoroutine(AnimatePopup(go, tmp));
        }

        private IEnumerator AnimatePopup(GameObject go, TextMeshPro tmp)
        {
            float duration = 0.6f;
            float elapsed = 0f;
            Vector3 startPos = go.transform.position;
            Vector3 targetPos = startPos + Vector3.up * .45f;
            Color startColor = tmp.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                go.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - (t * t));
                yield return null;
            }

            Destroy(go);
        }
    }
}
