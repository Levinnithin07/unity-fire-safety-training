using System;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEngine.Rendering;using UnityEngine.AI;using Unity.AI.Navigation;
using UnityEditor;using UnityEditor.SceneManagement;using TMPro;using Newtonsoft.Json.Linq;
public static class EnhanceCampusSports {
 const string R="Assets/_FireSafety/CampusSports/",C="Assets/_FireSafety/CampusExterior/";
 static Transform root;static Material blue,teal,clay,white,metal,padding,stone,wood;static int meshIndex;
 class Batch {public Transform parent;public Material material;public List<Vector3> v=new();public List<Vector2> uv=new();public List<int> t=new();}
 static Dictionary<string,Batch> batches=new();
 static Transform Group(string n){var g=new GameObject(n);g.transform.SetParent(root,false);return g.transform;}
 static Batch Get(Transform p,Material m){string k=p.name+"|"+m.name;if(!batches.TryGetValue(k,out var b)){b=new Batch{parent=p,material=m};batches[k]=b;}return b;}
 static void Quad(Batch b,Vector3 a,Vector3 c,Vector3 d,Vector3 e){int n=b.v.Count;b.v.AddRange(new[]{a,c,d,e});b.uv.AddRange(new[]{new Vector2(a.x,a.z),new Vector2(c.x,c.z),new Vector2(d.x,d.z),new Vector2(e.x,e.z)});b.t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
 static void Box(Transform p,string name,Vector3 pos,Vector3 size,Material m,bool collider=false){
  var b=Get(p,m);foreach(var n in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back}){
   var a=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);var c=Vector3.Cross(n,a);
   var q=new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}.Select(v=>pos+Vector3.Scale(n+a*v.x+c*v.y,size)*.5f).ToArray();Quad(b,q[0],q[1],q[2],q[3]);}
  if(collider){var g=new GameObject(name);g.transform.SetParent(p,false);g.transform.position=pos;g.AddComponent<BoxCollider>().size=size;}
 }
 static void Tube(Transform p,Vector3 a,Vector3 b,float radius,Material m,int sides=10){
  var batch=Get(p,m);var axis=(b-a).normalized;var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;var v=Vector3.Cross(axis,u);
  for(int i=0;i<sides;i++){float t=i*Mathf.PI*2/sides,t2=(i+1)*Mathf.PI*2/sides;var r=(u*Mathf.Cos(t)+v*Mathf.Sin(t))*radius;var s=(u*Mathf.Cos(t2)+v*Mathf.Sin(t2))*radius;Quad(batch,a+r,a+s,b+s,b+r);}
 }
 static void Line(Transform p,Vector3 a,Vector3 b,float width,Material m){
  var d=Vector3.Cross(Vector3.up,(b-a).normalized)*width*.5f;Quad(Get(p,m),a+d,a-d,b-d,b+d);
 }
 static void Arc(Transform p,Vector3 center,float radius,float from,float to,float width){for(int i=0;i<64;i++){float a=Mathf.Lerp(from,to,i/64f)*Mathf.Deg2Rad,b=Mathf.Lerp(from,to,(i+1)/64f)*Mathf.Deg2Rad;Line(p,center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius,center+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*radius,width,white);}}
 static void Rect(Transform p,float x,float z,float w,float h,float y,float line){var a=new Vector3(x-w/2,y,z-h/2);var b=new Vector3(x+w/2,y,z-h/2);var c=new Vector3(x+w/2,y,z+h/2);var d=new Vector3(x-w/2,y,z+h/2);Line(p,a,b,line,white);Line(p,b,c,line,white);Line(p,c,d,line,white);Line(p,d,a,line,white);}
 static Material Mat(string name,Color color,float smooth=.25f,float metallic=0,Material textureSource=null){
  var m=textureSource?new Material(textureSource):new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);m.enableInstancing=true;AssetDatabase.CreateAsset(m,R+"Materials/"+name+".mat");return m;
 }
 static void Flush(){foreach(var b in batches.Values){var m=new Mesh{name=b.parent.name+" "+b.material.name,indexFormat=IndexFormat.UInt32};m.SetVertices(b.v);m.SetUVs(0,b.uv);m.SetTriangles(b.t,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();AssetDatabase.CreateAsset(m,R+"Meshes/Detail-"+meshIndex+++".asset");var go=new GameObject(m.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(b.parent,false);go.GetComponent<MeshFilter>().sharedMesh=m;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=b.material;go.layer=30;GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);}}
 static GameObject Import(string name){
  string folder="Assets/ThirdParty/CampusSports/"+name+"/";var mats=JArray.Parse(File.ReadAllText(folder+"materials.json")).Select((x,i)=>{
   var color=x["color"].ToObject<float[]>();var m=Mat(name+" "+i,new Color(color[0],color[1],color[2],color[3]),(float)x["smoothness"],(float)x["metallic"]);
   if(x["texture"].Type!=JTokenType.Null){string path=folder+(string)x["texture"];var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=512;importer.mipmapEnabled=true;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));}
   if((bool)x["glass"]){m.SetColor("_BaseColor",new Color(.65f,.82f,.87f,.26f));m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=3000;}m.SetFloat("_Cull",0);return m;}).ToArray();
  var go=new GameObject(name+" - Innerscene CC0");using(var br=new BinaryReader(File.OpenRead(folder+"mesh.bin.bytes"))){int count=br.ReadInt32();for(int j=0;j<count;j++){int n=br.ReadInt32(),t=br.ReadInt32();var vs=new Vector3[n];var ns=new Vector3[n];var uv=new Vector2[n];for(int i=0;i<n;i++){vs[i]=new Vector3(br.ReadSingle(),br.ReadSingle(),br.ReadSingle());ns[i]=new Vector3(br.ReadSingle(),br.ReadSingle(),br.ReadSingle());uv[i]=new Vector2(br.ReadSingle(),br.ReadSingle());}var ids=new int[t];for(int i=0;i<t;i++)ids[i]=br.ReadInt32();var mesh=new Mesh{name=name+" material "+j,indexFormat=IndexFormat.UInt32};mesh.vertices=vs;mesh.normals=ns;mesh.uv=uv;mesh.triangles=ids;mesh.RecalculateBounds();mesh.RecalculateTangents();AssetDatabase.CreateAsset(mesh,R+"Meshes/"+name+"-"+j+".asset");var part=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(go.transform,false);part.layer=30;part.GetComponent<MeshFilter>().sharedMesh=mesh;part.GetComponent<Renderer>().sharedMaterial=mats[j];}}
  var prefab=PrefabUtility.SaveAsPrefabAsset(go,R+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return prefab;
 }
 static Bounds BoundsOf(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
 static string ObjPath(Transform t)=>t.parent?ObjPath(t.parent)+"/"+t.name:t.name;
 static object Protected()=>new{
  gameplay=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Where(c=>c&&c.GetType().Namespace=="FireSafety").Select(c=>new{path=ObjPath(c.transform),type=c.GetType().Name,json=EditorJsonUtility.ToJson(c)}).OrderBy(c=>c.path).ThenBy(c=>c.type).ToArray(),
  classroom=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t=>t.name=="Classroom - licensed source art"||t.name.EndsWith(" - environment only")).SelectMany(t=>t.GetComponentsInChildren<Component>(true)).Where(c=>c).Select(c=>new{id=GlobalObjectId.GetGlobalObjectIdSlow(c).ToString(),json=EditorJsonUtility.ToJson(c)}).OrderBy(c=>c.id).ToArray(),
  assembly=EditorJsonUtility.ToJson(UnityEngine.Object.FindAnyObjectByType<FireSafety.EvacuationManager>().assemblyZone)};
 public static object Run(){
  var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.isDirty||GameObject.Find("Campus sports and recreation"))throw new Exception("Saved original scene required");
  string backup="PhaseBackups/CampusSports-20261020";Directory.CreateDirectory(backup);File.Copy(scene.path,backup+"/TrainingScene.unity",false);File.Copy(scene.path+".meta",backup+"/TrainingScene.unity.meta",false);File.Copy("Assets/_FireSafety/SchoolExpansion/Navigation/SchoolNavMesh.asset",backup+"/SchoolNavMesh.asset",false);
  string before=Newtonsoft.Json.JsonConvert.SerializeObject(Protected());File.WriteAllText("../Documentation/CampusSportsProtectedBefore.json",before);
  foreach(var f in new[]{"Materials","Meshes","Prefabs"})Directory.CreateDirectory(R+f);AssetDatabase.Refresh();
  root=new GameObject("Campus sports and recreation").transform;
  var title=GameObject.Find("University lettering").GetComponent<TextMeshPro>();Undo.RecordObject(title,"Center university name");Undo.RecordObject(title.transform,"Center university name");title.text="SATHYABAMA UNIVERSITY";title.transform.position=new Vector3(7.28f,5.93f,-.2f);title.fontSize=5.0f;title.characterSpacing=1;title.rectTransform.sizeDelta=new Vector2(7.5f,.85f);title.ForceMeshUpdate();
  var subtitle=GameObject.Find("Academic block subtitle");Undo.RecordObject(subtitle,"Remove subtitle");subtitle.SetActive(false);
  var asphalt=AssetDatabase.LoadAssetAtPath<Material>(C+"Materials/Campus asphalt.mat");
  blue=Mat("Sports mineral blue",new Color(.12f,.29f,.36f),.18f,0,asphalt);
  teal=Mat("Sports muted jade",new Color(.25f,.40f,.36f),.18f,0,asphalt);
  clay=Mat("Sports warm clay",new Color(.56f,.30f,.22f),.18f,0,asphalt);
  white=Mat("Court line ivory",new Color(.94f,.94f,.85f),.15f);
  metal=AssetDatabase.LoadAssetAtPath<Material>(C+"Materials/Campus charcoal powdercoat.mat");
  stone=AssetDatabase.LoadAssetAtPath<Material>(C+"Materials/Campus limestone trim.mat");
  wood=AssetDatabase.LoadAssetAtPath<Material>(C+"Materials/Campus teak.mat");
  padding=Mat("Impact padding navy",new Color(.055f,.13f,.19f),.18f);
  var hoop=Import("basketball-hoop");var net=Import("volleyball-net");
  // Basketball has a compact 22 x 12 m playing area and 1 m decorative apron.
  var basket=Group("Basketball practice court - 22 by 12 metres");
  Box(basket,"Basketball paved apron",new Vector3(20.5f,.005f,13.5f),new Vector3(24,.06f,14),teal,true);
  Box(basket,"Basketball acrylic playing surface",new Vector3(20.5f,.041f,13.5f),new Vector3(22,.012f,12),blue);
  foreach(float x in new[]{11.65f,29.35f})Box(basket,"Key painted surface",new Vector3(x,.049f,13.5f),new Vector3(4.3f,.003f,3.85f),clay);
  Rect(basket,20.5f,13.5f,22,12,.054f,.06f);Line(basket,new Vector3(20.5f,.054f,7.5f),new Vector3(20.5f,.054f,19.5f),.06f,white);Arc(basket,new Vector3(20.5f,.054f,13.5f),1.42f,0,360,.06f);
  foreach(float side in new[]{-1f,1f}){
   float end=20.5f+11*side,ft=end-side*4.3f;Rect(basket,(end+ft)/2,13.5f,4.3f,3.85f,.055f,.06f);Arc(basket,new Vector3(ft,.055f,13.5f),1.42f,0,360,.055f);
   float ring=end-side*1.26f;Arc(basket,new Vector3(ring,.055f,13.5f),5.05f,side<0?-90:90,side<0?90:270,.055f);
   Line(basket,new Vector3(end,.055f,8.45f),new Vector3(ring,.055f,8.45f),.055f,white);Line(basket,new Vector3(end,.055f,18.55f),new Vector3(ring,.055f,18.55f),.055f,white);
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(hoop,basket);instance.name=side<0?"Basketball goal west":"Basketball goal east";instance.transform.position=new Vector3(end-side*.75f,.05f,13.5f);instance.transform.rotation=Quaternion.Euler(0,side<0?-90:90,0);
   // Support the downloaded wall-braced assembly on a paired outdoor steel mast.
   float mast=end+side*.42f;
   foreach(float dz in new[]{-.45f,.45f}){Box(basket,"Bolted goal base",new Vector3(mast,.08f,13.5f+dz),new Vector3(.44f,.08f,.34f),metal);Tube(basket,new Vector3(mast,.08f,13.5f+dz),new Vector3(mast,3.82f,13.5f+dz),.075f,metal,16);Box(basket,"Padded goal upright",new Vector3(mast,.82f,13.5f+dz),new Vector3(.24f,1.5f,.24f),padding,true);}
   Box(basket,"Goal upper cross member",new Vector3(mast,3.36f,13.5f),new Vector3(.14f,.14f,1.1f),metal);
  }
  // Volleyball stays on the other side of the central evacuation axis.
  var volley=Group("Volleyball practice court - 14 by 7 metres");
  Box(volley,"Volleyball paved apron",new Vector3(13.25f,.005f,-13),new Vector3(9,.06f,17),teal,true);
  Box(volley,"Volleyball acrylic surface",new Vector3(13.25f,.041f,-13),new Vector3(7,.012f,14),clay);
  Rect(volley,13.25f,-13,7,14,.054f,.055f);
  foreach(float z in new[]{-15.33f,-13f,-10.67f})Line(volley,new Vector3(9.75f,.055f,z),new Vector3(16.75f,.055f,z),.055f,white);
  var netInstance=(GameObject)PrefabUtility.InstantiatePrefab(net,volley);netInstance.name="Volleyball net and posts";netInstance.transform.position=new Vector3(13.25f,.05f,-13);netInstance.transform.localScale=new Vector3(.775f,1,1);
  foreach(float x in new[]{8.9875f,17.5125f})Box(volley,"Padded volleyball post",new Vector3(x,.77f,-13),new Vector3(.22f,1.45f,.22f),padding,true);
  var netCollider=new GameObject("Volleyball net collision");netCollider.transform.SetParent(volley,false);netCollider.transform.position=new Vector3(13.25f,1.97f,-13);netCollider.AddComponent<BoxCollider>().size=new Vector3(7.8f,1.02f,.035f);
  // Move complete northern planting beds as groups; leave southern gardens and buses in place.
  var landscaping=GameObject.Find("School expansion/Courtyard landscaping").transform;
  var leafy=GameObject.Find("Campus leafy trees").transform;
  foreach(float oldZ in new[]{10f,18f}){
   var group=Group("Relocated north garden "+oldZ);group.position=new Vector3(20,0,oldZ);
   foreach(Transform t in landscaping.Cast<Transform>().ToArray())if(t.position.z>oldZ-3.5f&&t.position.z<oldZ+3.5f)t.SetParent(group,true);
   foreach(Transform t in leafy.Cast<Transform>().ToArray())if(t.position.x>15&&Mathf.Abs(t.position.z-oldZ)<3.5f)t.SetParent(group,true);
   group.localScale=new Vector3(.75f,1,1);group.rotation=Quaternion.Euler(0,90,0);group.position=new Vector3(oldZ==10?14:26,0,22.2f);
  }
  foreach(Transform t in GameObject.Find("School expansion").transform)if(t.name=="Garden path border"&&t.position.z>0)t.gameObject.SetActive(false);
  // Inspect/reposition all eight existing benches. Put their lowest geometry directly on the paving.
  var benchChanges=new List<object>();
  var benches=UnityEngine.Object.FindObjectsByType<Transform>().Where(t=>t.name=="Garden bench"||t.name=="Courtyard slatted bench"||t.name=="Corridor bench - outside doorway clearance").OrderBy(t=>t.position.z).ToArray();
  foreach(var b in benches){
   var old=b.position;
   if(b.name=="Garden bench"){if(old.z>12)b.position=new Vector3(21.4f,0,22.6f);else if(old.z>0)b.position=new Vector3(18.7f,0,22.6f);else if(old.z<-12){b.position=new Vector3(12,0,-22.5f);b.rotation=Quaternion.Euler(0,180,0);}else b.position=new Vector3(24,0,-8);}
   if(b.name=="Corridor bench - outside doorway clearance")b.rotation=Quaternion.Euler(0,270,0);
   if(b.name=="Courtyard slatted bench"&&old.z<0)b.rotation=Quaternion.Euler(0,180,0);
   float ground=b.name.StartsWith("Corridor")?.039f:-.02f;b.position+=Vector3.up*(ground-BoundsOf(b.gameObject).min.y);
   benchChanges.Add(new{name=b.name,old=old.ToString(),position=b.position.ToString(),bounds=BoundsOf(b.gameObject).ToString()});
  }
  var shade=Group("North spectator pergola and paved seating");
  Box(shade,"Spectator stone terrace",new Vector3(20.1f,-.015f,22.1f),new Vector3(6,.01f,2.8f),stone);
  foreach(float x in new[]{17.35f,22.85f})foreach(float z in new[]{21.1f,23.2f})Box(shade,"Pergola upright",new Vector3(x,1.35f,z),new Vector3(.13f,2.7f,.13f),metal,true);
  foreach(float z in new[]{21.1f,23.2f})Box(shade,"Pergola fascia",new Vector3(20.1f,2.75f,z),new Vector3(5.9f,.2f,.12f),wood);
  for(float x=17.2f;x<23.1f;x+=.23f)Box(shade,"Pergola overhead slat",new Vector3(x,2.87f,22.15f),new Vector3(.085f,.1f,2.5f),wood);
  // A restrained metal backstop at the outer basketball end; no new realtime lights.
  var fence=Group("Basketball outer end ball stop");
  foreach(float z in new[]{8.5f,11f,13.5f,16f,18.5f})Tube(fence,new Vector3(33.15f,0,z),new Vector3(33.15f,3.1f,z),.035f,metal);
  foreach(float y in new[]{.08f,1.55f,3.05f})Tube(fence,new Vector3(33.15f,y,8.5f),new Vector3(33.15f,y,18.5f),.025f,metal);
  for(float z=8.5f;z<=18.5f;z+=.2f)Tube(fence,new Vector3(33.15f,.08f,z),new Vector3(33.15f,3.05f,z),.006f,metal,4);
  for(float y=.25f;y<3.05f;y+=.2f)Tube(fence,new Vector3(33.15f,y,8.5f),new Vector3(33.15f,y,18.5f),.006f,metal,4);
  var fc=new GameObject("Backstop collision");fc.transform.SetParent(fence,false);fc.transform.position=new Vector3(33.15f,1.55f,13.5f);fc.AddComponent<BoxCollider>().size=new Vector3(.08f,3.1f,10);
  var details=Group("Campus arrival bicycle stands and bins");
  for(int i=0;i<4;i++){float x=27.4f+i*.9f;Tube(details,new Vector3(x,.03f,-6.3f),new Vector3(x,.78f,-6.3f),.028f,metal,12);Tube(details,new Vector3(x,.78f,-6.3f),new Vector3(x,.78f,-5.5f),.028f,metal,12);Tube(details,new Vector3(x,.78f,-5.5f),new Vector3(x,.03f,-5.5f),.028f,metal,12);Box(details,"Bicycle rack footing",new Vector3(x,.02f,-5.9f),new Vector3(.12f,.04f,1.1f),stone);}
  foreach(var p in new[]{new Vector3(31.5f,0,22.5f),new Vector3(25.8f,0,-8)}){Box(details,"Waste bin housing",p+Vector3.up*.48f,new Vector3(.38f,.9f,.38f),metal,true);Box(details,"Waste bin lid",p+Vector3.up*.95f,new Vector3(.43f,.055f,.43f),stone);Box(details,"Waste bin opening",p+new Vector3(0,.78f,-.198f),new Vector3(.25f,.13f,.015f),padding);}
  Flush();Physics.SyncTransforms();
  string after=Newtonsoft.Json.JsonConvert.SerializeObject(Protected());File.WriteAllText("../Documentation/CampusSportsProtectedAfter.json",after);if(before!=after)throw new Exception("Protected classroom/gameplay state unexpectedly changed.");
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  var result=new{courts=new[]{new{name="Basketball",size="22 x 12",center="20.5,13.5"},new{name="Volleyball",size="14 x 7",center="13.25,-13"}},benchChanges,protectedStateUnchanged=true,batches=batches.Count,imported=new[]{"Innerscene basketball hoop CC0","Innerscene volleyball net CC0"}};
  File.WriteAllText("../Documentation/CampusSportsChanges.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
