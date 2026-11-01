using UnityEngine;
using UnityEngine.EventSystems;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    [RequireComponent(typeof(Combatant))]
    public sealed class PackEnemyTarget : MonoBehaviour, IPointerClickHandler
    {
        private Combatant _unit;
        private void Awake() => _unit = GetComponent<Combatant>();
        public void OnPointerClick(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) TrailDecisionDirector.Instance?.SelectEnemy(_unit);
        }
    }
}
