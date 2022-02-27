// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Tasks
{
    public class RandomABNav : Base
    {

        protected override bool NewTask()
        {
            // Reported in the SceneInfo message
            sean.pedestrianBehavior.SetScenarioName("RandomAB");

            robotGoal.SetActive(true);

            Vector3 startPosition = Util.Navmesh.RandomHit().position;
            startPosition.y = 0.75f;
            robotStart.transform.position = startPosition;
            robotStart.transform.rotation = Util.Navmesh.RandomRotation();
            Vector3 goalPosition = Util.Navmesh.RandomHit().position;
            goalPosition.y = 0.5f;
            robotGoal.transform.position = goalPosition;
            robotGoal.transform.rotation = Util.Navmesh.RandomRotation();
            SetTargetFlags(robotGoal);

            return true;
        }
    }
}