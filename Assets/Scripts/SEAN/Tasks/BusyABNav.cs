// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Tasks
{
    public class BusyABNav : Base
    {

        KMeansResults result;
        
        public float DistanceFromBiggestGroupCentroid = 5;

        protected override bool NewTask()
        {
            robotGoal.SetActive(true);

            // Reported in the SceneInfo message
            sean.pedestrianBehavior.SetScenarioName("CrowdedAB");

            Scenario.Trajectory.TrackedAgent[] agents = sean.pedestrianBehavior.agents;
            if (agents.Length == 0)
            {
                return false;
            }
            double[][] data = new double[agents.Length][];
            for (int i = 0; i < agents.Length; i++)
            {
                data[i] = new double[] { agents[i].transform.position.x, agents[i].transform.position.z };
            }
            result = KMeans.Cluster(data, 3, 10, 0);
            int biggestCluster = 0;
            int biggestClusterID = 0;
            for (int i = 0; i < result.clusters.Length; i++)
            {
                int clusterSize = result.clusters[i].Length;
                if (clusterSize > biggestCluster)
                {
                    biggestCluster = clusterSize;
                    biggestClusterID = i;
                }
            }
            Scenario.Trajectory.TrackedAgent centroid = agents[result.centroids[biggestClusterID]];
            
            // Set the Robot to start from farther away from the group
            Vector3 startPosition = Util.Navmesh.RandomHit(centroid.transform.position, DistanceFromBiggestGroupCentroid*3).position;
            while (startPosition.x == float.PositiveInfinity)
            {
                startPosition = Util.Navmesh.RandomHit(centroid.transform.position, DistanceFromBiggestGroupCentroid*3).position;
            }
            startPosition.y = 0.75f;
            robotStart.transform.position = startPosition;
            robotStart.transform.rotation = Util.Navmesh.RandomRotation();
            //start.transform.position = startPosition;
            //start.transform.rotation = GetRandomRotation();
            Vector3 goalPosition = Util.Navmesh.RandomHit(centroid.transform.position, DistanceFromBiggestGroupCentroid).position;
            while (goalPosition.x == float.PositiveInfinity)
            {
                goalPosition = Util.Navmesh.RandomHit(centroid.transform.position, DistanceFromBiggestGroupCentroid).position;
            }
            goalPosition.y = 0.5f;
            robotGoal.transform.position = goalPosition;
            robotGoal.transform.rotation = Util.Navmesh.RandomRotation();
            SetTargetFlags(robotGoal);
            return true;
        }
    }

}