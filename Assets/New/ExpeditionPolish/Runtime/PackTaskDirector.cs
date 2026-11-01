using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WorldOfThePack.Game;

namespace WorldOfThePack.Expedition
{
    public sealed class PackTaskDirector : MonoBehaviour
    {
        public PackTaskCatalog catalog;
        public TMP_Text titleLabel, detailLabel, resultLabel;
        public Image progressFill;
        private GameDirector _game;
        private Image _taskPlate;
        private PackTaskDrawer _drawer;
        private CampManager _camp;
        private int _index, _killsAtStart, _deployments, _deployAtStart, _moonshifts, _moonAtStart, _campsAtStart;
        private float _travelAtStart, _advanceDelay;
        private bool _moonActive, _rewarded;
        public int CompletedTasks { get; private set; }
        public int RewardsGranted { get; private set; }
        public PackTask CurrentTask => catalog != null && catalog.tasks != null && catalog.tasks.Length > 0 ? catalog.tasks[_index % catalog.tasks.Length] : null;
        public float CurrentProgress { get; private set; }
        private void Start()
        {
            _game = GameDirector.Instance;
            _drawer = GetComponent<PackTaskDrawer>();
            if (titleLabel != null)
            {
                var plate = titleLabel.transform.parent.Find("TaskPlate");
                if (plate != null) _taskPlate = plate.GetComponent<Image>();
            }
            _camp = CampManager.Instance;
            if (_game == null) return;
            _game.OnHeroDeployed += OnDeploy;
            _game.OnWolfInstinctChanged += OnMoon;
            _game.OnStateChanged += OnState;
            ResetTasks();
        }
        private void OnDestroy()
        {
            if (_game == null) return;
            _game.OnHeroDeployed -= OnDeploy;
            _game.OnWolfInstinctChanged -= OnMoon;
            _game.OnStateChanged -= OnState;
        }
        private void OnDeploy() => _deployments++;
        private void OnMoon(bool active, float seconds)
        {
            if (active && !_moonActive) _moonshifts++;
            _moonActive = active;
        }
        private void OnState(GameState state)
        {
            if (state == GameState.Playing && _game.RunSeconds < .05f) ResetTasks();
        }
        public void ResetTasks()
        {
            _index = _deployments = _moonshifts = CompletedTasks = RewardsGranted = 0;
            _moonActive = false;
            BeginTask();
        }
        private void BeginTask()
        {
            _killsAtStart = _game.TotalKills;
            _deployAtStart = _deployments;
            _moonAtStart = _moonshifts;
            _campsAtStart = _camp != null ? _camp.CampsVisited : 0;
            _travelAtStart = _game.RunSeconds;
            _rewarded = false;
            _advanceDelay = 0;
            CurrentProgress = 0;
        }
        private void Update()
        {
            if (_game == null || CurrentTask == null) return;
            bool showTask = _drawer == null && !(ArenaDirector.Instance != null && ArenaDirector.Instance.IsPreparing) &&
                !(TutorialManager.Instance != null && TutorialManager.Instance.IsShowing);
            if (_taskPlate != null) _taskPlate.enabled = showTask;
            if (titleLabel != null) titleLabel.enabled = showTask;
            if (detailLabel != null) detailLabel.enabled = showTask;
            if (progressFill != null) progressFill.enabled = showTask;
            var task = CurrentTask;
            bool active = _game.CurrentState == GameState.Playing && !_game.IsDecisionPending;
            if (active && !_rewarded)
            {
                CurrentProgress = task.kind switch
                {
                    PackTaskKind.Defeat => _game.TotalKills - _killsAtStart,
                    PackTaskKind.Deploy => _deployments - _deployAtStart,
                    PackTaskKind.Moonshift => _moonshifts - _moonAtStart,
                    PackTaskKind.Camp => (_camp != null ? _camp.CampsVisited : 0) - _campsAtStart,
                    _ => _game.RunSeconds - _travelAtStart
                };
                if (CurrentProgress >= task.target)
                {
                    _rewarded = true;
                    _advanceDelay = 2.5f;
                    CompletedTasks++;
                    RewardsGranted += task.reward;
                    _game.AddCharms(task.reward);
                }
            }
            if (active && _rewarded)
            {
                _advanceDelay -= Time.deltaTime;
                if (_advanceDelay <= 0) { _index++; BeginTask(); task = CurrentTask; }
            }
            if (titleLabel != null) titleLabel.text = _rewarded ? $"TASK COMPLETE  +{task.reward} CHARMS" : $"PACK TASK • {task.title}";
            if (detailLabel != null) detailLabel.text = _rewarded ? "Well done, pack. A new task is coming…" : $"{Mathf.Min(task.target, Mathf.FloorToInt(CurrentProgress))}/{task.target}  •  {task.hint}";
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(CurrentProgress / task.target);
            if (resultLabel != null) resultLabel.text = $"Protect the queen and her escort\n\n{Mathf.FloorToInt(_game.RunSeconds / 60):00}:{Mathf.FloorToInt(_game.RunSeconds % 60):00} survived  •  {_game.TotalKills} enemies defeated\n{CompletedTasks} pack tasks completed\n{_game.MoonCharms} Moon Charms remaining\n\nTip: place Bobo ahead of the throne.\nSave Moonshift for the heavy patrol.";
        }
    }
}
