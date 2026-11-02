using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class TestPhaseSixShortcuts
{
    public static async Task<string> Run()
    {
        var s=UnityEngine.Object.FindAnyObjectByType<TrainingSession>();var original=s.settings.Current;var k0=Keyboard.current;Keyboard k=null;var input=InputSystem.settings.editorInputBehaviorInPlayMode;var focus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;var checks=new List<object>();
        void Check(string name,bool pass)=>checks.Add(new{name,pass});
        try{
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;k=InputSystem.AddDevice<Keyboard>();
            s.MainMenu();await Task.Delay(150);InputSystem.QueueStateEvent(k,new KeyboardState(Key.LeftAlt,Key.Enter));await Task.Delay(200);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(150);
            Check("Alt Enter toggles preference in menu",s.settings.Current.fullscreen!=original.fullscreen);Check("Alt Enter does not submit selected menu button",s.Page=="Main");
            s.StartSimulation();await Task.Delay(100);InputSystem.QueueStateEvent(k,new KeyboardState(Key.LeftAlt,Key.Enter));await Task.Delay(200);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(150);Check("Alt Enter does not start emergency",!s.simulation.Running);Check("Second toggle restores preference",s.settings.Current.fullscreen==original.fullscreen);
            s.settings.BeginEdit();s.settings.Draft=original;s.settings.Preview();s.settings.Keep();s.MainMenu();
        }finally{if(k!=null)InputSystem.RemoveDevice(k);if(k0!=null&&k0.added)k0.MakeCurrent();InputSystem.settings.editorInputBehaviorInPlayMode=input;InputSystem.settings.backgroundBehavior=focus;Application.runInBackground=bg;}
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Actual keyboard binding; display preference verified in Editor, native fullscreen requires standalone verification",checks},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase6ShortcutTests.json"),json);return json;
    }
}
