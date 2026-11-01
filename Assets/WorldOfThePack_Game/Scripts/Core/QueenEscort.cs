using UnityEngine;

namespace WorldOfThePack.Game
{
    public sealed class QueenEscort : Combatant
    {
        private Sprite[] _frames;
        private FormationController _formation;
        protected override void Awake()
        {
            unitName = "The Queen";
            maxHealth = 240;
            faction = UnitFaction.Allies;
            bodyRenderer = gameObject.AddComponent<SpriteRenderer>();
            _frames = Resources.LoadAll<Sprite>("RoyalEscort/QueenEscort");
            System.Array.Sort(_frames, (a,b) => string.CompareOrdinal(a.name,b.name));
            if (_frames.Length > 0) bodyRenderer.sprite = _frames[0];
            base.Awake();
            healthBarPivot.localPosition = new Vector3(0,.62f,0);
            healthBarPivot.localScale = Vector3.one * .8f;
            healthBarPivot.gameObject.SetActive(false); // The queen uses the persistent heart HUD, keeping rear deployment tiles clear.
            _formation = FormationController.Instance;
            OnHealthChanged += ReportHealth;
        }
        private void ReportHealth(float current, float maximum) => GameDirector.Instance?.OnAlphaHealthChanged(current, maximum);
        protected override void UpdateBehavior()
        {
            if (_formation == null) return;
            transform.position = _formation.AnchorPosition + Vector3.down * .85f;
            if (_frames.Length >= 8)
            {
                bool walking = _formation.IsMarching && _formation.CurrentSpeed > .05f;
                bodyRenderer.sprite = _frames[(walking ? 4 : 0) + (int)(Time.time * (walking ? 7 : 3)) % 4];
            }
        }
        public void ResetEscort()
        {
            isDead = false;
            var formation = FormationController.Instance;
            if (formation != null) transform.position = formation.AnchorPosition + Vector3.down * .85f;
            currentHealth = maxHealth;
            if (healthBarPivot != null) healthBarPivot.gameObject.SetActive(false);
            UpdateHealthBarUI();
            ReportHealth(currentHealth,maxHealth);
        }
        protected override void Die(Combatant killer)
        {
            isDead = true;
            GameDirector.Instance?.TriggerGameOver("The queen has fallen. Guard her lanes and save charms for reinforcements.");
        }
    }
}
