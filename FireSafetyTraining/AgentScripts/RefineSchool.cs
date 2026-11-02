using System;using System.Linq;using UnityEngine;using UnityEngine.AI;using UnityEditor;using UnityEditor.SceneManagement;using FireSafety;
public static class RefineSchool {
public static object Run(){if(EditorApplication.isPlaying)throw new Exception("Edit Mode required");var crowd=UnityEngine.Object.FindAnyObjectByType<SchoolEvacuationController>();
foreach(var p in crowd.people){bool teacher=p.name=="Teacher";p.legacyAnimation.transform.localScale=Vector3.one*(teacher?.54f:.49f);p.legacyAnimation.transform.localPosition=Vector3.up*.025f;foreach(var r in p.GetComponentsInChildren<SkinnedMeshRenderer>()){r.updateWhenOffscreen=true;foreach(var m in r.sharedMaterials){if(m.name.EndsWith("_Skin"))m.color=teacher?new Color(.55f,.34f,.23f):new Color(.67f,.43f,.30f);if(m.name.EndsWith("_Shirt")&&!teacher)m.color=new Color(.34f,.53f,.65f);EditorUtility.SetDirty(m);}}}
for(int i=0;i<crowd.people.Length;i++)crowd.people[i].assemblyPosition=new Vector3(14.4f+(i%3)*.86f,0,-1.7f+(i/3)*.95f);
var player=crowd.session.player;var obstacle=player.GetComponent<NavMeshObstacle>();if(!obstacle)obstacle=player.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Capsule;obstacle.radius=.25f;obstacle.height=1.65f;obstacle.center=Vector3.up*.83f;obstacle.carving=false;
// Keep one readable central exit sign, offset from the original header.
var sign=GameObject.Find("Central exit identification");sign.transform.position=new Vector3(6.34f,3.08f,-.2f);
EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();return "Refined NPC materials and spacing; added non-carving player avoidance; preserved existing training objects.";}}

