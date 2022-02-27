using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace SEAN.TF
{
    public class OdometryPublisher : MonoBehaviour
    {
        ROSConnection ros;
        public float publishMessageFrequency = 0.5f;
        private float timeElapsed;
        public string topicName;

        public Transform PublishedTransform;
        public string FrameId = "map";

        private RosMessageTypes.Nav.MOdometry message;

        private float previousRealTime;
        private Vector3 previousPosition = Vector3.zero;
        private Quaternion previousRotation = Quaternion.identity;

        private double[] identityMatrix = {1, 0, 0, 0, 0, 0,
                                          0, 1, 0, 0, 0, 0,
                                          0, 0, 1, 0, 0, 0,
                                          0, 0, 0, 1, 0, 0,
                                          0, 0, 0, 0, 1, 0,
                                          0, 0, 0, 0, 0, 1};

        void Start()
        {
            ros = ROSConnection.instance;
            InitializeMessage();

        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new RosMessageTypes.Nav.MOdometry();
            message.pose.covariance = identityMatrix;
            message.twist.covariance = identityMatrix;
            message.child_frame_id = FrameId;
        }

        private void UpdateMessage()
        {
            float deltaTime = Time.realtimeSinceStartup - previousRealTime;
            timeElapsed += Time.deltaTime;

            Vector3 linearVelocity = (PublishedTransform.position - previousPosition) / deltaTime;
            Vector3 angularVelocity = (PublishedTransform.rotation.eulerAngles - previousRotation.eulerAngles) / deltaTime;

            previousRealTime = Time.realtimeSinceStartup;
            previousPosition = PublishedTransform.position;
            previousRotation = PublishedTransform.rotation;

            if (timeElapsed <= publishMessageFrequency)
            {
                return;
            }
            SEAN.instance.clock.UpdateMHeader(message.header);
            message.twist.twist.linear = Util.Geometry.GetGeometryVector3(linearVelocity.To<FLU>());
            message.twist.twist.angular = Util.Geometry.GetGeometryVector3(-angularVelocity.To<FLU>());
            message.pose.pose.position = Util.Geometry.GetGeometryPoint(PublishedTransform.position.To<FLU>());
            message.pose.pose.orientation = Util.Geometry.GetGeometryQuaternion(PublishedTransform.rotation.To<FLU>());
            ros.Send(topicName, message);
            timeElapsed = 0;
        }
    }
}