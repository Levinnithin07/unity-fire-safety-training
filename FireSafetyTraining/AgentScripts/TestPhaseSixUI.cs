using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class TestPhaseSixUI
{
    static List<object> checks;
    static void Check(string name,bool pass,object evidence=null)=>checks.Add(new{name,pass,evidence});
    static void Click(TrainingInterface ui,string id){var b=ui.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name==id);if(!b.gameObject.activeInHierarchy||!b.interactable)throw new Exception("Button unavailable: "+id);ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
    public static async Task<string> Run()
    {
        checks=new List<object>();var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var ui=s.ui;var set=s.settings;var m=s.simulation;
        var oldK=Keyboard.current;var oldMouse=Mouse.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;var oldBg=Application.runInBackground;Keyboard k=null;Mouse mouse=null;var original=set.Current;
        try{
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
            k=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();s.MainMenu();await Task.Delay(200);
            Check("Main menu freezes simulation and unlocks cursor",s.Page=="Main"&&!s.Playing&&Time.timeScale==0&&!s.player.Captured&&!m.Running);
            Check("One EventSystem and menu GraphicRaycaster",UnityEngine.Object.FindObjectsByType<EventSystem>().Length==1&&ui.GetComponent<UnityEngine.UI.GraphicRaycaster>()!=null);
            // Real mouse input exercises the UI module and graphics raycast on Start Training.
            var button=ui.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="StartTraining");var rect=(RectTransform)button.transform;var pos=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=pos});await Task.Delay(150);InputSystem.QueueStateEvent(mouse,new MouseState{position=pos}.WithButton(MouseButton.Left));await Task.Delay(150);InputSystem.QueueStateEvent(mouse,new MouseState{position=pos});await Task.Delay(150);
            Check("Real mouse click opens briefing",s.Page=="Briefing",s.Page);
            if(s.Page!="Briefing")Click(ui,"StartTraining");Click(ui,"Mode");Check("Practice focus changes",s.Mode=="Evacuation practice");Click(ui,"BriefingBack");
            foreach(string page in new[]{"Instructions","Controls","Credits"}){Click(ui,page);Check(page+" button opens page",s.Page==page);Click(ui,page+"Back");Check(page+" back returns to menu",s.Page=="Main");}
            Click(ui,"Settings");Check("Settings opens",s.Page=="Settings");var before=set.Draft;Click(ui,"DisplayMode");Click(ui,"Resolution");Click(ui,"Quality");ui.volumeSlider.value=.36f;ui.sensitivitySlider.value=.12f;
            Check("Settings controls update draft",set.Draft.fullscreen!=before.fullscreen&&set.Draft.quality!=before.quality&&Mathf.Abs(set.Draft.volume-.36f)<.01f&&Mathf.Abs(set.Draft.sensitivity-.12f)<.01f);
            Click(ui,"Apply");Check("Apply starts confirmation and applies graphics",set.AwaitingConfirmation&&QualitySettings.renderPipeline==set.pipelines[set.Draft.quality]);Click(ui,"Keep");Check("Keep persists selected settings",!set.AwaitingConfirmation&&PlayerPrefs.GetInt("Training.Quality")==set.Current.quality&&Mathf.Abs(PlayerPrefs.GetFloat("Training.Volume")-.36f)<.01f);
            var kept=set.Current;Click(ui,"CancelSettings");Check("Settings back retains confirmed values",s.Page=="Main"&&set.Current.quality==kept.quality&&Mathf.Approximately(set.Current.volume,kept.volume));Click(ui,"Settings");var saved=set.Current;Click(ui,"Quality");Click(ui,"Apply");await Task.Delay(15700);
            Check("15 second unscaled display timeout reverts while menu paused",!set.AwaitingConfirmation&&set.Current.quality==saved.quality&&Time.timeScale==0,set.Status);
            Click(ui,"CancelSettings");Click(ui,"StartTraining");Click(ui,"StartSimulation");await Task.Delay(120);
            Check("Start simulation enables movement and HUD",s.Playing&&Time.timeScale==1&&s.gameplayCanvas.enabled&&s.player.Captured&&!ui.canvas.enabled);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.Enter));await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(250);
            Check("Enter starts fire in gameplay",m.Running&&m.Elapsed>0);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.Escape));await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(100);float time=m.Elapsed,agent=s.evacuation.extinguisher.Remaining;Vector3 position=s.player.transform.position;
            EventSystem.current.SetSelectedGameObject(null);InputSystem.QueueStateEvent(k,new KeyboardState(Key.W,Key.Enter,Key.R,Key.V));await Task.Delay(650);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(100);
            Check("Escape pauses and blocks gameplay bindings",s.Page=="Pause"&&Mathf.Approximately(m.Elapsed,time)&&Vector3.Distance(position,s.player.transform.position)<.001f&&Mathf.Approximately(s.evacuation.extinguisher.Remaining,agent),new{time,after=m.Elapsed});
            Click(ui,"PauseControls");Click(ui,"ControlsBack");Check("Pause controls returns to pause",s.Page=="Pause");Click(ui,"PauseSettings");Click(ui,"CancelSettings");Check("Pause settings returns to pause",s.Page=="Pause");
            Click(ui,"Resume");await Task.Delay(250);Check("Resume restores advancing simulation",s.Playing&&m.Elapsed>time&&!AudioListener.pause);
            s.Pause();Click(ui,"Restart");await Task.Delay(100);Check("Restart resets emergency and telemetry",s.Playing&&!m.Running&&s.evaluation.Events.Count==0);
            s.Pause();Click(ui,"MainMenu");Check("Return to main menu clears attempt",!s.HasSession&&!m.Running&&s.Page=="Main");
            // Capture fixtures for result layout only; evaluation assertions use recorded component state.
            s.StartSimulation();s.evaluation.Finish(s.Mode);s.Show("Results");Check("Incomplete attempt does not show completion",!s.evaluation.Result.completed&&s.evaluation.Result.tasks==0&&s.evaluation.Result.overall<100);
            Click(ui,"RetryTraining");Check("Retry button clears previous report",s.Playing&&s.evaluation.Result==null);s.ShowResults();Click(ui,"ResultsMenu");Check("Results menu button returns to main",s.Page=="Main");
            set.BeginEdit();set.Draft=original;set.Preview();set.Keep();s.MainMenu();
        }finally{if(k!=null)InputSystem.RemoveDevice(k);if(mouse!=null)InputSystem.RemoveDevice(mouse);if(oldK!=null&&oldK.added)oldK.MakeCurrent();if(oldMouse!=null&&oldMouse.added)oldMouse.MakeCurrent();InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=oldBg;}
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{generatedUtc=DateTime.UtcNow,checks},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase6UITests.json"),json);return json;
    }
    public static async Task<string> Layout()
    {
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var rows=new List<object>();bool pass=true;
        foreach(var page in s.ui.pages){s.Show(page.name);await Task.Delay(160);Canvas.ForceUpdateCanvases();foreach(var text in s.ui.GetComponentsInChildren<TMPro.TextMeshProUGUI>()){
            text.ForceMeshUpdate();var corners=new Vector3[4];text.rectTransform.GetWorldCorners(corners);bool inside=corners.All(p=>p.x>=0&&p.y>=0&&p.x<=Screen.width+.1f&&p.y<=Screen.height+.1f);bool ok=inside&&!text.isTextOverflowing;pass&=ok;rows.Add(new{page=page.name,name=text.name,pass=ok,overflow=text.isTextOverflowing,inside});
        }}s.MainMenu();string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{width=Screen.width,height=Screen.height,pass,rows},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase6Layout-"+Screen.width+".json"),json);return json;
    }
}

