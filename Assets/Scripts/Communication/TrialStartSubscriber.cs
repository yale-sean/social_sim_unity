using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class TrialStartSubscriber : MonoBehaviour
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

    void Awake()
    {
        peoplePositions = new List<Vector3>();
        peopleRotations = new List<Quaternion>();
    }

    void Start()
    {
        throw new NotImplementedException("To implement");
        ROSConnection.instance.Subscribe<RosMessageTypes.SocialSimRos.MTrialStart>("/social_sim/start_trial", ReceiveMessage);
    }

    private void Update()
    {
        if (isMessageReceived && prevStamp != stamp)
        {
            prevStamp = stamp;
            StartTrial();
        }
    }

    void ReceiveMessage(RosMessageTypes.SocialSimRos.MTrialStart message)
    {
        stamp = message.header.stamp.secs;
        robotPosition = ((Vector3<FLU>)GetPosition(message.spawn)).toUnity;
        robotRotation = ((Quaternion<FLU>)GetRotation(message.spawn)).toUnity;
        targetPosition = ((Vector3<FLU>)GetPosition(message.target)).toUnity;
        targetRotation = ((Quaternion<FLU>)GetRotation(message.target)).toUnity;
        if (message.people.poses.Length <= 0)
        {
            Debug.LogError("People positions are empty, cannot start");
        }
        peoplePositions.Clear();
        peopleRotations.Clear();
        foreach (RosMessageTypes.Geometry.MPose pose in message.people.poses)
        {
            peoplePositions.Add(((Vector3<FLU>)GetPosition(pose)).toUnity);
            peopleRotations.Add(((Quaternion<FLU>)GetRotation(pose)).toUnity);
        }
        timeLimit = message.time_limit;
        isMessageReceived = true;
    }

    private void StartTrial()
    {
        //Debug.Log("Starting Trial");
        //GetComponent<TrialStatusPublisher>().StartTrial(robotPosition, robotRotation,
        //                                                targetPosition, targetRotation,
        //                                                peoplePositions, peopleRotations,
        //                                                timeLimit);
    }

    private Vector3 GetPosition(RosMessageTypes.Geometry.MPose message)
    {
        return new Vector3(
            (float)message.position.x,
            (float)message.position.y,
            (float)message.position.z);
    }

    private Quaternion GetRotation(RosMessageTypes.Geometry.MPose message)
    {
        return new Quaternion(
            (float)message.orientation.x,
            (float)message.orientation.y,
            (float)message.orientation.z,
            (float)message.orientation.w);
    }
}
