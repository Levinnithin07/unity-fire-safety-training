using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using FireSafety;

public static class BuildTrainingScene
{
    const string Target="Assets/_FireSafety/Scenes/TrainingScene.unity";
    public static string Run()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        if(File.Exists(Target)) throw new InvalidOperationException("TrainingScene already exists; use targeted updates.");
        var current=EditorSceneManager.GetActiveScene();
        if(current.isDirty) throw new InvalidOperationException("Save user edits before switching scenes.");
        var source=EditorSceneManager.OpenScene("Assets/_FireSafety/Scenes/AssetReview.unity");
        EditorSceneManager.SaveScene(source,Target,true);
        var scene=EditorSceneManager.OpenScene(Target);
        foreach(var cam in UnityEngine.Object.FindObjectsByType<Camera>()) UnityEngine.Object.DestroyImmediate(cam.gameObject);
        foreach(var light in UnityEngine.Object.FindObjectsByType<Light>()) UnityEngine.Object.DestroyImmediate(light.gameObject);
        var room=scene.GetRootGameObjects().First(g=>g.name.StartsWith("Classroom"));
        // Source art is never unpacked or rewritten. Scene-only transform overrides open the doors.
        foreach(var renderer in room.GetComponentsInChildren<MeshRenderer>()) {
            if(renderer.name=="DoorWood_2" || renderer.name=="DoorMetal_2") renderer.transform.RotateAround(new Vector3(3.3f,0,-3.20f),Vector3.up,-60);
            if(renderer.name=="DoorWood" || renderer.name=="DoorMetal") renderer.transform.RotateAround(new Vector3(3.3f,0,3.50f),Vector3.up,60);
            var filter=renderer.GetComponent<MeshFilter>();
            if(filter && filter.sharedMesh) {
                var col=renderer.gameObject.AddComponent<MeshCollider>(); col.sharedMesh=filter.sharedMesh;
            }
            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
        }
        var floor=room.GetComponentsInChildren<MeshRenderer>().First(r=>r.name=="Floor");
        var solidFloor=new GameObject("Floor collision support");
        solidFloor.transform.position=new Vector3(floor.bounds.center.x,-.11f,floor.bounds.center.z);
        solidFloor.AddComponent<BoxCollider>().size=new Vector3(floor.bounds.size.x,.2f,floor.bounds.size.z);
        var equipment=new GameObject("Safety equipment - future interaction");
        foreach(var root in scene.GetRootGameObjects().Where(g=>g.name.Contains("_2k"))) {
            root.transform.SetParent(equipment.transform);
            bool extinguisher=root.name.Contains("extinguisher");
            root.transform.position=extinguisher?new Vector3(3.65f,.015f,-1.25f):new Vector3(3.48f,1.35f,-.7f);
            root.transform.rotation=Quaternion.Euler(0,90,0)*root.transform.rotation;
            foreach(var mf in root.GetComponentsInChildren<MeshFilter>()) mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
        }
        var lighting=new GameObject("Training lighting");
        var sun=new GameObject("Window daylight").AddComponent<Light>();sun.transform.SetParent(lighting.transform);
        sun.type=LightType.Directional;sun.intensity=1.35f;sun.color=new Color(1,.94f,.84f);
        sun.transform.rotation=Quaternion.Euler(36,115,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.8f;sun.shadowBias=.03f;
        RenderSettings.sun=sun;RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.63f,.68f,.75f);
        RenderSettings.ambientEquatorColor=new Color(.43f,.46f,.5f);
        RenderSettings.ambientGroundColor=new Color(.22f,.23f,.25f);
        RenderSettings.reflectionIntensity=.65f;
        foreach(var fixture in room.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("CeilingLampLight"))) {
            var lamp=new GameObject("Fixture fill - "+fixture.name).AddComponent<Light>();lamp.transform.SetParent(lighting.transform);
            lamp.type=LightType.Point;lamp.transform.position=fixture.bounds.center+Vector3.down*.25f;
            lamp.range=4.3f;lamp.intensity=.75f;lamp.color=new Color(1,.93f,.82f);lamp.shadows=LightShadows.None;
        }
        foreach(float z in new[]{-2.7f,2.7f}) {
            var lamp=new GameObject("Passage fill").AddComponent<Light>();lamp.transform.SetParent(lighting.transform);
            lamp.type=LightType.Point;lamp.transform.position=new Vector3(4.9f,2.1f,z);lamp.range=4.5f;lamp.intensity=.8f;lamp.color=new Color(.9f,.95f,1);
        }
        var probe=new GameObject("Classroom reflection probe").AddComponent<ReflectionProbe>();probe.transform.SetParent(lighting.transform);
        probe.transform.position=new Vector3(.2f,1.5f,0);probe.size=new Vector3(6.2f,3,8);probe.boxProjection=true;
        probe.resolution=128;probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;
        probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;probe.cullingMask=1;probe.shadowDistance=0;probe.intensity=.65f;
        var player=new GameObject("Desktop Player");player.layer=2;player.transform.position=new Vector3(0,.08f,-1.95f);
        var motor=player.AddComponent<CharacterController>();motor.height=1.8f;motor.radius=.24f;motor.center=new Vector3(0,.9f,0);
        motor.stepOffset=.18f;motor.slopeLimit=45;motor.skinWidth=.025f;motor.minMoveDistance=0;
        var cameraObject=new GameObject("Player Camera");cameraObject.tag="MainCamera";cameraObject.layer=2;cameraObject.transform.SetParent(player.transform,false);
        cameraObject.transform.localPosition=new Vector3(0,1.67f,0);
        var camera=cameraObject.AddComponent<Camera>();camera.fieldOfView=68;camera.nearClipPlane=.045f;camera.farClipPlane=60;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.58f,.69f,.79f);
        var data=cameraObject.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;
        cameraObject.AddComponent<AudioListener>();player.AddComponent<DesktopPlayerController>().view=camera.transform;
        const string profilePath="Assets/_FireSafety/Settings/TrainingMedium.asset";
        Directory.CreateDirectory("Assets/_FireSafety/Settings");AssetDatabase.Refresh();
        var profile=UnityEngine.Object.Instantiate((UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline);
        profile.name="TrainingMedium";profile.renderScale=1;profile.msaaSampleCount=2;profile.shadowDistance=22;
        profile.mainLightShadowmapResolution=2048;profile.shadowCascadeCount=2;
        profile.maxAdditionalLightsCount=4;
        profile.supportsCameraDepthTexture=false;profile.supportsCameraOpaqueTexture=false;
        var serializedProfile=new SerializedObject(profile);
        serializedProfile.FindProperty("m_AdditionalLightsRenderingMode").intValue=(int)LightRenderingMode.PerPixel;
        serializedProfile.FindProperty("m_AdditionalLightShadowsSupported").boolValue=false;
        serializedProfile.FindProperty("m_UseSRPBatcher").boolValue=true;
        serializedProfile.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.CreateAsset(profile,profilePath);
        var display=new GameObject("Desktop presentation").AddComponent<DesktopDisplaySettings>();display.mediumPipeline=profile;
        PlayerSettings.productName="Virtual Reality Fire Safety Training System";
        PlayerSettings.companyName="Academic Fire Safety";
        PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.defaultIsNativeResolution=true;
        PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Target,true)};
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Physics.SyncTransforms();
        return "TrainingScene created with "+UnityEngine.Object.FindObjectsByType<Collider>().Length+" colliders. Source scene and materials unchanged.";
    }
}
