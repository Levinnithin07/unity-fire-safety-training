using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;

public static class PhaseFiveExtinguisherRegression
{
    static FireExtinguisherController e; static FireManager m; static DesktopPlayerController p;
    static Keyboard keyboard; static Mouse mouse;
    static readonly List<object> checks=new List<object>();
    static void Check(string name,bool pass,object evidence=null)=>checks.Add(new{name,pass,evidence});
    static void Warp(Vector3 pos){var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.SetPositionAndRotation(pos,Quaternion.identity);cc.enabled=true;Physics.SyncTransforms();}
    static Ray Look(Vector3 point){p.view.LookAt(point);Physics.SyncTransforms();return new Ray(p.view.position,p.view.forward);}
    static Ray Base(float yaw=0){var rotation=Quaternion.LookRotation(m.sources[0].transform.position+Vector3.up*.075f-p.view.position);p.view.rotation=Quaternion.Euler(0,yaw,0)*rotation;Physics.SyncTransforms();return new Ray(p.view.position,p.view.forward);}
    static void Step(bool spray,float yaw=0){var ray=Base(yaw);e.StepOperation(ray,spray,p.view.eulerAngles.y,.05f);m.StepSimulation(.05f);}
    static async Task KeyPress(Key key){InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));await Task.Delay(140);InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(140);}
    static async Task Button(bool down){InputSystem.QueueStateEvent(mouse,down?new MouseState().WithButton(MouseButton.Left):new MouseState());await Task.Delay(180);}
    static async Task Equip(bool start=true)
    {
        e.enabled=true;m.enabled=false;m.ResetScenario();e.ResetEquipment();p.enabled=false;p.SetCapture(true);
        Warp(new Vector3(4.7f,.025f,-1.25f));e.TryPickup(Look(e.pickupRenderer.bounds.center));e.PullPin();await Task.Delay(600);
        e.enabled=false;Warp(new Vector3(0,.025f,-1.8f));if(start)m.StartScenario();Base();
    }
    public static async Task<string> Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        checks.Clear();e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();m=e.simulation;p=e.player;
        var oldKeyboard=Keyboard.current;var oldMouse=Mouse.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;bool background=Application.runInBackground;
        Application.runInBackground=true;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        GameObject obstruction=null;
        try {
            keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();
            m.enabled=false;m.ResetScenario();e.enabled=true;e.ResetEquipment();p.enabled=false;p.SetCapture(true);
            Check("Controller and feedback references assigned",e.heldRoot&&e.pinObject&&e.nozzle&&e.trigger&&e.discharge&&e.spraySound&&e.pinSound&&e.pickupRenderer);
            Warp(new Vector3(2.9f,.025f,-1.25f));Check("Wall blocks pickup",!e.CheckPickup(Look(e.pickupRenderer.bounds.center)));
            Warp(new Vector3(6.1f,.025f,-1.25f));Check("Pickup range enforced",!e.CheckPickup(Look(e.pickupRenderer.bounds.center)));
            Warp(new Vector3(4.7f,.025f,-1.25f));Look(e.pickupRenderer.bounds.center);await Task.Delay(200);Check("Context prompt detects reachable equipment",e.CanPickUp,e.Feedback);
            await KeyPress(Key.E);Check("E binding picks up extinguisher",e.Held&&e.heldRoot.activeSelf&&!e.pickupRenderer.enabled);
            Check("Held mesh retains textured licensed materials",e.heldRoot.GetComponentsInChildren<Renderer>().Any(r=>r.sharedMaterials.Any(mat=>mat&&mat.GetTexture("_BaseMap"))));
            Check("Held model cannot collide with player",e.heldRoot.GetComponentsInChildren<Collider>().Length==0&&e.pickupObject.GetComponentsInChildren<Collider>().All(c=>!c.enabled));
            await Button(true);Check("Safety pin prevents discharge",!e.Spraying&&e.Remaining==e.capacitySeconds,e.Remaining);await Button(false);
            await KeyPress(Key.P);await Task.Delay(500);Check("P binding animates and removes pin",e.PinPulled&&!e.PullingPin&&!e.pinObject.activeSelf);
            await Button(true);await Task.Delay(200);Check("Left mouse sprays and consumes capacity",e.Spraying&&e.Remaining<e.capacitySeconds,new{e.Remaining,particles=e.discharge.particleCount});
            Check("Spray particles and audio active",e.discharge.particleCount>0&&e.spraySound.isPlaying);
            Check("Trigger lever visibly squeezes",Quaternion.Angle(e.trigger.localRotation,Quaternion.identity)>5,e.trigger.localEulerAngles.ToString());
            var samples=new float[1024];double rms=0;for(int i=0;i<12;i++){e.spraySound.GetOutputData(samples,0);rms=Math.Max(rms,Math.Sqrt(samples.Average(v=>(double)v*v)));await Task.Delay(50);}Check("Spray produces nonzero audio output",rms>.00001,rms);
            p.SetCapture(false);await Task.Delay(150);Check("Released cursor stops spraying",!e.Spraying);p.SetCapture(true);await Task.Delay(160);Check("Held recapture click does not fire",!e.Spraying);await Button(false);await Button(true);Check("Fresh click after release resumes spray",e.Spraying);await Button(false);Check("Mouse release stops spray and audio",!e.Spraying&&!e.spraySound.isPlaying);

            await Equip();Vector3 hit;Check("Base of fire reachable from front aisle",e.FindBaseTarget(Base(),out hit)==m.sources[0],hit.ToString());
            var high=Look(m.sources[0].transform.position+Vector3.up*1.2f);float initial=m.sources[0].intensity;
            for(int i=0;i<40;i++)e.StepOperation(high,true,p.view.eulerAngles.y,.05f);
            Check("High aim wastes agent without suppressing fire",e.Remaining<11&&m.sources[0].intensity==initial&&!e.SqueezeCompleted,new{e.Remaining,m.sources[0].intensity});
            var ray=Base();obstruction=new GameObject("TEMP Phase4 blocked spray");obstruction.transform.position=ray.GetPoint(1);obstruction.AddComponent<BoxCollider>().size=Vector3.one*.4f;Physics.SyncTransforms();
            Check("Obstacle blocks base aiming",e.FindBaseTarget(ray,out hit)==null);UnityEngine.Object.DestroyImmediate(obstruction);obstruction=null;Physics.SyncTransforms();
            var center=m.sources[0].transform.position+Vector3.up*.075f;Check("Too close and too far aiming rejected",e.FindBaseTarget(new Ray(center+Vector3.back*.8f,Vector3.forward),out hit)==null&&e.FindBaseTarget(new Ray(center+Vector3.back*5,Vector3.forward),out hit)==null);
            await Equip();for(int i=0;i<60;i++)Step(true);Check("Static aim cannot earn sweep or extinguish growing fire",!e.SweepCompleted&&!e.SuppressionCompleted&&m.sources[0].intensity>.18f,new{m.sources[0].intensity,e.AgentSuppression});
            await Equip();for(int i=0;i<50;i++)Step(true,i%2==0?-4:4);Check("Rapid shaking does not count as controlled sweep",!e.SweepCompleted&&!e.SuppressionCompleted);
            await Equip();for(int i=0;i<10;i++)Step(false);for(int i=0;i<160&&m.sources[0].Burning;i++)Step(true,Mathf.PingPong(i*.5f,8)-4);
            Check("Complete PASS sequence extinguishes manageable fire",e.PassComplete&&!m.sources[0].Burning,new{e.PinPulled,e.AimCompleted,e.SqueezeCompleted,e.SweepCompleted,e.SuppressionCompleted,e.Remaining,e.Accuracy,e.AgentSuppression,e.EffectiveSeconds,elapsed=m.Elapsed});
            Check("Suppression came from extinguisher, not sprinklers",m.ActiveSprinklers==0&&m.sources[0].WaterApplied==0&&m.sources[0].AgentApplied>0);
            await Equip();m.sources[1].Ignite(0);float unsafeIntensity=m.sources[0].intensity;for(int i=0;i<40;i++){ray=Base(Mathf.PingPong(i*.5f,8)-4);e.StepOperation(ray,true,p.view.eulerAngles.y,.05f);}
            Check("Spreading fire directs evacuation and blocks PASS credit",e.UnsafeEncountered&&!e.PassComplete&&m.sources[0].intensity==unsafeIntensity&&e.Feedback.Contains("UNSAFE"),e.Feedback);
            await Equip(false);for(int i=0;i<270;i++)e.StepOperation(new Ray(p.view.position,Vector3.up),true,0,.05f);Check("Limited agent reaches zero and stops",e.Remaining==0&&!e.Spraying&&Mathf.Abs(e.SpraySeconds-12)<.01f,new{e.Remaining,e.SpraySeconds});
            e.enabled=true;m.enabled=true;p.SetCapture(true);await KeyPress(Key.R);
            Check("R resets pickup, capacity, PASS, and fire state",!e.Held&&e.pickupRenderer.enabled&&!e.heldRoot.activeSelf&&e.pinObject.activeSelf&&e.Remaining==12&&!e.PinPulled&&!e.SweepCompleted&&e.AgentSuppression==0&&!m.Running);
            Check("Reset restores pickup collision",e.pickupObject.GetComponentsInChildren<Collider>().Any(c=>c.enabled));
        } finally {
            if(obstruction)UnityEngine.Object.DestroyImmediate(obstruction);
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);if(oldKeyboard!=null&&oldKeyboard.added)oldKeyboard.MakeCurrent();if(oldMouse!=null&&oldMouse.added)oldMouse.MakeCurrent();
            e.enabled=true;m.enabled=true;m.ResetScenario();e.ResetEquipment();p.enabled=false;Warp(new Vector3(0,.025f,-1.95f));p.view.localRotation=Quaternion.identity;p.enabled=true;p.SetCapture(false);
            InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=background;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{generatedUtc=DateTime.UtcNow,scope="Editor Play Mode; virtual Input System devices plus deterministic shared simulation methods",checks},Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.GetFullPath("../Documentation/Phase5ExtinguisherRegression.json"),json);return json;
    }
}

