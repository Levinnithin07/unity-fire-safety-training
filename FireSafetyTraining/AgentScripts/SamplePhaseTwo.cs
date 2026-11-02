using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
public static class SamplePhaseTwo
{
 public static async Task<string> Run(){
 if(!Application.isPlaying)throw new Exception("Play Mode required");
 var values=new List<float>();int last=Time.frameCount;double start=Time.realtimeSinceStartupAsDouble;
 while(Time.realtimeSinceStartupAsDouble-start<12){await Task.Delay(8);if(Time.frameCount!=last){last=Time.frameCount;values.Add(Time.unscaledDeltaTime*1000);}}
 values.Sort();
 var report=new {scope="12-second stationary Unity Editor Game view sample; not a standalone benchmark",gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,samples=values.Count,medianFrameMs=values.Count>0?values[values.Count/2]:0,p95FrameMs=values.Count>0?values[(int)((values.Count-1)*.95)]:0,averageSampledFps=values.Count>0?1000f/values.Average():0};
 string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase2EditorPerformance.json"),json);return json;
 }
}
