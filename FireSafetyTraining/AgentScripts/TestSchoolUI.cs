using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FireSafety;
public static class TestSchoolUI
{
    public static async Task<string> Run()
    {
        bool bg=Application.runInBackground;Application.runInBackground=true;await Task.Delay(500);
        var s=TrainingSession.Instance;var checks=new List<object>();var overflow=new List<object>();
        void Click(string id)=>s.ui.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name==id).onClick.Invoke();
        void Check(string name,bool pass)=>checks.Add(new{name,pass});
        void Fit(string label){Canvas.ForceUpdateCanvases();foreach(var t in s.ui.GetComponentsInChildren<TMPro.TMP_Text>()){t.ForceMeshUpdate();if(t.isTextOverflowing)overflow.Add(new{label,name=t.name,text=t.text});}}
        try {
            s.MainMenu();await Task.Delay(300);Fit("Main");ScreenCapture.CaptureScreenshot("Assets/Screenshots/School-MainMenu720.png");await Task.Delay(150);
            foreach(var mode in new[]{TrainingMode.FireExtinguisher,TrainingMode.Evacuation}){
                Click(mode==TrainingMode.FireExtinguisher?"FireExtinguisherTraining":"EvacuationTraining");Check(mode+" card opens matching briefing",s.SelectedMode==mode&&s.Page=="Briefing");Fit(mode+" briefing");
                Click("BriefingBack");Check(mode+" briefing back returns to main",s.Page=="Main");
                foreach(var page in new[]{"Instructions","Controls","Credits"}){Click(page);Fit(mode+" "+page);Click(page+"Back");Check(mode+" "+page+" back",s.Page=="Main");}
                Click("Settings");Fit(mode+" Settings");Click("CancelSettings");Check(mode+" settings cancel",s.Page=="Main");
                s.StartSimulation();await Task.Delay(200);s.Pause();Fit(mode+" Pause");Click("Resume");Check(mode+" pause/resume",s.Playing);s.Pause();Click("MainMenu");
            }
            s.ChooseMode(TrainingMode.FireExtinguisher);s.StartSimulation();s.evaluation.Finish(s.Mode,false);s.Show("Results");Fit("Fire retry result");Click("ResultsMenu");Check("Results back to main",s.Page=="Main");
            s.ChooseMode(TrainingMode.Evacuation);s.StartSimulation();s.evaluation.Finish(s.Mode,false);s.Show("Results");Fit("Evacuation retry result");Click("ResultsMenu");
            Check("All tested text fits at 720p",overflow.Count==0);
        } finally {s.MainMenu();Application.runInBackground=bg;}
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Existing Canvas layout and button-event navigation at 1280x720; incomplete results generated only as layout fixtures",checks,overflow},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/SchoolModeUI.json"),json);return json;
    }
}


