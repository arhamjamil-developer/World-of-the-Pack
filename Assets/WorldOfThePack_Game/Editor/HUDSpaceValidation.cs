using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WorldOfThePack.NewUI;
using WorldOfThePack.UI;

namespace WorldOfThePack.Game.Editor
{
    public static class HUDSpaceValidation
    {
        public static string Result { get; private set; }="Not run";
        private static readonly List<string> Checks=new List<string>();
        private const string Folder="Assets/Visuals/RoyalEscort/Documentation/";
        public static string Run()
        {
            if(!EditorApplication.isPlaying)return "Enter Play Mode first (tutorial completed for this check).";
            Checks.Clear();Result="Running";GameDirector.Instance.StartCoroutine(Validate());return Result;
        }
        private static bool Check(bool condition,string message)
        {Checks.Add((condition?"PASS: ":"FAIL: ")+message);if(!condition)Finish("Failed");return condition;}
        private static void Finish(string state){Result=state;File.WriteAllText(Folder+"HUD-Space-Checks.txt",state+"\n"+string.Join("\n",Checks));}
        private static IEnumerator Validate()
        {
            var hud=PackGameplayHUD.Instance;var view=hud.GetComponent<GameplayHUDVisibility>();var safe=hud.transform.Find("SafeArea");
            var controls=safe.Find("HUDVisibilityControls");var top=safe.Find("TopHUD").GetComponent<CanvasGroup>();var cards=safe.Find("CardHand").GetComponent<CanvasGroup>();
            view.ShowAll();yield return new WaitForSecondsRealtime(.4f);
            var originals=Object.FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None).ToDictionary(c=>c.name,c=>c.Definition);
            if(!Check(top!=null && cards!=null && top.alpha>.99f && cards.alpha>.99f,"Both panels initialize with valid visible groups"))yield break;
            controls.Find("Upper HUD handle").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);
            if(!Check(!view.UpperVisible && top.alpha<.01f && !top.blocksRaycasts && cards.alpha>.99f,"Upper handle hides only the upper HUD"))yield break;
            if(!Check(controls.Find("Upper HUD handle").GetComponentInChildren<TMPro.TMP_Text>().text.Contains("Show"),"Hidden upper panel has a readable Show HUD label"))yield break;
            controls.Find("Always available pause").GetComponent<Button>().onClick.Invoke();yield return null;
            if(!Check(GameDirector.Instance.CurrentState==GameState.Paused,"Pause remains available while upper HUD is hidden"))yield break;
            hud.resumeButton.onClick.Invoke();yield return null;
            if(!Check(GameDirector.Instance.CurrentState==GameState.Playing,"Resume works with hidden upper HUD"))yield break;
            controls.Find("Upper HUD handle").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);
            if(!Check(view.UpperVisible && top.alpha>.99f,"Upper handle restores the HUD"))yield break;
            var arena=ArenaDirector.Instance;float deadline=Time.realtimeSinceStartup+10;
            while(!arena.IsPreparing && Time.realtimeSinceStartup<deadline)yield return null;
            arena.SelectCard(originals.Keys.Select(name=>Object.FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None).First(c=>c.name==name)).First(c=>c.IsDeployReady));
            controls.Find("Lower card handle").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);
            var lower=(RectTransform)controls.Find("Lower card handle");
            if(!Check(!view.CardsVisible && cards.alpha<.01f && !cards.blocksRaycasts && arena.SelectedCard==null,"Hiding cards clears selection and disables hidden input"))yield break;
            if(!Check(Mathf.Abs(lower.anchoredPosition.y-24)<1 && lower.GetComponentInChildren<TMPro.TMP_Text>().text.Contains("Show"),"Collapsed lower handle stays at the screen edge with Show cards"))yield break;
            view.ToggleUpper();yield return new WaitForSecondsRealtime(.4f);
            ScreenCapture.CaptureScreenshot(Folder+"HUD-Both-Hidden-v3.png");yield return null;
            controls.Find("Lower card handle").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);
            if(!Check(view.CardsVisible && cards.alpha>.99f && !view.UpperVisible,"Cards can be restored independently"))yield break;
            ScreenCapture.CaptureScreenshot(Folder+"HUD-Upper-Hidden-v3.png");yield return null;
            view.ShowAll();yield return new WaitForSecondsRealtime(.4f);
            if(!Check(originals.All(pair=>Object.FindObjectsByType<HeroDeploymentCard>(FindObjectsSortMode.None).First(c=>c.name==pair.Key).Definition==pair.Value),"Card Inspector definitions remain unchanged"))yield break;
            if(!Check(!Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None).Any(t=>t.isTextOverflowing),"Visible text fits the restored full view"))yield break;
            ScreenCapture.CaptureScreenshot(Folder+"HUD-Full-v3.png");yield return null;
            var drawer=Object.FindFirstObjectByType<WorldOfThePack.Expedition.PackTaskDrawer>();
            drawer.Open();yield return new WaitForSecondsRealtime(.35f);
            ScreenCapture.CaptureScreenshot(Folder+"HUD-Compact-Task-v3.png");yield return null;drawer.Close();
            Finish("Passed");
        }
    }
}
