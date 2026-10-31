using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class StoryCampaignDirector : MonoBehaviour
    {
        private static StoryCampaignDirector instance;
        public static StoryCampaignDirector Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<StoryCampaignDirector>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("Campaign Configuration")]
        [SerializeField] private int activeChapter = 1;
        [SerializeField] private bool isEndlessMode = false;

        [Header("Boss Prefabs")]
        [SerializeField] private GameObject bossPrefab;

        [Header("Camp Interval (Waves between camps)")]
        [SerializeField] private int wavesPerCamp = 3;
        [SerializeField] private int wavesPerBoss = 5;

        private int wavesCompletedInCurrentChapter = 0;
        private bool bossEncounterActive = false;

        public int ActiveChapter => activeChapter;
        public bool IsEndlessMode => isEndlessMode;
        public bool IsBossEncounterActive => bossEncounterActive;

        public event Action<int, string> OnChapterStarted;
        public event Action<int> OnChapterCompleted;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            if (SaveManager.Instance != null)
            {
                activeChapter = Mathf.Clamp(SaveManager.Instance.Data.currentChapter, 1, 5);
            }
        }

        private void Start()
        {
            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveCompleted += HandleWaveCompleted;
            }

            BossUnit.OnBossDefeated += HandleBossDefeated;

            ApplyCurrentChapterBiome();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;

            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                wm.OnWaveCompleted -= HandleWaveCompleted;
            }
            BossUnit.OnBossDefeated -= HandleBossDefeated;
        }

        public void SetEndlessMode(bool endless)
        {
            isEndlessMode = endless;
            if (endless)
            {
                if (WorldBiomeGenerator.Instance != null)
                {
                    WorldBiomeGenerator.Instance.SetBiome(BiomeType.SunnyForestTrail);
                }
            }
        }

        private void HandleWaveCompleted(int waveNum)
        {
            wavesCompletedInCurrentChapter++;

            // Camp Check (every 3 waves)
            if (wavesCompletedInCurrentChapter == wavesPerCamp)
            {
                TriggerCampsite();
                return;
            }

            // Boss Check (at wave 5 of chapter)
            if (wavesCompletedInCurrentChapter >= wavesPerBoss && !bossEncounterActive)
            {
                SpawnChapterBoss();
            }
        }

        private void TriggerCampsite()
        {
            if (CampManager.Instance != null)
            {
                CampManager.Instance.ArriveAtCamp();
            }
        }

        private void SpawnChapterBoss()
        {
            bossEncounterActive = true;

            string bossId = activeChapter switch
            {
                1 => "AncientThornGuardian",
                2 => "MushroomKing",
                3 => "RiverGuardian",
                4 => "EmberlaneBanditChief",
                5 => "CloudstepStoneTitan",
                _ => "AncientThornGuardian"
            };

            float baseY = Camera.main != null ? Camera.main.transform.position.y : 0f;
            Vector3 spawnPos = new Vector3(0f, baseY + 6.5f, 0f);

            GameObject bossGo;
            if (bossPrefab != null)
            {
                bossGo = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                bossGo = CreateProceduralBossObject(spawnPos);
            }

            BossUnit boss = bossGo.GetComponent<BossUnit>();
            if (boss == null) boss = bossGo.AddComponent<BossUnit>();

#if UNITY_EDITOR
            string assetPath = $"Assets/WorldOfThePack_Game/Data/Bosses/{bossId}_Boss.asset";
            BossDefinition def = UnityEditor.AssetDatabase.LoadAssetAtPath<BossDefinition>(assetPath);
            // Reflect/bind definition
#endif

            // Announce boss in GameHUD
            GameHUDController hud = FindFirstObjectByType<GameHUDController>();
            if (hud != null)
            {
                hud.ShowBanner($"WARNING!\nBOSS APPROACHING!", 3.0f);
            }
        }

        private void HandleBossDefeated(BossUnit boss)
        {
            bossEncounterActive = false;
            wavesCompletedInCurrentChapter = 0;

            OnChapterCompleted?.Invoke(activeChapter);

            if (activeChapter >= 5)
            {
                // CAMPAIGN COMPLETE! Trigger ending cinematic!
                if (CinematicDirector.Instance != null)
                {
                    CinematicDirector.Instance.PlayEndingCinematic();
                }
            }
            else
            {
                // Advance to next chapter
                activeChapter++;
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.CompleteChapter(activeChapter - 1);
                }

                ApplyCurrentChapterBiome();
                OnChapterStarted?.Invoke(activeChapter, GetChapterName(activeChapter));

                GameHUDController hud = FindFirstObjectByType<GameHUDController>();
                if (hud != null)
                {
                    hud.ShowBanner($"CHAPTER {activeChapter} UNLOCKED!\n{GetChapterName(activeChapter)}", 3.0f);
                }
            }
        }

        private void ApplyCurrentChapterBiome()
        {
            if (WorldBiomeGenerator.Instance != null)
            {
                BiomeType biome = (BiomeType)Mathf.Clamp(activeChapter - 1, 0, 4);
                WorldBiomeGenerator.Instance.SetBiome(biome);
            }
        }

        public string GetChapterName(int chapter)
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

        private GameObject CreateProceduralBossObject(Vector3 pos)
        {
            GameObject go = new GameObject("BossUnit");
            go.transform.position = pos;

            GameObject body = new GameObject("Body");
            body.transform.SetParent(go.transform, false);
            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();

#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/CaptainBobo/Artwork/captain-bobo-walk-human-north-v01.png");
            sr.sprite = s;
#endif
            sr.sortingOrder = 10;
            return go;
        }
    }
}
