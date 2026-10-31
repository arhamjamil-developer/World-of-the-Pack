using System;
using System.IO;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public class SaveManager : MonoBehaviour
    {
        private const string SAVE_KEY = "WorldOfThePack_SaveData_v1";
        private static SaveManager instance;

        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                    instance.Load();
                }
                return instance;
            }
        }

        public GameSaveData Data { get; private set; } = new GameSaveData();

        public event Action<GameSaveData> OnSaveDataLoaded;
        public event Action<GameSaveData> OnSaveDataUpdated;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                if (transform.parent == null && gameObject.name == "SaveManager")
                {
                    DontDestroyOnLoad(gameObject);
                }
                Load();
            }
            else if (instance != this)
            {
                if (gameObject.name == "SaveManager")
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        public void Load()
        {
            try
            {
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    if (!string.IsNullOrEmpty(json))
                    {
                        Data = JsonUtility.FromJson<GameSaveData>(json);
                        if (Data == null)
                        {
                            Data = new GameSaveData();
                        }
                    }
                }
                else
                {
                    Data = new GameSaveData();
                    Save();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Failed to load save data, resetting to fresh defaults: {ex.Message}");
                Data = new GameSaveData();
            }

            OnSaveDataLoaded?.Invoke(Data);
        }

        public void Save()
        {
            try
            {
                if (Data == null) Data = new GameSaveData();
                string json = JsonUtility.ToJson(Data, true);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
                OnSaveDataUpdated?.Invoke(Data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Error saving data: {ex.Message}");
            }
        }

        public void CompleteChapter(int chapterIndex)
        {
            if (Data.currentChapter <= chapterIndex)
            {
                Data.currentChapter = chapterIndex + 1;
            }

            Data.moonFragmentsCollected = Mathf.Clamp(Data.moonFragmentsCollected + 1, 0, 5);

            if (chapterIndex >= 5)
            {
                Data.isCampaignCompleted = true;
                Data.isEndlessUnlocked = true;
            }

            Save();
        }

        public void AddMoonShards(int amount)
        {
            Data.moonShards = Mathf.Max(0, Data.moonShards + amount);
            Save();
        }

        public bool SpendMoonShards(int amount)
        {
            if (Data.moonShards >= amount)
            {
                Data.moonShards -= amount;
                Save();
                return true;
            }
            return false;
        }

        public void RecordExpeditionRun(float distance, int kills, int bosses, int heroesRescued)
        {
            if (distance > Data.bestDistance) Data.bestDistance = distance;
            Data.totalEnemiesDefeated += kills;
            Data.totalBossesDefeated += bosses;
            Data.totalHeroesRescued += heroesRescued;
            Save();
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Data = new GameSaveData();
            Save();
            OnSaveDataLoaded?.Invoke(Data);
        }
    }
}
