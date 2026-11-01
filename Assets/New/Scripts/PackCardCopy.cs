using TMPro;
using UnityEngine;
using WorldOfThePack.UI;

namespace WorldOfThePack.NewUI
{
    /// <summary>Editable TMP copy over clean button artwork; charge remains owned by the game card.</summary>
    public sealed class PackCardCopy : MonoBehaviour
    {
        public HeroDeploymentCard card;
        public TMP_Text actionLabel;
        public string deploy = "DEPLOY";
        public string charging = "CHARGING";
        private void LateUpdate()
        {
            if (card == null || actionLabel == null) return;
            var game = WorldOfThePack.Game.GameDirector.Instance;
            var arena = WorldOfThePack.Game.ArenaDirector.Instance;
            var tutorial = WorldOfThePack.Game.TutorialManager.Instance;
            bool guided = tutorial != null && card.Definition != null && !tutorial.AllowsCard(card.Definition.heroId);
            actionLabel.text = game != null && game.IsDecisionPending ? "WAITING" :
                game != null && game.CurrentState == WorldOfThePack.Game.GameState.Paused ? "PAUSED" :
                guided && card.HasEnoughEnergy ? "FOLLOW GUIDE" : card.IsPending ? "DEPLOYING" : card.IsReady ?
                (arena != null && arena.IsPreparing ? (arena.SelectedCard==card ? "CHOOSE TILE" : "SELECT") : deploy) :
                card.HasEnoughEnergy ? (game != null && card.Definition != null && game.CanAfford(card.Definition.heroId) ? "PACK FULL" : "NEED CHARMS") : "CHARGING";
            actionLabel.color = card.IsReady ? new Color(1, 1, .75f) : new Color(.65f, .85f, 1);
        }
    }
}
