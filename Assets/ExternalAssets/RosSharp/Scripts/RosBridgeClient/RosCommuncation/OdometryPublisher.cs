using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class OdometryPublisher : UnityPublisher<MessageTypes.Nav.Odometry>
    {
        public Transform PublishedTransform;
        public string FrameId = "Unity";

        private MessageTypes.Nav.Odometry message;

        private float previousRealTime;
        private Vector3 previousPosition = Vector3.zero;
        private Quaternion previousRotation = Quaternion.identity;

        private double[] identityMatrix = {1, 0, 0, 0, 0, 0,
                                           0, 1, 0, 0, 0, 0,
                                           0, 0, 1, 0, 0, 0,
                                           0, 0, 0, 1, 0, 0,
                                           0, 0, 0, 0, 1, 0,
                                           0, 0, 0, 0, 0, 1};

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Nav.Odometry();
            message.pose.covariance = identityMatrix;
            message.twist.covariance = identityMatrix;
            message.child_frame_id = FrameId;
        }

        private void UpdateMessage()
        {
            message.header.Update();

            float deltaTime = Time.realtimeSinceStartup - previousRealTime;

            Vector3 linearVelocity = (PublishedTransform.position - previousPosition) / deltaTime;
            Vector3 angularVelocity = (PublishedTransform.rotation.eulerAngles - previousRotation.eulerAngles) / deltaTime;
            
            message.twist.twist.linear = GetGeometryVector3(linearVelocity.Unity2Ros()); ;
            message.twist.twist.angular = GetGeometryVector3(- angularVelocity.Unity2Ros());

            message.pose.pose.position = GetGeometryPoint(PublishedTransform.position.Unity2Ros());
            message.pose.pose.orientation = GetGeometryQuaternion(PublishedTransform.rotation.Unity2Ros());

            previousRealTime = Time.realtimeSinceStartup;
            previousPosition = PublishedTransform.position;
            previousRotation = PublishedTransform.rotation;

            Publish(message);
        }

        private static MessageTypes.Geometry.Vector3 GetGeometryVector3(Vector3 vector3)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = vector3.x;
            geometryVector3.y = vector3.y;
            geometryVector3.z = vector3.z;
            return geometryVector3;
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
