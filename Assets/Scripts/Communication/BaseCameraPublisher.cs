using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;

[RequireComponent(typeof(Camera))]
public abstract class BaseCameraPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string Topic = "";
    public string FrameId;
    protected Camera camera;

    protected void Start()
    {
        ros = ROSConnection.instance;
        camera = GetComponent<Camera>();
        if (Topic.EndsWith("/"))
        {
            Topic = Topic.Remove(Topic.Length - 1, 1);
        }
        if (Topic == "")
        {
            throw new ArgumentException("Topic must be set.");
        }
        if (Topic.EndsWith("compressed") || Topic.EndsWith("raw") || Topic.EndsWith("camera_info"))
        {
            throw new ArgumentException("Topic name should not include format.");
        }
    }

    private RosMessageTypes.Sensor.MCameraInfo CameraInfo()
    {
        uint height = checked((uint)camera.pixelHeight);
        uint width = checked((uint)camera.pixelWidth);
        height = checked((uint)480);
        width = checked((uint)640);
        RosMessageTypes.Sensor.MCameraInfo message = new RosMessageTypes.Sensor.MCameraInfo
        {
            header = new RosMessageTypes.Std.MHeader { frame_id = FrameId },
            height = height,
            width = width,
            distortion_model = "plumb_bob",
            D = new double[0],
            K = new double[9],
            R = new double[9],
            P = new double[12]
        };
        message.header.frame_id = FrameId;
        // set R to identity
        message.R[0] = 1;
        message.R[4] = 1;
        message.R[8] = 1;
        var fx = camera.focalLength * 20;
        var fy = camera.focalLength * 20;
        var cx = width / 2;
        var cy = height / 2;
        var Tx = 0;
        var Ty = 0;
        // assign fx, cx, Tx, fy, cy, Ty to K and P (no ' values as there is no distortion)
        message.K[0] = fx;
        message.K[2] = cx;
        message.K[4] = fy;
        message.K[5] = cy;
        message.K[8] = 1;
        message.P[0] = fx;
        message.P[2] = cx;
        message.P[3] = Tx;
        message.P[5] = fy;
        message.P[6] = cy;
        message.P[7] = Ty;
        message.P[10] = 1;

        return message;
    }

    protected void PublishWithCameraInfo(RosMessageTypes.Sensor.MCompressedImage imageMessage)
    {
        RosMessageTypes.Sensor.MCameraInfo infoMessage = CameraInfo();
        SEAN.SEAN.instance.clock.UpdateMHeaders(new List<RosMessageTypes.Std.MHeader>() { imageMessage.header, infoMessage.header });
        imageMessage.header.frame_id = FrameId;
        ros.Send(Topic + "/camera_info", infoMessage);
        ros.Send(Topic + "/compressed", imageMessage);
    }
}
