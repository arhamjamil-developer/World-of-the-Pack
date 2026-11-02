using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    public sealed class PackTaskDrawer : MonoBehaviour
    {
        [Header("Task journal")]
        [SerializeField] private PackTaskDirector _tasks;
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private CanvasGroup _panelGroup;
        [SerializeField] private Button _handle, _close;
        [SerializeField] private TMP_Text _title, _instruction, _progress, _reward, _tip, _handleProgress;
        [SerializeField] private Image _fill, _unreadDot;
        private GameDirector _game;
        private CampManager _camp;
        private PackTask _lastTask;
        private int _completed;
        private bool _unread;
        private ArenaPhase _lastPhase;
        public bool IsOpen { get; private set; }
        public string DisplayedTitle => _title != null ? _title.text : "";
        public bool HasOverflow => (_title != null && _title.isTextOverflowing) || (_instruction != null && _instruction.isTextOverflowing) || (_tip != null && _tip.isTextOverflowing);
        private void Start()
        {
            _game=GameDirector.Instance; _camp=CampManager.Instance;
            _handle.onClick.AddListener(Toggle); _close.onClick.AddListener(Close);
            Close(); _panel.anchoredPosition=new Vector2(-430,0); _panelGroup.alpha=0;
        }
        private void OnDestroy()
        { if(_handle!=null)_handle.onClick.RemoveListener(Toggle);if(_close!=null)_close.onClick.RemoveListener(Close); }
        public void Toggle() { if(IsOpen)Close();else Open(); }
        public void Open() { IsOpen=true;_unread=false; }
        public void Close() => IsOpen=false;
        private void LateUpdate()
        {
            if(_game==null || _tasks==null || _root==null)return;
            bool visible=_game.CurrentState==GameState.Playing && (_camp==null || !_camp.IsAtCamp) &&
                !(TutorialManager.Instance!=null && TutorialManager.Instance.IsShowing);
            _root.SetActive(visible);
            if(!visible){Close();return;}
            var arena=ArenaDirector.Instance;
            if(arena!=null && arena.Phase!=_lastPhase)
            { if(arena.Phase==ArenaPhase.Battle)Close();_lastPhase=arena.Phase; }
            var task=_tasks.CurrentTask;if(task==null){_root.SetActive(false);return;}
            if(task!=_lastTask || _completed!=_tasks.CompletedTasks)
            {_lastTask=task;_completed=_tasks.CompletedTasks;_unread=!IsOpen;}
            int target=Mathf.Max(1,task.target), value=Mathf.Clamp(Mathf.FloorToInt(_tasks.CurrentProgress),0,target);
            bool complete=value>=target;
            Set(_title,complete?"Task complete":System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase((task.title??"").ToLowerInvariant()));
            string instruction=task.hint??"";int separator=instruction.IndexOf('•');
            if(separator>=0)instruction=instruction.Substring(0,separator).Trim();
            Set(_instruction,complete?"Well done! Your charms have been added.":instruction);
            string unit=task.kind==PackTaskKind.Defeat?"enemies defeated":task.kind==PackTaskKind.Deploy?"allies recruited":
                task.kind==PackTaskKind.Moonshift?"Moonshift used":task.kind==PackTaskKind.Camp?"camps reached":"seconds survived";
            Set(_progress,$"{value} / {target}  {unit}");
            Set(_reward,$"{(complete?"Earned":"Reward")}  +{task.reward} charms");
            Set(_handleProgress,$"{value}/{target}");
            Set(_tip,TipFor(task.kind));
            _fill.fillAmount=(float)value/target;
            _fill.color=complete?new Color(.83f,.57f,.13f):new Color(.14f,.47f,.4f);
            _unreadDot.enabled=_unread;
            float t=1-Mathf.Exp(-16*Time.unscaledDeltaTime);
            _panel.anchoredPosition=Vector2.Lerp(_panel.anchoredPosition,new Vector2(IsOpen?54:-430,0),t);
            _panelGroup.alpha=Mathf.Lerp(_panelGroup.alpha,IsOpen?1:0,t);
            _panelGroup.interactable=_panelGroup.blocksRaycasts=IsOpen;
        }
        private static void Set(TMP_Text label,string text){if(label!=null && label.text!=text)label.text=text;}
        private static string TipFor(PackTaskKind kind)
        {
            switch(kind)
            {
                case PackTaskKind.Defeat:return "Tip: Guard ranged allies with a tank.";
                case PackTaskKind.Deploy:return "Tip: Full charge + charms = a recruit.";
                case PackTaskKind.Moonshift:return "Tip: Save Moonshift for heavy enemies.";
                case PackTaskKind.Camp:return "Tip: Hold ahead to reach a camp.";
                default:return "Tip: Protect the queen's hearts.";
            }
        }
    }
}
