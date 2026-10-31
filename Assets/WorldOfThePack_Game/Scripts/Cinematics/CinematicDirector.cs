using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public class CinematicDirector : MonoBehaviour
    {
        public static CinematicDirector Instance { get; private set; }

        [Header("Cinematic UI Overlay")]
        [SerializeField] private GameObject cinematicCanvas;
        [SerializeField] private CanvasGroup cinematicGroup;
        [SerializeField] private Image narrativeBackdrop;
        [SerializeField] private TMP_Text narrativeSubtitle;
        [SerializeField] private Image characterPortraitDisplay;
        [SerializeField] private Button skipCinematicButton;

        [Header("Victory Screen Elements")]
        [SerializeField] private GameObject victoryScreenModal;
        [SerializeField] private TMP_Text victoryStatsText;
        [SerializeField] private Button continueEndlessButton;
        [SerializeField] private Button returnMainMenuButton;

        public bool IsCinematicPlaying { get; private set; } = false;

        public event Action OnOpeningCinematicCompleted;
        public event Action OnEndingCinematicCompleted;

        private Coroutine activeCinematicRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            EnsureUIHierarchy();
        }

        private void EnsureUIHierarchy()
        {
            if (cinematicCanvas == null)
            {
                Transform canvasT = transform.Find("CinematicCanvas");
                if (canvasT != null) cinematicCanvas = canvasT.gameObject;
            }

            if (skipCinematicButton != null)
            {
                skipCinematicButton.onClick.AddListener(SkipActiveCinematic);
            }

            if (continueEndlessButton != null)
            {
                continueEndlessButton.onClick.AddListener(() =>
                {
                    if (victoryScreenModal != null) victoryScreenModal.SetActive(false);
                    if (GameDirector.Instance != null) GameDirector.Instance.StartEndlessMode();
                });
            }

            if (returnMainMenuButton != null)
            {
                returnMainMenuButton.onClick.AddListener(() =>
                {
                    if (victoryScreenModal != null) victoryScreenModal.SetActive(false);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                });
            }
        }

        public void PlayOpeningCinematic()
        {
            if (activeCinematicRoutine != null) StopCoroutine(activeCinematicRoutine);
            activeCinematicRoutine = StartCoroutine(OpeningCinematicRoutine());
        }

        public void PlayEndingCinematic()
        {
            if (activeCinematicRoutine != null) StopCoroutine(activeCinematicRoutine);
            activeCinematicRoutine = StartCoroutine(EndingCinematicRoutine());
        }

        public void SkipActiveCinematic()
        {
            if (!IsCinematicPlaying) return;

            if (activeCinematicRoutine != null)
            {
                StopCoroutine(activeCinematicRoutine);
                activeCinematicRoutine = null;
            }

            HideCinematicUI();
            IsCinematicPlaying = false;

            if (ExpeditionIntroController.Instance != null)
            {
                ExpeditionIntroController.Instance.PlayIntroSequence();
            }

            OnOpeningCinematicCompleted?.Invoke();
        }

        private IEnumerator OpeningCinematicRoutine()
        {
            IsCinematicPlaying = true;
            ShowCinematicUI();

            // SCENE 1: THE GREAT MOON TREE
            yield return ShowSubtitle("Long ago, moonlight protected the world of Wolvia...", 3.2f);

            // SCENE 2: THE FADING LIGHT
            yield return ShowSubtitle("But one night, its light began to disappear...", 3.2f);

            // SCENE 3: THE SEVEN GUARDIANS
            yield return ShowSubtitle("Seven guardians answered the call of the moon.", 3.2f);

            // SCENE 4: HERO WOLVY
            yield return ShowSubtitle("And together, they began a journey beyond the known forest.", 3.5f);

            // SCENE 5: EXPEDITION BEGINS
            yield return FadeOutCinematicUI(1.0f);

            IsCinematicPlaying = false;
            OnOpeningCinematicCompleted?.Invoke();

            if (ExpeditionIntroController.Instance != null)
            {
                ExpeditionIntroController.Instance.PlayIntroSequence();
            }
        }

        private IEnumerator EndingCinematicRoutine()
        {
            IsCinematicPlaying = true;
            ShowCinematicUI();

            // SCENE 1: RETURN TO THE MOON TREE
            yield return ShowSubtitle("The guardians returned to the ancient heart of the forest...", 3.5f);

            // SCENE 2: RESTORING THE LIGHT
            yield return ShowSubtitle("The five Moon Fragments resonated, returning brilliant light to the Great Moon Tree.", 4.0f);

            // SCENE 3: THE PACK TRANSFORMS
            yield return ShowSubtitle("Bathed in celestial radiance, the pack embraced their wild spirit.", 3.5f);

            // SCENE 4: THE FOREST AWAKENS
            yield return ShowSubtitle("The corruption dissolved, and the great wilderness bloomed once more.", 3.5f);

            // SCENE 5: FINAL HERO MOMENT
            yield return ShowSubtitle("The moonlight returned...\nAnd wherever the path may lead, the pack will always journey together.", 4.5f);

            HideCinematicUI();
            IsCinematicPlaying = false;

            // SCENE 6: VICTORY SCREEN
            ShowVictoryScreen();
            OnEndingCinematicCompleted?.Invoke();
        }

        private IEnumerator ShowSubtitle(string text, float duration)
        {
            if (narrativeSubtitle != null)
            {
                narrativeSubtitle.text = text;
                narrativeSubtitle.alpha = 0f;

                // Fade in
                float t = 0f;
                while (t < 0.4f)
                {
                    t += Time.deltaTime;
                    narrativeSubtitle.alpha = t / 0.4f;
                    yield return null;
                }
                narrativeSubtitle.alpha = 1f;

                yield return new WaitForSeconds(duration - 0.8f);

                // Fade out
                t = 0f;
                while (t < 0.4f)
                {
                    t += Time.deltaTime;
                    narrativeSubtitle.alpha = 1f - (t / 0.4f);
                    yield return null;
                }
                narrativeSubtitle.alpha = 0f;
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private void ShowCinematicUI()
        {
            if (cinematicCanvas != null) cinematicCanvas.SetActive(true);
            if (cinematicGroup != null) cinematicGroup.alpha = 1f;
            if (skipCinematicButton != null) skipCinematicButton.gameObject.SetActive(true);
        }

        private void HideCinematicUI()
        {
            if (cinematicCanvas != null) cinematicCanvas.SetActive(false);
            if (skipCinematicButton != null) skipCinematicButton.gameObject.SetActive(false);
        }

        private IEnumerator FadeOutCinematicUI(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (cinematicGroup != null) cinematicGroup.alpha = 1f - (elapsed / duration);
                yield return null;
            }
            HideCinematicUI();
        }

        private void ShowVictoryScreen()
        {
            if (victoryScreenModal != null)
            {
                victoryScreenModal.SetActive(true);
                if (victoryStatsText != null && SaveManager.Instance != null)
                {
                    var data = SaveManager.Instance.Data;
                    victoryStatsText.text = $"CAMPAIGN COMPLETE!\n\n" +
                                            $"Moon Fragments: {data.moonFragmentsCollected}/5\n" +
                                            $"Total Distance: {data.bestDistance:F0}m\n" +
                                            $"Enemies Defeated: {data.totalEnemiesDefeated}\n" +
                                            $"Bosses Conquered: {data.totalBossesDefeated}\n\n" +
                                            $"ENDLESS EXPEDITION MODE UNLOCKED!";
                }
            }
        }
    }
}
