// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Tasks
{
    public class LeaveGroup : Base
    {
        protected override bool NewTask()
        {
            robotGoal.SetActive(true);

            // Reported in the SceneInfo message
            ((Scenario.PedestrianBehavior.GraphNav)sean.pedestrianBehavior).SetScenarioName("LeaveGroup");

            Vector3 startPosition;
            Quaternion startRotation;
            bool result = GetRandomGroupMembershipTransform(out startPosition, out startRotation);
            if (!result)
            {
                Debug.LogWarning("Cannot find any groups, waiting for groups to start new task.");
                return false;
            }
            startPosition.y = 0.5f;
            robotStart.transform.position = startPosition;
            robotStart.transform.rotation = startRotation;
            Vector3 goalPosition = Util.Navmesh.RandomHit().position;
            goalPosition.y = 0.75f;
            robotGoal.transform.position = goalPosition;
            robotGoal.transform.rotation = Util.Navmesh.RandomRotation();
            SetTargetFlags(robotGoal);
            return true;
        }

    }
}
