using UnityEngine;
namespace FireSafety
{
    [DefaultExecutionOrder(-200)]
    public sealed class SchoolEvacuationController : MonoBehaviour
    {
        public TrainingSession session;
        public ExitDoor exit;
        public SchoolEvacuee[] people;
        public int ArrivedCount { get {int n=0;foreach(var p in people)if(p.Arrived)n++;return n;} }
        void OnEnable(){session.AttemptReset+=ResetPeople;session.SimulationStarted+=BeginEvacuation;}
        void OnDisable(){if(session){session.AttemptReset-=ResetPeople;session.SimulationStarted-=BeginEvacuation;}}
        public void ResetPeople(){foreach(var p in people)p.ResetPerson(session,exit);}
        public void BeginEvacuation(){foreach(var p in people)p.Begin();}
    }
}
