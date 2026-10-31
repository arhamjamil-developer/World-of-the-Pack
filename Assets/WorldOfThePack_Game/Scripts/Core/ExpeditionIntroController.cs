using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public class ExpeditionIntroController : MonoBehaviour
    {
        public static ExpeditionIntroController Instance { get; private set; }

        [Header("Intro Timings (Seconds)")]
        [SerializeField] private float envRevealDuration = 2.0f;
        [SerializeField] private float heroSpawnDelay = 2.0f;
        [SerializeField] private float packMarchDelay = 4.0f;
        [SerializeField] private float firstEnemyDelay = 6.5f;

        [Header("UI Feedback")]
        [SerializeField] private GameObject skipButtonObj;
        [SerializeField] private GameObject bannerPanel;
        [SerializeField] private TMP_Text bannerText;

        public bool IsIntroActive { get; private set; } = false;
        public event Action OnIntroCompleted;

        private Coroutine introCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }
        }

        public void PlayIntroSequence()
        {
            if (introCoroutine != null) StopCoroutine(introCoroutine);
            introCoroutine = StartCoroutine(IntroSequenceRoutine());
        }

        public void SkipIntro()
        {
            if (!IsIntroActive) return;

            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
                introCoroutine = null;
            }

            // Immediately spawn starter pack if not spawned yet
            if (GameDirector.Instance != null && GameDirector.Instance.ActiveAllyCount == 0)
            {
                GameDirector.Instance.SpawnStarterPack();
            }

            // Start marching
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(true);
            }

            if (bannerPanel != null) bannerPanel.SetActive(false);
            if (skipButtonObj != null) skipButtonObj.SetActive(false);

            IsIntroActive = false;
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.SetPlayingState();
            }

            // Start wave 1 directly
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.StartFirstWave();
            }

            OnIntroCompleted?.Invoke();
        }

        private IEnumerator IntroSequenceRoutine()
        {
            IsIntroActive = true;
            if (skipButtonObj != null) skipButtonObj.SetActive(true);

            // Phase 1: 0 - 2s: Camera reveals the scenic Sunny Forest Trail
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(false);
                FormationController.Instance.SetAnchorPosition(new Vector3(0f, -1.8f, 0f));
            }

            ShowNotification("Sunny Forest Trail", 1.8f);
            yield return new WaitForSeconds(envRevealDuration);

            // Phase 2: 2 - 4s: Starting characters materialize in Human Form
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.SpawnStarterPack();
            }

            yield return new WaitForSeconds(heroSpawnDelay);

            // Phase 3: 4 - 6s: Notification appears & pack begins walking North
            ShowNotification("A New Adventure Begins!", 2.2f);

            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(true);
            }

            yield return new WaitForSeconds(packMarchDelay - heroSpawnDelay);

            if (skipButtonObj != null) skipButtonObj.SetActive(false);
            IsIntroActive = false;
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.SetPlayingState();
            }

            // Phase 4: 6 - 10s: First enemy approaches from North
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.StartFirstWave();
            }

            OnIntroCompleted?.Invoke();
        }

        public void ShowNotification(string message, float duration = 2.0f)
        {
            if (bannerText != null) bannerText.text = message;
            if (bannerPanel != null)
            {
                bannerPanel.SetActive(true);
                StartCoroutine(HideNotificationAfter(duration));
            }
        }

        private IEnumerator HideNotificationAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bannerPanel != null) bannerPanel.SetActive(false);
        }
    }
}
