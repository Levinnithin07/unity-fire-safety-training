using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using FireSafety;
public static class DiagnoseExtinguisher
{
    public static Task<string> Before()=>Run("Before");
    public static Task<string> After()=>Run("After");
    static async Task<string> Run(string stage)
    {
        var session=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();session.StartSimulation();var e=session.evacuation.extinguisher;var m=e.simulation;var p=e.player;var v=e.evacuation;var cc=p.GetComponent<CharacterController>();var rows=new List<object>();
        var cases=new[]{new{name="Controlled sweep 60 FPS",fps=60,style=0},new{name="Controlled sweep 20 FPS",fps=20,style=0},new{name="Controlled sweep 10 FPS",fps=10,style=0},new{name="Steady correct base aim",fps=60,style=1},new{name="Sweep with small hand jitter",fps=60,style=2}};
        try{
            m.enabled=false;v.enabled=false;p.enabled=false;
            foreach(var c in cases){
                e.enabled=true;m.ResetScenario();e.ResetEquipment();cc.enabled=false;p.transform.position=new Vector3(4.7f,.025f,-1.25f);cc.enabled=true;p.view.LookAt(e.pickupRenderer.bounds.center);Physics.SyncTransforms();e.TryPickup(new Ray(p.view.position,p.view.forward));e.enabled=false;
                // Prepared-pin fixture isolates suppression timing from Editor focus/animation timing.
                typeof(FireExtinguisherController).GetProperty("PinPulled").SetValue(e,true);
                cc.enabled=false;p.transform.position=new Vector3(0,.025f,-1.8f);cc.enabled=true;Physics.SyncTransforms();m.StartScenario();float dt=1f/c.fps;float seconds=0;int valid=0;
                for(int i=0;i<c.fps*10&&m.sources[0].Burning;i++){
                    float t=i*dt;float yaw=c.style==1?0:c.style==0?Mathf.PingPong(t*40,8)-4:3.3f*Mathf.Sin(t*Mathf.PI*1.2f)+.32f*Mathf.Sin(t*Mathf.PI*24);
                    p.view.rotation=Quaternion.Euler(0,yaw,0)*Quaternion.LookRotation(m.sources[0].transform.position+Vector3.up*.075f-p.view.position);var ray=new Ray(p.view.position,p.view.forward);
                    e.StepOperation(ray,true,p.view.eulerAngles.y,dt);if(e.CorrectAim)valid++;
                    float rest=dt;while(rest>.00001f){float step=Mathf.Min(rest,.05f);m.StepSimulation(step);rest-=step;}seconds+=dt;
                }
                rows.Add(new{c.name,c.fps,seconds,intensity=m.sources[0].intensity,e.Spraying,e.SpraySeconds,e.CorrectAimSeconds,e.SweepCompleted,e.PassComplete,e.SuppressionCompleted,e.AgentSuppression,e.Remaining,validFrames=valid,e.Feedback});
            }
        }finally{e.enabled=true;m.enabled=true;v.enabled=true;p.enabled=true;session.MainMenu();}
        await Task.CompletedTask;string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{stage,scope="Deterministic component simulation with prepared-pin fixture at specified frame intervals using real classroom colliders; not a hardware FPS benchmark",rows},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/ExtinguisherReliability-"+stage+".json"),json);return json;
    }
}

