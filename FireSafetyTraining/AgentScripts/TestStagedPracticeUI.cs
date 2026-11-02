using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FireSafety;
public static class TestStagedPracticeUI
{
    public static async Task<string> Run()
    {
        bool bg=Application.runInBackground;Application.runInBackground=true;
        var rows=new List<object>();
        try {
            await Task.Delay(600);var s=TrainingSession.Instance;
            foreach(var page in new[]{"Briefing","Instructions"}){
                s.Show(page);await Task.Delay(150);Canvas.ForceUpdateCanvases();
                foreach(var t in s.ui.GetComponentsInChildren<TMPro.TMP_Text>()){
                    t.ForceMeshUpdate();if(t.name=="PASS briefing"||t.name=="Evacuation instructions")rows.Add(new{page,t.name,pass=!t.isTextOverflowing,t.isTextOverflowing,screen=Screen.width+"x"+Screen.height});
                }
            }
            s.Show("Briefing");await Task.Delay(300);ScreenCapture.CaptureScreenshot("Assets/Screenshots/StagedPracticeBriefing720.png");await Task.Delay(200);s.MainMenu();
        } finally {Application.runInBackground=bg;}
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(rows,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/StagedPracticeUI.json"),json);return json;
    }
}
