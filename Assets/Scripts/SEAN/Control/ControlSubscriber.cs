// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Control
{
    public abstract class ControlSubscriber : MonoBehaviour
    {
        protected ROSConnection ros;
        protected SEAN sean;
        public string Topic = "/mobile_base_controller/cmd_vel";

        protected void Start()
        {
            sean = SEAN.instance;
            ros = ROSConnection.instance;
            ros.Subscribe<RosMessageTypes.Geometry.MTwist>(Topic, CmdVelMessage);
        }

        void Update()
        {
            // Optionally bypass ROS when localControl is enabled
            if (sean.ControlledAgent != Scenario.Agents.ControlledAgent.Robot) { return; }
            if (!sean.input.LocalInput) { return; }
            if (sean.input.L1)
            {
                CmdVelMessage(sean.input.CmdVel);
            }
        }

        protected abstract void CmdVelMessage(RosMessageTypes.Geometry.MTwist msg);
    }
}