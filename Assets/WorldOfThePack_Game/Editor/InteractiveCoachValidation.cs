using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game.Editor
{
    // Runs the real UI raycasts and event handlers, including the spotlight's input hole.
    public static class InteractiveCoachValidation
    {
        public static string Result { get; private set; }="Not run";
        private static readonly List<string> Checks=new List<string>();
        private const string Folder="Assets/Visuals/RoyalEscort/Documentation/";
        public static string Run()
        {
            if(!EditorApplication.isPlaying)return "Enter Play Mode with the tutorial reset first.";
            Checks.Clear();Result="Running";GameDirector.Instance.StartCoroutine(Validate());return Result;
        }
        private static bool Check(bool condition,string name)
        {
            Checks.Add((condition?"PASS: ":"FAIL: ")+name);
            if(!condition)Finish("Failed");return condition;
        }
        private static void Finish(string state)
        {Result=state;File.WriteAllText(Folder+"Interactive-Coach-Checks.txt",state+"\n"+string.Join("\n",Checks));}
        private static Vector2 Screen(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.position);
        private static void Click(Vector2 screen)
        {
            var events=EventSystem.current;var pointer=new PointerEventData(events){position=screen,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();events.RaycastAll(pointer,hits);
            if(hits.Count==0)
            {
                var camera=Camera.main;var point=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,-camera.transform.position.z));point.z=0;
                ArenaDirector.Instance.TryPlaceSelected(point);return;
            }
            var hit=hits[0].gameObject;
            var press=ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.pointerDownHandler);
            if(press==null)press=ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit);
            if(press==null)return;
            ExecuteEvents.Execute(press,pointer,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(press,pointer,ExecuteEvents.pointerClickHandler);
        }
        private static Button Next()=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name=="SHOW ME");
        private static IEnumerator Validate()
        {
            var game=GameDirector.Instance;var coach=TutorialManager.Instance;var arena=ArenaDirector.Instance;
            if(!Check(game.ActiveAllyCount==1 && coach.Step==CoachStep.Queen,"Opening has one leader and the queen lesson"))yield break;
            Directory.CreateDirectory(Folder);ScreenCapture.CaptureScreenshot(Folder+"Coach-01-Queen.png");yield return null;
            Click(Screen((RectTransform)Next().transform));
            float deadline=Time.realtimeSinceStartup+12;
            while(!arena.IsPreparing && Time.realtimeSinceStartup<deadline)yield return null;
            if(!Check(arena.IsPreparing && EnemyUnit.ActiveEnemies.Count(e=>e.IsAlive)==2,"Enemy formation is staged before battle"))yield break;
            Click(Screen((RectTransform)Next().transform));yield return null;
            if(!Check(coach.Step==CoachStep.SelectCard,"Next leads to card-selection action"))yield break;
            ScreenCapture.CaptureScreenshot(Folder+"Coach-02-Select-Card.png");yield return null;
            var cards=Object.FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None);
            var elva=cards.First(c=>c.Definition.heroId=="PrincessElva");var wolvy=cards.First(c=>c.Definition.heroId=="HeroWolvy");
            Click(Screen((RectTransform)wolvy.transform));yield return null;
            if(!Check(arena.SelectedCard==null && game.ActiveAllyCount==1,"Spotlight blocks the wrong card"))yield break;
            Click(Screen((RectTransform)elva.transform));yield return null;
            if(!Check(arena.SelectedCard==elva && coach.Step==CoachStep.PlaceUnit,"Clicking the highlighted card selects without deploying"))yield break;
            ScreenCapture.CaptureScreenshot(Folder+"Coach-03-Place-Unit.png");yield return null;
            int charms=game.MoonCharms;
            Click(Camera.main.WorldToScreenPoint(game.AlphaLeader.Position));yield return null;
            if(!Check(game.ActiveAllyCount==1 && game.MoonCharms==charms,"Wrong grid tile spends nothing"))yield break;
            Click(Camera.main.WorldToScreenPoint(coach.SuggestedCell));yield return null;
            if(!Check(game.ActiveAllyCount==2 && elva.Energy==0 && game.MoonCharms==charms-game.CharmCost("PrincessElva") && coach.Step==CoachStep.EnergySpent,"Target tile deploys one ally and spends charge and charms once"))yield break;
            Click(Screen((RectTransform)Next().transform));yield return null;
            if(!Check(coach.Step==CoachStep.BeginBattle,"Charge lesson requires acknowledgement"))yield break;
            Click(Screen(arena.FightButtonRect));yield return null;
            if(!Check(arena.Phase==ArenaPhase.Battle && coach.Step==CoachStep.Combat,"Begin Battle starts real combat"))yield break;
            yield return new WaitForSecondsRealtime(.8f);
            ScreenCapture.CaptureScreenshot(Folder+"Coach-04-Combat.png");yield return null;
            deadline=Time.realtimeSinceStartup+60;
            while(coach.Step!=CoachStep.Victory && game.CurrentState==GameState.Playing && Time.realtimeSinceStartup<deadline)yield return null;
            if(!Check(coach.Step==CoachStep.Victory && game.TotalKills>=2 && arena.Queen.IsAlive,"Leader plus ranged support survive the first wave"))yield break;
            if(!Check(elva.Energy>0,"Enemy defeats recharge the spent card"))yield break;
            Click(Screen((RectTransform)Next().transform));yield return null;
            if(!Check(PlayerPrefs.GetInt(TutorialManager.CompletedKey)==1,"Completing the interactive lesson persists v2 preference"))yield break;
            Finish("Passed");
        }
    }
}
