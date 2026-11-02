using UnityEngine;
using UnityEngine.InputSystem;

namespace FireSafety
{
    [DefaultExecutionOrder(100)]
    public sealed class FireExtinguisherController : MonoBehaviour
    {
        public FireManager simulation;
        public DesktopPlayerController player;
        public EvacuationManager evacuation;
        public GameObject pickupObject, heldRoot, pinObject;
        public Transform nozzle, trigger;
        public ParticleSystem discharge;
        public AudioSource spraySound, pinSound;
        public Renderer pickupRenderer;
        public float capacitySeconds = 12, pickupRange = 2.1f, minimumRange = 1.4f, maximumRange = 4;
        public LayerMask environmentMask = 1;
        public bool Held { get; private set; }
        public bool TrainingEnabled { get; private set; }=true;
        public float SuppressedAt { get; private set; }=-1;
        public void SetTrainingEnabled(bool value){ResetEquipment();TrainingEnabled=value;enabled=value;}
        public bool PinPulled { get; private set; }
        public bool PullingPin { get; private set; }
        public bool CanPickUp { get; private set; }
        public bool Spraying { get; private set; }
        public bool CorrectAim { get; private set; }
        public bool AimCompleted { get; private set; }
        public bool SqueezeCompleted { get; private set; }
        public bool SweepCompleted { get; private set; }
        public bool SuppressionCompleted { get; private set; }
        public bool UnsafeEncountered { get; private set; }
        public float Remaining { get; private set; }
        public float SpraySeconds { get; private set; }
        public float CorrectAimSeconds { get; private set; }
        public float EffectiveSeconds { get; private set; }
        public float AgentSuppression { get; private set; }
        public float SweepSpeed { get; private set; }
        public float Accuracy => SpraySeconds > .01f ? 100 * CorrectAimSeconds / SpraySeconds : 0;
        public string Feedback { get; private set; } = "Find the extinguisher in the passage beside the classroom doorway.";
        public string AimFeedback { get; private set; } = "Aim at the base of the flames.";
        public FireSource AimedFire { get; private set; }
        public bool SafeToAttempt => simulation.BurningCount <= 1 && simulation.TotalIntensity < .68f && simulation.SmokeLevel < .22f && simulation.ActiveSprinklers == 0;
        public bool PassComplete => PinPulled && AimCompleted && SqueezeCompleted && SweepCompleted && SuppressionCompleted && !UnsafeEncountered;
        InputAction interact, pull, squeeze;
        Collider[] pickupColliders;
        bool[] colliderStates;
        MaterialPropertyBlock highlight;
        Vector3 pinHome;
        Quaternion triggerHome;
        float pinProgress, aimSeconds, lastYaw, runDegrees, recentSweep, lostTargetSeconds;
        float strokePosition, strokeStart, strokeExtreme;
        int runSign, completedLegs;
        bool yawValid, targetSweepQualified, waitForMouseRelease;
        FireSource sweepTarget;

        void Awake()
        {
            interact = new InputAction("Pick up extinguisher", InputActionType.Button, "<Keyboard>/e");
            pull = new InputAction("Pull safety pin", InputActionType.Button, "<Keyboard>/p");
            squeeze = new InputAction("Squeeze extinguisher", InputActionType.Button, "<Mouse>/leftButton");
            pickupColliders = pickupObject.GetComponentsInChildren<Collider>(); colliderStates = new bool[pickupColliders.Length];
            for(int i=0;i<pickupColliders.Length;i++) colliderStates[i]=pickupColliders[i].enabled;
            pinHome=pinObject.transform.localPosition;triggerHome=trigger.localRotation;highlight=new MaterialPropertyBlock();
        }
        void OnEnable()
        {
            interact?.Enable();pull?.Enable();squeeze?.Enable();
            if(simulation)simulation.ScenarioReset+=ResetEquipment;
        }
        void Start() => ResetEquipment();
        void OnDisable()
        {
            interact?.Disable();pull?.Disable();squeeze?.Disable();
            if(simulation)simulation.ScenarioReset-=ResetEquipment;
            StopDischarge(); ClearHighlight();
        }
        void OnDestroy(){interact?.Dispose();pull?.Dispose();squeeze?.Dispose();}
        public void ResetEquipment()
        {
            StopDischarge();Held=PinPulled=PullingPin=CanPickUp=CorrectAim=false;
            AimCompleted=SqueezeCompleted=SweepCompleted=SuppressionCompleted=UnsafeEncountered=false;
            Remaining=capacitySeconds;SpraySeconds=CorrectAimSeconds=EffectiveSeconds=AgentSuppression=aimSeconds=pinProgress=0;
            SuppressedAt=-1;
            AimedFire=sweepTarget=null;ResetSweep();yawValid=false;waitForMouseRelease=true;
            if(heldRoot)heldRoot.SetActive(false);
            if(pickupRenderer)pickupRenderer.enabled=true;
            if(pickupColliders!=null)for(int i=0;i<pickupColliders.Length;i++)pickupColliders[i].enabled=colliderStates[i];
            if(pinObject){pinObject.SetActive(true);pinObject.transform.localPosition=pinHome;}
            if(trigger)trigger.localRotation=triggerHome;
            if(discharge)discharge.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(pinSound)pinSound.Stop();ClearHighlight();
            Feedback="Find the extinguisher in the passage beside the classroom doorway.";
        }
        void ClearHighlight(){if(pickupRenderer)pickupRenderer.SetPropertyBlock(null);}
        public bool CheckPickup(Ray ray)
        {
            if(!TrainingEnabled||Held)return false;
            return Physics.Raycast(ray,out var hit,pickupRange,environmentMask,QueryTriggerInteraction.Ignore)
                && (hit.transform==pickupObject.transform||hit.transform.IsChildOf(pickupObject.transform));
        }
        public bool TryPickup(Ray ray)
        {
            if(!CheckPickup(ray))return false;
            Held=true;heldRoot.SetActive(true);pickupRenderer.enabled=false;
            foreach(var c in pickupColliders)c.enabled=false;
            ClearHighlight();waitForMouseRelease=true;
            Feedback="P - Pull the pin. In a real emergency, alert others and keep an escape route behind you.";
            return true;
        }
        public bool PullPin()
        {
            if(!TrainingEnabled||!Held||PinPulled||PullingPin)return false;
            PullingPin=true;pinProgress=0;if(pinSound)pinSound.Play();return true;
        }
        void Update()
        {
            if(!TrainingSession.InputAllowed)return;
            // Match FireManager's elapsed-time limit so low frame rates do not weaken discharge.
            float dt=Mathf.Min(Time.deltaTime,.25f);
            Ray ray=new Ray(player.view.position,player.view.forward);
            CanPickUp=player.Captured&&CheckPickup(ray);
            if(!Held){
                if(CanPickUp){highlight.SetColor("_EmissionColor",new Color(.08f,.22f,.22f));pickupRenderer.SetPropertyBlock(highlight);Feedback="E - Pick up fire extinguisher";}
                else {ClearHighlight();Feedback="Find the extinguisher in the passage beside the classroom doorway.";}
                if(player.Captured&&interact.WasPressedThisFrame())TryPickup(ray);
            }
            if(Held&&player.Captured&&pull.WasPressedThisFrame())PullPin();
            if(PullingPin){
                pinProgress=Mathf.Min(1,pinProgress+dt/.4f);
                pinObject.transform.localPosition=pinHome+Vector3.left*(.13f*Mathf.SmoothStep(0,1,pinProgress));
                if(pinProgress>=1){PinPulled=true;PullingPin=false;pinObject.SetActive(false);}
            }
            if(!player.Captured||player.LastCaptureFrame==Time.frameCount)waitForMouseRelease=true;
            if(!squeeze.IsPressed())waitForMouseRelease=false;
            bool fire=player.Captured&&!waitForMouseRelease&&squeeze.IsPressed();
            StepOperation(ray,fire,player.view.eulerAngles.y,dt);
            if(Held&&PinPulled&&player.Captured&&waitForMouseRelease&&squeeze.IsPressed()&&!(evacuation&&(evacuation.Active||evacuation.Completed)))
                Feedback="Release LEFT MOUSE once, then hold it to spray.";
            if(trigger)trigger.localRotation=triggerHome*Quaternion.Euler(Spraying?14:0,0,0);
        }
        public FireSource FindBaseTarget(Ray ray, out Vector3 point)
        {
            point=ray.GetPoint(maximumRange);FireSource best=null;float nearest=maximumRange;
            AimFeedback="Aim at the base of the flames; hold LEFT MOUSE to spray.";
            foreach(var f in simulation.sources){
                if(!f.Burning)continue;
                Vector3 center=f.transform.position+Vector3.up*.075f;
                float distance=Vector3.Dot(center-ray.origin,ray.direction);
                if(distance<minimumRange){AimFeedback="Too close. Step back at least 1.4 m and aim at the base.";continue;}
                if(distance>nearest){if(!best)AimFeedback="Too far away. Move within 4 m with a clear escape route.";continue;}
                Vector3 p=ray.GetPoint(distance);Vector3 flat=p-center;flat.y=0;
                if(Mathf.Abs(p.y-center.y)>.17f){AimFeedback=p.y>center.y?"Aim LOWER, at the base of the flames, until the crosshair turns green.":"Aim slightly higher, at the base of the flames, until the crosshair turns green.";continue;}
                if(flat.magnitude>.28f){AimFeedback="Move the crosshair onto the BASE of the flames until it turns green.";continue;}
                if(Physics.Raycast(ray,out _,Mathf.Max(0,distance-.09f),environmentMask,QueryTriggerInteraction.Ignore)){AimFeedback="View blocked. Move into a clear aisle before spraying.";continue;}
                Vector3 fromNozzle=p-nozzle.position;
                if(Physics.Raycast(nozzle.position,fromNozzle.normalized,Mathf.Max(0,fromNozzle.magnitude-.09f),environmentMask,QueryTriggerInteraction.Ignore)){AimFeedback="Nozzle blocked by furniture. Step sideways into a clear aisle.";continue;}
                nearest=distance;point=p;best=f;
            }
            return best;
        }
        void ResetSweep(){runDegrees=recentSweep=lostTargetSeconds=strokePosition=strokeStart=strokeExtreme=0;runSign=completedLegs=0;targetSweepQualified=false;SweepSpeed=0;}
        void TrackSweep(float turn)
        {
            strokePosition+=turn;
            if(runSign==0){if(Mathf.Abs(turn)<.0001f)return;runSign=turn>0?1:-1;strokeStart=strokePosition-turn;strokeExtreme=strokePosition;}
            else {
                strokeExtreme=runSign>0?Mathf.Max(strokeExtreme,strokePosition):Mathf.Min(strokeExtreme,strokePosition);
                // A real reversal must travel 0.75 degrees; tiny hand jitter cannot erase a valid stroke.
                if((strokeExtreme-strokePosition)*runSign>=.75f){
                    if(Mathf.Abs(strokeExtreme-strokeStart)>=3)completedLegs++;else completedLegs=0;
                    strokeStart=strokeExtreme;runSign=-runSign;strokeExtreme=strokePosition;
                }
            }
            runDegrees=Mathf.Abs(strokeExtreme-strokeStart);
            if(completedLegs>=1&&runDegrees>=3){targetSweepQualified=true;SweepCompleted=true;}
        }
        public void StepOperation(Ray ray,bool pressed,float yaw,float dt)
        {
            if(!TrainingEnabled){StopDischarge();return;}
            if(dt<=0)return;dt=Mathf.Min(dt,.25f);
            float turn=yawValid?Mathf.DeltaAngle(lastYaw,yaw):0;lastYaw=yaw;yawValid=true;recentSweep=Mathf.Max(0,recentSweep-dt);
            if(!Held){StopDischarge();return;}
            if(evacuation&&(evacuation.Active||evacuation.Completed)){
                StopDischarge();CorrectAim=false;Feedback=evacuation.Completed?"Stay at assembly point A. R restarts training.":"Spray disabled: "+evacuation.Reason+". Evacuate, or press R to restart practice.";return;
            }
            AimedFire=FindBaseTarget(ray,out var targetPoint);CorrectAim=AimedFire;
            if(SuppressionCompleted&&simulation.BurningCount==0){
                StopDischarge();Feedback="Fire Successfully Extinguished. Your results are ready.";return;
            }
            bool safe=SafeToAttempt;
            if(CorrectAim&&PinPulled&&safe){aimSeconds+=dt;if(aimSeconds>=.4f)AimCompleted=true;}
            if(!PinPulled){StopDischarge();Feedback=PullingPin?"Pulling the safety pin...":"P - Pull the pin before squeezing.";return;}
            if(Remaining<=0){StopDischarge();Feedback="Extinguisher empty. Leave by a safe route; do not keep attempting suppression.";return;}
            if(!safe&&simulation.Running&&simulation.BurningCount>0)Feedback="UNSAFE: fire has grown or spread. Stop the attempt and evacuate.";
            else if(SuppressionCompleted)Feedback="Fire suppressed. Stay clear, watch for re-ignition and leave by a safe route.";
            else if(!simulation.Running)Feedback="Ready. Keep an exit behind you. ENTER starts the practice fire.";
            else if(!CorrectAim)Feedback=AimFeedback;
            else Feedback="Base targeted. Hold LEFT MOUSE and sweep slowly from side to side.";
            if(!pressed){StopDischarge();lostTargetSeconds+=dt;if(lostTargetSeconds>.6f)ResetSweep();return;}
            float used=Mathf.Min(dt,Remaining);Remaining=Mathf.Max(0,Remaining-used);SpraySeconds+=used;
            Spraying=true;
            if(!safe&&simulation.BurningCount>0)UnsafeEncountered=true;
            if(!CorrectAim||!safe){lostTargetSeconds+=dt;if(lostTargetSeconds>.25f){ResetSweep();sweepTarget=null;}}
            else {
                if(sweepTarget!=AimedFire){ResetSweep();sweepTarget=AimedFire;}
                lostTargetSeconds=0;CorrectAimSeconds+=used;SqueezeCompleted=CorrectAimSeconds>=.25f;
                SweepSpeed=Mathf.Abs(turn)/dt;
                bool controlled=SweepSpeed>=2&&SweepSpeed<=55;
                if(SweepSpeed<=55){TrackSweep(turn);if(controlled)recentSweep=.35f;}
                else {ResetSweep();}
                // Correct base aiming has a visible effect; controlled coverage is substantially stronger.
                float rate=targetSweepQualified&&recentSweep>0?.16f:.055f;
                float applied=AimedFire.ApplyExtinguishingAgent(rate*used);AgentSuppression+=applied;
                if(rate>.1f){EffectiveSeconds+=used;Feedback="Controlled sweep - keep covering the base.";}
                else Feedback="Base targeted - reducing fire. Sweep steadily left AND right for full PASS credit.";
                if(AimedFire.ExtinguishedByAgent){SuppressionCompleted=true;SuppressedAt=simulation.Elapsed;Feedback="Fire Successfully Extinguished";}
            }
            UpdateDischarge(ray,targetPoint);
        }
        void UpdateDischarge(Ray ray,Vector3 targetPoint)
        {
            if(!CorrectAim){targetPoint=ray.GetPoint(maximumRange);if(Physics.Raycast(ray,out var hit,maximumRange,environmentMask,QueryTriggerInteraction.Ignore))targetPoint=hit.point;}
            discharge.transform.position=nozzle.position;
            Vector3 direction=targetPoint-nozzle.position;
            if(direction.sqrMagnitude>.001f)discharge.transform.rotation=Quaternion.LookRotation(direction);
            float range=Mathf.Min(maximumRange,direction.magnitude);
            if(Physics.Raycast(nozzle.position,discharge.transform.forward,out var block,range,environmentMask,QueryTriggerInteraction.Ignore))range=block.distance;
            var main=discharge.main;main.startLifetime=Mathf.Clamp(range/8,.025f,.5f);
            if(!discharge.isPlaying)discharge.Play();if(spraySound&&!spraySound.isPlaying)spraySound.Play();
        }
        void StopDischarge()
        {
            Spraying=false;
            if(discharge&&discharge.isEmitting)discharge.Stop(false,ParticleSystemStopBehavior.StopEmitting);
            if(spraySound&&spraySound.isPlaying)spraySound.Stop();
        }
    }
}

