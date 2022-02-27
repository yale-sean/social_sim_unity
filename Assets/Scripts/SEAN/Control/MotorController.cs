// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Control
{
    public class MotorController : MonoBehaviour
    {
        public string Topic;


        public float maxTorque = 10.0f;
        public float P = 0.1f;
        public float I = 0.1f;
        public float D = 0.1f;
        public float F = 0.25f;
        public string wheelName;

        private WheelCollider wheelColl;
        private float targetVelocity = 0;

        private Transform wheelTransform;

        private float integral, lastError;

        private bool stopped = false;

        ROSConnection ros;


        void Start()
        {
            ROSConnection.instance.Subscribe<RosMessageTypes.Std.MFloat64>(Topic, ReceiveMessage);
            //base.Start();

            wheelTransform = transform.Find("base_link");
            if (wheelTransform != null)
            {
                wheelTransform = wheelTransform.Find("chassis_link").Find(wheelName);
            }
            else
            {
                //Debug.Log(wheelName + " not found in base_link/chassis_link/. Checking base_footprint/base_link/.");
                wheelTransform = transform.Find("base_footprint").Find("base_link").Find(wheelName);
            }
            if (wheelTransform == null)
            {
                Debug.Log(wheelName + " not found.");
            }
            else
            {
                wheelColl = wheelTransform.GetComponent<WheelCollider>();
            }



        }

        void ReceiveMessage(RosMessageTypes.Std.MFloat64 message)
        {
            //Debug.Log("AAAAAAAAAAAAA");
            targetVelocity = targetSpeed(message.data);
        }

        private void FixedUpdate()
        {
            ApplyLocalPositionToVisuals(wheelColl);

            //torkWheel.motorTorque = Pid(targetVelocity, torkWheel.velocity, Time.deltaTime);
            if (stopped || targetVelocity == 0.0f)
            {
                //Debug.Log(wheelName + " msg (braking): " + targetVelocity);
                wheelColl.brakeTorque = 10.0f;
                wheelColl.motorTorque = 0.0f;
            }
            else
            {
                wheelColl.brakeTorque = 0.0f;
                // diff_drive_controller output is in rad/s, compute wheel velocity in rad/sec as well
                float curSpeed = wheelColl.rpm / 60 * 2 * Mathf.PI;
                float torque = F * Pid(targetVelocity, curSpeed, Time.deltaTime);
                //Debug.Log(wheelName + "| torque: '" + torque + "' RPM: " + wheelColl.rpm + ", current vel: '" + curSpeed + "', target vel: '" + targetVelocity + "'");
                wheelColl.motorTorque = torque;
            }
        }

        private float targetSpeed(double targetVel)
        {
            if (Mathf.Approximately((float)targetVel, 0.0f))
            {
                return 0.0f;
            }
            return (float)targetVel;
        }

        public void ApplyLocalPositionToVisuals(WheelCollider collider)
        {
            if (collider.transform.childCount == 0)
            {
                return;
            }

            Transform visualWheel = collider.transform.GetChild(0);

            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }

        private float Pid(float setpoint, float actual, float timeFrame)
        {
            float present = setpoint - actual;
            integral += present * timeFrame;
            float deriv = (present - lastError) / timeFrame;
            lastError = present;
            return present * P + integral * I + deriv * D;
        }

        public void StopMotor(bool stoppedVal)
        {
            stopped = stoppedVal;
        }
    }
}