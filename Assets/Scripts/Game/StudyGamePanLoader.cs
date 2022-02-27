using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyGamePanLoader : MonoBehaviour
{
    public Camera panCamera;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        panCamera.GetComponent<Camera>().transform.Rotate(0.0f, 0.005f, 0.0f, Space.Self);
        //camera.trainsform.rotation = Quaternion.Slerp(camera.transform.rotation, targetRotation, 3 * Time.deltaTime);
    }
}