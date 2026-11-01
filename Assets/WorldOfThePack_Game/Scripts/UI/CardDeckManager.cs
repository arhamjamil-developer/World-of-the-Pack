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

        private void Update()
        {
            // Combat awards charge; waiting does not silently fill every card.
        }

        private void FindCardSlotsIfNeeded()
        {
            if (heroHand == null) heroHand = FindFirstObjectByType<ResponsiveHeroHand>();
            if (cardSlots == null || cardSlots.Length == 0)
            {
                if (heroHand != null)
                {
                    cardSlots = heroHand.GetComponentsInChildren<HeroDeploymentCard>(true);
                }
                else
                {
                    var found = FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None);
                    System.Array.Sort(found, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                    cardSlots = found;
                }
            }

            if (heroHand != null && (heroHand.cards == null || heroHand.cards.Length == 0))
            {
                heroHand.cards = cardSlots;
            }

            if (heroHand != null)
            {
                heroHand.transformationRequested.RemoveListener(ToggleAllForms);
                heroHand.transformationRequested.AddListener(ToggleAllForms);
            }
        }

        private void LoadDefaultDefinitionsIfNeeded()
        {
            if (heroDefinitions.Count >= 7 && heroDefinitions[0] != null && heroDefinitions[0].heroId == "HeroWolvy" && heroDefinitions[6] != null && heroDefinitions[6].heroId == "TinkerTilly") return;

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
            isWolfForm = false;
            if (cardSlots == null || cardSlots.Length == 0 || heroDefinitions.Count == 0) return;

            for (int i = 0; i < cardSlots.Length; i++)
            {
                if (cardSlots[i] == null) continue;

                // Match definition by heroId if card slot contains heroId in its name, else fallback by index
                HeroCardDefinition def = cardSlots[i].Definition;
                string slotName = cardSlots[i].gameObject.name;
                foreach (var d in heroDefinitions)
                {
                    if (def != null) break;
                    if (d != null && slotName.Contains(d.heroId))
                    {
                        def = d;
                        break;
                    }
                }
                if (def == null) def = heroDefinitions[i % heroDefinitions.Count];

                // Remove previous listeners to avoid double hook
                cardSlots[i].deployRequested.RemoveAllListeners();
                cardSlots[i].deployAtPositionRequested.RemoveAllListeners();

                // The opening arena offers the full roster; combat earns subsequent charges.
                cardSlots[i].Bind(def, initialEnergy: 100f, wolf: false);

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

            if (ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing)
            {
                card.CancelDeployment();
                ArenaDirector.Instance.SelectCard(card);
                return;
            }
            HeroUnit spawnedHero = GameDirector.Instance.DeployHero(heroDef);
            if (spawnedHero != null)
            {
                card.ConfirmDeployment(spawnedHero);
                TutorialManager.Instance?.UnitPlaced(spawnedHero);
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
                TutorialManager.Instance?.UnitPlaced(spawnedHero);
            }
            else
            {
                card.CancelDeployment();
            }
        }

        public void DistributeEnergy(float amount)
        {
            if (cardSlots == null) return;
            amount *= CampManager.Instance != null ? CampManager.Instance.EnergyRegenMultiplier : 1f;
            foreach (var card in cardSlots)
            {
                if (card != null)
                {
                    card.AddEnergy(amount);
                }
            }
        }

        public void DistributeKillEnergy(float amount, Vector3 sourceWorldPos)
        {
            if (cardSlots == null) return;
            foreach (var card in cardSlots)
            {
                if (card != null)
                {
                    card.AddEnergy(amount);
                    WorldOfThePack.VFX.EnergyOrbVFX.Spawn(sourceWorldPos, card.transform.position, amount);
                }
            }
        }

        public void LinkSpawnedHeroToCard(HeroUnit hero)
        {
            if (hero == null || cardSlots == null) return;
            foreach (var card in cardSlots)
            {
                if (card != null && card.Definition != null && card.Definition.heroId == hero.HeroId)
                {
                    card.SetDeployedHero(hero);
                    break;
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
