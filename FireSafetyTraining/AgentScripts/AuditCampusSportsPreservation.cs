using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Newtonsoft.Json.Linq;
public static class AuditCampusSportsPreservation {
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 static JToken Normal(string json){
  var token=JObject.Parse(json);
  foreach(var p in token.DescendantsAndSelf().OfType<JProperty>().Where(p=>p.Name=="instanceID").ToArray())p.Value=0;
  return token;
 }
 public static string Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");
  var baseline=JObject.Parse(File.ReadAllText("../Documentation/CampusSportsProtectedBefore.json"));var differences=new List<string>();
  var behaviours=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Where(b=>b&&b.GetType().Namespace=="FireSafety").ToArray();
  foreach(var old in baseline["gameplay"]){var b=behaviours.SingleOrDefault(b=>PathOf(b.transform)==(string)old["path"]&&b.GetType().Name==(string)old["type"]);if(!b||!JToken.DeepEquals(Normal((string)old["json"]),Normal(EditorJsonUtility.ToJson(b))))differences.Add("Gameplay: "+old["path"]+" "+old["type"]);}
  var components=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t=>t.name=="Classroom - licensed source art"||t.name.EndsWith(" - environment only")).SelectMany(t=>t.GetComponentsInChildren<Component>(true)).Where(c=>c).ToDictionary(c=>GlobalObjectId.GetGlobalObjectIdSlow(c).ToString());
  foreach(var old in baseline["classroom"]){if(!components.TryGetValue((string)old["id"],out var c)||!JToken.DeepEquals(Normal((string)old["json"]),Normal(EditorJsonUtility.ToJson(c))))differences.Add("Classroom component: "+old["id"]);}
  bool assembly=JToken.DeepEquals(Normal((string)baseline["assembly"]),Normal(EditorJsonUtility.ToJson(UnityEngine.Object.FindAnyObjectByType<FireSafety.EvacuationManager>().assemblyZone)));
  var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);int missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
  var badMaterials=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude).Where(r=>r.sharedMaterials.Any(m=>!m||m.shader.name.Contains("InternalError"))).Select(r=>r.name).ToArray();
  bool counts=behaviours.Length==baseline["gameplay"].Count()&&components.Count==baseline["classroom"].Count();
  string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=differences.Count==0&&assembly&&missing==0&&badMaterials.Length==0&&counts,gameplayComponents=behaviours.Length,classroomComponents=components.Count,countsUnchanged=counts,differences,assemblyUnchanged=assembly,missingScripts=missing,badMaterials,scope="Saved scene compared with pre-campus checkpoint. Transient Unity instance IDs normalized across Editor restarts; serialized values and persistent classroom object IDs compared."},Newtonsoft.Json.Formatting.Indented);
  File.WriteAllText("../Documentation/CampusSportsPreservation.json",json);return json;
 }
}
