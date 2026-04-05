using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaSetup
{
    [MenuItem("SlapArena/Create Base Arena Scene")]
    public static void CreateBaseArena()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";
        cameraObj.AddComponent<AudioListener>();
        cameraObj.transform.position = new Vector3(0, 10, -10);
        cameraObj.transform.rotation = Quaternion.Euler(45, 0, 0);

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        GameObject arenaRoot = new GameObject("Arena");

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.SetParent(arenaRoot.transform);
        platform.transform.position = new Vector3(0, -0.5f, 0);
        platform.transform.localScale = new Vector3(20, 1, 20);

        GameObject boundsTrigger = new GameObject("OOB_Trigger");
        boundsTrigger.transform.SetParent(arenaRoot.transform);
        boundsTrigger.transform.position = new Vector3(0, -5f, 0);
        boundsTrigger.transform.localScale = new Vector3(50, 1, 50);
        BoxCollider triggerCollider = boundsTrigger.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        string scenePath = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenePath))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        
        string fullPath = scenePath + "/PlatformArena.unity";
        EditorSceneManager.SaveScene(newScene, fullPath);
        Debug.Log("Arena Scene created at: " + fullPath);
    }
}
