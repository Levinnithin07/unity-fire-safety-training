using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public static class InspectExtinguisher
{
 public static string Run(){
 var go=GameObject.Find("korean_fire_extinguisher_01_2k");var mesh=go.GetComponent<MeshFilter>().sharedMesh;var v=mesh.vertices;
 var parent=Enumerable.Range(0,v.Length).ToArray();Func<int,int> find=null;find=i=>parent[i]==i?i:parent[i]=find(parent[i]);
 Action<int,int> join=(a,b)=>parent[find(a)]=find(b);
 var positions=new Dictionary<string,int>();for(int i=0;i<v.Length;i++){var p=v[i];string key=$"{Mathf.RoundToInt(p.x*1000000)},{Mathf.RoundToInt(p.y*1000000)},{Mathf.RoundToInt(p.z*1000000)}";if(positions.TryGetValue(key,out int n))join(i,n);else positions[key]=i;}
 for(int s=0;s<mesh.subMeshCount;s++){var t=mesh.GetTriangles(s);for(int i=0;i<t.Length;i+=3){join(t[i],t[i+1]);join(t[i],t[i+2]);}}
 var groups=Enumerable.Range(0,v.Length).GroupBy(find).OrderByDescending(g=>g.Count()).ToArray();
 var result=groups.Select((g,id)=>{var indices=g.ToArray();var b=new Bounds(go.transform.TransformPoint(v[indices[0]])-go.transform.position,Vector3.zero);foreach(int i in indices)b.Encapsulate(go.transform.TransformPoint(v[i])-go.transform.position);return new{id,vertices=indices.Length,center=b.center.ToString("F4"),size=b.size.ToString("F4"),indices};}).ToArray();
 string json=Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase4ExtinguisherIslands.json"),json);
 return Newtonsoft.Json.JsonConvert.SerializeObject(result.Select(g=>new{g.id,g.vertices,g.center,g.size}));
 }
}
