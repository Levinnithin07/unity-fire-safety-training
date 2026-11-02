using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using FireSafety;

public static class PolishPhaseSeven
{
    static AudioClip CreateClip(string name, int variant, bool success)
    {
        const int rate = 22050;
        int count = Mathf.RoundToInt(rate * (success ? .9f : .24f));
        string path = "Assets/_FireSafety/Audio/" + name + ".wav";
        if (!File.Exists(path)) {
            var random = new System.Random(713 + variant);
            float low = 0;
            using (var w = new BinaryWriter(File.Create(path))) {
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + count * 2);
                w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); w.Write(16);
                w.Write((short)1); w.Write((short)1); w.Write(rate); w.Write(rate * 2);
                w.Write((short)2); w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(count * 2);
                for (int i = 0; i < count; i++) {
                    float t = i / (float)rate, value = 0;
                    if (success) {
                        float[] notes = {523.25f, 659.25f, 783.99f};
                        for (int n = 0; n < notes.Length; n++) {
                            float u = t - n * .14f;
                            if (u >= 0) value += .21f * Mathf.Sin(2 * Mathf.PI * notes[n] * u) * Mathf.Exp(-u * 7) * Mathf.Clamp01(u / .008f);
                        }
                    } else {
                        float noise = (float)random.NextDouble() * 2 - 1;
                        low = .80f * low + .20f * noise;
                        float impact = Mathf.Exp(-t * 30);
                        float toe = Mathf.Exp(-Mathf.Pow((t - .065f) / .024f, 2));
                        value = .30f * Mathf.Sin(2 * Mathf.PI * (85 + variant * 8) * t) * impact
                            + low * (1.6f * impact + .65f * toe);
                    }
                    value *= Mathf.Clamp01(i / 90f) * Mathf.Clamp01((count - 1 - i) / 300f);
                    w.Write((short)(Mathf.Clamp(value, -.75f, .75f) * 32767));
                }
            }
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (AudioImporter)AssetImporter.GetAtPath(path);
        importer.forceToMono = true;
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        importer.defaultSampleSettings = settings;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static AudioSource Source(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (!child) { child = new GameObject(name).transform; child.SetParent(parent, false); }
        var source = child.GetComponent<AudioSource>();
        if (!source) source = child.gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false; source.loop = false; source.spatialBlend = 0;
        source.dopplerLevel = 0; source.volume = 1; source.priority = 160;
        return source;
    }
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Configure only in Edit Mode.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/_FireSafety/Scenes/TrainingScene.unity") throw new Exception("Unexpected scene.");
        var session = UnityEngine.Object.FindAnyObjectByType<TrainingSession>();
        var feedback = session.player.GetComponent<TrainingFeedback>();
        if (!feedback) feedback = session.player.gameObject.AddComponent<TrainingFeedback>();
        feedback.session = session; feedback.player = session.player; feedback.extinguisher = session.evacuation.extinguisher;
        feedback.footsteps = Source(session.player.transform, "Footsteps - movement feedback");
        feedback.completion = Source(session.player.transform, "Extinguisher completion feedback");
        feedback.completion.priority = 96;
        feedback.footstepClips = Enumerable.Range(0,3).Select(i=>CreateClip("Footstep " + (i+1) + " - original",i,false)).ToArray();
        feedback.completionClip = CreateClip("Extinguisher success - original",0,true);
        // Keep clip references on the sources as well for Editor preview and audit.
        feedback.footsteps.clip = feedback.footstepClips[0]; feedback.completion.clip = feedback.completionClip;
        var quality = session.settings.GetComponent<TrainingEffectsQuality>();
        if (!quality) quality = session.settings.gameObject.AddComponent<TrainingEffectsQuality>();
        if (quality.budgets == null || quality.budgets.Length == 0)
            quality.budgets = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ParticleSystem>(true))
                .Select(p=>new TrainingEffectsQuality.Budget{particles=p, highLimit=p.main.maxParticles}).ToArray();
        session.settings.effectsQuality = quality;
        // Store the authored high limits. Settings applies the selected preset at runtime.
        quality.Apply(2);
        EditorUtility.SetDirty(feedback); EditorUtility.SetDirty(quality); EditorUtility.SetDirty(session.settings);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        var clips = feedback.footstepClips.Concat(new[]{feedback.completionClip}).Select(c=>new{c.name,c.channels,c.length,c.frequency,loadType=c.loadType.ToString()});
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=scene.path,clips,particleSystems=quality.budgets.Length,highLimit=quality.TotalParticleLimit},Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.GetFullPath("../Documentation/Phase7Configuration.json"),json);
        return json;
    }
}
