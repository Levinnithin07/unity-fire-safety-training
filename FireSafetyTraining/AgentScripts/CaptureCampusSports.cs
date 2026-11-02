using System;using System.IO;using System.Threading.Tasks;using UnityEngine;using UnityEngine.Rendering.Universal;using FireSafety;
public static class CaptureCampusSports {
 public static async Task<string> Run(){
  if(!Application.isPlaying)throw new Exception("Play Mode required");var s=TrainingSession.Instance;s.MainMenu();s.player.enabled=false;
  var camera=s.player.view.GetComponent<Camera>();var oldPos=camera.transform.localPosition;var oldRot=camera.transform.localRotation;float oldFov=camera.fieldOfView;
  string dir=Path.GetFullPath("../Documentation/CampusSportsScreenshots");Directory.CreateDirectory(dir);
  var positions=new[]{new Vector3(39,29,-30),new Vector3(18,5,-.2f),new Vector3(31,6,4.5f),new Vector3(25,5,-3.5f),new Vector3(20,2.2f,18.8f)};
  var targets=new[]{new Vector3(15,0,0),new Vector3(7.28f,5.93f,-.2f),new Vector3(20,1,14),new Vector3(13,1,-13),new Vector3(20,1.3f,22.5f)};
  var names=new[]{"01-Complete-campus","02-University-name-board","03-Basketball-court","04-Volleyball-court","05-Seating-and-landscaping"};
  for(int i=0;i<names.Length;i++){camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);camera.fieldOfView=i==1?48:65;await Task.Delay(250);var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,names[i]+".png"),tex.EncodeToPNG());camera.targetTexture=oldTarget;RenderTexture.active=oldActive;UnityEngine.Object.Destroy(tex);rt.Release();UnityEngine.Object.Destroy(rt);}
  camera.transform.localPosition=oldPos;camera.transform.localRotation=oldRot;camera.fieldOfView=oldFov;s.player.enabled=true;return dir;
 }
}




