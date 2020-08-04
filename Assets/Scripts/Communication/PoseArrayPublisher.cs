using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class PoseArrayPublisher : UnityPublisher<MessageTypes.Geometry.PoseArray>
    {
        // Tag for objects that can be robot and agent spawn locations
        public string SpawnTag = "Spawn";

        private MessageTypes.Geometry.PoseArray message;
        private List<Transform> possiblePositions = new List<Transform>();

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void Update()
        {
            if (possiblePositions.Count != GameObject.FindGameObjectsWithTag(SpawnTag).Length) {
                UpdateMessage();
            }
            Publish(message);
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.PoseArray();
        }

        private void UpdateMessage()
        {
            possiblePositions.Clear();
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(SpawnTag)) {
                possiblePositions.Add(obj.transform);
            }

            InitializeMessage();
            message.header.stamp = new MessageTypes.Std.Time();
            message.poses = new MessageTypes.Geometry.Pose[possiblePositions.Count];

            int i = 0;
            foreach (Transform pose in possiblePositions)
            {
                MessageTypes.Geometry.Pose rosPose = new MessageTypes.Geometry.Pose();
                rosPose.position = GetGeometryPoint(pose.transform.position.Unity2Ros());
                rosPose.orientation = GetGeometryQuaternion(pose.transform.rotation.Unity2Ros());
                message.poses[i++] = rosPose;
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