/*
    Copyright (c) 2020, Members of Yale Interactive Machines Group, Yale University,
    Mohamed Hussein
    All rights reserved.
    This source code is licensed under the BSD-style license found in the
    LICENSE file in the root directory of this source tree. 
*/

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
//using RosSharp;

public class HeadPoseSubscriber : MonoBehaviour
{
    public Transform head;

    public float rotationStep = 0.7f;

    private float currTiltAngle;
    private float targetTiltAngle = 0.0f;

    private float minTiltAngle;
    private float maxTiltAngle;

    void Start()
    {
        //base.Start();
        //minTiltAngle = -head.GetComponent<HingeJointLimitsManager>().LargeAngleLimitMax;
        //maxTiltAngle = -head.GetComponent<HingeJointLimitsManager>().LargeAngleLimitMin;
    }

    private void FixedUpdate()
    {
        currTiltAngle = head.localEulerAngles.x;

        if (currTiltAngle > 180.0f)
            currTiltAngle -= 360.0f;

        targetTiltAngle = Mathf.Min(targetTiltAngle, maxTiltAngle);
        targetTiltAngle = Mathf.Max(targetTiltAngle, minTiltAngle);

        float newTiltAngle = Mathf.MoveTowards(currTiltAngle, targetTiltAngle, rotationStep);

        Vector3 newAngles = new Vector3(newTiltAngle,
                                        head.localEulerAngles.y,
                                        head.localEulerAngles.z);

        head.localEulerAngles = newAngles;
    }

    void ReceiveMessage(RosMessageTypes.Gizmo.MHeadPose headPose)
    {
        targetTiltAngle = headPose.tilt;
    }
}
