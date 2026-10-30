using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.UI
{
    public sealed class UIFlipbookEffect : MonoBehaviour
    {
        public Image image;
        public Sprite[] frames;
        [Min(1)] public float framesPerSecond = 12;
        public bool reduceMotion;
        public float horizontalDrift;
        public bool IsPlaying { get; private set; }
        public int PlayCount { get; private set; }
        private bool loop;
        private float elapsed;
        private RectTransform rect;
        private Vector2 restingPosition;

        private void Awake()
        {
            rect = (RectTransform)transform;
            restingPosition = rect.anchoredPosition;
            if (!IsPlaying) Stop();
        }

        public void Play(bool repeat)
        {
            if (image == null || frames == null || frames.Length == 0) return;
            if (IsPlaying && loop && repeat) return;
            loop = repeat; elapsed = 0; IsPlaying = true; PlayCount++;
            image.enabled = true; image.sprite = frames[0];
        }

        public void Stop()
        {
            IsPlaying = false;
            if (image != null) image.enabled = false;
            if (rect != null) rect.anchoredPosition = restingPosition;
        }

        private void OnDisable() => Stop();

        private void Update()
        {
            if (!IsPlaying || frames == null || frames.Length == 0) return;
            elapsed += Time.unscaledDeltaTime;
            int frame = Mathf.FloorToInt(elapsed * framesPerSecond);
            if (!loop && frame >= frames.Length) { Stop(); return; }
            image.sprite = frames[frame % frames.Length];
            if (!reduceMotion && horizontalDrift != 0 && rect != null)
                rect.anchoredPosition = restingPosition + new Vector2(Mathf.Sin(elapsed * 2) * horizontalDrift, 0);
        }
    }
}
