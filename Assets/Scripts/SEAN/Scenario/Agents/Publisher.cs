// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Scenario.Agents
{

    public class Publisher : MonoBehaviour
    {
        private ROSConnection ros;
        private SEAN sean;

        public string topicName = "/social_sim/agent_positions";
        public string frame = "map";

        void Start()
        {
            ros = ROSConnection.instance;
            sean = SEAN.instance;
        }

        private void Update()
        {
            RosMessageTypes.Geometry.MPoseArray message = new RosMessageTypes.Geometry.MPoseArray();
            message.header.frame_id = frame;
            message.header.stamp = sean.clock.LastPublishedTime();
            // Filter out the robot
            List<Scenario.Trajectory.TrackedAgent> people = new List<Scenario.Trajectory.TrackedAgent>();
            foreach (Trajectory.TrackedAgent person in sean.pedestrianBehavior.agents)
            {
                //if (person is IVI.RobotAgent) { continue; }
                people.Add(person);
            }
            message.poses = new RosMessageTypes.Geometry.MPose[people.Count];
            int i = 0;
            foreach (Trajectory.TrackedAgent person in people)
            {
                message.poses[i++] = Util.Geometry.GetMPose(person.gameObject.transform);
            }
            ros.Send(topicName, message);
        }
    }
}
