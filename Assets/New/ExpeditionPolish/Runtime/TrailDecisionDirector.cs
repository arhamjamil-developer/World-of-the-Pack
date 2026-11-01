using System.Collections;
using UnityEngine;
using WorldOfThePack.Game;
using WorldOfThePack.NewUI;

namespace WorldOfThePack.Expedition
{
    public enum TrailChoice { Spring, Hunt, Rescue, Continue }
    public sealed class TrailDecisionDirector : MonoBehaviour
    {
        public static TrailDecisionDirector Instance { get; private set; }
        public GameObject routeRoot, rescueRoot, focusMarker;
        public TrailChoiceNode springNode, huntNode, rescueNode, continueNode;
        public Sprite[] cageFrames, rescuedHeroSprites;
        public int rescueCost = 4;
        public Combatant FocusedTarget { get; private set; }
        public int HuntEncountersRemaining { get; private set; }
        public int RoutesChosen { get; private set; }
        public int HeroesRescued { get; private set; }
        public bool IsChoosing { get; private set; }
        public int ExtraEnemies => HuntEncountersRemaining > 0 ? 2 : 0;
        public float CharmMultiplier => HuntEncountersRemaining > 0 ? 1.5f : 1;
        private GameDirector _game;
        private WaveManager _waves;
        private CampManager _camp;
        private bool _rescueChoice, _resolving;
        private float _focusRemaining;
        private int _rescueIndex;
        private static readonly string[] RescueIds = { "ScoutPip", "WizardMomo", "TinkerTilly" };
        private static readonly string[] RescueNames = { "PIP", "MOMO", "TILLY" };
        private void Awake() => Instance = this;
        private void Start()
        {
            _game = GameDirector.Instance; _waves = WaveManager.Instance; _camp = CampManager.Instance;
            if (_waves != null) _waves.OnWaveCompleted += OnWaveCompleted;
            if (_game != null) _game.OnStateChanged += OnState;
            if (_camp != null) _camp.OnCampReached += CancelForCamp;
            ResetForRun();
        }
        private void OnDestroy()
        {
            if (_waves != null) _waves.OnWaveCompleted -= OnWaveCompleted;
            if (_game != null) _game.OnStateChanged -= OnState;
            if (_camp != null) _camp.OnCampReached -= CancelForCamp;
            if (Instance == this) Instance = null;
        }
        private void OnState(GameState state)
        {
            if (state == GameState.Playing && _game.RunSeconds < .05f) ResetForRun();
            if (state == GameState.GameOver) CancelForCamp();
        }
        public void ResetForRun()
        {
            StopAllCoroutines();
            HuntEncountersRemaining = RoutesChosen = HeroesRescued = _rescueIndex = 0;
            IsChoosing = _resolving = false;
            if (_game != null) _game.IsDecisionPending = false;
            if (routeRoot != null) routeRoot.SetActive(false);
            if (rescueRoot != null) rescueRoot.SetActive(false);
            ClearFocus();
        }
        private void CancelForCamp()
        {
            StopAllCoroutines(); IsChoosing = _resolving = false;
            if (_game != null) _game.IsDecisionPending = false;
            routeRoot.SetActive(false); rescueRoot.SetActive(false); ClearFocus();
        }
        private void OnWaveCompleted(int wave)
        {
            if (ArenaDirector.Instance != null) return;
            if (HuntEncountersRemaining > 0) HuntEncountersRemaining--;
            if (_game == null || _game.CurrentState != GameState.Playing || (_camp != null && _camp.IsAtCamp)) return;
            if (wave % 4 == 2) BeginChoice(false);
            else if (wave % 4 == 3 && _game.ActiveAllyCount < _game.MaxAllies) BeginChoice(true);
        }
        public bool BeginChoice(bool rescue)
        {
            if (IsChoosing || _game == null || _game.CurrentState != GameState.Playing ||
                (_camp != null && _camp.IsAtCamp) || HasLivingThreat()) return false;
            IsChoosing = true; _rescueChoice = rescue; _resolving = false;
            _game.IsDecisionPending = true; ClearFocus();
            _waves.PauseForCamp(); FormationController.Instance.SetMarching(false);
            Vector3 anchor = FormationController.Instance.AnchorPosition;
            routeRoot.transform.position = rescueRoot.transform.position = anchor + Vector3.up * 2.1f;
            routeRoot.SetActive(!rescue); rescueRoot.SetActive(rescue);
            if (rescue)
            {
                _rescueIndex = HeroesRescued % RescueIds.Length;
                rescueNode.SetLabel($"FREE {RescueNames[_rescueIndex]}\n{rescueCost} CHARMS");
                rescueNode.SetCaptive(rescuedHeroSprites != null && _rescueIndex < rescuedHeroSprites.Length ? rescuedHeroSprites[_rescueIndex] : null);
                rescueNode.SetSprite(cageFrames[0]);
            }
            PackGameplayHUD.Instance?.ShowBanner(rescue ? "A PACKMATE IS TRAPPED • Rescue or save your charms" : "CHOOSE A ROUTE • Recovery or a richer, harder hunt", 30);
            return true;
        }
        private bool HasLivingThreat()
        {
            foreach (var unit in Combatant.LivingUnits) if (unit != null && unit.IsAlive && unit.Faction == UnitFaction.Hunters) return true;
            return false;
        }
        public bool Choose(TrailChoice choice)
        {
            if (!IsChoosing || _resolving || _game.CurrentState != GameState.Playing) return false;
            if (_rescueChoice)
            {
                if (choice == TrailChoice.Continue) { FinishChoice(); return true; }
                if (choice != TrailChoice.Rescue) return false;
                var hero = _game.RecruitRescuedHero(RescueIds[_rescueIndex], rescueCost);
                if (hero == null) { PackGameplayHUD.Instance?.ShowBanner($"Rescue needs {rescueCost} charms and a free pack slot", 3); return false; }
                HeroesRescued++; _resolving = true; StartCoroutine(OpenCage()); return true;
            }
            if (choice != TrailChoice.Spring && choice != TrailChoice.Hunt) return false;
            RoutesChosen++;
            if (choice == TrailChoice.Spring)
            {
                foreach (var unit in Combatant.LivingUnits) if (unit is HeroUnit hero && hero.IsAlive) hero.Heal(hero.MaxHealth * .2f);
                PackGameplayHUD.Instance?.ShowBanner("SPRING ROUTE • Pack restored by 20%", 3);
            }
            else
            {
                HuntEncountersRemaining = 2;
                PackGameplayHUD.Instance?.ShowBanner("HUNT ROUTE • +2 foes and +50% charms for 2 encounters", 4);
            }
            FinishChoice(); return true;
        }
        private IEnumerator OpenCage()
        {
            for (int i = 1; i < cageFrames.Length; i++) { rescueNode.SetSprite(cageFrames[i]); yield return new WaitForSeconds(.18f); }
            PackGameplayHUD.Instance?.ShowBanner($"{RescueNames[_rescueIndex]} JOINS THE PACK • Card charge preserved", 3);
            FinishChoice();
        }
        private void FinishChoice()
        {
            IsChoosing = _resolving = false; _game.IsDecisionPending = false;
            routeRoot.SetActive(false); rescueRoot.SetActive(false);
            _waves.ResumeFromCamp(); FormationController.Instance.SetMarching(true);
        }
        public bool SelectEnemy(Combatant target)
        {
            if (_game == null || _game.CurrentState != GameState.Playing || IsChoosing || (_camp != null && _camp.IsAtCamp) ||
                target == null || !target.IsAlive || target.Faction != UnitFaction.Hunters) return false;
            FocusedTarget = target; _focusRemaining = 6;
            focusMarker.SetActive(true);
            PackGameplayHUD.Instance?.ShowBanner("FOCUS FIRE • " + target.UnitName, 2);
            return true;
        }
        private void ClearFocus() { FocusedTarget = null; if (focusMarker != null) focusMarker.SetActive(false); }
        private void Update()
        {
            if (_game == null || _game.CurrentState != GameState.Playing || IsChoosing || (_camp != null && _camp.IsAtCamp)) return;
            if (FocusedTarget == null || !FocusedTarget.IsAlive || (_focusRemaining -= Time.deltaTime) <= 0) { ClearFocus(); return; }
            focusMarker.transform.position = FocusedTarget.Position + Vector3.up * .12f;
            focusMarker.transform.localScale = Vector3.one * (1 + .05f * Mathf.Sin(Time.time * 8));
        }
    }
}
