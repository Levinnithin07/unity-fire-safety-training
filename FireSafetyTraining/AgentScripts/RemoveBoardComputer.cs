using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
public static class RemoveBoardComputer {
 public static object Run(){
  var scene=EditorSceneManager.GetActiveScene();
  if(EditorApplication.isPlaying||scene.path!="Assets/_FireSafety/Scenes/TrainingScene.unity"||scene.isDirty)throw new Exception("Saved TrainingScene in Edit Mode required.");
  var computer=GameObject.Find("Desktop computer 2");var desk=GameObject.Find("Second computer desk");
  if(!computer||!desk||!GameObject.Find("Desktop computer 1"))throw new Exception("Expected furniture not found.");
  var folder="PhaseBackups/BoardClearance-"+DateTime.Now.ToString("yyyyMMdd-HHmmss");Directory.CreateDirectory(folder);
  const string nav="Assets/_FireSafety/SchoolExpansion/Navigation/SchoolNavMesh.asset";
  File.Copy(scene.path,folder+"/TrainingScene.unity");File.Copy(nav,folder+"/SchoolNavMesh.asset");
  var parent=computer.transform.parent;
  Undo.DestroyObjectImmediate(computer);Undo.DestroyObjectImmediate(desk);
  Undo.RecordObject(parent.gameObject,"Update computer group label");parent.name="Classroom 1 - desktop computer";
  Physics.SyncTransforms();var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();surface.BuildNavMesh();
  var generated=surface.navMeshData;var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(nav);
  EditorUtility.CopySerialized(generated,saved);surface.RemoveData();surface.navMeshData=saved;surface.AddData();UnityEngine.Object.DestroyImmediate(generated);EditorUtility.SetDirty(saved);
  int routes=0;foreach(var person in UnityEngine.Object.FindObjectsByType<FireSafety.SchoolEvacuee>()){
   var path=new NavMeshPath();var filter=new NavMeshQueryFilter{agentTypeID=surface.agentTypeID,areaMask=NavMesh.AllAreas};
   if(!NavMesh.CalculatePath(person.initialPosition,person.assemblyPosition,filter,path)||path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Disconnected NPC route: "+person.name);
   routes++;
  }
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  return new{removed=new[]{"Desktop computer 2 (monitor, keyboard, mouse)","Second computer desk"},remainingComputers=1,connectedNPCRoutes=routes,backup=folder};
 }
}
