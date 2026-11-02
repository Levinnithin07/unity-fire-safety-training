using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Unity.AI.Navigation;using UnityEngine.AI;
public static class RefineCampusSports {
 public static object Run(){
  if(Application.isPlaying)throw new Exception("Edit mode only");
  var group=GameObject.Find("Courtyard architectural lanterns").transform;
  var old=new[]{new Vector3(24,0,10),new Vector3(24,0,20)};
  var next=new[]{new Vector3(26,0,5.5f),new Vector3(29.5f,0,22.5f)};
  int moved=0;
  foreach(var mf in group.GetComponentsInChildren<MeshFilter>()){
   var mesh=UnityEngine.Object.Instantiate(mf.sharedMesh);var v=mesh.vertices;bool changed=false;
   for(int i=0;i<v.Length;i++){var world=mf.transform.TransformPoint(v[i]);for(int j=0;j<2;j++)if(Mathf.Abs(world.x-old[j].x)<.5f&&Mathf.Abs(world.z-old[j].z)<.5f){v[i]=mf.transform.InverseTransformPoint(world+next[j]-old[j]);changed=true;break;}}
   if(changed){mesh.vertices=v;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,"Assets/_FireSafety/CampusSports/Meshes/RelocatedLantern-"+moved+++".asset");mf.sharedMesh=mesh;EditorUtility.SetDirty(mf);}else UnityEngine.Object.DestroyImmediate(mesh);
  }
  foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)){
   if(!(t.name=="Fluted lantern base collision"||t.name=="Lantern post"||t.name=="Lantern housing"||t.name=="Courtyard lantern"))continue;
   for(int j=0;j<2;j++)if(Mathf.Abs(t.position.x-old[j].x)<.05f&&Mathf.Abs(t.position.z-old[j].z)<.05f){t.position+=next[j]-old[j];break;}
  }
  // Match the actual terrace / courtyard paving top rather than burying bench feet.
  foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude).Where(t=>t.name.Contains("bench")||t.name.Contains("Bench"))){
   if(t.position.x<8||t.GetComponentsInChildren<Renderer>().Length==0)continue;
   var rs=t.GetComponentsInChildren<Renderer>();float floor=rs.Min(r=>r.bounds.min.y);
   if(Mathf.Abs(floor+.02f)<.005f)t.position+=Vector3.up*.01f;
  }
  var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();Physics.SyncTransforms();surface.BuildNavMesh();
  var generated=surface.navMeshData;var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/_FireSafety/SchoolExpansion/Navigation/SchoolNavMesh.asset");
  EditorUtility.CopySerialized(generated,saved);surface.RemoveData();surface.navMeshData=saved;surface.AddData();UnityEngine.Object.DestroyImmediate(generated);EditorUtility.SetDirty(saved);
  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  return new{lanternMeshes=moved,navTriangles=NavMesh.CalculateTriangulation().indices.Length/3};
 }
}
