using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FireSafety;
public static class ConfigureStagedPractice
{
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Edit Mode required");
        var ui=UnityEngine.Object.FindAnyObjectByType<TrainingInterface>();
        var texts=ui.GetComponentsInChildren<TMPro.TMP_Text>(true);
        var briefing=texts.Single(t=>t.name=="PASS briefing");
        var instructions=texts.Single(t=>t.name=="Evacuation instructions");
        Undo.RecordObjects(new UnityEngine.Object[]{briefing,instructions,ui.session.evacuation},"Set staged extinguisher practice");
        briefing.text="PASS\nP — Pull the safety pin\nA — Aim low at the base\nS — Squeeze the handle\nS — Sweep from side to side\n\nE picks up equipment; P pulls the pin. ENTER starts 10 seconds of PASS practice, then evacuation. V leaves early. Reach Assembly Point A.";
        instructions.text="EVACUATION\nPASS mode: practise for 10 seconds, then follow the exit route. V or unsafe conditions ends practice early. Evacuation-only mode responds to the alarm. Follow the green guidance.\n\nPress E near the exterior door, then reach Assembly Point A. Stay out and report in.\n\nThe fire and smoke are simplified simulations. Follow your institution's real emergency plan.";
        ui.session.evacuation.extinguisherPracticeSeconds=10;
        EditorUtility.SetDirty(briefing);EditorUtility.SetDirty(instructions);EditorUtility.SetDirty(ui.session.evacuation);
        var scene=ui.gameObject.scene;EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return scene.path+": saved two instruction labels and 10-second practice duration";
    }
}
