using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using FireSafety;
using TMPro;
public static class AuditPhaseFive
{
    public static string UI()
    {
        Canvas.ForceUpdateCanvases();var v=UnityEngine.Object.FindAnyObjectByType<EvacuationManager>();
        var rows=UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().Select(t=>{t.ForceMeshUpdate();var points=new Vector3[4];t.rectTransform.GetWorldCorners(points);return new{name=t.name,overflow=t.isTextOverflowing,inside=points.All(p=>p.x>=-.1f&&p.y>=-.1f&&p.x<=Screen.width+.1f&&p.y<=Screen.height+.1f)};}).ToArray();
        var ui=UnityEngine.Object.FindAnyObjectByType<EvacuationUI>();string old=ui.detail.text;ui.detail.text="No clear route detected. Do not enter smoke; seek a safer location and signal for help.";ui.detail.ForceMeshUpdate();bool worstFits=!ui.detail.isTextOverflowing;ui.detail.text=old;
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{width=Screen.width,height=Screen.height,v.Active,v.Completed,worstFits,passed=rows.All(r=>!r.overflow&&r.inside)&&worstFits,rows},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase5HUD-"+Screen.width+".json"),json);return json;
    }
    public static string Scene()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var v=UnityEngine.Object.FindAnyObjectByType<EvacuationManager>();var missing=new System.Collections.Generic.List<string>();
        foreach(var b in all.SelectMany(t=>t.GetComponents<MonoBehaviour>()).Where(b=>b&&b.GetType().Namespace=="FireSafety")){var so=new SerializedObject(b);var p=so.GetIterator();while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference&&p.name!="m_Script"&&p.objectReferenceValue==null)missing.Add(b.GetType().Name+"."+p.propertyPath);}
        var mats=all.SelectMany(t=>t.GetComponents<Renderer>()).SelectMany(r=>r.sharedMaterials).ToArray();var s=UnityEngine.Object.FindAnyObjectByType<Unity.AI.Navigation.NavMeshSurface>();v.navigation.Recalculate(v.spawn);
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=scene.path,scene.isDirty,playMode=Application.isPlaying,missingScripts=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),unassignedReferences=missing,invalidMaterials=mats.Count(m=>!m||!m.shader||!m.shader.isSupported),navAsset=AssetDatabase.GetAssetPath(s.navMeshData),navTriangles=UnityEngine.AI.NavMesh.CalculateTriangulation().indices.Length/3,s.voxelSize,agent=v.navigation.agentType,initialRoute=v.navigation.RouteName,initialPathValid=v.navigation.Available,closedDoorColliders=v.exitDoor.GetComponentsInChildren<Collider>().Length,assemblyZoneIsTrigger=v.assemblyZone.isTrigger,particleCap=all.SelectMany(t=>t.GetComponents<ParticleSystem>()).Sum(ps=>ps.main.maxParticles)},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase5SceneAudit.json"),json);return json;
    }
}
