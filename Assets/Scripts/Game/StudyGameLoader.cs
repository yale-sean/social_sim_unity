using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StudyGameLoader : MonoBehaviour {

    private Dictionary<string, Scene> scenes;

    // Use this for initialization
    void Start()
    {
        //scenes = new Dictionary<string, Scene>();
        //Debug.Log("Scene count: "+ SceneManager.sceneCount);
        //Scene[] Scenes = new Scene[SceneManager.sceneCount];
        //for (int i = 0; i < SceneManager.sceneCount; i++) {
        //    Scene scene = SceneManager.GetSceneAt(i);
        //    scenes[scene.name] = scene;
        //    Debug.Log("Scene " + scene.name);
        //}
        //myLoadedAssetBundle = AssetBundle.LoadFromFile("Assets/AssetBundles/Scenes");
        //scenePaths = myLoadedAssetBundle.GetAllScenePaths();
        //Debug.Log("Scenes: " + scenePaths);
        LoadScene();
    }

    private void LoadScene() {
        string[] args = System.Environment.GetCommandLineArgs ();
        string scene = "";
        for (int i = 0; i < args.Length; i++) {
            Debug.Log ("ARG " + i + ": " + args [i]);
            if (args [i] == "-scene") {
                scene = args [i + 1];
                Debug.Log("Scene: " + scene);
                break;
            }
        }
        if (scene != "") {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        } else {
            Debug.Log("No scene specified");
        }
        //SceneManager.LoadScene(scenePaths[0], LoadSceneMode.Single);
    }
}