using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.UI;
using WorldOfThePack.VFX;

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

        [Header("Alpha Leader Condition")]
        [SerializeField] private HeroUnit alphaLeader;
        [SerializeField] private float maxPackHealth = 130f;
        [SerializeField] private float currentPackHealth = 130f;

        [Header("Expedition Shared Energy (0-10 Pool)")]
        [SerializeField] private float maxEnergy = 10f;
        [SerializeField] private float currentEnergy = 5f;
        [SerializeField] private float baseEnergyRegenRate = 0.8f; // ~1 energy per 1.25s

        [Header("Pack Morale (0-100%) & Wolf Instinct")]
        [SerializeField] private float maxMorale = 100f;
        [SerializeField] private float currentMorale = 0f;
        [SerializeField] private bool isWolfInstinctActive = false;
        private float wolfInstinctTimer = 0f;

        [Header("Population")]
        [SerializeField] private int maxAllies = 10;

        [Header("Starter Pack (Core Trio)")]
        [SerializeField] private List<string> starterHeroIds = new List<string> { "HeroWolvy", "CaptainBobo", "PrincessElva" };

        [Header("Hero Unit Prefabs")]
        [SerializeField] private List<HeroPrefabEntry> heroPrefabs = new List<HeroPrefabEntry>();

        [System.Serializable]
        public struct HeroPrefabEntry
        {
            public string heroId;
            public GameObject prefab;
        }

        public GameState CurrentState { get; private set; } = GameState.Intro;
        public HeroUnit AlphaLeader => alphaLeader;
        public float PackHealthPercent => alphaLeader != null ? alphaLeader.HealthPercent : Mathf.Clamp01(currentPackHealth / Mathf.Max(1f, maxPackHealth));
        public float CurrentPackHealth => alphaLeader != null ? alphaLeader.CurrentHealth : currentPackHealth;
        public float MaxPackHealth => alphaLeader != null ? alphaLeader.MaxHealth : maxPackHealth;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float CurrentMorale => currentMorale;
        public float MaxMorale => maxMorale;
        public bool IsWolfInstinctActive => isWolfInstinctActive;
        public float WolfInstinctTimeRemaining => Mathf.Max(0f, wolfInstinctTimer);

        public int TotalKills { get; private set; }
        public int Score { get; private set; }
        public int ActiveAllyCount => activeAllies.Count;
        public int MaxAllies => maxAllies;

        public event Action<float, float> OnPackHealthChanged;
        public event Action<float, float> OnEnergyChanged;
        public event Action<float, float> OnMoraleChanged;
        public event Action<bool, float> OnWolfInstinctChanged;
        public event Action<int> OnScoreChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<HeroUnit> OnHeroSpawned;

        private readonly List<HeroUnit> activeAllies = new List<HeroUnit>();
        private CardDeckManager deckManager;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            deckManager = GetComponent<CardDeckManager>();
        }

        private void Start()
        {
            currentEnergy = 5f;
            currentMorale = 0f;
            StartGame();
        }

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            // Shared energy regeneration
            float regenMult = CampManager.Instance != null ? CampManager.Instance.EnergyRegenMultiplier : 1.0f;
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + baseEnergyRegenRate * regenMult * Time.deltaTime);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            // Wolf Instinct countdown
            if (isWolfInstinctActive)
            {
                wolfInstinctTimer -= Time.deltaTime;
                currentMorale = Mathf.Max(0f, (wolfInstinctTimer / 10f) * maxMorale);
                OnMoraleChanged?.Invoke(currentMorale, maxMorale);
                OnWolfInstinctChanged?.Invoke(true, wolfInstinctTimer);

                if (wolfInstinctTimer <= 0f)
                {
                    EndWolfInstinct();
                }
            }
        }

        public void StartGame()
        {
            CurrentState = GameState.Intro;
            Time.timeScale = 1f;

            // Trigger the scenic opening sequence
            if (ExpeditionIntroController.Instance != null)
            {
                ExpeditionIntroController.Instance.PlayIntroSequence();
            }
            else
            {
                // Fallback direct start
                SpawnStarterPack();
                if (FormationController.Instance != null)
                {
                    FormationController.Instance.SetMarching(true);
                }
                CurrentState = GameState.Playing;
                WaveManager wm = GetComponent<WaveManager>();
                if (wm != null) wm.StartFirstWave();
            }

            OnStateChanged?.Invoke(CurrentState);
            OnPackHealthChanged?.Invoke(CurrentPackHealth, MaxPackHealth);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            OnMoraleChanged?.Invoke(currentMorale, maxMorale);
        }

        public void SetPlayingState()
        {
            CurrentState = GameState.Playing;
            OnStateChanged?.Invoke(CurrentState);
        }

        public void SpawnStarterPack()
        {
            if (activeAllies.Count > 0) return;

            // 1. Spawn Alpha Leader in Center Lane (Lane 2)
            Vector3 alphaPos = FormationController.Instance != null
                ? FormationController.Instance.AnchorPosition + new Vector3(LaneSystem.GetLaneX(2), 0.75f, 0f)
                : new Vector3(0f, -1.0f, 0f);

            HeroUnit alpha = SpawnHeroUnitById("HeroWolvy", alphaPos, false);
            if (alpha != null)
            {
                alpha.SetAlpha(true);
                alpha.SetAssignedLane(2);
                alphaLeader = alpha;
                currentPackHealth = alpha.MaxHealth;
                maxPackHealth = alpha.MaxHealth;
            }

            // 2. Spawn Guardian Tank in Mid-Left Lane (Lane 1)
            Vector3 tankPos = FormationController.Instance != null
                ? FormationController.Instance.AnchorPosition + new Vector3(LaneSystem.GetLaneX(1), 0.75f, 0f)
                : new Vector3(-0.72f, -1.0f, 0f);

            HeroUnit tank = SpawnHeroUnitById("CaptainBobo", tankPos, false);
            if (tank != null) tank.SetAssignedLane(1);

            // 3. Spawn Hunter Archer in Mid-Right Lane (Lane 3)
            Vector3 hunterPos = FormationController.Instance != null
                ? FormationController.Instance.AnchorPosition + new Vector3(LaneSystem.GetLaneX(3), -0.45f, 0f)
                : new Vector3(0.72f, -1.6f, 0f);

            HeroUnit hunter = SpawnHeroUnitById("PrincessElva", hunterPos, false);
            if (hunter != null) hunter.SetAssignedLane(3);

            OnPackHealthChanged?.Invoke(CurrentPackHealth, MaxPackHealth);
        }

        public bool TrySpendEnergy(float amount)
        {
            if (currentEnergy >= amount)
            {
                currentEnergy -= amount;
                OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
                return true;
            }
            return false;
        }

        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        public void AddMorale(float amount)
        {
            if (isWolfInstinctActive) return;

            currentMorale = Mathf.Min(maxMorale, currentMorale + amount);
            OnMoraleChanged?.Invoke(currentMorale, maxMorale);

            if (currentMorale >= maxMorale)
            {
                TriggerWolfInstinct(10f);
            }
        }

        public void TriggerWolfInstinct(float duration = 10f)
        {
            isWolfInstinctActive = true;
            wolfInstinctTimer = duration;

            // Transform all pack members into radiant wolves!
            foreach (var ally in activeAllies)
            {
                if (ally != null && ally.IsAlive)
                {
                    ally.ActivateWolfSurge(duration);
                }
            }

            GameHUDController hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null) hud.ShowBanner("★ WOLF INSTINCT ACTIVATED! ★", 3.0f);

            OnWolfInstinctChanged?.Invoke(true, duration);
        }

        private void EndWolfInstinct()
        {
            isWolfInstinctActive = false;
            currentMorale = 0f;

            foreach (var ally in activeAllies)
            {
                if (ally != null && ally.IsAlive)
                {
                    ally.DeactivateWolfSurge();
                }
            }

            OnMoraleChanged?.Invoke(0f, maxMorale);
            OnWolfInstinctChanged?.Invoke(false, 0f);
        }

        public HeroUnit DeployHeroInLane(HeroCardDefinition def, int laneIndex, float energyCost = 3f)
        {
            if (activeAllies.Count >= maxAllies) return null;
            if (!TrySpendEnergy(energyCost)) return null;

            int lane = Mathf.Clamp(laneIndex, 0, LaneSystem.LaneCount - 1);
            float laneX = LaneSystem.GetLaneX(lane);

            Vector3 spawnPos = new Vector3(laneX, -2.8f, 0f);
            if (FormationController.Instance != null)
            {
                spawnPos = new Vector3(laneX, FormationController.Instance.AnchorPosition.y - 2.5f, 0f);
            }

            HeroUnit unit = SpawnHeroUnitById(def.heroId, spawnPos, false);
            if (unit != null)
            {
                unit.SetAssignedLane(lane);
            }
            return unit;
        }

        public HeroUnit DeployHero(HeroCardDefinition def)
        {
            int preferredLane = 2;
            if (FormationController.Instance != null)
            {
                preferredLane = FormationController.Instance.GetPreferredLaneForHero(def.heroId);
            }
            return DeployHeroInLane(def, preferredLane, GetCardEnergyCost(def.heroId));
        }

        public HeroUnit DeployHeroAtPosition(HeroCardDefinition def, Vector3 pos)
        {
            int lane = LaneSystem.GetNearestLaneIndex(pos.x);
            return DeployHeroInLane(def, lane, GetCardEnergyCost(def.heroId));
        }

        public float GetCardEnergyCost(string heroId)
        {
            return heroId switch
            {
                "HeroWolvy" => 4f,
                "PrincessElva" => 3f,
                "CaptainBobo" => 3f,
                "ScoutPip" => 2f,
                "FairyLulu" => 4f,
                "WizardMomo" => 5f,
                "TinkerTilly" => 3f,
                _ => 3f
            };
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

            hero.SetForm(wolfForm);

            // Apply route bonuses if applicable
            if (CampManager.Instance != null && CampManager.Instance.PackDamageBuff > 1.0f)
            {
                hero.SetDamageMultiplier(CampManager.Instance.PackDamageBuff);
            }

            activeAllies.Add(hero);
            hero.OnDefeated += OnHeroDefeated;
            OnHeroSpawned?.Invoke(hero);

            return hero;
        }

        public void OnAlphaHealthChanged(float cur, float max)
        {
            currentPackHealth = cur;
            maxPackHealth = max;
            OnPackHealthChanged?.Invoke(cur, max);
        }

        public void OnAlphaDied(HeroUnit alpha)
        {
            TriggerGameOver("The Alpha Leader has fallen!");
        }

        private void OnHeroDefeated(Combatant c)
        {
            if (c is HeroUnit hu)
            {
                activeAllies.Remove(hu);
                if (hu == alphaLeader)
                {
                    OnAlphaDied(hu);
                }
            }
        }

        public void DamagePack(float amount)
        {
            if (alphaLeader != null && alphaLeader.IsAlive)
            {
                alphaLeader.TakeDamage(amount);
            }
            else
            {
                currentPackHealth = Mathf.Max(0f, currentPackHealth - amount);
                OnPackHealthChanged?.Invoke(currentPackHealth, maxPackHealth);
                if (currentPackHealth <= 0f)
                {
                    TriggerGameOver("The Pack was wiped out!");
                }
            }
        }

        public void HealPack(float amount)
        {
            if (alphaLeader != null && alphaLeader.IsAlive)
            {
                alphaLeader.Heal(amount);
            }
            else
            {
                currentPackHealth = Mathf.Min(maxPackHealth, currentPackHealth + amount);
                OnPackHealthChanged?.Invoke(currentPackHealth, maxPackHealth);
            }
        }

        public void OnEnemyKilled(Combatant enemy, float energyReward)
        {
            TotalKills++;
            float scoreMult = CampManager.Instance != null ? CampManager.Instance.ScoreMultiplier : 1.0f;
            Score += Mathf.RoundToInt(energyReward * 10f * scoreMult);
            OnScoreChanged?.Invoke(Score);

            // Reward shared energy, morale, and distribute independent energy to cards!
            AddEnergy(1.5f);
            AddMorale(10f);
            FindFirstObjectByType<CardDeckManager>()?.DistributeEnergy(energyReward > 0 ? energyReward : 20f);

            // Spawn radiant energy orbs arcing down towards the card hand
            Vector3 targetBottom = Camera.main != null ? Camera.main.transform.position + Vector3.down * 4.0f : enemy.Position + Vector3.down * 3f;
            for (int i = 0; i < 2; i++)
            {
                Vector3 spread = new Vector3(UnityEngine.Random.Range(-0.9f, 0.9f), UnityEngine.Random.Range(-0.2f, 0.2f), 0f);
                WorldOfThePack.VFX.EnergyOrbVFX.Spawn(enemy.Position, targetBottom + spread, 20f, 0.55f + i * 0.15f);
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing || CurrentState == GameState.Intro)
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

        public void StartEndlessMode()
        {
            if (StoryCampaignDirector.Instance != null)
            {
                StoryCampaignDirector.Instance.SetEndlessMode(true);
            }
            RestartGame();
            GameHUDController hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null) hud.ShowBanner("ENDLESS EXPEDITION BEGUN!\nMARCH NORTH FOREVER!", 3.5f);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;

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
            alphaLeader = null;
            currentEnergy = 5f;
            currentMorale = 0f;
            isWolfInstinctActive = false;

            StartGame();
        }

        public void TriggerGameOver(string reason = "Defeat")
        {
            if (CurrentState == GameState.GameOver) return;
            CurrentState = GameState.GameOver;
            Time.timeScale = 0.4f;

            GameHUDController hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null) hud.ShowBanner(reason.ToUpper(), 3.5f);

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
