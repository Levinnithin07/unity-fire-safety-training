using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FireSafety;
public static class TestPhaseSevenSwitching
{
    public static async Task<string> Run()
    {
        bool bg=Application.runInBackground;Application.runInBackground=true;await Task.Delay(500);
        var s=TrainingSession.Instance;var m=s.simulation;var v=s.evacuation;var e=v.extinguisher;var p=s.player;var cc=p.GetComponent<CharacterController>();var checks=new List<object>();
        void Check(string name,bool pass)=>checks.Add(new{name,pass});
        void Warp(Vector3 pos){cc.enabled=false;p.transform.SetPositionAndRotation(pos,Quaternion.identity);cc.enabled=true;Physics.SyncTransforms();}
        void Equip(){p.enabled=false;Warp(new Vector3(4.7f,.025f,-1.25f));p.view.LookAt(e.pickupRenderer.bounds.center);e.TryPickup(new Ray(p.view.position,p.view.forward));typeof(FireExtinguisherController).GetProperty("PinPulled").SetValue(e,true);e.enabled=false;Warp(new Vector3(0,.025f,-1.8f));}
        Ray Aim(float yaw=0){p.view.rotation=Quaternion.Euler(0,yaw,0)*Quaternion.LookRotation(m.sources[0].transform.position+Vector3.up*.075f-p.view.position);Physics.SyncTransforms();return new Ray(p.view.position,p.view.forward);}
        try {
            s.MainMenu();s.ChooseMode(TrainingMode.FireExtinguisher);s.StartSimulation();m.enabled=false;
            for(int i=0;i<2400;i++)m.StepSimulation(.05f);
            Check("Controlled fire remains manageable after two minutes without spraying",m.sources[0].intensity==.45f&&m.BurningCount==1&&e.SafeToAttempt&&!e.SuppressionCompleted&&!v.Active);
            foreach(var sprinkler in m.sprinklers)for(int i=0;i<200;i++)sprinkler.Step(1000,.05f,m.Elapsed);
            Check("Disabled sprinklers reject even high-heat activation and suppression",m.sprinklers.All(x=>!x.Active&&!x.water.isEmitting&&!x.waterSound.isPlaying&&x.SuppressionAt(m.sources[0].transform.position)==0)&&m.sources.All(x=>x.WaterApplied==0));
            Equip();var high=new Ray(p.view.position,(m.sources[0].transform.position+Vector3.up*1.2f-p.view.position).normalized);float initial=m.sources[0].intensity;
            for(int i=0;i<40;i++)e.StepOperation(high,true,p.view.eulerAngles.y,.05f);
            Check("Wrong aim consumes agent without changing fire",e.Remaining<e.capacitySeconds&&m.sources[0].intensity==initial&&!e.SqueezeCompleted);
            s.Retry();m.enabled=false;Check("Retry resets fire, pin, capacity, result and route state",m.Elapsed==0&&e.Remaining==12&&!e.PinPulled&&!e.Held&&s.evaluation.Result==null&&!v.navigation.Available&&v.navigation.Corners.Length==0);
            s.MainMenu();s.ChooseMode(TrainingMode.Evacuation);s.StartSimulation();m.enabled=false;
            Check("Switch restores sprinkler availability and disables pickup",m.sprinklers.All(x=>x.Available)&&!e.TrainingEnabled&&!e.enabled&&!e.Held&&v.enabled&&v.Active);
            for(int i=0;i<800;i++)m.StepSimulation(.05f);await Task.Delay(200);
            Check("Restored sprinklers actually activate in evacuation mode",m.alarm.Active&&m.ActiveSprinklers>0&&m.sprinklers.Any(x=>x.water.isEmitting));
            s.MainMenu();Check("Main menu resets alarm, water, timers, scores and navigation",!m.alarm.Active&&m.ActiveSprinklers==0&&m.Elapsed==0&&v.Elapsed==0&&s.evaluation.Result==null&&s.evaluation.Events.Count==0&&!v.navigation.Available&&m.sprinklers.All(x=>!x.water.isEmitting&&!x.waterSound.isPlaying));
            s.ChooseMode(TrainingMode.FireExtinguisher);s.StartSimulation();m.enabled=false;
            Check("Returning to extinguisher restores interaction and excludes evacuation",e.TrainingEnabled&&e.enabled&&!v.enabled&&!v.Active&&m.sprinklers.All(x=>!x.Available&&!x.Active));
            Equip();for(int i=0;i<400&&m.sources[0].Burning;i++){var ray=Aim(Mathf.PingPong(i*.5f,8)-4);e.StepOperation(ray,true,p.view.eulerAngles.y,.05f);m.StepSimulation(.05f);}
            Check("Full PASS works after the mode switch round trip",e.PassComplete&&m.sources[0].ExtinguishedByAgent&&m.sources[0].WaterApplied==0&&!v.Completed);
            var result=s.evaluation.Finish(s.Mode,false);Check("Extinguisher result ignores evacuation objectives",result.completed&&result.extinguisherMode&&result.tasksRequired==5&&result.navigation==0&&result.evacuationSeconds==0);
            s.Retry();m.enabled=false;Equip();for(int i=0;i<250;i++)e.StepOperation(new Ray(p.view.position,Vector3.up),true,0,.05f);await Task.Delay(2100);
            Check("Exhausted agent produces retry result instead of false success",s.Page=="Results"&&s.evaluation.Result!=null&&!s.evaluation.Result.completed&&!s.evaluation.Result.suppressed);
            s.MainMenu();s.ChooseMode(TrainingMode.Evacuation);s.StartSimulation();m.enabled=false;
            Check("Second evacuation start has fresh independent state",m.sources[0].intensity==.18f&&m.ActiveSprinklers==0&&!m.alarm.Active&&e.SpraySeconds==0&&v.Elapsed<.1f&&s.evaluation.Result==null);
        } finally {m.enabled=true;p.enabled=true;e.enabled=e.TrainingEnabled;s.MainMenu();Application.runInBackground=bg;}
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Play Mode state-machine regression, shared simulation steps, prepared-pin fixture for suppression after mode switching",checks},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase7ModeC.json"),json);return json;
    }
}

