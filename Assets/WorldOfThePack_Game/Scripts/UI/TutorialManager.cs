using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game
{
    public enum CoachStep { Queen, ReadEnemies, SelectCard, PlaceUnit, EnergySpent, BeginBattle, Combat, Victory }
    public sealed class TutorialManager : MonoBehaviour
    {
        public const string CompletedKey="WorldOfThePack.RoyalEscort.Tutorial.v2";
        public static TutorialManager Instance { get; private set; }
        public bool RequiresDeployment => _active && _step!=CoachStep.Victory;
        public bool IsShowing => _active;
        public CoachStep Step => _step;
        public bool CanBeginBattle => !_active || _step==CoachStep.BeginBattle || _step==CoachStep.Combat;
        public Vector3 SuggestedCell => ArenaDirector.Instance.CellPosition(2,0);
        private bool _active, _rewardObserved;
        private CoachStep _step;
        private GameDirector _game;
        private ArenaDirector _arena;
        private HeroDeploymentCard _card;
        private GameObject _root;
        private TMP_Text _title, _copy, _nextLabel;
        private Button _next;
        private CoachSpotlight _spotlight;
        private CoachFocusFrame _frame;
        private CoachFingerGraphic _finger;
        private RectTransform _canvasRect;
        private RectTransform _callout;
        private int _killsBeforeBattle;
        private readonly Vector3[] _corners=new Vector3[4];
        private void Awake()=>Instance=this;
        private void Start()
        {
            _game=GameDirector.Instance;_arena=ArenaDirector.Instance;_game.OnStateChanged+=OnState;
            _active=PlayerPrefs.GetInt(CompletedKey,0)==0;if(!_active)return;
            var cards=FindObjectsByType<HeroDeploymentCard>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var card in cards)if(card.Definition!=null && card.Definition.heroId=="PrincessElva")_card=card;
            if(_card==null)foreach(var card in cards)if(card.Definition!=null && card.gameObject.activeInHierarchy){_card=card;break;}
            BuildCoach();SetStep(CoachStep.Queen);
        }
        private static T Graphic<T>(Transform parent,string name) where T:UnityEngine.UI.Graphic
        {var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer));go.transform.SetParent(parent,false);return go.AddComponent<T>();}
        private void BuildCoach()
        {
            _canvasRect=(RectTransform)_arena.OverlayCanvas.transform;
            _root=new GameObject("Interactive coach",typeof(RectTransform));_root.transform.SetParent(_canvasRect,false);
            RoyalUI.Place(_root.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);
            _spotlight=Graphic<CoachSpotlight>(_root.transform,"Spotlight input hole");
            RoyalUI.Place(_spotlight.rectTransform,Vector2.zero,Vector2.one);_spotlight.color=new Color(0,.025f,.035f,.65f);
            _frame=Graphic<CoachFocusFrame>(_root.transform,"Pulsing target frame");_frame.raycastTarget=false;
            _finger=Graphic<CoachFingerGraphic>(_root.transform,"Animated finger");_finger.raycastTarget=false;_finger.rectTransform.sizeDelta=new Vector2(120,170);
            var panel=RoyalUI.Panel(_root.transform,"Coach callout",new Vector2(.05f,.755f),new Vector2(.95f,.9f));
            _callout=(RectTransform)panel.transform;
            _title=RoyalUI.Label(panel.transform,"Action title",new Vector2(.07f,.72f),new Vector2(.93f,.94f),31);
            _copy=RoyalUI.Label(panel.transform,"Action hint",new Vector2(.07f,.29f),new Vector2(.93f,.73f),27);
            _copy.enableAutoSizing=true;_copy.fontSizeMin=21;_copy.fontSizeMax=27;
            RoyalUI.Button(panel.transform,"SKIP",new Vector2(.07f,.055f),new Vector2(.31f,.26f),Skip);
            _next=RoyalUI.Button(panel.transform,"SHOW ME",new Vector2(.53f,.055f),new Vector2(.93f,.26f),Next);
            _nextLabel=_next.GetComponentInChildren<TMP_Text>();_nextLabel.fontSize=23;
        }
        private string CardName()=>_card!=null && _card.Definition!=null?_card.Definition.shortName:"Your card";
        private void SetStep(CoachStep step)
        {
            _step=step;_root.SetActive(true);_root.transform.SetAsLastSibling();
            RoyalUI.Place(_callout,new Vector2(.05f,step==CoachStep.BeginBattle?.6f:.755f),new Vector2(.95f,step==CoachStep.BeginBattle?.745f:.9f));
            _next.gameObject.SetActive(step==CoachStep.Queen || step==CoachStep.ReadEnemies || step==CoachStep.EnergySpent || step==CoachStep.Victory);
            _nextLabel.text=step==CoachStep.Victory?"KEEP GOING":step==CoachStep.EnergySpent?"READY TO FIGHT":"SHOW ME";
            switch(step)
            {
                case CoachStep.Queen:_title.text="ONE GUARD. ONE QUEEN.";_copy.text="Wolvy is your first defender. Keep the queen's hearts above zero. Build the rest of your pack with cards.";break;
                case CoachStep.ReadEnemies:_title.text="READ THEIR FORMATION";_copy.text="Red bars show enemy health. Soldiers push forward. Wolvy meets them; a ranged ally helps from a safe tile behind.";break;
                case CoachStep.SelectCard:_title.text="TAP THE GLOWING CARD";_copy.text=$"{CardName()}: 100 charge means READY. Check its charm price, then tap the card to choose it.";break;
                case CoachStep.PlaceUnit:_title.text="NOW TAP THE GLOWING TILE";_copy.text="Place support behind Wolvy. Front tiles meet swords first; rear tiles give ranged allies time to attack.";break;
                case CoachStep.EnergySpent:_title.text="ONE CARD. ONE RECRUIT.";_copy.text=$"See {CardName()}'s empty charge bar? Deploying spends that card's charge and charms. Enemy defeats recharge every card.";break;
                case CoachStep.BeginBattle:_title.text="YOUR FORMATION IS READY";_copy.text="Tap BEGIN BATTLE. Watch Wolvy intercept the soldiers while your new ally supports him.";break;
                case CoachStep.Combat:_title.text="WATCH YOUR PLAN COME ALIVE";_copy.text="Your pack fights automatically. Watch enemy health and your card charge. Reinforce only when a card is READY.";break;
                case CoachStep.Victory:_title.text="FIRST WAVE SURVIVED!";_copy.text="Read each new formation. Bobo holds the front, Elva fires from safety, and Lulu supports wounded fighters. Save charms.";break;
            }
            Time.timeScale=step==CoachStep.Queen || step==CoachStep.Victory?0:1;UpdateFocus();
        }
        public bool AllowsCard(string id)
        {
            if(!_active || _step==CoachStep.Combat)return true;
            return (_step==CoachStep.SelectCard || _step==CoachStep.PlaceUnit) && _card!=null && _card.Definition.heroId==id;
        }
        public bool AllowsPlacement(string id,Vector3 point)
        {return !_active || _step==CoachStep.Combat || (_step==CoachStep.PlaceUnit && AllowsCard(id) && Vector3.SqrMagnitude(point-SuggestedCell)<.05f);}
        public void CardSelected(HeroDeploymentCard card)
        {if(_active && card==_card && _step==CoachStep.SelectCard)SetStep(CoachStep.PlaceUnit);}
        public void UnitPlaced(HeroUnit unit)
        {if(_active && _step==CoachStep.PlaceUnit && _card!=null && unit.HeroId==_card.Definition.heroId)SetStep(CoachStep.EnergySpent);}
        public void EncounterReady(int wave)
        {if(_active && wave==1 && _step==CoachStep.ReadEnemies)UpdateFocus();}
        public void BattleStarted()
        {if(_active){_killsBeforeBattle=_game.TotalKills;SetStep(CoachStep.Combat);}}
        public void Next()
        {
            if(!_active)return;
            switch(_step)
            {
                case CoachStep.Queen:SetStep(CoachStep.ReadEnemies);break;
                case CoachStep.ReadEnemies:if(_arena.IsPreparing)SetStep(CoachStep.SelectCard);break;
                case CoachStep.EnergySpent:SetStep(CoachStep.BeginBattle);break;
                case CoachStep.Victory:Complete();break;
            }
        }
        private void LateUpdate()
        {
            if(!_active || _root==null)return;
            if(_game.CurrentState==GameState.GameOver){_root.SetActive(false);return;}
            if(_step==CoachStep.ReadEnemies)_next.interactable=_arena.IsPreparing;
            if(_step==CoachStep.Combat && !_rewardObserved && _game.TotalKills>_killsBeforeBattle)
            {_rewardObserved=true;_title.text="A DEFEAT RECHARGED YOUR CARDS";_copy.text="Watch the green charge bars fill. You still need enough charms before you can recruit again.";}
            UpdateFocus();
        }
        private void UpdateFocus()
        {
            if(_root==null)return;Rect focus;
            switch(_step)
            {
                case CoachStep.Queen:focus=WorldRect(_arena.Queen.Position,1.8f,1.4f);break;
                case CoachStep.ReadEnemies:focus=WorldRect(_arena.EnemyFrontCenter,3.8f,1.3f);break;
                case CoachStep.SelectCard:focus=_card!=null?UIRect((RectTransform)_card.transform):WorldRect(SuggestedCell,.7f,.7f);break;
                case CoachStep.PlaceUnit:focus=WorldRect(SuggestedCell,.72f,.7f);break;
                case CoachStep.BeginBattle:focus=UIRect(_arena.FightButtonRect);break;
                case CoachStep.EnergySpent:case CoachStep.Combat:focus=_card!=null && _card.energyLabel!=null?UIRect(_card.energyLabel.rectTransform):WorldRect(SuggestedCell,.7f,.7f);break;
                default:focus=WorldRect(_arena.Queen.Position,1.8f,1.4f);break;
            }
            focus=new Rect(focus.xMin-8,focus.yMin-8,focus.width+16,focus.height+16);
            bool playing=_step==CoachStep.Combat;_spotlight.gameObject.SetActive(!playing);_spotlight.Focus(focus);
            _frame.rectTransform.anchoredPosition=focus.center;_frame.rectTransform.sizeDelta=focus.size;
            float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*5);_frame.color=new Color(.45f,1,.73f,.55f+.45f*pulse);
            _finger.gameObject.SetActive(!playing);var tip=new Vector2(focus.center.x,focus.center.y-15);
            if(_step==CoachStep.PlaceUnit && _card!=null)
            {
                float travel=Mathf.Repeat(Time.unscaledTime*.65f,1);
                tip=Vector2.Lerp(UIRect((RectTransform)_card.transform).center,focus.center,Mathf.SmoothStep(0,1,Mathf.Clamp01(travel*1.5f)));
            }
            else tip.y-=pulse*12;
            _finger.rectTransform.anchoredPosition=tip;_finger.rectTransform.localScale=Vector3.one*(.95f+pulse*.06f);
            _arena.SetTutorialCell(_step==CoachStep.PlaceUnit?SuggestedCell:(Vector3?)null);
        }
        private Rect UIRect(RectTransform target)
        {
            target.GetWorldCorners(_corners);var a=Local(RectTransformUtility.WorldToScreenPoint(null,_corners[0]));var b=Local(RectTransformUtility.WorldToScreenPoint(null,_corners[2]));
            return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        private Rect WorldRect(Vector3 center,float width,float height)
        {
            var cam=Camera.main;if(cam==null)return new Rect(-50,-50,100,100);
            var a=Local(cam.WorldToScreenPoint(center-new Vector3(width,height)*.5f));var b=Local(cam.WorldToScreenPoint(center+new Vector3(width,height)*.5f));
            return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        private Vector2 Local(Vector2 point)
        {RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect,point,null,out var result);return result;}
        public void WaveSurvived(int wave){if(_active && wave==1)SetStep(CoachStep.Victory);}
        private void OnState(GameState state)
        {if(_active && state==GameState.Playing && _game.RunSeconds<.05f && _root!=null && _step!=CoachStep.Queen){_rewardObserved=false;SetStep(CoachStep.Queen);}}
        public void Skip()=>Complete();
        private void Complete()
        {PlayerPrefs.SetInt(CompletedKey,1);PlayerPrefs.Save();_active=false;if(_root!=null)_root.SetActive(false);_arena?.SetTutorialCell(null);Time.timeScale=1;}
        public static void ResetTutorial(){PlayerPrefs.DeleteKey(CompletedKey);PlayerPrefs.Save();}
        private void OnDestroy(){if(_game!=null)_game.OnStateChanged-=OnState;if(Instance==this)Instance=null;}
    }
}
