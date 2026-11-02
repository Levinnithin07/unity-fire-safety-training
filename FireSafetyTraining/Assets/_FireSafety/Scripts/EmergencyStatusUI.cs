using UnityEngine;
using TMPro;
namespace FireSafety
{
    public sealed class EmergencyStatusUI : MonoBehaviour
    {
        public FireManager simulation;
        public FireExtinguisherController extinguisher;
        public EvacuationManager evacuation;
        public TMP_Text heading, detail, instruments, controls;
        public UnityEngine.UI.Image accent;
        float refresh;
        void Update()
        {
            refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=.15f;
            var m=simulation;if(!m)return;
            bool equipment=TrainingSession.Instance?TrainingSession.Instance.IsExtinguisherMode:extinguisher.TrainingEnabled;
            accent.color=equipment?new Color(1,.66f,.25f):new Color(.21f,.79f,.77f);
            if(equipment){
                heading.text=extinguisher.SuppressionCompleted?"FIRE SUCCESSFULLY EXTINGUISHED":"FIRE EXTINGUISHER TRAINING";
                detail.text=extinguisher.SuppressionCompleted?"Release the mouse. Preparing your PASS results.":!extinguisher.Held?"Controlled fire ready. Find the passage extinguisher; E picks it up.":!extinguisher.PinPulled?"P - Pull the safety pin before spraying.":"Aim at the base of the flames until the crosshair is green. Hold LEFT MOUSE and sweep.";
                instruments.text=$"PRACTICE TIME  {m.Elapsed:0.0}s   |   Controlled fire   |   Sprinklers OFF";
                controls.text="E  Pick up     P  Pull pin     Hold LEFT MOUSE  Spray     R  Retry     ESC  Pause";
            } else {
                heading.text=evacuation.Completed?"EVACUATION SUCCESSFUL":"EMERGENCY EVACUATION TRAINING";
                detail.text=evacuation.Completed?"Assembly Point A reached. Stay outside and report in.":m.ActiveSprinklers>0?"Sprinklers active. Follow a clear exit route; do not wait by the fire.":m.alarm.Active?"Smoke alarm active. Follow the guidance to Assembly Point A.":"Fire detected. Follow the green guidance to the outside assembly point.";
                if(m.NearHeat&&!evacuation.Completed)detail.text="DANGER: move away from the heat and use a clear alternative route.";
                instruments.text=$"{m.Elapsed:0}s   |   Smoke {m.SmokeLevel*100:0}%*   |   Alarm {(m.alarm.Active?"ON":"standby")}   |   Water {m.ActiveSprinklers}/{m.sprinklers.Length}\n*Relative simulation index; not a measured concentration.";
                controls.text=$"E  Open exit     R  Retry     M  Alarm audio {(m.alarm.Muted?"OFF":"ON")}     ESC  Pause";
            }
            controls.text+="\nEducational simulation - supplements, does not replace, certified training.";
        }
    }
}

