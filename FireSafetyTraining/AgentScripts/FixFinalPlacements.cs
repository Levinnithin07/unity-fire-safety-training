using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using FireSafety;

public static class FixFinalPlacements
{
    static string PathOf(Transform t) => t.parent ? PathOf(t.parent)+"/"+t.name : t.name;
    static Dictionary<Component,string> Snapshot() =>
        UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
        .Where(t=>t.gameObject.scene==EditorSceneManager.GetActiveScene())
        .SelectMany(t=>t.GetComponents<Component>().Where(c=>c).Select((c,i)=>new {
            key=c, value=EditorJsonUtility.ToJson(c)
        })).ToDictionary(x=>x.key,x=>x.value);
    public static object Run()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlaying || scene.isDirty || scene.path!="Assets/_FireSafety/Scenes/TrainingScene.unity")
            throw new Exception("Saved TrainingScene in Edit Mode required");
        string backup=System.IO.Path.GetFullPath("PhaseBackups/FinalPlacements-20261020");
        Directory.CreateDirectory(backup);
        File.Copy(scene.path,backup+"/TrainingScene.unity",false);
        File.Copy(scene.path+".meta",backup+"/TrainingScene.unity.meta",false);
        var before=Snapshot();
        var board=GameObject.Find("Classroom - licensed source art").transform.Find("NoticeBoards/CorkBoard1").gameObject;
        var alarm=UnityEngine.Object.FindAnyObjectByType<FireAlarmController>();
        var point=GameObject.Find("fire_alarm_2k").transform;
        var old=new {board=PathOf(board.transform),alarm=alarm.transform.position.ToString("F4"),callPoint=point.position.ToString("F4")};
        // All three retain their identities; disabling this one board also removes its papers/pins/collision.
        Undo.RecordObject(board,"Remove overlapping corkboard");board.SetActive(false);
        PrefabUtility.RecordPrefabInstancePropertyModifications(board);
        Undo.RecordObject(alarm.transform,"Wall mount sounder");
        Undo.RecordObject(point,"Wall mount call point");
        if(!Physics.Raycast(new Vector3(4.5f,1.35f,2f),Vector3.left,out var wall,1.4f) || wall.collider.name!="Walls_2")
            throw new Exception("Expected solid wall beside doorway not found");
        alarm.transform.position=new Vector3(wall.point.x+.001f,2.40f,2f);
        point.position=new Vector3(wall.point.x+.001f,1.35f,2f);
        // Match rear of existing imported model to wall without changing its scale/orientation.
        float rear=point.GetComponentsInChildren<Renderer>().Min(r=>r.bounds.min.x);
        point.position+=Vector3.right*(wall.point.x+.001f-rear);
        PrefabUtility.RecordPrefabInstancePropertyModifications(alarm.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(point);
        Physics.SyncTransforms();
        var after=Snapshot();
        var changed=before.Where(k=>after[k.Key]!=k.Value).Select(k=>k.Key).ToArray();
        var changes=changed.Select(c=>PathOf(c.transform)+"/"+c.GetType().Name).ToArray();
        if(changed.Any(c=>c!=alarm.transform&&c!=point))
            throw new Exception("Unexpected component mutation: "+string.Join(", ",changes));
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        var result=new {old,boardInactive=!board.activeSelf,alarmPosition=alarm.transform.position.ToString("F4"),
            callPointPosition=point.position.ToString("F4"),wall=PathOf(wall.transform),wallX=wall.point.x,
            changedComponents=changes,otherComponentsUnchanged=true,callPointComponents=point.GetComponentsInChildren<Component>(true).Select(c=>c.GetType().Name).Distinct().ToArray(),
            alarmReferencesPreserved=alarm.sounder&&alarm.beacon&&alarm.lens&&UnityEngine.Object.FindAnyObjectByType<FireManager>().alarm==alarm,
            detectorsPreserved=UnityEngine.Object.FindObjectsByType<SmokeDetector>(FindObjectsInactive.Include).All(d=>d.alarm==alarm)};
        File.WriteAllText("../Documentation/FinalPlacements.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
        return result;
    }
}
