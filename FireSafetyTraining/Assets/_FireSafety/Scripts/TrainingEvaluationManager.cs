using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FireSafety
{
    [Serializable] public sealed class TrainingEvent { public string action; public float seconds; }
    [Serializable] public sealed class TrainingResult
    {
        public string completedUtc, practiceMode, status, feedback, formula;
        public bool completed, handlingAssessed, pin, aim, squeeze, sweep, suppressed, extinguisherMode;
        public float effectiveSpraySeconds, effectiveSweepSeconds, capacityConsumedPercent, extinguishSeconds;
        public float overall, tasks, handling, aimAccuracy, safety, navigation, evacuationSeconds, exposureSeconds, spraySeconds;
        public int tasksCompleted, tasksRequired, reroutes;
        public List<TrainingEvent> events;
    }
    [DefaultExecutionOrder(300)]
    public sealed class TrainingEvaluationManager : MonoBehaviour
    {
        public FireManager simulation;
        public FireExtinguisherController equipment;
        public EvacuationManager evacuation;
        public TrainingResult Result { get; private set; }
        public string SaveStatus { get; private set; } = "";
        public readonly List<TrainingEvent> Events = new List<TrainingEvent>();
        readonly HashSet<string> recorded = new HashSet<string>();
        public void ResetEvaluation(){Result=null;SaveStatus="";Events.Clear();recorded.Clear();}
        void Record(string action,bool happened){if(happened&&recorded.Add(action))Events.Add(new TrainingEvent{action=action,seconds=simulation.Elapsed});}
        public void Observe()
        {
            Record("Emergency started",simulation.Running);
            if(equipment.TrainingEnabled){
                Record("Equipment collected",equipment.Held);Record("Safety pin removed",equipment.PinPulled);
                Record("Base aimed",equipment.AimCompleted);Record("Correct squeeze",equipment.SqueezeCompleted);
                Record("Controlled sweep",equipment.SweepCompleted);Record("Fire suppressed by extinguisher",equipment.SuppressionCompleted);
            } else {
                Record("Alarm activated",simulation.alarm.Active);Record("Sprinkler activated",simulation.ActiveSprinklers>0);
                Record("Evacuation started",evacuation.Active||evacuation.Completed);Record("Exterior exit crossed",evacuation.CrossedExit);Record("Assembly reached",evacuation.Completed);
            }
        }
        void LateUpdate(){if(Result==null&&TrainingSession.InputAllowed)Observe();}
        public TrainingResult Finish(string mode,bool persist=true)
        {
            if(Result!=null)return Result;Observe();
            var e=equipment;var v=evacuation;
            bool assessed=e.TrainingEnabled;
            int steps=(e.PinPulled?1:0)+(e.AimCompleted?1:0)+(e.SqueezeCompleted?1:0)+(e.SweepCompleted?1:0)+(e.SuppressionCompleted?1:0);
            int core=(simulation.Running?1:0)+(v.StartedAt>=0?1:0)+(v.CrossedExit?1:0)+(v.Completed?1:0);
            float tasks=assessed?steps*20f:core*25f;
            float consumed=100*Mathf.Clamp01(1-e.Remaining/e.capacitySeconds);
            float handling=assessed?tasks*.5f+e.Accuracy*.3f+(e.SuppressionCompleted?100-consumed:0)*.2f:0;
            float safety=Mathf.Clamp(100-v.HazardExposure*5,0,100);
            float navigation=v.Completed?Mathf.Clamp(100-Mathf.Max(0,v.Elapsed-90)*.5f,40,100):0;
            float overall=assessed?handling:tasks*.4f+safety*.4f+navigation*.2f;
            bool complete=assessed?e.SuppressionCompleted:v.Completed;
            string feedback;
            if(assessed){
                feedback=e.SuppressionCompleted?"Fire Successfully Extinguished.":"Agent exhausted before the fire was suppressed. Retry the mode.";
                if(e.PassComplete)feedback+=" You completed every PASS step.";
                else {if(!e.PinPulled)feedback+=" Pull the pin before use.";if(e.Accuracy<75)feedback+=" Aim at the base of the flames and keep the nozzle path clear.";if(!e.SweepCompleted)feedback+=" Practise a controlled left-and-right sweep.";}
            } else {
                feedback=v.Completed?"You reached Assembly Point A. Stay outside and report in.":"Follow a clear route to the assembly point to complete this mode.";
                if(v.HazardExposure>.2f)feedback+=" Move away from hot or smoky doorways sooner.";
            }
            Result=new TrainingResult{completedUtc=DateTime.UtcNow.ToString("o"),practiceMode=mode,extinguisherMode=assessed,status=complete?(assessed?"FIRE SUCCESSFULLY EXTINGUISHED":"EVACUATION COMPLETE"):"TRY AGAIN",completed=complete,handlingAssessed=assessed,pin=e.PinPulled,aim=e.AimCompleted,squeeze=e.SqueezeCompleted,sweep=e.SweepCompleted,suppressed=e.SuppressionCompleted,overall=overall,tasks=tasks,handling=handling,aimAccuracy=e.Accuracy,safety=assessed?0:safety,navigation=assessed?0:navigation,evacuationSeconds=v.Elapsed,exposureSeconds=v.HazardExposure,spraySeconds=e.SpraySeconds,effectiveSpraySeconds=e.CorrectAimSeconds,effectiveSweepSeconds=e.EffectiveSeconds,capacityConsumedPercent=consumed,extinguishSeconds=e.SuppressedAt,tasksCompleted=assessed?steps:core,tasksRequired=assessed?5:4,reroutes=v.Reroutes,feedback=feedback,events=new List<TrainingEvent>(Events),formula=assessed?"Score: PASS/suppression objectives 50%, on-target accuracy 30%, remaining agent after successful suppression 20%.":"Score: evacuation objectives 40%, safety 40%, navigation 20%. Safety loses 5 points per hazard second. Navigation: 100 up to 90s, then -0.5/s, minimum 40 on completion."};
            if(persist)try{string dir=Path.Combine(Application.persistentDataPath,"TrainingResults");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"Training-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+".json"),JsonUtility.ToJson(Result,true));SaveStatus="Report saved locally in TrainingResults.";}catch(Exception ex)when(ex is IOException||ex is UnauthorizedAccessException){SaveStatus="Report shown here; local saving was unavailable.";}
            return Result;
        }
    }
}

