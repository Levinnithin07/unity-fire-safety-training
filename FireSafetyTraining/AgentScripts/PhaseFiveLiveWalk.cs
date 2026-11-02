using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class PhaseFiveLiveWalk
{
    public static Task<string> Front()=>Run(false);
    public static Task<string> Rear()=>Run(true);
    static async Task<string> Run(bool rear)
    {
        var v=UnityEngine.Object.FindAnyObjectByType<EvacuationManager>();var m=v.simulation;var p=v.player;var oldK=Keyboard.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;Keyboard k=null;
        Application.runInBackground=true;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        double start=0;int frame=0;string json="";bool doorCapture=false;
        try{
            k=InputSystem.AddDevice<Keyboard>();v.enabled=true;m.enabled=false;m.ResetScenario();p.ResetPose(v.spawn);p.enabled=true;p.SetCapture(true);m.StartScenario();
            if(rear)for(int i=0;i<600;i++)m.StepSimulation(.05f);m.enabled=true;
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.V));await Task.Delay(130);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(130);
            start=Time.realtimeSinceStartupAsDouble;frame=Time.frameCount;
            while(!v.Completed&&Time.realtimeSinceStartupAsDouble-start<25){
                v.Replan();if(!v.navigation.Available)break;
                Vector3 delta=v.navigation.NextPoint(p.transform.position)-p.transform.position;delta.y=0;
                if(delta.sqrMagnitude>.01f)p.transform.rotation=Quaternion.LookRotation(delta);
                InputSystem.QueueStateEvent(k,v.exitDoor.CanInteract?new KeyboardState(Key.W,Key.E):new KeyboardState(Key.W));
                if(!doorCapture&&p.transform.position.x>5.2f){ScreenCapture.CaptureScreenshot("Assets/Screenshots/Phase5-Exit-"+(rear?"Rear":"Front")+".png");doorCapture=true;}
                await Task.Delay(35);
            }
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(150);p.SetCapture(false);
            var samples=new float[1024];double audio=0;for(int i=0;i<10;i++){v.arrivalSound.GetOutputData(samples,0);audio=Math.Max(audio,Math.Sqrt(samples.Average(x=>(double)x*x)));await Task.Delay(30);}
            p.transform.rotation=Quaternion.Euler(0,90,0);p.view.localRotation=Quaternion.identity;await Task.Delay(100);ScreenCapture.CaptureScreenshot("Assets/Screenshots/Phase5-Assembly-"+(rear?"Rear":"Front")+".png");await Task.Delay(150);
            json=Newtonsoft.Json.JsonConvert.SerializeObject(new{generatedUtc=DateTime.UtcNow,route=rear?"Rear (front smoke hazard)":"Front",scope="Real-time Play Mode, virtual W/V/E bindings; automated yaw steering follows NavMesh corners",passed=v.Completed&&v.CrossedExit,elapsed=v.Elapsed,wallSeconds=Time.realtimeSinceStartupAsDouble-start,frames=Time.frameCount-frame,position=p.transform.position.ToString("F3"),v.Reason,v.Reroutes,doorOpen=v.exitDoor.IsPassable,completionAudioRms=audio},Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.GetFullPath("../Documentation/Phase5Live"+(rear?"Rear":"Front")+".json"),json);
        }finally{if(k!=null)InputSystem.RemoveDevice(k);if(oldK!=null&&oldK.added)oldK.MakeCurrent();InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;p.SetCapture(false);}
        return json;
    }
}
