using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace FireSafety
{
    public sealed class DesktopDisplaySettings : MonoBehaviour
    {
        public RenderPipelineAsset mediumPipeline;
        RenderPipelineAsset previousPipeline;
        int previousVsync, previousTarget;
        void Awake()
        {
            previousPipeline=QualitySettings.renderPipeline;
            previousVsync=QualitySettings.vSyncCount; previousTarget=Application.targetFrameRate;
            if(mediumPipeline) QualitySettings.renderPipeline=mediumPipeline;
            QualitySettings.vSyncCount=1; Application.targetFrameRate=60;
            if(!Application.isEditor) ApplySavedDisplay();
        }
        void Update()
        {
            var k=Keyboard.current;
            if(k!=null && k.enterKey.wasPressedThisFrame && (k.leftAltKey.isPressed || k.rightAltKey.isPressed) && !Application.isEditor)
                SetFullscreen(!Screen.fullScreen);
        }
        public void ApplySavedDisplay()
        {
            bool full=PlayerPrefs.GetInt("Training.Fullscreen",1)!=0;
            var native=Screen.currentResolution;
            int width=full?native.width:PlayerPrefs.GetInt("Training.WindowWidth",1280);
            int height=full?native.height:PlayerPrefs.GetInt("Training.WindowHeight",720);
            Screen.SetResolution(width,height,full?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
        }
        public void SetFullscreen(bool full)
        {
            if(!Screen.fullScreen){PlayerPrefs.SetInt("Training.WindowWidth",Screen.width);PlayerPrefs.SetInt("Training.WindowHeight",Screen.height);}
            PlayerPrefs.SetInt("Training.Fullscreen",full?1:0); PlayerPrefs.Save(); ApplySavedDisplay();
        }
        void OnDestroy()
        {
            if(Application.isEditor){QualitySettings.renderPipeline=previousPipeline;QualitySettings.vSyncCount=previousVsync;Application.targetFrameRate=previousTarget;}
        }
    }
}
