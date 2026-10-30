using UnityEngine;

namespace WorldOfThePack.VFX
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFeedbackPlayer : MonoBehaviour
    {
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
        private SpriteRenderer target;
        private MaterialPropertyBlock block;
        private float remaining, duration;
        private void Awake() { target = GetComponent<SpriteRenderer>(); block = new MaterialPropertyBlock(); }
        public void Flash(Color color, float seconds = .12f)
        {
            Ensure(); duration = Mathf.Max(.01f, seconds); remaining = duration;
            target.GetPropertyBlock(block); block.SetColor(FlashColor, color); block.SetFloat(FlashAmount, 1); target.SetPropertyBlock(block);
        }
        public void SetDissolve(float progress)
        {
            Ensure(); target.GetPropertyBlock(block); block.SetFloat(Dissolve, Mathf.Clamp01(progress)); target.SetPropertyBlock(block);
        }
        private void Ensure() { if (!target) target = GetComponent<SpriteRenderer>(); if (block == null) block = new MaterialPropertyBlock(); }
        private void Update()
        {
            if (remaining <= 0) return;
            remaining = Mathf.Max(0, remaining - Time.deltaTime);
            target.GetPropertyBlock(block); block.SetFloat(FlashAmount, remaining / duration); target.SetPropertyBlock(block);
        }
        private void OnDisable()
        {
            remaining = 0;
            if (!target || block == null) return;
            target.GetPropertyBlock(block); block.SetFloat(FlashAmount, 0); block.SetFloat(Dissolve, 0); target.SetPropertyBlock(block);
        }
    }
}
