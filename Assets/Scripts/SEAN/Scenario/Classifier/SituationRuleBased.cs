// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Classifier
{
    public class SituationRuleBased : SituationClassifier
    {

        protected override string ClassifierType { get { return "rule_based/"; } }

        #region Parameters

        ///<summary>Classifer update frequency</summary>
        public float UpdateFrequency = 1f;
        ///<summary>Pedestrians within this many meters of the robot will be considered.</summary>
        public float ParamNearbyPedestrianThreshold = 1.2f * 2;
        ///<summary>Groups within this many meters of the robot will be considered.</summary>
        public float ParamNearbyGroupThreshold = 1.2f * 2;
        ///<summary>+/- this many degrees from 90 or 180 will still be considered parallel or perpendicular</summary>
        public float ParamThetaDegreesEpsilon = 15f;
        // TODO: unify paramtrajectoryminvel and vectormagnitudeminmoving
        ///<summary>Minimum average velocity to consider a pedestrian trajectory as "moving"</summary>
        public float ParamTrajectoryMinVel = 0.15f;
        ///<summary>Minimum magnitude to consider a vector "moving"</summary>
        public float VectorMagnitudeMinMoving = 0.01f;

        #endregion

        override public void Start()
        {
            base.Start();
        }

        void Update()
        {
            if (Time.time - lastUpdateTime >= 1 / UpdateFrequency)
            {
                CheckClasses();
                lastUpdateTime = Time.time;
            }
        }

        private bool GoalInGroup()
        {
            return GroupsContain(sean.robotTask.robotGoal.transform).Count > 0;
        }

        private bool StartInGroup()
        {
            return GroupsContain(sean.robotTask.robotStart.transform).Count > 0;
        }

        /// <summary>
        ///   - [Group Membership] given an o-space center location
        ///     - [Distance] Find all people within 0.7 to 1.2 (average) to 1.7m radius of the o-space center
        ///     - [Orientation] Looking at o-space center w/in 45 degrees
        ///     - [Velocity] with a velocity of less than walking (1.5m/s) TODO: check scaling?
        /// </summary>
        private List<Trajectory.TrackedGroup> GroupsContain(Transform transform)
        {
            List<Trajectory.TrackedGroup> groups = new List<Trajectory.TrackedGroup>();
            foreach (Trajectory.TrackedGroup group in sean.pedestrianBehavior.groups)
            {
                // print("Checking " + transform.gameObject.name + " against " + group.gameObject.name);
                if (group.ContainsTransform(transform))
                {
                    groups.Add(group);
                }
            }
            return groups;
        }

        private bool RobotMovingCloserToGoal(float minVel = 0.5f)
        {
            return sean.robot.trajectory.movingTowards(sean.robotTask.robotGoal.transform.position, minVelocity: minVel);
        }

        void CheckGoalClasses(List<Trajectory.TrackedAgent> agentsNearbyRobot)
        {
            if (!sean.robotTask.isRunning) { return; }
            Vector3 robotPosition = sean.robot.transform.position;
            robotPosition.y = 0;
            Vector3 goalPosition = sean.robotTask.robotGoal.transform.position;
            goalPosition.y = 0;
            float goalAndRobotDistance = Vector3.Distance(robotPosition, goalPosition);

            bool goalInGroup = GoalInGroup();
            //print("goalInGroup: " + goalInGroup + ", nearby: " + agentsNearbyRobot.Count + ", goalAndRobotDistance < ParamNearbyGroupThreshold: " + goalAndRobotDistance + " < " + ParamNearbyGroupThreshold);
            if (agentsNearbyRobot.Count > 0 && goalAndRobotDistance < ParamNearbyGroupThreshold && goalInGroup)
            {
                // Join Group:
                //  - [Goal is in a group] If the goal is within R meters of an o-space center where, R is defined by the furthest person from the center of a group where we determine:
                //    - [Group Membership] given an o-space center location
                //      - [Distance] Find all people within 0.7 to 1.2 (average) to 1.7m radius of the o-space center
                //      - [Orientation] Looking at o-space center w/in 45 degrees
                //      - [Velocity] with a velocity of less than walking (1.5m/s)
                //  - AND [Robot Position] The robot has moved closer to the goal over the window period
                Publish(joinGroup.Set(1.0f));
            }
            else
            {
                Publish(joinGroup.Set(0.0f));
            }

            Vector3 startPosition = sean.robotTask.robotStart.transform.position;
            startPosition.y = 0;
            float startAndRobotDistance = Vector3.Distance(robotPosition, startPosition);

            bool startInGroup = StartInGroup();
            if (agentsNearbyRobot.Count > 0 && startAndRobotDistance < ParamNearbyGroupThreshold && startInGroup)
            {
                // Leave Group:
                //  - [!(Goal is in a group)] (Goal does not meet "Goal is in a group" definition for any groups)
                //  - AND [Robot Position] The robot has moved closer to the goal over the window period
                Publish(leaveGroup.Set(1.0f));
            }
            else
            {
                Publish(leaveGroup.Set(0.0f));
            }
        }

        void CheckTrajectoryClasses(List<Trajectory.TrackedAgent> agentsNearbyRobot)
        {
            if (agentsNearbyRobot.Count < 1)
            {
                Publish(empty.Set(1.0f));
            }
            else
            {
                Publish(empty.Set(0.0f));
            }
            bool crossPathFlag = false;
            bool downPathFlag = false;
            // If one or more agents matches the critera for cross path or down path, set the appropriate class to true
            foreach (Trajectory.TrackedAgent agent in agentsNearbyRobot)
            {
                if (agent.trajectory.velocity < ParamTrajectoryMinVel)
                {
                    //Debug.Log("vel: " + agent.trajectory.velocity + " < " + ParamTrajectoryMinVel);
                    continue;
                }
                // We dont mind if the robot is stationary, so we'll use a unit vector in the direction of the robot instead in case the robot trajectory vector is 0
                float deg = agent.trajectory.Angle(sean.robot.trajectory);
                if (sean.robot.trajectory.vector.magnitude < VectorMagnitudeMinMoving)
                {
                    //print("rot: " + sean.robot.transform.rotation);
                    Vector3 robotForward = sean.robot.transform.rotation * Vector3.forward;
                    deg = agent.trajectory.Angle(new Vector2(robotForward.x, robotForward.z));
                }
                //print(agent.name + " deg: " + deg);
                //print(" 90:" + Mathf.Abs(deg - 90) + ", 270: " + Mathf.Abs(deg - 270));
                //print(" 0:" + Mathf.Abs(deg) + ", 180: " + Mathf.Abs(deg - 180));
                // directionality of agent
                if (!crossPath && (Mathf.Abs(deg - 90) < ParamThetaDegreesEpsilon) ||
                    (Mathf.Abs(deg - 270) < ParamThetaDegreesEpsilon))
                {
                    Publish(crossPath.Set(1.0f));
                    //print(" CROSS");
                    //Debug.Break();
                    crossPathFlag = true;
                }
                if (!downPath && (deg < ParamThetaDegreesEpsilon) ||
                    (Mathf.Abs(deg - 180) < ParamThetaDegreesEpsilon))
                {
                    Publish(downPath.Set(1.0f));
                    //print(" DOWN");
                    //Debug.Break();
                    downPathFlag = true;
                }
            }
            if (!crossPathFlag)
            {
                Publish(crossPath.Set(0.0f));
            }
            if (!downPathFlag)
            {
                Publish(downPath.Set(0.0f));
            }
        }

        void CheckClasses()
        {
            //print("Checking classes");
            // Get all pedestrians
            // Calculate pedestrian trajectories
            List<Trajectory.TrackedAgent> agentsNearbyRobot = sean.robot.trajectory.nearbyAgents(ParamNearbyPedestrianThreshold);
            //print("agentsNearbyRobot: " + agentsNearbyRobot.Count);
            CheckGoalClasses(agentsNearbyRobot);
            CheckTrajectoryClasses(agentsNearbyRobot);
        }
    }
}
