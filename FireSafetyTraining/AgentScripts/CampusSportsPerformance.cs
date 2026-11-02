using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Threading.Tasks;using UnityEngine;using UnityEditor;using FireSafety;
public static class CampusSportsPerformance {
 static async Task<object> Sample(string label){
  var samples=new List<float>();double start=Time.realtimeSinceStartupAsDouble;int last=Time.frameCount;
  while(Time.realtimeSinceStartupAsDouble-start<8){await Task.Delay(5);if(Time.frameCount!=last){last=Time.frameCount;samples.Add(Time.unscaledDeltaTime*1000);}}
  samples.Sort();return new{label,samples=samples.Count,averageFps=1000/samples.Average(),medianMs=samples[samples.Count/2],p95Ms=samples[(int)((samples.Count-1)*.95)],drawCalls=UnityStats.drawCalls,triangles=UnityStats.triangles};
 }
 public static async Task<string> Run(){
  Application.runInBackground=true;var s=TrainingSession.Instance;var p=s.player;var v=p.view;var local=v.localPosition;var rotation=v.localRotation;var data=new List<object>();
  s.MainMenu();s.settings.BeginEdit();s.settings.Draft.quality=1;s.settings.Preview();s.ChooseMode(TrainingMode.FireExtinguisher);s.StartSimulation();p.enabled=false;
  try{
   v.position=new Vector3(28,1.8f,4.5f);v.LookAt(new Vector3(20,1,14));await Task.Delay(2000);data.Add(await Sample("Basketball and north seating at player height"));
   s.settings.Cancel();s.settings.BeginEdit();s.settings.Draft.quality=1;s.settings.Preview();
   v.position=new Vector3(9,1.8f,-3.5f);v.LookAt(new Vector3(13,1,-13));await Task.Delay(2000);data.Add(await Sample("Volleyball garden and bus parking at player height"));
  }finally{v.localPosition=local;v.localRotation=rotation;p.enabled=true;s.MainMenu();s.settings.Cancel();}
  var report=new{scope="Editor Game view, Medium preview, 2s warmup and 8s samples at two outdoor viewpoints; standalone player not running. Not standalone outdoor FPS certification.",gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,samples=data};
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText("../Documentation/CampusSportsEditorPerformance.json",json);return json;
 }
}
