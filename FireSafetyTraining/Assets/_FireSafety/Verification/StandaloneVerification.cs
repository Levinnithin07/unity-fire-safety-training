using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FireSafety
{
    // Explicit local QA command-line mode; never instantiated during normal play.
    public sealed class StandaloneVerification : MonoBehaviour
    {
        public static string OutputDirectory { get; private set; }
        static bool resume, displayOnly;
        readonly List<object> checks = new List<object>(), performance = new List<object>();
        readonly List<string> errors = new List<string>();
        readonly List<float> frames = new List<float>();
        bool sampling;
        int focusedFrames;
        static readonly string[] IntKeys={"Training.Fullscreen","Training.DisplayWidth","Training.DisplayHeight","Training.Quality"};
        static readonly string[] FloatKeys={"Training.Volume","Training.MouseSensitivity"};
        [Serializable] class Pref { public string key; public bool exists, floating; public float value; }
        [Serializable] class Prefs { public Pref[] items; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if(Application.isEditor)return;
            var args=Environment.GetCommandLineArgs();
            var argument=args.FirstOrDefault(a=>a.StartsWith("--training-verify="));
            if(argument==null)return;
            OutputDirectory=Path.GetFullPath(argument.Substring("--training-verify=".Length));
            Directory.CreateDirectory(OutputDirectory);resume=args.Contains("--training-verify-resume");
            displayOnly=args.Contains("--training-display-only");
            string backup=Path.Combine(OutputDirectory,"PreferencesBackup.json");
            if(!resume){
                if(!File.Exists(backup))File.WriteAllText(backup,JsonUtility.ToJson(new Prefs{items=IntKeys.Select(k=>new Pref{key=k,exists=PlayerPrefs.HasKey(k),value=PlayerPrefs.GetInt(k)}).Concat(FloatKeys.Select(k=>new Pref{key=k,exists=PlayerPrefs.HasKey(k),floating=true,value=PlayerPrefs.GetFloat(k)})).ToArray()},true));
                foreach(var k in IntKeys.Concat(FloatKeys))PlayerPrefs.DeleteKey(k);
                PlayerPrefs.Save();
            }
            var go=new GameObject("Opt-in standalone verification");DontDestroyOnLoad(go);go.AddComponent<StandaloneVerification>();
        }
        void Awake(){Application.runInBackground=true;Application.logMessageReceived+=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+trace);}
        void Update(){if(sampling){if(TrainingSession.Instance.Page=="Pause")TrainingSession.Instance.Resume();frames.Add(Time.unscaledDeltaTime);if(Application.isFocused)focusedFrames++;}}
        void Check(string name,bool pass,object evidence=null){checks.Add(new{name,pass,evidence});Progress(name);}
        void Progress(string text)=>File.WriteAllText(Path.Combine(OutputDirectory,"Progress.txt"),DateTime.UtcNow.ToString("o")+" "+text);
        void Save(string file,object value)=>File.WriteAllText(Path.Combine(OutputDirectory,file),Newtonsoft.Json.JsonConvert.SerializeObject(value,Newtonsoft.Json.Formatting.Indented));
        void RestorePreferences()
        {
            var prefs=JsonUtility.FromJson<Prefs>(File.ReadAllText(Path.Combine(OutputDirectory,"PreferencesBackup.json")));
            foreach(var p in prefs.items)if(!p.exists)PlayerPrefs.DeleteKey(p.key);else if(p.floating)PlayerPrefs.SetFloat(p.key,p.value);else PlayerPrefs.SetInt(p.key,(int)p.value);
            PlayerPrefs.Save();
        }
        async void Start()
        {
            try {
                await Task.Delay(2500);
                var s=TrainingSession.Instance;var settings=s.settings;
                Save(resume?"RelaunchHardware.json":"Hardware.json",new{SystemInfo.operatingSystem,SystemInfo.processorType,SystemInfo.systemMemorySize,SystemInfo.graphicsDeviceName,SystemInfo.graphicsMemorySize,SystemInfo.graphicsDeviceVersion,Screen.width,Screen.height,fullscreen=Screen.fullScreenMode.ToString(),nativeWidth=Screen.currentResolution.width,nativeHeight=Screen.currentResolution.height,Application.unityVersion,Application.version,settings.Current});
                if(resume){
                    Check("Saved settings survive executable restart",!settings.Current.fullscreen&&settings.Current.width==1280&&settings.Current.height==720&&settings.Current.quality==0&&Screen.fullScreenMode==FullScreenMode.Windowed&&Screen.width==1280&&Screen.height==720);
                    ScreenCapture.CaptureScreenshot(Path.Combine(OutputDirectory,"Relaunch-Windowed.png"));await Task.Delay(400);
                    Save("Relaunch.json",new{checks,errors,exitRequested=true});
                    s.ui.Activate("ExitGame");RestorePreferences();return;
                }
                Check("Fresh launch is native borderless fullscreen",Screen.fullScreenMode==FullScreenMode.FullScreenWindow&&Screen.width==Screen.currentResolution.width&&Screen.height==Screen.currentResolution.height);
                Check("Fresh launch opens main menu",s.Page=="Main");
                ScreenCapture.CaptureScreenshot(Path.Combine(OutputDirectory,"MainMenu-Fullscreen.png"));await Task.Delay(300);
                s.OpenSettings();settings.Draft.fullscreen=false;settings.Draft.width=1280;settings.Draft.height=720;settings.Preview();await Task.Delay(1200);
                var displaySamples=new List<object>();
                for(int i=0;i<40;i++){
                    displaySamples.Add(new{Screen.width,Screen.height,mode=Screen.fullScreenMode.ToString()});
                    if(Screen.fullScreenMode==FullScreenMode.Windowed&&Screen.width==1280&&Screen.height==720)break;
                    await Task.Delay(100);
                }
                Check("Windowed 1280x720 display preview applies",Screen.fullScreenMode==FullScreenMode.Windowed&&Screen.width==1280&&Screen.height==720,displaySamples);
                settings.Cancel();await Task.Delay(1200);
                Check("Cancel restores fullscreen",Screen.fullScreenMode==FullScreenMode.FullScreenWindow);
                var oldKeyboard=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftAlt,Key.Enter));await Task.Delay(150);InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(1000);
                Check("Alt Enter switches to windowed",Screen.fullScreenMode==FullScreenMode.Windowed);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftAlt,Key.Enter));await Task.Delay(150);InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(1000);
                Check("Alt Enter restores fullscreen",Screen.fullScreenMode==FullScreenMode.FullScreenWindow);
                InputSystem.RemoveDevice(keyboard);if(oldKeyboard!=null&&oldKeyboard.added)oldKeyboard.MakeCurrent();
                if(displayOnly){
                    Save("DisplayOnly.json",new{checks,errors});RestorePreferences();Application.Quit();return;
                }
                s.MainMenu();
                if(Environment.GetCommandLineArgs().Contains("--training-campus-sports-checks"))
                    await CampusSportsChecks.Run(OutputDirectory);
                if(Environment.GetCommandLineArgs().Contains("--training-placement-checks"))
                    await FinalPlacementChecks.Run(OutputDirectory);
                Progress("Standalone extinguisher workflow");
                var extinguisherReport=Newtonsoft.Json.Linq.JObject.Parse(await StandaloneExtinguisherChecks.Run());
                if(extinguisherReport["checks"].Any(c=>!(bool)c["pass"]))throw new Exception("Standalone extinguisher checks failed; inspect Extinguisher.json.");
                Progress("Standalone evacuation workflow");
                var evacuationReport=Newtonsoft.Json.Linq.JObject.Parse(await StandaloneEvacuationChecks.Run());
                if(evacuationReport["checks"].Any(c=>!(bool)c["pass"]))throw new Exception("Standalone evacuation checks failed; inspect Evacuation.json.");
                foreach(var mode in new[]{TrainingMode.FireExtinguisher,TrainingMode.Evacuation})foreach(int quality in new[]{0,1,2}){
                    s.MainMenu();settings.BeginEdit();settings.Draft.quality=quality;settings.Preview();settings.Keep();
                    s.ChooseMode(mode);s.StartSimulation();
                    if(mode==TrainingMode.Evacuation){s.simulation.enabled=false;for(int i=0;i<660;i++)s.simulation.StepSimulation(.05f);}
                    s.player.enabled=false;s.player.view.LookAt(s.simulation.sources[0].transform.position+Vector3.up*.6f);
                    Progress("Performance "+mode+" quality "+quality);await Task.Delay(2500);
                    frames.Clear();focusedFrames=0;sampling=true;await Task.Delay(8000);sampling=false;
                    var sorted=frames.OrderBy(x=>x).ToArray();
                    performance.Add(new{mode=mode.ToString(),quality,Screen.width,Screen.height,renderScale=((UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.renderPipeline).renderScale,frames=sorted.Length,averageFps=sorted.Length/frames.Sum(),medianMs=sorted[sorted.Length/2]*1000,p95Ms=sorted[(int)(sorted.Length*.95f)]*1000,focusedFrames,particles=s.settings.effectsQuality.budgets.Sum(b=>b.particles.particleCount)});
                    ScreenCapture.CaptureScreenshot(Path.Combine(OutputDirectory,"Performance-"+mode+"-"+quality+".png"));await Task.Delay(200);
                    s.simulation.enabled=true;s.player.enabled=true;
                }
                s.MainMenu();settings.BeginEdit();settings.Draft.fullscreen=false;settings.Draft.width=1280;settings.Draft.height=720;settings.Draft.quality=0;settings.Preview();await Task.Delay(1200);settings.Keep();
                Check("Settings Keep stores test preference",PlayerPrefs.GetInt("Training.Fullscreen",1)==0&&PlayerPrefs.GetInt("Training.DisplayWidth")==1280&&PlayerPrefs.GetInt("Training.Quality",1)==0);
                Check("No runtime errors observed",errors.Count==0,errors.ToArray());
                Save("Standalone.json",new{checks,performance,errors,exitRequested=true,scope="Release Windows executable; automated fixtures and virtual Input System events, real rendering. Performance uses fixed viewpoint, 2.5s warmup and 8s sampling per preset; evacuation hazard state frozen after 33 simulated seconds."});
                Progress("Complete; quitting through menu");s.ui.Activate("ExitGame");
            } catch(Exception ex){Save("Failure.json",new{exception=ex.ToString(),checks,errors});RestorePreferences();Application.Quit(2);}
        }
        void OnDestroy(){Application.logMessageReceived-=Log;}
    }
}
