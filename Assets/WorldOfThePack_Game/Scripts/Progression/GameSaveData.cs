using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    [Serializable]
    public class UpgradeEntry
    {
        public string upgradeKey;
        public int level;
    }

    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public int currentChapter = 1; // 1: Sunny Forest, 2: Mushroom Grove, 3: River Meadow, 4: Emberlane, 5: Cloudstep
        public bool isCampaignCompleted = false;
        public bool isEndlessUnlocked = false;
        public bool isTutorialCompleted = false;

        public int moonFragmentsCollected = 0;
        public int moonShards = 150; // Starter currency

        public List<string> unlockedHeroIds = new List<string> { "HeroWolvy", "CaptainBobo", "PrincessElva" };
        public List<UpgradeEntry> upgrades = new List<UpgradeEntry>();

        public float bestDistance = 0f;
        public int totalEnemiesDefeated = 0;
        public int totalBossesDefeated = 0;
        public int totalHeroesRescued = 0;

        // Settings
        public float masterVolume = 1.0f;
        public float musicVolume = 0.85f;
        public float sfxVolume = 0.95f;
        public bool hapticsEnabled = true;

        public int GetUpgradeLevel(string key)
        {
            var entry = upgrades.Find(u => u.upgradeKey == key);
            return entry != null ? entry.level : 0;
        }

        public void SetUpgradeLevel(string key, int level)
        {
            var entry = upgrades.Find(u => u.upgradeKey == key);
            if (entry != null) entry.level = level;
            else upgrades.Add(new UpgradeEntry { upgradeKey = key, level = level });
        }

        public bool IsHeroUnlocked(string heroId)
        {
            return unlockedHeroIds.Contains(heroId);
        }

        public void UnlockHero(string heroId)
        {
            if (!unlockedHeroIds.Contains(heroId))
            {
                unlockedHeroIds.Add(heroId);
            }
        }
    }
}
