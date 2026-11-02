using System;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEngine.Rendering;using UnityEditor;using UnityEditor.SceneManagement;using TMPro;
public static class BuildCampusExterior {
 const string R="Assets/_FireSafety/CampusExterior/", T="Assets/ThirdParty/CampusExterior/";
 static Transform root,group;static TMP_FontAsset font;static Material cream,terra,stone,metal,glass,white,navy,paving,asphalt,wood;
 static Dictionary<string,Batch> batches=new Dictionary<string,Batch>();
 class Batch {public Transform parent;public Material material;public List<Vector3> v=new List<Vector3>();public List<Vector2> uv=new List<Vector2>();public List<int> tri=new List<int>();}
 static Material Mat(string name,Color color,float smooth=.2f,float metallic=0,string texture=null){
  var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);m.enableInstancing=true;
  if(texture!=null){var path=T+texture+"/";var files=Directory.GetFiles(path);var diffuse=files.First(p=>p.Contains("_diff_")&&!p.EndsWith(".meta"));var normal=files.First(p=>p.Contains("_nor_gl_")&&!p.EndsWith(".meta"));m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(diffuse.Replace('\\','/')));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normal.Replace('\\','/')));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.35f);}
  AssetDatabase.CreateAsset(m,R+"Materials/"+name+".mat");return m;
 }
 static Transform Group(string name){var g=new GameObject(name);g.transform.SetParent(root,false);return g.transform;}
 static void Box(string name,Vector3 pos,Vector3 size,Material mat,bool collider=false){
  string key=group.name+"|"+mat.name;if(!batches.TryGetValue(key,out var b)){b=new Batch{parent=group,material=mat};batches.Add(key,b);}
  foreach(var n in new[]{Vector3.up,Vector3.down,Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){
   var a=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);var c=Vector3.Cross(n,a);int start=b.v.Count;
   foreach(var q in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){var p=pos+Vector3.Scale(n+a*q.x+c*q.y,size)*.5f;b.v.Add(p);b.uv.Add(new Vector2(Vector3.Dot(p,a),Vector3.Dot(p,c))*.5f);}
   b.tri.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
  }
  if(collider){var g=new GameObject(name+" collision");g.transform.SetParent(group,false);g.transform.position=pos;g.AddComponent<BoxCollider>().size=size;}
 }
 static void Flush(){int i=0;foreach(var b in batches.Values){var m=new Mesh{name=b.parent.name+" - "+b.material.name,indexFormat=IndexFormat.UInt32};m.SetVertices(b.v);m.SetUVs(0,b.uv);m.SetTriangles(b.tri,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();AssetDatabase.CreateAsset(m,R+"Meshes/Architecture-"+i+++".asset");var g=new GameObject(m.name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(b.parent,false);g.GetComponent<MeshFilter>().sharedMesh=m;var renderer=g.GetComponent<MeshRenderer>();renderer.sharedMaterial=b.material;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);}}
 static void Text(string name,string words,Vector3 pos,float yaw,Vector2 size,float fontSize,Color color){var t=new GameObject(name,typeof(TextMeshPro)).GetComponent<TextMeshPro>();t.transform.SetParent(group,false);t.transform.position=pos;t.transform.rotation=Quaternion.Euler(0,yaw,0);t.rectTransform.sizeDelta=size;t.font=font;t.fontSize=fontSize;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.text=words;t.color=color;t.textWrappingMode=TextWrappingModes.NoWrap;t.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;}
 static void Rail(float x,float y,float start,float end,bool collide){float mid=(start+end)/2,len=end-start;Box("Railing lower rail",new Vector3(x,y+.13f,mid),new Vector3(.055f,.06f,len),metal);Box("Railing handrail",new Vector3(x,y+.98f,mid),new Vector3(.085f,.07f,len),metal);Box("Railing middle rail",new Vector3(x,y+.77f,mid),new Vector3(.04f,.035f,len),metal);for(float z=start;z<=end;z+=.17f)Box("Railing baluster",new Vector3(x,y+.54f,z),new Vector3(.027f,.83f,.027f),metal);if(collide){var go=new GameObject("Railing safety collision");go.transform.SetParent(group,false);go.transform.position=new Vector3(x,y+.5f,mid);go.AddComponent<BoxCollider>().size=new Vector3(.12f,1,len);}}
 static void Window(float x,float y,float z,float width,float height){
  Box("Deep window recess",new Vector3(x-.08f,y,z),new Vector3(.06f,height,width),navy);
  Box("Reflective window panes",new Vector3(x+.015f,y,z),new Vector3(.022f,height-.09f,width-.09f),glass);
  foreach(float s in new[]{-1f,1f}){Box("Window jamb",new Vector3(x+.04f,y,z+s*width*.5f),new Vector3(.11f,height+.12f,.065f),metal);Box("Window head and sill",new Vector3(x+.04f,y+s*height*.5f,z),new Vector3(.11f,.065f,width),metal);}
  for(int j=1;j<3;j++)Box("Window mullion",new Vector3(x+.06f,y,z-width*.5f+width*j/3),new Vector3(.09f,height,.045f),metal);
  Box("Window transom",new Vector3(x+.065f,y+height*.25f,z),new Vector3(.08f,.04f,width),metal);
  for(int i=0;i<4;i++)Box("Ventilation louvre",new Vector3(x+.075f,y+height*.28f+i*.065f,z),new Vector3(.07f,.025f,width-.09f),metal);
  Box("Projecting stone window sill",new Vector3(x+.08f,y-height*.5f-.07f,z),new Vector3(.3f,.09f,width+.23f),stone);
 }
 static void Lamp(float x,float y,float z,bool pole){
  if(pole){Box("Fluted lantern base",new Vector3(x,.14f,z),new Vector3(.3f,.28f,.3f),metal,true);Box("Lantern mast",new Vector3(x,y*.5f,z),new Vector3(.085f,y,.085f),metal);}
  Box("Lantern bracket",new Vector3(x-.12f,y,z),new Vector3(.3f,.055f,.055f),metal);
  Box("Lantern luminous core",new Vector3(x,y+.16f,z),new Vector3(.19f,.33f,.19f),white);
  foreach(float dx in new[]{-.12f,.12f})foreach(float dz in new[]{-.12f,.12f})Box("Lantern cage",new Vector3(x+dx,y+.16f,z+dz),new Vector3(.025f,.36f,.025f),metal);
  Box("Lantern lid",new Vector3(x,y+.36f,z),new Vector3(.3f,.065f,.3f),metal);Box("Lantern cap",new Vector3(x,y+.41f,z),new Vector3(.16f,.06f,.16f),metal);
 }
 public static object Run(){
  if(EditorApplication.isPlaying||EditorSceneManager.GetActiveScene().isDirty)throw new Exception("Saved Edit Mode required");if(GameObject.Find("Campus exterior upgrade"))throw new Exception("Campus already exists");
  foreach(var p in new[]{"Materials","Meshes","Prefabs"})Directory.CreateDirectory(R+p);AssetDatabase.Refresh();
  // Snapshot serialized gameplay before changing only environment objects.
  var gameplay=UnityEngine.Object.FindObjectsByType<MonoBehaviour>().Where(c=>c&&c.GetType().Namespace=="FireSafety").Select(c=>new{name=c.name,type=c.GetType().Name,json=EditorJsonUtility.ToJson(c)}).ToArray();
  File.WriteAllText("../Documentation/CampusGameplayBefore.json",Newtonsoft.Json.JsonConvert.SerializeObject(gameplay,Newtonsoft.Json.Formatting.Indented));
  cream=Mat("Campus warm ivory plaster",new Color(.89f,.82f,.66f),.13f,0,"plastered_wall_02");terra=Mat("Campus terracotta plaster",new Color(.56f,.27f,.17f),.15f,0,"plastered_wall_02");stone=Mat("Campus limestone trim",new Color(.66f,.6f,.47f),.25f,0,"plastered_wall_02");metal=Mat("Campus charcoal powdercoat",new Color(.065f,.085f,.09f),.48f,.5f);glass=Mat("Campus recessed upper glass",new Color(.16f,.25f,.29f),.86f,.6f);white=Mat("Campus porcelain",new Color(.91f,.88f,.75f),.45f);navy=Mat("Campus navy",new Color(.025f,.065f,.105f),.22f);wood=Mat("Campus teak",new Color(.36f,.19f,.085f),.27f);asphalt=Mat("Campus asphalt",Color.white,.1f,0,"asphalt_02");
  var oldPaving=UnityEngine.Object.FindObjectsByType<Renderer>().SelectMany(r=>r.sharedMaterials).First(m=>m&&m.name=="Courtyard - Poly Haven concrete pavers");paving=new Material(oldPaving){name="Campus corridor stone tile"};paving.color=new Color(.79f,.74f,.62f);AssetDatabase.CreateAsset(paving,R+"Materials/Campus corridor stone tile.mat");
  font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");root=new GameObject("Campus exterior upgrade").transform;
  var school=GameObject.Find("School expansion");
  foreach(Transform t in school.transform)if(new[]{"Corridor exterior masonry","Corridor exterior glazing","Corridor lintel","Window mullion","School boundary masonry","School boundary coping","School boundary east","School exterior identity","Lantern post","Lantern housing"}.Contains(t.name))t.gameObject.SetActive(false);
  foreach(Transform t in school.transform.Find("School exterior finishing"))if(t.name!="Continuous corridor roof"&&t.name!="Central exit canopy")t.gameObject.SetActive(false);
  GameObject.Find("Phase 5 - evacuation and assembly/Assembly sign").SetActive(false);GameObject.Find("Phase 5 - evacuation and assembly/Assembly sign post").SetActive(false);
  group=Group("Decorative upper storey - no access");
  Box("Upper slab",new Vector3(1.8f,3.22f,-.2f),new Vector3(10.4f,.25f,43.5f),stone);
  Box("Upper rear wall",new Vector3(-3.07f,4.82f,-.2f),new Vector3(.24f,3.1f,43.25f),cream);
  foreach(float z in new[]{-21.7f,21.3f})Box("Upper end wall",new Vector3(1.55f,4.82f,z),new Vector3(9.5f,3.1f,.2f),cream);
  Box("Upper roof slab",new Vector3(1.9f,6.41f,-.2f),new Vector3(10.7f,.2f,43.7f),cream);
  foreach(float x in new[]{-3.44f,7.2f}){Box("Roof parapet",new Vector3(x,6.73f,-.2f),new Vector3(.18f,.52f,43.7f),cream);Box("Terracotta roof coping",new Vector3(x,7.01f,-.2f),new Vector3(.25f,.08f,43.85f),terra);}
  foreach(float z in new[]{-22.03f,21.63f}){Box("Return parapet",new Vector3(1.88f,6.73f,z),new Vector3(10.8f,.52f,.16f),cream);Box("Return coping",new Vector3(1.88f,7.01f,z),new Vector3(10.9f,.08f,.24f),terra);}
  for(int i=0;i<5;i++){float z=(i-2)*8.55f;
   Box("Upper classroom facade",new Vector3(3.35f,4.83f,z),new Vector3(.18f,3.0f,8.55f),cream);
   // Shallow framed recesses are decorative; no hidden room interiors are built.
   Window(3.47f,4.95f,z+.65f,3.35f,1.62f);
   Box("Upper door frame",new Vector3(3.49f,4.5f,z-2.8f),new Vector3(.1f,2.34f,1.12f),metal);Box("Upper door panel",new Vector3(3.55f,4.48f,z-2.8f),new Vector3(.04f,2.22f,1),wood);Box("Upper door glazed inset",new Vector3(3.58f,4.94f,z-2.8f),new Vector3(.02f,.92f,.76f),glass);Box("Door pull",new Vector3(3.65f,4.43f,z-2.45f),new Vector3(.06f,.23f,.035f),metal);
   Box("Upper number panel",new Vector3(3.56f,5.88f,z-2.8f),new Vector3(.04f,.26f,.88f),navy);Text("Upper room label","C-20"+(i+1),new Vector3(3.59f,5.88f,z-2.8f),-90,new Vector2(.8f,.22f),1.45f,Color.white);
  }
  Rail(6.67f,3.34f,-21.55f,21.15f,false);
  group=Group("Terracotta facade colonnade");
  for(int i=0;i<=10;i++){float z=-21.575f+i*4.275f;Box("Terracotta column",new Vector3(6.75f,3.5f,z),new Vector3(.44f,7,.42f),terra,true);Box("Column plinth",new Vector3(6.75f,.24f,z),new Vector3(.55f,.48f,.55f),stone,true);Box("Column capital",new Vector3(6.75f,6.35f,z),new Vector3(.58f,.15f,.57f),terra);}
  Box("Balcony fascia",new Vector3(6.88f,3.15f,-.2f),new Vector3(.22f,.4f,43.4f),cream);Box("Balcony terracotta trim",new Vector3(7.01f,3.38f,-.2f),new Vector3(.055f,.1f,43.4f),terra);
  // The sign is centered over the existing exit, not over either wing.
  group=Group("SATHYABAMA UNIVERSITY name board");
  Box("Sign mounting frame",new Vector3(7.13f,5.93f,-.2f),new Vector3(.2f,1.28f,8.12f),terra);Box("Ivory enamel sign face",new Vector3(7.25f,5.93f,-.2f),new Vector3(.04f,1.02f,7.85f),white);
  Text("University lettering","SATHYABAMA UNIVERSITY",new Vector3(7.28f,5.98f,-.2f),-90,new Vector2(7.5f,.65f),4.8f,new Color(.035f,.09f,.15f));
  Text("Academic block subtitle","ACADEMIC BLOCK  |  FIRE SAFETY TRAINING",new Vector3(7.28f,5.62f,-.2f),-90,new Vector2(7.3f,.22f),1.25f,new Color(.16f,.22f,.23f));
  group=Group("Ground floor covered corridor");
  Box("Stone corridor floor finish",new Vector3(4.91f,.013f,-.2f),new Vector3(2.95f,.016f,42.85f),paving);
  foreach(float x in new[]{3.53f,6.27f})Box("Corridor border inlay",new Vector3(x,.023f,-.2f),new Vector3(.1f,.005f,42.85f),stone);
  for(float z=-21.5f;z<21.2f;z+=.75f)Box("Tile joint",new Vector3(4.91f,.025f,z),new Vector3(2.85f,.003f,.009f),stone);
  foreach(var seg in new[]{new Vector2(-21.5f,-2.15f),new Vector2(1.75f,21.1f)}){Box("Corridor low parapet",new Vector3(6.57f,.14f,(seg.x+seg.y)/2),new Vector3(.2f,.28f,seg.y-seg.x),cream,true);Rail(6.57f,.28f,seg.x,seg.y,true);}
  foreach(float z in new[]{-19f,-10.5f,-2f,6.5f,15f}){Box("Ceiling fixture surround",new Vector3(4.9f,2.9f,z),new Vector3(.35f,.08f,.35f),metal);Box("Ceiling fixture diffuser",new Vector3(4.9f,2.85f,z),new Vector3(.27f,.035f,.27f),white);}
  foreach(float z in new[]{-18f,-9f,9f,18f})Lamp(7.08f,2.1f,z,false);
  group=Group("Courtyard and entrance paving");
  Box("Extended courtyard base",new Vector3(20.4f,-.13f,-.2f),new Vector3(27.8f,.22f,48),paving,true);
  Box("Campus entrance drive",new Vector3(32.1f,.005f,-.2f),new Vector3(8.8f,.035f,7.1f),asphalt,true);
  Box("Bus parking asphalt",new Vector3(26.6f,.015f,-17.2f),new Vector3(12.7f,.05f,12.6f),asphalt,true);
  foreach(float z in new[]{-22.8f,-18.6f,-14.4f,-10.8f})Box("Bus parking bay line",new Vector3(26.6f,.044f,z),new Vector3(11.8f,.008f,.1f),white);
  for(int i=0;i<9;i++)Box("Entrance crossing stripe",new Vector3(31.4f,.032f,-3.25f+i*.76f),new Vector3(2.1f,.009f,.38f),white);
  foreach(float z in new[]{-3.85f,3.45f})Box("Entrance kerb",new Vector3(31.5f,.09f,z),new Vector3(9.8f,.18f,.18f),stone,true);
  group=Group("Campus perimeter and central double gate");
  foreach(var seg in new[]{new Vector2(-24.2f,-4.05f),new Vector2(3.65f,23.8f)}){
   var mid=(seg.x+seg.y)/2;var len=seg.y-seg.x;Box("Front compound wall",new Vector3(34.15f,.87f,mid),new Vector3(.28f,1.74f,len),cream,true);Box("Front wall coping",new Vector3(34.15f,1.78f,mid),new Vector3(.42f,.12f,len),stone);Rail(34.15f,1.5f,seg.x,seg.y,false);
   for(float z=seg.x;z<seg.y;z+=4){Box("Boundary pillar",new Vector3(34.15f,1.2f,z),new Vector3(.48f,2.4f,.48f),terra,true);Box("Boundary pillar cap",new Vector3(34.15f,2.45f,z),new Vector3(.6f,.12f,.6f),stone);}
  }
  foreach(float z in new[]{-24.1f,23.7f}){Box("Side compound wall",new Vector3(13.8f,1.05f,z),new Vector3(40.7f,2.1f,.28f),cream,true);Box("Side wall coping",new Vector3(13.8f,2.15f,z),new Vector3(40.9f,.12f,.4f),stone);for(float x=-6.4f;x<=34;x+=4){Box("Side wall pier",new Vector3(x,1.15f,z),new Vector3(.44f,2.3f,.44f),terra,true);}}
  Box("Rear boundary",new Vector3(-6.5f,1.05f,-.2f),new Vector3(.25f,2.1f,48),cream,true);Box("Rear boundary coping",new Vector3(-6.5f,2.16f,-.2f),new Vector3(.4f,.12f,48),stone);
  // Rear-side walkway permits exterior inspection of the original classroom windows.
  Box("Rear campus paving",new Vector3(-4.7f,-.1f,-.2f),new Vector3(3.6f,.18f,47.6f),paving,true);
  foreach(float z in new[]{-22.9f,22.5f})Box("Side return walkway",new Vector3(1.8f,-.1f,z),new Vector3(13,.18f,2.1f),paving,true);
  foreach(float z in new[]{-4.05f,3.65f}){Box("Main gate pillar",new Vector3(34.15f,1.55f,z),new Vector3(.85f,3.1f,.85f),terra,true);Box("Gate pillar inset",new Vector3(34.59f,1.58f,z),new Vector3(.03f,2.1f,.47f),cream);Box("Gate pillar plinth",new Vector3(34.15f,.2f,z),new Vector3(1.02f,.4f,1.02f),stone);Box("Gate pillar capital",new Vector3(34.15f,3.13f,z),new Vector3(1.03f,.17f,1.03f),stone);Lamp(34.15f,3.3f,z,false);}
  for(int side=0;side<2;side++){float start=side==0?-3.59f:-.16f,end=side==0?-.24f:3.19f;float mid=(start+end)/2,len=end-start;Box("Gate lower steel panel",new Vector3(34.15f,.37f,mid),new Vector3(.11f,.65f,len),metal,true);Box("Gate top rail",new Vector3(34.15f,2.38f,mid),new Vector3(.095f,.09f,len),metal);Box("Gate cross rail",new Vector3(34.15f,1.9f,mid),new Vector3(.075f,.07f,len),metal);for(float z=start;z<=end;z+=.18f)Box("Gate vertical grille",new Vector3(34.15f,1.48f,z),new Vector3(.045f,1.85f,.045f),metal);foreach(float z in new[]{start,end}){Box("Gate frame stile",new Vector3(34.15f,1.25f,z),new Vector3(.12f,2.46f,.1f),metal);foreach(float y in new[]{.6f,1.9f})Box("Gate hinge",new Vector3(34.13f,y,z),new Vector3(.19f,.12f,.15f),metal);}}
  var gateCollider=new GameObject("Decorative gate collision");gateCollider.transform.SetParent(group,false);gateCollider.transform.position=new Vector3(34.15f,1.25f,-.2f);gateCollider.AddComponent<BoxCollider>().size=new Vector3(.2f,2.5f,6.9f);
  group=Group("Courtyard architectural lanterns");foreach(float z in new[]{-20f,-10f,10f,20f})Lamp(24,2.7f,z,true);foreach(float z in new[]{-6f,5.6f})Lamp(30,2.7f,z,true);
  Flush();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  return new{architectureBatches=batches.Count,upperFloorHeight=7.05,gateX=34.15,sign="SATHYABAMA UNIVERSITY",assemblyTriggerPreserved=true};
 }
}

