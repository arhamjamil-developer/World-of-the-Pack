using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game
{
    public enum ArenaPhase { March, Deployment, Battle }
    public sealed class ArenaDirector : MonoBehaviour
    {
        public static ArenaDirector Instance { get; private set; }
        public ArenaPhase Phase { get; private set; }
        public QueenEscort Queen { get; private set; }
        public bool IsPreparing => Phase == ArenaPhase.Deployment;
        public int DeployedThisEncounter { get; private set; }
        private readonly Dictionary<HeroUnit,Vector3> _positions = new Dictionary<HeroUnit,Vector3>();
        private GameObject _grid;
        private GameObject _controls;
        private TMP_Text _status;
        private Button _fight;
        private GameDirector _game;
        private FormationController _formation;
        private Material _lineMaterial;
        private Canvas _canvas;
        private readonly List<ArenaCellView> _cells = new List<ArenaCellView>();
        private readonly HashSet<int> _enemySlots = new HashSet<int>();
        private HeroDeploymentCard _selectedCard;
        private Vector3? _tutorialCell;
        private Sprite _tileSprite;
        private Camera _camera;
        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();
        private PointerEventData _pointerData;
        private string _encounterTitle;
        private CanvasGroup _ordersGroup;
        public void CancelSelection() => ClearSelection();
        public void ApplyCompactStyle(TMP_FontAsset font)
        {
            if(font==null)return;
            _status.font=font;_status.fontStyle=FontStyles.Bold;
            var art=Resources.Load<Sprite>("PackHUD/HUDToggleBanner");
            if(art!=null){_controls.GetComponent<Image>().sprite=art;_fight.GetComponent<Image>().sprite=art;}
            var label=_fight.GetComponentInChildren<TMP_Text>();label.font=font;label.fontStyle=FontStyles.Bold;label.fontSize=22;
        }
        public void SetOrdersVisible(bool visible)
        {
            if(_ordersGroup==null)return;
            _ordersGroup.alpha=Mathf.Lerp(_ordersGroup.alpha,visible?1:0,1-Mathf.Exp(-16*Time.unscaledDeltaTime));
            _ordersGroup.interactable=_ordersGroup.blocksRaycasts=visible;
        }
        public HeroDeploymentCard SelectedCard => _selectedCard;
        public RectTransform FightButtonRect => (RectTransform)_fight.transform;
        public Vector3 EnemyFrontCenter => new Vector3(0,AnchorY+2.75f,0);
        private float AnchorY => _formation!=null?_formation.AnchorPosition.y:(FormationController.Instance!=null?FormationController.Instance.AnchorPosition.y:0);
        public Vector3 CellPosition(int lane,int row) => new Vector3(LaneSystem.GetLaneX(lane),AnchorY+row*.7f,0);
        public void SetTutorialCell(Vector3? point) => _tutorialCell=point;
        public bool IsValidCell(Vector3 point) => Mathf.Abs(point.x)<=1.8f && point.y>=AnchorY-.35f && point.y<=AnchorY+2.45f && CellFree(SnapDeployment(point));
        public Canvas OverlayCanvas => _canvas;
        private void Awake()
        {
            Instance = this;
            _game = GetComponent<GameDirector>();
            var queen = new GameObject("Queen • Royal Escort");
            Queen = queen.AddComponent<QueenEscort>();
            _game.OnHeroDeployed += OnDeployment;
            BuildUI();
            gameObject.AddComponent<TutorialManager>();
        }
        private void Start() { _formation=FormationController.Instance; _camera=Camera.main; }
        private void OnDestroy()
        {
            if (_game != null) _game.OnHeroDeployed -= OnDeployment;
            if (_lineMaterial != null) Destroy(_lineMaterial);
            if (_canvas != null) Destroy(_canvas.gameObject);
            if (_grid != null) Destroy(_grid);
            if (Queen != null) Destroy(Queen.gameObject);
            if (Instance == this) Instance = null;
        }
        public void ResetRun()
        {
            ClearSelection(); _positions.Clear(); Phase = ArenaPhase.March;
            _controls.SetActive(false);
            if (_grid != null) _grid.SetActive(false);
            Queen.ResetEscort();
        }
        private void OnDeployment()
        {
            if (IsPreparing) DeployedThisEncounter++;
        }
        public IEnumerator PrepareEncounter(int wave, int enemies, string name, System.Action stageEnemies)
        {
            while (_game.IsDecisionPending || (CampManager.Instance != null && CampManager.Instance.IsAtCamp)) yield return null;
            Phase = ArenaPhase.Deployment;
            WorldOfThePack.NewUI.PackGameplayHUD.Instance?.SetWave(wave);
            DeployedThisEncounter = 0;
            _formation = FormationController.Instance;
            _formation?.SetMarching(false);
            _positions.Clear();
            foreach (var combatant in Combatant.LivingUnits)
            {
                if (!(combatant is HeroUnit hero) || !hero.IsAlive) continue;
                var position=hero==_game.AlphaLeader?CellPosition(2,2):SnapDeployment(hero.Position);
                if (TryReserveCell(ref position, true)) { _positions[hero]=position; hero.transform.position=position; }
            }
            _enemySlots.Clear();
            stageEnemies?.Invoke();
            BuildGrid();
            _controls.SetActive(true);
            _encounterTitle = $"WAVE {wave}  •  {enemies} ENEMIES";
            RefreshOrders();
            TutorialManager.Instance?.EncounterReady(wave);
            _fight.interactable = true;
            while (IsPreparing && _game.CurrentState != GameState.GameOver)
            {
                _fight.interactable = TutorialManager.Instance == null || TutorialManager.Instance.CanBeginBattle;
                yield return null;
            }
            _controls.SetActive(false);
        }
        public void BeginBattle()
        {
            if (!IsPreparing || _game.CurrentState != GameState.Playing) return;
            if (TutorialManager.Instance != null && !TutorialManager.Instance.CanBeginBattle) return;
            Phase = ArenaPhase.Battle;
            ClearSelection();
            TutorialManager.Instance?.BattleStarted();
        }
        public void EndBattle(int wave)
        {
            Phase = ArenaPhase.March;
            if (_grid!=null)_grid.SetActive(false);
            _positions.Clear();
            _game.AddCharms(Mathf.Max(1, 5 - wave / 3));
            _formation?.SetMarching(true);
            TutorialManager.Instance?.WaveSurvived(wave);
        }
        public Vector3 SnapDeployment(Vector3 point)
        {
            float y = _formation != null ? _formation.AnchorPosition.y : 0;
            return new Vector3(LaneSystem.SnapToLane(point.x), y + Mathf.Clamp(Mathf.Round((point.y-y)/.7f), 0, 3)*.7f, 0);
        }
        public void RegisterPosition(HeroUnit hero, Vector3 position) => _positions[hero] = position;
        public bool TryReserveCell(ref Vector3 position, bool findAlternative)
        {
            if (Phase == ArenaPhase.March) return true;
            position=SnapDeployment(position);
            if (CellFree(position)) return true;
            if (!findAlternative) return false;
            float y=_formation!=null ? _formation.AnchorPosition.y : 0;
            for(int row=0;row<4;row++)
            for(int lane=0;lane<5;lane++)
            {
                var candidate=new Vector3(LaneSystem.GetLaneX(lane),y+row*.7f,0);
                if(CellFree(candidate)){position=candidate;return true;}
            }
            return false;
        }
        private bool CellFree(Vector3 point)
        {
            foreach(var pair in _positions)
                if(pair.Key!=null && pair.Key.IsAlive && Vector3.SqrMagnitude(pair.Value-point)<.1f) return false;
            return true;
        }
        public bool TryGetPosition(HeroUnit hero, out Vector3 position)
        {
            position = default;
            return Phase != ArenaPhase.March && _positions.TryGetValue(hero,out position);
        }
        public void SelectCard(HeroDeploymentCard card)
        {
            if (!IsPreparing || card==null || !card.IsDeployReady) return;
            if (TutorialManager.Instance!=null && !TutorialManager.Instance.AllowsCard(card.Definition.heroId)) return;
            ClearSelection(); _selectedCard=card;card.SetSelected(true);
            _status.text=$"{card.Definition.shortName}  •  {Mathf.RoundToInt(card.Energy)}/100 CHARGE  •  {card.CharmPrice} CHARMS\n{RoleAdvice(card.Definition.heroId)}\nTap an open glowing tile. Remaining charms: {_game.MoonCharms}.";
            TutorialManager.Instance?.CardSelected(card);
        }
        private void ClearSelection()
        { if(_selectedCard!=null)_selectedCard.SetSelected(false);_selectedCard=null;RefreshOrders(); }
        private void RefreshOrders()
        {
            if (_status != null) _status.text=$"{_encounterTitle}\n{TacticalAdvice()}\nTap a ready card, then an open blue tile.";
        }
        private static string RoleAdvice(string id)
        {
            switch(id)
            {
                case "CaptainBobo":return "TANK: high health. Put him ahead of your ranged allies.";
                case "PrincessElva":return "RANGED: fire over the front line; keep her behind a defender.";
                case "FairyLulu":return "HEALER: support wounded fighters. She needs a front-line guard.";
                case "WizardMomo":return "AREA DAMAGE: useful against clustered enemies. Protect his back rank.";
                case "ScoutPip":return "FAST MELEE: reinforce weak lanes. Avoid a heavy guard alone.";
                case "TinkerTilly":return "RANGED TRAPS: support a busy lane from behind your melee fighters.";
                default:return "MELEE: intercept swords in the front ranks and guard your support.";
            }
        }
        public bool TryPlaceSelected(Vector3 point)
        {
            if(!IsPreparing || _selectedCard==null || !_selectedCard.IsDeployReady || !IsValidCell(point))return false;
            var cell=SnapDeployment(point);
            if(TutorialManager.Instance!=null && !TutorialManager.Instance.AllowsPlacement(_selectedCard.Definition.heroId,cell))return false;
            var card=_selectedCard;int before=DeployedThisEncounter;
            card.deployAtPositionRequested.Invoke(card.Definition,cell);
            if(DeployedThisEncounter<=before)return false;
            ClearSelection();return true;
        }
        private void Update()
        {
            if(!IsPreparing || _selectedCard==null || _game.CurrentState!=GameState.Playing)return;
            bool pressed=false;Vector2 position=default;
            if(Touchscreen.current!=null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {pressed=true;position=Touchscreen.current.primaryTouch.position.ReadValue();}
            else if(Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame)
            {pressed=true;position=Mouse.current.position.ReadValue();}
            if(!pressed || _camera==null)return;
            var events=EventSystem.current;
            if(events!=null)
            {
                if(_pointerData==null)_pointerData=new PointerEventData(events);
                _pointerData.position=position;_uiHits.Clear();events.RaycastAll(_pointerData,_uiHits);
                if(_uiHits.Count>0)return;
            }
            var point=_camera.ScreenToWorldPoint(new Vector3(position.x,position.y,-_camera.transform.position.z));point.z=0;
            TryPlaceSelected(point);
        }
        private void LateUpdate()
        {
            if(_grid==null || !_grid.activeSelf)return;
            float visibility=IsPreparing?1:.14f;
            foreach(var cell in _cells)
            {
                bool occupied=cell.Enemy?cell.EnemyOccupied:!CellFree(CellPosition(cell.Lane,cell.Row));
                bool focus=!cell.Enemy && _tutorialCell.HasValue && Vector3.SqrMagnitude(CellPosition(cell.Lane,cell.Row)-_tutorialCell.Value)<.05f;
                if(!cell.Enemy && !_tutorialCell.HasValue && _selectedCard!=null && !occupied)
                {
                    bool front=_selectedCard.Definition.heroId=="CaptainBobo" || _selectedCard.Definition.heroId=="HeroWolvy" || _selectedCard.Definition.heroId=="ScoutPip";
                    focus=front?cell.Row>=2:cell.Row<=1;
                }
                cell.Refresh(occupied,focus,visibility);
            }
        }
        public Vector3 ReserveEnemyPosition(EnemyType type,bool boss=false)
        {
            if(boss)
            { _enemySlots.Add(6);_enemySlots.Add(7);_enemySlots.Add(8);return new Vector3(0,AnchorY+3.3f,0); }
            int preferred=type==EnemyType.Elsa?2:type==EnemyType.Rudra?1:0;
            var candidates=new List<int>();
            for(int lane=0;lane<5;lane++)if(!_enemySlots.Contains(preferred*5+lane))candidates.Add(preferred*5+lane);
            if(candidates.Count==0)for(int i=0;i<15;i++)if(!_enemySlots.Contains(i))candidates.Add(i);
            if(candidates.Count==0)return EnemyFrontCenter;
            int slot=candidates[Random.Range(0,candidates.Count)];_enemySlots.Add(slot);
            return new Vector3(LaneSystem.GetLaneX(slot%5),AnchorY+2.75f+(slot/5)*.55f,0);
        }
        private string TacticalAdvice()
        {
            bool heavy=false,ranged=false;int count=0;
            foreach(var enemy in EnemyUnit.ActiveEnemies)
            {if(!enemy.IsAlive)continue;count++;heavy|=enemy.Type==EnemyType.Rudra;ranged|=enemy.Type==EnemyType.Elsa;}
            if(heavy && ranged)return "HEAVY + BOW: Bobo ahead, Elva behind; reinforce weak lanes.";
            if(heavy)return "HEAVY GUARD: protect a ranged attacker with a front-line tank.";
            if(ranged)return "BOW THREAT: a tank absorbs fire while your damage dealers advance.";
            return count>=4?"A CROWD: hold the front; Momo's area attacks help against groups.":"SWORDS: hold the front with Wolvy; keep Elva behind him.";
        }
        private void BuildGrid()
        {
            if(_grid!=null)Destroy(_grid);_cells.Clear();
            _grid=new GameObject("Arena • allied and enemy formations");_grid.transform.position=new Vector3(0,AnchorY,0);
            if(_lineMaterial==null)_lineMaterial=new Material(Shader.Find("Sprites/Default"));
            if(_tileSprite==null)_tileSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
            for(int side=0;side<2;side++)for(int row=0;row<(side==0?4:3);row++)for(int lane=0;lane<5;lane++)
            {
                bool enemy=side==1;float x=LaneSystem.GetLaneX(lane),y=(enemy?2.75f:0)+row*(enemy?.55f:.7f);
                var go=new GameObject($"{(enemy?"Enemy":"Ally")} lane {lane+1} rank {row+1}");go.transform.SetParent(_grid.transform,false);go.transform.localPosition=new Vector3(x,y,0);
                var view=go.AddComponent<ArenaCellView>();view.Lane=lane;view.Row=row;view.Enemy=enemy;view.EnemyOccupied=enemy && _enemySlots.Contains(row*5+lane);
                var fill=new GameObject("Tinted tile");fill.transform.SetParent(go.transform,false);fill.transform.localScale=new Vector3(.66f,enemy?.5f:.64f,1);
                view.Fill=fill.AddComponent<SpriteRenderer>();view.Fill.sprite=_tileSprite;view.Fill.sortingOrder=2;
                var cross=new GameObject("Open slot marker");cross.transform.SetParent(go.transform,false);cross.transform.localScale=new Vector3(.1f,.025f,1);
                view.Cross=cross.AddComponent<SpriteRenderer>();view.Cross.sprite=_tileSprite;view.Cross.sortingOrder=4;
                var vertical=new GameObject("Open slot vertical");vertical.transform.SetParent(go.transform,false);vertical.transform.localScale=new Vector3(.025f,.1f,1);
                view.CrossVertical=vertical.AddComponent<SpriteRenderer>();view.CrossVertical.sprite=_tileSprite;view.CrossVertical.sortingOrder=4;
                view.Border=go.AddComponent<LineRenderer>();var line=view.Border;line.sharedMaterial=_lineMaterial;line.useWorldSpace=false;line.loop=true;line.positionCount=4;line.sortingOrder=3;
                float edge=enemy?.25f:.32f;
                line.SetPositions(new[]{new Vector3(-.33f,-edge),new Vector3(.33f,-edge),new Vector3(.33f,edge),new Vector3(-.33f,edge)});
                _cells.Add(view);
            }
        }
        private void BuildUI()
        {
            var root=new GameObject("Royal Escort Overlay",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            _canvas=root.GetComponent<Canvas>(); _canvas.renderMode=RenderMode.ScreenSpaceOverlay; _canvas.sortingOrder=150;
            var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920); scaler.matchWidthOrHeight=.5f;
            _controls=RoyalUI.Panel(root.transform,"Deployment orders",new Vector2(.06f,.8f),new Vector2(.94f,.88f));
            _ordersGroup=_controls.AddComponent<CanvasGroup>();
            _status=RoyalUI.Label(_controls.transform,"Orders",new Vector2(.055f,.38f),new Vector2(.945f,.92f),22);
            _fight=RoyalUI.Button(_controls.transform,"BEGIN BATTLE",new Vector2(.23f,.065f),new Vector2(.77f,.32f),BeginBattle);
            _controls.SetActive(false);
        }
    }
}
