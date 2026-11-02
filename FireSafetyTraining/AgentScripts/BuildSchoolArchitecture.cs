using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using FireSafety;
public static class BuildSchoolArchitecture
{
 const string R="Assets/_FireSafety/SchoolExpansion/";
 static Transform root;static Material plaster,wood,paving,metal,green,glass;static TMP_FontAsset font;static int meshId;
 static Mesh MeshBox(Vector3 s){var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();foreach(var n in new[]{Vector3.up,Vector3.down,Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){var a=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);var b=Vector3.Cross(n,a);int start=v.Count;foreach(var q in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){var p=Vector3.Scale(n+a*q.x+b*q.y,s)*.5f;v.Add(p);uv.Add(new Vector2(Vector3.Dot(p,a),Vector3.Dot(p,b))*.6f);}tri.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});}var m=new Mesh{name="School mesh "+meshId++};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateTangents();AssetDatabase.CreateAsset(m,R+"Meshes/"+m.name+".asset");return m;}
 static GameObject Box(string n,Transform p,Vector3 pos,Vector3 size,Material mat,bool collision=true){var g=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.transform.localPosition=pos;g.GetComponent<MeshFilter>().sharedMesh=MeshBox(size);g.GetComponent<MeshRenderer>().sharedMaterial=mat;if(collision)g.AddComponent<BoxCollider>().size=size;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);return g;}
 static void Sign(string n,Transform p,Vector3 pos,float yaw,Vector2 size,string words){var b=Box(n,p,pos,new Vector3(size.x,.04f+size.y,.055f),green,false);b.transform.localRotation=Quaternion.Euler(0,yaw,0);var t=new GameObject("Lettering",typeof(TextMeshPro)).GetComponent<TextMeshPro>();t.transform.SetParent(b.transform,false);t.transform.localPosition=Vector3.back*.033f;t.rectTransform.sizeDelta=size;t.font=font;t.fontSize=2.4f;t.enableAutoSizing=true;t.fontSizeMin=1;t.fontSizeMax=2.7f;t.alignment=TextAlignmentOptions.Center;t.text=words;t.color=Color.white;}
 static void OpenPassage(Transform room){
  var mf=room.Find("Hallway/Walls").GetComponent<MeshFilter>();var old=mf.sharedMesh;var vertices=old.vertices;var mesh=UnityEngine.Object.Instantiate(old);mesh.name="Open-ended passage wall - source art preserved";
  int removed=0;for(int s=0;s<mesh.subMeshCount;s++){var before=old.GetTriangles(s);var after=new List<int>();for(int i=0;i<before.Length;i+=3){var a=mf.transform.TransformPoint(vertices[before[i]]);var b=mf.transform.TransformPoint(vertices[before[i+1]]);var c=mf.transform.TransformPoint(vertices[before[i+2]]);bool cut=(a.z>3.86f&&b.z>3.86f&&c.z>3.86f)||(a.z<-3.96f&&b.z<-3.96f&&c.z<-3.96f);if(cut)removed++;else after.AddRange(new[]{before[i],before[i+1],before[i+2]});}mesh.SetTriangles(after,s);}
  mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,R+"Meshes/OpenPassageWalls.asset");mf.sharedMesh=mesh;mf.GetComponent<MeshCollider>().sharedMesh=mesh;Debug.Log("Opened passage ends, removed "+removed+" end-cap triangles; main classroom walls unchanged.");
 }
 public static string Run(){
  var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.isDirty)throw new Exception("Saved scene in Edit Mode required");if(GameObject.Find("School expansion"))throw new Exception("Expansion exists; use targeted refinement");
  foreach(var d in new[]{"Meshes","Materials","Prefabs","Navigation"})Directory.CreateDirectory(R+d);AssetDatabase.Refresh();
  var mats=UnityEngine.Object.FindObjectsByType<Renderer>().SelectMany(r=>r.sharedMaterials).Where(m=>m).ToArray();plaster=mats.First(m=>m.name=="Classroom_PaintedPlasterWall");wood=mats.First(m=>m.name=="Classroom_WoodFloor");glass=mats.First(m=>m.name=="Classroom_Glass");paving=mats.First(m=>m.name=="Courtyard - Poly Haven concrete pavers");metal=mats.First(m=>m.name=="Exit - anodized dark metal");green=mats.First(m=>m.name=="Exit - safety green");font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
  root=new GameObject("School expansion").transform;Undo.RegisterCreatedObjectUndo(root.gameObject,"Expand existing school");
  var main=GameObject.Find("Classroom - licensed source art");OpenPassage(main.transform);
  var template=UnityEngine.Object.Instantiate(main);template.name="Environment classroom";
  if(PrefabUtility.IsPartOfPrefabInstance(template))PrefabUtility.UnpackPrefabInstance(template,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
  foreach(var script in template.GetComponentsInChildren<MonoBehaviour>(true))UnityEngine.Object.DestroyImmediate(script);
  var support=UnityEngine.Object.Instantiate(GameObject.Find("Floor collision support"),template.transform,true);support.name="Floor support";
  var lights=new GameObject("Classroom local lighting").transform;lights.SetParent(template.transform,false);
  foreach(var l in UnityEngine.Object.FindObjectsByType<Light>().Where(l=>l.name.StartsWith("Fixture fill"))){var copy=UnityEngine.Object.Instantiate(l.gameObject,lights,true);copy.GetComponent<Light>().shadows=LightShadows.None;}
  var prefab=PrefabUtility.SaveAsPrefabAsset(template,R+"Prefabs/EnvironmentOnlyClassroom.prefab");UnityEngine.Object.DestroyImmediate(template);
  int number=2;foreach(float z in new[]{-17.1f,-8.55f,8.55f,17.1f}){var room=(GameObject)PrefabUtility.InstantiatePrefab(prefab);room.name="Classroom "+number+" - environment only";room.transform.SetParent(root,false);room.transform.position=new Vector3(0,0,z);Sign("Classroom "+number+" number",root,new Vector3(3.5f,2.25f,z-1.7f),-90,new Vector2(.8f,.32f),"CLASSROOM "+number);number++;}
  Sign("Classroom 1 training identification",root,new Vector3(3.5f,2.25f,-1.7f),-90,new Vector2(1.2f,.42f),"CLASSROOM 1\nFIRE SAFETY TRAINING");
  Box("Continuous corridor floor",root,new Vector3(4.94f,-.055f,-.18f),new Vector3(2.96f,.12f,42.85f),paving);
  foreach(float z in new[]{-21.55f,21.25f}){Box("Corridor end wall",root,new Vector3(4.94f,1.48f,z),new Vector3(3.1f,2.96f,.14f),plaster);Sign("Wing noticeboard",root,new Vector3(4.94f,1.8f,z+(z<0?.08f:-.08f)),z<0?180:0,new Vector2(1.6f,.9f),"SCHOOL LEARNING WING\nKeep corridors clear\nCentral exit - Assembly A");}
  foreach(float z in new[]{-12.9f,12.6f}){
   Box("Corridor exterior masonry",root,new Vector3(6.53f,.6f,z),new Vector3(.14f,1.2f,17.3f),plaster);
   Box("Corridor exterior glazing",root,new Vector3(6.53f,1.9f,z),new Vector3(.025f,1.4f,17.3f),glass);
   Box("Corridor lintel",root,new Vector3(6.53f,2.83f,z),new Vector3(.16f,.47f,17.3f),plaster);
   for(float offset=-8;offset<=8;offset+=2){Box("Window mullion",root,new Vector3(6.50f,1.9f,z+offset),new Vector3(.08f,1.5f,.055f),metal,false);}
  }
  foreach(float z in new[]{-18f,-13f,-8f,8f,13f,18f}){var lamp=new GameObject("Corridor ceiling light").AddComponent<Light>();lamp.transform.SetParent(root,false);lamp.transform.position=new Vector3(4.9f,2.55f,z);lamp.type=LightType.Point;lamp.range=5;lamp.intensity=.55f;lamp.shadows=LightShadows.None;Sign("Central exit direction",root,new Vector3(3.52f,2.4f,z),-90,new Vector2(1.4f,.28f),z<0?"CENTRAL EXIT   >":"<   CENTRAL EXIT");}
  Sign("Central exit identification",root,new Vector3(6.36f,2.77f,-.2f),90,new Vector2(2.1f,.3f),"CENTRAL EXIT   -   ASSEMBLY A");
  // Extend the school courtyard, retaining the original paved approach and assembly position.
  var oldCourt=GameObject.Find("Outdoor assembly courtyard");foreach(Transform c in oldCourt.transform)if(c.name.Contains("Perimeter")||c.name.Contains("perimeter")||c.name.Contains("Guard")||c.name.Contains("guard")||c.name.Contains("Masonry coping"))c.gameObject.SetActive(false);
  Box("Expanded courtyard foundation",root,new Vector3(16.4f,-.15f,-.2f),new Vector3(19.8f,.24f,48),paving);
  foreach(float z in new[]{-24.1f,23.7f}){Box("School boundary masonry",root,new Vector3(16.4f,.4f,z),new Vector3(20,.8f,.2f),plaster);Box("School boundary coping",root,new Vector3(16.4f,.83f,z),new Vector3(20,.07f,.27f),paving);}
  Box("School boundary east",root,new Vector3(26.3f,.4f,-.2f),new Vector3(.2f,.8f,48),plaster);
  foreach(float z in new[]{-17f,-8f,8f,17f}){var bench=UnityEngine.Object.Instantiate(oldCourt.transform.Cast<Transform>().First(c=>c.name=="Courtyard slatted bench").gameObject,root,true);bench.name="Garden bench";bench.transform.position=new Vector3(22,0,z);}
  foreach(float z in new[]{-20f,-10f,10f,20f}){Box("Garden path border",root,new Vector3(18,.04f,z),new Vector3(.15f,.1f,6),paving);var l=new GameObject("Courtyard lantern").AddComponent<Light>();l.transform.SetParent(root,false);l.transform.position=new Vector3(24,2.5f,z);l.range=5;l.intensity=.5f;l.color=new Color(1,.9f,.7f);l.shadows=LightShadows.None;Box("Lantern post",root,new Vector3(24,1.25f,z),new Vector3(.08f,2.5f,.08f),metal);Box("Lantern housing",root,new Vector3(24,2.55f,z),new Vector3(.35f,.24f,.35f),glass,false);}
  Sign("School exterior identity",root,new Vector3(6.7f,2.7f,5.6f),-90,new Vector2(2.6f,.45f),"SCHOOL SAFETY LEARNING CENTRE");
  var evac=UnityEngine.Object.FindAnyObjectByType<EvacuationManager>();evac.assemblyZone.size=new Vector3(4.8f,2,4.8f);
  foreach(Transform c in evac.assemblyZone.transform)if(c.name.Contains("boundary marking"))c.gameObject.SetActive(false);
  foreach(float sign in new[]{-1f,1f}){Box("Assembly perimeter",root,new Vector3(14.2f,.018f,-.2f+sign*2.4f),new Vector3(4.8f,.025f,.07f),green,false);Box("Assembly perimeter",root,new Vector3(14.2f+sign*2.4f,.018f,-.2f),new Vector3(.07f,.025f,4.8f),green,false);}
  var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();surface.center=new Vector3(10,1.4f,0);surface.size=new Vector3(35,5,49);surface.RemoveData();surface.navMeshData=null;Physics.SyncTransforms();surface.BuildNavMesh();AssetDatabase.CreateAsset(surface.navMeshData,R+"Navigation/SchoolNavMesh.asset");
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Five classrooms and central exit configured. Nav triangles: "+NavMesh.CalculateTriangulation().indices.Length/3;
 }
}

