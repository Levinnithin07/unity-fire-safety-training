using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using FireSafety;
public static class TestPhaseSixExit
{
    public static string Main()=>Run("ExitGame");
    public static string Pause()=>Run("PauseExit");
    public static string Results()=>Run("ResultsExit");
    static string Run(string button)
    {
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var v=s.settings.Current;
        bool persisted=v.quality==PlayerPrefs.GetInt("Training.Quality",1)&&Mathf.Approximately(v.volume,PlayerPrefs.GetFloat("Training.Volume",.75f))&&Mathf.Approximately(s.player.mouseSensitivity,PlayerPrefs.GetFloat("Training.MouseSensitivity",.085f))&&v.fullscreen==(PlayerPrefs.GetInt("Training.Fullscreen",1)!=0)&&v.width==PlayerPrefs.GetInt("Training.DisplayWidth",v.width)&&v.height==PlayerPrefs.GetInt("Training.DisplayHeight",v.height);
        if(button=="PauseExit"){s.StartSimulation();s.Pause();}else if(button=="ResultsExit"){s.evaluation.Finish("Exit button test",false);s.Show("Results");}else s.MainMenu();
        var b=s.ui.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name==button);
        ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{button,settingsLoadedFromPrefs=persisted,exitRequested=!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode,scope="Pointer-click callback invokes the real quit implementation; Editor branch stops Play Mode"},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase6-"+button+".json"),json);return json;
    }
}
