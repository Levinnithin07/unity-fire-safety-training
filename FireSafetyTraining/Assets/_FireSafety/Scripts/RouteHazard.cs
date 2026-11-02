using UnityEngine;
using TMPro;
namespace FireSafety
{
    public sealed class RouteHazard : MonoBehaviour
    {
        public FireManager simulation;
        public string routeName;
        public int navigationArea;
        public float smokeTransport=1, smokeClosure=.16f;
        public ParticleSystem smoke;
        public TMP_Text sign;
        public float Smoke => simulation.SmokeLevel*smokeTransport;
        public float Heat => simulation.HeatAt(transform.position);
        public bool Obstructed { get; private set; }
        public bool Blocked => Smoke>=smokeClosure||Heat>=65||Obstructed;
        public string Reason => Smoke>=smokeClosure?"SMOKE":Heat>=65?"HEAT":Obstructed?"OBSTRUCTION":"CLEAR";
        public void Refresh()
        {
            var start=transform.position+Vector3.left*.65f;
            Obstructed=Physics.CapsuleCast(start+Vector3.up*.3f,start+Vector3.up*1.5f,.22f,Vector3.right,1.3f,1,QueryTriggerInteraction.Ignore);
            if(sign){sign.text=Blocked?routeName+"\nUSE OTHER ROUTE":routeName+"\nEXIT  >";sign.color=Blocked?new Color(1,.4f,.23f):new Color(.65f,1,.82f);}
            if(smoke){var em=smoke.emission;em.rateOverTime=Mathf.Clamp(Smoke*90,0,24);if(Smoke>.015f){if(!smoke.isPlaying)smoke.Play();}else smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        }
    }
}
