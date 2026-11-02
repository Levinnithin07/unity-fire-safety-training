using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class StandaloneEvacuationChecks
{
    public static async Task<string> Run()
    {
        var s=TrainingSession.Instance;var m=s.simulation;var v=s.evacuation;var p=s.player;var e=v.extinguisher;
        var checks=new List<object>();void Check(string name,bool pass,object evidence=null)=>checks.Add(new{name,pass,evidence});
        void Click(string name)=>s.ui.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name==name).onClick.Invoke();
        var oldK=Keyboard.current;var oldFocus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;Keyboard k=null;
        Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        try {
            await Task.Delay(500);k=InputSystem.AddDevice<Keyboard>();s.MainMenu();Click("EvacuationTraining");Check("Menu selects independent evacuation mode",s.SelectedMode==TrainingMode.Evacuation&&s.Page=="Briefing");Click("StartSimulation");await Task.Delay(700);
            Check("Emergency and evacuation begin without equipment",m.Running&&!m.ControlledTraining&&v.Active&&!e.Held);
            Check("Extinguisher interactions and PASS HUD disabled",!e.TrainingEnabled&&!e.enabled&&!UnityEngine.Object.FindAnyObjectByType<ExtinguisherStatusUI>().techniquePanel.activeSelf&&!e.TryPickup(new Ray(p.view.position,e.pickupRenderer.bounds.center-p.view.position)));
            Check("Fire and smoke particles visible",m.sources[0].flames.particleCount>0&&m.sources[0].smoke.isEmitting);
            Check("Sprinklers are available after extinguisher mode",m.sprinklers.All(x=>x.Available&&x.enabled));
            Check("Initial route is available",v.navigation.Available&&v.navigation.Corners.Length>=2);
            m.enabled=false;
            for(int i=0;i<1400&&m.ActiveSprinklers==0;i++)m.StepSimulation(.05f);
            float activation=m.sprinklers.Where(x=>x.Active).Select(x=>x.ActivatedAt).DefaultIfEmpty(-1).Min();
            Check("Smoke-driven alarm precedes heat-driven sprinkler",m.alarm.Active&&m.alarm.ActivatedAt>0&&activation>m.alarm.ActivatedAt,new{m.alarm.ActivatedAt,sprinklerAt=activation,m.SmokeLevel});
            float before=m.sources[0].intensity,waterBefore=m.sources[0].WaterApplied;
            for(int i=0;i<80;i++)m.StepSimulation(.05f);
            Check("Water contributes to reducing nearby fire",m.sources[0].WaterApplied>waterBefore&&m.sources[0].intensity<before,new{before,after=m.sources[0].intensity,m.sources[0].WaterApplied});
            m.enabled=true;await Task.Delay(800);
            Check("Active sprinkler has water particles and audio",m.sprinklers.Any(x=>x.Active&&x.water.particleCount>0&&x.waterSound.isPlaying));
            v.Replan();Check("Guidance remains traversable with emergency systems active",v.navigation.Available,new{v.navigation.RouteName,v.navigation.Distance});
            ScreenCapture.CaptureScreenshot(FireSafety.StandaloneVerification.OutputDirectory+"/Evacuation-Water.png");await Task.Delay(150);
            Check("Completion requires reaching assembly",!v.Completed&&s.evaluation.Result==null);
            p.SetCapture(true);double start=Time.realtimeSinceStartupAsDouble;
            while(!v.Completed&&Time.realtimeSinceStartupAsDouble-start<35){
                v.Replan();if(!v.navigation.Available)break;Vector3 delta=v.navigation.NextPoint(p.transform.position)-p.transform.position;delta.y=0;
                if(delta.sqrMagnitude>.01f){p.transform.rotation=Quaternion.LookRotation(delta);p.view.localRotation=Quaternion.identity;typeof(DesktopPlayerController).GetField("pitch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(p,0f);}
                InputSystem.QueueStateEvent(k,v.exitDoor.CanInteract?new KeyboardState(Key.W,Key.E):new KeyboardState(Key.W));await Task.Delay(35);
            }
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(2300);var r=s.evaluation.Result;
            Check("Walk through working exit to assembly completes mode",v.Completed&&v.CrossedExit&&v.exitDoor.IsPassable,new{position=p.transform.position.ToString(),v.Elapsed});
            Check("Evacuation results exclude extinguisher scoring",s.Page=="Results"&&r!=null&&r.completed&&!r.extinguisherMode&&!r.handlingAssessed&&r.handling==0&&!s.ui.resultMetrics.text.Contains("PASS")&&!s.ui.resultMetrics.text.Contains("accuracy"));
            Check("No extinguisher use required",!e.Held&&!e.PinPulled&&e.SpraySeconds==0&&m.sources.All(x=>x.AgentApplied==0));
            float elapsed=v.Elapsed;await Task.Delay(200);Check("Exit timer stops at completion",v.Elapsed==elapsed);
            ScreenCapture.CaptureScreenshot(FireSafety.StandaloneVerification.OutputDirectory+"/Evacuation-Results.png");await Task.Delay(150);
        } finally {
            if(k!=null)InputSystem.RemoveDevice(k);if(oldK!=null&&oldK.added)oldK.MakeCurrent();m.enabled=true;
            InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Menu button events, deterministic 50ms emergency simulation to exercise heat/smoke thresholds, then real Update movement with virtual W/E and automatic yaw; no teleport during evacuation",checks,result=s.evaluation.Result},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.Combine(FireSafety.StandaloneVerification.OutputDirectory,"Evacuation.json"),json);return json;
    }
}



