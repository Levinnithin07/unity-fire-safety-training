using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FireSafety
{
    // Runs only when explicitly requested by the standalone QA command line.
    public static class FinalPlacementChecks
    {
        public static async Task Run(string output)
        {
            var s=TrainingSession.Instance;
            var board=GameObject.Find("Classroom - licensed source art").transform.Find("NoticeBoards/CorkBoard1");
            var alarm=s.simulation.alarm;
            var point=GameObject.Find("fire_alarm_2k").transform;
            var checks=new System.Collections.Generic.List<object>();
            bool passed=true;
            void Check(string name,bool pass){checks.Add(new{name,pass});passed&=pass;}
            Check("Only targeted main classroom board is inactive",!board.gameObject.activeSelf&&
                board.parent.Find("CorkBoard2").gameObject.activeSelf);
            Check("Sounder retains controller audio beacon and detector connections",
                alarm.sounder&&alarm.beacon&&alarm.lens&&s.simulation.detectors.All(d=>d.alarm==alarm));
            Check("Sounder beside doorway at 2.40m",Vector3.Distance(alarm.transform.position,new Vector3(3.4462f,2.4f,2f))<.002f);
            Check("Call point beside doorway at 1.35m",Vector3.Distance(point.position,new Vector3(3.4458f,1.35f,2f))<.002f);
            Check("Existing call point mesh colliders preserved",point.GetComponentsInChildren<MeshCollider>().Length==2);
            foreach(var t in new[]{alarm.transform,point}) {
                // Both original devices face +X; test the solid wall immediately behind them.
                var hits=Physics.RaycastAll(new Vector3(3.7f,t.position.y,2f),Vector3.left,.4f);
                Check(t.name+" backed by solid wall",hits.Any(h=>h.collider.name=="Walls_2"&&Mathf.Abs(h.point.x-3.44515f)<.005f));
            }
            s.ChooseMode(TrainingMode.Evacuation);s.StartSimulation();
            bool simulationEnabled=s.simulation.enabled;
            try {
                s.simulation.enabled=false;
                for(int i=0;i<700&&!alarm.Active;i++)s.simulation.StepSimulation(.05f);
                await Task.Delay(150);
                Check("Relocated sounder plays after smoke detector activation",alarm.Active&&alarm.sounder.isPlaying);
            } finally {s.simulation.enabled=simulationEnabled;s.MainMenu();}
            File.WriteAllText(Path.Combine(output,"Placements.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{checks,passed,
                manualCallPointInteraction="Not implemented in the original project; model and colliders retained."},Newtonsoft.Json.Formatting.Indented));
            if(!passed)throw new Exception("Built scene placement checks failed.");
            var camera=s.player.view.GetComponent<Camera>();
            var pos=camera.transform.localPosition;var rot=camera.transform.localRotation;float fov=camera.fieldOfView;
            bool playerEnabled=s.player.enabled;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            var enabled=canvases.Select(c=>c.enabled).ToArray();
            try {
                s.player.enabled=false;
                foreach(var c in canvases)c.enabled=false;
                camera.transform.position=new Vector3(5.6f,1.7f,.1f);
                camera.transform.LookAt(new Vector3(3.45f,1.8f,.7f));camera.fieldOfView=70;
                await Task.Delay(400);
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"Corrected-Window-And-Alarms.png"));
                await Task.Delay(500);
            } finally {
                camera.transform.localPosition=pos;camera.transform.localRotation=rot;camera.fieldOfView=fov;
                s.player.enabled=playerEnabled;
                for(int i=0;i<canvases.Length;i++)canvases[i].enabled=enabled[i];
            }
        }
    }
}
