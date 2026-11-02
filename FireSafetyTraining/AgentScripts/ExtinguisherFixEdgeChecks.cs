using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using FireSafety;
public static class ExtinguisherFixEdgeChecks
{
    public static async Task<string> Run()
    {
        bool background=Application.runInBackground;Application.runInBackground=true;
        await Task.Delay(500);
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();s.StartSimulation();s.enabled=false;
        var e=s.evacuation.extinguisher;var m=e.simulation;var p=e.player;var v=e.evacuation;var cc=p.GetComponent<CharacterController>();
        var checks=new List<object>();GameObject block=null;
        try {
            m.enabled=false;v.enabled=false;p.enabled=false;
            cc.enabled=false;p.transform.position=new Vector3(4.7f,.025f,-1.25f);cc.enabled=true;p.view.LookAt(e.pickupRenderer.bounds.center);Physics.SyncTransforms();e.TryPickup(new Ray(p.view.position,p.view.forward));e.enabled=false;
            typeof(FireExtinguisherController).GetProperty("PinPulled").SetValue(e,true);
            cc.enabled=false;p.transform.position=new Vector3(0,.025f,-1.8f);cc.enabled=true;m.StartScenario();
            for(int i=0;i<180;i++){
                float yaw=.3f*Mathf.Sin(i/60f*Mathf.PI*16);
                p.view.rotation=Quaternion.Euler(0,yaw,0)*Quaternion.LookRotation(m.sources[0].transform.position+Vector3.up*.075f-p.view.position);
                Physics.SyncTransforms();e.StepOperation(new Ray(p.view.position,p.view.forward),true,p.view.eulerAngles.y,1/60f);m.StepSimulation(1/60f);
            }
            checks.Add(new{name="Tiny hand jitter cannot earn sweep credit",pass=!e.SweepCompleted&&!e.PassComplete});
            p.view.LookAt(m.sources[0].transform.position+Vector3.up*.075f);Physics.SyncTransforms();var ray=new Ray(p.view.position,p.view.forward);Vector3 point;
            checks.Add(new{name="Base aim is clear before obstruction",pass=e.FindBaseTarget(ray,out point)==m.sources[0]});
            block=new GameObject("TEMP nozzle obstruction");block.transform.position=e.nozzle.position+(point-e.nozzle.position).normalized*.15f;block.AddComponent<BoxCollider>().size=Vector3.one*.08f;Physics.SyncTransforms();
            bool blocked=e.FindBaseTarget(ray,out point)==null;
            checks.Add(new{name="Nozzle obstruction gets specific feedback",pass=blocked&&e.AimFeedback.Contains("Nozzle blocked"),feedback=e.AimFeedback});
            UnityEngine.Object.DestroyImmediate(block);block=null;Physics.SyncTransforms();
            for(int i=0;i<800&&!m.alarm.Active;i++)m.StepSimulation(.05f);
            v.enabled=true;await Task.Delay(600);
            float remaining=e.Remaining;e.StepOperation(ray,true,p.view.eulerAngles.y,.05f);await Task.Delay(150);
            var ui=UnityEngine.Object.FindAnyObjectByType<ExtinguisherStatusUI>();ui.prompt.ForceMeshUpdate();
            checks.Add(new{name="Smoke alarm initiates evacuation and stops spray",pass=m.alarm.Active&&v.Active&&!e.Spraying&&e.Remaining==remaining,reason=v.Reason});
            checks.Add(new{name="Evacuation HUD explains why spraying stopped",pass=ui.prompt.text.Contains("spray disabled")&&ui.prompt.text.Contains("Smoke alarm"),text=ui.prompt.text});
            checks.Add(new{name="Feedback fits current Game view",pass=!ui.prompt.isTextOverflowing,width=Screen.width,height=Screen.height,overflow=ui.prompt.isTextOverflowing});
        } finally {
            if(block)UnityEngine.Object.DestroyImmediate(block);
            e.enabled=true;m.enabled=true;v.enabled=true;p.enabled=true;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/ExtinguisherFixEdgeChecks.json"),json);
        // Leave a frozen, visible gameplay HUD for a screenshot, only in this Play Mode instance.
        s.Resume();s.enabled=false;p.enabled=false;m.enabled=false;v.enabled=false;Time.timeScale=0;p.SetCapture(false);
        Application.runInBackground=background;return json;
    }
}
