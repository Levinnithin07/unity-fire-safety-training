using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using FireSafety;
public static class InspectSchoolExpansion {
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 static object V(Vector3 v)=>new {x=v.x,y=v.y,z=v.z};
 public static string Run(){
 var scene=EditorSceneManager.GetActiveScene();
 var data=new {scene=scene.path,dirty=scene.isDirty,playing=EditorApplication.isPlaying,
 renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Select(r=>new {path=PathOf(r.transform),center=V(r.bounds.center),size=V(r.bounds.size),pos=V(r.transform.position),materials=r.sharedMaterials.Select(m=>m?AssetDatabase.GetAssetPath(m):"MISSING")}).ToArray(),
 roots=scene.GetRootGameObjects().Select(g=>new {g.name,pos=V(g.transform.position),children=g.transform.Cast<Transform>().Select(t=>t.name).ToArray()}).ToArray(),
 surfaces=UnityEngine.Object.FindObjectsByType<NavMeshSurface>().Select(s=>new {s.name,s.agentTypeID,center=V(s.center),size=V(s.size),data=AssetDatabase.GetAssetPath(s.navMeshData)}).ToArray(),
 hazards=UnityEngine.Object.FindObjectsByType<RouteHazard>().Select(r=>new {r.name,pos=V(r.transform.position)}).ToArray(),
 assemblies=UnityEngine.Object.FindObjectsByType<EvacuationManager>().Select(e=>new {spawn=V(e.spawn),assembly=V(e.assemblyZone.bounds.center),assemblySize=V(e.assemblyZone.bounds.size),door=V(e.exitDoor.transform.position),destination=V(e.navigation.assembly.position)}).ToArray()};
 string path=Path.GetFullPath("../Documentation/SchoolExpansionPreflight.json");File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(data,Newtonsoft.Json.Formatting.Indented));return path;
 }
}
