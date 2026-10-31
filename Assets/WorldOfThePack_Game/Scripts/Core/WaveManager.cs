using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class WaveManager : MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnConfig
        {
            public EnemyType type;
            public float spawnDelay;
            public float laneX; // between -1.6 and 1.6
        }

        [System.Serializable]
        public class WaveDefinition
        {
            public string waveName;
            public List<EnemySpawnConfig> spawns = new List<EnemySpawnConfig>();
        }

        [Header("Wave Configuration")]
        [SerializeField] private List<WaveDefinition> authoredWaves = new List<WaveDefinition>();
        [SerializeField] private float waveRestDuration = 3.5f;

        [Header("Prefabs")]
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private GameObject houndPrefab;
        [SerializeField] private GameObject captainPrefab;

        public int CurrentWaveIndex { get; private set; } = 0;
        public string CurrentWaveName => CurrentWaveIndex < authoredWaves.Count ? authoredWaves[CurrentWaveIndex].waveName : "Endless Patrol " + (CurrentWaveIndex + 1);
        public bool IsWaveActive { get; private set; }

        public event Action<int, string> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        private int remainingEnemiesInWave;
        private readonly List<EnemyUnit> activeEnemies = new List<EnemyUnit>();
        private bool isPausedForCamp = false;

        public void PauseForCamp()
        {
            isPausedForCamp = true;
        }

        public void ResumeFromCamp()
        {
            isPausedForCamp = false;
        }

        private void Start()
        {
            BuildDefaultWavesIfNeeded();
        }

        public void StartFirstWave()
        {
            CurrentWaveIndex = 0;
            StartCoroutine(WaveRoutine());
        }

        public void JumpToWave(int waveIndex)
        {
            StopAllCoroutines();
            // Clear remaining enemies from previous wave
            foreach (var e in activeEnemies)
            {
                if (e != null && e.gameObject != null) Destroy(e.gameObject);
            }
            activeEnemies.Clear();
            remainingEnemiesInWave = 0;

            CurrentWaveIndex = Mathf.Clamp(waveIndex, 0, authoredWaves.Count - 1);
            StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            while (true)
            {
                while (isPausedForCamp)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                float delay = CurrentWaveIndex == 0 ? 1.0f : waveRestDuration;
                yield return new WaitForSeconds(delay);

                while (isPausedForCamp)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                IsWaveActive = true;
                string wName = CurrentWaveName;
                OnWaveStarted?.Invoke(CurrentWaveIndex + 1, wName);

                WaveDefinition currentWave = GetWave(CurrentWaveIndex);
                remainingEnemiesInWave = currentWave.spawns.Count;

                if (CurrentWaveIndex == 4)
                {
                    SpawnBoss();
                }

                for (int i = 0; i < currentWave.spawns.Count; i++)
                {
                    var cfg = currentWave.spawns[i];
                    if (cfg.spawnDelay > 0f)
                    {
                        yield return new WaitForSeconds(cfg.spawnDelay);
                    }

                    SpawnEnemy(cfg.type, cfg.laneX);
                }

                // Wait until all enemies in wave are defeated
                while (remainingEnemiesInWave > 0)
                {
                    yield return new WaitForSeconds(0.4f);
                }

                IsWaveActive = false;
                OnWaveCompleted?.Invoke(CurrentWaveIndex + 1);

                CurrentWaveIndex++;
                if (CurrentWaveIndex >= authoredWaves.Count)
                {
                    OnAllWavesCompleted?.Invoke();
                }

                yield return new WaitForSeconds(waveRestDuration);
            }
        }

        private void SpawnEnemy(EnemyType type, float laneX)
        {
            // Position ahead of current formation anchor / camera viewport
            float baseY = Camera.main != null ? Camera.main.transform.position.y : (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f);
            Vector3 spawnPos = new Vector3(Mathf.Clamp(laneX, -1.6f, 1.6f), baseY + 6.8f, 0f);

            CombatVFXPool.SpawnEffect("EnemySpawn", spawnPos);

            GameObject prefab = type switch
            {
                EnemyType.Hound => houndPrefab,
                EnemyType.Captain => captainPrefab,
                _ => soldierPrefab
            };

            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, spawnPos, Quaternion.identity);
            }
            else
            {
                go = CreateProceduralEnemy(type, spawnPos);
            }

            EnemyUnit enemy = go.GetComponent<EnemyUnit>();
            if (enemy == null) enemy = go.AddComponent<EnemyUnit>();

            activeEnemies.Add(enemy);
            enemy.OnDefeated += OnEnemyDefeated;
        }

        private void SpawnBoss()
        {
            float baseY = Camera.main != null ? Camera.main.transform.position.y : (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f);
            Vector3 bossPos = new Vector3(0f, baseY + 4.8f, 0f);
            GameObject bossPrefab = null;
#if UNITY_EDITOR
            bossPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WorldOfThePack_Game/Prefabs/Units/Boss_AncientThornGuardian.prefab");
#endif
            if (bossPrefab != null)
            {
                GameObject bossGo = Instantiate(bossPrefab, bossPos, Quaternion.identity);
                BossUnit bu = bossGo.GetComponent<BossUnit>();
                if (bu != null)
                {
                    remainingEnemiesInWave++;
                    bu.OnDefeated += (c) => remainingEnemiesInWave = Mathf.Max(0, remainingEnemiesInWave - 1);
                }
            }
        }

        private void OnEnemyDefeated(Combatant c)
        {
            if (c is EnemyUnit eu)
            {
                activeEnemies.Remove(eu);
                remainingEnemiesInWave = Mathf.Max(0, remainingEnemiesInWave - 1);
            }
        }

        private WaveDefinition GetWave(int index)
        {
            if (index < authoredWaves.Count) return authoredWaves[index];

            // Procedural endless wave scaling
            WaveDefinition def = new WaveDefinition
            {
                waveName = "Endless Hunt " + (index + 1)
            };
            int count = 4 + index * 2;
            for (int i = 0; i < count; i++)
            {
                EnemyType t = EnemyType.Soldier;
                if (i % 3 == 0) t = EnemyType.Hound;
                if (i == count - 1 && index >= 3) t = EnemyType.Captain;

                def.spawns.Add(new EnemySpawnConfig
                {
                    type = t,
                    spawnDelay = UnityEngine.Random.Range(0.6f, 1.6f),
                    laneX = UnityEngine.Random.Range(-1.5f, 1.5f)
                });
            }
            return def;
        }

        private void BuildDefaultWavesIfNeeded()
        {
            if (authoredWaves.Count > 0) return;

            // Wave 1: First skirmish
            WaveDefinition w1 = new WaveDefinition { waveName = "Forest Patrol" };
            w1.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.5f, laneX = -0.6f });
            w1.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 2.2f, laneX = 0.6f });
            authoredWaves.Add(w1);

            // Wave 2: Scout Hounds
            WaveDefinition w2 = new WaveDefinition { waveName = "Hound Pursuit" };
            w2.spawns.Add(new EnemySpawnConfig { type = EnemyType.Hound, spawnDelay = 0.5f, laneX = -1.0f });
            w2.spawns.Add(new EnemySpawnConfig { type = EnemyType.Hound, spawnDelay = 1.2f, laneX = 1.0f });
            w2.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 2.0f, laneX = 0.0f });
            authoredWaves.Add(w2);

            // Wave 3: Heavy Ambush
            WaveDefinition w3 = new WaveDefinition { waveName = "Treeline Ambush" };
            w3.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.5f, laneX = -1.2f });
            w3.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 1.0f, laneX = 1.2f });
            w3.spawns.Add(new EnemySpawnConfig { type = EnemyType.Hound, spawnDelay = 1.5f, laneX = 0.0f });
            w3.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 2.0f, laneX = -0.5f });
            authoredWaves.Add(w3);

            // Wave 4: Captain Arrival
            WaveDefinition w4 = new WaveDefinition { waveName = "Hunter Captain Vanguard" };
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Hound, spawnDelay = 0.5f, laneX = -0.8f });
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Hound, spawnDelay = 0.8f, laneX = 0.8f });
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Captain, spawnDelay = 2.2f, laneX = 0.0f });
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 2.0f, laneX = -1.0f });
            authoredWaves.Add(w4);

            // Wave 5: Bramble Brute Forest Colossus
            WaveDefinition w5 = new WaveDefinition { waveName = "Bramble Brute Forest Colossus" };
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.4f, laneX = -1.1f });
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.4f, laneX = 1.1f });
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Captain, spawnDelay = 2.0f, laneX = -0.5f });
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Hound, spawnDelay = 2.0f, laneX = 0.5f });
            authoredWaves.Add(w5);
        }

        private GameObject CreateProceduralEnemy(EnemyType type, Vector3 position)
        {
            GameObject go = new GameObject("Enemy_" + type);
            go.transform.position = position;

            GameObject body = new GameObject("Body");
            body.transform.SetParent(go.transform, false);
            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();

#if UNITY_EDITOR
            string path = "Assets/Characters/ScoutPip/Wolf/ScoutPip_Wolf_North.png";
            var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var s in sprites)
            {
                if (s is Sprite sp) { sr.sprite = sp; break; }
            }
#endif
            if (sr.sprite == null)
            {
                sr.sprite = GetDefaultEnemySprite();
            }

            sr.color = type switch
            {
                EnemyType.Captain => new Color(0.95f, 0.45f, 0.35f, 1f),
                EnemyType.Hound => new Color(0.85f, 0.65f, 0.45f, 1f),
                _ => new Color(0.9f, 0.5f, 0.5f, 1f)
            };
            body.transform.localScale = type == EnemyType.Captain ? new Vector3(1.3f, 1.3f, 1f) : Vector3.one;

            EnemyUnit unit = go.AddComponent<EnemyUnit>();
            var field = typeof(EnemyUnit).GetField("enemyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(unit, type);

            return go;
        }

        private static Sprite cachedEnemySprite;
        private static Sprite GetDefaultEnemySprite()
        {
            if (cachedEnemySprite != null) return cachedEnemySprite;
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (Vector2.Distance(new Vector2(x, y), center) <= size / 2f) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            cachedEnemySprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.2f), 32f);
            return cachedEnemySprite;
        }
    }
}
