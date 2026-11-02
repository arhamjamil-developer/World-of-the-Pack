using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Game;
using WorldOfThePack.UI;

namespace WorldOfThePack.NewUI
{
    public sealed class PackGameplayHUD : MonoBehaviour
    {
        public static PackGameplayHUD Instance { get; private set; }
        public TMP_Text waveLabel, campLabel, berryLabel, moonLabel, bossLabel, modalTitle;
        public TMP_Text noticeLabel;
        public Button packStrikeButton;
        public TMP_Text packStrikeLabel;
        private TMP_Text formationLabel;
        private BossUnit currentBoss;
        public Image campFill, berryFill, bossFill;
        public Image[] hearts;
        public Sprite heartFull, heartEmpty;
        public Button pauseButton, formationButton, moonButton, resumeButton, retryButton;
        public Button[] campOptions;
        public GameObject pausePanel, campPanel, endPanel, bossPanel;
        public CardDeckManager deck;
        public HeroDeploymentCard[] cards;
        [SerializeField, Range(1, 99999)] private int _wave = 1;
        [SerializeField, Min(0)] private float _moonCooldownSeconds = 12;
        [SerializeField, Min(.01f)] private float _metersPerWorldUnit = 4;
        private GameDirector _game;
        private CampManager _camp;
        private WaveManager _waves;
        private FormationController _formation;
        private float _cooldown;
        private Coroutine _bannerCoroutine;
        public int Wave => _wave;
        public float MoonCooldown => _cooldown;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _game = GameDirector.Instance;
            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
                if (!string.IsNullOrEmpty(label.text) && label.text.Contains("LEADER HEALTH")) label.text = "QUEEN HEALTH";
            _camp = CampManager.Instance;
            _waves = WaveManager.Instance;
            _formation = FormationController.Instance;
            pauseButton.onClick.AddListener(TogglePause);
            resumeButton.onClick.AddListener(TogglePause);
            retryButton.onClick.AddListener(() => _game?.RestartGame());
            formationButton.onClick.AddListener(ChangeFormation);
            formationLabel = formationButton.GetComponentInChildren<TMP_Text>();
            if (packStrikeButton != null) packStrikeButton.onClick.AddListener(() => currentBoss?.TryPackStrike());
            moonButton.onClick.AddListener(ActivateMoonshift);
            for (int i = 0; i < campOptions.Length; i++)
            {
                int option = i;
                campOptions[i].onClick.AddListener(() => _camp?.ChooseCampOption(option));
            }
            if (campOptions != null)
            {
                if (campOptions.Length > 0 && campOptions[0] != null)
                {
                    var tmp = campOptions[0].GetComponentInChildren<TMP_Text>();
                    if (tmp != null) tmp.text = "RESTORE 25% QUEEN HEALTH";
                }
                if (campOptions.Length > 1 && campOptions[1] != null)
                {
                    var tmp = campOptions[1].GetComponentInChildren<TMP_Text>();
                    if (tmp != null) tmp.text = "PACK POWER • +20% ATTACK DAMAGE";
                }
                if (campOptions.Length > 2 && campOptions[2] != null)
                {
                    var tmp = campOptions[2].GetComponentInChildren<TMP_Text>();
                    if (tmp != null) tmp.text = "MOON CHARGE • +30% RECHARGE RATE";
                }
                for (int i = 3; i < campOptions.Length; i++)
                {
                    if (campOptions[i] != null)
                    {
                        campOptions[i].gameObject.SetActive(false);
                        var tmpSibling = campOptions[i].transform.parent != null ? campOptions[i].transform.parent.Find(campOptions[i].name + "_TMP") : null;
                        if (tmpSibling != null) tmpSibling.gameObject.SetActive(false);
                    }
                }
            }
            if (_game != null)
            {
                _game.OnPackHealthChanged += SetHearts;
                _game.OnStateChanged += ResetRunPresentation;
                SetHearts(_game.CurrentPackHealth, _game.MaxPackHealth);
            }
            if (_waves != null)
            {
                _waves.OnWaveStarted += OnWave;
                SetWave(_waves.CurrentWaveIndex + 1);
            }
            BossUnit.OnBossSpawned += ShowBoss;
            BossUnit.OnBossHealthUpdated += SetBossHealth;
            BossUnit.OnBossDefeated += HideBoss;
            SetWave(_wave);
        }
        private void OnDestroy()
        {
            if (_game != null) { _game.OnPackHealthChanged -= SetHearts; _game.OnStateChanged -= ResetRunPresentation; }
            if (_waves != null) _waves.OnWaveStarted -= OnWave;
            BossUnit.OnBossSpawned -= ShowBoss;
            BossUnit.OnBossHealthUpdated -= SetBossHealth;
            BossUnit.OnBossDefeated -= HideBoss;
        }
        private void Update()
        {
            if (_game == null) return;
            bool playing = _game.CurrentState == GameState.Playing && !_game.IsDecisionPending;
            if (formationLabel != null && _formation != null) formationLabel.text = _formation.FormationName;
            if (packStrikeButton != null)
            {
                bool bossAlive = currentBoss != null && currentBoss.IsAlive;
                packStrikeButton.gameObject.SetActive(bossAlive && (ArenaDirector.Instance == null || !ArenaDirector.Instance.IsPreparing));
                packStrikeButton.interactable = playing && bossAlive && currentBoss.CanPackStrike;
                if (bossAlive && packStrikeLabel != null)
                    packStrikeLabel.text = currentBoss.IsWindingUp ? "SLAM — GUARD NOW" : currentBoss.CanPackStrike ? "PACK STRIKE — NOW!" :
                        currentBoss.IsExposed ? "BOSS EXPOSED — FOCUS!" : $"PACK STRIKE • {Mathf.Min(3, currentBoss.PackContributors)}/3 HEROES";
            }
            if (playing && (ArenaDirector.Instance == null || !ArenaDirector.Instance.IsPreparing))
                _cooldown = Mathf.Max(0, _cooldown - Time.deltaTime);
            bool atCamp = _camp != null && _camp.IsAtCamp;
            moonButton.interactable = playing && !atCamp && !_game.IsWolfInstinctActive && _cooldown <= 0 && _game.CurrentMorale >= 50;
            moonLabel.text = _game.IsWolfInstinctActive ? $"NIGHT • {Mathf.CeilToInt(_game.WolfInstinctTimeRemaining)}s" :
                _cooldown > 0 ? $"MOON • {Mathf.CeilToInt(_cooldown)}s" :
                _game.CurrentMorale < 50 ? $"MOON • {Mathf.FloorToInt(_game.CurrentMorale)}/50" : "MOONSHIFT";
            if (_camp != null)
            {
                if (_bannerCoroutine == null)
                {
                    if (_camp.IsAtCamp)
                    {
                        campLabel.text = "CAMP REACHED";
                        campFill.fillAmount = 1f;
                    }
                    else
                    {
                        campLabel.text = $"NEXT CAMP • {Mathf.CeilToInt(_camp.RemainingDistance * _metersPerWorldUnit)}m";
                        campFill.fillAmount = _camp.LegProgress;
                    }
                }
                campPanel.SetActive(_camp.IsAtCamp);
            }
            berryLabel.text = $"CHARMS  {_game.MoonCharms}";
            if (berryFill != null) berryFill.fillAmount = Mathf.Clamp01(_game.MoonCharms / 20f);
            pausePanel.SetActive(_game.CurrentState == GameState.Paused);
            bool ended = _game.CurrentState == GameState.GameOver || _game.CurrentState == GameState.Victory;
            endPanel.SetActive(ended);
            if (ended)
            {
                modalTitle.text = _game.CurrentState == GameState.Victory ? "TRAIL COMPLETE" : "THE QUEEN HAS FALLEN";
            }
            foreach (var card in cards) if (card != null && (!playing || atCamp)) card.deployButton.interactable = false;
        }
        public void TogglePause() => _game?.TogglePause();
        public void ChangeFormation()
        {
            if (_game == null || _game.CurrentState != GameState.Playing || _game.IsDecisionPending) return;
            _formation?.CycleFormation();
            ShowBanner(_formation.CurrentFormationIndex == 1 ? "GUARD • -30% incoming damage, -20% attack" :
                _formation.CurrentFormationIndex == 2 ? "FOCUS • +20% attack, +20% damage taken" : "MARCH • Balanced formation", 2);
        }
        public void SetWave(int value)
        {
            _wave = Mathf.Clamp(value, 1, WaveManager.MaxWaveNumber);
            if (waveLabel != null) waveLabel.text = _wave < 100 ? $"WAVE {_wave:00}" : $"WAVE {_wave}";
        }
        private void OnWave(int number, string title) => SetWave(number);
        public void SetHearts(float current, float maximum)
        {
            float count = Mathf.Clamp01(current / Mathf.Max(1, maximum)) * hearts.Length;
            for (int i = 0; i < hearts.Length; i++) hearts[i].sprite = i < Mathf.CeilToInt(count) ? heartFull : heartEmpty;
        }
        public void ActivateMoonshift()
        {
            if (_game == null || _game.CurrentState != GameState.Playing || _game.IsDecisionPending || _cooldown > 0 ||
                _game.IsWolfInstinctActive || _game.CurrentMorale < 50 || (_camp != null && _camp.IsAtCamp)) return;
            _game.TriggerWolfInstinct(10f);
            _cooldown = Mathf.Max(18f, _moonCooldownSeconds);
        }
        private void ResetRunPresentation(GameState state)
        {
            if (state != GameState.Playing || _game.RunSeconds > .05f) return;
            _cooldown = 0;
            SetWave(1);
            bossPanel.SetActive(false);
            currentBoss = null;
            if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = null;
            if (noticeLabel != null) { noticeLabel.text = string.Empty; noticeLabel.gameObject.SetActive(false); }
        }
        private void ShowBoss(BossUnit boss)
        {
            currentBoss = boss;
            bossPanel.SetActive(true);
            bossLabel.text = boss.UnitName.ToUpperInvariant();
            SetBossHealth(boss.CurrentHealth, boss.MaxHealth);
        }
        private void SetBossHealth(float value, float max) => bossFill.fillAmount = Mathf.Clamp01(value / Mathf.Max(1, max));
        private void HideBoss(BossUnit boss) { currentBoss = null; bossPanel.SetActive(false); }

        public void ShowBanner(string text, float duration = 2.5f)
        {
            if (!gameObject.activeInHierarchy) return;
            if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = StartCoroutine(BannerRoutine(text, duration));
        }

        private System.Collections.IEnumerator BannerRoutine(string text, float duration)
        {
            if (noticeLabel != null) { noticeLabel.text = text; noticeLabel.gameObject.SetActive(true); }
            yield return new WaitForSeconds(duration);
            if (noticeLabel != null) noticeLabel.gameObject.SetActive(false);
            if (campLabel != null && _camp != null)
            {
                campLabel.text = _camp.IsAtCamp ? "CAMP REACHED" : $"NEXT CAMP • {Mathf.CeilToInt(_camp.RemainingDistance * _metersPerWorldUnit)}m";
            }
            _bannerCoroutine = null;
        }
    }
}
