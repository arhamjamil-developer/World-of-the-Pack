using UnityEngine;

namespace WorldOfThePack.VFX
{
    public sealed class CombatVFXShowcase : MonoBehaviour
    {
        public SpriteFlipbookEffect[] effects;
        private bool previousBackground;
        private float restart;
        public bool hold;
        private void Awake() { previousBackground = Application.runInBackground; Application.runInBackground = true; }
        private void Update()
        {
            if (hold) return;
            restart -= Time.deltaTime;
            if (restart > 0) return;
            restart = 1.8f;
            foreach (var effect in effects) { effect.gameObject.SetActive(true); effect.Play(); }
        }
        public void HoldFrame(int frame)
        {
            hold = true;
            foreach (var effect in effects) { effect.gameObject.SetActive(true); effect.Pause(); effect.ShowFrame(frame); }
        }
        private void OnDestroy() => Application.runInBackground = previousBackground;
    }
}
