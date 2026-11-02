using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;

public static class PhaseFiveMovementRegression
{
    static DesktopPlayerController p; static CharacterController cc;
    static readonly List<object> results=new List<object>();
    static void Record(string test,bool pass,string evidence){results.Add(new{test,pass,evidence});}
    static void Warp(Vector3 position,float yaw=0){cc.enabled=false;p.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));cc.enabled=true;Physics.SyncTransforms();}
    static void Steps(Vector2 input,int frames,bool sprint=false,bool crouch=false){for(int i=0;i<frames;i++)p.StepMotor(input,sprint,crouch,1f/60);}
    static float Distance(bool sprint,bool crouch){Warp(new Vector3(0,.05f,-1.8f));Steps(Vector2.zero,15);var a=p.transform.position;Steps(Vector2.right,18,sprint,crouch);return Vector3.Distance(a,p.transform.position);}
    static string Position()=>p.transform.position.ToString("F3");
    static void Travel(Vector3 target)
    {
        for(int i=0;i<600;i++) {
            var delta=target-p.transform.position;delta.y=0;
            if(delta.magnitude<.06f)return;
            var local=p.transform.InverseTransformDirection(delta.normalized);
            p.StepMotor(new Vector2(local.x,local.z),false,false,1f/60);
        }
    }
    public static async Task<string> Run()
    {
        if(!Application.isPlaying)throw new Exception("Tests require Play Mode");
        results.Clear();p=UnityEngine.Object.FindAnyObjectByType<DesktopPlayerController>();cc=p.GetComponent<CharacterController>();
        var spawn=new Vector3(0,.05f,-1.95f);p.enabled=false;
        Keyboard keyboard=null;Mouse mouse=null;var oldKeyboard=Keyboard.current;var oldMouse=Mouse.current;
        var oldInputBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
        var oldBackgroundBehavior=InputSystem.settings.backgroundBehavior;
        bool previousBackground=Application.runInBackground;
        Application.runInBackground=true;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        try {
            Warp(spawn);Steps(Vector2.zero,300);
            Record("Gravity and floor stability",p.Grounded&&Mathf.Abs(p.transform.position.y)<.1f,Position()+" grounded="+p.Grounded);
            float walk=Distance(false,false),sprint=Distance(true,false),crouch=Distance(false,true);
            Record("Walk / sprint / crouch speeds",sprint>walk*1.45f&&crouch<walk*.7f,$"0.3s distances: walk {walk:F3}m; sprint {sprint:F3}m; crouch {crouch:F3}m");
            Warp(new Vector3(0,.05f,-3.15f));Steps(Vector2.down,120);
            Record("Front wall blocks movement",p.transform.position.z> -4.1f,Position());
            Warp(new Vector3(.6f,.05f,-1.6f));Steps(Vector2.up,120);
            Record("Desk collision obstructs and deflects movement",p.transform.position.z<.1f,Position()+"; unobstructed target z=2.8m");
            Warp(new Vector3(2f,.05f,-2.3f));Steps(Vector2.down,90);
            Record("Teacher chair collision obstructs movement",p.transform.position.z> -2.9f,Position()+"; unobstructed target z=-5.6m");
            Warp(new Vector3(-2.55f,.05f,-1.7f));Steps(Vector2.left,120);
            Record("Window wall blocks movement",p.transform.position.x> -3.1f,Position());
            Warp(new Vector3(4.8f,.05f,0));Steps(Vector2.right,180);
            Record("Passage outer glazing blocks movement",p.transform.position.x<6.5f,Position());
            foreach(float z in new[]{-2.65f,3.02f}) {
                Warp(spawn);Travel(new Vector3(2.55f,0,-1.95f));
                Travel(new Vector3(2.55f,0,z));Travel(new Vector3(4.6f,0,z));
                bool arrived=p.transform.position.x>4.4f&&Mathf.Abs(p.transform.position.z-z)<.2f;
                Record(z<0?"Front doorway classroom to passage":"Rear doorway classroom to passage",arrived,Position());
                Travel(new Vector3(2.55f,0,z));Record(z<0?"Front doorway return":"Rear doorway return",p.transform.position.x<2.75f,Position());
            }
            Warp(new Vector3(0,.05f,-1.8f));Steps(Vector2.zero,15,false,true);
            var obstruction=new GameObject("TEMP crouch clearance test");obstruction.transform.position=p.transform.position+Vector3.up*1.35f;
            obstruction.AddComponent<BoxCollider>().size=new Vector3(1,.2f,1);Physics.SyncTransforms();
            Steps(Vector2.zero,20,false,false);
            Record("Crouch cannot stand into overhead obstruction",p.IsCrouching&&cc.height<1.1f,"capsule height="+cc.height);
            UnityEngine.Object.DestroyImmediate(obstruction);Physics.SyncTransforms();Steps(Vector2.zero,20);
            Record("Standing resumes after clearance",!p.IsCrouching&&cc.height>1.7f,"capsule height="+cc.height);
            // Exercise the real Input System action bindings, using temporary virtual devices.
            keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();
            int startFrame=Time.frameCount;
            foreach(var key in new[]{Key.W,Key.A,Key.S,Key.D}) {
                p.enabled=false;Warp(spawn);p.enabled=true;p.SetCapture(true);
                var before=p.transform.position;InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));
                await Task.Delay(350);
                var delta=p.transform.position-before;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(80);
                bool good=key==Key.W?delta.z>.15f:key==Key.S?delta.z<-.15f:key==Key.A?delta.x<-.15f:delta.x>.15f;
                Record("Input binding "+key,good,delta.ToString("F3"));
            }
            p.enabled=false;Warp(spawn);p.enabled=true;p.SetCapture(true);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftCtrl));await Task.Delay(160);
            Record("Input binding Ctrl crouch",p.IsCrouching,"capsule height="+cc.height);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.LeftShift));await Task.Delay(160);
            Record("Input binding Shift sprint",p.LastSprintInput&&p.LastMoveInput.y>.5f,"sprint="+p.LastSprintInput);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(80);
            var yaw=p.transform.eulerAngles.y;var pitch=p.Pitch;
            InputSystem.QueueDeltaStateEvent(mouse.delta,new Vector2(120,40));await Task.Delay(150);
            Record("Mouse look binding",Mathf.Abs(Mathf.DeltaAngle(yaw,p.transform.eulerAngles.y))>1&&Mathf.Abs(p.Pitch-pitch)>.5f,$"yaw={p.transform.eulerAngles.y:F2}, pitch={p.Pitch:F2}");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));await Task.Delay(100);
            Record("Escape releases cursor",!p.Captured&&Cursor.lockState==CursorLockMode.None,"captured="+p.Captured);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));await Task.Delay(100);
            Record("Click recaptures cursor",p.Captured,"captured="+p.Captured);
            var materials=UnityEngine.Object.FindObjectsByType<Renderer>().SelectMany(r=>r.sharedMaterials).ToArray();
            Record("All renderers have supported materials",materials.All(m=>m&&m.shader&&m.shader.isSupported),"material slots="+materials.Length);
            Record("Input tests used advancing Play Mode frames",Time.frameCount>startFrame+20,"frames="+(Time.frameCount-startFrame));
        } finally {
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);
            oldKeyboard?.MakeCurrent();oldMouse?.MakeCurrent();p.enabled=false;Warp(spawn);Steps(Vector2.zero,30);p.enabled=true;p.SetCapture(false);
            InputSystem.settings.editorInputBehaviorInPlayMode=oldInputBehavior;
            InputSystem.settings.backgroundBehavior=oldBackgroundBehavior;
            Application.runInBackground=previousBackground;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Unity Editor Play Mode",generatedUtc=DateTime.UtcNow,results},Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.GetFullPath("../Documentation/Phase5MovementRegression.json"),json);return json;
    }
}



