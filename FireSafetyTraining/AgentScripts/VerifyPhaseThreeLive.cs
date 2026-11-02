using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FireSafety;
public static class VerifyPhaseThreeLive
{
    public static async Task<string> Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();
        bool previousBackground=Application.runInBackground;Application.runInBackground=true;
        m.player.SetCapture(false);m.player.transform.rotation=Quaternion.Euler(0,-30,0);
        m.ResetScenario();m.enabled=true;m.StartScenario();
        var values=new List<float>();var milestones=new List<object>();int last=Time.frameCount;int peakParticles=0,peakFires=0;
        float alarmTime=-1,waterTime=-1,maxAudioRms=0;var samples=new float[1024];
        double start=Time.realtimeSinceStartupAsDouble;
        try {
            while(Time.realtimeSinceStartupAsDouble-start<36){
                await Task.Delay(8);if(Time.frameCount==last)continue;last=Time.frameCount;values.Add(Time.unscaledDeltaTime*1000);
                peakFires=Mathf.Max(peakFires,m.BurningCount);
                peakParticles=Mathf.Max(peakParticles,UnityEngine.Object.FindObjectsByType<ParticleSystem>().Sum(p=>p.particleCount));
                if(m.alarm.Active&&alarmTime<0){alarmTime=m.Elapsed;milestones.Add(new{eventName="Alarm",time=m.Elapsed,smoke=m.detectors[0].Reading});}
                if(m.ActiveSprinklers>0&&waterTime<0){waterTime=m.Elapsed;milestones.Add(new{eventName="Water",time=m.Elapsed,temperature=m.sprinklers[0].Temperature});}
                if(m.alarm.sounder.isPlaying){m.alarm.sounder.GetOutputData(samples,0);float squares=0;foreach(float v in samples)squares+=v*v;maxAudioRms=Mathf.Max(maxAudioRms,Mathf.Sqrt(squares/samples.Length));}
            }
            values.Sort();
            var report=new{scope="36-second real-time Unity Editor observation from ignition through local sprinkler activation; Game view at 1920x1080; not a standalone benchmark",gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,samples=values.Count,averageSampledFps=values.Count>0?1000f/values.Average():0,medianFrameMs=values.Count>0?values[values.Count/2]:0,p95FrameMs=values.Count>0?values[(int)((values.Count-1)*.95)]:0,simulatedSeconds=m.Elapsed,peakFires,peakParticles,maxAlarmOutputRms=maxAudioRms,alarmTime,waterTime,passed=alarmTime>0&&waterTime>alarmTime&&peakFires>=2&&m.sprinklers[0].water.particleCount>0&&maxAudioRms>0,milestones};
            string json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase3LiveObservation.json"),json);return json;
        } finally {m.enabled=false;Application.runInBackground=previousBackground;}
    }
}
