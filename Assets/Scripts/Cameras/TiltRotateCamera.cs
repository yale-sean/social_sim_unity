using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltRotateCamera : MonoBehaviour
{
    public bool enableTilt = true;
    public bool enableRotation = false;

    public float tiltSpeed = 30.0f;
    public float rotationSpeed = 30.0f;

    void FixedUpdate()
    {
        float tiltAngle = transform.eulerAngles.x;
        if (tiltAngle > 180.0f)
        {
            tiltAngle -= 360.0f;
        }
        
        if (enableTilt)
        {
            if (Input.GetKey(KeyCode.UpArrow) && tiltAngle > -75.0f)
            {
                transform.Rotate(Vector3.left * tiltSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.DownArrow) && tiltAngle < 45.0f)
            {
                transform.Rotate(Vector3.right * tiltSpeed * Time.deltaTime);
            }
        }

        if (enableRotation)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Rotate(Vector3.down * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
