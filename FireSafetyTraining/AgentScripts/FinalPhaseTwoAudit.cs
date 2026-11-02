using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using FireSafety;
public static class FinalPhaseTwoAudit
{
 public static string Run(){
 var scene=EditorSceneManager.GetActiveScene();var player=UnityEngine.Object.FindAnyObjectByType<DesktopPlayerController>();
 var renderers=UnityEngine.Object.FindObjectsByType<Renderer>();var materials=renderers.SelectMany(r=>r.sharedMaterials).ToArray();
 var profile=(UniversalRenderPipelineAsset)UnityEngine.Object.FindAnyObjectByType<DesktopDisplaySettings>().mediumPipeline;
 var extinguisher=GameObject.Find("korean_fire_extinguisher_01_2k").GetComponentInChildren<Renderer>();
 var report=new {scene=scene.path,playMode=Application.isPlaying,playerSpawn=player.transform.position.ToString("F3"),
 colliderCount=UnityEngine.Object.FindObjectsByType<Collider>().Length,
 missingMaterials=materials.Count(m=>!m),unsupportedShaders=materials.Count(m=>m && !m.shader.isSupported),
 missingScripts=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),
 extinguisherHeight=extinguisher.bounds.size.y,extinguisherUpright=extinguisher.bounds.size.y>.6f,
 fullscreenMode=PlayerSettings.fullScreenMode.ToString(),nativeResolution=PlayerSettings.defaultIsNativeResolution,
 renderScale=profile.renderScale,msaa=profile.msaaSampleCount,shadowDistance=profile.shadowDistance,
 shadowResolution=profile.mainLightShadowmapResolution,maxAdditionalLights=profile.maxAdditionalLightsCount,
 additionalLightShadows=profile.supportsAdditionalLightShadows,srpBatcher=profile.useSRPBatcher,
 buildScene=EditorBuildSettings.scenes.Select(s=>new{s.path,s.enabled}).ToArray()};
 string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);
 File.WriteAllText(Path.GetFullPath("../Documentation/Phase2SceneAudit.json"),json);return json;
 }
}
