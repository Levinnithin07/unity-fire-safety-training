using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.Rendering;
public static class RefineCampus {
 public static object Run(){
 const string R="Assets/_FireSafety/CampusExterior/";
 for(int lod=0;lod<2;lod++)using(var r=new BinaryReader(File.OpenRead("../SourceAssets/CampusExterior/tree-campus-lod"+lod+".bin"))){int parts=r.ReadInt32();var v=new List<Vector3>();var uv=new List<Vector2>();var norm=new List<Vector3>();var subs=new List<int[]>();for(int s=0;s<parts;s++){int n=r.ReadInt32(),count=r.ReadInt32(),offset=v.Count;for(int i=0;i<n;i++){v.Add(new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle()));uv.Add(new Vector2(r.ReadSingle(),r.ReadSingle()));norm.Add(new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle()).normalized);}var ids=new int[count];for(int i=0;i<count;i++)ids[i]=r.ReadInt32()+offset;subs.Add(ids);}var m=AssetDatabase.LoadAssetAtPath<Mesh>(R+"Meshes/LeafyTree-LOD"+lod+".asset");m.Clear();m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetUVs(0,uv);m.SetNormals(norm);m.subMeshCount=parts;for(int i=0;i<parts;i++)m.SetTriangles(subs[i],i);m.RecalculateBounds();m.RecalculateTangents();EditorUtility.SetDirty(m);}
 // Use the actual bush variant rather than a single stem of the licensed collection.
 var full=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/SchoolExpansion/shrub_01/shrub_01.fbx");var source=full.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("_g_"));
 foreach(var bush in GameObject.Find("Campus exterior upgrade/Campus leafy trees").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Fuller campus shrub"))){var filter=bush.GetComponentInChildren<MeshFilter>();filter.sharedMesh=source.sharedMesh;var rs=bush.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var rr in rs)b.Encapsulate(rr.bounds);bush.localScale*=.9f/b.size.y;b=rs[0].bounds;foreach(var rr in rs)b.Encapsulate(rr.bounds);var p=bush.position;bush.position+=new Vector3(p.x-b.center.x,p.y-b.min.y,p.z-b.center.z);}
 var floor=GameObject.Find("Campus exterior upgrade/Ground floor covered corridor/Architecture-12");floor.transform.position+=Vector3.up*.018f;floor.GetComponent<Renderer>().receiveShadows=false;
 GameObject.Find("Campus exterior upgrade/Ground floor covered corridor/Architecture-13").SetActive(false);
 foreach(var r in GameObject.Find("Campus exterior upgrade/Decorative upper storey - no access").GetComponentsInChildren<Renderer>())r.receiveShadows=false;
 foreach(var t in GameObject.Find("Campus exterior upgrade/Campus school buses").GetComponentsInChildren<TMPro.TMP_Text>())if(t.text=="SCHOOL BUS"){t.transform.localPosition=new Vector3(-5.26f,2.17f,0);t.fontSize=1.45f;}
 EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();return "Leaf UVs preserved, full bushes, overlapping tile accents removed, bus labels adjusted";
 }
}

