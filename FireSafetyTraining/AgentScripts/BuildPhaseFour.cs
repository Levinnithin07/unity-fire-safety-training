using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using FireSafety;
using Newtonsoft.Json.Linq;
public static class BuildPhaseFour
{
    const string Root="Assets/_FireSafety/";
    static int layer;
    static Mesh source;static Transform original;static Dictionary<int,int> island;
    static Mesh Extract(string name,int[] parts,bool exclude=false,Vector3 pivot=default)
    {
        var oldVertices=source.vertices;var oldNormals=source.normals;var oldUv=source.uv;var oldTangents=source.tangents;
        var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tangents=new List<Vector4>();
        var map=new Dictionary<int,int>();var triangles=new List<int>[source.subMeshCount];
        for(int s=0;s<source.subMeshCount;s++){
            triangles[s]=new List<int>();var indices=source.GetTriangles(s);
            for(int i=0;i<indices.Length;i+=3){
                bool match=parts.Contains(island[indices[i]]);if(match==exclude)continue;
                for(int k=0;k<3;k++){int old=indices[i+k];if(!map.TryGetValue(old,out int n)){
                    n=vertices.Count;map[old]=n;vertices.Add(original.TransformPoint(oldVertices[old])-original.position-pivot);
                    normals.Add(original.TransformDirection(oldNormals[old]).normalized);uv.Add(oldUv[old]);
                    if(oldTangents.Length==oldVertices.Length){var t=oldTangents[old];var dir=original.TransformDirection(new Vector3(t.x,t.y,t.z)).normalized;tangents.Add(new Vector4(dir.x,dir.y,dir.z,t.w));}
                }triangles[s].Add(n);}
            }
        }
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);if(tangents.Count==vertices.Count)mesh.SetTangents(tangents);
        mesh.subMeshCount=source.subMeshCount;for(int s=0;s<triangles.Length;s++)mesh.SetTriangles(triangles[s],s);mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Root+"Models/"+name+".asset");return mesh;
    }
    static GameObject Part(string name,Transform parent,Mesh mesh,Material[] materials,Vector3 pos=default)
    {
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.layer=layer;go.transform.SetParent(parent,false);go.transform.localPosition=pos;
        go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterials=materials;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return go;
    }
    static Mesh Hose()
    {
        var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
        Vector3 a=new Vector3(.226f,-.044f,.535f),b=new Vector3(.46f,-.35f,.55f),c=new Vector3(.28f,-.43f,.78f),d=new Vector3(.17f,-.13f,.74f);
        Func<float,Vector3> point=u=>Mathf.Pow(1-u,3)*a+3*(1-u)*(1-u)*u*b+3*(1-u)*u*u*c+u*u*u*d;
        for(int i=0;i<=32;i++){
            float f=i/32f;var p=point(f);var tangent=(point(Mathf.Min(1,f+.01f))-point(Mathf.Max(0,f-.01f))).normalized;
            var right=Vector3.Cross(tangent,Vector3.forward).normalized;var up=Vector3.Cross(right,tangent).normalized;
            for(int j=0;j<=10;j++){float angle=j*Mathf.PI*2/10;v.Add(p+.006f*(right*Mathf.Cos(angle)+up*Mathf.Sin(angle)));uv.Add(new Vector2(j/10f,f*6));
                if(i<32&&j<10){int n=i*11+j;t.AddRange(new[]{n,n+1,n+11,n+1,n+12,n+11});}}
        }
        var mesh=new Mesh{name="Extinguisher deployed hose"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Root+"Models/Extinguisher deployed hose.asset");return mesh;
    }
    static AudioClip Sound(string name,bool click)
    {
        int rate=22050,count=click?4410:rate*4;var random=new System.Random(148);float low=0;
        string path=Root+"Audio/"+name+".wav";
        using(var writer=new BinaryWriter(File.Create(path))){
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
            for(int i=0;i<count;i++){float time=i/(float)rate;float noise=(float)random.NextDouble()*2-1;low=low*.82f+noise*.18f;float value=click?(noise*.3f+Mathf.Sin(time*7000)*.15f)*Mathf.Exp(-time*35):noise*.20f+low*.55f;
                value*=Mathf.Clamp01(Mathf.Min(i,count-1-i)/150f);writer.Write((short)(value*32767));}
        }
        AssetDatabase.ImportAsset(path);var importer=(AudioImporter)AssetImporter.GetAtPath(path);importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static TMP_Text Text(string name,Transform parent,string value,int size,Vector2 min,Vector2 max)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
        var text=go.AddComponent<TextMeshProUGUI>();text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");text.text=value;text.fontSize=size;text.color=Color.white;text.alignment=TextAlignmentOptions.MidlineLeft;text.enableAutoSizing=false;text.raycastTarget=false;return text;
    }
    static UnityEngine.UI.Image Panel(string name,Transform parent,Vector2 min,Vector2 max,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
        var image=go.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return image;
    }
    public static string Run()
    {
        var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.isDirty||scene.path!=Root+"Scenes/TrainingScene.unity")throw new Exception("Saved TrainingScene in Edit Mode required.");
        if(UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>())throw new Exception("Phase 4 already exists; use targeted updates.");
        var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();var player=m.player;var camera=player.view.GetComponent<Camera>();
        var pickup=GameObject.Find("korean_fire_extinguisher_01_2k");original=pickup.transform;source=pickup.GetComponent<MeshFilter>().sharedMesh;
        var groups=JArray.Parse(File.ReadAllText(Path.GetFullPath("../Documentation/Phase4ExtinguisherIslands.json")));island=new Dictionary<int,int>();
        foreach(var g in groups)foreach(int i in g["indices"].Values<int>())island[i]=(int)g["id"];
        var materials=pickup.GetComponent<Renderer>().sharedMaterials;
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");layer=-1;
        for(int i=8;i<32;i++)if(string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)){layer=i;break;}
        if(layer<0)throw new Exception("No free viewmodel layer");layers.GetArrayElementAtIndex(layer).stringValue="FirstPersonEquipment";tags.ApplyModifiedPropertiesWithoutUndo();
        var root=new GameObject("Phase 4 - PASS extinguisher");Undo.RegisterCreatedObjectUndo(root,"Add Phase 4");
        var controller=root.AddComponent<FireExtinguisherController>();controller.simulation=m;controller.player=player;controller.pickupObject=pickup;controller.pickupRenderer=pickup.GetComponent<Renderer>();
        var held=new GameObject("Held extinguisher assembly");held.transform.SetParent(player.view,false);held.layer=layer;controller.heldRoot=held;
        var bottle=new GameObject("Bottle and working controls");bottle.transform.SetParent(held.transform,false);bottle.transform.localPosition=new Vector3(.28f,-.62f,.57f);bottle.transform.localRotation=Quaternion.Euler(0,90,0);bottle.layer=layer;
        Part("Licensed extinguisher body",bottle.transform,Extract("Extinguisher held body",new[]{2,3,4,6,10,13},true),materials);
        controller.pinObject=Part("Removable source safety pin",bottle.transform,Extract("Extinguisher safety pin",new[]{2}),materials);
        Vector3 pivot=new Vector3(.0297f,.5945f,-.026f);
        controller.trigger=Part("Squeeze lever",bottle.transform,Extract("Extinguisher squeeze lever",new[]{10},false,pivot),materials,pivot).transform;
        var nozzle=Part("Deployed source nozzle",held.transform,Extract("Extinguisher nozzle",new[]{13},false,new Vector3(.0327f,.1914f,-.1041f)),materials,new Vector3(.17f,-.13f,.77f));nozzle.transform.localRotation=Quaternion.Euler(-90,0,0);
        var rubber=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Extinguisher rubber hose"};rubber.SetColor("_BaseColor",new Color(.025f,.03f,.035f));rubber.SetFloat("_Smoothness",.3f);AssetDatabase.CreateAsset(rubber,Root+"Materials/Extinguisher rubber hose.mat");
        Part("Flexible deployed hose",held.transform,Hose(),new[]{rubber});
        var muzzle=new GameObject("Nozzle discharge origin");muzzle.transform.SetParent(held.transform,false);muzzle.transform.localPosition=new Vector3(.17f,-.13f,.813f);controller.nozzle=muzzle.transform;
        var overlayObject=new GameObject("Equipment overlay camera");overlayObject.transform.SetParent(player.view,false);overlayObject.layer=2;
        var overlay=overlayObject.AddComponent<Camera>();overlay.cullingMask=1<<layer;overlay.fieldOfView=camera.fieldOfView;overlay.nearClipPlane=.025f;overlay.farClipPlane=3;overlay.clearFlags=CameraClearFlags.Depth;overlay.allowHDR=true;
        var data=overlayObject.AddComponent<UniversalAdditionalCameraData>();data.renderType=CameraRenderType.Overlay;data.renderShadows=false;
        var cameraSettings=new SerializedObject(data);cameraSettings.FindProperty("m_ClearDepth").boolValue=true;cameraSettings.ApplyModifiedPropertiesWithoutUndo();
        camera.cullingMask&=~(1<<layer);camera.GetUniversalAdditionalCameraData().cameraStack.Add(overlay);
        var particles=new GameObject("Extinguishing agent plume").AddComponent<ParticleSystem>();particles.transform.SetParent(root.transform,false);particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=particles.main;main.playOnAwake=false;main.loop=true;main.maxParticles=130;main.startLifetime=.5f;main.startSpeed=8;main.startSize=new ParticleSystem.MinMaxCurve(.055f,.13f);main.startColor=new Color(.93f,.91f,.83f,.55f);main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=particles.emission;emission.rateOverTime=210;var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=5;shape.radius=.012f;
        var size=particles.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.4f),new Keyframe(1,2.3f)));
        var fade=particles.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.5f,.7f),new GradientAlphaKey(0,1)});fade.color=gradient;
        var particleMaterial=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Fire - smoke.mat"));particleMaterial.name="Extinguishing agent powder";AssetDatabase.CreateAsset(particleMaterial,Root+"Materials/Extinguishing agent powder.mat");
        var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;controller.discharge=particles;
        controller.spraySound=root.AddComponent<AudioSource>();controller.spraySound.clip=Sound("Extinguisher discharge - original",false);controller.spraySound.loop=true;controller.spraySound.playOnAwake=false;controller.spraySound.volume=.4f;controller.spraySound.spatialBlend=0;
        var pinAudio=new GameObject("Pin audio").AddComponent<AudioSource>();pinAudio.transform.SetParent(root.transform,false);pinAudio.clip=Sound("Extinguisher pin - original",true);pinAudio.volume=.4f;pinAudio.playOnAwake=false;controller.pinSound=pinAudio;
        var emergency=UnityEngine.Object.FindAnyObjectByType<EmergencyStatusUI>();emergency.extinguisher=controller;
        var canvas=emergency.GetComponent<Canvas>().transform;var ui=root.AddComponent<ExtinguisherStatusUI>();ui.extinguisher=controller;
        var panel=Panel("PASS technique card",canvas,new Vector2(.025f,.16f),new Vector2(.26f,.48f),new Color(.025f,.045f,.055f,.94f));ui.techniquePanel=panel.gameObject;
        Text("PASS title",panel.transform,"EXTINGUISHER / PASS",22,new Vector2(.05f,.84f),new Vector2(.96f,.96f));
        ui.pass=Text("PASS progress",panel.transform,"P Pull\nA Aim\nS Squeeze\nS Sweep",24,new Vector2(.05f,.36f),new Vector2(.96f,.83f));
        ui.capacity=Text("Agent capacity",panel.transform,"AGENT 100%",20,new Vector2(.05f,.25f),new Vector2(.95f,.36f));
        Panel("Agent bar background",panel.transform,new Vector2(.05f,.21f),new Vector2(.95f,.235f),new Color(.1f,.16f,.18f));
        ui.fill=Panel("Agent fill",panel.transform,new Vector2(.05f,.21f),new Vector2(.95f,.235f),new Color(.3f,.85f,.75f));ui.fill.type=UnityEngine.UI.Image.Type.Filled;ui.fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
        // A white 1px sprite gives the uGUI Filled image a valid fill geometry.
        var pixel=new Texture2D(1,1);pixel.SetPixel(0,0,Color.white);pixel.Apply();File.WriteAllBytes(Root+"UI/WhitePixel.png",pixel.EncodeToPNG());UnityEngine.Object.DestroyImmediate(pixel);AssetDatabase.ImportAsset(Root+"UI/WhitePixel.png");
        var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"UI/WhitePixel.png");importer.textureType=TextureImporterType.Sprite;importer.mipmapEnabled=false;importer.SaveAndReimport();ui.fill.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"UI/WhitePixel.png");
        ui.accuracy=Text("Handling feedback",panel.transform,"P Pull pin | LMB Spray",18,new Vector2(.05f,.025f),new Vector2(.96f,.19f));
        var prompt=Panel("Interaction prompt",canvas,new Vector2(.30f,.105f),new Vector2(.87f,.175f),new Color(.025f,.045f,.055f,.88f));
        ui.prompt=Text("Interaction text",prompt.transform,"Find the extinguisher in the passage",22,new Vector2(.025f,.05f),new Vector2(.98f,.95f));
        ui.crosshair=Panel("Aim dot",canvas,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Color.white);ui.crosshair.rectTransform.sizeDelta=new Vector2(5,5);
        held.SetActive(false);panel.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Phase 4 saved: licensed mesh split into working controls, viewmodel overlay, pickup, PASS controller, powder and responsive UI.";
    }
}
