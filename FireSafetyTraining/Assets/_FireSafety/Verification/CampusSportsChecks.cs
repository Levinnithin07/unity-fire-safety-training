using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace FireSafety
{
    // Explicit opt-in QA only; does not run during normal training.
    public static class CampusSportsChecks
    {
        public static async Task Run(string output)
        {
            var checks=new List<object>();bool passed=true;
            void Check(string name,bool pass,object evidence=null){checks.Add(new{name,pass,evidence});passed&=pass;}
            var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            var sign=GameObject.Find("SATHYABAMA UNIVERSITY name board");
            var text=sign.GetComponentsInChildren<TMPro.TMP_Text>();
            Check("University board has only the requested name",text.Length==1&&text[0].text=="SATHYABAMA UNIVERSITY");
            var basket=GameObject.Find("Basketball practice court - 22 by 12 metres");
            var volley=GameObject.Find("Volleyball practice court - 14 by 7 metres");
            Check("Both separate practice courts are present",basket&&volley);
            Check("Two imported basketball hoops",basket.GetComponentsInChildren<Transform>().Count(t=>t.name=="Basketball goal west"||t.name=="Basketball goal east")==2);
            Check("Imported volleyball net is present",volley.GetComponentsInChildren<Transform>().Any(t=>t.name=="Volleyball net and posts"));
            Check("Five original ground-floor classrooms remain",all.Count(t=>t.name.EndsWith(" - environment only"))==4&&GameObject.Find("Classroom - licensed source art"));
            var benches=all.Where(t=>t.gameObject.activeInHierarchy&&(t.name=="Garden bench"||t.name=="Courtyard slatted bench"||t.name=="Corridor bench - outside doorway clearance")).ToArray();
            var placements=new List<object>();
            foreach(var bench in benches)
            {
                var renderers=bench.GetComponentsInChildren<Renderer>();
                var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
                var overlaps=Physics.OverlapBox(bounds.center,bounds.extents-Vector3.one*.012f,Quaternion.identity,~0,QueryTriggerInteraction.Ignore)
                    .Where(c=>!c.transform.IsChildOf(bench)&&c.bounds.max.y>bounds.min.y+.06f&&!c.GetComponentInParent<DesktopPlayerController>()&&!c.GetComponentInParent<SchoolEvacuee>())
                    .Select(c=>c.name).Distinct().ToArray();
                var hits=Physics.RaycastAll(new Vector3(bounds.center.x,bounds.min.y+.03f,bounds.center.z),Vector3.down,.15f,~0,QueryTriggerInteraction.Ignore)
                    .Where(h=>!h.collider.transform.IsChildOf(bench)).OrderBy(h=>h.distance).ToArray();
                bool grounded=hits.Length>0&&Mathf.Abs(hits[0].point.y-bounds.min.y)<.035f;
                Check("Bench clear and grounded at "+bench.position.ToString(),overlaps.Length==0&&grounded,new{overlaps,grounded});
                placements.Add(new{name=bench.name,position=bench.position.ToString(),bottom=bounds.min.y});
            }
            Check("All eight existing benches inspected",benches.Length==8);
            var s=TrainingSession.Instance;var nav=UnityEngine.Object.FindAnyObjectByType<NavigationManager>();
            Check("Assembly trigger and destination unchanged",Vector3.Distance(s.evacuation.assemblyZone.bounds.center,new Vector3(14.2f,1,-.2f))<.01f&&
                Vector3.Distance(s.evacuation.assemblyZone.bounds.size,new Vector3(4.8f,2,4.8f))<.01f&&Vector3.Distance(nav.assembly.position,new Vector3(14.2f,0,-.2f))<.01f);
            foreach(var target in new[]{new Vector3(20.5f,.05f,13.5f),new Vector3(13.25f,.05f,-8),new Vector3(13.25f,.05f,-18),new Vector3(31,.05f,-.2f),new Vector3(14.2f,.05f,-.2f)})
            {
                var path=new NavMeshPath();NavMeshHit a,b;
                bool valid=NavMesh.SamplePosition(new Vector3(7,.05f,-.2f),out a,.5f,NavMesh.AllAreas)&&NavMesh.SamplePosition(target,out b,.5f,NavMesh.AllAreas)&&NavMesh.CalculatePath(a.position,b.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
                Check("Connected pedestrian route to "+target,valid);
            }
            Check("No lantern occupies either playing surface",all.Where(t=>t.name=="Fluted lantern base collision").All(t=>!(t.position.x>9.5f&&t.position.x<31.5f&&t.position.z>7.5f&&t.position.z<19.5f)&&!(t.position.x>9.75f&&t.position.x<16.75f&&t.position.z>-20&&t.position.z<-6)));
            File.WriteAllText(Path.Combine(output,"CampusSports.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{checks,placements,passed},Newtonsoft.Json.Formatting.Indented));
            if(!passed)throw new Exception("Campus sports placement validation failed; inspect CampusSports.json.");
            s.MainMenu();var camera=s.player.view.GetComponent<Camera>();s.player.enabled=false;
            var pos=camera.transform.localPosition;var rot=camera.transform.localRotation;float fov=camera.fieldOfView;
            var positions=new[]{new Vector3(37,26,-27),new Vector3(18,5,-.2f),new Vector3(33,7,3.5f),new Vector3(8,5,-2.5f),new Vector3(22,3,18.5f)};
            var targets=new[]{new Vector3(15,0,0),new Vector3(7.28f,5.93f,-.2f),new Vector3(20.5f,1,13.5f),new Vector3(13.25f,1,-13),new Vector3(20,1,22.4f)};
            var names=new[]{"01-Complete-campus","02-University-name-board","03-Basketball-court","04-Volleyball-court","05-Seating-and-landscaping"};
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            var enabled=canvases.Select(c=>c.enabled).ToArray();
            try {
                foreach(var c in canvases)c.enabled=false;
                for(int i=0;i<names.Length;i++){
                    camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);camera.fieldOfView=i==1?48:65;
                    await Task.Delay(650);
                    ScreenCapture.CaptureScreenshot(Path.Combine(output,names[i]+".png"));
                    await Task.Delay(300);
                }
            } finally {camera.transform.localPosition=pos;camera.transform.localRotation=rot;camera.fieldOfView=fov;s.player.enabled=true;for(int i=0;i<canvases.Length;i++)canvases[i].enabled=enabled[i];}
        }
    }
}
