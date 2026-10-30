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

        private bool isWolfForm = true;

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
            if (heroDefinitions.Count > 0) return;

#if UNITY_EDITOR
            string[] heroNames = { "HeroWolvy", "PrincessElva", "CaptainBobo", "FairyLulu", "ScoutPip", "WizardMomo", "TinkerTilly" };
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

                // Bind card
                cardSlots[i].Bind(def, initialEnergy: 20f, wolf: isWolfForm);

                // Listen to deploy request
                HeroDeploymentCard card = cardSlots[i];
                card.deployRequested.AddListener((heroDef) => HandleDeployRequest(card, heroDef));
            }
        }

        private void HandleDeployRequest(HeroDeploymentCard card, HeroCardDefinition heroDef)
        {
            if (GameDirector.Instance == null)
            {
                card.CancelDeployment();
                return;
            }

            bool deployed = GameDirector.Instance.TryDeployHero(heroDef);
            if (deployed)
            {
                card.ConfirmDeployment();
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
                if (card != null && !card.IsReady)
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
