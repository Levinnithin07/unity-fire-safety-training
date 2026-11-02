using UnityEngine;
using UnityEngine.AI;
namespace FireSafety
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class SchoolEvacuee : MonoBehaviour
    {
        public Vector3 initialPosition, assemblyPosition;
        public Quaternion initialRotation=Quaternion.identity;
        public float departureDelay;
        public Animation legacyAnimation;
        public string idleClip, walkClip;
        public bool Arrived { get; private set; }
        public bool Departed { get; private set; }
        public float DistanceTravelled { get; private set; }
        public bool PathFailed { get; private set; }
        NavMeshAgent agent;
        ExitDoor exit;
        TrainingSession session;
        float delay;
        bool evacuating;
        Vector3 previous;
        string currentClip;
        public void ResetPerson(TrainingSession owner,ExitDoor door)
        {
            session=owner;exit=door;agent=GetComponent<NavMeshAgent>();
            if(agent.enabled&&agent.isOnNavMesh){agent.isStopped=true;agent.ResetPath();}
            agent.enabled=false;transform.SetPositionAndRotation(initialPosition,initialRotation);agent.enabled=true;
            Arrived=Departed=PathFailed=evacuating=false;DistanceTravelled=0;delay=departureDelay;previous=transform.position;
            Pose(false);
        }
        public void Begin(){evacuating=true;delay=departureDelay;}
        void Pose(bool moving)
        {
            if(!legacyAnimation)return;
            string clip=moving?walkClip:idleClip;
            if(string.IsNullOrEmpty(clip)||currentClip==clip)return;
            if(legacyAnimation[clip]!=null){legacyAnimation[clip].wrapMode=WrapMode.Loop;legacyAnimation.CrossFade(clip,.18f);currentClip=clip;}
        }
        void Update()
        {
            if(!evacuating||Arrived||!agent||!agent.enabled)return;
            if(!session.Playing&&session.Page!="Results")return;
            if(!agent.isOnNavMesh){PathFailed=true;return;}
            delay-=Time.deltaTime;
            if(delay>0)return;
            if(!Departed){Departed=true;agent.isStopped=false;PathFailed=!agent.SetDestination(assemblyPosition);}
            bool waitForDoor=transform.position.x<7.3f&&Vector3.Distance(transform.position,exit.transform.position)<2.3f&&!exit.IsPassable;
            if(waitForDoor)exit.RequestOpenNearby(transform.position);
            agent.isStopped=waitForDoor;
            DistanceTravelled+=Vector3.Distance(previous,transform.position);previous=transform.position;
            if(!agent.pathPending&&agent.pathStatus==NavMeshPathStatus.PathInvalid)PathFailed=true;
            if(!agent.pathPending&&Vector3.Distance(transform.position,assemblyPosition)<.3f){
                Arrived=true;evacuating=false;agent.isStopped=true;agent.ResetPath();transform.rotation=Quaternion.Euler(0,-90,0);Pose(false);
            }else Pose(!waitForDoor&&agent.velocity.sqrMagnitude>.04f);
        }
    }
}
