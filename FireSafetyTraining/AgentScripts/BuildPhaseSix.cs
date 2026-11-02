using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using FireSafety;
public static class BuildPhaseSix
{
    public static string Run()
    {
        var scene=EditorSceneManager.GetActiveScene();if(EditorApplication.isPlaying||scene.isDirty||scene.path!="Assets/_FireSafety/Scenes/TrainingScene.unity")throw new Exception("Saved TrainingScene in Edit Mode required.");
        if(UnityEngine.Object.FindAnyObjectByType<TrainingSession>())throw new Exception("Phase 6 exists. Use targeted edits.");
        var m=UnityEngine.Object.FindAnyObjectByType<FireManager>();var e=UnityEngine.Object.FindAnyObjectByType<FireExtinguisherController>();var v=UnityEngine.Object.FindAnyObjectByType<EvacuationManager>();var old=UnityEngine.Object.FindAnyObjectByType<DesktopDisplaySettings>();
        var root=new GameObject("Phase 6 - training flow");Undo.RegisterCreatedObjectUndo(root,"Phase 6 training flow");var s=root.AddComponent<TrainingSession>();var evaluation=root.AddComponent<TrainingEvaluationManager>();var settings=root.AddComponent<TrainingSettings>();
        evaluation.simulation=m;evaluation.equipment=e;evaluation.evacuation=v;settings.player=m.player;settings.pipelines=new UnityEngine.Rendering.RenderPipelineAsset[3];
        var source=(UniversalRenderPipelineAsset)old.mediumPipeline;
        for(int i=0;i<3;i++){
            var a=UnityEngine.Object.Instantiate(source);a.name="Training "+new[]{"Low","Medium","High"}[i];
            a.renderScale=i==0?.7f:i==1?.85f:1;a.shadowDistance=i==0?12:i==1?20:30;a.msaaSampleCount=i==0?1:2;a.mainLightShadowmapResolution=i==0?512:i==1?1024:2048;
            AssetDatabase.CreateAsset(a,"Assets/_FireSafety/Materials/"+a.name+".asset");settings.pipelines[i]=a;
        }
        Undo.DestroyObjectImmediate(old);
        s.simulation=m;s.player=m.player;s.evacuation=v;s.evaluation=evaluation;s.settings=settings;s.gameplayCanvas=UnityEngine.Object.FindAnyObjectByType<EmergencyStatusUI>().GetComponent<Canvas>();
        var menu=new GameObject("Training menu",typeof(RectTransform));menu.transform.SetParent(root.transform,false);var ui=menu.AddComponent<TrainingInterface>();ui.session=s;ui.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");s.ui=ui;ui.Build();
        if(!UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>()){
            var es=new GameObject("Training EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));es.transform.SetParent(root.transform);es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
        }
        PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.defaultIsNativeResolution=true;PlayerSettings.resizableWindow=true;PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene.path,true)};
        EditorUtility.SetDirty(settings);EditorUtility.SetDirty(s);EditorUtility.SetDirty(ui);EditorUtility.SetDirty(evaluation);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Phase 6 persisted: eight menu pages, one EventSystem, evaluation/session/settings references, three URP quality assets, TrainingScene build entry.";
    }
}
