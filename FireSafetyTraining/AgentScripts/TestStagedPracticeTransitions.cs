using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FireSafety;
public static class TestStagedPracticeTransitions
{
    public static string Run()
    {
        var s=TrainingSession.Instance;var m=s.simulation;var v=s.evacuation;var e=v.extinguisher;
        var checks=new List<object>();
        void Check(string name,bool pass)=>checks.Add(new{name,pass});
        void Advance(float seconds){for(int i=0;i<Mathf.RoundToInt(seconds/.05f);i++){m.StepSimulation(.05f);v.EvaluateTransition();}}
        s.StartSimulation();m.enabled=false;v.enabled=false;
        try {
            m.StartScenario();Advance(9.9f);Check("No automatic evacuation before ten seconds",!v.Active&&v.Practising);
            Advance(.1f);Check("Time limit evacuates even without successful suppression",v.Active&&!e.SuppressionCompleted&&Mathf.Abs(v.StartedAt-10)<.01f);
            s.StartSimulation();v.ResetEvacuation();m.StartScenario();v.EvaluateTransition(true);Check("Voluntary evacuation bypasses countdown",v.Active&&v.StartedAt<1);
            s.StartSimulation();v.ResetEvacuation();m.StartScenario();m.sources[1].Ignite(0);v.EvaluateTransition();Check("Unsafe spread evacuates immediately",v.Active&&v.Reason=="Fire conditions are unsafe");
            s.StartSimulation();v.ResetEvacuation();m.StartScenario();m.alarm.Activate("Test detector",0);v.EvaluateTransition();Check("Alarm alone does not cancel safe practice interval",m.alarm.Active&&!v.Active&&v.Practising);
            s.SelectMode();s.StartSimulation();v.ResetEvacuation();Check("Evacuation-only mode does not require equipment",m.ReadyToStart&&!v.UsesPracticeStage);m.StartScenario();m.alarm.Activate("Test detector",0);v.EvaluateTransition();Check("Evacuation-only mode still responds immediately to alarm",v.Active&&v.Reason=="Smoke alarm activated");
            s.SelectMode();s.StartSimulation();v.ResetEvacuation();Check("Restart restores stage and full countdown",!m.Running&&!v.Active&&v.PracticeRemaining==10&&!e.Held&&e.Remaining==e.capacitySeconds);
        } finally {m.enabled=true;v.enabled=true;s.MainMenu();}
        var json=Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/StagedPracticeTransitions.json"),json);return json;
    }
}
