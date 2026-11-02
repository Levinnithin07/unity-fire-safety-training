using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace FireSafety
{
    [DefaultExecutionOrder(-400)]
    public sealed class TrainingSettings : MonoBehaviour
    {
        [Serializable] public struct Values { public bool fullscreen; public int width,height,quality; public float volume,sensitivity; }
        public DesktopPlayerController player;
        public TrainingEffectsQuality effectsQuality;
        public RenderPipelineAsset[] pipelines;
        public Values Current { get; private set; }
        public Values Draft;
        public bool AwaitingConfirmation { get; private set; }
        public float ConfirmSeconds { get; private set; }
        public Vector2Int[] Resolutions { get; private set; }
        public string Status { get; private set; }="";
        Values before;
        bool pendingWindowSize;
        int windowWidth,windowHeight,windowRetries;
        float nextWindowRetry;
        RenderPipelineAsset oldPipeline;int oldVsync,oldTarget,oldTexture;float oldLod,oldVolume;
        void Awake()
        {
            oldPipeline=QualitySettings.renderPipeline;oldVsync=QualitySettings.vSyncCount;oldTarget=Application.targetFrameRate;oldTexture=QualitySettings.globalTextureMipmapLimit;oldLod=QualitySettings.lodBias;oldVolume=AudioListener.volume;
            var native=Screen.currentResolution;var list=Screen.resolutions.Select(r=>new Vector2Int(r.width,r.height)).Where(r=>r.x>=960&&r.y>=540).Distinct().ToList();
            if(list.Count==0)list.Add(new Vector2Int(Mathf.Max(1280,native.width),Mathf.Max(720,native.height)));
            foreach(var r in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)})if(r.x<=Mathf.Max(native.width,1920)&&r.y<=Mathf.Max(native.height,1080)&&!list.Contains(r))list.Add(r);
            Resolutions=list.OrderBy(r=>r.x*r.y).ToArray();
            var value=new Values{fullscreen=PlayerPrefs.GetInt("Training.Fullscreen",1)!=0,width=PlayerPrefs.GetInt("Training.DisplayWidth",native.width>0?native.width:1920),height=PlayerPrefs.GetInt("Training.DisplayHeight",native.height>0?native.height:1080),quality=Mathf.Clamp(PlayerPrefs.GetInt("Training.Quality",1),0,2),volume=Mathf.Clamp01(PlayerPrefs.GetFloat("Training.Volume",.75f)),sensitivity=Mathf.Clamp(PlayerPrefs.GetFloat("Training.MouseSensitivity",.085f),.02f,.3f)};
            if(!Resolutions.Contains(new Vector2Int(value.width,value.height))){value.width=Resolutions.Last().x;value.height=Resolutions.Last().y;}
            Current=Draft=value;Apply(value);Status="Changes are saved only after Apply and Keep.";
        }
        void Update()
        {
            // Windows may apply the mode before accepting its requested client size.
            // Retry only this pending transition; later user window resizing remains free.
            if(pendingWindowSize&&Time.realtimeSinceStartup>=nextWindowRetry){
                if(Screen.fullScreenMode==FullScreenMode.Windowed&&Screen.width==windowWidth&&Screen.height==windowHeight)pendingWindowSize=false;
                else if(windowRetries++<5){Screen.SetResolution(windowWidth,windowHeight,FullScreenMode.Windowed);nextWindowRetry=Time.realtimeSinceStartup+.35f;}
                else pendingWindowSize=false;
            }
            if(AwaitingConfirmation){ConfirmSeconds-=Time.unscaledDeltaTime;if(ConfirmSeconds<=0){Cancel();Status="Display change reverted after 15 seconds.";}}
            var k=Keyboard.current;if(k!=null&&k.enterKey.wasPressedThisFrame&&(k.leftAltKey.isPressed||k.rightAltKey.isPressed)){
                if(AwaitingConfirmation)Cancel();var v=Current;v.fullscreen=!v.fullscreen;Current=Draft=before=v;Apply(v);SaveCurrent();
            }
        }
        public void BeginEdit(){if(AwaitingConfirmation)Cancel();before=Current;Draft=Current;Status="Changes are saved only after Apply and Keep.";}
        public void CycleResolution(){if(AwaitingConfirmation)return;int i=Array.IndexOf(Resolutions,new Vector2Int(Draft.width,Draft.height));var r=Resolutions[(i+1)%Resolutions.Length];Draft.width=r.x;Draft.height=r.y;}
        public void CycleQuality(){if(!AwaitingConfirmation)Draft.quality=(Draft.quality+1)%3;}
        public void ToggleFullscreen(){if(!AwaitingConfirmation)Draft.fullscreen=!Draft.fullscreen;}
        public void SetVolume(float value){if(!AwaitingConfirmation){Draft.volume=value;AudioListener.volume=value;}}
        public void SetSensitivity(float value){if(!AwaitingConfirmation)Draft.sensitivity=value;}
        public void Preview(){if(AwaitingConfirmation)return;before=Current;Apply(Draft);AwaitingConfirmation=true;ConfirmSeconds=15;Status="Keep these settings? Automatic revert in 15 seconds.";}
        public void Keep(){if(!AwaitingConfirmation)return;Current=before=Draft;AwaitingConfirmation=false;SaveCurrent();Status="Settings saved.";}
        public void Cancel(){Apply(before);Current=Draft=before;AwaitingConfirmation=false;Status="Changes cancelled.";}
        public void SaveCurrent()
        {
            var v=Current;PlayerPrefs.SetInt("Training.Fullscreen",v.fullscreen?1:0);PlayerPrefs.SetInt("Training.DisplayWidth",v.width);PlayerPrefs.SetInt("Training.DisplayHeight",v.height);PlayerPrefs.SetInt("Training.Quality",v.quality);PlayerPrefs.SetFloat("Training.Volume",v.volume);PlayerPrefs.SetFloat("Training.MouseSensitivity",v.sensitivity);PlayerPrefs.Save();
        }
        void Apply(Values v)
        {
            if(effectsQuality)effectsQuality.Apply(v.quality);
            if(pipelines!=null&&pipelines.Length==3&&pipelines[v.quality])QualitySettings.renderPipeline=pipelines[v.quality];
            QualitySettings.vSyncCount=1;Application.targetFrameRate=60;QualitySettings.globalTextureMipmapLimit=v.quality==0?1:0;QualitySettings.lodBias=v.quality==0?.65f:v.quality==1?1:1.5f;
            AudioListener.volume=v.volume;if(player)player.mouseSensitivity=v.sensitivity;
            pendingWindowSize=false;
            if(!Application.isEditor){
                Screen.SetResolution(v.width,v.height,v.fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
                if(!v.fullscreen){pendingWindowSize=true;windowWidth=v.width;windowHeight=v.height;windowRetries=0;nextWindowRetry=Time.realtimeSinceStartup+.35f;}
            }
        }
        void OnDestroy(){if(Application.isEditor){QualitySettings.renderPipeline=oldPipeline;QualitySettings.vSyncCount=oldVsync;Application.targetFrameRate=oldTarget;QualitySettings.globalTextureMipmapLimit=oldTexture;QualitySettings.lodBias=oldLod;AudioListener.volume=oldVolume;}}
    }
}
