using System.Collections;
using TMPro;
using UnityEngine;

namespace WorldOfThePack.UI
{
    public sealed class CardUIShowcase : MonoBehaviour
    {
        public ResponsiveHeroHand hand;
        public HeroCardDefinition[] roster;
        public TMP_Text status;
        public bool autoConfirmForDemo = true;
        public bool wolf = true;
        private int first;
        private int formation;
        private bool previousRunInBackground;

        private void Awake()
        {
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true; // Keep only this isolated review scene ticking while tools inspect it.
        }

        private void OnDestroy() => Application.runInBackground = previousRunInBackground;

        private void Start()
        {
            for (int i = 0; i < hand.cards.Length; i++)
            {
                var card = hand.cards[i];
                card.deployRequested.AddListener(_ => { if (autoConfirmForDemo) StartCoroutine(ConfirmDemo(card)); });
            }
            hand.formationRequested.AddListener(CycleFormation);
            hand.transformationRequested.AddListener(ToggleForm);
            SelectHero(0);
        }

        public void SelectHero(int index)
        {
            first = index;
            for (int i = 0; i < hand.cards.Length; i++) hand.cards[i].Bind(roster[(index + i) % roster.Length], i == 0 ? 100 : i == 1 ? 60 : 40, wolf);
            hand.cards[0].SetSelected(true);
            if (status != null) status.text = roster[index].displayName + "  |  " + (wolf ? "Wolf portraits" : "Human portraits");
        }

        public void ToggleForm()
        {
            wolf = !wolf;
            foreach (var card in hand.cards) card.SetForm(wolf);
            if (hand.transformationLabel != null) hand.transformationLabel.text = wolf ? "SHOW HUMAN" : "SHOW WOLF";
            if (status != null) status.text = roster[first].displayName + "  |  " + (wolf ? "Wolf portraits" : "Human portraits");
        }

        public void AddDemoEnergy() { foreach (var card in hand.cards) card.AddEnergy(20); }
        private void CycleFormation()
        {
            formation = (formation + 1) % 3;
            hand.formationLabel.text = new[] { "FORMATION", "WEDGE", "GUARD" }[formation];
            if (status != null) status.text = "Formation preview: " + new[] { "Line", "Wedge", "Guard" }[formation];
        }

        private IEnumerator ConfirmDemo(HeroDeploymentCard card)
        {
            if (status != null) status.text = "Demo deployment requested — confirming…";
            yield return new WaitForSecondsRealtime(.2f);
            if (card != null && card.isActiveAndEnabled && card.ConfirmDeployment() && status != null) status.text = "Deployed preview • energy reset • no gameplay unit was spawned";
        }
    }
}
