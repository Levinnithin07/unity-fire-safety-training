using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using FireSafety;
public static class TestPhaseFive
{
    static EvacuationManager v;static FireManager m;static DesktopPlayerController p;static ExitDoor door;static Keyboard k;
    static readonly List<object> checks=new List<object>();
    static void Check(string name,bool pass,object evidence=null)=>checks.Add(new{name,pass,evidence});
    static void Reset(){v.enabled=true;m.enabled=false;m.ResetScenario();p.enabled=false;p.ResetPose(v.spawn);v.enabled=false;Physics.SyncTransforms();}
    static void Advance(float seconds){for(int i=0;i<seconds/.05f;i++)m.StepSimulation(.05f);}
    static async Task KeyPress(Key key){InputSystem.QueueStateEvent(k,new KeyboardState(key));await Task.Delay(130);InputSystem.QueueStateEvent(k,new KeyboardState());await Task.Delay(130);}
    static async Task Open()
    {
        p.ResetPose(new Vector3(5.4f,.025f,-1.32f));p.view.LookAt(new Vector3(6.47f,1.05f,-1.32f));p.SetCapture(true);Physics.SyncTransforms();await KeyPress(Key.E);await Task.Delay(800);
    }
    static object WalkToAssembly()
    {
        int steps=0;float minZ=100,maxZ=-100;v.Replan();
        for(;steps<2000&&!v.Completed;steps++){
            if(!v.navigation.Available)break;var target=v.navigation.NextPoint(p.transform.position);var delta=target-p.transform.position;delta.y=0;
            var local=p.transform.InverseTransformDirection(delta.normalized);p.StepMotor(new Vector2(local.x,local.z),false,false,1f/60);v.Step(1f/60);
            minZ=Mathf.Min(minZ,p.transform.position.z);maxZ=Mathf.Max(maxZ,p.transform.position.z);
        }
        return new{steps,position=p.transform.position.ToString("F3"),v.Completed,v.Elapsed,minZ,maxZ};
    }
    public static async Task<string> Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");checks.Clear();v=UnityEngine.Object.FindAnyObjectByType<EvacuationManager>();m=v.simulation;p=v.player;door=v.exitDoor;
        var oldK=Keyboard.current;var oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;var oldFocus=InputSystem.settings.backgroundBehavior;bool bg=Application.runInBackground;GameObject block=null;
        Application.runInBackground=true;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        try {
            k=InputSystem.AddDevice<Keyboard>();Reset();Check("Evacuation cannot start before emergency",!v.Begin("test")&&!v.Active);
            p.ResetPose(v.assemblyZone.transform.position);v.Step(1);Check("Exploration at assembly gives no false completion",!v.Completed);
            Reset();m.StartScenario();v.enabled=true;p.SetCapture(true);await KeyPress(Key.V);Check("V binding begins voluntary evacuation",v.Active&&v.Reason.Contains("chose"),new{v.Reason,v.Elapsed});v.enabled=false;
            Check("Initial route uses front classroom door",v.navigation.Available&&v.navigation.RouteName=="FRONT ROUTE",v.navigation.Distance);
            Check("Navigation uses baked collision-based surface",UnityEngine.AI.NavMesh.CalculateTriangulation().indices.Length>0);
            p.ResetPose(new Vector3(5.6f,.025f,-1.32f));for(int i=0;i<100;i++)p.StepMotor(Vector2.right,false,false,1f/60);
            Check("Closed exit blocks the character capsule",p.transform.position.x<6.3f,p.transform.position.ToString());
            Check("Exit interaction range enforced",!door.CheckInteraction(new Ray(new Vector3(3,1,-1.32f),Vector3.right)));
            await Open();Check("E opens exit leaves through real Input System",door.OpenRequested&&door.IsPassable,new{left=door.leftLeaf.localEulerAngles.ToString(),right=door.rightLeaf.localEulerAngles.ToString()});
            p.ResetPose(v.spawn);v.Replan();int completedEvents=0;Action onComplete=()=>completedEvents++;v.Evacuated+=onComplete;
            var front=WalkToAssembly();Check("Front route physically traverses classroom, exit and courtyard",v.Completed&&v.CrossedExit,front);
            Check("Measured evacuation time recorded",v.Elapsed>3&&v.Elapsed<60,v.Elapsed);float frozen=v.Elapsed;for(int i=0;i<100;i++)v.Step(.05f);
            Check("Completion fires once and freezes timer",completedEvents==1&&v.Elapsed==frozen,new{completedEvents,v.Elapsed});v.Evacuated-=onComplete;
            await Task.Delay(100);Check("Assembly confirmation audio plays",v.arrivalSound.isPlaying);
            v.enabled=true;m.enabled=true;p.SetCapture(true);await KeyPress(Key.R);Check("R resets evacuation, doors and player",!v.Active&&!v.Completed&&!door.OpenRequested&&v.Elapsed==0&&Vector3.Distance(p.transform.position,v.spawn)<.15f,new{position=p.transform.position.ToString(),v.Elapsed});
            Reset();m.StartScenario();v.enabled=true;Advance(18);await Task.Delay(140);Check("Smoke alarm automatically starts evacuation",m.alarm.Active&&v.Active&&v.Reason.Contains("alarm"),v.Reason);v.enabled=false;
            Reset();m.StartScenario();v.Begin("Smoke rerouting test");Advance(30);v.Replan();
            Check("Smoke state closes front route but leaves rear clear",v.navigation.routes[0].Blocked&&!v.navigation.routes[1].Blocked,v.navigation.routes.Select(r=>new{r.routeName,r.Smoke,r.Reason}).ToArray());
            Check("Hazard change reroutes front to rear",v.navigation.Available&&v.navigation.RouteName=="REAR ROUTE"&&v.Reroutes==1,new{v.Reroutes,v.navigation.Distance});
            p.view.LookAt(v.navigation.routes[0].smoke.transform.position);await Task.Delay(600);Check("Hazard has visible smoke particles and warning sign",v.navigation.routes[0].smoke.particleCount>0&&v.navigation.routes[0].sign.text.Contains("OTHER"),v.navigation.routes[0].smoke.particleCount);
            await Open();p.ResetPose(v.spawn);v.Replan();var rear=WalkToAssembly();Check("Rear route physically reaches assembly while front is hazardous",v.Completed,rear);
            Reset();m.StartScenario();v.Begin("No route test");Advance(30);
            block=new GameObject("TEMP Phase5 rear obstruction");block.transform.position=new Vector3(3.3f,1,3.02f);block.AddComponent<BoxCollider>().size=new Vector3(.16f,2,1.05f);Physics.SyncTransforms();v.Replan();v.Step(.05f);
            Check("Blocked alternative never produces false safe guidance",v.navigation.routes[1].Obstructed&&!v.navigation.Available&&v.Notice.Contains("No clear route"),v.Notice);
            UnityEngine.Object.DestroyImmediate(block);block=null;Physics.SyncTransforms();v.Replan();Check("Clearing obstruction restores valid alternative",v.navigation.Available&&v.navigation.RouteName=="REAR ROUTE");
            p.ResetPose(new Vector3(2.8f,.025f,-2.65f));v.Step(.25f);Check("Entering hazardous doorway records exposure and warns",v.NearHazard&&v.HazardExposure>.2f,v.Notice);
            Reset();m.StartScenario();v.Begin("Physical rerouting test");block=new GameObject("TEMP Phase5 front obstruction");block.transform.position=new Vector3(3.3f,1,-2.65f);block.AddComponent<BoxCollider>().size=new Vector3(.16f,2,1.05f);Physics.SyncTransforms();v.Replan();
            Check("Physical front obstruction also reroutes",v.navigation.Available&&v.navigation.RouteName=="REAR ROUTE"&&v.navigation.routes[0].Obstructed);
            UnityEngine.Object.DestroyImmediate(block);block=null;Physics.SyncTransforms();
            Reset();m.StartScenario();v.enabled=true;m.sources[1].Ignite(0);await Task.Delay(160);Check("Unsafe spreading fire automatically starts evacuation",v.Active&&v.Reason.Contains("unsafe"),v.Reason);
            Reset();v.extinguisher.ResetEquipment();p.ResetPose(new Vector3(4.7f,.025f,-1.25f));p.view.LookAt(v.extinguisher.pickupRenderer.bounds.center);Physics.SyncTransforms();v.extinguisher.TryPickup(new Ray(p.view.position,p.view.forward));v.extinguisher.PullPin();await Task.Delay(600);m.StartScenario();v.Begin("Stop firefighting test");float capacity=v.extinguisher.Remaining;v.extinguisher.StepOperation(new Ray(p.view.position,p.view.forward),true,0,.05f);
            Check("Committed evacuation stops extinguisher discharge",!v.extinguisher.Spraying&&v.extinguisher.Remaining==capacity);
            Reset();p.ResetPose(new Vector3(17.5f,.025f,0));for(int i=0;i<100;i++)p.StepMotor(Vector2.right,false,false,1f/60);Check("Courtyard perimeter prevents walking off platform",p.transform.position.x<18,p.transform.position.ToString());
        }finally{
            if(block)UnityEngine.Object.DestroyImmediate(block);if(k!=null)InputSystem.RemoveDevice(k);if(oldK!=null&&oldK.added)oldK.MakeCurrent();v.enabled=true;m.enabled=true;m.ResetScenario();p.enabled=false;p.ResetPose(v.spawn);p.enabled=true;p.SetCapture(false);InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;InputSystem.settings.backgroundBehavior=oldFocus;Application.runInBackground=bg;
        }
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{generatedUtc=DateTime.UtcNow,scope="Editor Play Mode; real Input System bindings and deterministic CharacterController route traversal",checks},Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Path.GetFullPath("../Documentation/Phase5PlayModeTests.json"),json);return json;
    }
}

