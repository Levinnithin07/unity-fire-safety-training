using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using FireSafety;
public static class TestPhaseSixScoring
{
    static void Set(object target,string property,object value)=>target.GetType().GetProperty(property).SetValue(target,value);
    public static string Run()
    {
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var e=s.evacuation.extinguisher;var v=s.evacuation;var m=s.simulation;var eval=s.evaluation;var checks=new List<object>();
        void Check(string name,bool pass,object value)=>checks.Add(new{name,pass,value});
        s.StartSimulation();m.StartScenario();Set(v,"StartedAt",1f);Set(v,"CrossedExit",true);Set(v,"Completed",true);Set(v,"Elapsed",12f);
        var safe=eval.Finish("Scoring fixture",false);Check("Safe evacuation gets full credit without discharge",safe.overall==100&&!safe.handlingAssessed&&safe.tasks==100,safe.overall);
        eval.ResetEvaluation();Set(e,"SpraySeconds",2f);Set(e,"CorrectAimSeconds",1f);Set(e,"PinPulled",true);Set(e,"AimCompleted",true);Set(e,"SqueezeCompleted",true);Set(e,"SweepCompleted",false);Set(e,"SuppressionCompleted",false);Set(e,"UnsafeEncountered",true);Set(v,"Elapsed",120f);Set(v,"HazardExposure",4f);
        var poor=eval.Finish("Scoring fixture",false);Check("Incorrect technique produces measured partial handling score",Mathf.Abs(poor.handling-56)<.001f&&poor.aimAccuracy==50,poor.handling);
        Check("Exposure and unsafe discharge lower safety score",poor.safety==45,poor.safety);Check("Long navigation time lowers navigation score",poor.navigation==85,poor.navigation);Check("Weighted overall is calculated from all metrics",Mathf.Abs(poor.overall-71.35f)<.001f,poor.overall);
        Check("Feedback responds to aim, sweep and exposure",poor.feedback.Contains("Aim lower")&&poor.feedback.Contains("left-and-right")&&poor.feedback.Contains("Move away"),poor.feedback);
        Check("Result snapshot is stable after component changes",ReferenceEquals(poor,eval.Finish("Ignored",false)),poor.overall);
        s.StartSimulation();m.StartScenario();var incomplete=eval.Finish("Scoring fixture",false);Check("Incomplete route cannot earn completion",!incomplete.completed&&incomplete.tasks==25&&incomplete.navigation==0,incomplete.overall);
        eval.ResetEvaluation();Set(e,"SpraySeconds",3f);Set(e,"CorrectAimSeconds",3f);Set(e,"PinPulled",true);Set(e,"AimCompleted",true);Set(e,"SqueezeCompleted",true);Set(e,"SweepCompleted",true);Set(e,"SuppressionCompleted",true);Set(v,"StartedAt",4f);Set(v,"CrossedExit",true);Set(v,"Completed",true);Set(v,"Elapsed",8f);
        var pass=eval.Finish("Scoring fixture",false);Check("Complete PASS receives assessed handling credit",pass.handling==100&&pass.overall==100&&pass.handlingAssessed,pass.handling);
        // Longest realistic feedback is deliberately used for layout testing, not as a gameplay result.
        eval.ResetEvaluation();Set(e,"PinPulled",false);Set(e,"AimCompleted",false);Set(e,"SqueezeCompleted",false);Set(e,"SweepCompleted",false);Set(e,"SuppressionCompleted",false);Set(e,"CorrectAimSeconds",0f);Set(e,"UnsafeEncountered",true);Set(v,"HazardExposure",10f);eval.Finish("Layout stress fixture",false);s.Show("Results");
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Controlled telemetry fixtures via property setters; no claim of played actions; fixtures are not saved as user reports",checks},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase6ScoringTests.json"),json);return json;
    }
}
