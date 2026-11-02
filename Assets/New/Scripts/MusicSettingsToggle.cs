using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Audio;

namespace WorldOfThePack.NewUI
{
    public sealed class MusicSettingsToggle : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _checkboxIcon;
        [SerializeField] private Sprite _checkedSprite;
        [SerializeField] private Sprite _uncheckedSprite;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null) _button.onClick.AddListener(OnClickToggle);
        }

        private void OnEnable()
        {
            UpdateVisuals();
            var music = BackgroundMusicController.Instance;
            if (music != null) music.OnMusicToggled += OnMusicStateChanged;
        }

        private void OnDisable()
        {
            var music = BackgroundMusicController.Instance;
            if (music != null) music.OnMusicToggled -= OnMusicStateChanged;
        }

        private void OnMusicStateChanged(bool enabled)
        {
            UpdateVisuals();
        }

        public void OnClickToggle()
        {
            var music = BackgroundMusicController.Instance;
            if (music != null)
            {
                music.ToggleMusic();
            }
            else
            {
                bool current = PlayerPrefs.GetInt(BackgroundMusicController.PrefsKey, 1) == 1;
                PlayerPrefs.SetInt(BackgroundMusicController.PrefsKey, current ? 0 : 1);
                PlayerPrefs.Save();
                UpdateVisuals();
            }
        }

        public void UpdateVisuals()
        {
            var music = BackgroundMusicController.Instance;
            bool enabled = music != null 
                ? music.IsMusicEnabled 
                : PlayerPrefs.GetInt(BackgroundMusicController.PrefsKey, 1) == 1;

            if (_label != null)
            {
                _label.text = enabled ? "MUSIC: [ \u2713 ON ]" : "MUSIC: [ \u2717 OFF ]";
                _label.color = enabled 
                    ? new Color32(255, 235, 175, 255) 
                    : new Color32(160, 160, 160, 220);
            }

            if (_checkboxIcon != null)
            {
                if (_checkedSprite != null && _uncheckedSprite != null)
                {
                    _checkboxIcon.sprite = enabled ? _checkedSprite : _uncheckedSprite;
                }
                _checkboxIcon.color = enabled ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.7f);
            }
        }
    }
}
