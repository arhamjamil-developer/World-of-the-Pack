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
        private static GameDirector instance;
        public static GameDirector Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<GameDirector>();
                return instance;
            }
            private set => instance = value;
        }

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
        [SerializeField] private int maxAllies = 16;

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
        public float PackHealthPercent => ArenaDirector.Instance != null && ArenaDirector.Instance.Queen != null ? ArenaDirector.Instance.Queen.HealthPercent : alphaLeader != null ? alphaLeader.HealthPercent : Mathf.Clamp01(currentPackHealth / Mathf.Max(1f, maxPackHealth));
        public float CurrentPackHealth => ArenaDirector.Instance != null && ArenaDirector.Instance.Queen != null ? ArenaDirector.Instance.Queen.CurrentHealth : alphaLeader != null ? alphaLeader.CurrentHealth : currentPackHealth;
        public float MaxPackHealth => ArenaDirector.Instance != null && ArenaDirector.Instance.Queen != null ? ArenaDirector.Instance.Queen.MaxHealth : alphaLeader != null ? alphaLeader.MaxHealth : maxPackHealth;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float CurrentMorale => currentMorale;
        public float MaxMorale => maxMorale;
        public bool IsWolfInstinctActive => isWolfInstinctActive;
        public float WolfInstinctTimeRemaining => Mathf.Max(0f, wolfInstinctTimer);

        public int TotalKills { get; private set; }
        public int Score { get; private set; }
        public int MoonCharms { get; private set; } = 24;
        public float RunSeconds { get; private set; }
        public bool IsDecisionPending { get; set; }
        public event Action<int> OnCharmsChanged;
        public event Action OnHeroDeployed;
        public int CharmCost(string id) => id switch
        {
            "ScoutPip" => 3, "PrincessElva" => 4, "CaptainBobo" => 5,
            "FairyLulu" => 6, "WizardMomo" => 7, "TinkerTilly" => 5, _ => 4
        };
        public void AddCharms(int amount)
        {
            MoonCharms = Mathf.Clamp(MoonCharms + Mathf.Max(0, amount), 0, 9999);
            OnCharmsChanged?.Invoke(MoonCharms);
        }
        public bool CanAfford(string id) => MoonCharms >= CharmCost(id);
        public bool TrySpendCharms(int amount)
        {
            if (amount < 0 || MoonCharms < amount || CurrentState != GameState.Playing) return false;
            MoonCharms -= amount; OnCharmsChanged?.Invoke(MoonCharms); return true;
        }
        public HeroUnit RecruitRescuedHero(string id, int price)
        {
            if (CurrentState != GameState.Playing || price < 0 || MoonCharms < price || ActiveAllyCount >= MaxAllies) return null;
            var anchor = FormationController.Instance != null ? FormationController.Instance.AnchorPosition : Vector3.zero;
            var hero = SpawnHeroUnitById(id, anchor + Vector3.down * .2f, false);
            if (hero == null) return null;
            TrySpendCharms(price); OnHeroDeployed?.Invoke(); return hero;
        }
        private void PayForDeployment(string id)
        {
            MoonCharms -= CharmCost(id);
            OnCharmsChanged?.Invoke(MoonCharms);
            OnHeroDeployed?.Invoke();
        }
        public int ActiveAllyCount
        {
            get
            {
                activeAllies.RemoveAll(h => h == null);
                return activeAllies.Count;
            }
        }
        public int MaxAllies => maxAllies;
        public string GameOverReason { get; private set; } = "";

        public void LogCombatMetrics()
        {
            var wm = WaveManager.Instance;
            int spawns = CombatArt.CombatSpriteProjectile.SpawnedCount;
            int impacts = CombatArt.CombatSpriteProjectile.ImpactCount;
            string txt = $"WAVE: {wm?.CurrentWaveIndex} ({wm?.CurrentWaveName}) | KILLS: {TotalKills} | PROJ: spawns={spawns}, impacts={impacts} | ALPHA: {alphaLeader?.CurrentHealth}/{alphaLeader?.MaxHealth} | ALLIES: {activeAllies.Count}";
            System.IO.File.WriteAllText("B:/temp_combat_metrics.txt", txt);
            Debug.Log($"[COMBAT_METRICS] {txt}");
        }

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
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            Application.runInBackground = true;
            deckManager = GetComponent<CardDeckManager>();
            if (GetComponent<ArenaDirector>() == null) gameObject.AddComponent<ArenaDirector>();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
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
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing) return;
            if (IsDecisionPending) return;

            if (CampManager.Instance != null && CampManager.Instance.IsAtCamp) return;
            RunSeconds += Time.deltaTime;
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
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            ArenaDirector.Instance?.ResetRun();

            // Spawn the starting core pack immediately on frame 1
            SpawnStarterPack();

            // Begin pack march immediately
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(true);
            }

            // Start wave 1 right away
            WaveManager wm = WaveManager.Instance != null ? WaveManager.Instance : GetComponent<WaveManager>();
            if (wm != null)
            {
                wm.StartFirstWave();
            }

            // Non-blocking scenic notification
            if (ExpeditionIntroController.Instance != null)
            {
                ExpeditionIntroController.Instance.ShowNotification("Sunny Forest Trail • Wave 1", 2.2f);
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
            activeAllies.RemoveAll(h => h == null || !h.gameObject.activeInHierarchy);
            if (alphaLeader != null && alphaLeader.IsAlive) return;

            // 1. Spawn Alpha Leader Wolvy at the top / front
            Vector3 alphaPos = FormationController.Instance != null
                ? FormationController.Instance.GetSlotWorldPositionByIndex(0)
                : new Vector3(0f, 0.65f, 0f);

            HeroUnit alpha = SpawnHeroUnitById("HeroWolvy", alphaPos, false);
            if (alpha != null)
            {
                alpha.SetAlpha(false);
                alpha.SetAssignedLane(2);
                if (alphaLeader != null)
                {
                    alphaLeader.OnHealthChanged -= OnAlphaHealthChanged;
                }
                alphaLeader = alpha;
                // Queen health owns the run HUD.
                currentPackHealth = alpha.CurrentHealth;
                maxPackHealth = alpha.MaxHealth;
            }

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

            // Reaching full morale readies the player's Moonshift command.
        }

        public void TriggerWolfInstinct(float duration = 10f)
        {
            if (CurrentState != GameState.Playing || IsDecisionPending || isWolfInstinctActive ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) return;
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

        public HeroUnit DeployHeroInLane(HeroCardDefinition def, int laneIndex, float energyCost = 0f)
        {
            if (def == null || CurrentState != GameState.Playing || IsDecisionPending || !CanAfford(def.heroId) ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) return null;
            activeAllies.RemoveAll(h => h == null);
            if (activeAllies.Count >= maxAllies) return null;

            int lane = Mathf.Clamp(laneIndex, 0, LaneSystem.LaneCount - 1);
            float laneX = LaneSystem.GetLaneX(lane);

            // Stagger position slightly for multiple units
            int existingOfSameType = activeAllies.FindAll(h => h != null && h.HeroId == def.heroId).Count;
            float staggerX = existingOfSameType > 0 ? (existingOfSameType % 2 == 1 ? -0.25f : 0.25f) : 0f;
            float staggerY = existingOfSameType * -0.2f;

            Vector3 spawnPos = new Vector3(laneX + staggerX, staggerY, 0f);
            if (FormationController.Instance != null)
            {
                spawnPos = new Vector3(laneX + staggerX, FormationController.Instance.AnchorPosition.y - 0.5f + staggerY, 0f);
            }

            if (ArenaDirector.Instance != null && !ArenaDirector.Instance.TryReserveCell(ref spawnPos, true)) return null;
            HeroUnit unit = SpawnHeroUnitById(def.heroId, spawnPos, false);
            if (unit != null)
            {
                unit.SetAssignedLane(lane);
                ArenaDirector.Instance?.RegisterPosition(unit, ArenaDirector.Instance.SnapDeployment(spawnPos));
                PayForDeployment(def.heroId);
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
            return DeployHeroInLane(def, preferredLane, 0f);
        }

        public HeroUnit DeployHeroAtPosition(HeroCardDefinition def, Vector3 pos)
        {
            if (def == null || CurrentState != GameState.Playing || IsDecisionPending || !CanAfford(def.heroId) ||
                (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) return null;
            activeAllies.RemoveAll(h => h == null);
            if (activeAllies.Count >= maxAllies) return null;

            int lane = Mathf.Clamp(LaneSystem.GetNearestLaneIndex(pos.x), 0, LaneSystem.LaneCount - 1);
            float laneX = LaneSystem.GetLaneX(lane);

            // Keep deployed heroes firmly within the visible battlefield corridor (above Card Hand UI)
            float camY = Camera.main != null ? Camera.main.transform.position.y : (FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f);
            float minAllowedY = camY - 1.5f;
            float maxAllowedY = camY + 2.5f;
            float clampedY = Mathf.Clamp(pos.y, minAllowedY, maxAllowedY);
            Vector3 spawnPos = new Vector3(laneX, clampedY, 0f);
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.Phase != ArenaPhase.March) spawnPos = ArenaDirector.Instance.SnapDeployment(pos);
            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing && !ArenaDirector.Instance.IsValidCell(pos)) return null;
            if (TutorialManager.Instance != null && !TutorialManager.Instance.AllowsPlacement(def.heroId,spawnPos)) return null;
            if (ArenaDirector.Instance != null && !ArenaDirector.Instance.TryReserveCell(ref spawnPos, false)) return null;

            HeroUnit unit = SpawnHeroUnitById(def.heroId, spawnPos, false);
            if (unit != null)
            {
                unit.SetAssignedLane(lane);
                ArenaDirector.Instance?.RegisterPosition(unit, ArenaDirector.Instance.SnapDeployment(spawnPos));
                PayForDeployment(def.heroId);
            }
            return unit;
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

            if (deckManager != null)
            {
                deckManager.LinkSpawnedHeroToCard(hero);
            }
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
            if (ArenaDirector.Instance == null) TriggerGameOver("Wolvy has fallen!");
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
            if (ArenaDirector.Instance != null) { ArenaDirector.Instance.Queen.TakeDamage(amount); return; }
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
            if (ArenaDirector.Instance != null) { ArenaDirector.Instance.Queen.Heal(amount); return; }
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
            if (CurrentState != GameState.Playing) return;
            TotalKills++;
            int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWaveIndex : 0;
            int charmReward = enemy is BossUnit ? 6 : wave < 3 ? 2 : 1;
            float routeMultiplier = WorldOfThePack.Expedition.TrailDecisionDirector.Instance != null ? WorldOfThePack.Expedition.TrailDecisionDirector.Instance.CharmMultiplier : 1;
            AddCharms(Mathf.CeilToInt(charmReward * routeMultiplier));
            float scoreMult = CampManager.Instance != null ? CampManager.Instance.ScoreMultiplier : 1.0f;
            Score += Mathf.RoundToInt(energyReward * 10f * scoreMult);
            OnScoreChanged?.Invoke(Score);

            // Reward shared energy, morale, and distribute independent energy to cards!
            AddEnergy(1.5f);
            AddMorale(10f);
            if (deckManager != null) deckManager.DistributeEnergy(energyReward > 0 ? energyReward : 20f);

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
            CurrentState = GameState.Intro;
            IsDecisionPending = false;
            DamagePopupManager.Instance.ClearPopups();
            CombatVFXPool.Instance.ClearForRun();
            foreach (var pulse in FindObjectsByType<WorldOfThePack.Expedition.PackImpactPulse>(FindObjectsSortMode.None)) Destroy(pulse.gameObject);
            foreach (var marker in FindObjectsByType<TelegraphIndicator>(FindObjectsSortMode.None)) Destroy(marker.gameObject);
            foreach (var orb in FindObjectsByType<WorldOfThePack.VFX.EnergyOrbVFX>(FindObjectsSortMode.None)) Destroy(orb.gameObject);
            foreach (var projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Destroy(projectile.gameObject);
            if (WaveManager.Instance != null) WaveManager.Instance.StopForReset();
            foreach (var boss in FindObjectsByType<BossUnit>(FindObjectsSortMode.None))
            {
                boss.gameObject.SetActive(false);
                Destroy(boss.gameObject);
            }
            foreach (var effect in FindObjectsByType<CombatArt.AttackSpriteEffect>(FindObjectsSortMode.None))
                Destroy(effect.gameObject);
            if (StoryCampaignDirector.Instance != null) StoryCampaignDirector.Instance.ResetRun();

            HeroUnit[] allHeroes = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var hero in allHeroes)
            {
                if (hero != null && hero.gameObject != null)
                {
                    hero.OnDefeated -= OnHeroDefeated;
                    hero.gameObject.SetActive(false);
                    Destroy(hero.gameObject);
                }
            }
            activeAllies.Clear();

            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                if (e != null && e.gameObject != null)
                {
                    e.gameObject.SetActive(false);
                    Destroy(e.gameObject);
                }
            }

            var projectiles = FindObjectsByType<WorldOfThePack.CombatArt.CombatSpriteProjectile>(FindObjectsSortMode.None);
            foreach (var p in projectiles)
            {
                if (p != null && p.gameObject != null)
                {
                    p.gameObject.SetActive(false);
                    Destroy(p.gameObject);
                }
            }

            TotalKills = 0;
            Score = 0;
            MoonCharms = 24;
            RunSeconds = 0;
            OnCharmsChanged?.Invoke(MoonCharms);
            if (alphaLeader != null)
            {
                alphaLeader.OnHealthChanged -= OnAlphaHealthChanged;
            }
            alphaLeader = null;
            currentEnergy = 5f;
            currentMorale = 0f;
            isWolfInstinctActive = false;
            Time.timeScale = 1.0f;
            GameOverReason = "";

            if (FormationController.Instance != null)
            {
                FormationController.Instance.ResetFormationSlots();
            }

            if (FormationController.Instance != null)
                FormationController.Instance.SetAnchorPosition(new Vector3(0f, 0.4f, 0f));
            if (CampManager.Instance != null)
            {
                CampManager.Instance.ResetCampState();
            }

            // 1. Reset World Biome Generator so the background tiles are placed seamlessly without gaps
            if (WorldBiomeGenerator.Instance != null)
            {
                WorldBiomeGenerator.Instance.ResetWorld();
            }

            // 2. Reset Formation Anchor to central starting corridor
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetAnchorPosition(new Vector3(0f, 0.4f, 0f));
                FormationController.Instance.SetMarching(false);
            }

            // 3. Reset Cinemachine Camera framing and damping
            if (CinemachineCombatDirector.Instance != null)
            {
                CinemachineCombatDirector.Instance.ResetDirector();
            }

            // 4. Reset Wave Manager back to Wave 1
            WaveManager wm = WaveManager.Instance != null ? WaveManager.Instance : GetComponent<WaveManager>();
            if (wm != null)
            {
                wm.StopForReset();
            }

            // 5. Reset Card Hand Tray
            if (deckManager != null)
            {
                deckManager.InitializeHand();
            }

            StartGame();
        }

        public void TriggerGameOver(string reason = "Wolvy has fallen!")
        {
            if (CurrentState == GameState.GameOver) return;
            GameOverReason = reason;
            CurrentState = GameState.GameOver;
            Time.timeScale = 0f;

            // Stop waves and pause enemy movement/combat behind the defeat screen
            WaveManager wm = WaveManager.Instance != null ? WaveManager.Instance : GetComponent<WaveManager>();
            if (wm != null)
            {
                wm.PauseForCamp();
            }
            EnemyUnit[] activeEnemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null) enemy.enabled = false;
            }

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
