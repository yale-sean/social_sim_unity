using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

// TODO: Namespace
public class SpawnArrayPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "social_sim/agent_spawn_positions";
    // Tag for objects that can be robot and agent spawn locations
    public string SpawnTag = "Spawn";
    public bool ContinuousPublish = false;
    private string frame = "map";

    private RosMessageTypes.Geometry.MPoseArray message;
    public List<Transform> possiblePositions = new List<Transform>();

    void Start()
    {
        InitializeMessage();
        ros = ROSConnection.instance;

    }

    private void Update()
    {
        if ((possiblePositions.Count != GameObject.FindGameObjectsWithTag(SpawnTag).Length) || ContinuousPublish)
        {
            UpdateMessage();
        }
    }

    private void InitializeMessage()
    {
        message = new RosMessageTypes.Geometry.MPoseArray();
        message.header.frame_id = frame;
    }

    private void UpdateMessage()
    {
        possiblePositions.Clear();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(SpawnTag))
        {
            possiblePositions.Add(obj.transform);
        }

        InitializeMessage();
        message.header.stamp = new RosMessageTypes.Std.MTime();
        message.poses = new RosMessageTypes.Geometry.MPose[possiblePositions.Count];

        int i = 0;
        foreach (Transform pose in possiblePositions)
        {
            RosMessageTypes.Geometry.MPose rosPose = new RosMessageTypes.Geometry.MPose();

            RosMessageTypes.Geometry.MTransform ros_position = pose.transform.To<FLU>();

            rosPose.position.x = ros_position.translation.x;
            rosPose.position.y = ros_position.translation.y;
            rosPose.position.z = ros_position.translation.z;

            rosPose.orientation.w = ros_position.rotation.w;
            rosPose.orientation.x = ros_position.rotation.x;
            rosPose.orientation.y = ros_position.rotation.y;
            rosPose.orientation.z = ros_position.rotation.z;

            message.poses[i++] = rosPose;
        }
        SEAN.SEAN.instance.clock.UpdateMHeader(message.header);

        ros.Send(topicName, message);
    }
}
