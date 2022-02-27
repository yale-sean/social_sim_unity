// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Metrics
{
    public class Metrics : MonoBehaviour
    {
        protected SEAN sean;

        #region parameters
        private bool ShowDebug = false;

        public float UpdateFrequencyHz = 1f;
        public float IntimateDistance = 0.45f;
        public float PersonalDistance = 1.2f;

        // How long of a window should we look at over which we check to see if the robot has moved?
        public float StuckWindowSeconds = 5f;
        // How far does the robot need to move over the stuck window to not be considered stuck?
        public float StuckWindowDist = 0.05f;
        #endregion

        private float lastUpdate = 0f;
        private float UpdateSec { get { return 1 / UpdateFrequencyHz; } }
        public RosMessageTypes.Std.MTime StartTime { get; private set; }
        public uint ObjectCollisions { get; private set; }
        public double MinDistToPed { get; private set; }
        public double MinDistToTarget { get; private set; }

        public bool HasActors { get { return sean.pedestrianBehavior.agents.Length > 0; } }
        public List<Pose> RobotPoses { get; private set; }
        public List<RosMessageTypes.Std.MTime> RobotPosesTS { get; private set; }

        public uint RobotOnPersonCollisions { get; private set; }
        public uint PersonOnRobotCollisions { get; private set; }
        public uint RobotOnPersonIntimateDistViolations { get; private set; }
        public uint PersonOnRobotIntimateDistViolations { get; private set; }
        public uint RobotOnPersonPersonalDistViolations { get; private set; }
        public uint PersonOnRobotPersonalDistViolations { get; private set; }

        public void OnNewTask()
        {
            if (ShowDebug)
            {
                print("Metrics OnNewTask");
            }
            Reset();
        }

        public void Start()
        {
            sean = SEAN.instance;
            Reset();
            // Register the delegate for new task events
            if (ShowDebug)
            {
                print("Registering Metrics delegate");
            }
            sean.robotTask.onNewTask += OnNewTask;
            // Make sure our robot has a count collision component
            if (sean.robot.base_link.GetComponent<CountCollisions>() == null)
            {
                sean.robot.base_link.AddComponent<CountCollisions>();
            }
        }

        public void Update()
        {
            if (!sean.robotTask.isRunning) { return; }
            if (lastUpdate != 0 && Time.time - lastUpdate < UpdateSec)
            {
                return;
            }
            UpdateMetrics();
            lastUpdate = Time.time;
        }

        private void Reset()
        {
            if (ShowDebug)
            {
                print("Metrics Reset");
            }
            StartTime = sean.clock.Now();
            RobotPoses = new List<Pose>();
            RobotPosesTS = new List<RosMessageTypes.Std.MTime>();
            ObjectCollisions = 0;
            MinDistToPed = double.MaxValue;
            MinDistToTarget = double.MaxValue;

            RobotOnPersonCollisions = 0;
            PersonOnRobotCollisions = 0;
            RobotOnPersonIntimateDistViolations = 0;
            PersonOnRobotIntimateDistViolations = 0;
            RobotOnPersonPersonalDistViolations = 0;
            PersonOnRobotPersonalDistViolations = 0;
        }


        private void UpdateMetrics()
        {
            // add a location
            RobotPoses.Add(new Pose(sean.robot.position, sean.robot.rotation));
            RobotPosesTS.Add(sean.clock.Now());
            MinDistToTarget = Math.Min(MinDistToTarget, TargetDist);
            MinDistToPed = Math.Min(MinDistToPed, NearestPedestrian);
        }

        public void IncrementObjectCollisions()
        {
            ObjectCollisions++;
        }

        public void IncrementPeopleCollisions(bool isRobotAtFault)
        {
            if (isRobotAtFault)
            {
                RobotOnPersonCollisions++;
            }
            else
            {
                PersonOnRobotCollisions++;
            }
        }

        public void IncrementIntimateSpaceViolations(bool isRobotAtFault)
        {
            if (isRobotAtFault)
            {
                RobotOnPersonIntimateDistViolations++;
            }
            else
            {
                PersonOnRobotIntimateDistViolations++;
            }
        }

        public void IncrementPersonalDistViolations(bool isRobotAtFault)
        {
            if (isRobotAtFault)
            {
                RobotOnPersonPersonalDistViolations++;
            }
            else
            {
                PersonOnRobotPersonalDistViolations++;
            }
        }

        public double PathLength
        {
            get
            {
                double pathLength = 0;
                for (int i = 0; i < RobotPoses.Count - 1; i++)
                {
                    pathLength += Util.Geometry.GroundPlaneDist(RobotPoses[i].position, RobotPoses[i + 1].position);
                }
                return pathLength;
            }
        }

        public double TargetDist
        {
            get
            {
                return Util.Geometry.GroundPlaneDist(sean.robot.position, sean.robotTask.robotGoal.transform.position);
            }
        }

        public double TargetDistNorm
        {
            get
            {
                return TargetDist / PathLength;
            }
        }

        private double NearestPedestrian
        {
            get
            {
                double minDist = double.MaxValue;
                foreach (Scenario.Trajectory.TrackedAgent agents in sean.pedestrianBehavior.agents)
                {
                    double dist = Util.Geometry.GroundPlaneDist(sean.robot.position, agents.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }
                return minDist;
            }
        }
    }

}
