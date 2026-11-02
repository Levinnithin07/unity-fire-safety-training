using UnityEngine;
using UnityEngine.InputSystem;

namespace FireSafety
{
    public enum TrainingMode { FireExtinguisher, Evacuation }
    [DefaultExecutionOrder(-300)]
    public sealed class TrainingSession : MonoBehaviour
    {
        public static TrainingSession Instance { get; private set; }
        public static bool InputAllowed => !Instance || !Instance.enabled || Instance.Playing;
        public FireManager simulation;
        public DesktopPlayerController player;
        public EvacuationManager evacuation;
        public TrainingEvaluationManager evaluation;
        public TrainingInterface ui;
        public TrainingSettings settings;
        public Canvas gameplayCanvas;
        public bool Playing { get; private set; }
        public bool HasSession { get; private set; }
        public TrainingMode SelectedMode { get; private set; }=TrainingMode.FireExtinguisher;
        public bool IsExtinguisherMode => SelectedMode==TrainingMode.FireExtinguisher;
        public string Mode => IsExtinguisherMode?"Fire Extinguisher Training":"Emergency Evacuation Training";
        public string Page { get; private set; }="Main";
        public string ReturnPage { get; private set; }="Main";
        public event System.Action AttemptReset;
        public event System.Action SimulationStarted;
        float arrivalDelay;
        void Awake(){Instance=this;}
        void Start(){MainMenu();}
        void Update()
        {
            var k=Keyboard.current;
            if(k!=null&&k.escapeKey.wasPressedThisFrame){
                if(Playing)Pause();
                else if(Page=="Pause")Resume();
                else if(Page=="Settings"){settings.Cancel();Show(ReturnPage);}
                else if(Page=="Instructions"||Page=="Controls"||Page=="Credits")Show(ReturnPage);
                else if(Page=="Briefing")Show("Main");
            }
            if(Playing&&k!=null&&k.rKey.wasPressedThisFrame){StartSimulation();return;}
            bool finished=IsExtinguisherMode?(evacuation.extinguisher.SuppressionCompleted||evacuation.extinguisher.Remaining<=0):evacuation.Completed;
            if(Playing&&finished){arrivalDelay+=Time.deltaTime;if(arrivalDelay>=1.8f)ShowResults();}
            if(Playing&&Application.isFocused&&!player.Captured&&k!=null&&k.anyKey.wasPressedThisFrame)player.SetCapture(true);
        }
        void OnApplicationFocus(bool focus){if(!focus&&Playing)Pause();}
        void Freeze(bool frozen)
        {
            // Background evacuees may finish walking while the player's completed report is shown.
            Playing=!frozen;Time.timeScale=frozen&&Page!="Results"?0:1;AudioListener.pause=frozen;
            gameplayCanvas.enabled=!frozen;player.SetCapture(!frozen);
        }
        public void Show(string page){Page=page;Freeze(true);ui.ShowPage(page);}
        void ResetAttempt()
        {
            simulation.ResetScenario();evacuation.extinguisher.ResetEquipment();evacuation.ResetEvacuation();
            evaluation.ResetEvaluation();arrivalDelay=0;player.ResetPose(evacuation.spawn);
            AttemptReset?.Invoke();
        }
        void ConfigureMode()
        {
            simulation.ConfigureTraining(IsExtinguisherMode);
            evacuation.extinguisher.SetTrainingEnabled(IsExtinguisherMode);
            evacuation.enabled=!IsExtinguisherMode;
        }
        public void MainMenu(){ResetAttempt();HasSession=false;Show("Main");}
        public void ChooseMode(TrainingMode mode){ResetAttempt();SelectedMode=mode;ConfigureMode();HasSession=false;Show("Briefing");}
        public void Briefing()=>ChooseMode(TrainingMode.FireExtinguisher);
        public void SelectMode()=>ChooseMode(IsExtinguisherMode?TrainingMode.Evacuation:TrainingMode.FireExtinguisher);
        public void StartSimulation()
        {
            ResetAttempt();ConfigureMode();HasSession=true;Page="Gameplay";ui.Hide();Freeze(false);
            simulation.StartScenario();
            if(!IsExtinguisherMode)evacuation.Begin("Fire emergency — follow the safe exit route");
            SimulationStarted?.Invoke();
        }
        public void Pause(){if(Playing)Show("Pause");}
        public void Resume(){if(!HasSession)return;Page="Gameplay";ui.Hide();Freeze(false);}
        public void Information(string page){ReturnPage=HasSession?"Pause":"Main";Show(page);}
        public void OpenSettings(){ReturnPage=HasSession?"Pause":"Main";settings.BeginEdit();Show("Settings");}
        public void Back(){Show(ReturnPage);}
        public void Retry(){StartSimulation();}
        public void ShowResults(){evaluation.Finish(Mode);Show("Results");}
        public void Quit()
        {
            settings.SaveCurrent();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        void OnDestroy(){if(Instance==this){Instance=null;Time.timeScale=1;AudioListener.pause=false;}}
    }
}
