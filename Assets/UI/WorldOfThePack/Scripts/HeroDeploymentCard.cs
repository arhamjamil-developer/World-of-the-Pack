using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorldOfThePack.UI
{
    [Serializable] public sealed class HeroDeployEvent : UnityEvent<HeroCardDefinition> { }

    public sealed class HeroDeploymentCard : MonoBehaviour
    {
        public HeroCardDefinition definition;
        public CardPortraitView portraitView;
        public TMP_Text nameLabel;
        public TMP_Text energyLabel;
        public TMP_Text stateLabel;
        public Image energyFill;
        public Image actionArtwork;
        public Image selectionFrame;
        public Button deployButton;
        public Sprite readyArtwork;
        public Sprite chargingArtwork;
        public UIFlipbookEffect chargingEffect;
        public UIFlipbookEffect readyEffect;
        public UIFlipbookEffect deployedEffect;
        public string deployText = "DEPLOY";
        public string chargingText = "CHARGING";
        public string pendingText = "DEPLOYING";
        public string lockedText = "LOCKED";
        public string displayNameOverride;
        public HeroDeployEvent deployRequested = new HeroDeployEvent();
        [SerializeField] private float energy;
        [SerializeField] private bool locked;
        [SerializeField] private bool pending;
        public float Energy => energy;
        public int MaximumEnergy => definition == null ? 100 : Mathf.Max(1, definition.maximumEnergy);
        public bool IsPending => pending;
        public bool IsReady => definition != null && !locked && !pending && energy >= MaximumEnergy;

        private void OnEnable()
        {
            if (deployButton != null) deployButton.onClick.AddListener(OnDeployClicked);
            Refresh();
        }

        private void OnDisable()
        {
            if (deployButton != null) deployButton.onClick.RemoveListener(OnDeployClicked);
            pending = false;
        }

        public void Bind(HeroCardDefinition hero, float initialEnergy, bool wolf = true)
        {
            definition = hero; pending = false; locked = false; displayNameOverride = null;
            if (deployedEffect != null) deployedEffect.Stop();
            if (portraitView != null) portraitView.Bind(hero, wolf);
            SetEnergy(initialEnergy);
        }

        public void SetEnergy(float value)
        {
            energy = float.IsNaN(value) ? 0 : Mathf.Clamp(value, 0, MaximumEnergy);
            Refresh();
        }

        public void AddEnergy(float amount) => SetEnergy(energy + amount);
        public void SetLocked(bool value) { locked = value; Refresh(); }
        public void SetForm(bool wolf) { if (portraitView != null) portraitView.SetForm(wolf); }
        public void SetSelected(bool value) { if (selectionFrame != null) selectionFrame.enabled = value; }
        public void SetDisplayName(string value) { displayNameOverride = value; Refresh(); }

        private void OnDeployClicked()
        {
            if (!IsReady) return;
            pending = true; Refresh();
            deployRequested.Invoke(definition);
        }

        // Game code calls this only after the actual spawn succeeds.
        public bool ConfirmDeployment()
        {
            if (!pending) return false;
            pending = false; energy = 0; Refresh();
            if (deployedEffect != null) deployedEffect.Play(false);
            return true;
        }

        // A blocked lane/population limit does not consume the player's full card.
        public void CancelDeployment() { pending = false; Refresh(); }

        public void Refresh()
        {
            if (nameLabel != null) nameLabel.text = !string.IsNullOrEmpty(displayNameOverride) ? displayNameOverride : definition == null ? "HERO" : definition.shortName;
            if (energyLabel != null) energyLabel.SetText("{0:0}/{1:0}", Mathf.Floor(energy), MaximumEnergy);
            if (energyFill != null) energyFill.fillAmount = Mathf.Clamp01(energy / MaximumEnergy);
            bool ready = IsReady;
            if (deployButton != null) deployButton.interactable = ready;
            if (actionArtwork != null) actionArtwork.sprite = ready || pending ? readyArtwork : chargingArtwork;
            if (stateLabel != null) stateLabel.text = locked ? lockedText : pending ? pendingText : ready ? deployText : chargingText;
            if (chargingEffect != null) { if (!ready && !pending && !locked) chargingEffect.Play(true); else chargingEffect.Stop(); }
            if (readyEffect != null) { if (ready) readyEffect.Play(true); else readyEffect.Stop(); }
        }
    }
}
