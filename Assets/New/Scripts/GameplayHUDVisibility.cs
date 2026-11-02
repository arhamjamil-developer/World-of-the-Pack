using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Game;

namespace WorldOfThePack.NewUI
{
    public sealed class GameplayHUDVisibility : MonoBehaviour
    {
        [SerializeField] private RectTransform _top, _bottom, _lowerHandle;
        [SerializeField] private CanvasGroup _topGroup, _bottomGroup;
        [SerializeField] private Button _upperButton, _lowerButton, _pause;
        [SerializeField] private TMP_Text _upperLabel, _lowerLabel;
        [SerializeField] private GameObject _controls;
        [Header("Icon-only controls")]
        // Labels are deliberately glyph-only: a red close mark while a panel is visible,
        // and an upward chevron when it can be restored. This keeps the controls readable
        // at iPhone size without relying on locale-specific Hide/Show copy.
        private Vector2 _topOrigin, _bottomOrigin;
        private Vector2 _lowerOrigin;
        private GameDirector _game;
        private PackGameplayHUD _hud;
        private ArenaPhase _lastPhase;
        public bool UpperVisible { get; private set; }=true;
        public bool CardsVisible { get; private set; }=true;
        private void Awake()
        {
            _topOrigin=_top.anchoredPosition;_bottomOrigin=_bottom.anchoredPosition;_lowerOrigin=_lowerHandle.anchoredPosition;
            if(_topGroup==null && !_top.TryGetComponent(out _topGroup))_topGroup=_top.gameObject.AddComponent<CanvasGroup>();
            if(_bottomGroup==null && !_bottom.TryGetComponent(out _bottomGroup))_bottomGroup=_bottom.gameObject.AddComponent<CanvasGroup>();
        }
        private void Start()
        {
            _game=GameDirector.Instance;_hud=GetComponent<PackGameplayHUD>();
            if(_upperLabel!=null)ArenaDirector.Instance?.ApplyCompactStyle(_upperLabel.font);
            _upperButton.onClick.AddListener(ToggleUpper);_lowerButton.onClick.AddListener(ToggleCards);_pause.onClick.AddListener(Pause);
        }
        private void OnDestroy()
        {
            if(_upperButton!=null)_upperButton.onClick.RemoveListener(ToggleUpper);
            if(_lowerButton!=null)_lowerButton.onClick.RemoveListener(ToggleCards);
            if(_pause!=null)_pause.onClick.RemoveListener(Pause);
        }
        public void ToggleUpper(){if(!Guided())UpperVisible=!UpperVisible;}
        public void ToggleCards(){if(!Guided())SetCardsVisible(!CardsVisible);}
        public void ShowAll(){UpperVisible=true;SetCardsVisible(true);}
        private void SetCardsVisible(bool visible)
        { CardsVisible=visible;if(!visible)ArenaDirector.Instance?.CancelSelection(); }
        private static bool Guided()=>TutorialManager.Instance!=null && TutorialManager.Instance.IsShowing;
        private void Pause()=>_hud?.TogglePause();
        private void LateUpdate()
        {
            if(_game==null)return;
            bool guided=Guided();if(guided)ShowAll();
            var arena=ArenaDirector.Instance;
            if(arena!=null && arena.Phase!=_lastPhase)
            {if(arena.IsPreparing)ShowAll();_lastPhase=arena.Phase;}
            bool active=_game.CurrentState==GameState.Playing || _game.CurrentState==GameState.Paused;
            _controls.SetActive(active);
            _pause.gameObject.SetActive(!UpperVisible);
            arena?.SetOrdersVisible(UpperVisible || guided);
            _upperButton.interactable=_lowerButton.interactable=!guided;
            float t=1-Mathf.Exp(-16*Time.unscaledDeltaTime);
            var topTarget=_topOrigin+Vector2.up*(UpperVisible?0:_top.rect.height+32);
            float bottomHeight=_bottom.rect.height*_bottom.localScale.y;
            var bottomTarget=_bottomOrigin+Vector2.down*(CardsVisible?0:bottomHeight+50);
            _top.anchoredPosition=Vector2.Lerp(_top.anchoredPosition,topTarget,t);
            _bottom.anchoredPosition=Vector2.Lerp(_bottom.anchoredPosition,bottomTarget,t);
            _topGroup.alpha=Mathf.Lerp(_topGroup.alpha,UpperVisible?1:0,t);
            _bottomGroup.alpha=Mathf.Lerp(_bottomGroup.alpha,CardsVisible?1:0,t);
            _topGroup.interactable=_topGroup.blocksRaycasts=UpperVisible;
            _bottomGroup.interactable=_bottomGroup.blocksRaycasts=CardsVisible;
            _lowerHandle.anchoredPosition=new Vector2(_lowerOrigin.x,Mathf.Max(24,_lowerOrigin.y+_bottom.anchoredPosition.y-_bottomOrigin.y));
            SetGlyph(_upperLabel, UpperVisible);
            SetGlyph(_lowerLabel, CardsVisible);
        }

        private static void SetGlyph(TMP_Text label, bool panelVisible)
        {
            if (label == null) return;
            label.gameObject.SetActive(true);
            label.text = panelVisible ? "×" : "⌃";
            label.color = panelVisible ? new Color32(202, 62, 53, 255) : new Color32(255, 236, 181, 255);
            label.outlineColor = panelVisible ? new Color32(80, 19, 18, 255) : new Color32(43, 28, 13, 255);
            label.outlineWidth = .2f;
        }
    }
}
