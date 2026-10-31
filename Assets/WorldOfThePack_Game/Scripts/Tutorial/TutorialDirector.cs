using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public class TutorialDirector : MonoBehaviour
    {
        public static TutorialDirector Instance { get; private set; }

        [Header("Tutorial UI Components")]
        [SerializeField] private GameObject tutorialOverlay;
        [SerializeField] private TMP_Text stepTitleText;
        [SerializeField] private TMP_Text stepDescriptionText;
        [SerializeField] private Button nextStepButton;
        [SerializeField] private Button skipTutorialButton;
        [SerializeField] private RectTransform spotlightRect;

        [Header("Tutorial State")]
        private int currentStepIndex = 0;
        private bool isTutorialActive = false;

        public bool IsTutorialActive => isTutorialActive;

        private static readonly string[] StepTitles =
        {
            "STEP 1: EXPEDITION MOVEMENT",
            "STEP 2: AUTOMATIC ENGAGEMENT",
            "STEP 3: CARD DEPLOYMENT",
            "STEP 4: SHARED EXPEDITION ENERGY",
            "STEP 5: WOLF TRANSFORMATION",
            "STEP 6: SPECIAL ABILITIES",
            "STEP 7: PACK FORMATION TACTICS",
            "STEP 8: ATTACK TELEGRAPHS",
            "STEP 9: LANTERN REFUGE CAMPS",
            "STEP 10: MOON FRAGMENTS & PROGRESSION"
        };

        private static readonly string[] StepDescriptions =
        {
            "Your wolf pack automatically marches north through the shifting wilderness of Wolvia. The camera follows your advance.",
            "Enemies approach from the north. Frontline allies automatically detect and intercept approaching threats within range.",
            "Drag a hero card from the bottom hand onto an open lane to deploy reinforcements into the pack!",
            "Deploying heroes consumes shared expedition energy. Energy regenerates continuously and refills upon defeating enemy units.",
            "Tap MOONSHIFT or fill an active hero's meter to unleash their ferocious Wolf Form with bonus damage and speed!",
            "In wolf form, heroes unleash powerful unique specials like Wolvy's Paw Dash and Bobo's Acorn Slam!",
            "Tanks protect the front row, rangers and skirmishers hold the middle, while healers and mages support from the back.",
            "Beware of red ground telegraphs! Bosses and elite captains charge heavy attacks that deal severe area damage.",
            "Campsites appear along your expedition route. Rest your heroes, restore vitality, purchase upgrades, and choose new routes.",
            "Recover all 5 scattered Moon Fragments to restore the Great Moon Tree and unlock Endless Expedition Mode!"
        };

        public event Action OnTutorialCompleted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            EnsureUIHierarchy();
        }

        private void EnsureUIHierarchy()
        {
            if (tutorialOverlay == null)
            {
                Canvas hudCanvas = FindFirstObjectByType<Canvas>();
                if (hudCanvas != null)
                {
                    GameObject tutObj = new GameObject("TutorialOverlay");
                    tutObj.transform.SetParent(hudCanvas.transform, false);
                    RectTransform rt = tutObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.08f, 0.35f);
                    rt.anchorMax = new Vector2(0.92f, 0.65f);
                    rt.offsetMin = rt.offsetMax = Vector2.zero;

                    Image bg = tutObj.AddComponent<Image>();
                    bg.color = new Color(0.08f, 0.12f, 0.18f, 0.98f);

                    TMP_FontAsset font = null;
#if UNITY_EDITOR
                    font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/WorldOfThePack/Fonts/LilitaOne-Dynamic.asset");
#endif

                    // Title
                    GameObject titleObj = new GameObject("StepTitle");
                    titleObj.transform.SetParent(tutObj.transform, false);
                    RectTransform trt = titleObj.AddComponent<RectTransform>();
                    trt.anchorMin = new Vector2(0.05f, 0.72f); trt.anchorMax = new Vector2(0.95f, 0.95f);
                    trt.offsetMin = trt.offsetMax = Vector2.zero;
                    stepTitleText = titleObj.AddComponent<TextMeshProUGUI>();
                    if (font != null) stepTitleText.font = font;
                    stepTitleText.alignment = TextAlignmentOptions.Center;
                    stepTitleText.fontSize = 28;
                    stepTitleText.color = new Color(1f, 0.88f, 0.3f);

                    // Description
                    GameObject descObj = new GameObject("StepDescription");
                    descObj.transform.SetParent(tutObj.transform, false);
                    RectTransform drt = descObj.AddComponent<RectTransform>();
                    drt.anchorMin = new Vector2(0.06f, 0.30f); drt.anchorMax = new Vector2(0.94f, 0.70f);
                    drt.offsetMin = drt.offsetMax = Vector2.zero;
                    stepDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
                    if (font != null) stepDescriptionText.font = font;
                    stepDescriptionText.alignment = TextAlignmentOptions.Center;
                    stepDescriptionText.fontSize = 22;
                    stepDescriptionText.color = Color.white;

                    // Next button
                    GameObject nextBtnObj = new GameObject("NextButton");
                    nextBtnObj.transform.SetParent(tutObj.transform, false);
                    RectTransform nrt = nextBtnObj.AddComponent<RectTransform>();
                    nrt.anchorMin = new Vector2(0.52f, 0.06f); nrt.anchorMax = new Vector2(0.92f, 0.26f);
                    nrt.offsetMin = nrt.offsetMax = Vector2.zero;
                    Image nImg = nextBtnObj.AddComponent<Image>();
                    nImg.color = new Color(0.25f, 0.75f, 0.35f, 1f);
                    nextStepButton = nextBtnObj.AddComponent<Button>();

                    GameObject nTxtObj = new GameObject("Text");
                    nTxtObj.transform.SetParent(nextBtnObj.transform, false);
                    RectTransform ntrt = nTxtObj.AddComponent<RectTransform>();
                    ntrt.anchorMin = Vector2.zero; ntrt.anchorMax = Vector2.one; ntrt.offsetMin = ntrt.offsetMax = Vector2.zero;
                    TextMeshProUGUI nTmp = nTxtObj.AddComponent<TextMeshProUGUI>();
                    if (font != null) nTmp.font = font;
                    nTmp.alignment = TextAlignmentOptions.Center;
                    nTmp.fontSize = 24;
                    nTmp.text = "CONTINUE  ➡";
                    nTmp.color = Color.white;

                    // Skip button
                    GameObject skipBtnObj = new GameObject("SkipButton");
                    skipBtnObj.transform.SetParent(tutObj.transform, false);
                    RectTransform srt = skipBtnObj.AddComponent<RectTransform>();
                    srt.anchorMin = new Vector2(0.08f, 0.06f); srt.anchorMax = new Vector2(0.48f, 0.26f);
                    srt.offsetMin = srt.offsetMax = Vector2.zero;
                    Image sImg = skipBtnObj.AddComponent<Image>();
                    sImg.color = new Color(0.35f, 0.38f, 0.44f, 1f);
                    skipTutorialButton = skipBtnObj.AddComponent<Button>();

                    GameObject sTxtObj = new GameObject("Text");
                    sTxtObj.transform.SetParent(skipBtnObj.transform, false);
                    RectTransform strt = sTxtObj.AddComponent<RectTransform>();
                    strt.anchorMin = Vector2.zero; strt.anchorMax = Vector2.one; strt.offsetMin = strt.offsetMax = Vector2.zero;
                    TextMeshProUGUI sTmp = sTxtObj.AddComponent<TextMeshProUGUI>();
                    if (font != null) sTmp.font = font;
                    sTmp.alignment = TextAlignmentOptions.Center;
                    sTmp.fontSize = 20;
                    sTmp.text = "SKIP TUTORIAL";
                    sTmp.color = Color.white;

                    tutorialOverlay = tutObj;
                    tutorialOverlay.SetActive(false);
                }
            }

            if (nextStepButton != null) nextStepButton.onClick.AddListener(AdvanceStep);
            if (skipTutorialButton != null) skipTutorialButton.onClick.AddListener(SkipTutorial);
        }

        public void CheckAndStartTutorial()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data.isTutorialCompleted)
            {
                return; // Tutorial already finished
            }

            StartTutorial();
        }

        public void StartTutorial()
        {
            isTutorialActive = true;
            currentStepIndex = 0;
            if (tutorialOverlay != null) tutorialOverlay.SetActive(true);
            ShowStep(currentStepIndex);
        }

        public void AdvanceStep()
        {
            currentStepIndex++;
            if (currentStepIndex >= StepTitles.Length)
            {
                CompleteTutorial();
            }
            else
            {
                ShowStep(currentStepIndex);
            }
        }

        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            isTutorialActive = false;
            if (tutorialOverlay != null) tutorialOverlay.SetActive(false);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.isTutorialCompleted = true;
                SaveManager.Instance.Save();
            }

            Time.timeScale = 1f;
            OnTutorialCompleted?.Invoke();
        }

        private void ShowStep(int index)
        {
            if (index < 0 || index >= StepTitles.Length) return;

            if (stepTitleText != null) stepTitleText.text = StepTitles[index];
            if (stepDescriptionText != null) stepDescriptionText.text = StepDescriptions[index];

            // Slow down time briefly to let player read
            Time.timeScale = 0.25f;
        }
    }
}
