using UnityEngine;
using TMPro;
namespace FireSafety
{
    public sealed class ExtinguisherStatusUI : MonoBehaviour
    {
        public FireExtinguisherController extinguisher;
        public TMP_Text prompt,pass,capacity,accuracy;
        public UnityEngine.UI.Image crosshair,fill;
        public GameObject techniquePanel;
        float next;
        void Awake(){prompt.enableAutoSizing=true;prompt.fontSizeMin=18;prompt.fontSizeMax=22;}
        static string Mark(bool value)=>value?"<color=#76E5CC>DONE</color>":"<color=#9DADB7>WAIT</color>";
        void Update()
        {
            next-=Time.unscaledDeltaTime;if(next>0)return;next=.1f;
            var e=extinguisher;if(!e)return;
            if(!e.TrainingEnabled){techniquePanel.SetActive(false);crosshair.color=new Color(1,1,1,.8f);prompt.text="Follow the exit guidance to Assembly Point A.";return;}
            techniquePanel.SetActive(e.Held);prompt.text=e.Feedback;
            crosshair.color=e.CorrectAim?new Color(.3f,1,.65f):e.CanPickUp?new Color(.2f,.95f,.95f):e.Spraying?new Color(1,.65f,.25f):new Color(1,1,1,.8f);
            fill.fillAmount=e.Remaining/e.capacitySeconds;fill.color=e.Remaining<3?new Color(1,.45f,.2f):new Color(.3f,.85f,.75f);
            capacity.text=$"AGENT  {100*e.Remaining/e.capacitySeconds:0}%";
            pass.text=$"P  Pull     {Mark(e.PinPulled)}\nA  Aim     {Mark(e.AimCompleted)}\nS  Squeeze     {Mark(e.SqueezeCompleted)}\nS  Sweep     {Mark(e.SweepCompleted)}";
            accuracy.text=e.PassComplete?"PASS complete - fire suppressed":e.SpraySeconds>.01f?$"Safe on-target discharge: {e.Accuracy:0}%\nEffective sweep: {e.EffectiveSeconds:0.0}s":"P  Pull pin  |  Hold LMB  Spray\nMove mouse left/right to sweep";
        }
    }
}
