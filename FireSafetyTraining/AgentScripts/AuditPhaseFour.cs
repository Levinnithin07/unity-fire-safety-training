using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using FireSafety;
using TMPro;
public static class AuditPhaseFour
{
    public static string UI()
    {
        Canvas.ForceUpdateCanvases();var e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();var ui=UnityEngine.Object.FindAnyObjectByType<ExtinguisherStatusUI>();string before=ui.prompt.text;
        var messages=new[]{"P - Pull the pin. In a real emergency, alert others and keep an escape route behind you.","Aim low at the burning desk surface, from 1.4-4m. Keep the nozzle path clear.","UNSAFE: fire has grown or spread. Stop the attempt and evacuate.","Extinguisher empty. Leave by a safe route; do not keep attempting suppression.","Fire suppressed by your extinguisher. Stay clear and leave by a safe route."};
        var prompts=messages.Select(s=>{ui.prompt.text=s;ui.prompt.ForceMeshUpdate();return new{text=s,overflow=ui.prompt.isTextOverflowing};}).ToArray();ui.prompt.text=before;
        var rows=UnityEngine.Object.FindObjectsByType<TMP_Text>().Select(t=>{t.ForceMeshUpdate();var v=new Vector3[4];t.rectTransform.GetWorldCorners(v);return new{name=t.name,overflow=t.isTextOverflowing,inside=v.All(p=>p.x>=-.1f&&p.y>=-.1f&&p.x<=Screen.width+.1f&&p.y<=Screen.height+.1f)};}).ToArray();
        var report=new{width=Screen.width,height=Screen.height,held=e.Held,labels=rows,prompts,passed=e.Held&&rows.All(r=>!r.overflow&&r.inside)&&prompts.All(t=>!t.overflow)};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase4HUD-"+Screen.width+".json"),json);return json;
    }
    public static string Scene()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();
        var missing=new System.Collections.Generic.List<string>();
        foreach(var b in all.SelectMany(t=>t.GetComponents<MonoBehaviour>()).Where(b=>b&&b.GetType().Namespace=="FireSafety")){
            var so=new SerializedObject(b);var prop=so.GetIterator();while(prop.NextVisible(true))if(prop.propertyType==SerializedPropertyType.ObjectReference&&prop.name!="m_Script"&&prop.objectReferenceValue==null)missing.Add(b.GetType().Name+"."+prop.propertyPath);
        }
        var mats=all.SelectMany(t=>t.GetComponents<Renderer>()).SelectMany(r=>r.sharedMaterials).ToArray();
        var report=new{scene=scene.path,scene.isDirty,playing=Application.isPlaying,missingScripts=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),unassignedReferences=missing.ToArray(),invalidMaterials=mats.Count(m=>!m||!m.shader||!m.shader.isSupported),heldMeshes=e.heldRoot.GetComponentsInChildren<MeshFilter>(true).Select(f=>new{f.name,vertices=f.sharedMesh.vertexCount}).ToArray(),capacitySeconds=e.capacitySeconds,particleBudget=all.SelectMany(t=>t.GetComponents<ParticleSystem>()).Sum(ps=>ps.main.maxParticles),overlayCameras=all.SelectMany(t=>t.GetComponents<Camera>()).Count(),fullscreen=PlayerSettings.fullScreenMode.ToString(),native=PlayerSettings.defaultIsNativeResolution};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase4SceneAudit.json"),json);return json;
    }
    public static async Task<string> Performance()
    {
        var e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();var m=e.simulation;var p=e.player;bool bg=Application.runInBackground;Application.runInBackground=true;
        try {
            m.ResetScenario();p.enabled=false;p.SetCapture(true);var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(4.7f,.025f,-1.25f);cc.enabled=true;p.view.LookAt(e.pickupRenderer.bounds.center);Physics.SyncTransforms();e.TryPickup(new Ray(p.view.position,p.view.forward));e.PullPin();await Task.Delay(600);
            cc.enabled=false;p.transform.position=new Vector3(0,.025f,-1.8f);cc.enabled=true;p.view.LookAt(m.sources[0].transform.position+Vector3.up*.5f);e.enabled=false;m.enabled=true;m.StartScenario();
            var buffers=new float[1024];double audio=0;int peak=0;int frame=Time.frameCount;double start=Time.realtimeSinceStartupAsDouble;bool captured=false;
            while(Time.realtimeSinceStartupAsDouble-start<8){
                await Task.Delay(20);e.StepOperation(new Ray(p.view.position,p.view.forward),true,p.view.eulerAngles.y,Time.deltaTime);
                e.spraySound.GetOutputData(buffers,0);audio=Math.Max(audio,Math.Sqrt(buffers.Average(x=>(double)x*x)));peak=Math.Max(peak,UnityEngine.Object.FindObjectsByType<ParticleSystem>().Sum(ps=>ps.particleCount));
                if(!captured&&Time.realtimeSinceStartupAsDouble-start>3){ScreenCapture.CaptureScreenshot("Assets/Screenshots/Phase4-Spray-1080.png");captured=true;}
            }
            double seconds=Time.realtimeSinceStartupAsDouble-start;var report=new{scope="Editor, fixed camera, live growing fire plus held textured model and continuous off-target discharge; not a standalone benchmark",gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,seconds,frames=Time.frameCount-frame,averageFps=(Time.frameCount-frame)/seconds,peakParticles=peak,sprayOutputRms=audio};
            string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase4Performance.json"),json);return json;
        }finally{e.enabled=true;p.SetCapture(false);Application.runInBackground=bg;}
    }
}
