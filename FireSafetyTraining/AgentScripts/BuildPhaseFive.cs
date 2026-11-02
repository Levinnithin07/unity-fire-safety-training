using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using TMPro;
using FireSafety;
public static class BuildPhaseFive
{
    const string R="Assets/_FireSafety/";
    static Transform root;static Material metal,green,white,paving,wood,plaster,glass;static TMP_FontAsset font;static int meshId;
    static Material Solid(string name,Color color,float metallic=0,float smooth=.25f){var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};mat.SetColor("_BaseColor",color);mat.SetFloat("_Metallic",metallic);mat.SetFloat("_Smoothness",smooth);AssetDatabase.CreateAsset(mat,R+"Materials/"+name+".mat");return mat;}
    static Mesh BoxMesh(Vector3 s)
    {
        var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        foreach(var normal in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back}){
            Vector3 tangent=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,normal);Vector3 vertical=Vector3.Cross(normal,tangent);
            int n=v.Count;foreach(var p in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){var local=Vector3.Scale(normal+tangent*p.x+vertical*p.y,s)*.5f;v.Add(local);uv.Add(new Vector2(Vector3.Dot(local,tangent),Vector3.Dot(local,vertical))/1.6f);}
            tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
        }
        var mesh=new Mesh{name="Phase5 architectural mesh "+meshId++};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,R+"Models/"+mesh.name+".asset");return mesh;
    }
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material material,bool collider=true)
    {
        var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.GetComponent<MeshFilter>().sharedMesh=BoxMesh(size);g.GetComponent<MeshRenderer>().sharedMaterial=material;if(collider)g.AddComponent<BoxCollider>().size=size;return g;
    }
    static TextMeshPro Sign(string name,Transform parent,Vector3 pos,Vector2 size,float yaw,string words,Material back)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(0,yaw,0);
        Box("Powder coated sign case",g.transform,Vector3.zero,new Vector3(size.x+.06f,size.y+.06f,.045f),back,false);
        var t=new GameObject("Lettering",typeof(RectTransform),typeof(TextMeshPro)).GetComponent<TextMeshPro>();t.transform.SetParent(g.transform,false);t.transform.localPosition=Vector3.back*.027f;t.rectTransform.sizeDelta=size;t.font=font;t.fontSize=2.2f;t.text=words;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;t.enableAutoSizing=true;t.fontSizeMin=1;t.fontSizeMax=3;t.textWrappingMode=TextWrappingModes.Normal;return t;
    }
    static TMP_Text Label(string name,Transform parent,Vector2 min,Vector2 max,int size)
    {
        var t=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();t.transform.SetParent(parent,false);var rect=t.rectTransform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;t.font=font;t.fontSize=size;t.color=Color.white;t.raycastTarget=false;t.alignment=TextAlignmentOptions.MidlineLeft;return t;
    }
    static AudioClip Sound(string name,bool arrival)
    {
        int rate=22050,n=arrival?rate:rate/2;var rng=new System.Random(56);string path=R+"Audio/"+name+".wav";
        using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);for(int i=0;i<n;i++){float t=i/(float)rate;double v=arrival?Math.Sin(t*2*Math.PI*(t<.35?660:880))*.15*Math.Exp(-t*3):(rng.NextDouble()*2-1)*.18*Math.Exp(-t*12)+Math.Sin(t*2*Math.PI*85)*.12*Math.Exp(-t*8);w.Write((short)(Mathf.Clamp((float)v,-1,1)*32767));}}
        AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static void Paving()
    {
        string path="Assets/ThirdParty/PolyHavenPaving/hexagonal_concrete_paving_";
        foreach(string suffix in new[]{"diff_2k.jpg","nor_gl_2k.jpg","rough_2k.jpg"}){string p=path+suffix;var i=(TextureImporter)AssetImporter.GetAtPath(p);i.maxTextureSize=2048;i.mipmapEnabled=true;i.textureCompression=TextureImporterCompression.Compressed;i.sRGBTexture=suffix.StartsWith("diff");i.textureType=suffix.StartsWith("nor")?TextureImporterType.NormalMap:TextureImporterType.Default;i.isReadable=suffix.StartsWith("rough");i.SaveAndReimport();}
        var rough=new Texture2D(2,2,TextureFormat.RGBA32,false,true);rough.LoadImage(File.ReadAllBytes(path+"rough_2k.jpg"));var pix=rough.GetPixels32();for(int i=0;i<pix.Length;i++)pix[i]=new Color32(0,0,0,(byte)(255-pix[i].r));var smooth=new Texture2D(rough.width,rough.height,TextureFormat.RGBA32,false,true);smooth.SetPixels32(pix);smooth.Apply();string sp=R+"Textures/Courtyard metallic smoothness.png";File.WriteAllBytes(sp,smooth.EncodeToPNG());UnityEngine.Object.DestroyImmediate(smooth);UnityEngine.Object.DestroyImmediate(rough);AssetDatabase.ImportAsset(sp);
        var si=(TextureImporter)AssetImporter.GetAtPath(sp);si.sRGBTexture=false;si.maxTextureSize=2048;si.mipmapEnabled=true;si.textureCompression=TextureImporterCompression.Compressed;si.SaveAndReimport();var ri=(TextureImporter)AssetImporter.GetAtPath(path+"rough_2k.jpg");ri.isReadable=false;ri.SaveAndReimport();
        paving=Solid("Courtyard - Poly Haven concrete pavers",Color.white,0,.65f);paving.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path+"diff_2k.jpg"));paving.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path+"nor_gl_2k.jpg"));paving.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(sp));paving.EnableKeyword("_NORMALMAP");paving.EnableKeyword("_METALLICSPECGLOSSMAP");EditorUtility.SetDirty(paving);
    }
    public static string Run()
    {
        var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.isDirty||scene.path!=R+"Scenes/TrainingScene.unity")throw new Exception("Saved TrainingScene in Edit Mode required");if(UnityEngine.Object.FindAnyObjectByType<EvacuationManager>())throw new Exception("Phase 5 already created; use targeted edits");
        AssetDatabase.Refresh();font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();var e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();var p=m.player;
        var materials=UnityEngine.Object.FindObjectsByType<Renderer>().SelectMany(r=>r.sharedMaterials).Where(mat=>mat).ToArray();wood=materials.First(mat=>mat.name=="Classroom_WoodFloor");plaster=materials.First(mat=>mat.name=="Classroom_PaintedPlasterWall");glass=materials.First(mat=>mat.name=="Classroom_Glass");
        Paving();metal=Solid("Exit - anodized dark metal",new Color(.11f,.15f,.16f),.7f,.55f);green=Solid("Exit - safety green",new Color(.015f,.24f,.11f),.05f,.4f);green.EnableKeyword("_EMISSION");green.SetColor("_EmissionColor",new Color(.02f,.15f,.065f));white=Solid("Exit - lettering and markers",new Color(.78f,.84f,.80f),.1f,.45f);
        root=new GameObject("Phase 5 - evacuation and assembly").transform;Undo.RegisterCreatedObjectUndo(root.gameObject,"Build Phase 5");
        // The existing full-height glazing becomes an operable glazed exit; source FBX/material assets are untouched.
        var glazing=UnityEngine.Object.FindObjectsByType<MeshRenderer>().First(r=>r.name=="HallWindowGlass");glazing.gameObject.SetActive(false);
        var doorObject=new GameObject("Exterior glazed panic exit");doorObject.transform.SetParent(root,false);doorObject.transform.position=new Vector3(6.47f,0,-.2f);var door=doorObject.AddComponent<ExitDoor>();door.player=p;
        foreach(float z in new[]{-1.64f,1.64f})Box("Exit jamb",doorObject.transform,new Vector3(0,1.27f,z),new Vector3(.12f,2.55f,.07f),metal);
        Box("Exit header",doorObject.transform,new Vector3(0,2.51f,0),new Vector3(.12f,.07f,3.32f),metal);
        for(int side=0;side<2;side++){
            int sign=side==0?1:-1;var leaf=new GameObject(side==0?"Left outward leaf":"Right outward leaf");leaf.transform.SetParent(doorObject.transform,false);leaf.transform.localPosition=new Vector3(0,0,-sign*1.59f);if(side==0)door.leftLeaf=leaf.transform;else door.rightLeaf=leaf.transform;
            var mod=leaf.AddComponent<NavMeshModifier>();mod.ignoreFromBuild=true;
            foreach(float z in new[]{0,1.57f})Box("Door stile",leaf.transform,new Vector3(0,1.24f,sign*z),new Vector3(.07f,2.46f,.055f),metal,false);
            foreach(float y in new[]{.05f,.3f,1.1f,2.45f})Box("Door rail",leaf.transform,new Vector3(0,y,sign*.785f),new Vector3(.07f,.055f,1.57f),metal,false);
            Box("Glazed leaf",leaf.transform,new Vector3(0,1.25f,sign*.785f),new Vector3(.015f,2.36f,1.51f),glass,false);
            Box("Kick plate",leaf.transform,new Vector3(-.045f,.17f,sign*.785f),new Vector3(.025f,.23f,1.48f),metal,false);
            Box("Panic push bar",leaf.transform,new Vector3(-.11f,1.05f,sign*.785f),new Vector3(.075f,.055f,1.18f),white,false);
            foreach(float z in new[]{.23f,1.34f})Box("Push bar bracket",leaf.transform,new Vector3(-.073f,1.05f,sign*z),new Vector3(.1f,.12f,.055f),metal,false);
            var collider=leaf.AddComponent<BoxCollider>();collider.center=new Vector3(0,1.24f,sign*.785f);collider.size=new Vector3(.08f,2.46f,1.57f);
        }
        door.mechanism=doorObject.AddComponent<AudioSource>();door.mechanism.clip=Sound("Exit push bar - original",false);door.mechanism.playOnAwake=false;door.mechanism.spatialBlend=1;door.mechanism.minDistance=1;door.mechanism.maxDistance=8;door.mechanism.volume=.4f;
        Sign("Exterior exit sign",root,new Vector3(6.37f,2.77f,-.2f),new Vector2(1.8f,.32f),90,"EXIT  >   ASSEMBLY A",green);
        Sign("Push instruction",root,new Vector3(6.36f,1.6f,-.2f),new Vector2(.6f,.18f),90,"PUSH TO EXIT",green);
        // Outdoor assembly court, UV-scaled to the publisher's 1.6 m paving capture.
        var court=new GameObject("Outdoor assembly courtyard").transform;court.SetParent(root,false);
        Box("PBR paved court",court,new Vector3(12.25f,-.11f,-.2f),new Vector3(11.7f,.2f,10),paving);
        foreach(float z in new[]{-5.1f,4.7f}){
            Box("Perimeter masonry",court,new Vector3(12.25f,.32f,z),new Vector3(11.7f,.66f,.25f),plaster);
            Box("Masonry coping",court,new Vector3(12.25f,.68f,z),new Vector3(11.75f,.08f,.31f),paving);
            Box("Upper guard rail",court,new Vector3(12.25f,1.12f,z),new Vector3(11.75f,.045f,.045f),metal);
            for(float x=6.6f;x<18;x+=.65f)Box("Guard baluster",court,new Vector3(x,.91f,z),new Vector3(.022f,.43f,.022f),metal,false);
            var railBarrier=new GameObject("Perimeter collision").AddComponent<BoxCollider>();railBarrier.transform.SetParent(court,false);railBarrier.transform.position=new Vector3(12.25f,.8f,z);railBarrier.size=new Vector3(11.8f,1.6f,.12f);
        }
        Box("Far perimeter wall",court,new Vector3(18.05f,.8f,-.2f),new Vector3(.25f,1.6f,10),plaster);
        // Slatted outdoor benches and metal supports, clear of the evacuation path.
        foreach(float z in new[]{-3.9f,3.5f}){
            var bench=new GameObject("Courtyard slatted bench").transform;bench.SetParent(court,false);bench.position=new Vector3(12,.0f,z);
            for(int i=0;i<5;i++)Box("Timber seat slat",bench,new Vector3(0,.47f,(i-2)*.11f),new Vector3(2.3f,.055f,.09f),wood);
            for(int i=0;i<4;i++)Box("Timber back slat",bench,new Vector3(0,.65f+i*.11f,z<0?-.28f:.28f),new Vector3(2.3f,.09f,.045f),wood);
            foreach(float x in new[]{-.85f,.85f}){Box("Steel seat leg",bench,new Vector3(x,.235f,0),new Vector3(.07f,.47f,.42f),metal);Box("Steel back upright",bench,new Vector3(x,.7f,z<0?-.28f:.28f),new Vector3(.045f,.55f,.045f),metal,false);}
        }
        var assembly=new GameObject("Assembly point A");assembly.transform.SetParent(root,false);assembly.transform.position=new Vector3(14.2f,0,-.2f);var zone=assembly.AddComponent<BoxCollider>();zone.isTrigger=true;zone.center=Vector3.up;zone.size=new Vector3(2.8f,2,2.8f);
        foreach(float offset in new[]{-1.4f,1.4f}){Box("Assembly boundary marking",assembly.transform,new Vector3(offset,.006f,0),new Vector3(.07f,.012f,2.85f),green,false);Box("Assembly boundary marking",assembly.transform,new Vector3(0,.006f,offset),new Vector3(2.85f,.012f,.07f),green,false);}
        Box("Assembly sign post",root,new Vector3(15.8f,1.1f,-.2f),new Vector3(.06f,2.2f,.06f),metal);
        Sign("Assembly sign",root,new Vector3(15.72f,2,-.2f),new Vector2(1.6f,.9f),90,"ASSEMBLY POINT\nA\nSTAY HERE / REPORT IN",green);
        var arrival=assembly.AddComponent<AudioSource>();arrival.clip=Sound("Assembly arrival - original",true);arrival.playOnAwake=false;arrival.volume=.25f;
        var nav=root.gameObject.AddComponent<NavigationManager>();nav.assembly=assembly.transform;
        var hazards=new List<RouteHazard>();
        for(int i=0;i<2;i++){
            float z=i==0?-2.65f:3.02f;var go=new GameObject(i==0?"Front doorway smoke transport":"Rear doorway smoke transport");go.transform.SetParent(root,false);go.transform.position=new Vector3(3.3f,0,z);
            var hazard=go.AddComponent<RouteHazard>();hazard.simulation=m;hazard.routeName=i==0?"FRONT ROUTE":"REAR ROUTE";hazard.navigationArea=3+i;hazard.smokeTransport=i==0?1.35f:.45f;
            var volume=go.AddComponent<NavMeshModifierVolume>();volume.area=3+i;volume.center=Vector3.up;volume.size=new Vector3(1.4f,2.4f,1.45f);
            hazard.sign=Sign("Illuminated route sign",root,new Vector3(3.18f,2.67f,z),new Vector2(1.1f,.32f),90,hazard.routeName+"\nEXIT  >",green);
            var ps=new GameObject("Localized doorway smoke").AddComponent<ParticleSystem>();ps.transform.SetParent(go.transform,false);ps.transform.localPosition=new Vector3(0,1.7f,0);ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.playOnAwake=false;main.maxParticles=70;main.startLifetime=2.8f;main.startSize=new ParticleSystem.MinMaxCurve(.35f,.7f);main.startSpeed=.025f;main.startColor=new Color(.17f,.16f,.15f,.4f);main.simulationSpace=ParticleSystemSimulationSpace.Local;var em=ps.emission;em.rateOverTime=0;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(.65f,.65f,.85f);var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.2f),new GradientAlphaKey(0,1)});color.color=gradient;ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(R+"Materials/Fire - smoke.mat");hazard.smoke=ps;hazards.Add(hazard);
        }
        nav.routes=hazards.ToArray();
        var evac=root.gameObject.AddComponent<EvacuationManager>();evac.simulation=m;evac.extinguisher=e;evac.player=p;evac.navigation=nav;evac.exitDoor=door;evac.assemblyZone=zone;evac.arrivalSound=arrival;e.evacuation=evac;
        var existing=UnityEngine.Object.FindAnyObjectByType<EmergencyStatusUI>();existing.evacuation=evac;
        var ui=root.gameObject.AddComponent<EvacuationUI>();ui.evacuation=evac;ui.equipmentUI=UnityEngine.Object.FindAnyObjectByType<ExtinguisherStatusUI>();
        var panel=new GameObject("Evacuation route card",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(existing.GetComponent<Canvas>().transform,false);var rect=(RectTransform)panel.transform;rect.anchorMin=new Vector2(.60f,.74f);rect.anchorMax=new Vector2(.975f,.96f);rect.offsetMin=rect.offsetMax=Vector2.zero;panel.GetComponent<UnityEngine.UI.Image>().color=new Color(.015f,.07f,.055f,.94f);panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        ui.card=panel;ui.heading=Label("Evacuation heading",panel.transform,new Vector2(.04f,.77f),new Vector2(.96f,.95f),25);ui.detail=Label("Evacuation guidance",panel.transform,new Vector2(.04f,.32f),new Vector2(.96f,.7f),21);ui.timer=Label("Evacuation timer",panel.transform,new Vector2(.04f,.06f),new Vector2(.96f,.24f),21);ui.arrow=Label("Turn direction",panel.transform,new Vector2(.04f,.57f),new Vector2(.96f,.76f),23);ui.detail.rectTransform.anchorMax=new Vector2(.96f,.57f);
        var line=new GameObject("Floor evacuation guidance").AddComponent<LineRenderer>();line.transform.SetParent(root,false);line.sharedMaterial=Solid("Evacuation floor trail",new Color(.1f,.8f,.5f));line.sharedMaterial.EnableKeyword("_EMISSION");line.sharedMaterial.SetColor("_EmissionColor",new Color(.1f,.8f,.5f));line.widthMultiplier=.055f;line.numCapVertices=3;line.shadowCastingMode=ShadowCastingMode.Off;line.positionCount=0;line.useWorldSpace=true;ui.trail=line;panel.SetActive(false);
        var settings=NavMesh.CreateSettings();settings.agentRadius=.25f;settings.agentHeight=1.8f;settings.agentClimb=.15f;settings.agentSlope=45;var navSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);var agents=navSettings.FindProperty("m_Settings");for(int j=0;j<agents.arraySize;j++){var a=agents.GetArrayElementAtIndex(j);if(a.FindPropertyRelative("agentTypeID").intValue==settings.agentTypeID){a.FindPropertyRelative("agentRadius").floatValue=.25f;a.FindPropertyRelative("agentHeight").floatValue=1.8f;a.FindPropertyRelative("agentClimb").floatValue=.15f;a.FindPropertyRelative("agentSlope").floatValue=45;navSettings.FindProperty("m_SettingNames").GetArrayElementAtIndex(j).stringValue="Desktop trainee";}}var areas=navSettings.FindProperty("areas");areas.GetArrayElementAtIndex(3).FindPropertyRelative("name").stringValue="Front evacuation doorway";areas.GetArrayElementAtIndex(4).FindPropertyRelative("name").stringValue="Rear evacuation doorway";navSettings.ApplyModifiedPropertiesWithoutUndo();nav.agentType=settings.agentTypeID;
        var surface=new GameObject("Evacuation walkable surface").AddComponent<NavMeshSurface>();surface.transform.SetParent(root,false);surface.agentTypeID=settings.agentTypeID;surface.collectObjects=CollectObjects.Volume;surface.center=new Vector3(7.5f,1.4f,0);surface.size=new Vector3(24,4,12);surface.layerMask=1;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.overrideVoxelSize=true;surface.voxelSize=.055f;surface.overrideTileSize=true;surface.tileSize=128;surface.BuildNavMesh();if(!surface.navMeshData)throw new Exception("NavMesh bake failed");AssetDatabase.CreateAsset(surface.navMeshData,R+"Scenes/TrainingEvacuationNavMesh.asset");
        foreach(var route in hazards)route.Refresh();nav.Recalculate(p.transform.position);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=scene.path,navAvailable=nav.Available,route=nav.RouteName,distance=nav.Distance,navTriangles=NavMesh.CalculateTriangulation().indices.Length/3,agent=settings.agentTypeID});
    }
}


