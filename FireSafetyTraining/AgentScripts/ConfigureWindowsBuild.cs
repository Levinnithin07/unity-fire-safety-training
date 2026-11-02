using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using FireSafety;
public static class ConfigureWindowsBuild
{
    public static string Run()
    {
        if(BuildPipeline.isBuildingPlayer||EditorApplication.isPlaying)throw new Exception("Editor must be idle.");
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSettings>();
        var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        if(lights.Any(l=>l.cookie))throw new Exception("A light cookie is in use; review before stripping support.");
        foreach(var asset in s.pipelines){
            var so=new SerializedObject(asset);
            // Retain Forward+, SSAO, reflections and shadows. These optional systems are unused here.
            foreach(string field in new[]{"m_SupportsLightCookies","m_SupportsLightLayers","m_SupportsTerrainHoles","m_EnableLODCrossFade"})so.FindProperty(field).boolValue=false;
            so.FindProperty("m_GPUResidentDrawerMode").intValue=0;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);
        }
        var qualityAsset=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0];
        var quality=new SerializedObject(qualityAsset);var levels=quality.FindProperty("m_QualitySettings");levels.arraySize=3;
        for(int i=0;i<3;i++){
            var item=levels.GetArrayElementAtIndex(i);item.FindPropertyRelative("name").stringValue=new[]{"Low","Medium","High"}[i];
            item.FindPropertyRelative("customRenderPipeline").objectReferenceValue=s.pipelines[i];
            item.FindPropertyRelative("excludedTargetPlatforms").arraySize=0;
            item.FindPropertyRelative("vSyncCount").intValue=1;
        }
        quality.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(qualityAsset);
        QualitySettings.SetQualityLevel(1);GraphicsSettings.defaultRenderPipeline=s.pipelines[1];
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
        PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.defaultIsNativeResolution=true;
        AssetDatabase.SaveAssets();
        return "Windows DX11; actual Low/Medium/High pipelines registered for build; unused GPU Resident Drawer, light-layer/cookie, terrain-hole and LOD cross-fade variants disabled. Forward+, SSAO, PBR/reflections and shadow quality retained.";
    }
}
