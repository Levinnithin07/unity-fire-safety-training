using System;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using FireSafety;
// Character FBX animation bounds are oversized. These scales were verified in rendered Play Mode.
public static class FixSchoolCharacters {
public static string Run(){if(EditorApplication.isPlaying)throw new Exception("Edit Mode required");foreach(var p in UnityEngine.Object.FindAnyObjectByType<SchoolEvacuationController>().people){p.legacyAnimation.transform.localScale=Vector3.one*(p.name=="Teacher"?.54f:.49f);p.legacyAnimation.transform.localPosition=Vector3.up*.025f;foreach(var r in p.GetComponentsInChildren<SkinnedMeshRenderer>())r.updateWhenOffscreen=true;}EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());return "Applied visually verified character scales";}}
