using System;
using System.Linq;
using UnityEngine;
using TMPro;

namespace FireSafety
{
    public sealed class TrainingInterface : MonoBehaviour
    {
        public TrainingSession session;
        public TMP_FontAsset font;
        public Canvas canvas;
        public CanvasGroup fade;
        public GameObject[] pages;
        public TMP_Text pageTitle, modeText, displayText, resolutionText, qualityText, volumeText, sensitivityText, settingsStatus, resultScore, resultMetrics, resultFeedback;
        public UnityEngine.UI.Slider volumeSlider,sensitivitySlider;
        public AudioSource clickSound;
        public string VisiblePage { get; private set; }
        static readonly Color Ink=new Color(.035f,.06f,.085f,.97f), Panel=new Color(.065f,.10f,.13f,.97f), Muted=new Color(.66f,.75f,.8f), Accent=new Color(1,.53f,.26f);
        void Awake()
        {
            foreach(var b in GetComponentsInChildren<UnityEngine.UI.Button>(true)){string id=b.name;b.onClick.AddListener(()=>Activate(id));}
            volumeSlider.onValueChanged.AddListener(v=>{session.settings.SetVolume(v);Refresh();});
            sensitivitySlider.onValueChanged.AddListener(v=>{session.settings.SetSensitivity(v);Refresh();});
            if(!clickSound.clip){var clip=AudioClip.Create("Original menu click",2205,1,22050,false);var samples=new float[2205];for(int i=0;i<samples.Length;i++)samples[i]=Mathf.Sin(i*2*Mathf.PI*880/22050)*.10f*Mathf.Exp(-i/300f);clip.SetData(samples,0);clickSound.clip=clip;}
        }
        void Update(){if(canvas.enabled){fade.alpha=Mathf.MoveTowards(fade.alpha,1,Time.unscaledDeltaTime*8);if(VisiblePage=="Settings")Refresh();}}
        public void Activate(string id)
        {
            var keyboard=UnityEngine.InputSystem.Keyboard.current;
            if(keyboard!=null&&keyboard.enterKey.wasPressedThisFrame&&(keyboard.leftAltKey.isPressed||keyboard.rightAltKey.isPressed))return;
            if(clickSound&&clickSound.clip)clickSound.PlayOneShot(clickSound.clip);
            switch(id){
                case "StartTraining":case "FireExtinguisherTraining":session.ChooseMode(TrainingMode.FireExtinguisher);break;
                case "EvacuationTraining":session.ChooseMode(TrainingMode.Evacuation);break;
                case "Instructions":session.Information("Instructions");break;
                case "Controls":case "PauseControls":session.Information("Controls");break;
                case "Settings":case "PauseSettings":session.OpenSettings();break;
                case "Credits":session.Information("Credits");break;
                case "ExitGame":case "PauseExit":case "ResultsExit":session.Quit();break;
                case "Mode":break;
                case "StartSimulation":session.StartSimulation();break;
                case "BriefingBack":session.MainMenu();break;
                case "Resume":session.Resume();break;
                case "Restart":case "RetryTraining":session.Retry();break;
                case "MainMenu":case "ResultsMenu":session.MainMenu();break;
                case "InstructionsBack":case "ControlsBack":case "CreditsBack":session.Back();break;
                case "DisplayMode":session.settings.ToggleFullscreen();break;
                case "Resolution":session.settings.CycleResolution();break;
                case "Quality":session.settings.CycleQuality();break;
                case "Apply":session.settings.Preview();break;
                case "Keep":session.settings.Keep();break;
                case "CancelSettings":session.settings.Cancel();session.Back();break;
            }
            Refresh();
        }
        public void Hide(){canvas.enabled=false;VisiblePage="Gameplay";UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);}
        public void ShowPage(string name)
        {
            VisiblePage=name;canvas.enabled=true;fade.alpha=0;
            foreach(var p in pages)p.SetActive(p.name==name);
            pageTitle.text=name=="Main"?"FIRE SAFETY\nTRAINING":name=="Results"?"YOUR TRAINING\nREPORT":name.ToUpperInvariant();
            Refresh();
            var selected=GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b=>b.interactable);
            if(selected)UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(selected.gameObject);
        }
        static string Step(bool done)=>done?"DONE":"NOT COMPLETED";
        public void Refresh()
        {
            if(!session)return;
            modeText.text="SELECTED MODE   /   "+session.Mode;RefreshModeText();
            var s=session.settings;var d=s.Draft;
            displayText.text="DISPLAY MODE   /   "+(d.fullscreen?"Borderless fullscreen":"Windowed")+"   >";
            resolutionText.text=$"RESOLUTION   /   {d.width} × {d.height}   >";
            qualityText.text="GRAPHICS   /   "+new[]{"Low","Medium — Iris Xe","High"}[d.quality]+"   >";
            volumeText.text=$"MASTER VOLUME     {d.volume*100:0}%";sensitivityText.text=$"MOUSE SENSITIVITY     {d.sensitivity:0.000}";
            volumeSlider.SetValueWithoutNotify(d.volume);sensitivitySlider.SetValueWithoutNotify(d.sensitivity);
            volumeSlider.interactable=sensitivitySlider.interactable=!s.AwaitingConfirmation;
            settingsStatus.text=(s.AwaitingConfirmation?$"KEEP THESE SETTINGS?   Reverting in {Mathf.CeilToInt(s.ConfirmSeconds)}s":s.Status)+(Application.isEditor?"\nDisplay mode and resolution apply in the Windows executable.":"");
            foreach(var b in pages.First(p=>p.name=="Settings").GetComponentsInChildren<UnityEngine.UI.Button>(true))b.interactable=b.name=="Keep"?s.AwaitingConfirmation:b.name=="CancelSettings"||!s.AwaitingConfirmation;
            var r=session.evaluation.Result;if(r==null)return;
            resultScore.text=$"<size=100>{r.overall:0}</size><size=30> / 100</size>\n<color=#FF8842><size=26>{r.status}</size></color>\n<size=22>{r.practiceMode}</size>";
            if(r.extinguisherMode)
                resultMetrics.text=$"PASS OBJECTIVES     {r.tasks:0}% ({r.tasksCompleted}/{r.tasksRequired})\nP  Pull: {Step(r.pin)}\nA  Aim: {Step(r.aim)}\nS  Squeeze: {Step(r.squeeze)}\nS  Sweep: {Step(r.sweep)}\nFire suppressed: {Step(r.suppressed)}\n\nCorrect aiming     {r.aimAccuracy:0}%\nEffective spray     {r.effectiveSpraySeconds:0.0}s\nControlled sweep     {r.effectiveSweepSeconds:0.0}s\nAgent consumed     {r.capacityConsumedPercent:0}%\nTime to extinguish     {(r.suppressed?r.extinguishSeconds.ToString("0.0")+"s":"Not extinguished")}";
            else resultMetrics.text=$"EVACUATION OBJECTIVES     {r.tasks:0}% ({r.tasksCompleted}/{r.tasksRequired})\n\nSafe assembly reached     {(r.completed?"YES":"NO")}\nEvacuation time     {r.evacuationSeconds:0.0}s\nSafety score     {r.safety:0}/100\nNavigation score     {r.navigation:0}/100\nHazard exposure     {r.exposureSeconds:0.0}s\nRoute changes     {r.reroutes}\n\nFollow a clear route. Stay outside.\nReport to the person in charge.";
            resultFeedback.text=r.feedback+"\n\n<size=19>"+r.formula+"\nPractice feedback, not certification.\n"+session.evaluation.SaveStatus+"</size>";
        }
        TMP_Text[] contentTexts;
        void Content(string name,string value){contentTexts??=GetComponentsInChildren<TMP_Text>(true);var t=contentTexts.FirstOrDefault(x=>x.name==name);if(t)t.text=value;}
        public void RefreshModeText()
        {
            bool fire=session.IsExtinguisherMode;
            string objective=fire?"CONTROLLED FIRE PRACTICE\n\nA small training fire is ready when you start. Take your time finding the extinguisher in the passage.\n\nUse PASS to suppress it. Your attempt ends when the fire is out or the agent is exhausted.":"REACH ASSEMBLY POINT A\n\nA fire emergency begins when you start. Follow the green route guidance toward a clear exit.\n\nSmoke detectors trigger the alarm; ceiling sprinklers respond to heat. Do not wait beside the fire.";
            string technique=fire?"PASS TECHNIQUE\n\nE — Pick up the extinguisher\nP — Pull the safety pin\nAim at the base of the flames\nHold LEFT MOUSE to spray\nSweep slowly left and right\n\nStand 1.4–4m away. Aim where the flames meet the chair writing surface; the crosshair turns green. Sprinklers are disabled.":"SAFE EXIT\n\nFollow the route and directional arrow. If a doorway becomes hazardous, use the alternative guidance.\n\nE opens the exterior exit doors. Continue outside into the marked assembly point. Stay out and report in.\n\nNo extinguisher use is required.";
            Content("Briefing objectives",objective);Content("PASS briefing",technique);
            Content("Instructions body",objective);Content("Evacuation instructions",technique);
            Content("Interaction controls",fire?"EXTINGUISHER CONTROLS\n\nE                 Pick up\nP                 Pull safety pin\nLeft mouse  Hold to spray\nMouse         Sweep left/right\nR                 Retry mode\nESC             Pause / main menu":"EVACUATION CONTROLS\n\nE                 Open exit doors\nR                 Retry mode\nM                Toggle alarm audio\nESC             Pause / main menu\n\nFollow the green route to Assembly Point A.");
            Content("Pause description","TAKE A MOMENT\n\nThe simulation and timers are paused. Resume when ready.\n\nRetry clears this attempt. Return to the main menu to choose another training mode.");
        }
        // Targeted upgrade of the existing menu, called once by the Editor configuration helper.
        public void ConfigureIndependentMenu()
        {
            var main=pages.First(p=>p.name=="Main").transform;
            var first=main.Find("StartTraining")??main.Find("FireExtinguisherTraining");
            first.name="FireExtinguisherTraining";
            var card=(RectTransform)first;card.anchorMin=new Vector2(0,.40f);card.anchorMax=new Vector2(.48f,.83f);card.offsetMin=card.offsetMax=Vector2.zero;
            var label=first.GetComponentInChildren<TMP_Text>(true);label.text="MODE 1\nFIRE EXTINGUISHER TRAINING\n\n<size=22>Learn correct extinguisher operation\nusing the PASS technique.</size>";label.fontSize=28;
            if(!main.Find("EvacuationTraining"))Button("EvacuationTraining",main,"MODE 2\nEMERGENCY EVACUATION TRAINING\n\n<size=22>Practise safe building evacuation\nby following emergency exit guidance.</size>",new Vector2(.52f,.40f),new Vector2(1,.83f));
            var second=main.Find("EvacuationTraining");second.GetComponentInChildren<TMP_Text>(true).fontSize=28;second.GetComponent<UnityEngine.UI.Image>().color=new Color(.07f,.28f,.31f,1);
            string[] utilities={"Instructions","Controls","Settings","Credits","ExitGame"};
            for(int i=0;i<utilities.Length;i++){var r=(RectTransform)main.Find(utilities[i]);r.anchorMin=new Vector2(i*.205f,.10f);r.anchorMax=new Vector2(i*.205f+.18f,.24f);r.offsetMin=r.offsetMax=Vector2.zero;}
            var intro=main.Find("Introduction").GetComponent<TMP_Text>();intro.text="CHOOSE YOUR TRAINING";intro.fontSize=30;intro.rectTransform.anchorMin=new Vector2(0,.87f);intro.rectTransform.anchorMax=new Vector2(1,1);
            var note=main.Find("Session length").GetComponent<TMP_Text>();note.text="Two independent modes. Each has its own objectives and results.";note.fontSize=23;note.rectTransform.anchorMin=new Vector2(0,.27f);note.rectTransform.anchorMax=new Vector2(1,.36f);
            modeText.transform.parent.GetComponent<UnityEngine.UI.Button>().interactable=false;
            var retry=pages.First(p=>p.name=="Results").transform.Find("RetryTraining").GetComponentInChildren<TMP_Text>(true);retry.text="RETRY MODE";
            contentTexts=null;RefreshModeText();modeText.text="SELECTED MODE   /   "+session.Mode;
        }
        RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        UnityEngine.UI.Image Image(string name,Transform parent,Vector2 min,Vector2 max,Color color,bool ray=false)
        {
            var r=Rect(name,parent,min,max);var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=ray;return i;
        }
        TMP_Text Text(string name,Transform parent,Vector2 min,Vector2 max,string content,float size,Color? color=null)
        {
            var t=Rect(name,parent,min,max).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=content;t.fontSize=size;t.color=color??Color.white;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.alignment=TextAlignmentOptions.TopLeft;return t;
        }
        TMP_Text Button(string id,Transform parent,string label,Vector2 min,Vector2 max,bool primary=false)
        {
            var img=Image(id,parent,min,max,primary?Accent:Panel,true);var b=img.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=img;
            var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.18f,1.18f,1.18f);c.selectedColor=new Color(1.13f,1.13f,1.13f);c.pressedColor=new Color(.7f,.8f,.8f);c.disabledColor=new Color(.5f,.5f,.5f,.6f);c.fadeDuration=.12f;b.colors=c;
            var t=Text("Label",img.transform,new Vector2(.045f,.16f),new Vector2(.96f,.84f),label,25,primary?Ink:Color.white);t.alignment=TextAlignmentOptions.MidlineLeft;return t;
        }
        GameObject Page(string name){return Rect(name,transform,new Vector2(.055f,.12f),new Vector2(.945f,.72f)).gameObject;}
        UnityEngine.UI.Slider Slider(string name,Transform parent,float y,float min,float max)
        {
            var r=Rect(name,parent,new Vector2(.03f,y),new Vector2(.46f,y+.035f));var s=r.gameObject.AddComponent<UnityEngine.UI.Slider>();s.minValue=min;s.maxValue=max;
            Image("Track",r,Vector2.zero,Vector2.one,new Color(.2f,.27f,.31f),true);
            var fill=Image("Fill",r,Vector2.zero,Vector2.one,Accent);s.fillRect=fill.rectTransform;
            var area=Rect("Handle area",r,Vector2.zero,Vector2.one);var handle=Image("Handle",area,Vector2.zero,new Vector2(0,1),Color.white,true);handle.rectTransform.sizeDelta=new Vector2(18,12);s.handleRect=handle.rectTransform;s.targetGraphic=handle;s.direction=UnityEngine.UI.Slider.Direction.LeftToRight;return s;
        }
        public void Build()
        {
            if(transform.childCount>0)throw new InvalidOperationException("Menu already exists; edit it in place.");
            canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();fade=gameObject.AddComponent<CanvasGroup>();
            Image("Backdrop",transform,Vector2.zero,Vector2.one,new Color(.018f,.035f,.05f,.87f),true);
            Image("Accent rail",transform,new Vector2(.055f,.8f),new Vector2(.06f,.94f),Accent);
            Text("Brand",transform,new Vector2(.075f,.94f),new Vector2(.7f,.975f),"VIRTUAL REALITY FIRE SAFETY TRAINING SYSTEM",19,Muted);
            pageTitle=Text("Page title",transform,new Vector2(.075f,.755f),new Vector2(.85f,.93f),"FIRE SAFETY\nTRAINING",60);
            Text("Edition",transform,new Vector2(.76f,.85f),new Vector2(.94f,.94f),"CLASSROOM  /  01\nDESKTOP SIMULATION",20,Muted);
            Text("Footer",transform,new Vector2(.055f,.035f),new Vector2(.945f,.08f),"KEYBOARD + MOUSE    •    NO HEADSET REQUIRED\nEducational simulation. Supplements, but does not replace, certified fire safety training.",19,Muted);
            pages=new[]{Page("Main"),Page("Briefing"),Page("Instructions"),Page("Controls"),Page("Settings"),Page("Credits"),Page("Pause"),Page("Results")};
            var p=pages[0].transform;
            Text("Introduction",p,new Vector2(0,.38f),new Vector2(.50f,.98f),"RECOGNIZE. RESPOND. EVACUATE.\n\nExplore a detailed classroom, practise PASS on a manageable fire, and find a clear route to safety.\n\nYour actions shape the training report.",32);
            Text("Session length",p,new Vector2(0,.18f),new Vector2(.46f,.32f),"ONE ENVIRONMENT\nTwo practice focuses. One safe assembly point.",23,Muted);
            string[] ids={"StartTraining","Instructions","Controls","Settings","Credits","ExitGame"};string[] labels={"START TRAINING  >","Instructions","Controls","Settings","Credits","Exit game"};
            for(int i=0;i<ids.Length;i++)Button(ids[i],p,labels[i],new Vector2(.59f,.84f-i*.15f),new Vector2(1,.97f-i*.15f),i==0);
            p=pages[1].transform;
            Text("Briefing objectives",p,new Vector2(0,.28f),new Vector2(.49f,.98f),"01  EXPLORE\nIdentify the equipment and both classroom exits.\n\n02  RESPOND\nOnly trained people with a clear escape route should attempt a small, contained fire.\n\n03  LEAVE\nEvacuate promptly when smoke, spread or blocked escape makes suppression unsafe.",27);
            Text("PASS briefing",p,new Vector2(.55f,.3f),new Vector2(1,.98f),"PASS\nP — Pull the safety pin\nA — Aim low at the base\nS — Squeeze the handle\nS — Sweep from side to side\n\nE picks up equipment; P pulls the pin. ENTER starts 10 seconds of PASS practice, then evacuation. V leaves early. Reach Assembly Point A.",27);
            modeText=Button("Mode",p,"",new Vector2(0,.15f),new Vector2(1,.25f));Button("BriefingBack",p,"Back",new Vector2(0,0),new Vector2(.25f,.12f));Button("StartSimulation",p,"START SIMULATION  >",new Vector2(.55f,0),new Vector2(1,.12f),true);
            p=pages[2].transform;
            Text("Instructions body",p,new Vector2(0,.22f),new Vector2(.48f,.98f),"ASSESS FIRST\nAlert others. Keep a clear escape route behind you. This demonstration uses a small desk fire. Do not attempt a spreading fire or enter heavy smoke.\n\nPASS PRACTICE\nFind the extinguisher in the passage. E picks it up; P removes its pin. Aim at the base of the flames from 1.4–4m. Hold the left mouse button and sweep slowly across the base.",28);
            Text("Evacuation instructions",p,new Vector2(.55f,.22f),new Vector2(1,.98f),"EVACUATION\nPASS mode: practise for 10 seconds, then follow the exit route. V or unsafe conditions ends practice early. Evacuation-only mode responds to the alarm. Follow the green guidance.\n\nPress E near the exterior door, then reach Assembly Point A. Stay out and report in.\n\nThe fire and smoke are simplified simulations. Follow your institution's real emergency plan.",28);
            Button("InstructionsBack",p,"Back",Vector2.zero,new Vector2(.25f,.12f));
            p=pages[3].transform;
            Text("Movement controls",p,new Vector2(0,.18f),new Vector2(.48f,.98f),"MOVEMENT\n\nW A S D          Walk\nMouse             Look around\nLeft Shift        Sprint\nLeft Ctrl          Crouch\n\nESC                 Pause / resume\nAlt + Enter      Toggle fullscreen",30);
            Text("Interaction controls",p,new Vector2(.55f,.18f),new Vector2(1,.98f),"INTERACTION\n\nE                 Pick up / open exit\nP                 Pull extinguisher pin\nLeft mouse  Hold to spray\nENTER        Start emergency\nV                 Choose evacuation\nR                 Restart practice\nM                Toggle alarm audio",30);Button("ControlsBack",p,"Back",Vector2.zero,new Vector2(.25f,.12f));
            p=pages[4].transform;
            displayText=Button("DisplayMode",p,"",new Vector2(0,.84f),new Vector2(.49f,.97f));resolutionText=Button("Resolution",p,"",new Vector2(0,.67f),new Vector2(.49f,.80f));qualityText=Button("Quality",p,"",new Vector2(0,.50f),new Vector2(.49f,.63f));
            volumeText=Text("Volume label",p,new Vector2(.03f,.38f),new Vector2(.49f,.44f),"",23);volumeSlider=Slider("Volume",p,.30f,0,1);
            sensitivityText=Text("Sensitivity label",p,new Vector2(.03f,.20f),new Vector2(.49f,.26f),"",23);sensitivitySlider=Slider("Sensitivity",p,.12f,.02f,.3f);
            Text("Quality description",p,new Vector2(.56f,.48f),new Vector2(1,.97f),"TUNED FOR DESKTOP\n\nLow: reduced render scale, textures and shadows.\nMedium: balanced for Intel Iris Xe.\nHigh: sharper rendering and longer shadows.\n\nBorderless defaults to your monitor's native resolution on first launch.",26);
            settingsStatus=Text("Settings status",p,new Vector2(.56f,.22f),new Vector2(1,.46f),"",24,Accent);
            Button("Apply",p,"APPLY",new Vector2(.56f,.05f),new Vector2(.76f,.17f),true);Button("Keep",p,"KEEP",new Vector2(.79f,.05f),new Vector2(1,.17f),true);Button("CancelSettings",p,"Cancel / Back",new Vector2(0,-.04f),new Vector2(.3f,.06f));
            p=pages[5].transform;
            Text("Art credits",p,new Vector2(0,.2f),new Vector2(.48f,.98f),"ENVIRONMENT + EQUIPMENT\n\nClassroom — Christophe Seux / Blender (CC0)\nUnity mesh conversion — lkewis (Unlicense)\nKorean Fire Extinguisher 01 — UM JOORIN\nFire Alarm — Slinc\nConcrete Paving — Stephan Seeliger\nPoly Haven assets: CC0\n\nPowered by Poly Haven",27);
            Text("Other credits",p,new Vector2(.55f,.2f),new Vector2(1,.98f),"EFFECTS + TRAINING\n\nParticle textures — Kenney (CC0)\nLiberation Sans — SIL Open Font License\nAdditional meshes, audio and gameplay — project-authored.\n\nSafety references: U.S. Fire Administration and Pennsylvania OSFC.\nFull source links and alterations: Documentation/AssetAttribution.md",27);Button("CreditsBack",p,"Back",Vector2.zero,new Vector2(.25f,.12f));
            p=pages[6].transform;
            Text("Pause description",p,new Vector2(0,.3f),new Vector2(.48f,.98f),"TAKE A MOMENT\n\nThe simulation and evacuation timer are paused. Resume when ready.\n\nRestart clears this attempt. Returning to the main menu also clears current progress.",32);
            string[] pauseIds={"Resume","PauseSettings","PauseControls","Restart","MainMenu","PauseExit"};string[] pauseLabels={"RESUME  >","Settings","Controls","Restart training","Return to main menu","Exit game"};for(int i=0;i<6;i++)Button(pauseIds[i],p,pauseLabels[i],new Vector2(.59f,.84f-i*.15f),new Vector2(1,.97f-i*.15f),i==0);
            p=pages[7].transform;
            resultScore=Text("Overall score",p,new Vector2(0,.67f),new Vector2(.47f,1.04f),"",32);
            resultFeedback=Text("Personal feedback",p,new Vector2(0,.18f),new Vector2(.47f,.65f),"",25);
            resultMetrics=Text("Performance metrics",p,new Vector2(.53f,.17f),new Vector2(1,1.01f),"",25);
            Button("RetryTraining",p,"RETRY TRAINING",Vector2.zero,new Vector2(.31f,.12f),true);Button("ResultsMenu",p,"Main menu",new Vector2(.35f,0),new Vector2(.66f,.12f));Button("ResultsExit",p,"Exit game",new Vector2(.70f,0),new Vector2(1,.12f));
            clickSound=gameObject.AddComponent<AudioSource>();clickSound.playOnAwake=false;clickSound.ignoreListenerPause=true;clickSound.volume=.5f;
            foreach(var page in pages)page.SetActive(page.name=="Main");
            ConfigureIndependentMenu();
        }
    }
}




