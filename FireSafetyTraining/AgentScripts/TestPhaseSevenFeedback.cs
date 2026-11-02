using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;

public static class TestPhaseSevenFeedback
{
    public static async Task<string> Run()
    {
        var checks = new List<object>();
        void Check(string name, bool pass, object evidence = null) => checks.Add(new {name,pass,evidence});
        bool bg = Application.runInBackground;
        var oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
        var oldFocus = InputSystem.settings.backgroundBehavior;
        var oldK = Keyboard.current; Keyboard k = null;
        Application.runInBackground = true;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        await Task.Delay(600);
        var s = TrainingSession.Instance; var p = s.player;
        var feedback = p.GetComponent<TrainingFeedback>();
        int completionBaseline = feedback.CompletionCuesPlayed;
        GameObject wall = null;
        var q = s.settings.effectsQuality;
        var cc = p.GetComponent<CharacterController>();
        void Warp(Vector3 point) { cc.enabled=false;p.transform.SetPositionAndRotation(point,Quaternion.identity);cc.enabled=true;Physics.SyncTransforms(); }
        try {
            k = InputSystem.AddDevice<Keyboard>();
            s.MainMenu(); s.ChooseMode(TrainingMode.FireExtinguisher); s.StartSimulation();
            await Task.Delay(300);
            int baseline = feedback.StepsPlayed;
            await Task.Delay(500);
            Check("Standing still does not play footsteps", feedback.StepsPlayed == baseline);
            // Front classroom aisle; collision-resolved movement through the actual Input System.
            Warp(new Vector3(0,.025f,-1.8f)); await Task.Delay(200);p.SetCapture(true);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.D));await Task.Delay(700);
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(80);
            Check("Walking produces footsteps from actual travel",feedback.StepsPlayed>baseline,new{steps=feedback.StepsPlayed-baseline,position=p.transform.position.ToString()});
            float walkVolume=feedback.LastStepVolume; baseline=feedback.StepsPlayed;
            Warp(new Vector3(0,.025f,-1.8f));await Task.Delay(200);p.SetCapture(true);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.D,Key.LeftCtrl));await Task.Delay(1100);
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(80);
            Check("Crouching uses quieter footsteps",feedback.StepsPlayed>baseline&&feedback.LastStepVolume<walkVolume,new{walkVolume,crouchVolume=feedback.LastStepVolume});
            baseline=feedback.StepsPlayed;Warp(new Vector3(0,.025f,-1.8f));await Task.Delay(200);p.SetCapture(true);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.D,Key.LeftShift));await Task.Delay(650);
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(80);
            Check("Sprinting produces stronger footsteps",feedback.StepsPlayed>baseline&&feedback.LastStepVolume>walkVolume,new{walkVolume,sprintVolume=feedback.LastStepVolume});
            Warp(new Vector3(0,.025f,-1.8f));
            wall=new GameObject("TEMP Phase7 footstep collision fixture");wall.transform.position=new Vector3(.45f,1,-1.8f);wall.AddComponent<BoxCollider>().size=new Vector3(.1f,2,2);Physics.SyncTransforms();
            await Task.Delay(200);baseline=feedback.StepsPlayed;p.SetCapture(true);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.D));await Task.Delay(900);
            InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(80);
            Check("Holding movement against a wall does not create footsteps",feedback.StepsPlayed==baseline,new{position=p.transform.position.ToString()});
            UnityEngine.Object.DestroyImmediate(wall);wall=null;Physics.SyncTransforms();
            baseline=feedback.StepsPlayed;
            s.Pause();InputSystem.QueueStateEvent(k,new KeyboardState(Key.D));await Task.Delay(300);
            Check("Pause suppresses footsteps",feedback.StepsPlayed==baseline&&!feedback.footsteps.isPlaying);
            InputSystem.QueueStateEvent(k,new KeyboardState());s.Resume();await Task.Delay(150);
            baseline=feedback.StepsPlayed;Warp(new Vector3(2,.025f,-1.8f));await Task.Delay(250);
            Check("Reset or teleport does not create a footstep",feedback.StepsPlayed==baseline);

            // The generated mono PCM clips must contain an unclipped signal with silent boundaries.
            foreach(var clip in feedback.footstepClips.Concat(new[]{feedback.completionClip})) {
                var samples=new float[clip.samples*clip.channels];bool read=clip.GetData(samples,0);
                double energy=samples.Select(x=>(double)x*x).Average();float peak=samples.Max(x=>Mathf.Abs(x));
                Check("Valid original audio: "+clip.name,read&&clip.channels==1&&energy>.00001&&peak<.9f&&Mathf.Abs(samples[0])<.001f&&Mathf.Abs(samples[samples.Length-1])<.001f,new{rms=Math.Sqrt(energy),peak,clip.length});
            }
            s.Pause();int original=s.settings.Current.quality;float fire=s.simulation.sources[0].intensity;var totals=new List<int>();
            for(int i=0;i<3;i++) {
                s.settings.BeginEdit();s.settings.Draft.quality=i;s.settings.Preview();
                totals.Add(q.TotalParticleLimit);
                Check("Preset "+i+" applies every particle budget",q.AppliedQuality==i&&q.budgets.All(b=>b.particles.main.maxParticles==Mathf.Max(8,Mathf.CeilToInt(b.highLimit*(i==0?.55f:i==1?.8f:1f)))));
                s.settings.Cancel();
                Check("Cancel preset "+i+" restores rendering budget",q.AppliedQuality==original);
            }
            Check("Low Medium High have increasing visual budgets",totals[0]<totals[1]&&totals[1]<totals[2],totals);
            Check("Quality changes do not alter fire physics",s.simulation.sources[0].intensity==fire);
            Check("No false extinguisher success cue before suppression",feedback.CompletionCuesPlayed==completionBaseline);
            s.MainMenu();
        } finally {
            if(wall)UnityEngine.Object.DestroyImmediate(wall);
            if(k!=null)InputSystem.RemoveDevice(k);if(oldK!=null&&oldK.added)oldK.MakeCurrent();
            InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Editor Play Mode; virtual movement input with front-aisle placement fixtures; PCM data and settings preview/cancel checks",checks},Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.GetFullPath("../Documentation/Phase7FeedbackTests.json"),json);return json;
    }
}
