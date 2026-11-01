using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

namespace WorldOfThePack.Game
{
    public class CinemachineCombatDirector : MonoBehaviour
    {
        private static CinemachineCombatDirector instance;
        public static CinemachineCombatDirector Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<CinemachineCombatDirector>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("Combat Clash Tracking Variable (Gameplay Alignment)")]
        [Tooltip("Real-time tracked distance between the leading ally and the nearest incoming enemy")]
        [SerializeField] private float combatClashDistance = 99f;
        [Tooltip("Center Y of the clash zone between enemy crew and ally pack")]
        [SerializeField] private float clashCenterY = 0f;
        [Tooltip("True when enemies are actively engaged in combat with the pack")]
        [SerializeField] private bool hasActiveCombatClash = false;

        public float CombatClashDistance => combatClashDistance;
        public float ClashCenterY => clashCenterY;
        public bool HasActiveCombatClash => hasActiveCombatClash;

        [Header("Cinemachine Camera Settings (Safe Above Bottom Cards)")]
        [SerializeField] private float defaultOrthoSize = 6.2f;
        [SerializeField] private float minHorizontalWidth = 5.5f;
        [SerializeField] private float verticalLead = 0.95f;
        [SerializeField] private float combatLockDamping = 0.35f;

        [Header("Screen Shake / Juice")]
        [SerializeField] private float shakeDecay = 4.0f;

        private CinemachineBrain brain;
        private CinemachineCamera vcam;
        private Transform trackingTarget;
        private bool isEncounterLocked = false;
        private Vector3 lockPosition;
        private float currentShakeIntensity = 0f;
        private Vector3 cameraVelocity;

        public bool IsEncounterLocked => isEncounterLocked;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            SetupCinemachine();
        }

        private void Start()
        {
            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveStarted += OnWaveStarted;
                wm.OnWaveCompleted += OnWaveCompleted;
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;

            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveStarted -= OnWaveStarted;
                wm.OnWaveCompleted -= OnWaveCompleted;
            }
        }

        private void SetupCinemachine()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            // 1. Ensure CinemachineBrain is on Main Camera
            brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
            }

            // Disable legacy camera script to avoid conflicting transforms
            ExpeditionCamera expCam = mainCam.GetComponent<ExpeditionCamera>();
            if (expCam != null)
            {
                expCam.enabled = false;
            }

            // 2. Create or find Virtual Camera
            GameObject vcamObj = GameObject.Find("Cinemachine_ExpeditionCam");
            if (vcamObj == null)
            {
                vcamObj = new GameObject("Cinemachine_ExpeditionCam");
            }

            vcam = vcamObj.GetComponent<CinemachineCamera>();
            if (vcam == null)
            {
                vcam = vcamObj.AddComponent<CinemachineCamera>();
            }

            // Create tracking target GameObject
            GameObject targetObj = GameObject.Find("Cinemachine_TrackingTarget");
            if (targetObj == null)
            {
                targetObj = new GameObject("Cinemachine_TrackingTarget");
            }
            trackingTarget = targetObj.transform;

            vcam.Follow = trackingTarget;

            var follow = vcamObj.GetComponent<CinemachineFollow>();
            if (follow == null) follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 0f, -10f);

            // Configure orthographic lens
            var lens = vcam.Lens;
            lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            float aspect = Mathf.Max(0.1f, mainCam.aspect);
            float requiredVerticalForWidth = (minHorizontalWidth * 0.5f) / aspect;
            lens.OrthographicSize = Mathf.Max(defaultOrthoSize, requiredVerticalForWidth);
            vcam.Lens = lens;

            // Initial positioning
            if (FormationController.Instance != null)
            {
                trackingTarget.position = FormationController.Instance.AnchorPosition + Vector3.up * verticalLead;
                vcamObj.transform.position = new Vector3(0f, trackingTarget.position.y, -10f);
            }
        }

        private void LateUpdate()
        {
            UpdateTrackingTarget();
            UpdateOrthoFraming();
            UpdateScreenShake();
        }

        private void UpdateTrackingTarget()
        {
            if (trackingTarget == null || FormationController.Instance == null) return;

            Vector3 currentPos = trackingTarget.position;
            Vector3 anchor = FormationController.Instance.AnchorPosition;
            Vector3 desiredPos;
            if (ArenaDirector.Instance != null)
            {
                // Keep the throne above the card hand even when a distant squad is staged.
                isEncounterLocked = false;
                trackingTarget.position = Vector3.SmoothDamp(currentPos, new Vector3(0,anchor.y+1f,0), ref cameraVelocity,.2f);
                return;
            }

            // Search for active enemies to calculate real-time combat clash distance & midpoint
            EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);

            if (enemies != null && enemies.Length > 0 && allies != null && allies.Length > 0)
            {
                hasActiveCombatClash = true;
                float nearestEnemyY = float.MaxValue;
                float leadAllyY = float.MinValue;

                foreach (var e in enemies)
                {
                    if (e != null && e.IsAlive && e.transform.position.y < nearestEnemyY)
                    {
                        nearestEnemyY = e.transform.position.y;
                    }
                }

                foreach (var a in allies)
                {
                    if (a != null && a.IsAlive && a.transform.position.y > leadAllyY)
                    {
                        leadAllyY = a.transform.position.y;
                    }
                }

                if (nearestEnemyY < 99f && leadAllyY > -99f)
                {
                    combatClashDistance = Mathf.Max(0f, nearestEnemyY - leadAllyY);
                    clashCenterY = (leadAllyY + nearestEnemyY) * 0.5f;

                    // When enemies are within visual clash range (< 4.5 units), lock framing to clash center
                    if (combatClashDistance < 4.5f)
                    {
                        isEncounterLocked = true;
                        // Frame midpoint between our pack and the enemy squad
                        desiredPos = new Vector3(0f, clashCenterY - 0.20f, 0f);
                    }
                    else
                    {
                        isEncounterLocked = false;
                        desiredPos = new Vector3(0f, anchor.y + verticalLead, 0f);
                    }
                }
                else
                {
                    hasActiveCombatClash = false;
                    isEncounterLocked = false;
                    desiredPos = new Vector3(0f, anchor.y + verticalLead, 0f);
                }
            }
            else
            {
                hasActiveCombatClash = false;
                isEncounterLocked = false;
                desiredPos = new Vector3(0f, anchor.y + verticalLead, 0f);
            }

            // CRITICAL REQUIREMENT: "ensure everything is visible upper the bottom cards"
            // Bottom card hand sits up to viewport Y ~ -2.26.
            // Clamping camera desiredPos.y >= anchor.y + 0.85f guarantees that all heroes and their
            // formation slots remain strictly framed in the center corridor ABOVE the cards!
            desiredPos.y = Mathf.Max(desiredPos.y, anchor.y + 0.85f);

            // Apply smooth damp towards desired framing (cinematic, silky smooth, non-jittery)
            float smoothTime = isEncounterLocked ? 0.25f : 0.16f;
            currentPos = Vector3.SmoothDamp(currentPos, desiredPos, ref cameraVelocity, smoothTime);

            trackingTarget.position = currentPos;
        }

        public void ResetDirector()
        {
            isEncounterLocked = false;
            currentShakeIntensity = 0f;
            cameraVelocity = Vector3.zero;

            if (FormationController.Instance != null)
            {
                Vector3 anchor = FormationController.Instance.AnchorPosition;
                Vector3 resetPos = new Vector3(0f, anchor.y + verticalLead, 0f);

                if (trackingTarget != null)
                {
                    trackingTarget.position = resetPos;
                }
                if (vcam != null)
                {
                    vcam.transform.position = new Vector3(0f, resetPos.y, -10f);
                    vcam.PreviousStateIsValid = false;
                    vcam.ForceCameraPosition(new Vector3(0f, resetPos.y, -10f), Quaternion.identity);
                }
                if (Camera.main != null) Camera.main.transform.position = new Vector3(0f, resetPos.y, -10f);
            }
        }

        private void UpdateOrthoFraming()
        {
            if (vcam == null || Camera.main == null) return;

            float aspect = Mathf.Max(0.1f, Camera.main.aspect);
            float requiredVerticalForWidth = (minHorizontalWidth * 0.5f) / aspect;
            float targetSize = Mathf.Max(defaultOrthoSize, requiredVerticalForWidth);

            if (isEncounterLocked)
            {
                // Slightly zoom in/adjust during intense combat for dramatic focus
                targetSize = Mathf.Max(defaultOrthoSize * 1.05f, requiredVerticalForWidth);
            }

            var lens = vcam.Lens;
            lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, targetSize, Time.deltaTime * 3f);
            vcam.Lens = lens;
        }

        private void UpdateScreenShake()
        {
            if (currentShakeIntensity > 0.001f && vcam != null)
            {
                currentShakeIntensity = Mathf.Max(0f, currentShakeIntensity - Time.deltaTime * shakeDecay);
                Vector3 shakeOffset = new Vector3(
                    UnityEngine.Random.Range(-currentShakeIntensity, currentShakeIntensity),
                    UnityEngine.Random.Range(-currentShakeIntensity, currentShakeIntensity),
                    0f
                );
                vcam.transform.position += shakeOffset;
            }
        }

        public void TriggerScreenShake(float intensity = 0.15f)
        {
            currentShakeIntensity = Mathf.Max(currentShakeIntensity, intensity);
        }

        /// <summary>
        /// Locks camera on the combat encounter line when enemies approach at specific angles.
        /// Frames the clash squarely in the center battlefield corridor, well above the cards.
        /// </summary>
        public void LockCombatEncounter(Vector3 enemyCenter)
        {
            if (FormationController.Instance == null) return;

            isEncounterLocked = true;
            Vector3 packCenter = FormationController.Instance.AnchorPosition;
            // Anticipate or track frontline clash zone (clamped so incoming enemies don't pull camera off-screen)
            float targetCombatY = Mathf.Min(enemyCenter.y, packCenter.y + 2.0f);
            float clashMidpointY = (packCenter.y + targetCombatY) * 0.5f;

            // Frame camera so the clash point sits cleanly in the upper-middle battlefield
            lockPosition = new Vector3(0f, clashMidpointY - 0.25f, 0f);
        }

        public void UnlockCombatEncounter()
        {
            isEncounterLocked = false;
        }

        private void OnWaveStarted(int waveIndex, string waveName)
        {
            // Reset any previous combat lock so pack advances smoothly
            UnlockCombatEncounter();
        }

        private void OnWaveCompleted(int waveIndex)
        {
            UnlockCombatEncounter();
            TriggerScreenShake(0.08f);
        }
    }
}
