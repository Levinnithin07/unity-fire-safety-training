using System;using System.IO;using System.Threading.Tasks;using UnityEngine;using UnityEngine.Rendering.Universal;using FireSafety;
public static class CaptureCampus {
 public static async Task<string> Run(){
  if(!Application.isPlaying)throw new Exception("Play Mode required");var s=TrainingSession.Instance;s.MainMenu();s.player.enabled=false;
  var camera=s.player.view.GetComponent<Camera>();var oldPos=camera.transform.localPosition;var oldRot=camera.transform.localRotation;float oldFov=camera.fieldOfView;
  string dir=Path.GetFullPath("../Documentation/CampusScreenshots");Directory.CreateDirectory(dir);
  var positions=new[]{new Vector3(29,2.5f,-.2f),new Vector3(18,5,-.2f),new Vector3(5.7f,1.65f,-2),new Vector3(29,2.1f,9),new Vector3(27,4.3f,-7),new Vector3(18.8f,2.2f,4.6f)};
  var targets=new[]{new Vector3(3.5f,3.3f,-.2f),new Vector3(7.2f,5.9f,-.2f),new Vector3(3.25f,1.75f,.6f),new Vector3(34.15f,1.6f,-.2f),new Vector3(27.6f,1.2f,-19),new Vector3(13.3f,.9f,-.2f)};
  var names=new[]{"01-Two-storey-academic-block","02-University-name-board","03-Corridor-and-classroom-window","04-Campus-wall-and-main-gate","05-School-bus-parking","06-Assembly-without-upright-board"};
  for(int i=0;i<names.Length;i++){camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);camera.fieldOfView=i==1?48:65;await Task.Delay(250);var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,names[i]+".png"),tex.EncodeToPNG());camera.targetTexture=oldTarget;RenderTexture.active=oldActive;UnityEngine.Object.Destroy(tex);rt.Release();UnityEngine.Object.Destroy(rt);}
  camera.transform.localPosition=oldPos;camera.transform.localRotation=oldRot;camera.fieldOfView=oldFov;s.player.enabled=true;return dir;
 }
}



