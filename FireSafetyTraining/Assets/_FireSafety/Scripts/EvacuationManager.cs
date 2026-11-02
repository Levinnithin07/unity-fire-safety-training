using UnityEngine;
using UnityEngine.InputSystem;
namespace FireSafety
{
    [DefaultExecutionOrder(150)]
    public sealed class EvacuationManager : MonoBehaviour
    {
        public FireManager simulation;
        public FireExtinguisherController extinguisher;
        public DesktopPlayerController player;
        public NavigationManager navigation;
        public ExitDoor exitDoor;
        public BoxCollider assemblyZone;
        public AudioSource arrivalSound;
        public Vector3 spawn=new Vector3(0,.08f,-1.95f);
        public bool Active { get; private set; }
        public bool Completed { get; private set; }
        public bool CrossedExit { get; private set; }
        public float Elapsed { get; private set; }
        public float StartedAt { get; private set; }=-1;
        public float HazardExposure { get; private set; }
        public int Reroutes { get; private set; }
        public string Reason { get; private set; }="";
        public string Notice { get; private set; }="";
        public bool NearHazard { get; private set; }
        public float RerouteNoticeUntil { get; private set; }
        public event System.Action Evacuated;
        InputAction evacuate;
        float refresh;
        string priorRoute="";
        void Awake(){evacuate=new InputAction("Choose evacuation",InputActionType.Button,"<Keyboard>/v");}
        void OnEnable(){evacuate?.Enable();if(simulation)simulation.ScenarioReset+=ResetEvacuation;}
        void OnDisable(){evacuate?.Disable();if(simulation)simulation.ScenarioReset-=ResetEvacuation;}
        void OnDestroy()=>evacuate?.Dispose();
        public void ResetEvacuation()
        {
            bool returnToStart=Active||Completed;
            Active=Completed=CrossedExit=NearHazard=false;Elapsed=HazardExposure=refresh=RerouteNoticeUntil=0;StartedAt=-1;Reroutes=0;Reason=Notice=priorRoute="";
            if(returnToStart&&Application.isPlaying)player.ResetPose(spawn);
            exitDoor.ResetDoor();navigation.ResetNavigation();foreach(var route in navigation.routes)route.Refresh();if(arrivalSound)arrivalSound.Stop();
        }
        public bool Begin(string reason)
        {
            if(!enabled)return false;
            if(Active||Completed||!simulation.Running)return false;
            Active=true;Reason=reason;StartedAt=simulation.Elapsed;Elapsed=0;refresh=0;
            CrossedExit=player.transform.position.x>7.3f;
            Replan();return true;
        }
        void Update()
        {
            if(!TrainingSession.InputAllowed)return;
            EvaluateTransition(player.Captured&&evacuate.WasPressedThisFrame());
            Step(Time.deltaTime);
        }
        public void EvaluateTransition(bool evacuationChosen=false)
        {
            if(!enabled||Active||Completed||!simulation.Running)return;
            if(evacuationChosen)Begin("You chose to evacuate");
            else if(simulation.BurningCount>0&&!extinguisher.SafeToAttempt)Begin("Fire conditions are unsafe");
            else if(extinguisher.SuppressionCompleted)Begin("Fire suppressed — leave and stay out");
            else if(simulation.alarm.Active)Begin("Smoke alarm activated");
        }
        public void Replan()
        {
            navigation.Recalculate(player.transform.position);
            string next=navigation.RouteName;
            if((priorRoute=="FRONT ROUTE"&&next=="REAR ROUTE")||(priorRoute=="REAR ROUTE"&&next=="FRONT ROUTE")) {Reroutes++;RerouteNoticeUntil=Elapsed+5;}
            priorRoute=next;
        }
        public void Step(float dt)
        {
            if(!enabled||!Active||Completed||dt<=0)return;
            Elapsed+=dt;refresh-=dt;if(refresh<=0){Replan();refresh=.3f;}
            NearHazard=simulation.HeatAt(player.transform.position)>=65;
            foreach(var route in navigation.routes)if(route.Blocked&&Vector3.Distance(player.transform.position,route.transform.position)<1.4f)NearHazard=true;
            if(NearHazard)HazardExposure+=dt;
            if(exitDoor.IsPassable&&player.transform.position.x>7.3f)CrossedExit=true;
            Notice=NearHazard?"Move away from this hazard. Use a clear alternative.":!navigation.Available?"No clear route detected. Do not enter smoke; seek a safer location and signal for help.":Elapsed<RerouteNoticeUntil?"Route changed — follow the alternative doorway.":"Keep moving to the outside assembly point. Do not re-enter.";
            if(CrossedExit&&assemblyZone.bounds.Contains(player.transform.position+Vector3.up*.5f)&&simulation.HeatAt(player.transform.position)<40){
                Completed=true;Active=false;Notice="Assembly point reached. Stay here and report to the person in charge.";if(arrivalSound)arrivalSound.Play();Evacuated?.Invoke();
            }
        }
    }
}
