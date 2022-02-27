using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StudyGameLoader : MonoBehaviour
{

    private Dictionary<string, Scene> scenes;

    void Start()
    {
        LoadScene();
    }

    private void LoadScene()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string scene = "";
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("ARG " + i + ": " + args[i]);
            if (args[i] == "-scene")
            {
                scene = args[i + 1];
                Debug.Log("Scene: " + scene);
                break;
            }
        }
        if (scene != "")
        {
            SceneManager.LoadScene("Scenes/" + scene, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("No scene specified");
        }
    }
}