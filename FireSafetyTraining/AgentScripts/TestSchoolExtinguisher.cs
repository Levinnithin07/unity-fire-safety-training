using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class TestSchoolExtinguisher
{
    public static async Task<string> Run()
    {
        var s=TrainingSession.Instance;var e=s.evacuation.extinguisher;var m=s.simulation;var p=s.player;var v=s.evacuation;
        var checks=new List<object>();var samples=new List<object>();
        var feedback=p.GetComponent<TrainingFeedback>();int cuesBefore=feedback.CompletionCuesPlayed;
        void Check(string name,bool pass,object evidence=null)=>checks.Add(new{name,pass,evidence});
        void Click(string name)=>s.ui.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name==name).onClick.Invoke();
        var oldK=Keyboard.current;var oldM=Mouse.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;Keyboard k=null;Mouse mouse=null;
        Application.runInBackground=true;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        async Task Key(Key key){InputSystem.QueueStateEvent(k,new KeyboardState(key));await Task.Delay(140);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(140);}
        try {
            await Task.Delay(600);k=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();s.MainMenu();Click("FireExtinguisherTraining");
            Check("Menu card selects extinguisher mode",s.SelectedMode==TrainingMode.FireExtinguisher&&s.Page=="Briefing");Click("StartSimulation");await Task.Delay(250);
            Check("Controlled fire starts automatically",m.Running&&m.ControlledTraining&&m.sources[0].Burning);
            Check("No evacuation objective or active navigation",!v.enabled&&!v.Active&&UnityEngine.Object.FindAnyObjectByType<EvacuationUI>().trail.positionCount==0);
            Check("Sprinklers unavailable with no water or audio",m.sprinklers.All(x=>!x.Available&&!x.Active&&!x.water.isEmitting&&!x.waterSound.isPlaying));
            var cc=p.GetComponent<CharacterController>();p.enabled=false;cc.enabled=false;p.transform.position=new Vector3(4.7f,.025f,-1.25f);cc.enabled=true;p.view.LookAt(e.pickupRenderer.bounds.center);Physics.SyncTransforms();p.SetCapture(true);
            await Key(UnityEngine.InputSystem.Key.E);Check("E picks up visible held model",e.Held&&e.heldRoot.activeSelf);
            InputSystem.QueueStateEvent(mouse,new MouseState());await Task.Delay(100);InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));await Task.Delay(200);Check("Pin blocks discharge",!e.Spraying&&e.Remaining==e.capacitySeconds);InputSystem.QueueStateEvent(mouse,new MouseState());await Task.Delay(150);
            await Key(UnityEngine.InputSystem.Key.P);await Task.Delay(500);Check("P removes safety pin",e.PinPulled&&!e.pinObject.activeSelf);
            cc.enabled=false;p.transform.position=new Vector3(0,.025f,-1.8f);cc.enabled=true;p.view.LookAt(m.sources[0].transform.position+Vector3.up*.075f);
            var angles=p.view.eulerAngles;p.transform.rotation=Quaternion.Euler(0,angles.y-3.5f,0);typeof(DesktopPlayerController).GetField("pitch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(p,Mathf.DeltaAngle(0,angles.x));p.view.localRotation=Quaternion.Euler(angles.x,0,0);p.enabled=true;p.SetCapture(true);Physics.SyncTransforms();await Task.Delay(150);
            var fire=m.sources[0];float startIntensity=fire.intensity,startSize=fire.flames.main.startSize.constantMax,startEmission=fire.flames.emission.rateOverTime.constant,startSmoke=fire.smoke.emission.rateOverTime.constant;
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/School-Fire-Before.png");await Task.Delay(150);
            InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));bool mid=false,gradual=false,visual=false;int direction=1;
            for(int i=0;i<110&&fire.Burning;i++){
                if(i>0&&i%10==0)direction=-direction;
                InputSystem.QueueDeltaStateEvent(mouse.delta,new Vector2(direction*.7f/p.mouseSensitivity,0));await Task.Delay(100);
                float size=fire.flames.main.startSize.constantMax,emission=fire.flames.emission.rateOverTime.constant;
                samples.Add(new{m.Elapsed,fire.intensity,size,emission,light=fire.fireLight.intensity,smoke=fire.smoke.emission.rateOverTime.constant,e.CorrectAim,e.Remaining});
                if(fire.intensity>0&&fire.intensity<startIntensity-.02f)gradual=true;
                if(fire.intensity>.05f&&fire.intensity<.25f){visual=size<startSize&&emission<startEmission&&fire.smoke.emission.rateOverTime.constant<startSmoke;if(!mid){ScreenCapture.CaptureScreenshot("Assets/Screenshots/School-Fire-Reduced.png");mid=true;}}
            }
            InputSystem.QueueStateEvent(mouse,new MouseState());
            Check("Fire progressively reduces through intermediate intensities",gradual);
            Check("Flame size/emission and smoke respond to suppression",visual);
            Check("Agent actually reaches and extinguishes fire",fire.ExtinguishedByAgent&&fire.intensity==0&&fire.AgentApplied>0,new{fire.AgentApplied,e.SuppressedAt});
            Check("All PASS steps completed through runtime input",e.PassComplete,new{e.PinPulled,e.AimCompleted,e.SqueezeCompleted,e.SweepCompleted});
            Check("No water contributed to suppression",fire.WaterApplied==0&&m.ActiveSprinklers==0);
            Check("Extinguishment plays one success cue",feedback.CompletionCuesPlayed==cuesBefore+1&&feedback.completion.isPlaying);
            await Task.Delay(2300);
            var r=s.evaluation.Result;Check("Suppression opens extinguisher-only results",s.Page=="Results"&&r!=null&&r.completed&&r.extinguisherMode&&!v.Completed);
            Check("Success cue does not repeat on result screen",feedback.CompletionCuesPlayed==cuesBefore+1);
            Check("Results record accuracy, duration and capacity",r!=null&&r.aimAccuracy>0&&r.effectiveSpraySeconds>0&&r.capacityConsumedPercent>0&&r.extinguishSeconds>0);
            Check("No evacuation metrics on extinguisher result",!s.ui.resultMetrics.text.Contains("EVACUATION")&&!s.ui.resultMetrics.text.Contains("Hazard exposure"));
            Check("Flame emission and fire light stop",!fire.flames.isEmitting&&!fire.fireLight.enabled);
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/School-Fire-Results.png");await Task.Delay(200);
            Click("RetryTraining");await Task.Delay(150);Check("Retry resets PASS capacity and starts a fresh fire",s.Playing&&m.sources[0].intensity==.45f&&!e.Held&&e.Remaining==e.capacitySeconds&&!e.PinPulled&&s.evaluation.Result==null);
            s.Pause();Click("MainMenu");Check("Back to main menu clears attempt",s.Page=="Main"&&!m.Running&&!e.Held&&s.evaluation.Result==null);
        } finally {
            if(k!=null)InputSystem.RemoveDevice(k);if(mouse!=null)InputSystem.RemoveDevice(mouse);if(oldK!=null&&oldK.added)oldK.MakeCurrent();if(oldM!=null&&oldM.added)oldM.MakeCurrent();p.enabled=true;
            InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Connected Editor Play Mode; menu button events plus virtual E/P/LMB/mouse delta through runtime Update; equipment positioning fixture",checks,samples},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/SchoolModeA.json"),json);return json;
    }
}


