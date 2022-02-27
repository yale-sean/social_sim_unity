// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using System.Collections.Generic;

namespace SEAN.TF
{
    public class WorldTransformPublishers : BaseTransformPublisher
    {
        SEAN sean;
        GameObject MapToRobot;
        GameObject MapToOdom;
        private bool initialized = false;
        RosMessageTypes.Geometry.MPoseStamped mapToRobotPoseStamped = new RosMessageTypes.Geometry.MPoseStamped();
        RosMessageTypes.Geometry.MPoseStamped mapToOdomPoseStamped = new RosMessageTypes.Geometry.MPoseStamped();


        private void Start()
        {
            sean = SEAN.instance;
            base.Start();

            MapToRobot = new GameObject();
            MapToOdom = new GameObject();

            MapToRobot.name = "map_to_base_link";
            MapToOdom.name = "map_to_odom";

            mapToRobotPoseStamped.header.frame_id = "map";
            mapToOdomPoseStamped.header.frame_id = "map";
        }

        private void Update()
        {

            if (!initialized)
            {
                MapToOdom.transform.position = sean.robot.position;
                MapToOdom.transform.rotation = sean.robot.rotation;
                mapToOdomPoseStamped.pose.position = Util.Geometry.GetGeometryPoint(MapToOdom.transform.position.To<FLU>());
                mapToOdomPoseStamped.pose.orientation = Util.Geometry.GetGeometryQuaternion(MapToOdom.transform.rotation.To<FLU>());
                initialized = true;
            }

            MapToRobot.transform.position = sean.robot.position;
            MapToRobot.transform.rotation = sean.robot.rotation;
            mapToRobotPoseStamped.pose.position = Util.Geometry.GetGeometryPoint(MapToRobot.transform.position.To<FLU>());
            mapToRobotPoseStamped.pose.orientation = Util.Geometry.GetGeometryQuaternion(MapToRobot.transform.rotation.To<FLU>());
            List<NamedTransform> transforms = new List<NamedTransform>(){
                new NamedTransform("/" + MapToOdom.name, mapToOdomPoseStamped),
                new NamedTransform("/" + MapToRobot.name, mapToRobotPoseStamped)
            };
            PublishIfNew(transforms);
        }
    }
}
