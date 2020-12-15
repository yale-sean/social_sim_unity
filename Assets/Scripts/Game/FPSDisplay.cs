using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{

    public Text instruction;

    public string msg = "Move the player to the yellow target";

    private bool debugUnity;

    // Start is called before the first frame update
    void Start()
    {
        string envUri = System.Environment.GetEnvironmentVariable("UNITY_DEBUG");
        debugUnity = !string.IsNullOrEmpty(envUri) && (envUri.ToLower() == "true" || envUri.ToLower() == "1");
    }

    // Update is called once per frame
    void Update()
    {
        if (debugUnity) {
            instruction.text = msg + " " + (int)(1.0f / Time.smoothDeltaTime) + "fps";
        }
    }
}
