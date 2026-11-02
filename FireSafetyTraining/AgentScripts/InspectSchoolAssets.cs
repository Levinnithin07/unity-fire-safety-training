using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
public static class InspectSchoolAssets {
 public static string Run(){
 AssetDatabase.Refresh();
 var paths=AssetDatabase.FindAssets("t:Model",new[]{"Assets/ThirdParty/SchoolExpansion"}).Select(AssetDatabase.GUIDToAssetPath);
 var data=paths.Select(p=>{var g=AssetDatabase.LoadAssetAtPath<GameObject>(p);var mesh=AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>();return new {path=p,meshes=mesh.Select(m=>new {m.name,m.vertexCount,triangles=m.triangles.Length/3,size=m.bounds.size.ToString()}).ToArray(),materials=AssetDatabase.LoadAllAssetsAtPath(p).OfType<Material>().Select(m=>new {m.name,color=m.HasProperty("_Color")?m.color.ToString():"none"}).ToArray(),clips=AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().Select(c=>new{c.name,c.length}).ToArray()};}).ToArray();
 var path=Path.GetFullPath("../Documentation/SchoolAssetInventory.json");File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(data,Newtonsoft.Json.Formatting.Indented));return path;
 }
}
