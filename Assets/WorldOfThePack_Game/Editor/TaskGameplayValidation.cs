using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.Expedition;
using WorldOfThePack.NewUI;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game.Editor
{
    public static class TaskGameplayValidation
    {
        public static string Result { get; private set; }="Not run";
        private static readonly List<string> Checks=new List<string>();
        private const string Folder="Assets/Visuals/RoyalEscort/Documentation/";
        public static string Run()
        {
            if(!EditorApplication.isPlaying)return "Enter Play Mode with the tutorial completed for this regression check.";
            Checks.Clear();Result="Running";GameDirector.Instance.StartCoroutine(Validate());return Result;
        }
        private static bool Check(bool condition,string message)
        {Checks.Add((condition?"PASS: ":"FAIL: ")+message);if(!condition)Finish("Failed");return condition;}
        private static void Finish(string state)
        {Result=state;File.WriteAllText(Folder+"Task-Gameplay-Checks.txt",state+"\n"+string.Join("\n",Checks));}
        private static HeroDeploymentCard Card(string id)=>Object.FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None).First(c=>c.Definition.heroId==id);
        private static bool Recruit(string id)
        {
            var arena=ArenaDirector.Instance;arena.SelectCard(Card(id));
            for(int rank=0;rank<4;rank++)for(int lane=0;lane<5;lane++)
            { int row=id=="CaptainBobo"?3-rank:rank;if(arena.TryPlaceSelected(arena.CellPosition(lane,row)))return true; }
            return false;
        }
        private static IEnumerator Validate()
        {
            var game=GameDirector.Instance;var arena=ArenaDirector.Instance;var hud=PackGameplayHUD.Instance;
            var tasks=Object.FindFirstObjectByType<PackTaskDirector>();var drawer=Object.FindFirstObjectByType<PackTaskDrawer>();
            game.RestartGame();drawer.Close();yield return null;yield return null;
            if(!Check(game.ActiveAllyCount==1 && !drawer.IsOpen,"Restart has one leader and a collapsed task handle"))yield break;
            var handle=hud.transform.Find("SafeArea/TaskSidebar/Quiet left task handle").GetComponent<Button>();
            handle.onClick.Invoke();yield return new WaitForSecondsRealtime(.25f);
            if(!Check(drawer.IsOpen && !drawer.HasOverflow,"Task handle opens readable task artwork"))yield break;
            var field=typeof(PackTaskDirector).GetField("_index",BindingFlags.Instance|BindingFlags.NonPublic);
            for(int i=0;i<tasks.catalog.tasks.Length;i++)
            {
                field.SetValue(tasks,i);yield return null;yield return null;
                if(!Check(!drawer.HasOverflow,"Task text fits: "+tasks.CurrentTask.title))yield break;
            }
            game.RestartGame();drawer.Close();
            float deadline=Time.realtimeSinceStartup+10;
            while(!arena.IsPreparing && Time.realtimeSinceStartup<deadline)yield return null;
            if(!Check(arena.IsPreparing && Recruit("PrincessElva"),"First-wave ranged recruit succeeds"))yield break;
            arena.BeginBattle();deadline=Time.realtimeSinceStartup+45;
            while(WaveManager.Instance.CurrentWaveIndex<1 && game.CurrentState==GameState.Playing && Time.realtimeSinceStartup<deadline)yield return null;
            if(!Check(game.TotalKills==2 && arena.Queen.IsAlive && tasks.CompletedTasks==0,"Wave one survives; incomplete three-kill task grants no reward"))yield break;
            deadline=Time.realtimeSinceStartup+12;
            while(!arena.IsPreparing && Time.realtimeSinceStartup<deadline)yield return null;
            int charms=game.MoonCharms;arena.SelectCard(Card("PrincessElva"));
            if(!Check(arena.SelectedCard==null && game.MoonCharms==charms,"A partly charged card cannot be selected or spent"))yield break;
            var lulu=Card("FairyLulu");arena.SelectCard(lulu);
            bool occupied=arena.TryPlaceSelected(game.AlphaLeader.Position);
            if(!Check(!occupied && game.MoonCharms==charms && lulu.Energy==100,"Occupied-cell rejection retains charge and charms"))yield break;
            if(!Check(Recruit("FairyLulu") && Recruit("CaptainBobo"),"Wave-two support and tank recruitment succeed"))yield break;
            arena.BeginBattle();deadline=Time.realtimeSinceStartup+45;
            while(WaveManager.Instance.CurrentWaveIndex<2 && game.CurrentState==GameState.Playing && Time.realtimeSinceStartup<deadline)yield return null;
            if(!Check(game.TotalKills>=4 && arena.Queen.IsAlive && tasks.CompletedTasks==1 && tasks.RewardsGranted==3,"Mixed sword-and-bow wave clears; kill task pays exactly once"))yield break;
            deadline=Time.realtimeSinceStartup+12;
            while(!arena.IsPreparing && Time.realtimeSinceStartup<deadline)yield return null;
            if(!Check(tasks.CurrentTask.kind==PackTaskKind.Deploy && Recruit("TinkerTilly"),"Task advances to recruiting and accepts a new ally"))yield break;
            yield return null;
            if(!Check(tasks.CompletedTasks==2 && tasks.RewardsGranted==6,"Recruitment task pays once and records the correct total"))yield break;
            drawer.Open();game.TogglePause();var position=game.AlphaLeader.Position;
            yield return new WaitForSecondsRealtime(.3f);
            if(!Check(game.CurrentState==GameState.Paused && game.AlphaLeader.Position==position && !drawer.IsOpen,"Pause freezes gameplay and closes the task drawer"))yield break;
            game.TogglePause();yield return null;
            game.AddMorale(100);hud.ActivateMoonshift();float shared=game.WolfInstinctTimeRemaining,hero=game.AlphaLeader.WolfSurgeRemaining,cooldown=hud.MoonCooldown;
            yield return new WaitForSecondsRealtime(.5f);
            if(!Check(game.IsWolfInstinctActive && Mathf.Abs(game.WolfInstinctTimeRemaining-shared)<.01f && Mathf.Abs(game.AlphaLeader.WolfSurgeRemaining-hero)<.01f && Mathf.Abs(hud.MoonCooldown-cooldown)<.01f,"Deployment freezes shared, hero and HUD cooldown timers together"))yield break;
            WaveManager.Instance.JumpToWave(9);deadline=Time.realtimeSinceStartup+12;
            while((WaveManager.Instance.CurrentWaveIndex!=9 || !arena.IsPreparing) && Time.realtimeSinceStartup<deadline)yield return null;
            // JumpToWave keeps the previous phase during its rest; wait for actual staged enemies.
            while(EnemyUnit.ActiveEnemies.Count(e=>e.IsAlive)<9 && Time.realtimeSinceStartup<deadline)yield return null;
            var enemies=EnemyUnit.ActiveEnemies.Where(e=>e.IsAlive).ToArray();
            if(!Check(enemies.Length==9 && enemies.Select(e=>e.Position.ToString()).Distinct().Count()==9 && Object.FindObjectsByType<BossUnit>(FindObjectsSortMode.None).Length==1,"Wave ten stages nine unique enemies plus a boss"))yield break;
            drawer.Open();yield return new WaitForSecondsRealtime(.25f);
            ScreenCapture.CaptureScreenshot(Folder+"Task-Drawer-Mixed-Wave.png");yield return null;
            arena.Queen.TakeDamage(10000);yield return null;
            if(!Check(game.CurrentState==GameState.GameOver && !drawer.IsOpen && hud.hearts.All(h=>h.sprite==hud.heartEmpty),"Queen death shows defeat, empty hearts and no task overlay"))yield break;
            game.RestartGame();yield return null;yield return null;
            if(!Check(game.CurrentState==GameState.Playing && game.ActiveAllyCount==1 && arena.Queen.CurrentHealth==240 && game.MoonCharms==24 && tasks.CompletedTasks==0,"Retry resets the queen, one-leader pack, currency and tasks"))yield break;
            Finish("Passed");
        }
    }
}
