using UnityEngine;
using UnityEngine.Events;

namespace WorldOfThePack.VFX
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFlipbookEffect : MonoBehaviour
    {
        public CombatEffectDefinition definition;
        public bool playOnEnable = true;
        public bool loop;
        public bool deactivateOnComplete = true;
        public UnityEvent completed = new UnityEvent();
        public int CurrentFrame { get; private set; }
        public bool IsPlaying { get; private set; }
        public int CompletedCount { get; private set; }
        private SpriteRenderer target;
        private float elapsed;
        private bool finished;
        private void Awake() => target = GetComponent<SpriteRenderer>();
        private void OnEnable() { if (playOnEnable) Play(); }
        private void OnDisable() { IsPlaying = false; elapsed = 0; }
        public void Play()
        {
            if (!target) target = GetComponent<SpriteRenderer>();
            elapsed = 0;
            finished = false;
            ShowFrame(0);
            IsPlaying = definition && !definition.staticMarker && definition.frames != null && definition.frames.Length > 0;
        }
        public void ShowFrame(int index)
        {
            if (!target) target = GetComponent<SpriteRenderer>();
            if (!definition || definition.frames == null || definition.frames.Length == 0) return;
            CurrentFrame = Mathf.Clamp(index, 0, definition.frames.Length - 1);
            target.sprite = definition.frames[CurrentFrame];
            target.sortingOrder = definition.sortingOrder;
            target.enabled = true;
        }
        public void Pause() => IsPlaying = false;
        public void Resume() { if (definition && !definition.staticMarker && !finished) IsPlaying = true; }
        private void Update()
        {
            if (!IsPlaying || !definition) return;
            elapsed += Time.deltaTime;
            int index = Mathf.FloorToInt(elapsed * definition.framesPerSecond);
            if (index < definition.frames.Length) { ShowFrame(index); return; }
            if (loop) { elapsed %= definition.frames.Length / definition.framesPerSecond; ShowFrame(Mathf.FloorToInt(elapsed * definition.framesPerSecond)); return; }
            IsPlaying = false;
            finished = true;
            target.enabled = false;
            CompletedCount++;
            completed.Invoke();
            if (deactivateOnComplete) gameObject.SetActive(false);
        }
    }
}
