using UnityEngine;

namespace WorldOfThePack.Audio
{
    public sealed class BackgroundMusicController : MonoBehaviour
    {
        public static BackgroundMusicController Instance { get; private set; }

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _musicClip;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.55f;

        public const string PrefsKey = "MusicEnabled";
        public bool IsMusicEnabled { get; private set; } = true;

        public event System.Action<bool> OnMusicToggled;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (_musicClip == null)
            {
                _musicClip = Resources.Load<AudioClip>("Music_Peaceful_Violin_Loop");
            }

            _audioSource.clip = _musicClip;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = _volume;
            _audioSource.ignoreListenerPause = true; // Music continues playing when game is paused

            IsMusicEnabled = PlayerPrefs.GetInt(PrefsKey, 1) == 1;
        }

        private void Start()
        {
            if (IsMusicEnabled && _audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
            }
        }

        public void SetMusicEnabled(bool enabled)
        {
            IsMusicEnabled = enabled;
            PlayerPrefs.SetInt(PrefsKey, enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (_audioSource != null)
            {
                if (enabled)
                {
                    if (!_audioSource.isPlaying) _audioSource.Play();
                    _audioSource.mute = false;
                }
                else
                {
                    _audioSource.mute = true;
                }
            }

            OnMusicToggled?.Invoke(enabled);
        }

        public void ToggleMusic()
        {
            SetMusicEnabled(!IsMusicEnabled);
        }
    }
}
