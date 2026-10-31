using System.Collections.Generic;
using UnityEngine;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game
{
    public class CardDeckManager : MonoBehaviour
    {
        [Header("Card Hand Reference")]
        [SerializeField] private ResponsiveHeroHand heroHand;
        [SerializeField] private HeroDeploymentCard[] cardSlots;

        [Header("Hero Definitions")]
        [SerializeField] private List<HeroCardDefinition> heroDefinitions = new List<HeroCardDefinition>();

        private bool isWolfForm = false;

        private void Start()
        {
            FindCardSlotsIfNeeded();
            LoadDefaultDefinitionsIfNeeded();
            InitializeHand();
        }

        private void FindCardSlotsIfNeeded()
        {
            if (heroHand == null) heroHand = FindFirstObjectByType<ResponsiveHeroHand>();
            if (cardSlots == null || cardSlots.Length == 0)
            {
                if (heroHand != null && heroHand.cards != null && heroHand.cards.Length > 0)
                {
                    cardSlots = heroHand.cards;
                }
                else
                {
                    cardSlots = FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None);
                }
            }

            if (heroHand != null)
            {
                heroHand.transformationRequested.AddListener(ToggleAllForms);
            }
        }

        private void LoadDefaultDefinitionsIfNeeded()
        {
            if (heroDefinitions.Count >= 5 && heroDefinitions[0] != null && heroDefinitions[0].heroId == "HeroWolvy" && heroDefinitions[2] != null && heroDefinitions[2].heroId == "FairyLulu") return;

            heroDefinitions.Clear();

#if UNITY_EDITOR
            string[] heroNames = { "HeroWolvy", "PrincessElva", "FairyLulu", "CaptainBobo", "ScoutPip", "WizardMomo", "TinkerTilly" };
            foreach (string name in heroNames)
            {
                string path = "Assets/UI/WorldOfThePack/Data/" + name + ".asset";
                HeroCardDefinition def = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroCardDefinition>(path);
                if (def != null) heroDefinitions.Add(def);
            }
#endif
        }

        public void InitializeHand()
        {
            if (cardSlots == null || cardSlots.Length == 0 || heroDefinitions.Count == 0) return;

            for (int i = 0; i < cardSlots.Length; i++)
            {
                if (cardSlots[i] == null) continue;
                HeroCardDefinition def = heroDefinitions[i % heroDefinitions.Count];

                // Remove previous listeners to avoid double hook
                cardSlots[i].deployRequested.RemoveAllListeners();
                cardSlots[i].deployAtPositionRequested.RemoveAllListeners();

                // Slot 0 (Hero Wolvy) starts ready at 100 energy for instant first deployment!
                // Other slots match mockup progression: Elva 60, Lulu 40, Bobo 80, Pip 30
                float initialEnergy = i switch
                {
                    0 => 100f,
                    1 => 60f,
                    2 => 40f,
                    3 => 80f,
                    4 => 30f,
                    _ => 20f + i * 10f
                };

                // Start in Human Form
                cardSlots[i].Bind(def, initialEnergy: initialEnergy, wolf: false);

                // Listen to deployment requests
                HeroDeploymentCard card = cardSlots[i];
                card.deployRequested.AddListener((heroDef) => HandleDeployRequest(card, heroDef));
                card.deployAtPositionRequested.AddListener((heroDef, pos) => HandleDeployAtPositionRequest(card, heroDef, pos));
            }
        }

        private void HandleDeployRequest(HeroDeploymentCard card, HeroCardDefinition heroDef)
        {
            if (GameDirector.Instance == null)
            {
                card.CancelDeployment();
                return;
            }

            HeroUnit spawnedHero = GameDirector.Instance.DeployHero(heroDef);
            if (spawnedHero != null)
            {
                card.ConfirmDeployment(spawnedHero);
            }
            else
            {
                card.CancelDeployment();
            }
        }

        private void HandleDeployAtPositionRequest(HeroDeploymentCard card, HeroCardDefinition heroDef, Vector3 worldPos)
        {
            if (GameDirector.Instance == null)
            {
                card.CancelDeployment();
                return;
            }

            HeroUnit spawnedHero = GameDirector.Instance.DeployHeroAtPosition(heroDef, worldPos);
            if (spawnedHero != null)
            {
                card.ConfirmDeployment(spawnedHero);
            }
            else
            {
                card.CancelDeployment();
            }
        }

        public void DistributeEnergy(float amount)
        {
            if (cardSlots == null) return;
            foreach (var card in cardSlots)
            {
                if (card != null)
                {
                    card.AddEnergy(amount);
                }
            }
        }

        public void ToggleAllForms()
        {
            isWolfForm = !isWolfForm;
            if (cardSlots != null)
            {
                foreach (var card in cardSlots)
                {
                    if (card != null) card.SetForm(isWolfForm);
                }
            }

            // Also toggle active heroes in world
            HeroUnit[] worldAllies = FindObjectsByType<HeroUnit>(FindObjectsSortMode.None);
            foreach (var ally in worldAllies)
            {
                if (ally != null) ally.SetForm(isWolfForm);
            }
        }
    }
}
