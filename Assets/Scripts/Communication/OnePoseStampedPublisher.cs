using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class OnePoseStampedPublisher : UnityPublisher<MessageTypes.Geometry.PoseStamped>
    {
        public string FrameId = "Unity";
        public int SendOnceDelaySec = 5;

        private Transform publishedTransform;
        private MessageTypes.Geometry.PoseStamped message;
        private bool isInfoUpdated;

        private int fixedUpdateDelay;
        private int currentFixedUpdateDelay;


        protected override void Start()
        {
            base.Start();

            // Fixed update is usually at 50 calls/sec, so wait 5 sec before calling
            fixedUpdateDelay = 50*SendOnceDelaySec;
            currentFixedUpdateDelay = fixedUpdateDelay;

            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.PoseStamped
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                }
            };
        }

        private void UpdateMessage()
        {
            if (!isInfoUpdated) {
                return;
            }
            if (currentFixedUpdateDelay > 0) {
                currentFixedUpdateDelay -= 1;
                return;
            }

            message.header.Update();
            message.pose.position = GetGeometryPoint(publishedTransform.position.Unity2Ros());
            message.pose.orientation = GetGeometryQuaternion(publishedTransform.rotation.Unity2Ros());

            Publish(message);

            isInfoUpdated = false;
            currentFixedUpdateDelay = fixedUpdateDelay;
        }

        public void SendOnce(Transform t) {
            publishedTransform = t;
            isInfoUpdated = true;
            Debug.Log("SendOnce set transform: " + publishedTransform + ", isInfoUpdated: " + isInfoUpdated);
        }

        private MessageTypes.Geometry.Point GetGeometryPoint(Vector3 position)
        {
            MessageTypes.Geometry.Point geometryPoint = new MessageTypes.Geometry.Point();
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = position.z;
            return geometryPoint;
        }

        private MessageTypes.Geometry.Quaternion GetGeometryQuaternion(Quaternion quaternion)
        {
            MessageTypes.Geometry.Quaternion geometryQuaternion = new MessageTypes.Geometry.Quaternion();
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
            return geometryQuaternion;
        }

    }
}