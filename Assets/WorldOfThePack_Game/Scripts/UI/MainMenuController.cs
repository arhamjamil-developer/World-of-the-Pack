using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public class MainMenuController : MonoBehaviour
    {
        public static MainMenuController Instance { get; private set; }

        [Header("Menu Panels")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject modePanel;
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private GameObject deckBuilderPanel;
        [SerializeField] private GameObject upgradesPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Home Navigation")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button endlessButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button upgradesButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private TMP_Text moonShardsLabel;

        [Header("Mode Selection")]
        [SerializeField] private Button campaignStartButton;
        [SerializeField] private Button endlessStartButton;
        [SerializeField] private GameObject endlessLockedBadge;
        [SerializeField] private TMP_Text campaignChapterLabel;

        [Header("Collection & Deck Panels")]
        [SerializeField] private Transform collectionGridParent;
        [SerializeField] private Button collectionBackButton;
        [SerializeField] private Button deckBuilderBackButton;

        [Header("Upgrades Panel")]
        [SerializeField] private TMP_Text shardsCurrencyText;
        [SerializeField] private Button upgradeHealthButton;
        [SerializeField] private Button upgradeEnergyButton;
        [SerializeField] private Button upgradeMoraleButton;
        [SerializeField] private Button upgradesBackButton;

        [Header("Options Panel")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Button replayIntroButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button optionsBackButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(this); return; }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            SetupButtonListeners();
            RefreshSaveDataUI();
            ShowHome();
        }

        private void SetupButtonListeners()
        {
            // Home buttons
            if (playButton != null) playButton.onClick.AddListener(ShowModeSelect);
            if (endlessButton != null) endlessButton.onClick.AddListener(OnEndlessClicked);
            if (collectionButton != null) collectionButton.onClick.AddListener(ShowCollection);
            if (deckBuilderButton != null) deckBuilderButton.onClick.AddListener(ShowDeckBuilder);
            if (upgradesButton != null) upgradesButton.onClick.AddListener(ShowUpgrades);
            if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);

            // Back buttons
            if (collectionBackButton != null) collectionBackButton.onClick.AddListener(ShowHome);
            if (deckBuilderBackButton != null) deckBuilderBackButton.onClick.AddListener(ShowHome);
            if (upgradesBackButton != null) upgradesBackButton.onClick.AddListener(ShowHome);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(ShowHome);

            // Mode launches
            if (campaignStartButton != null) campaignStartButton.onClick.AddListener(() => LaunchGameplay("Story"));
            if (endlessStartButton != null) endlessStartButton.onClick.AddListener(() => LaunchGameplay("Endless"));

            // Upgrades
            if (upgradeHealthButton != null) upgradeHealthButton.onClick.AddListener(() => PurchaseUpgrade("PackHealth", 50));
            if (upgradeEnergyButton != null) upgradeEnergyButton.onClick.AddListener(() => PurchaseUpgrade("EnergyRegen", 60));
            if (upgradeMoraleButton != null) upgradeMoraleButton.onClick.AddListener(() => PurchaseUpgrade("MoraleSurge", 75));

            // Settings
            if (replayIntroButton != null) replayIntroButton.onClick.AddListener(ReplayOpeningCinematic);
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(ResetSaveData);

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(v => {
                    AudioListener.volume = v;
                    if (SaveManager.Instance != null) SaveManager.Instance.Data.masterVolume = v;
                });
            }
        }

        public void RefreshSaveDataUI()
        {
            if (SaveManager.Instance == null) return;
            var data = SaveManager.Instance.Data;

            if (moonShardsLabel != null) moonShardsLabel.text = $"{data.moonShards} SHARDS";
            if (shardsCurrencyText != null) shardsCurrencyText.text = $"MOON SHARDS: {data.moonShards}";

            if (campaignChapterLabel != null)
            {
                campaignChapterLabel.text = $"CHAPTER {data.currentChapter}: {GetChapterName(data.currentChapter)}";
            }

            // Both advertised modes must be immediately playable. Campaign progression
            // still records chapters, but Endless is a separate, always-available run.
            if (endlessLockedBadge != null) endlessLockedBadge.SetActive(false);
            if (endlessStartButton != null) endlessStartButton.interactable = true;
        }

        private void OnEndlessClicked()
        {
            LaunchGameplay("Endless");
        }

        private void PurchaseUpgrade(string key, int cost)
        {
            if (SaveManager.Instance != null && SaveManager.Instance.SpendMoonShards(cost))
            {
                int cur = SaveManager.Instance.Data.GetUpgradeLevel(key);
                SaveManager.Instance.Data.SetUpgradeLevel(key, cur + 1);
                SaveManager.Instance.Save();
                RefreshSaveDataUI();
            }
        }

        private void ReplayOpeningCinematic()
        {
            PlayerPrefs.SetInt("ReplayIntroCinematic", 1);
            PlayerPrefs.Save();
            LaunchGameplay("Story");
        }

        private void ResetSaveData()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ResetProgress();
                RefreshSaveDataUI();
            }
        }

        public void ShowHome()
        {
            SetAllPanelsInactive();
            if (homePanel != null) homePanel.SetActive(true);
            RefreshSaveDataUI();
        }

        public void ShowModeSelect()
        {
            SetAllPanelsInactive();
            if (modePanel != null) modePanel.SetActive(true);
            RefreshSaveDataUI();
        }

        public void ShowCollection()
        {
            SetAllPanelsInactive();
            if (collectionPanel != null) collectionPanel.SetActive(true);
        }

        public void ShowDeckBuilder()
        {
            SetAllPanelsInactive();
            if (deckBuilderPanel != null) deckBuilderPanel.SetActive(true);
        }

        public void ShowUpgrades()
        {
            SetAllPanelsInactive();
            if (upgradesPanel != null) upgradesPanel.SetActive(true);
            RefreshSaveDataUI();
        }

        public void ShowOptions()
        {
            SetAllPanelsInactive();
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void SetAllPanelsInactive()
        {
            if (homePanel != null) homePanel.SetActive(false);
            if (modePanel != null) modePanel.SetActive(false);
            if (collectionPanel != null) collectionPanel.SetActive(false);
            if (deckBuilderPanel != null) deckBuilderPanel.SetActive(false);
            if (upgradesPanel != null) upgradesPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        public void LaunchGameplay(string mode)
        {
            PlayerPrefs.SetString("GameMode", mode);
            PlayerPrefs.Save();
            SceneManager.LoadScene("SampleScene");
        }

        private string GetChapterName(int chapter)
        {
            return chapter switch
            {
                1 => "Sunny Forest Trail",
                2 => "Mushroom Grove",
                3 => "River Meadow",
                4 => "Emberlane Refuge",
                5 => "Cloudstep Highlands",
                _ => "Endless Wilderness"
            };
        }
    }
}
