using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class TestPhaseSixWalk
{
    public static Task<string> Evacuate()=>Run(false);
    public static Task<string> AfterPASS()=>Run(true);
    static async Task<string> Run(bool afterPASS)
    {
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var v=s.evacuation;var p=s.player;var m=s.simulation;var oldK=Keyboard.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;Keyboard k=null;string json="";
        Application.runInBackground=true;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        try{
            k=InputSystem.AddDevice<Keyboard>();if(!afterPASS){s.StartSimulation();m.StartScenario();InputSystem.QueueStateEvent(k,new KeyboardState(Key.V));await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(150);}p.SetCapture(true);
            double start=Time.realtimeSinceStartupAsDouble;
            while(!v.Completed&&Time.realtimeSinceStartupAsDouble-start<35){
                v.Replan();if(!v.navigation.Available)break;Vector3 delta=v.navigation.NextPoint(p.transform.position)-p.transform.position;delta.y=0;
                if(delta.sqrMagnitude>.01f){p.transform.rotation=Quaternion.LookRotation(delta);p.view.localRotation=Quaternion.identity;typeof(DesktopPlayerController).GetField("pitch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(p,0f);}
                InputSystem.QueueStateEvent(k,v.exitDoor.CanInteract?new KeyboardState(Key.W,Key.E):new KeyboardState(Key.W));await Task.Delay(35);
            }
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(1800);var r=s.evaluation.Result;
            bool pass=v.Completed&&s.Page=="Results"&&r!=null&&r.completed&&r.tasks==100&&r.handlingAssessed==afterPASS&&r.events.Any(x=>x.action=="Assembly reached")&&(!afterPASS||r.suppressed);
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/Phase6-Results-"+(afterPASS?"PASS":"Evacuation")+".png");await Task.Delay(150);
            json=Newtonsoft.Json.JsonConvert.SerializeObject(new{generatedUtc=DateTime.UtcNow,pass,scope="Actual Update movement with virtual W/E/V and automatic yaw steering; no teleport during evacuation",page=s.Page,elapsed=v.Elapsed,result=r,save=s.evaluation.SaveStatus},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase6Workflow-"+(afterPASS?"PASS":"Evacuation")+".json"),json);
        }finally{if(k!=null)InputSystem.RemoveDevice(k);if(oldK!=null&&oldK.added)oldK.MakeCurrent();InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;}
        return json;
    }
}
