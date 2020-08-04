using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{
    public class TrialStartSubscriber : UnitySubscriber<MessageTypes.SocialSimRos.TrialStart>
    {
        private ulong stamp;
        private ulong prevStamp;
        private Vector3 robotPosition;
        private Quaternion robotRotation;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private List<Vector3> peoplePositions;
        private List<Quaternion> peopleRotations;

        private double timeLimit;

        private bool isMessageReceived;

        void Awake() {
            peoplePositions = new List<Vector3>();
            peopleRotations = new List<Quaternion>();
        }

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived && prevStamp != stamp)
            {
                prevStamp = stamp;
                StartTrial();
            }
        }

        protected override void ReceiveMessage(MessageTypes.SocialSimRos.TrialStart message)
        {
            stamp = message.header.stamp.secs;
            robotPosition = GetPosition(message.spawn).Ros2Unity();
            robotRotation = GetRotation(message.spawn).Ros2Unity();
            targetPosition = GetPosition(message.target).Ros2Unity();
            targetRotation = GetRotation(message.target).Ros2Unity();
            if (message.people.poses.Length <= 0) {
                Debug.LogError("People positions are empty, cannot start");
            }
            peoplePositions.Clear();
            peopleRotations.Clear();
            foreach (MessageTypes.Geometry.Pose pose in message.people.poses) {
                peoplePositions.Add(GetPosition(pose).Ros2Unity());
                peopleRotations.Add(GetRotation(pose).Ros2Unity());
            }
            timeLimit = message.time_limit;
            isMessageReceived = true;
        }

        private void StartTrial()
        {
            Debug.Log("Starting Trial");
            GetComponent<TrialStatusPublisher>().StartTrial(robotPosition, robotRotation,
                                                            targetPosition, targetRotation,
                                                            peoplePositions, peopleRotations,
                                                            timeLimit);
        }

        private Vector3 GetPosition(MessageTypes.Geometry.Pose message)
        {
            return new Vector3(
                (float)message.position.x,
                (float)message.position.y,
                (float)message.position.z);
        }

        private Quaternion GetRotation(MessageTypes.Geometry.Pose message)
        {
            return new Quaternion(
                (float)message.orientation.x,
                (float)message.orientation.y,
                (float)message.orientation.z,
                (float)message.orientation.w);
        }
    }
}