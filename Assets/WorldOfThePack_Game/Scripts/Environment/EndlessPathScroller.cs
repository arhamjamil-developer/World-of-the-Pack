using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class EndlessPathScroller : MonoBehaviour
    {
        [Header("Scroll Configuration")]
        [SerializeField] private float scrollSpeed = 1.2f;
        [SerializeField] private float tileHeight = 21.72f;
        [SerializeField] private int tileCount = 2;
        [SerializeField] private Sprite pathSprite;
        [SerializeField] private Sprite edgePropsSprite;

        private readonly List<Transform> activeTiles = new List<Transform>();

        private void Start()
        {
            SetupTiles();
        }

        private void SetupTiles()
        {
            if (pathSprite == null)
            {
#if UNITY_EDITOR
                string path = "Assets/Environemnt/EndlessPathSystem/Artwork/endless-lane-sunny-forest-master-v01.png";
                pathSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (pathSprite == null)
                {
                    path = "Assets/Environemnt/SunnyForestTrail/Artwork/sunny-forest-loop-source-v01.png";
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

            // Create background tiles
            for (int i = 0; i < tileCount; i++)
            {
                GameObject tileObj = new GameObject("PathTile_" + i);
                tileObj.transform.SetParent(transform, false);
                tileObj.transform.position = new Vector3(0f, i * tileHeight, 0f);

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = pathSprite;
                sr.sortingOrder = -100;

                activeTiles.Add(tileObj.transform);
            }
        }

        private void Update()
        {
            float delta = scrollSpeed * Time.deltaTime;

            for (int i = 0; i < activeTiles.Count; i++)
            {
                Transform t = activeTiles[i];
                t.position += Vector3.down * delta;

                // Loop around when scrolled past camera bottom
                if (t.position.y <= -tileHeight)
                {
                    t.position += Vector3.up * (tileHeight * activeTiles.Count);
                }
            }
        }

        public void SetScrollSpeed(float speed)
        {
            scrollSpeed = speed;
        }
    }
}
