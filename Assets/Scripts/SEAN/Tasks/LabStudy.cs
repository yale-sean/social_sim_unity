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

        private int currentTrajectory;
        private int currentPoint;

        public List<List<string>> trajectories;

        public override void Start()
        {
            base.Start();
            if (!(sean.pedestrianBehavior is Scenario.PedestrianBehavior.LabStudy))
            {
                throw new Exception("Please activate the LabStudyScenario");
            }
            scenario = (Scenario.PedestrianBehavior.LabStudy)sean.pedestrianBehavior;
            SetTrajectories();
            NewTask();
            onNewTask.Invoke();
            Publish();
        }

        private void SetTrajectories()
        {
            trajectories = new List<List<string>>();

            List<string> one = new List<string>();
            one.Add(pointA);
            one.Add(pointB);
            trajectories.Add(one);

            List<string> two = new List<string>();
            two.Add(pointB);
            two.Add(pointC);
            two.Add(pointD);
            trajectories.Add(two);

            List<string> three = new List<string>();
            three.Add(pointD);
            three.Add(pointA);
            three.Add(pointB);
            three.Add(pointE);
            trajectories.Add(three);

            currentPoint = -1;
            currentTrajectory = 0;

            //List<string> four = new List<string>();
            //four.Add(PointE);
            //four.Add(PointF);
            //four.Add(PointC);
            //four.Add(PointA);
            //four.Add(PointD);
        }

        protected override bool NewTask()
        {
            foreach (GameObject position in scenario.positions.Values)
            {
                position.SetActive(false);
            }

            if (currentTrajectory < trajectories.Count)
            {

                if (currentPoint < trajectories[currentTrajectory].Count - 2)
                {
                    currentPoint += 1;
                }
                else
                {
                    currentTrajectory += 1;
                    currentPoint = 0;
                }
            }

            robotStart.transform.position = scenario.positions[trajectories[currentTrajectory][currentPoint]].transform.position;
            robotStart.transform.rotation = scenario.positions[trajectories[currentTrajectory][currentPoint]].transform.rotation;
            robotGoal.transform.position = scenario.positions[trajectories[currentTrajectory][currentPoint + 1]].transform.position;
            robotGoal.transform.rotation = scenario.positions[trajectories[currentTrajectory][currentPoint + 1]].transform.rotation;

            playerStart.transform.position = scenario.positions[trajectories[currentTrajectory][currentPoint]].transform.position;

            // increase playerStart y-axis to prevent player from jumping at start of task
            // increase playerStart x-axis to prevent player from spawning on top of the robot
            Vector3 startPos = playerStart.transform.position + new Vector3(0.5f, 1, 0);
            playerStart.transform.position = startPos;
            playerStart.transform.rotation = scenario.positions[trajectories[currentTrajectory][currentPoint]].transform.rotation;
            playerGoal.transform.position = scenario.positions[trajectories[currentTrajectory][currentPoint + 1]].transform.position + Vector3.up;
            playerGoal.transform.rotation = scenario.positions[trajectories[currentTrajectory][currentPoint + 1]].transform.rotation;

            SetTargetFlags(playerGoal);
            return true;
        }

    }
}