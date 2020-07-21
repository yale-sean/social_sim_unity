using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class MotorController : UnitySubscriber<MessageTypes.Std.Float64>
    {
        public float maxTorque = 10.0f;
        public float P = 100.0f;
        public string wheelName;

        private WheelCollider wheelColl;
        private double targetVelocity;
        private bool isMessageReceived;

        protected override void Start()
        {
            base.Start();
            wheelColl = transform.Find("base_link").Find("chassis_link").Find(wheelName).GetComponent<WheelCollider>();
        }

        protected override void ReceiveMessage(MessageTypes.Std.Float64 message)
        {
            targetVelocity = message.data;
            isMessageReceived = true;
        }


        private void Update()
        {
            if (isMessageReceived) {
                ProcessMessage();
            }
        }

        private void FixedUpdate()
        {
            ApplyLocalPositionToVisuals(wheelColl);
        }

        private void ProcessMessage()
        {
            //wheelColl.brakeTorque = 0.3f;
            if (Mathf.Approximately((float)targetVelocity, 0.0f)) {
                //Debug.Log(wheelName + " msg (braking): " + targetVelocity);
                wheelColl.brakeTorque = 0.1f;
                wheelColl.motorTorque = 0.0f;
            } else {
                wheelColl.brakeTorque = 0.0f;
                wheelColl.motorTorque = pControl(targetSpeed(targetVelocity));
                //Debug.Log(wheelName + ": " + wheelColl.motorTorque + "       RPM: " + wheelColl.rpm);
            }
            isMessageReceived = false;
        }

        private float targetSpeed(double targetVel) {
            if (Mathf.Approximately((float)targetVel, 0.0f)) {
                return 0.0f;
            }
            return (float) targetVel;
        }

        float currentSpeed() {
            return wheelColl.rpm * 2 * Mathf.PI / 60 * wheelColl.radius;
        }

        private float pControl(float targetVel) {
            return Mathf.Clamp((targetVel - currentSpeed()) * P, -maxTorque, maxTorque);
        }

        public void ApplyLocalPositionToVisuals(WheelCollider collider)
        {
            if (collider.transform.childCount == 0) {
                return;
            }
         
            Transform visualWheel = collider.transform.GetChild(0);
         
            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);
         
            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }
    }
}