using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FireSafety.EditorTools
{
    public static class ProjectAudit
    {
        [Serializable] public class ModelRecord
        {
            public string path;
            public int meshes, renderers, triangles;
            public string[] materials;
            public Vector3 center, size;
        }
        [Serializable] public class Audit
        {
            public string unityVersion, pipeline, generatedUtc;
            public bool windowsBuildSupported;
            public ModelRecord[] models;
            public int textures;
        }

        public static void Write()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var paths = AssetDatabase.FindAssets("t:Model", new[]{"Assets/ThirdParty"})
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
            var records = paths.Select(path => {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!model) throw new InvalidOperationException("Model did not import: " + path);
                var renderers = model.GetComponentsInChildren<Renderer>(true);
                var meshes = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().ToArray();
                var bounds = new Bounds();
                if (renderers.Length > 0) { bounds = renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds); }
                return new ModelRecord {
                    path=path, meshes=meshes.Length, renderers=renderers.Length,
                    triangles=meshes.Sum(m => Enumerable.Range(0,m.subMeshCount).Sum(s => (int)m.GetIndexCount(s)/3)),
                    materials=renderers.SelectMany(r=>r.sharedMaterials).Where(m=>m).Select(m=>m.name).Distinct().OrderBy(n=>n).ToArray(),
                    center=bounds.center, size=bounds.size
                };
            }).ToArray();
            var audit = new Audit {
                unityVersion=Application.unityVersion,
                pipeline=GraphicsSettings.defaultRenderPipeline ? GraphicsSettings.defaultRenderPipeline.GetType().FullName : "Built-in",
                generatedUtc=DateTime.UtcNow.ToString("O"),
                windowsBuildSupported=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64),
                models=records, textures=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/ThirdParty"}).Length
            };
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Documentation/ImportAudit.json"));
            File.WriteAllText(output,JsonUtility.ToJson(audit,true));
            AssetDatabase.SaveAssets();
            Debug.Log("[FireSafety] Import audit saved: " + output);
        }
    }
}
