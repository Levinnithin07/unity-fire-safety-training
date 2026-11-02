using UnityEngine;
using TMPro;
namespace FireSafety
{
    [DefaultExecutionOrder(250)]
    public sealed class EvacuationUI : MonoBehaviour
    {
        public EvacuationManager evacuation;
        public ExtinguisherStatusUI equipmentUI;
        public TMP_Text heading,detail,timer,arrow;
        public GameObject card;
        public LineRenderer trail;
        void LateUpdate()
        {
            if(!evacuation.enabled){card.SetActive(false);trail.positionCount=0;return;}
            var e=evacuation;bool show=e.Active||e.Completed;card.SetActive(show);
            if(show){
                equipmentUI.techniquePanel.SetActive(false);
                string stopped=e.extinguisher.SuppressionCompleted?"Fire out: spray ended.":e.simulation.alarm.Active?"Smoke alarm: spray disabled.":!e.extinguisher.SafeToAttempt?"Unsafe fire: spray disabled.":"Evacuating: spray disabled.";
                equipmentUI.prompt.text=e.Active&&e.extinguisher.Held?stopped+" Follow exit route. R: restart.":e.Notice;
            }
            else if(e.simulation.Running&&!e.extinguisher.Held)equipmentUI.prompt.text="V - Choose evacuation. Use a clear exit and go to assembly point A.";
            if(e.exitDoor.CanInteract)equipmentUI.prompt.text="E - Push open the exterior exit doors";
            if(!show){trail.positionCount=0;return;}
            heading.text=e.Completed?"EVACUATION COMPLETE":e.navigation.Available?e.navigation.RouteName:"NO CLEAR ROUTE";
            detail.text=e.Completed?"ASSEMBLY POINT A  /  Stay out":e.Notice;
            timer.text=$"EXIT TIME  {Mathf.FloorToInt(e.Elapsed/60):00}:{e.Elapsed%60:00.0}"+(e.Completed?"":$"   |   {e.navigation.Distance:0}m remaining");
            Vector3 d=e.navigation.NextPoint(e.player.transform.position)-e.player.transform.position;d.y=0;
            float angle=Vector3.SignedAngle(e.player.transform.forward,d,Vector3.up);
            arrow.text=e.Completed?"SAFE":!e.navigation.Available?"STOP":Mathf.Abs(angle)<22?"FORWARD":Mathf.Abs(angle)>145?"TURN BACK":angle>0?"TURN RIGHT  >":"<  TURN LEFT";
            if(!e.Completed&&e.navigation.Available){var c=e.navigation.Corners;trail.positionCount=c.Length;for(int i=0;i<c.Length;i++)trail.SetPosition(i,c[i]+Vector3.up*.045f);}else trail.positionCount=0;
        }
    }
}
