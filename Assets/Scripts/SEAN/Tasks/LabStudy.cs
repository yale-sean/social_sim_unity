// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Tasks
{
    public class LabStudy : Base
    {
        Scenario.PedestrianBehavior.LabStudy scenario;

        public string pointA = "PointA";
        public string pointB = "PointB";
        public string pointC = "PointC";
        public string pointD = "PointD";
        public string pointE = "PointE";
        public string pointF = "PointF";

        private int trajectoryID;
        private int currentPoint;

        public List<string> trajectory;

        public override void Start()
        {
            base.Start();
            if (!(sean.pedestrianBehavior is Scenario.PedestrianBehavior.LabStudy))
            {
                throw new Exception("Please activate the LabStudyScenario");
            }
            scenario = (Scenario.PedestrianBehavior.LabStudy)sean.pedestrianBehavior;

            trajectoryID = sean.robotTask.taskID;
            currentPoint = 0;

            trajectory = GetTrajectory(trajectoryID);
            InitializeTaskPositions();
            onNewTask.Invoke();
            Publish(interactiveGoal);
        }

        private List<string> GetTrajectory(int id)
        {
            trajectory = new List<string>();

            switch(id)
            {
                case 0:
                    // walk around room
                    trajectory.Add(pointA);
                    trajectory.Add(pointB);
                    trajectory.Add(pointE);
                    trajectory.Add(pointD);
                    return trajectory;
                case 1:
                    // around blocks and through middle
                    trajectory.Add(pointA);
                    trajectory.Add(pointC);
                    trajectory.Add(pointE);
                    trajectory.Add(pointF);
                    trajectory.Add(pointD);
                    trajectory.Add(pointC);
                    return trajectory;
                case 2:
                    // figure 8 through center
                    trajectory.Add(pointB);
                    trajectory.Add(pointE);
                    trajectory.Add(pointF);
                    trajectory.Add(pointC);
                    trajectory.Add(pointA);
                    trajectory.Add(pointD);
                    trajectory.Add(pointC);
                    trajectory.Add(pointB);
                    return trajectory;
                case 3:
                    // navigate through narrow spaces
                    trajectory.Add(pointE);
                    trajectory.Add(pointF);
                    trajectory.Add(pointC);
                    trajectory.Add(pointA);
                    trajectory.Add(pointD);
                    trajectory.Add(pointF);
                    trajectory.Add(pointE);
                    return trajectory;
            }
            throw new Exception("Please use a valid task ID");
        }

        protected override void CheckNewTask()
        {
            //Debug.Log("Curr Trajectory " + currentTrajectory);
            //Debug.Log("Curr Point " + currentPoint);
            float distToWayPoint = Vector3.Distance(sean.player.transform.GetChild(0).position, robotGoal.transform.position);
            if (distToWayPoint < completionDistance)
            {
                NewTask();
            }
        }

        private void UpdateWaypoints()
        {
            robotGoal.transform.position = scenario.positions[trajectory[currentPoint + 1]].transform.position;
            robotGoal.transform.rotation = scenario.positions[trajectory[currentPoint + 1]].transform.rotation;
            playerGoal.transform.position = scenario.positions[trajectory[currentPoint + 1]].transform.position + Vector3.up;
            playerGoal.transform.rotation = scenario.positions[trajectory[currentPoint + 1]].transform.rotation;
            Publish(interactiveGoal);
        }

        private void InitializeTaskPositions()
        {
            foreach (GameObject position in scenario.positions.Values)
            {
                position.SetActive(false);
            }
            Vector3 offset1 = new Vector3(-0.75f, 0, 0);
            Vector3 offset2 = new Vector3(0.75f, 0, 0);
            if (trajectoryID == 0 || trajectoryID == 1)
            {
                robotStart.transform.position = scenario.positions[trajectory[currentPoint]].transform.position + offset1;
            }
            if (trajectoryID == 2 || trajectoryID == 3)
            {
                robotStart.transform.position = scenario.positions[trajectory[currentPoint]].transform.position + offset2;
            }
            robotStart.transform.rotation = scenario.positions[trajectory[currentPoint]].transform.rotation;

            playerStart.transform.position = scenario.positions[trajectory[currentPoint]].transform.position;
            // increase playerStart y-axis to prevent player from jumping at start of task
            Vector3 startPos = playerStart.transform.position + new Vector3(0, 1, 0);
            playerStart.transform.position = startPos;
            playerStart.transform.rotation = scenario.positions[trajectory[currentPoint]].transform.rotation;
            UpdateWaypoints();
        }

        protected override bool NewTask()
        {
            if (currentPoint < trajectory.Count - 2)
            {
                currentPoint += 1;
                UpdateWaypoints();
                // subtask, not new task
                return false;
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            return true;
        }

    }
}