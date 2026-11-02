using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FireSafety;
public static class AuditPhaseSeven
{
    public static string Run()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var nulls=new List<string>();
        foreach(var b in all.SelectMany(t=>t.GetComponents<MonoBehaviour>()).Where(b=>b&&b.GetType().Namespace=="FireSafety")){var so=new SerializedObject(b);var p=so.GetIterator();while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference&&p.name!="m_Script"&&p.objectReferenceValue==null)nulls.Add(b.GetType().Name+"."+p.propertyPath);}
        var module=UnityEngine.Object.FindAnyObjectByType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();var refs=new[]{module.point,module.leftClick,module.rightClick,module.middleClick,module.move,module.submit,module.cancel,module.scrollWheel,module.trackedDevicePosition,module.trackedDeviceOrientation};
        var mats=all.SelectMany(t=>t.GetComponents<Renderer>()).SelectMany(r=>r.sharedMaterials).ToArray();
        var quality=s.settings.pipelines.Cast<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>().Select(a=>new{a.name,a.renderScale,a.shadowDistance,a.msaaSampleCount,a.mainLightShadowmapResolution,path=AssetDatabase.GetAssetPath(a)}).ToArray();
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=scene.path,scene.isDirty,playMode=Application.isPlaying,missingScripts=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),unassignedReferences=nulls,invalidMaterials=mats.Count(m=>!m||!m.shader||!m.shader.isSupported),eventSystems=all.Count(t=>t.GetComponent<UnityEngine.EventSystems.EventSystem>()),uiActionsResolveToSavedAsset=refs.All(r=>r&&r.action!=null&&!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(r.action.actionMap.asset))),menuPages=s.ui.pages.Select(p=>p.name),menuButtons=s.ui.GetComponentsInChildren<UnityEngine.UI.Button>(true).Select(b=>b.name),quality,fullscreen=PlayerSettings.fullScreenMode.ToString(),nativeResolution=PlayerSettings.defaultIsNativeResolution,buildScenes=EditorBuildSettings.scenes.Select(x=>new{x.path,x.enabled}),navTriangles=UnityEngine.AI.NavMesh.CalculateTriangulation().indices.Length/3,reportDirectory=Path.Combine(Application.persistentDataPath,"TrainingResults")},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase7ModesSceneAudit.json"),json);return json;
    }
}


