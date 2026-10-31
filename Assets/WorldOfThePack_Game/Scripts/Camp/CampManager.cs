using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.UI;
using WorldOfThePack.VFX;

namespace WorldOfThePack.Game
{
    public enum ExpeditionRoute
    {
        SunlitForest,   // +50% energy regen
        ShadowMire,     // +50% score, harder enemies
        FullMoonPlateau // Wolf instincts & transformations amplified
    }

    public class CampManager : MonoBehaviour
    {
        private static CampManager instance;
        public static CampManager Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<CampManager>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("Camp Spawning Configuration")]
        [SerializeField] private float firstCampDistance = 80f;
        [SerializeField] private float campInterval = 120f;
        [SerializeField] private Sprite campsiteSprite;

        [Header("State")]
        [SerializeField] private bool isAtCamp = false;
        [SerializeField] private float nextCampDistance;
        [SerializeField] private int campsVisited = 0;
        [SerializeField] private ExpeditionRoute currentRoute = ExpeditionRoute.SunlitForest;

        [Header("Route Modifiers")]
        public float EnergyRegenMultiplier = 1.0f;
        public float ScoreMultiplier = 1.0f;
        public float WolfPowerMultiplier = 1.0f;
        public float PackDamageBuff = 1.0f;

        private GameObject activeCampVisual;
        private float startY;
        private bool campSpawnedForUpcoming = false;
        private float upcomingCampWorldY = 0f;

        public bool IsAtCamp => isAtCamp;
        public int CampsVisited => campsVisited;
        public ExpeditionRoute CurrentRoute => currentRoute;

        public event Action OnCampReached;
        public event Action<ExpeditionRoute> OnCampDeparted;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            LoadCampsiteSprite();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Start()
        {
            startY = FormationController.Instance != null ? FormationController.Instance.AnchorPosition.y : 0f;
            nextCampDistance = firstCampDistance;
        }

        private void LoadCampsiteSprite()
        {
            if (campsiteSprite != null) return;
#if UNITY_EDITOR
            string path = "Assets/Environemnt/EndlessPathSystem/Artwork/endless-patch-lantern-refuge-v01.png";
            var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in all)
            {
                if (a is Sprite s) { campsiteSprite = s; break; }
            }
#endif
        }

        private void Update()
        {
            if (isAtCamp || GameDirector.Instance == null || GameDirector.Instance.CurrentState != GameState.Playing) return;
            if (FormationController.Instance == null) return;

            float currentY = FormationController.Instance.AnchorPosition.y;
            float traveled = currentY - startY;

            // Spawn campsite visual ahead when nearing camp
            if (!campSpawnedForUpcoming && traveled >= (nextCampDistance - 25f))
            {
                SpawnUpcomingCampsite(startY + nextCampDistance);
            }

            // Arrive at campsite
            if (campSpawnedForUpcoming && currentY >= upcomingCampWorldY - 1.2f)
            {
                ArriveAtCamp();
            }
        }

        private void SpawnUpcomingCampsite(float worldY)
        {
            campSpawnedForUpcoming = true;
            upcomingCampWorldY = worldY;

            if (activeCampVisual != null) Destroy(activeCampVisual);

            activeCampVisual = new GameObject("Campsite_LanternRefuge");
            activeCampVisual.transform.position = new Vector3(0f, worldY, 0f);

            SpriteRenderer sr = activeCampVisual.AddComponent<SpriteRenderer>();
            sr.sprite = campsiteSprite;
            sr.sortingOrder = -95; // Just above endless grass background
        }

        public void ArriveAtCamp()
        {
            isAtCamp = true;
            campsVisited++;

            // Stop march smoothly
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(false);
            }

            // Pause wave spawns temporarily
            WaveManager wm = WaveManager.Instance != null ? WaveManager.Instance : GetComponent<WaveManager>();
            if (wm != null)
            {
                wm.PauseForCamp();
            }

            // Trigger camp reached event & popup UI
            OnCampReached?.Invoke();
            ShowCampUI();

            // Ambient celebration VFX
            Vector3 campCenter = FormationController.Instance != null ? FormationController.Instance.AnchorPosition : transform.position;
            CombatVFXPool.SpawnEffect("MagicImpact", campCenter + Vector3.up * 1.5f);
        }

        public void ChooseCampOption(int optionIndex)
        {
            switch (optionIndex)
            {
                case 0: // Hearth & Rest (Heal Pack)
                    HealEntirePack(0.5f);
                    GameHUDController hud = FindFirstObjectByType<GameHUDController>();
                    if (hud != null) hud.ShowBanner("PACK HEALED (+50% HEALTH)!", 2.5f);
                    break;

                case 1: // Wolf Blessing (+Damage buff)
                    PackDamageBuff += 0.25f;
                    ApplyDamageBuffToAllies(PackDamageBuff);
                    GameHUDController hud2 = FindFirstObjectByType<GameHUDController>();
                    if (hud2 != null) hud2.ShowBanner("WOLF BLESSING: +25% ATTACK DAMAGE!", 2.5f);
                    break;

                case 2: // Forest Route (+Energy Regen)
                    SelectRoute(ExpeditionRoute.SunlitForest);
                    break;

                case 3: // Cursed Mire (+Score / Harder Enemies)
                    SelectRoute(ExpeditionRoute.ShadowMire);
                    break;

                case 4: // Moon Plateau (Lunar Wolf Instinct)
                    SelectRoute(ExpeditionRoute.FullMoonPlateau);
                    break;
            }

            DepartCamp();
        }

        public void SelectRoute(ExpeditionRoute route)
        {
            currentRoute = route;
            switch (route)
            {
                case ExpeditionRoute.SunlitForest:
                    EnergyRegenMultiplier = 1.5f;
                    ScoreMultiplier = 1.0f;
                    WolfPowerMultiplier = 1.0f;
                    break;
                case ExpeditionRoute.ShadowMire:
                    EnergyRegenMultiplier = 1.0f;
                    ScoreMultiplier = 1.75f;
                    WolfPowerMultiplier = 1.0f;
                    break;
                case ExpeditionRoute.FullMoonPlateau:
                    EnergyRegenMultiplier = 1.0f;
                    ScoreMultiplier = 1.25f;
                    WolfPowerMultiplier = 1.5f;
                    break;
            }

            GameHUDController hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null) hud.ShowBanner($"ROUTE CHOSEN: {route.ToString().ToUpper()}!", 2.5f);
        }

        private void HealEntirePack(float pct)
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var ally in allies)
            {
                if (ally != null && ally.IsAlive)
                {
                    ally.Heal(ally.MaxHealth * pct);
                    CombatVFXPool.SpawnEffect("HealingCircle", ally.Position);
                }
            }
        }

        private void ApplyDamageBuffToAllies(float multiplier)
        {
            HeroUnit[] allies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var ally in allies)
            {
                if (ally != null && ally.IsAlive)
                {
                    ally.SetDamageMultiplier(multiplier);
                }
            }
        }

        private void DepartCamp()
        {
            isAtCamp = false;
            campSpawnedForUpcoming = false;

            // Schedule next camp
            nextCampDistance += campInterval;

            HideCampUI();

            // Resume march
            if (FormationController.Instance != null)
            {
                FormationController.Instance.SetMarching(true);
            }

            // Resume waves
            WaveManager wm = WaveManager.Instance != null ? WaveManager.Instance : GetComponent<WaveManager>();
            if (wm != null)
            {
                wm.ResumeFromCamp();
            }

            OnCampDeparted?.Invoke(currentRoute);
        }

        #region Camp UI Handling

        private void ShowCampUI()
        {
            var hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null)
            {
                hud.ShowCampModal(true);
            }
        }

        private void HideCampUI()
        {
            var hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null)
            {
                hud.ShowCampModal(false);
            }
        }

        #endregion
    }
}
