using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class EndlessPathScroller : MonoBehaviour
    {
        [Header("Scroll Configuration")]
        [SerializeField] private float ambientScrollSpeed = 0.5f;
        [SerializeField] private float tileHeight = 21.72f;
        [SerializeField] private int tileCount = 4;
        [SerializeField] private Sprite pathSprite;

        private readonly List<Transform> activeTiles = new List<Transform>();
        private Camera targetCamera;

        private void Start()
        {
            targetCamera = Camera.main;
            SetupTiles();
        }

        private void SetupTiles()
        {
            if (pathSprite == null)
            {
#if UNITY_EDITOR
                string path = "Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-loop-source-v01.png";
                pathSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (pathSprite == null)
                {
                    path = "Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-sunny-forest-master-v01.png";
                    pathSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
#endif
            }

            if (pathSprite != null)
            {
                tileHeight = pathSprite.rect.height / pathSprite.pixelsPerUnit;
            }
            else
            {
                tileHeight = 16f;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            activeTiles.Clear();

            float camY = targetCamera != null ? targetCamera.transform.position.y : 0f;

            // Instantiate 4 seamless background tiles spanning below and far above the camera
            for (int i = 0; i < tileCount; i++)
            {
                GameObject tileObj = new GameObject("PathTile_" + i);
                tileObj.transform.SetParent(transform, false);
                tileObj.transform.position = new Vector3(0f, camY + (i - 1) * tileHeight, 0f);

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = pathSprite;
                sr.sortingOrder = -100;

                activeTiles.Add(tileObj.transform);
            }
        }

        private void Update()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            float camY = targetCamera != null ? targetCamera.transform.position.y : 0f;

            // Optional ambient scroll
            if (ambientScrollSpeed > 0f)
            {
                float delta = ambientScrollSpeed * Time.deltaTime;
                for (int i = 0; i < activeTiles.Count; i++)
                {
                    activeTiles[i].position += Vector3.down * delta;
                }
            }

            // Recycle tiles that have scrolled below the camera's bottom frustum
            float bottomThreshold = camY - tileHeight * 1.5f;

            for (int i = 0; i < activeTiles.Count; i++)
            {
                Transform t = activeTiles[i];
                if (t.position.y < bottomThreshold)
                {
                    float highestY = float.MinValue;
                    for (int j = 0; j < activeTiles.Count; j++)
                    {
                        if (activeTiles[j].position.y > highestY)
                        {
                            highestY = activeTiles[j].position.y;
                        }
                    }
                    t.position = new Vector3(0f, highestY + tileHeight, 0f);
                }
            }
        }

        public void SetScrollSpeed(float speed)
        {
            ambientScrollSpeed = speed;
        }
    }
}
