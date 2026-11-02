using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using FireSafety;
public static class RefinePhaseThree
{
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Edit Mode required");
        var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();
        if(!m)throw new Exception("Phase 3 scene not open");
        Vector3[] bases={new Vector3(-1.951f,.805f,.068f),new Vector3(-1.860f,.805f,.880f),new Vector3(-1.925f,.805f,1.728f)};
        for(int i=0;i<m.sources.Length;i++){
            var f=m.sources[i];f.transform.position=bases[i];
            var smoke=f.smoke.main;smoke.startColor=new Color(.16f,.15f,.14f,.9f);smoke.startSize=.8f;
            var flame=f.flames.main;flame.startColor=new Color(1,.32f,.025f,1);flame.startSpeed=.95f;
        }
        var mat=m.sources[0].flames.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        mat.SetFloat("_Blend",0);mat.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
        mat.SetColor("_BaseColor",new Color(3,3,3,1));
        EditorUtility.SetDirty(mat);
        foreach(var detector in m.detectors){
            var vents=detector.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Optical chamber vent").ToArray();
            if(vents.Length>1){
                var combined=new Mesh(){name=detector.inPassage?"Passage detector vents":"Classroom detector vents"};
                combined.CombineMeshes(vents.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=detector.transform.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());
                AssetDatabase.CreateAsset(combined,"Assets/_FireSafety/Models/"+combined.name+".asset");
                var go=new GameObject("Combined ventilation ring",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(detector.transform,false);
                go.GetComponent<MeshFilter>().sharedMesh=combined;go.GetComponent<MeshRenderer>().sharedMaterial=vents[0].GetComponent<Renderer>().sharedMaterial;
                foreach(var vent in vents)UnityEngine.Object.DestroyImmediate(vent.gameObject);
                PrefabUtility.SaveAsPrefabAsset(detector.gameObject,"Assets/_FireSafety/Prefabs/"+(detector.inPassage?"PassageSmokeDetector":"ClassroomSmokeDetector")+".prefab");
            }
        }
        AssetDatabase.ForceReserializeAssets(new[]{"Assets/TextMesh Pro/Fonts/LiberationSans.ttf"},ForceReserializeAssetsOptions.ReserializeMetadata);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        return "Saved refined particle opacity, flame blending, desk contact positions, and upgraded included font metadata.";
    }
}
