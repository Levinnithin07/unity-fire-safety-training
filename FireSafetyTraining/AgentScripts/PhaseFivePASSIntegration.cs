using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class PhaseFivePASSIntegration
{
    public static async Task<string> Run()
    {
        var e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();var m=e.simulation;var p=e.player;
        var oldKeyboard=Keyboard.current;var oldMouse=Mouse.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;
        Keyboard k=null;Mouse mouse=null;var points=new List<object>();string result="";
        Application.runInBackground=true;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        try {
            k=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();m.enabled=true;e.enabled=true;m.ResetScenario();p.enabled=false;p.SetCapture(true);
            var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(4.7f,.025f,-1.25f);cc.enabled=true;p.view.LookAt(e.pickupRenderer.bounds.center);Physics.SyncTransforms();
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.E));await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(150);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.P));await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(550);
            cc.enabled=false;p.transform.position=new Vector3(0,.025f,-1.8f);cc.enabled=true;p.view.LookAt(m.sources[0].transform.position+Vector3.up*.075f);
            var angles=p.view.eulerAngles;p.transform.rotation=Quaternion.Euler(0,angles.y-3.5f,0);typeof(DesktopPlayerController).GetField("pitch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(p,Mathf.DeltaAngle(0,angles.x));p.view.localRotation=Quaternion.Euler(angles.x,0,0);p.enabled=true;p.SetCapture(true);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.Enter));await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(150);
            InputSystem.QueueStateEvent(mouse,new MouseState());await Task.Delay(100);InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));
            int startFrame=Time.frameCount;double start=Time.realtimeSinceStartupAsDouble;int direction=1;
            for(int i=0;i<100&&m.sources[0].Burning;i++){
                if(i>0&&i%10==0)direction=-direction;
                InputSystem.QueueDeltaStateEvent(mouse.delta,new Vector2(direction*.7f/p.mouseSensitivity,0));await Task.Delay(100);
                points.Add(new{time=Time.realtimeSinceStartupAsDouble-start,intensity=m.sources[0].intensity,yaw=p.transform.eulerAngles.y,e.CorrectAim,e.SweepSpeed,e.Remaining,e.SweepCompleted});
            }
            InputSystem.QueueStateEvent(mouse,new MouseState());await Task.Delay(200);
            result=Newtonsoft.Json.JsonConvert.SerializeObject(new{generatedUtc=DateTime.UtcNow,pass=e.PassComplete&&m.sources[0].ExtinguishedByAgent&&e.evacuation.Active,evacuationStarted=e.evacuation.Active,evacuationReason=e.evacuation.Reason,input="Virtual E, P, Enter, LMB and mouse delta through actual Update bindings",e.Held,e.PinPulled,e.AimCompleted,e.SqueezeCompleted,e.SweepCompleted,e.SuppressionCompleted,e.Accuracy,e.Remaining,seconds=Time.realtimeSinceStartupAsDouble-start,frames=Time.frameCount-startFrame,points},Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.GetFullPath("../Documentation/Phase5PASSIntegration.json"),result);
        } finally {
            if(k!=null)InputSystem.RemoveDevice(k);if(mouse!=null)InputSystem.RemoveDevice(mouse);if(oldKeyboard!=null&&oldKeyboard.added)oldKeyboard.MakeCurrent();if(oldMouse!=null&&oldMouse.added)oldMouse.MakeCurrent();
            InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;p.SetCapture(false);
        }
        return result;
    }
}

