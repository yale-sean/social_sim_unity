using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class PoseArrayPublisher : UnityPublisher<MessageTypes.SocialSimRos.PoseArray>
    {
        private MessageTypes.SocialSimRos.PoseArray message;
        private Transform[] possiblePositions = new Transform[0];

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void Update()
        {
            if (this.gameObject.transform.childCount > possiblePositions.Length)
            {
                UpdateMessage();
            }
            else
            {
                Publish(message);
            }
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.SocialSimRos.PoseArray();
        }

        private void UpdateMessage()
        {
            possiblePositions = GetComponentsInChildren<Transform>();
            InitializeMessage();
            message.positions = new MessageTypes.Geometry.Pose[possiblePositions.Length];

            int i = 0;
            foreach (Transform pose in possiblePositions)
            {
                MessageTypes.Geometry.Pose rosPose = new MessageTypes.Geometry.Pose();
                rosPose.position = GetGeometryPoint(pose.transform.position.Unity2Ros());
                rosPose.orientation = GetGeometryQuaternion(pose.transform.rotation.Unity2Ros());
                message.positions[i++] = rosPose;
            }

            Publish(message);
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