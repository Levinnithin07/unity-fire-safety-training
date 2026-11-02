using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ValidatePhaseOne
{
    [Serializable] public class Report
    {
        public string generatedUtc, editorVersion, activePipeline;
        public int modelCount, textureCount, materialsInScene, missingMaterials, incompatibleMaterials;
        public int texturesExceeding2K, texturesWithoutMipmaps, normalsNotConfigured;
        public bool windowsBuildSupported, inputSystemInstalled;
        public string scope="Phase 1 asset import and editor checks only. Gameplay, final lighting, collision navigation and performance are not tested.";
    }
    public static string Run()
    {
        var rendererMaterials=UnityEngine.Object.FindObjectsByType<Renderer>().SelectMany(r=>r.sharedMaterials).ToArray();
        var textures=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/ThirdParty"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var importers=textures.Select(p=>(TextureImporter)AssetImporter.GetAtPath(p)).ToArray();
        var r=new Report {
            generatedUtc=DateTime.UtcNow.ToString("O"),editorVersion=Application.unityVersion,
            activePipeline=GraphicsSettings.currentRenderPipeline?.GetType().FullName,
            modelCount=AssetDatabase.FindAssets("t:Model",new[]{"Assets/ThirdParty"}).Length,
            textureCount=textures.Length,materialsInScene=rendererMaterials.Where(m=>m).Distinct().Count(),
            missingMaterials=rendererMaterials.Count(m=>!m),
            incompatibleMaterials=rendererMaterials.Where(m=>m).Distinct().Count(m=>!m.shader || !m.shader.name.StartsWith("Universal Render Pipeline/")),
            texturesExceeding2K=importers.Count(t=>t.maxTextureSize>2048),
            texturesWithoutMipmaps=importers.Count(t=>!t.mipmapEnabled),
            normalsNotConfigured=importers.Count(t=>(t.assetPath.ToLowerInvariant().Contains("_normal") || t.assetPath.Contains("_nor_gl")) && t.textureType!=TextureImporterType.NormalMap),
            windowsBuildSupported=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64),
            inputSystemInstalled=UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p=>p.name=="com.unity.inputsystem")
        };
        string json=JsonUtility.ToJson(r,true);
        File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../Documentation/Phase1Validation.json")),json);
        if(r.missingMaterials+r.incompatibleMaterials+r.texturesExceeding2K+r.texturesWithoutMipmaps+r.normalsNotConfigured>0)
            throw new InvalidOperationException("Phase 1 asset checks failed; see Phase1Validation.json");
        return json;
    }
}
