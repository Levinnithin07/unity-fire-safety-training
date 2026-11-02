using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
public static class InspectEnvironment
{
 [Serializable] public class Item {public string name,path;public Vector3 center,size,position;}
 [Serializable] public class Inventory {public string scene; public Item[] items;}
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 public static string Run(){
 var i=new Inventory {scene=EditorSceneManager.GetActiveScene().path,items=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Select(r=>new Item{name=r.name,path=PathOf(r.transform),center=r.bounds.center,size=r.bounds.size,position=r.transform.position}).ToArray()};
 File.WriteAllText(System.IO.Path.GetFullPath("../Documentation/EnvironmentInventory.json"),Newtonsoft.Json.JsonConvert.SerializeObject(i, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore }));
 return i.scene+" : "+i.items.Length+" renderers";
 }
}

