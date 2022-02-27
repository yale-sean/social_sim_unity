// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

// This code is based off of "Simple Car Controller in Unity Tutorial" on Youtube.
// Link: https://www.youtube.com/watch?v=Z4HA8zJhGEk

public class KuriKeyboardController : MonoBehaviour
{
    private bool forward;
    private bool left;
    private bool right;
    private bool reverse;

    // Make these fields modifiable in Unity GUI.
    [SerializeField] private float motorTorque;
    [SerializeField] private WheelCollider leftWheelColl;
    [SerializeField] private WheelCollider rightWheelColl;

    // FixedUpdate uses Unity's physics engine. Thus no need for the start and loop functions.
    private void FixedUpdate()
    {
        // Get keyboard input
        GetInput();

        // Give motor commands in Unity.
        HandleMotor();
    }

    // Handle motor commands. Different cases for different keyboard combinations. 
    // The setup is
    //    I   (forward)
    //  J   L (left/right)
    //    K   (reverse)
    private void HandleMotor()
    {
        if (forward && right)
        {
            leftWheelColl.brakeTorque = 0.0f;
            rightWheelColl.brakeTorque = 0.0f;
            leftWheelColl.motorTorque = motorTorque;
            rightWheelColl.motorTorque = motorTorque / 2;
        }
        else if (forward && left)
        {
            leftWheelColl.brakeTorque = 0.0f;
            rightWheelColl.brakeTorque = 0.0f;
            leftWheelColl.motorTorque = motorTorque / 2;
            rightWheelColl.motorTorque = motorTorque;
        }
        else if (forward/*i is pressed*/)
        {
            leftWheelColl.brakeTorque = 0.0f;
            rightWheelColl.brakeTorque = 0.0f;
            leftWheelColl.motorTorque = motorTorque;
            rightWheelColl.motorTorque = motorTorque;
        }
        else if (left/*j is pressed*/)
        {
            leftWheelColl.brakeTorque = 0.0f;
            rightWheelColl.brakeTorque = 0.0f;
            leftWheelColl.motorTorque = -1.0f * motorTorque;
            rightWheelColl.motorTorque = motorTorque;
        }
        else if (right/*l is pressed*/)
        {
            leftWheelColl.brakeTorque = 0.0f;
            rightWheelColl.brakeTorque = 0.0f;
            leftWheelColl.motorTorque = motorTorque;
            rightWheelColl.motorTorque = -1.0f * motorTorque;
        }
        else if (reverse/*k is pressed*/)
        {
            leftWheelColl.brakeTorque = 0.0f;
            rightWheelColl.brakeTorque = 0.0f;
            leftWheelColl.motorTorque = -1.0f * motorTorque;
            rightWheelColl.motorTorque = -1.0f * motorTorque;
        }
        else //nothing pressed
        {
            leftWheelColl.motorTorque = 0.0f;
            rightWheelColl.motorTorque = 0.0f;
            leftWheelColl.brakeTorque = motorTorque;
            rightWheelColl.brakeTorque = motorTorque;
        }
    }

    // Get input from the keyboard.
    private void GetInput()
    {
        forward = Input.GetKey(KeyCode.I);
        left = Input.GetKey(KeyCode.J);
        right = Input.GetKey(KeyCode.L);
        reverse = Input.GetKey(KeyCode.K);
    }
}