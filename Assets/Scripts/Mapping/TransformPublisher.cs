using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class TransformPublisher : MonoBehaviour
    {
        public Rigidbody Robot;

        GameObject OdomToRobot;
        GameObject MapToOdom;

        PoseStampedPublisher odomToRobotPub;
        PoseStampedPublisher mapToOdomPub;

        private bool initialized = false;

        private void Start() {
            OdomToRobot = new GameObject();
            OdomToRobot.name = "OdomToRobot";
            MapToOdom = new GameObject();
            MapToOdom.name = "MapToOdom";

            odomToRobotPub = gameObject.AddComponent<PoseStampedPublisher>() as PoseStampedPublisher;
            odomToRobotPub.PublishedTransform = OdomToRobot.transform;
            odomToRobotPub.FrameId = "odom";
            odomToRobotPub.Topic = "/odom_to_robot";

            mapToOdomPub = gameObject.AddComponent<PoseStampedPublisher>() as PoseStampedPublisher;
            mapToOdomPub.PublishedTransform = MapToOdom.transform;
            mapToOdomPub.FrameId = "map";
            mapToOdomPub.Topic = "/map_to_odom";
        }

        private void Update() {

            if (!initialized) {
                MapToOdom.transform.position = Robot.transform.position;
                MapToOdom.transform.rotation = Robot.transform.rotation;
                initialized =  true;
            }
            OdomToRobot.transform.position = Robot.transform.position - MapToOdom.transform.position;
            OdomToRobot.transform.rotation = Robot.transform.rotation * Quaternion.Inverse(MapToOdom.transform.rotation);
        }
    }
}
