using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
public static class WireTrainingUIInput
{
    public static string Run()
    {
        var module=Object.FindAnyObjectByType<InputSystemUIInputModule>();const string path="Assets/InputSystem_Actions.inputactions";
        var refs=AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>().Where(r=>r.action!=null).ToArray();
        InputActionReference Ref(string name)=>refs.First(r=>r.action.actionMap.name=="UI"&&r.action.name==name);
        module.actionsAsset=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);module.point=Ref("Point");module.leftClick=Ref("Click");module.rightClick=Ref("RightClick");module.middleClick=Ref("MiddleClick");module.move=Ref("Navigate");module.submit=Ref("Submit");module.cancel=Ref("Cancel");module.scrollWheel=Ref("ScrollWheel");module.trackedDevicePosition=Ref("TrackedDevicePosition");module.trackedDeviceOrientation=Ref("TrackedDeviceOrientation");
        EditorUtility.SetDirty(module);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(module.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(module.gameObject.scene);return "Persisted UI action asset and all ten imported action references.";
    }
}
