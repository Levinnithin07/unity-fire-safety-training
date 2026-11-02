using UnityEngine;
using UnityEngine.InputSystem;

namespace FireSafety
{
    // Demonstration model: normalized smoke and heat inputs, not a fire-engineering solver.
    public sealed class FireManager : MonoBehaviour
    {
        public FireSource[] sources;
        public SmokeDetector[] detectors;
        public SprinklerController[] sprinklers;
        public FireAlarmController alarm;
        public DesktopPlayerController player;
        public event System.Action ScenarioReset;
        public bool Running { get; private set; }
        public float Elapsed { get; private set; }
        public float SmokeLevel { get; private set; }
        public float PassageSmoke { get; private set; }
        public int BurningCount { get { int n = 0; foreach (var s in sources) if (s.Burning) n++; return n; } }
        public int ActiveSprinklers { get { int n = 0; foreach (var s in sprinklers) if (s.Active) n++; return n; } }
        public float TotalIntensity { get { float n = 0; foreach (var s in sources) n += s.intensity; return n; } }
        public bool NearHeat => player && HeatAt(player.transform.position) > 70;
        public bool ControlledTraining { get; private set; }
        public bool ReadyToStart => true;
        public void ConfigureTraining(bool controlled)
        {
            ControlledTraining=controlled;
            foreach(var s in sprinklers)s.SetAvailable(!controlled);
        }
        InputAction start, reset, mute;
        float accumulator;
        void Awake()
        {
            start = new InputAction("Start emergency demonstration", InputActionType.Button, "<Keyboard>/enter");
            reset = new InputAction("Reset emergency demonstration", InputActionType.Button, "<Keyboard>/r");
            mute = new InputAction("Mute alarm for inspection", InputActionType.Button, "<Keyboard>/m");
        }
        void OnEnable() { start?.Enable(); reset?.Enable(); mute?.Enable(); }
        void Start() => ResetScenario();
        void Update()
        {
            if(!TrainingSession.InputAllowed)return;
            var k=Keyboard.current;bool alt=k!=null&&(k.leftAltKey.isPressed||k.rightAltKey.isPressed);
            if (!TrainingSession.Instance && reset.WasPressedThisFrame()) ResetScenario();
            else if (start.WasPressedThisFrame() && !alt && !Running && ReadyToStart) StartScenario();
            if (mute.WasPressedThisFrame() && alarm) alarm.ToggleMute();
            if (!Running) return;
            accumulator += Mathf.Min(Time.deltaTime, .25f);
            while (accumulator >= .05f) { StepSimulation(.05f); accumulator -= .05f; }
        }
        public void StartScenario()
        {
            if (Running || sources.Length == 0) return;
            Running = true; sources[0].Ignite(Elapsed);
            if(ControlledTraining)sources[0].intensity=.45f;
        }
        public void ResetScenario()
        {
            Running = false; Elapsed = SmokeLevel = PassageSmoke = accumulator = 0;
            foreach (var s in sources) s.ResetFire();
            foreach (var d in detectors) d.ResetDetector();
            foreach (var s in sprinklers) s.ResetSprinkler();
            if (alarm) alarm.ResetAlarm();
            ScenarioReset?.Invoke();
        }
        public float HeatAt(Vector3 point)
        {
            float temperature = 22;
            foreach (var s in sources) if (s.Burning) {
                Vector3 d = point - s.transform.position; d.y = 0;
                temperature += 118 * s.intensity / (1 + d.sqrMagnitude * 1.6f);
            }
            return temperature;
        }
        public void StepSimulation(float dt)
        {
            if (!Running || dt <= 0) return;
            // Public for deterministic verification; all caller steps are <= 50 ms.
            dt = Mathf.Min(dt, .05f); Elapsed += dt;
            foreach (var s in sprinklers) s.Step(HeatAt(s.transform.position), dt, Elapsed);
            foreach (var f in sources) {
                float water = 0;
                foreach (var s in sprinklers) water += s.SuppressionAt(f.transform.position);
                // A supervised training fire remains contained until the learner suppresses it.
                if(!ControlledTraining)f.Step(dt, water);
            }
            foreach (var f in sources) if (!ControlledTraining && f.Burning && f.intensity > .4f && f.spreadTargets != null)
                foreach (var target in f.spreadTargets) if (target && !target.Burning) {
                    float distance = Vector3.Distance(f.transform.position, target.transform.position);
                    if (distance < 1.5f) target.Expose(f.intensity * .42f / (1 + distance), dt, Elapsed);
                }
            SmokeLevel = Mathf.Clamp01(SmokeLevel + (TotalIntensity * .014f - SmokeLevel * .022f) * dt);
            if(ControlledTraining)SmokeLevel=Mathf.Min(SmokeLevel,.08f);
            PassageSmoke = Mathf.Clamp01(PassageSmoke + (SmokeLevel * .12f - PassageSmoke) * dt * .08f);
            if(!ControlledTraining)foreach (var d in detectors) d.Step(d.inPassage ? PassageSmoke : SmokeLevel, dt, Elapsed);
        }
        void OnDisable() { start?.Disable(); reset?.Disable(); mute?.Disable(); }
        void OnDestroy() { start?.Dispose(); reset?.Dispose(); mute?.Dispose(); }
    }
}
