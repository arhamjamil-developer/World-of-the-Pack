using System.Collections;
using TMPro;
using UnityEngine;
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

        [Header("Banner & Dialogs")]
        [SerializeField] private GameObject bannerPanel;
        [SerializeField] private TMP_Text bannerText;
        [SerializeField] private GameObject pauseModal;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
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
            if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(OnRestartClicked);
            if (retryButton != null) retryButton.onClick.AddListener(OnRestartClicked);
            if (victoryContinueButton != null) victoryContinueButton.onClick.AddListener(OnRestartClicked);
        }

        private void SubscribeToEvents()
        {
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.OnPackHealthChanged += UpdateHealthBar;
                GameDirector.Instance.OnScoreChanged += UpdateScore;
                GameDirector.Instance.OnStateChanged += UpdateGameStateUI;
            }

            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveStarted += OnWaveStarted;
                wm.OnWaveCompleted += OnWaveCompleted;
                wm.OnAllWavesCompleted += OnAllWavesCompleted;
            }
        }

        private void Update()
        {
            if (GameDirector.Instance != null && packCountLabel != null)
            {
                packCountLabel.text = $"PACK: {GameDirector.Instance.ActiveAllyCount}/{GameDirector.Instance.MaxAllies}";
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
                packHealthLabel.text = $"PACK HP: {Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
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
                waveLabel.text = $"WAVE {waveNum}: {waveName.ToUpper()}";
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
            if (pauseModal != null) pauseModal.SetActive(state == GameState.Paused);
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

        private void UpdateAllUI()
        {
            if (GameDirector.Instance != null)
            {
                UpdateHealthBar(GameDirector.Instance.CurrentPackHealth, GameDirector.Instance.MaxPackHealth);
                UpdateScore(GameDirector.Instance.Score);
            }
        }
    }
}
