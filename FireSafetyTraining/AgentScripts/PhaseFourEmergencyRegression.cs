using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;

public static class PhaseFourEmergencyRegression
{
    static readonly List<object> checks = new List<object>();
    static FireManager m;
    static void Check(string name, bool pass, object evidence) => checks.Add(new { name, pass, evidence });
    static void Advance(float seconds) { for(int i=0;i<Mathf.RoundToInt(seconds/.05f);i++) m.StepSimulation(.05f); }
    public static async Task<string> Run()
    {
        if(!Application.isPlaying) throw new Exception("Play Mode required");
        m=UnityEngine.Object.FindAnyObjectByType<FireManager>(); checks.Clear();
        bool previousBackground=Application.runInBackground;
        var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        var oldFocus=InputSystem.settings.backgroundBehavior;
        var oldKeyboard=Keyboard.current; Keyboard keyboard=null;
        Application.runInBackground=true;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        try {
            UnityEditor.EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();
            m.ResetScenario(); int frame=Time.frameCount; await Task.Delay(500);
            Check("Actual Play Mode frames advance",Time.frameCount>frame,Time.frameCount-frame);
            Check("Safe exploration before manual start",!m.Running&&m.TotalIntensity==0&&!m.alarm.Active&&m.ActiveSprinklers==0,"No automatic emergency on scene load");
            keyboard=InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));await Task.Delay(160);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(160);
            Check("Enter binding starts fire",m.Running&&m.sources[0].Burning,m.Elapsed);
            Check("Fire emits actual particles and sound",m.sources[0].flames.particleCount>0&&m.sources[0].crackle.isPlaying,new{particles=m.sources[0].flames.particleCount,audio=m.sources[0].crackle.isPlaying});
            m.enabled=false;m.ResetScenario();m.StartScenario();
            foreach(var f in m.sources) f.ResetFire(); Advance(180);
            Check("No fire means no detector or sprinkler activation after 180s",!m.alarm.Active&&m.ActiveSprinklers==0&&m.SmokeLevel==0,new{m.Elapsed,m.SmokeLevel});
            m.ResetScenario();m.StartScenario(); Advance(8);
            Check("Fire grows from initial intensity",m.sources[0].intensity>.30f,m.sources[0].intensity);
            Check("Smoke accumulates from burning fuel",m.SmokeLevel>0,m.SmokeLevel);
            Check("Below threshold detector stays idle",m.detectors[0].Reading<m.detectors[0].threshold&&!m.detectors[0].Activated,new{m.detectors[0].Reading,m.detectors[0].threshold});
            m.ResetScenario();m.StartScenario();
            var timeline=new List<object>();int peakBurning=0;float peakIntensity=0;float firstWaterIntensity=0;
            for(int i=0;i<2400;i++) {
                m.StepSimulation(.05f);peakBurning=Mathf.Max(peakBurning,m.BurningCount);peakIntensity=Mathf.Max(peakIntensity,m.TotalIntensity);
                if(firstWaterIntensity==0&&m.ActiveSprinklers>0)firstWaterIntensity=m.TotalIntensity;
                if(i%20==0)timeline.Add(new{time=m.Elapsed,intensity=m.TotalIntensity,smoke=m.SmokeLevel,burning=m.BurningCount,alarm=m.alarm.Active,water=m.ActiveSprinklers,temperature=m.sprinklers[0].Temperature});
            }
            Check("Smoke threshold causes alarm",m.detectors[0].Activated&&m.alarm.Active&&m.alarm.ActivatedAt==m.detectors[0].ActivatedAt,new{m.alarm.ActivatedAt,m.alarm.TriggeredBy});
            Check("Predefined neighboring desk catches fire",peakBurning>=2,new{peakBurning,ignitionTimes=m.sources.Select(f=>f.IgnitedAt).ToArray()});
            Check("Alarm precedes heat sprinkler",m.alarm.ActivatedAt>0&&m.sprinklers[0].ActivatedAt>m.alarm.ActivatedAt,new{alarm=m.alarm.ActivatedAt,water=m.sprinklers[0].ActivatedAt});
            Check("Only locally heated sprinkler activates",m.sprinklers[0].Active&&!m.sprinklers[1].Active,m.sprinklers.Select(s=>new{s.name,s.Active,s.Temperature,s.ActivatedAt}).ToArray());
            Check("Suppression is gradual, not immediate",firstWaterIntensity>.2f,firstWaterIntensity);
            Check("Water reduces and suppresses local fires",m.TotalIntensity<peakIntensity*.3f&&m.sources.Any(f=>f.WaterApplied>0),new{peakIntensity,final=m.TotalIntensity,water=m.sources.Select(f=>f.WaterApplied).ToArray()});
            Check("Coverage excludes distant and above-head sources",m.sprinklers[0].SuppressionAt(new Vector3(5,.9f,2))==0&&m.sprinklers[0].SuppressionAt(new Vector3(-1.5f,3.2f,.8f))==0,"Two out-of-coverage probes");
            Check("Alarm and sprinkler stay latched after cooling",m.alarm.Active&&m.sprinklers[0].Active,"Alarm/water persist until explicit reset");
            m.ResetScenario();
            for(int i=0;i<2000;i++){m.detectors[0].Step(.5f,.05f,i*.05f);foreach(var s in m.sprinklers)s.Step(22,.05f,i*.05f);}
            Check("Smoke alone does not trigger sprinklers",m.alarm.Active&&m.ActiveSprinklers==0,"100s of smoke input, ambient heat");
            m.ResetScenario();
            for(int i=0;i<500;i++) {m.sprinklers[0].Step(100,.05f,i*.05f);m.sprinklers[1].Step(22,.05f,i*.05f);}
            Check("Heat independently activates one head without smoke alarm",m.sprinklers[0].Active&&!m.sprinklers[1].Active&&!m.alarm.Active,new{m.sprinklers[0].Temperature,m.alarm.Active});
            m.ResetScenario();m.StartScenario();Advance(30);await Task.Delay(600);
            Check("Active water has visible particles and playing source",m.sprinklers[0].Active&&m.sprinklers[0].water.particleCount>0&&m.sprinklers[0].waterSound.isPlaying,new{active=m.sprinklers[0].Active,particles=m.sprinklers[0].water.particleCount,audio=m.sprinklers[0].waterSound.isPlaying});
            Check("Alarm audio source is playing",m.alarm.sounder.isPlaying,new{clip=m.alarm.sounder.clip.name,m.alarm.sounder.isPlaying});
            Check("Smoke is bounded below ceiling",SmokeBelowCeiling(),"All live smoke particles y <= 2.66m");
            m.enabled=true;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.M));await Task.Delay(140);InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(140);
            Check("M toggles alarm mute without clearing emergency",m.alarm.Muted&&m.alarm.sounder.mute&&m.alarm.Active,m.alarm.Muted);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));await Task.Delay(160);InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(160);
            Check("R resets all emergency state",!m.Running&&m.TotalIntensity==0&&!m.alarm.Active&&m.ActiveSprinklers==0&&m.SmokeLevel==0,m.Elapsed);
            Check("Reset clears audio, effects and restores sprinkler bulb",!m.alarm.sounder.isPlaying&&m.sources.All(f=>f.flames.particleCount==0&&!f.crackle.isPlaying)&&m.sprinklers.All(s=>s.water.particleCount==0&&!s.waterSound.isPlaying&&s.glassBulb.activeSelf),"All active systems cleared");
            m.alarm.ToggleMute();
            var renderers=UnityEngine.Object.FindObjectsByType<Renderer>();
            var broken=renderers.SelectMany(r=>r.sharedMaterials).Count(mat=>!mat||!mat.shader||!mat.shader.isSupported);
            Check("No missing or unsupported materials",broken==0,broken);
            int missing=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>UnityEditor.GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
            Check("No missing scripts",missing==0,missing);
            string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{checks,timeline},Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.GetFullPath("../Documentation/Phase4EmergencyRegression.json"),json);return json;
        } finally {
            m.ResetScenario();m.enabled=true;
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(oldKeyboard!=null&&oldKeyboard.added)oldKeyboard.MakeCurrent();
            InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=previousBackground;
        }
    }
    static bool SmokeBelowCeiling()
    {
        var particles=new ParticleSystem.Particle[100];
        foreach(var f in m.sources){int n=f.smoke.GetParticles(particles);for(int i=0;i<n;i++)if(particles[i].position.y>2.66f)return false;}
        return true;
    }
}

