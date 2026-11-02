using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PrepareAssetReview
{
    const string Out = "Assets/_FireSafety/Materials/Imported";
    static Texture2D Texture(string root, string name) => AssetDatabase.FindAssets("t:Texture2D", new[]{root})
        .Select(AssetDatabase.GUIDToAssetPath).Where(p=>Path.GetFileNameWithoutExtension(p)==name)
        .Select(AssetDatabase.LoadAssetAtPath<Texture2D>).FirstOrDefault();

    static Material Make(string name)
    {
        string path=Out+"/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material) { material=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material,path); }
        material.SetFloat("_Smoothness",0.35f);
        return material;
    }

    static Material ClassroomMaterial(string name)
    {
        var m=Make("Classroom_"+name);
        string root="Assets/ThirdParty/Classroom/Textures/Classroom";
        string group=name;
        if (name=="BlurredMetal" || name=="CeilingVent") group="BareMetal";
        if (name=="FrostedGlass") group="Glass";
        var albedo=Texture(root,group+"_BaseColor") ?? Texture(root,group+"_BaseMap");
        var normal=Texture(root,group+"_Normal");
        var mask=Texture(root,group+"_MaskMap");
        if(albedo) m.SetTexture("_BaseMap",albedo);
        if(normal) { m.SetTexture("_BumpMap",normal); m.EnableKeyword("_NORMALMAP"); }
        if(mask) {
            // HDRP mask R=metallic, G=AO, A=smoothness also matches the channels used here.
            m.SetTexture("_MetallicGlossMap",mask); m.SetTexture("_OcclusionMap",mask);
            m.EnableKeyword("_METALLICSPECGLOSSMAP"); m.EnableKeyword("_OCCLUSIONMAP");
            m.SetFloat("_Smoothness",0.7f);
        }
        if(name=="DeskPlastic") m.SetColor("_BaseColor",new Color(0.12f,0.19f,0.20f));
        if(name=="Plastics") m.SetColor("_BaseColor",new Color(0.22f,0.24f,0.23f));
        if(name=="RadiatorGlossyPaint") { m.SetColor("_BaseColor",new Color(0.74f,0.72f,0.64f)); m.SetFloat("_Smoothness",0.55f); }
        if(name=="Glass" || name=="FrostedGlass") {
            m.SetFloat("_Surface",1); m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite",0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue=3000;
            m.SetColor("_BaseColor",new Color(0.7f,0.83f,0.85f,name=="Glass"?0.16f:0.45f));
            m.SetFloat("_Smoothness",0.8f);
        }
        if(name=="CeilingLampLight") {
            m.SetColor("_BaseColor",new Color(0.85f,0.87f,0.82f)); m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor",new Color(1.1f,1.2f,1.05f));
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material PolyMaterial(string name,string root)
    {
        var m=Make(name);
        var color=Texture(root,name+"_diff_2k");
        var normal=Texture(root,name+"_nor_gl_2k");
        var metal=Texture(root,name+"_metal_2k");
        if(color) m.SetTexture("_BaseMap",color);
        if(normal) {m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
        if(metal) {m.SetTexture("_MetallicGlossMap",metal);m.EnableKeyword("_METALLICSPECGLOSSMAP");}
        m.SetFloat("_Smoothness",0.3f);
        EditorUtility.SetDirty(m);
        return m;
    }

    public static string Build()
    {
        Directory.CreateDirectory(Out);
        AssetDatabase.Refresh();
        string[] paths=AssetDatabase.FindAssets("t:Model",new[]{"Assets/ThirdParty"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        foreach(var path in paths) {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            // Map every imported material explicitly to URP and retain the original mesh.
            foreach(var old in model.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m).Distinct()) {
                string name=old.name.Replace("Classroom_","");
                Material material=path.Contains("Classroom")?ClassroomMaterial(name):PolyMaterial(name,Path.GetDirectoryName(path).Replace('\\','/'));
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),name),material);
            }
            importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        scene.name="AssetReview";
        var classroomPath=paths.First(p=>p.Contains("Classroom"));
        var room=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(classroomPath));
        room.name="Classroom - licensed source art";
        var cameraObject=new GameObject("Asset Review Camera");
        var cam=cameraObject.AddComponent<Camera>(); cameraObject.tag="MainCamera";
        cam.transform.position=new Vector3(4.5f,1.7f,-3.2f);
        cam.transform.LookAt(new Vector3(0,1.3f,1.7f)); cam.fieldOfView=68; cam.nearClipPlane=0.05f;
        cam.backgroundColor=new Color(0.55f,0.65f,0.72f); cam.clearFlags=CameraClearFlags.SolidColor;
        cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(0.6f,0.65f,0.72f);
        RenderSettings.ambientEquatorColor=new Color(0.42f,0.45f,0.47f);
        RenderSettings.ambientGroundColor=new Color(0.23f,0.22f,0.2f);
        var sun=new GameObject("Daylight").AddComponent<Light>();sun.type=LightType.Directional;
        sun.intensity=1.2f;sun.color=new Color(1,0.94f,0.83f);sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadows=LightShadows.Soft;
        for(int z=-1;z<=1;z+=2) for(int x=-1;x<=1;x+=2) {
            var lamp=new GameObject("Ceiling fill").AddComponent<Light>();lamp.type=LightType.Point;
            lamp.transform.position=new Vector3(1.4f+x*2.4f,2.75f,z*2.3f);
            lamp.range=8;lamp.intensity=1.7f;lamp.color=new Color(0.94f,0.96f,1);lamp.shadows=LightShadows.None;
        }
        foreach(var path in paths.Where(p=>!p.Contains("Classroom"))) {
            var item=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            item.transform.position=path.Contains("extinguisher")?new Vector3(4,0,2.6f):new Vector3(4.3f,1.3f,2.6f);
        }
        EditorSceneManager.SaveScene(scene,"Assets/_FireSafety/Scenes/AssetReview.unity");
        AssetDatabase.SaveAssets();
        return "AssetReview saved; imported models mapped to URP. This is an art inspection scene, not gameplay.";
    }
}
