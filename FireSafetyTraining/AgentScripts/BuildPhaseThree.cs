using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using FireSafety;

public static class BuildPhaseThree
{
    const string Base = "Assets/_FireSafety/";
    static Material porcelain, chrome, brass, red, dark, indicator, flameMat, smokeMat, waterMat, emberMat;
    static TMP_FontAsset font;
    static Transform root;
    static Mesh Lathe(string name, Vector2[] profile, int segments = 40)
    {
        var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
        for (int j = 0; j < profile.Length; j++) for (int i = 0; i <= segments; i++) {
            float a = i * Mathf.PI * 2 / segments;
            vertices.Add(new Vector3(Mathf.Cos(a) * profile[j].x, profile[j].y, Mathf.Sin(a) * profile[j].x));
            uv.Add(new Vector2(i / (float)segments, j / (float)(profile.Length - 1)));
            if (j < profile.Length - 1 && i < segments) {
                int n = j * (segments + 1) + i;
                triangles.AddRange(new[] { n, n + segments + 1, n + 1, n + 1, n + segments + 1, n + segments + 2 });
            }
        }
        var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, Base + "Models/" + name + ".asset"); return mesh;
    }
    static Mesh BoxMesh(string name, Vector3 size)
    {
        // Reusable hard-surface detail, not replacement architecture.
        var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mesh = UnityEngine.Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh); mesh.name = name;
        var v = mesh.vertices; for (int i = 0; i < v.Length; i++) v[i] = Vector3.Scale(v[i], size);
        mesh.vertices = v; mesh.RecalculateBounds(); UnityEngine.Object.DestroyImmediate(temp);
        AssetDatabase.CreateAsset(mesh, Base + "Models/" + name + ".asset"); return mesh;
    }
    static GameObject Part(string name, Transform parent, Mesh mesh, Material material, Vector3 pos = default)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
        go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material; return go;
    }
    static Material Solid(string name, Color color, float metallic = 0, float smoothness = .45f, bool emission = false)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        mat.SetColor("_BaseColor", color); mat.SetFloat("_Metallic", metallic); mat.SetFloat("_Smoothness", smoothness);
        if (emission) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Color.black); }
        AssetDatabase.CreateAsset(mat, Base + "Materials/" + name + ".mat"); return mat;
    }
    static Material ParticleMaterial(string name, string texture, bool additive)
    {
        string path = "Assets/ThirdParty/KenneyParticles/" + texture + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.alphaIsTransparency = true; importer.mipmapEnabled = true; importer.maxTextureSize = 512;
        importer.textureCompression = TextureImporterCompression.Compressed; importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = name };
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path)); mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Surface", 1); mat.SetFloat("_Blend", additive ? 2 : 0);
        mat.SetFloat("_SrcBlend", (int)BlendMode.SrcAlpha); mat.SetFloat("_DstBlend", (int)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
        mat.SetFloat("_ZWrite", 0); mat.SetFloat("_Cull", 0); mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent"); mat.renderQueue = 3000;
        AssetDatabase.CreateAsset(mat, Base + "Materials/" + name + ".mat"); return mat;
    }
    static ParticleSystem Particles(string name, Transform parent, Material mat, int budget, float lifetime, float speed, float size, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main; main.playOnAwake = false; main.loop = true; main.duration = 6;
        main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = budget;
        main.startLifetime = lifetime; main.startSpeed = speed; main.startSize = size; main.startColor = color;
        var emission = ps.emission; emission.rateOverTime = 0;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 10; shape.radius = .12f;
        go.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        var fade = ps.colorOverLifetime; fade.enabled = true;
        var gradient = new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
            new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(.75f,.55f),new GradientAlphaKey(0,1)});
        fade.color = gradient;
        return ps;
    }
    static AudioClip Audio(string name, string kind, int seconds)
    {
        const int sampleRate = 22050; int count = seconds * sampleRate;
        var random = new System.Random(421); float low = 0, pop = 0;
        var path = Base + "Audio/" + name + ".wav";
        using (var writer = new BinaryWriter(File.Create(path))) {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(sampleRate); writer.Write(sampleRate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
            for (int i = 0; i < count; i++) {
                float t = i / (float)sampleRate, noise = (float)random.NextDouble() * 2 - 1; low = low * .96f + noise * .04f;
                float value;
                if (kind == "alarm") {
                    float beat = t % 4; bool on = beat < .45f || (beat > .65f && beat < 1.1f) || (beat > 1.3f && beat < 1.75f);
                    value = on ? .26f * Mathf.Sin(2 * Mathf.PI * 880 * t) + .06f * Mathf.Sin(2 * Mathf.PI * 1760 * t) : 0;
                    float local = beat < .65f ? beat : beat < 1.3f ? beat - .65f : beat - 1.3f;
                    value *= Mathf.Clamp01(Mathf.Min(local, .45f - local) * 150);
                } else if (kind == "fire") {
                    if (random.NextDouble() < .0004) pop = .7f;
                    pop *= .994f; value = low * 1.6f + noise * pop;
                } else value = noise * .20f + low * .7f;
                float edge = Mathf.Min(1, Mathf.Min(t, seconds - t) * 50);
                writer.Write((short)(Mathf.Clamp(value * edge, -.95f, .95f) * 32767));
            }
        }
        AssetDatabase.ImportAsset(path);
        var importer = (AudioImporter)AssetImporter.GetAtPath(path); importer.forceToMono = true;
        var settings = importer.defaultSampleSettings; settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .65f; importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static AudioSource Sound(Transform parent, AudioClip clip, float volume, float spatial = 1)
    {
        var audio = parent.gameObject.AddComponent<AudioSource>(); audio.clip = clip; audio.loop = true; audio.playOnAwake = false;
        audio.volume = volume; audio.spatialBlend = spatial; audio.minDistance = 1.5f; audio.maxDistance = 15; audio.rolloffMode = AudioRolloffMode.Logarithmic;
        return audio;
    }
    static TMP_Text Label(string name, Transform parent, string text, int size, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var label = go.AddComponent<TextMeshProUGUI>(); label.font = font; label.text = text; label.fontSize = size;
        label.enableAutoSizing = false; label.color = color; label.raycastTarget = false; label.textWrappingMode = TextWrappingModes.Normal;
        label.alignment = TextAlignmentOptions.MidlineLeft; return label;
    }
    public static string Run()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode before building.");
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != Base + "Scenes/TrainingScene.unity" || scene.isDirty) throw new Exception("TrainingScene must be active and saved.");
        if (GameObject.Find("Phase 3 - emergency systems")) throw new Exception("Phase 3 already exists. Apply targeted changes instead.");
        foreach (string folder in new[]{"Materials","Models","Audio","Prefabs","UI"}) Directory.CreateDirectory(Base + folder);
        AssetDatabase.Refresh();
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (!font) throw new Exception("Import TMP essentials before building.");
        root = new GameObject("Phase 3 - emergency systems").transform; Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Phase 3");
        porcelain = Solid("Equipment - warm porcelain", new Color(.86f,.85f,.79f),.05f,.42f);
        chrome = Solid("Equipment - brushed chrome", new Color(.65f,.70f,.74f),.85f,.72f);
        brass = Solid("Equipment - brass", new Color(.65f,.42f,.13f),.78f,.6f);
        red = Solid("Equipment - red glass", new Color(.55f,.015f,.009f),.25f,.88f,true);
        dark = Solid("Equipment - graphite vents", new Color(.035f,.04f,.045f),.2f,.35f);
        indicator = Solid("Equipment - status LED",Color.white,.1f,.6f,true);
        flameMat = ParticleMaterial("Fire - flame", "flame_01", false); smokeMat = ParticleMaterial("Fire - smoke", "smoke_04", false);
        flameMat.SetColor("_BaseColor",new Color(3,3,3,1));
        emberMat = ParticleMaterial("Fire - ember", "spark_01", true); waterMat = ParticleMaterial("Sprinkler - droplets", "circle_05", false);
        var crackle = Audio("Fire crackle - original", "fire", 8); var alarmClip = Audio("Alarm signal - original", "alarm", 4); var waterClip = Audio("Water spray - original", "water", 6);
        var manager = root.gameObject.AddComponent<FireManager>(); manager.player = UnityEngine.Object.FindAnyObjectByType<DesktopPlayerController>();
        var fires = new List<FireSource>();
        Vector3[] positions = {new Vector3(-1.951f,.805f,.068f), new Vector3(-1.860f,.805f,.880f), new Vector3(-1.925f,.805f,1.728f)};
        for (int i = 0; i < positions.Length; i++) {
            var go = new GameObject("Combustible desk " + (i+1)); go.transform.SetParent(root); go.transform.position = positions[i];
            var fire = go.AddComponent<FireSource>(); fires.Add(fire);
            fire.flames = Particles("Flame tongues", go.transform, flameMat, 70, .85f, .95f, .32f, new Color(1,.32f,.025f,1));
            var flameMain = fire.flames.main; flameMain.startLifetime = new ParticleSystem.MinMaxCurve(.45f,1.1f);
            var flameNoise = fire.flames.noise; flameNoise.enabled = true; flameNoise.strength = .16f; flameNoise.frequency = 1.5f; flameNoise.quality = ParticleSystemNoiseQuality.Low;
            var flameSize = fire.flames.sizeOverLifetime; flameSize.enabled = true; flameSize.size = new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.55f),new Keyframe(.3f,1),new Keyframe(1,.12f)));
            fire.smoke = Particles("Rising smoke", go.transform, smokeMat, 90, 8, .45f, .8f, new Color(.16f,.15f,.14f,.9f));
            var smokeSize = fire.smoke.sizeOverLifetime; smokeSize.enabled = true; smokeSize.size = new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.45f),new Keyframe(.5f,1.6f),new Keyframe(1,2.1f)));
            var smokeMain = fire.smoke.main; smokeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI,Mathf.PI);
            var smokeNoise = fire.smoke.noise; smokeNoise.enabled = true; smokeNoise.strength = .10f; smokeNoise.frequency = .5f; smokeNoise.quality = ParticleSystemNoiseQuality.Low;
            fire.embers = Particles("Embers", go.transform, emberMat, 16, 1.3f, 1.1f, .026f, new Color(1,.28f,.02f,1));
            fire.fireLight = new GameObject("Fire flicker").AddComponent<Light>(); fire.fireLight.transform.SetParent(go.transform,false); fire.fireLight.transform.localPosition = Vector3.up*.3f;
            fire.fireLight.type = LightType.Point; fire.fireLight.range = 3; fire.fireLight.color = new Color(1,.29f,.035f); fire.fireLight.shadows = LightShadows.None; fire.fireLight.enabled = false;
            fire.crackle = Sound(go.transform,crackle,0);
        }
        for (int i=0;i<fires.Count;i++) fires[i].spreadTargets = i+1<fires.Count ? new[]{fires[i+1]} : Array.Empty<FireSource>();
        manager.sources = fires.ToArray();
        var bell = new GameObject("Automatic alarm sounder and beacon"); bell.transform.SetParent(root); bell.transform.position = new Vector3(3.51f,2.40f,-.70f);
        bell.transform.rotation = Quaternion.Euler(0,0,-90);
        var bellMesh = Lathe("Alarm sounder shell",new[]{new Vector2(0,0),new Vector2(.09f,0),new Vector2(.10f,.015f),new Vector2(.10f,.06f),new Vector2(.085f,.075f),new Vector2(0,.075f)});
        Part("Red sounder casing",bell.transform,bellMesh,red);
        var lensMesh = Lathe("Alarm lens",new[]{new Vector2(0,0),new Vector2(.06f,0),new Vector2(.06f,.025f),new Vector2(.045f,.05f),new Vector2(0,.06f)});
        var lens = Part("Beacon lens",bell.transform,lensMesh,indicator,new Vector3(0,.07f,0));
        var alarm = bell.AddComponent<FireAlarmController>(); manager.alarm = alarm; alarm.lens = lens.GetComponent<Renderer>(); alarm.sounder = Sound(bell.transform,alarmClip,.4f,.35f);
        alarm.beacon = new GameObject("Alarm flash").AddComponent<Light>(); alarm.beacon.transform.SetParent(bell.transform,false); alarm.beacon.transform.localPosition = new Vector3(0,.17f,0);
        alarm.beacon.type = LightType.Point; alarm.beacon.color = new Color(1,.08f,.03f); alarm.beacon.range = 2.2f; alarm.beacon.intensity = 1; alarm.beacon.enabled = false;
        var detectorMesh = Lathe("Detector sculpted housing",new[]{new Vector2(0,0),new Vector2(.072f,0),new Vector2(.085f,.008f),new Vector2(.085f,.022f),new Vector2(.080f,.027f),new Vector2(.080f,.043f),new Vector2(.065f,.058f),new Vector2(.045f,.064f),new Vector2(0,.064f)});
        var vent = BoxMesh("Detector ventilation slot", new Vector3(.004f,.014f,.005f));
        var led = Lathe("Detector LED lens",new[]{new Vector2(0,0),new Vector2(.0035f,0),new Vector2(.0035f,.003f),new Vector2(0,.004f)},16);
        var detectors = new List<SmokeDetector>();
        for(int i=0;i<2;i++) {
            var go = new GameObject(i==0?"Classroom optical smoke detector":"Passage optical smoke detector"); go.transform.SetParent(root);
            go.transform.position=i==0?new Vector3(-.5f,2.89f,.65f):new Vector3(4.6f,2.89f,0); go.transform.rotation=Quaternion.Euler(180,0,0);
            Part("Vented housing",go.transform,detectorMesh,porcelain);
            for(int v=0;v<36;v++){float a=v*Mathf.PI*2/36;var slot=Part("Optical chamber vent",go.transform,vent,dark,new Vector3(Mathf.Cos(a)*.080f,.034f,Mathf.Sin(a)*.080f));slot.transform.localRotation=Quaternion.Euler(0,-v*10,0);}
            var light=Part("Status LED",go.transform,led,indicator,new Vector3(.035f,.060f,0));
            var detector=go.AddComponent<SmokeDetector>();detector.indicator=light.GetComponent<Renderer>();detector.alarm=alarm;detector.inPassage=i==1;detectors.Add(detector);
            PrefabUtility.SaveAsPrefabAsset(go,Base+"Prefabs/"+(i==0?"ClassroomSmokeDetector":"PassageSmokeDetector")+".prefab");
        }
        manager.detectors=detectors.ToArray();
        var escutcheon=Lathe("Sprinkler escutcheon",new[]{new Vector2(0,0),new Vector2(.045f,0),new Vector2(.047f,.005f),new Vector2(.035f,.012f),new Vector2(.014f,.014f),new Vector2(.014f,.036f),new Vector2(0,.036f)});
        var armMesh=BoxMesh("Sprinkler yoke arm",new Vector3(.007f,.060f,.008f));
        var deflector=Lathe("Sprinkler deflector",new[]{new Vector2(0,0),new Vector2(.026f,0),new Vector2(.026f,.003f),new Vector2(.018f,.006f),new Vector2(0,.006f)},24);
        var bulb=Lathe("Sprinkler glass bulb",new[]{new Vector2(0,0),new Vector2(.002f,0),new Vector2(.004f,.008f),new Vector2(.004f,.034f),new Vector2(.002f,.042f),new Vector2(0,.042f)},16);
        var sprinklers = new List<SprinklerController>();
        for(int i=0;i<2;i++) {
            var go=new GameObject(i==0?"Sprinkler A - west desks":"Sprinkler B - east desks");go.transform.SetParent(root);
            go.transform.position=i==0?new Vector3(-1.5f,2.89f,.8f):new Vector3(1.8f,2.89f,2.2f);
            var model=new GameObject("Pendent sprinkler assembly");model.transform.SetParent(go.transform,false);model.transform.localRotation=Quaternion.Euler(180,0,0);
            Part("Chrome escutcheon and threaded neck",model.transform,escutcheon,chrome);
            Part("Brass yoke left",model.transform,armMesh,brass,new Vector3(-.017f,.063f,0));Part("Brass yoke right",model.transform,armMesh,brass,new Vector3(.017f,.063f,0));
            Part("Water deflector",model.transform,deflector,brass,new Vector3(0,.093f,0));
            var bulbObject=Part("Heat-sensitive bulb",model.transform,bulb,red,new Vector3(0,.038f,0));
            var s=go.AddComponent<SprinklerController>();s.glassBulb=bulbObject;
            s.water=Particles("Radial water spray",go.transform,waterMat,280,.65f,5,.018f,new Color(.70f,.84f,.92f,.38f));
            s.water.transform.localPosition=new Vector3(0,-.11f,0);s.water.transform.localRotation=Quaternion.Euler(90,0,0);
            var waterMain=s.water.main;waterMain.gravityModifier=.6f;waterMain.startSpeed=new ParticleSystem.MinMaxCurve(4.7f,5.5f);
            var shape=s.water.shape;shape.angle=36;shape.radius=.018f;
            var emission=s.water.emission;emission.rateOverTime=330;
            var waterRenderer=s.water.GetComponent<ParticleSystemRenderer>();waterRenderer.renderMode=ParticleSystemRenderMode.Stretch;waterRenderer.lengthScale=2;waterRenderer.velocityScale=.015f;
            s.waterSound=Sound(go.transform,waterClip,.24f);sprinklers.Add(s);
            PrefabUtility.SaveAsPrefabAsset(go,Base+"Prefabs/"+(i==0?"SprinklerWest":"SprinklerEast")+".prefab");
        }
        manager.sprinklers=sprinklers.ToArray();
        var canvasObject=new GameObject("Emergency status HUD",typeof(RectTransform));canvasObject.transform.SetParent(root,false);
        var canvas=canvasObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var panelObject=new GameObject("Status card",typeof(RectTransform));panelObject.transform.SetParent(canvasObject.transform,false);
        var rect=(RectTransform)panelObject.transform;rect.anchorMin=new Vector2(.025f,.78f);rect.anchorMax=new Vector2(.51f,.96f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        var bg=panelObject.AddComponent<UnityEngine.UI.Image>();bg.color=new Color(.025f,.045f,.055f,.90f);bg.raycastTarget=false;
        var accentObject=new GameObject("State accent",typeof(RectTransform));accentObject.transform.SetParent(panelObject.transform,false);
        var ar=(RectTransform)accentObject.transform;ar.anchorMin=Vector2.zero;ar.anchorMax=new Vector2(.006f,1);ar.offsetMin=ar.offsetMax=Vector2.zero;
        var accent=accentObject.AddComponent<UnityEngine.UI.Image>();accent.raycastTarget=false;
        var ui=canvasObject.AddComponent<EmergencyStatusUI>();ui.simulation=manager;ui.accent=accent;
        ui.heading=Label("State",rect,"CLASSROOM / EMERGENCY SYSTEMS",26,new Vector2(.025f,.69f),new Vector2(.98f,.94f),Color.white);
        ui.detail=Label("Guidance",rect,"Press ENTER to begin",23,new Vector2(.025f,.34f),new Vector2(.98f,.69f),new Color(.9f,.94f,.96f));
        ui.instruments=Label("System readings",rect,"PHASE 03",19,new Vector2(.025f,.05f),new Vector2(.98f,.34f),new Color(.65f,.76f,.80f));
        ui.controls=Label("Controls and notice",canvasObject.transform,"ENTER Start / R Reset",20,new Vector2(.025f,.025f),new Vector2(.78f,.085f),Color.white);
        var cp=ui.controls.gameObject.AddComponent<UnityEngine.UI.Shadow>();cp.effectColor=new Color(0,0,0,.9f);cp.effectDistance=new Vector2(1,-1);
        manager.ResetScenario();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Phase 3 saved: 3 combustible desks, 2 detectors, 2 independent sprinklers, alarm sounder, authored hardware, CC0 effects and responsive HUD.";
    }
}
