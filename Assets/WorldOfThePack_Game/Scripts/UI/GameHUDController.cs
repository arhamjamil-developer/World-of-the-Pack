using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public class GameHUDController : MonoBehaviour
    {
        [Header("Top HUD Elements")]
        [SerializeField] private Image packHealthFill;
        [SerializeField] private TMP_Text packHealthLabel;
        [SerializeField] private TMP_Text waveLabel;
        [SerializeField] private TMP_Text packCountLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button skipIntroButton;

        [Header("Mockup Visual Target Elements")]
        [SerializeField] private Image[] packHearts;
        [SerializeField] private Sprite heartFullSprite;
        [SerializeField] private Sprite heartEmptySprite;
        [SerializeField] private TMP_Text nextCampDistanceText;
        [SerializeField] private Button formationButton;
        [SerializeField] private Button moonshiftButton;
        [SerializeField] private TMP_Text moonshiftCooldownText;

        [Header("Shared Energy & Morale Bars")]
        [SerializeField] private Image sharedEnergyFill;
        [SerializeField] private TMP_Text sharedEnergyLabel;
        [SerializeField] private Image packMoraleFill;
        [SerializeField] private TMP_Text packMoraleLabel;

        [Header("Boss Encounter HUD")]
        [SerializeField] private GameObject bossHealthBarPanel;
        [SerializeField] private TMP_Text bossNameLabel;
        [SerializeField] private TMP_Text bossTitleLabel;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private TMP_Text bossHealthPercentLabel;

        [Header("Camp Modal")]
        [SerializeField] private GameObject campModal;
        [SerializeField] private Button campRestButton;
        [SerializeField] private Button campBuffButton;
        [SerializeField] private Button campRouteForestButton;
        [SerializeField] private Button campRouteCursedButton;
        [SerializeField] private Button campRouteMoonButton;

        public void ShowCampModal(bool show)
        {
            if (campModal != null) campModal.SetActive(show);
        }

        [Header("Banner & Dialogs")]
        [SerializeField] private GameObject bannerPanel;
        [SerializeField] private TMP_Text bannerText;

        [Header("Pause & Options Modal (Reference Flow)")]
        [SerializeField] private GameObject pauseModal;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private GameObject optionsSubPanel;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle vibrationToggle;

        [Header("Game Over & Victory Modals")]
        [SerializeField] private GameObject gameOverModal;
        [SerializeField] private TMP_Text gameOverStatsText;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private Button victoryContinueButton;

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToEvents();
            UpdateAllUI();
        }

        private void SetupButtonListeners()
        {
            if (pauseButton != null) pauseButton.onClick.AddListener(() => GameDirector.Instance.TogglePause());
            if (resumeButton != null) resumeButton.onClick.AddListener(() => GameDirector.Instance.TogglePause());
            if (retryButton != null) retryButton.onClick.AddListener(OnRestartClicked);
            if (victoryContinueButton != null) victoryContinueButton.onClick.AddListener(OnRestartClicked);

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(() =>
                {
                    if (optionsSubPanel != null) optionsSubPanel.SetActive(!optionsSubPanel.activeSelf);
                });
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (skipIntroButton != null)
            {
                skipIntroButton.onClick.AddListener(() =>
                {
                    if (ExpeditionIntroController.Instance != null)
                    {
                        ExpeditionIntroController.Instance.SkipIntro();
                    }
                });
            }

            // Camp Buttons
            if (campRestButton != null) campRestButton.onClick.AddListener(() => CampManager.Instance?.ChooseCampOption(0));
            if (campBuffButton != null) campBuffButton.onClick.AddListener(() => CampManager.Instance?.ChooseCampOption(1));
            if (campRouteForestButton != null) campRouteForestButton.onClick.AddListener(() => CampManager.Instance?.ChooseCampOption(2));
            if (campRouteCursedButton != null) campRouteCursedButton.onClick.AddListener(() => CampManager.Instance?.ChooseCampOption(3));
            if (campRouteMoonButton != null) campRouteMoonButton.onClick.AddListener(() => CampManager.Instance?.ChooseCampOption(4));

            // Mockup Lower Controls
            if (formationButton != null)
            {
                formationButton.onClick.AddListener(() =>
                {
                    if (FormationController.Instance != null)
                    {
                        FormationController.Instance.CycleFormation();
                    }
                });
            }

            if (moonshiftButton != null)
            {
                moonshiftButton.onClick.AddListener(() =>
                {
                    var deck = FindFirstObjectByType<CardDeckManager>();
                    if (deck != null)
                    {
                        deck.ToggleAllForms();
                    }
                });
            }
        }

        private void SubscribeToEvents()
        {
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.OnPackHealthChanged += UpdateHealthBar;
                GameDirector.Instance.OnScoreChanged += UpdateScore;
                GameDirector.Instance.OnEnergyChanged += UpdateEnergyUI;
                GameDirector.Instance.OnMoraleChanged += UpdateMoraleUI;
                GameDirector.Instance.OnWolfInstinctChanged += UpdateWolfInstinctUI;
                GameDirector.Instance.OnStateChanged += UpdateGameStateUI;
            }

            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveStarted += OnWaveStarted;
                wm.OnWaveCompleted += OnWaveCompleted;
                wm.OnAllWavesCompleted += OnAllWavesCompleted;
            }

            BossUnit.OnBossSpawned += HandleBossSpawned;
            BossUnit.OnBossHealthUpdated += HandleBossHealthUpdated;
            BossUnit.OnBossDefeated += HandleBossDefeated;
        }

        private void OnDestroy()
        {
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.OnPackHealthChanged -= UpdateHealthBar;
                GameDirector.Instance.OnScoreChanged -= UpdateScore;
                GameDirector.Instance.OnEnergyChanged -= UpdateEnergyUI;
                GameDirector.Instance.OnMoraleChanged -= UpdateMoraleUI;
                GameDirector.Instance.OnWolfInstinctChanged -= UpdateWolfInstinctUI;
                GameDirector.Instance.OnStateChanged -= UpdateGameStateUI;
            }

            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveStarted -= OnWaveStarted;
                wm.OnWaveCompleted -= OnWaveCompleted;
                wm.OnAllWavesCompleted -= OnAllWavesCompleted;
            }

            BossUnit.OnBossSpawned -= HandleBossSpawned;
            BossUnit.OnBossHealthUpdated -= HandleBossHealthUpdated;
            BossUnit.OnBossDefeated -= HandleBossDefeated;
        }

        private void HandleBossSpawned(BossUnit boss)
        {
            if (bossHealthBarPanel != null) bossHealthBarPanel.SetActive(true);
            if (bossNameLabel != null && boss.Definition != null) bossNameLabel.text = boss.Definition.bossName.ToUpper();
            if (bossTitleLabel != null && boss.Definition != null) bossTitleLabel.text = boss.Definition.bossTitle;
            UpdateBossHealthBar(boss.CurrentHealth, boss.MaxHealth);
            ShowBanner($"BOSS ENCOUNTER!\n{boss.UnitName.ToUpper()}", 3.0f);
        }

        private void HandleBossHealthUpdated(float cur, float max)
        {
            UpdateBossHealthBar(cur, max);
        }

        private void HandleBossDefeated(BossUnit boss)
        {
            UpdateBossHealthBar(0f, boss.MaxHealth);
            ShowBanner($"BOSS DEFEATED!\nMOON FRAGMENT RECOVERED!", 3.5f);
            StartCoroutine(HideBossHealthBarAfter(2.5f));
        }

        private void UpdateBossHealthBar(float cur, float max)
        {
            if (bossHealthFill != null) bossHealthFill.fillAmount = Mathf.Clamp01(cur / Mathf.Max(1f, max));
            if (bossHealthPercentLabel != null) bossHealthPercentLabel.text = $"{Mathf.RoundToInt(cur)} / {Mathf.RoundToInt(max)}";
        }

        private IEnumerator HideBossHealthBarAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bossHealthBarPanel != null) bossHealthBarPanel.SetActive(false);
        }

        private void Update()
        {
            if (GameDirector.Instance != null)
            {
                if (packCountLabel != null)
                {
                    packCountLabel.text = $"ALLIES: {GameDirector.Instance.ActiveAllyCount}/{GameDirector.Instance.MaxAllies}";
                }

                // Hide skip button once intro finishes
                if (skipIntroButton != null && ExpeditionIntroController.Instance != null)
                {
                    skipIntroButton.gameObject.SetActive(ExpeditionIntroController.Instance.IsIntroActive);
                }
            }

            // Update Next Camp distance
            if (nextCampDistanceText != null)
            {
                float traveled = FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f;
                float remaining = Mathf.Max(0f, 260f - (traveled * 4f));
                nextCampDistanceText.text = $"NEXT CAMP • {Mathf.RoundToInt(remaining)}m";
            }
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (packHealthFill != null)
            {
                packHealthFill.fillAmount = Mathf.Clamp01(current / Mathf.Max(1f, max));
            }
            if (packHealthLabel != null)
            {
                packHealthLabel.text = $"ALPHA HP: {Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            }

            if (packHearts != null && packHearts.Length > 0 && heartFullSprite != null && heartEmptySprite != null)
            {
                float fraction = Mathf.Clamp01(current / Mathf.Max(1f, max));
                int activeCount = Mathf.CeilToInt(fraction * packHearts.Length);
                for (int i = 0; i < packHearts.Length; i++)
                {
                    if (packHearts[i] != null)
                    {
                        packHearts[i].sprite = (i < activeCount) ? heartFullSprite : heartEmptySprite;
                    }
                }
            }
        }

        private void UpdateEnergyUI(float current, float max)
        {
            if (sharedEnergyFill != null)
            {
                sharedEnergyFill.fillAmount = Mathf.Clamp01(current / Mathf.Max(1f, max));
            }
            if (sharedEnergyLabel != null)
            {
                sharedEnergyLabel.text = $"ENERGY: {current:F1}/{max:F0}";
            }
        }

        private void UpdateMoraleUI(float current, float max)
        {
            if (packMoraleFill != null)
            {
                packMoraleFill.fillAmount = Mathf.Clamp01(current / Mathf.Max(1f, max));
            }
            if (packMoraleLabel != null)
            {
                packMoraleLabel.text = $"MORALE: {Mathf.RoundToInt(current)}%";
            }
        }

        private void UpdateWolfInstinctUI(bool active, float duration)
        {
            if (active)
            {
                if (packMoraleFill != null) packMoraleFill.color = new Color(1f, 0.85f, 0.2f, 1f);
                if (packMoraleLabel != null) packMoraleLabel.text = $"WOLF INSTINCT! {Mathf.Ceil(duration)}s";
            }
            else
            {
                if (packMoraleFill != null) packMoraleFill.color = new Color(0.3f, 0.65f, 1f, 1f);
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = $"SCORE: {score}";
            }
        }

        private void OnWaveStarted(int waveNum, string waveName)
        {
            if (waveLabel != null)
            {
                waveLabel.text = $"WAVE {waveNum:D2}";
            }
            ShowBanner($"WAVE {waveNum}\n{waveName}");
        }

        private void OnWaveCompleted(int waveNum)
        {
            ShowBanner($"WAVE {waveNum} CLEARED!");
        }

        private void OnAllWavesCompleted()
        {
            if (victoryModal != null) victoryModal.SetActive(true);
        }

        private void UpdateGameStateUI(GameState state)
        {
            if (pauseModal != null)
            {
                pauseModal.SetActive(state == GameState.Paused);
                if (state != GameState.Paused && optionsSubPanel != null)
                {
                    optionsSubPanel.SetActive(false);
                }
            }

            if (gameOverModal != null)
            {
                bool isOver = state == GameState.GameOver;
                gameOverModal.SetActive(isOver);
                if (isOver && gameOverStatsText != null && GameDirector.Instance != null)
                {
                    gameOverStatsText.text = $"KILLS: {GameDirector.Instance.TotalKills} | SCORE: {GameDirector.Instance.Score}";
                }
            }

            if (victoryModal != null) victoryModal.SetActive(state == GameState.Victory);
        }

        public void ShowBanner(string text, float duration = 2.0f)
        {
            if (bannerPanel == null || bannerText == null) return;
            bannerText.text = text;
            bannerPanel.SetActive(true);
            StopCoroutine(nameof(HideBannerRoutine));
            StartCoroutine(HideBannerRoutine(duration));
        }

        private IEnumerator HideBannerRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (bannerPanel != null) bannerPanel.SetActive(false);
        }

        private void OnRestartClicked()
        {
            if (pauseModal != null) pauseModal.SetActive(false);
            if (gameOverModal != null) gameOverModal.SetActive(false);
            if (victoryModal != null) victoryModal.SetActive(false);
            if (GameDirector.Instance != null) GameDirector.Instance.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                OnRestartClicked();
            }
        }

        private void UpdateAllUI()
        {
            if (GameDirector.Instance != null)
            {
                UpdateHealthBar(GameDirector.Instance.CurrentPackHealth, GameDirector.Instance.MaxPackHealth);
                UpdateScore(GameDirector.Instance.Score);
                UpdateEnergyUI(GameDirector.Instance.CurrentEnergy, GameDirector.Instance.MaxEnergy);
                UpdateMoraleUI(GameDirector.Instance.CurrentMorale, GameDirector.Instance.MaxMorale);
            }
        }
    }
}
