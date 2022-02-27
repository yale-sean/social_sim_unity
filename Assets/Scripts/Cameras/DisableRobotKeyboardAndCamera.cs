using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRobotKeyboardAndCamera : MonoBehaviour
{

    GameObject OverheadCamera;
    GameObject ThirdPersonCamera;
    GameObject ROSCamera;
    GameObject[] MainCameras;

    // Start is called before the first frame update
    void Start()
    {
        List<string> cameras = new List<string>() { "OverheadCamera", "ThirdPersonCamera", "ROSCameraRGB", "camera_rgb_frame" };
        foreach (string camera in cameras)
        {
            OverheadCamera = GameObject.Find(camera);
            if (OverheadCamera == null) { continue; }
            Camera cameraComponent = OverheadCamera.GetComponent<Camera>();
            if (cameraComponent == null) { continue; }
            cameraComponent.enabled = false;
            OverheadCamera.SetActive(false);
        }

        GameObject robotControl = GameObject.Find("RobotControl");
        //robotControl.GetComponent<KeyboardPublisher>().enabled = false;
        robotControl.GetComponent<GameDisplay>().enabled = false;
        GameObject ic = GameObject.Find("InstructionCanvas");
        if (ic != null)
        {
            ic.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {


    }
}
