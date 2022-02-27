using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCamIdx = -1;
    // Start is called before the first frame update
    void Start()
    {
        SwitchCam();
    }

    void SwitchCam()
    {
        currentCamIdx += 1;
        if (currentCamIdx >= cameras.Length)
        {
            currentCamIdx = 0;
        }
        print("Current Camera: " + currentCamIdx);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == currentCamIdx)
            {
                cameras[i].enabled = true;
            }
            else
            {
                cameras[i].enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCam();
        }

    }
}
