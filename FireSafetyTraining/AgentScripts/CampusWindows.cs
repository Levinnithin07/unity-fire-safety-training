using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class CampusWindows {
 const string R="Assets/_FireSafety/CampusExterior/";
 struct V {public Vector3 p,n;public Vector2 uv;public Vector4 tangent;public static V Lerp(V a,V b,float t)=>new V{p=Vector3.Lerp(a.p,b.p,t),n=Vector3.Lerp(a.n,b.n,t).normalized,uv=Vector2.Lerp(a.uv,b.uv,t),tangent=Vector4.Lerp(a.tangent,b.tangent,t)};}
 static List<V> Clip(List<V> input,Func<V,float> distance){var output=new List<V>();if(input.Count==0)return output;var a=input[input.Count-1];float da=distance(a);foreach(var b in input){float db=distance(b);if((da>=0)!=(db>=0))output.Add(V.Lerp(a,b,da/(da-db)));if(db>=0)output.Add(b);a=b;da=db;}return output;}
 static Mesh Cut(MeshFilter mf,Bounds cut,string file){
  var src=mf.sharedMesh;var positions=src.vertices;var normals=src.normals;var uv=src.uv;var tangents=src.tangents;var verts=new List<Vector3>();var ns=new List<Vector3>();var uvs=new List<Vector2>();var ts=new List<Vector4>();var sub=new List<int[]>();
  V Get(int i)=>new V{p=positions[i],n=normals.Length>i?normals[i]:Vector3.up,uv=uv.Length>i?uv[i]:Vector2.zero,tangent=tangents.Length>i?tangents[i]:new Vector4(1,0,0,1)};
  for(int s=0;s<src.subMeshCount;s++){var triangles=new List<int>();void Add(List<V> poly){if(poly.Count<3)return;int start=verts.Count;foreach(var v in poly){verts.Add(v.p);ns.Add(v.n);uvs.Add(v.uv);ts.Add(v.tangent);}for(int i=1;i<poly.Count-1;i++)triangles.AddRange(new[]{start,start+i,start+i+1});}
   var ids=src.GetTriangles(s);for(int i=0;i<ids.Length;i+=3){var poly=new List<V>{Get(ids[i]),Get(ids[i+1]),Get(ids[i+2])};var bounds=new Bounds(mf.transform.TransformPoint(poly[0].p),Vector3.zero);foreach(var v in poly)bounds.Encapsulate(mf.transform.TransformPoint(v.p));if(!bounds.Intersects(cut)){Add(poly);continue;}
    for(int axis=0;axis<3;axis++)for(int side=0;side<2;side++){int ax=axis;float limit=side==0?cut.min[axis]:cut.max[axis];float sign=side==0?1:-1;Func<V,float> d=v=>(mf.transform.TransformPoint(v.p)[ax]-limit)*sign;Add(Clip(poly,v=>-d(v)));poly=Clip(poly,d);}
   }sub.Add(triangles.ToArray());
  }
  var m=new Mesh{name=file,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(verts);m.SetNormals(ns);m.SetUVs(0,uvs);m.SetTangents(ts);m.subMeshCount=sub.Count;for(int s=0;s<sub.Count;s++)m.SetTriangles(sub[s],s);m.RecalculateBounds();AssetDatabase.CreateAsset(m,R+"Meshes/"+file+".asset");return m;
 }
 static GameObject Box(Transform parent,string n,Vector3 p,Vector3 s,Material mat,bool collision=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent,false);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);return g;}
 public static object Run(){if(EditorApplication.isPlaying)throw new Exception("Edit Mode required");var main=GameObject.Find("Classroom - licensed source art").transform;var allRooms=new List<Transform>{main};allRooms.AddRange(GameObject.Find("School expansion").transform.Cast<Transform>().Where(t=>t.name.EndsWith(" - environment only")));
  var hallway=main.Find("Hallway/Walls").GetComponent<MeshFilter>();var originalHall=hallway.sharedMesh;
  var open=Cut(hallway,new Bounds(new Vector3(6.8f,1.5f,0),new Vector3(1.18f,5,10)),"Open campus corridor facade");hallway.sharedMesh=open;
  var opening=new Bounds(new Vector3(3.35f,1.85f,.1f),new Vector3(.7f,1.3f,2.5f));var hallWindow=Cut(hallway,opening,"Corridor classroom window openings");hallway.sharedMesh=originalHall;
  var wall=main.Find("ClassRoom/Walls_2").GetComponent<MeshFilter>();var classroomWindow=Cut(wall,opening,"Classroom framed window openings");
  var frame=AssetDatabase.LoadAssetAtPath<Material>(R+"Materials/Campus charcoal powdercoat.mat");var sill=AssetDatabase.LoadAssetAtPath<Material>(R+"Materials/Campus limestone trim.mat");
  var glass=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Campus clear architectural glass"};glass.SetColor("_BaseColor",new Color(.38f,.58f,.64f,.16f));glass.SetFloat("_Surface",1);glass.SetFloat("_Blend",0);glass.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);glass.SetFloat("_ZWrite",0);glass.SetFloat("_Cull",0);glass.SetFloat("_Smoothness",.92f);glass.SetFloat("_Metallic",.12f);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.SetOverrideTag("RenderType","Transparent");glass.renderQueue=3000;glass.enableInstancing=true;AssetDatabase.CreateAsset(glass,R+"Materials/Campus clear architectural glass.mat");
  var windows=new GameObject("Campus ground floor window upgrades").transform;
  foreach(var room in allRooms){float z=room.position.z;foreach(var pair in new[]{new{path="Hallway/Walls",mesh=hallWindow},new{path="ClassRoom/Walls_2",mesh=classroomWindow}}){var mf=room.Find(pair.path).GetComponent<MeshFilter>();mf.sharedMesh=pair.mesh;var col=mf.GetComponent<MeshCollider>();if(col)col.sharedMesh=pair.mesh;PrefabUtility.RecordPrefabInstancePropertyModifications(mf);if(col)PrefabUtility.RecordPrefabInstancePropertyModifications(col);}
   var parent=new GameObject(room.name+" - glazed corridor window").transform;parent.SetParent(windows,false);
   Box(parent,"Clear safety glazing",new Vector3(3.35f,1.85f,z+.1f),new Vector3(.025f,1.25f,2.45f),glass,true);
   foreach(float side in new[]{-1f,1f}){Box(parent,"Recess frame jamb",new Vector3(3.35f,1.85f,z+.1f+side*1.25f),new Vector3(.36f,1.42f,.075f),frame);Box(parent,"Recess frame head and sill",new Vector3(3.35f,1.85f+side*.65f,z+.1f),new Vector3(.36f,.075f,2.5f),frame);}
   for(int i=1;i<3;i++)Box(parent,"Window mullion",new Vector3(3.35f,1.85f,z-1.15f+2.5f*i/3),new Vector3(.12f,1.3f,.045f),frame);
   Box(parent,"Window transom",new Vector3(3.35f,2.22f,z+.1f),new Vector3(.12f,.05f,2.5f),frame);
   for(int i=0;i<3;i++)Box(parent,"Vent louvre",new Vector3(3.48f,2.28f+i*.065f,z+.1f),new Vector3(.06f,.024f,2.37f),frame);
   Box(parent,"Corridor window sill",new Vector3(3.42f,1.15f,z+.1f),new Vector3(.48f,.08f,2.7f),sill);
   // Retain the licensed rear frame geometry and blinds; upgrade only the surface finish.
   var oldFrame=room.Find("Windows/WindowFrames").GetComponent<Renderer>();oldFrame.sharedMaterial=frame;PrefabUtility.RecordPrefabInstancePropertyModifications(oldFrame);
   var oldGlass=room.Find("Windows/WindowGlass").GetComponent<Renderer>();oldGlass.sharedMaterial=glass;PrefabUtility.RecordPrefabInstancePropertyModifications(oldGlass);
  }
  // Keep the central ground-floor exit completely clear below its upper facade pier.
  var columns=GameObject.Find("Campus exterior upgrade/Terracotta facade colonnade").transform;
  foreach(var col in columns.GetComponentsInChildren<BoxCollider>()){if(Mathf.Abs(col.transform.position.z+.2f)<.1f){if(col.size.y>6){col.transform.position=new Vector3(6.75f,5.22f,-.2f);col.size=new Vector3(.44f,3.56f,.42f);}else col.gameObject.SetActive(false);}}
  foreach(var mf in columns.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh.bounds.Intersects(new Bounds(new Vector3(6.75f,1.65f,-.2f),new Vector3(.9f,3.3f,.8f))))mf.sharedMesh=Cut(mf,new Bounds(new Vector3(6.75f,1.6f,-.2f),new Vector3(.9f,3.7f,.8f)),"Central portal clearance "+mf.sharedMesh.name.Replace(' ','_'));
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();return new{groundFloorWindows=5,rearWindowsPreserved=true,centralDoorClear=true};
 }
}
