using System;
using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game
{
    public enum GameState
    {
        Intro,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public class GameDirector : MonoBehaviour
    {
        public static GameDirector Instance { get; private set; }

        [Header("Pack Condition (Health)")]
        [SerializeField] private float maxPackHealth = 100f;
        [SerializeField] private float currentPackHealth = 100f;

        [Header("Population")]
        [SerializeField] private int maxAllies = 10;

        [Header("Starter Roster")]
        [SerializeField] private List<string> starterHeroIds = new List<string> { "HeroWolvy", "PrincessElva", "CaptainBobo" };

        [Header("Hero Unit Prefabs")]
        [SerializeField] private List<HeroPrefabEntry> heroPrefabs = new List<HeroPrefabEntry>();

        [System.Serializable]
        public struct HeroPrefabEntry
        {
            public string heroId;
            public GameObject prefab;
        }

        public GameState CurrentState { get; private set; } = GameState.Intro;
        public float PackHealthPercent => Mathf.Clamp01(currentPackHealth / Mathf.Max(1f, maxPackHealth));
        public float CurrentPackHealth => currentPackHealth;
        public float MaxPackHealth => maxPackHealth;
        public int TotalKills { get; private set; }
        public int Score { get; private set; }
        public int ActiveAllyCount => activeAllies.Count;
        public int MaxAllies => maxAllies;

        public event Action<float, float> OnPackHealthChanged;
        public event Action<int> OnScoreChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<HeroUnit> OnHeroSpawned;

        private readonly List<HeroUnit> activeAllies = new List<HeroUnit>();
        private WaveManager waveManager;
        private CardDeckManager deckManager;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            waveManager = GetComponent<WaveManager>();
            deckManager = GetComponent<CardDeckManager>();
        }

        private void Start()
        {
            currentPackHealth = maxPackHealth;
            StartGame();
        }

        public void StartGame()
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;

            // Spawn initial starter group
            SpawnStarterSquad();

            // Start wave encounters
            if (waveManager != null)
            {
                waveManager.StartFirstWave();
            }

            OnStateChanged?.Invoke(CurrentState);
            OnPackHealthChanged?.Invoke(currentPackHealth, maxPackHealth);
        }

        private void SpawnStarterSquad()
        {
            float[] laneOffsets = { 0f, -0.9f, 0.9f };
            for (int i = 0; i < starterHeroIds.Count; i++)
            {
                string id = starterHeroIds[i];
                float x = i < laneOffsets.Length ? laneOffsets[i] : UnityEngine.Random.Range(-1.2f, 1.2f);
                Vector3 pos = new Vector3(x, -2.5f + (i % 2) * 0.4f, 0f);
                SpawnHeroUnitById(id, pos, true);
            }
        }

        public bool TryDeployHero(HeroCardDefinition def)
        {
            if (CurrentState != GameState.Playing) return false;
            if (activeAllies.Count >= maxAllies) return false;

            // Deploy at base of combat lane with slight random horizontal spread
            float x = UnityEngine.Random.Range(-1.3f, 1.3f);
            Vector3 pos = new Vector3(x, -3.2f, 0f);

            // Spawn MarchDust effect
            CombatVFXPool.SpawnEffect("MarchDust", pos);

            HeroUnit unit = SpawnHeroUnitById(def.heroId, pos, true);
            if (unit != null)
            {
                CombatVFXPool.SpawnEffect("Ready", pos + Vector3.up * 0.3f);
                return true;
            }
            return false;
        }

        public HeroUnit SpawnHeroUnitById(string id, Vector3 pos, bool wolfForm)
        {
            GameObject prefab = GetHeroPrefab(id, wolfForm);
            GameObject go;

            if (prefab != null)
            {
                go = Instantiate(prefab, pos, Quaternion.identity);
            }
            else
            {
                go = CreateProceduralHero(id, pos, wolfForm);
            }

            HeroUnit hero = go.GetComponent<HeroUnit>();
            if (hero == null) hero = go.AddComponent<HeroUnit>();

            activeAllies.Add(hero);
            hero.OnDefeated += OnHeroDefeated;
            OnHeroSpawned?.Invoke(hero);

            return hero;
        }

        private void OnHeroDefeated(Combatant c)
        {
            if (c is HeroUnit hu)
            {
                activeAllies.Remove(hu);
            }
        }

        public void DamagePack(float amount)
        {
            if (CurrentState != GameState.Playing) return;

            currentPackHealth = Mathf.Max(0f, currentPackHealth - amount);
            OnPackHealthChanged?.Invoke(currentPackHealth, maxPackHealth);

            if (currentPackHealth <= 0f)
            {
                TriggerGameOver();
            }
        }

        public void HealPack(float amount)
        {
            currentPackHealth = Mathf.Min(maxPackHealth, currentPackHealth + amount);
            OnPackHealthChanged?.Invoke(currentPackHealth, maxPackHealth);
        }

        public void OnEnemyKilled(EnemyUnit enemy, float energyReward)
        {
            TotalKills++;
            Score += Mathf.RoundToInt(energyReward * 10f);
            OnScoreChanged?.Invoke(Score);

            // Distribute energy to deck cards
            if (deckManager != null)
            {
                deckManager.DistributeEnergy(energyReward);
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
            }
            else if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
            }
            OnStateChanged?.Invoke(CurrentState);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;

            // Clean up existing units
            foreach (var hero in activeAllies)
            {
                if (hero != null) Destroy(hero.gameObject);
            }
            activeAllies.Clear();

            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                if (e != null) Destroy(e.gameObject);
            }

            TotalKills = 0;
            Score = 0;
            currentPackHealth = maxPackHealth;

            StartGame();
        }

        private void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
            Time.timeScale = 0.4f; // Dramatic slow motion
            OnStateChanged?.Invoke(CurrentState);
        }

        public void TriggerVictory()
        {
            CurrentState = GameState.Victory;
            OnStateChanged?.Invoke(CurrentState);
        }

        private GameObject GetHeroPrefab(string heroId, bool wolfForm)
        {
            string form = wolfForm ? "Wolf" : "Human";
            string targetName = heroId + "_" + form + "_North";

            foreach (var entry in heroPrefabs)
            {
                if (entry.heroId == targetName || entry.heroId == heroId)
                {
                    return entry.prefab;
                }
            }

#if UNITY_EDITOR
            string packUnitPath = "Assets/WorldOfThePack_Game/Prefabs/Units/" + heroId + "_PackUnit.prefab";
            GameObject packUnit = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(packUnitPath);
            if (packUnit != null) return packUnit;

            string path = "Assets/Characters/" + heroId + "/Prefabs/" + targetName + ".prefab";
            GameObject p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (p != null) return p;
#endif
            return null;
        }

        private GameObject CreateProceduralHero(string id, Vector3 pos, bool wolfForm)
        {
            GameObject go = new GameObject("Hero_" + id);
            go.transform.position = pos;

            GameObject body = new GameObject("Body");
            body.transform.SetParent(go.transform, false);
            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();

#if UNITY_EDITOR
            string form = wolfForm ? "Wolf" : "Human";
            string path = "Assets/Characters/" + id + "/" + form + "/" + id + "_" + form + "_North.png";
            var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var s in sprites)
            {
                if (s is Sprite sp) { sr.sprite = sp; break; }
            }
#endif
            HeroUnit unit = go.AddComponent<HeroUnit>();
            var field = typeof(HeroUnit).GetField("heroId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(unit, id);

            return go;
        }
    }
}
