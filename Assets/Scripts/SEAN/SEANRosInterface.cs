
// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN
{
    public class SEANRosInterface: MonoBehaviour
    {
        protected ROSConnection ros;
        protected SEAN sean;
        public string TopicTaskNew = "/social_sim/control/task/new";

        protected void Start()
        {
            sean = SEAN.instance;
            ros = ROSConnection.instance;

            // Subscribers
            ros.Subscribe<RosMessageTypes.Std.MBool>(TopicTaskNew, RecvTaskNew);
        }

        void RecvTaskNew(RosMessageTypes.Std.MBool msg)
        {
            if (sean.EvaluationMode) {
                Debug.LogWarning("New Task disabled in evaluation mode");
                return;
            }
            if (msg.data) {
                sean.robotTask.StartNewTask();
            }
        }
    }
}