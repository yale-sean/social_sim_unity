using UnityEngine;

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

        private uint numPeds;
        private double timeLimit;

        private bool isMessageReceived;

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
            numPeds = message.num_peds;
            timeLimit = message.time_limit;
            isMessageReceived = true;
        }

        private void StartTrial()
        {
            GetComponent<TrialStatusPublisher>().StartTrial(robotPosition, robotRotation,
                                                            targetPosition, targetRotation,
                                                            numPeds, timeLimit);
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