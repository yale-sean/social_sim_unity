// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Metrics
{
    public class MetricsPublisher : MonoBehaviour
    {
        protected SEAN sean;
        protected ROSConnection ros;

        public string TopicName = "/social_sim/metrics";

        private RosMessageTypes.SocialSimRos.MTrialInfo trialInfoMessage;


        void Start()
        {
            trialInfoMessage = new RosMessageTypes.SocialSimRos.MTrialInfo();
            ros = ROSConnection.instance;
            sean = SEAN.instance;
        }

        private void Update()
        {
            if (!sean.robotTask.isRunning) { return; }
            sean.clock.UpdateMHeader(trialInfoMessage.header);

            // Information about the current interaction

            trialInfoMessage.trial_start = sean.metrics.StartTime;
            trialInfoMessage.timeout_time = sean.robotTask.timeoutTaskSeconds;
            trialInfoMessage.trial_name = sean.pedestrianBehavior.name + "_" + sean.robotTask.name;
            trialInfoMessage.trial_number = sean.robotTask.number;
            trialInfoMessage.num_actors = (uint)sean.pedestrianBehavior.agents.Length;

            // Robot start / goal
            trialInfoMessage.robot_start = Util.Geometry.GetMPose(sean.robotTask.robotStart);
            trialInfoMessage.robot_goal = Util.Geometry.GetMPose(sean.robotTask.robotGoal);

            // Robot location / distance relative to start / goal
            trialInfoMessage.dist_to_target = sean.metrics.TargetDist;
            trialInfoMessage.min_dist_to_target = sean.metrics.MinDistToTarget;
            trialInfoMessage.robot_poses = new RosMessageTypes.Geometry.MPose[sean.metrics.RobotPoses.Count];
            trialInfoMessage.robot_poses_ts = new RosMessageTypes.Std.MTime[sean.metrics.RobotPoses.Count];
            for (int i = 0; i < sean.metrics.RobotPoses.Count; i++)
            {
                trialInfoMessage.robot_poses[i] = Util.Geometry.GetMPose(sean.metrics.RobotPoses[i]);
                trialInfoMessage.robot_poses_ts[i] = sean.metrics.RobotPosesTS[i];
            }

            // Robot location relative to pedestrians
            trialInfoMessage.min_dist_to_ped = sean.metrics.MinDistToPed;

            trialInfoMessage.robot_on_person_intimate_dist_violations = sean.metrics.RobotOnPersonIntimateDistViolations;
            trialInfoMessage.person_on_robot_intimate_dist_violations = sean.metrics.PersonOnRobotIntimateDistViolations;
            trialInfoMessage.robot_on_person_personal_dist_violations = sean.metrics.RobotOnPersonPersonalDistViolations;
            trialInfoMessage.person_on_robot_personal_dist_violations = sean.metrics.PersonOnRobotPersonalDistViolations;
            trialInfoMessage.robot_on_person_collisions = sean.metrics.RobotOnPersonCollisions;
            trialInfoMessage.person_on_robot_collisions = sean.metrics.PersonOnRobotCollisions;

            // Collisions w/ static objects
            trialInfoMessage.obj_collisions = sean.metrics.ObjectCollisions;

            ros.Send(TopicName, trialInfoMessage);
        }


    }
}