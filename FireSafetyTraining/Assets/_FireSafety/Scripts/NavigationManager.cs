using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace FireSafety
{
    public sealed class NavigationManager : MonoBehaviour
    {
        public RouteHazard[] routes;
        public Transform assembly;
        public int agentType;
        public Vector3[] Corners { get; private set; }=new Vector3[0];
        public bool Available { get; private set; }
        public string RouteName { get; private set; }="";
        public float Distance { get; private set; }
        NavMeshPath path;
        public void ResetNavigation(){Corners=new Vector3[0];Available=false;Distance=0;RouteName="";path?.ClearCorners();}
        public void Recalculate(Vector3 position)
        {
            if(path==null)path=new NavMeshPath();
            int mask=NavMesh.AllAreas;
            foreach(var route in routes){route.Refresh();if(route.Blocked)mask&=~(1<<route.navigationArea);}
            var filter=new NavMeshQueryFilter{agentTypeID=agentType,areaMask=mask};
            foreach(var route in routes)filter.SetAreaCost(route.navigationArea,1+route.Smoke*12);
            Available=false;Corners=new Vector3[0];Distance=0;RouteName="NO CLEAR ROUTE";
            if(!NavMesh.SamplePosition(position,out var start,.7f,filter)||!NavMesh.SamplePosition(assembly.position,out var end,.5f,filter))return;
            if(!NavMesh.CalculatePath(start.position,end.position,filter,path)||path.status!=NavMeshPathStatus.PathComplete)return;
            var corners=path.corners;
            // Reject newly introduced physical obstructions. Operable exit leaves are expected and prompted separately.
            for(int i=1;i<corners.Length;i++){
                Vector3 a=corners[i-1]+Vector3.up*.65f,b=corners[i]+Vector3.up*.65f;Vector3 d=b-a;
                foreach(var hit in Physics.SphereCastAll(a,.19f,d.normalized,d.magnitude,1,QueryTriggerInteraction.Ignore))if(!hit.collider.GetComponentInParent<ExitDoor>())return;
            }
            Corners=corners;Available=true;RouteName=position.x>6.7f?"ASSEMBLY POINT A":"EXTERIOR EXIT";
            for(int i=1;i<corners.Length;i++){
                Distance+=Vector3.Distance(corners[i-1],corners[i]);
                Vector3 a=corners[i-1],b=corners[i];
                if((a.x-3.3f)*(b.x-3.3f)<=0&&Mathf.Abs(b.x-a.x)>.01f){float z=Mathf.Lerp(a.z,b.z,(3.3f-a.x)/(b.x-a.x));RouteName=z<0?"FRONT ROUTE":"REAR ROUTE";}
            }
        }
        public Vector3 NextPoint(Vector3 position)
        {
            // Corner zero is the recent query origin, not a waypoint to turn back toward.
            for(int i=1;i<Corners.Length;i++){var d=Corners[i]-position;d.y=0;if(d.magnitude>.5f)return Corners[i];}
            return assembly.position;
        }
    }
}
