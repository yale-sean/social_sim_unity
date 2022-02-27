using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceOptions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        bool enableLogs = true;
        string[] args = System.Environment.GetCommandLineArgs ();
        for (int i = 0; i < args.Length; i++)
        {
            if (args [i] == "-enablelogs")
            {
                enableLogs = args [i + 1] == "1";
                Debug.Log("enableLogs: " + enableLogs);
            }
        }
        Debug.unityLogger.logEnabled = enableLogs;
#endif
    }
}
