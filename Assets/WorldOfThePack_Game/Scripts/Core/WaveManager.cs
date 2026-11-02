using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public enum SpawnAngle
    {
        North = 0,         // Direct overhead (0 deg)
        FlankLeft = 1,     // Ambush from left thicket (+35 deg pointing down-right)
        FlankRight = 2,    // Ambush from right thicket (-35 deg pointing down-left)
        Pincer = 3         // Simultaneous dual flank
    }

    public class WaveManager : MonoBehaviour
    {
        // A practical, display-safe finish line for the generated expedition. The loop is
        // still procedural from wave six onward, so no authored-wave list can terminate it.
        public const int MaxWaveNumber = 99999;
        [System.Serializable]
        public class EnemySpawnConfig
        {
            public EnemyType type;
            public float spawnDelay;
            public float laneX; // between -1.6 and 1.6
            public SpawnAngle angle;
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
        [SerializeField] private GameObject rudraPrefab;
        [SerializeField] private GameObject elsaPrefab;
        [SerializeField] private GameObject bossPrefab;

        private static WaveManager instance;
        public static WaveManager Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<WaveManager>();
                return instance;
            }
            private set => instance = value;
        }

        public int CurrentWaveIndex { get; private set; } = 0;
        public string CurrentWaveName => CurrentWaveIndex < authoredWaves.Count ? authoredWaves[CurrentWaveIndex].waveName : "Endless Patrol " + (CurrentWaveIndex + 1);
        public bool IsWaveActive { get; private set; }

        public event Action<int, string> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        private int remainingEnemiesInWave;
        private readonly List<EnemyUnit> activeEnemies = new List<EnemyUnit>();
        private bool isPausedForCamp = false;
        private bool cancelEncounterForCamp;
        public void StopForReset()
        {
            StopAllCoroutines();
            isPausedForCamp = false;
            cancelEncounterForCamp = false;
            IsWaveActive = false;
            remainingEnemiesInWave = 0;
            activeEnemies.Clear();
            CurrentWaveIndex = 0;
        }
        public void CancelEncounterAtCamp()
        {
            isPausedForCamp = true;
            cancelEncounterForCamp = true;
            remainingEnemiesInWave = 0;
            activeEnemies.Clear();
        }

        public void PauseForCamp()
        {
            isPausedForCamp = true;
        }

        public void ResumeFromCamp()
        {
            isPausedForCamp = false;
        }

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            LoadPrefabsIfNeeded();
            BuildDefaultWavesIfNeeded();
        }

        private void Start()
        {
            if (!IsWaveActive && CurrentWaveIndex == 0)
            {
                StartFirstWave();
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void LoadPrefabsIfNeeded()
        {
#if UNITY_EDITOR
            if (soldierPrefab == null)
                soldierPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Soldier.prefab");
            if (rudraPrefab == null)
                rudraPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Rudra.prefab");
            if (elsaPrefab == null)
                elsaPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/New/Enemy/Prefabs/Enemy_Elsa.prefab");
            if (houndPrefab == null)
                houndPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WorldOfThePack_Game/Prefabs/Units/Enemy_Hound.prefab");
            if (captainPrefab == null)
                captainPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WorldOfThePack_Game/Prefabs/Units/Enemy_Captain.prefab");
#endif
        }

        public void StartFirstWave()
        {
            isPausedForCamp = false;
            cancelEncounterForCamp = false;
            StopAllCoroutines();
            foreach (var e in activeEnemies)
            {
                if (e != null && e.gameObject != null) Destroy(e.gameObject);
            }
            activeEnemies.Clear();
            remainingEnemiesInWave = 0;

            CurrentWaveIndex = 0;
            StartCoroutine(WaveRoutine());
        }

        public void JumpToWave(int waveIndex)
        {
            isPausedForCamp = false;
            cancelEncounterForCamp = false;
            StopAllCoroutines();
            foreach (var e in activeEnemies)
            {
                if (e != null && e.gameObject != null) Destroy(e.gameObject);
            }
            activeEnemies.Clear();
            remainingEnemiesInWave = 0;

            CurrentWaveIndex = Mathf.Max(0, waveIndex);
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

                float delay = CurrentWaveIndex == 0 ? 0.6f : waveRestDuration;
                yield return new WaitForSeconds(delay);

                while (isPausedForCamp)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                WaveDefinition preview = GetWave(CurrentWaveIndex);
                int bonusEnemies = WorldOfThePack.Expedition.TrailDecisionDirector.Instance != null ? WorldOfThePack.Expedition.TrailDecisionDirector.Instance.ExtraEnemies : 0;
                bool bossWave=(CurrentWaveIndex+1)%5==0;
                remainingEnemiesInWave=preview.spawns.Count+bonusEnemies;
                cancelEncounterForCamp=false;
                System.Action stage=()=>
                {
                    if(bossWave)SpawnBoss();
                    for(int i=0;i<preview.spawns.Count+bonusEnemies;i++)
                    {
                        var config=i<preview.spawns.Count?preview.spawns[i]:new EnemySpawnConfig {type=EnemyType.Soldier};
                        SpawnEnemy(config.type,config.laneX,config.angle);
                    }
                };
                if(ArenaDirector.Instance!=null)
                    yield return ArenaDirector.Instance.PrepareEncounter(CurrentWaveIndex+1,remainingEnemiesInWave+(bossWave?1:0),CurrentWaveName,stage);
                else stage();
                while(GameDirector.Instance!=null && GameDirector.Instance.CurrentState!=GameState.Playing)yield return null;
                IsWaveActive=true;
                OnWaveStarted?.Invoke(CurrentWaveIndex+1,CurrentWaveName);

                // Wait until all enemies in wave are defeated
                while (remainingEnemiesInWave > 0)
                {
                    yield return new WaitForSeconds(0.3f);
                }

                IsWaveActive = false;
                while (isPausedForCamp) yield return null;
                ArenaDirector.Instance?.EndBattle(CurrentWaveIndex + 1);
                OnWaveCompleted?.Invoke(CurrentWaveIndex + 1);

                if (CurrentWaveIndex + 1 >= MaxWaveNumber)
                {
                    OnAllWavesCompleted?.Invoke();
                    Debug.Log("[WaveManager] Endless Expedition reached wave 99,999.");
                    yield break;
                }

                CurrentWaveIndex++;
                if (CurrentWaveIndex == authoredWaves.Count)
                {
                    OnAllWavesCompleted?.Invoke();
                }

                yield return new WaitForSeconds(waveRestDuration);
            }
        }

        private void SpawnEnemy(EnemyType type, float laneX, SpawnAngle angle = SpawnAngle.North)
        {
            float baseY = Camera.main != null ? Camera.main.transform.position.y : (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f);
            if (ArenaDirector.Instance != null && FormationController.Instance != null) baseY = FormationController.Instance.AnchorPosition.y - 1.2f;
            Vector3 spawnPos;
            Vector2 entryDir;

            switch (angle)
            {
                case SpawnAngle.FlankLeft:
                    spawnPos = new Vector3(-1.44f, baseY + 3.4f, 0f);
                    entryDir = new Vector2(0.5f, -0.8f).normalized;
                    break;
                case SpawnAngle.FlankRight:
                    spawnPos = new Vector3(1.44f, baseY + 3.4f, 0f);
                    entryDir = new Vector2(-0.5f, -0.8f).normalized;
                    break;
                default: // North
                    spawnPos = new Vector3(Mathf.Clamp(laneX, -1.44f, 1.44f), baseY + 4.2f, 0f);
                    entryDir = Vector2.down;
                    break;
            }

            if (ArenaDirector.Instance != null) spawnPos=ArenaDirector.Instance.ReserveEnemyPosition(type);
            CombatVFXPool.SpawnEffect("EnemySpawn", spawnPos);

            GameObject prefab = type switch
            {
                EnemyType.Hound => soldierPrefab,
                EnemyType.Captain => rudraPrefab,
                EnemyType.Rudra => rudraPrefab,
                EnemyType.Elsa => elsaPrefab,
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

            enemy.SetEntryDirection(entryDir);
            enemy.ApplyWaveDifficulty(CurrentWaveIndex);

            activeEnemies.Add(enemy);
            enemy.OnDefeated += OnEnemyDefeated;

            // Frame/lock camera on the incoming threat line
            if (CinemachineCombatDirector.Instance != null)
            {
                CinemachineCombatDirector.Instance.LockCombatEncounter(spawnPos);
            }
        }

        private void SpawnBoss()
        {
            float baseY = Camera.main != null ? Camera.main.transform.position.y : (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f);
            Vector3 bossPos = new Vector3(0f, baseY + 5.0f, 0f);
            if (ArenaDirector.Instance != null) bossPos = ArenaDirector.Instance.ReserveEnemyPosition(EnemyType.Rudra,true);
            GameObject bPrefab = bossPrefab;
#if UNITY_EDITOR
            if (bPrefab == null)
            {
                bPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WorldOfThePack_Game/Prefabs/Units/Boss_AncientThornGuardian.prefab");
            }
#endif
            if (bPrefab != null)
            {
                GameObject bossGo = Instantiate(bPrefab, bossPos, Quaternion.identity);
                BossUnit bu = bossGo.GetComponent<BossUnit>();
                if (bu != null)
                {
                    bu.ApplyWaveDifficulty(CurrentWaveIndex);
                    remainingEnemiesInWave++;
                    bu.OnDefeated += (c) =>
                    {
                        remainingEnemiesInWave = Mathf.Max(0, remainingEnemiesInWave - 1);
                        if (CinemachineCombatDirector.Instance != null)
                        {
                            CinemachineCombatDirector.Instance.TriggerScreenShake(0.35f);
                            CinemachineCombatDirector.Instance.UnlockCombatEncounter();
                        }
                    };
                }
            }
        }

        private void OnEnemyDefeated(Combatant c)
        {
            if (c is EnemyUnit eu)
            {
                activeEnemies.Remove(eu);
                remainingEnemiesInWave = Mathf.Max(0, remainingEnemiesInWave - 1);

                if (activeEnemies.Count == 0 && CinemachineCombatDirector.Instance != null)
                {
                    CinemachineCombatDirector.Instance.UnlockCombatEncounter();
                }
            }
        }

        private WaveDefinition GetWave(int index)
        {
            if (index < authoredWaves.Count) return authoredWaves[index];

            // Procedural endless wave scaling with bounded threat budget (4 to 10 enemies max)
            WaveDefinition def = new WaveDefinition
            {
                waveName = "Endless Hunt " + (index + 1)
            };
            int count = Mathf.Clamp(4 + (index - 4), 4, 10);
            for (int i = 0; i < count; i++)
            {
                EnemyType t = EnemyType.Soldier;
                SpawnAngle a = SpawnAngle.North;

                if (i % 4 == 1)
                {
                    t = EnemyType.Elsa;
                }
                else if (i % 4 == 2)
                {
                    t = EnemyType.Rudra;
                }
                else if (i % 4 == 3)
                {
                    t = EnemyType.Soldier;
                    a = (i % 2 == 0) ? SpawnAngle.FlankLeft : SpawnAngle.FlankRight;
                }

                def.spawns.Add(new EnemySpawnConfig
                {
                    type = t,
                    spawnDelay = UnityEngine.Random.Range(0.8f, 1.8f),
                    laneX = UnityEngine.Random.Range(-1.2f, 1.2f),
                    angle = a
                });
            }
            return def;
        }

        private void BuildDefaultWavesIfNeeded()
        {
            if (authoredWaves.Count > 0) return;

            // Wave 1: Solitary Scouts (Teach visible sword windup and basic attack pacing)
            WaveDefinition w1 = new WaveDefinition { waveName = "Forest Patrol Scouts" };
            w1.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.4f, laneX = -0.4f, angle = SpawnAngle.North });
            w1.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 1.6f, laneX = 0.4f, angle = SpawnAngle.North });
            authoredWaves.Add(w1);

            // Wave 2: Royal Archer Elsa (Teach bow draw and readable arrow projectile)
            WaveDefinition w2 = new WaveDefinition { waveName = "Royal Archer Elsa" };
            w2.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.4f, laneX = -0.5f, angle = SpawnAngle.North });
            w2.spawns.Add(new EnemySpawnConfig { type = EnemyType.Elsa, spawnDelay = 1.2f, laneX = 0.5f, angle = SpawnAngle.North });
            authoredWaves.Add(w2);

            // Wave 3: Iron Bodyguard Rudra (Teach heavy gauntlet slam warning)
            WaveDefinition w3 = new WaveDefinition { waveName = "Iron Bodyguard Rudra" };
            w3.spawns.Add(new EnemySpawnConfig { type = EnemyType.Rudra, spawnDelay = 0.5f, laneX = 0.0f, angle = SpawnAngle.North });
            authoredWaves.Add(w3);

            // Wave 4: Royal Escort Formation (Combine: Rudra guarding Elsa + flanking Soldier)
            WaveDefinition w4 = new WaveDefinition { waveName = "Royal Escort Formation" };
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Rudra, spawnDelay = 0.3f, laneX = -0.3f, angle = SpawnAngle.North });
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Elsa, spawnDelay = 0.6f, laneX = 0.3f, angle = SpawnAngle.North });
            w4.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 1.4f, laneX = 1.2f, angle = SpawnAngle.FlankRight });
            authoredWaves.Add(w4);

            // Wave 5: Bramble Brute Boss & Escort
            WaveDefinition w5 = new WaveDefinition { waveName = "Ancient Thorn Guardian" };
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.4f, laneX = -0.8f, angle = SpawnAngle.North });
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Soldier, spawnDelay = 0.2f, laneX = 0.8f, angle = SpawnAngle.North });
            w5.spawns.Add(new EnemySpawnConfig { type = EnemyType.Rudra, spawnDelay = 1.6f, laneX = 0.0f, angle = SpawnAngle.North });
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
            string spriteName = type switch
            {
                EnemyType.Captain => "Boss_BrambleBrute_South_0",
                EnemyType.Hound => "Enemy_Priest_South_0",
                _ => "Enemy_Soldier_South_0"
            };
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite " + spriteName);
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                sr.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
#endif
            if (sr.sprite == null)
            {
                sr.sprite = GetDefaultEnemySprite();
            }

            sr.color = Color.white;
            body.transform.localScale = Vector3.one;

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
