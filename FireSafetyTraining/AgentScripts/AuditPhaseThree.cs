using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using FireSafety;
using TMPro;
public static class AuditPhaseThree
{
    public static string UI()
    {
        Canvas.ForceUpdateCanvases();
        var labels=UnityEngine.Object.FindObjectsByType<TMP_Text>();
        var rows=labels.Select(t=>{t.ForceMeshUpdate();var v=new Vector3[4];t.rectTransform.GetWorldCorners(v);return new{name=t.name,overflow=t.isTextOverflowing,inside=v.All(p=>p.x>=0&&p.y>=0&&p.x<=Screen.width&&p.y<=Screen.height)};}).ToArray();
        var report=new{width=Screen.width,height=Screen.height,labels=rows,passed=rows.All(r=>!r.overflow&&r.inside)};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.GetFullPath("../Documentation/Phase3HUD-"+Screen.width+".json"),json);return json;
    }
    public static async Task<string> Feedback()
    {
        var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();bool on=false,off=false;float fire=0,water=0;
        var buffer=new float[1024];double begin=Time.realtimeSinceStartupAsDouble;
        while(Time.realtimeSinceStartupAsDouble-begin<3){
            await Task.Delay(50);on|=m.alarm.beacon.enabled;off|=!m.alarm.beacon.enabled;
            m.sources[0].crackle.GetOutputData(buffer,0);fire=Mathf.Max(fire,Mathf.Sqrt(buffer.Sum(x=>x*x)/buffer.Length));
            m.sprinklers[0].waterSound.GetOutputData(buffer,0);water=Mathf.Max(water,Mathf.Sqrt(buffer.Sum(x=>x*x)/buffer.Length));
        }
        var report=new{flashOnObserved=on,flashOffObserved=off,maxFireOutputRms=fire,maxWaterOutputRms=water,passed=on&&off&&fire>0&&water>0};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase3FeedbackAudit.json"),json);return json;
    }
    public static string Scene()
    {
        var scene=EditorSceneManager.GetActiveScene();var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();
        var renderers=UnityEngine.Object.FindObjectsByType<Renderer>();var mats=renderers.SelectMany(r=>r.sharedMaterials).ToArray();
        var report=new{scene=scene.path,scene.isDirty,playMode=Application.isPlaying,emergencyEnabled=m.enabled,
            sourceCount=m.sources.Length,detectorCount=m.detectors.Length,sprinklerCount=m.sprinklers.Length,
            detectorAlarmReferences=m.detectors.All(d=>d.alarm==m.alarm),particleBudget=UnityEngine.Object.FindObjectsByType<ParticleSystem>().Sum(p=>p.main.maxParticles),
            particleTextures=new[]{"flame_01","smoke_04","spark_01","circle_05"}.Select(n=>AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/KenneyParticles/"+n+".png")).All(t=>t),
            missingMaterials=mats.Count(mat=>!mat),unsupportedShaders=mats.Count(mat=>mat&&!mat.shader.isSupported),
            missingScripts=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),
            colliders=UnityEngine.Object.FindObjectsByType<Collider>().Length,spawn=m.player.transform.position.ToString("F3"),
            fullscreen=PlayerSettings.fullScreenMode.ToString(),nativeResolution=PlayerSettings.defaultIsNativeResolution,
            fixedStepSeconds=.05f,smokeModel="Normalized room accumulator with passage leakage; no CFD or sensor certification",
            heatModel="Local intensity/distance with per-head thermal lag; demonstration threshold 68 C"};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase3SceneAudit.json"),json);return json;
    }
}
