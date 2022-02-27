// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Control
{
    public class VelocityController : ControlSubscriber
    {
        private Rigidbody rb;

        private float targetLinVelocity, targetAngVelocity;
        public float maxTimeDeltaSec = 0.25f;
        private float lastMessageTS = 0;

        // PID Controller
        public float P = 1, I = 1, D = 1;
        private float integral, lastError;

        protected void Start()
        {
            base.Start();
            rb = sean.robot.base_link.GetComponent<Rigidbody>();
            // ROSConnection.instance.Subscribe<RosMessageTypes.Geometry.MTwist>(Topic, CmdVelMessage);
        }

        private void FixedUpdate()
        {
            //ApplyLocalPositionToVisuals(wheelColl);
            //// All in local (base_link) coorindates
            //if (targetLinVelocity == 0.0f) {
            //    rb.velocity = new Vector3(0,0,0);
            //} else {
            //    rb.AddRelativeForce(Pid(targetLinVelocity, rb.transform.forward * rb.velocity, Time.deltaTime));
            //}
            //if (targetAngVelocity == 0.0f) {
            //    rb.angularVelocity = new Vector3(0,0,0);
            //} else {
            //    rb.AddRelativeTorque(Pid(targetAngVelocity, rb.transform.forward * rb.angularVelocity, Time.deltaTime));
            //}

            if (Time.time - lastMessageTS > maxTimeDeltaSec)
            {
                targetAngVelocity = targetLinVelocity = 0;
            }

            if (targetAngVelocity == 0.0f)
            {
                rb.angularVelocity = new Vector3(0, 0, 0);
            }
            else
            {
                rb.angularVelocity = new Vector3(0, -1 * targetAngVelocity, 0);
            }
            if (targetLinVelocity == 0.0f)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            else
            {
                rb.velocity = rb.transform.forward * targetLinVelocity;
                // print("velocity: " + rb.velocity);
            }
            //print("velocity: " + rb.velocity);
        }

        override sealed protected void CmdVelMessage(RosMessageTypes.Geometry.MTwist msg)
        {
            // print("in callback message");
            if (msg == null) { return; }
            if (rb == null) { return; }
            targetLinVelocity = (float)msg.linear.x;
            targetAngVelocity = (float)msg.angular.z;
            lastMessageTS = Time.time;
        }

        private float Pid(float setpoint, float actual, float timeFrame)
        {
            float present = setpoint - actual;
            integral += present * timeFrame;
            float deriv = (present - lastError) / timeFrame;
            lastError = present;
            return present * P + integral * I + deriv * D;
        }
    }
}
