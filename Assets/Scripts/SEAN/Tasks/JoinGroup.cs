// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Tasks
{
    public class JoinGroup : Base
    {
        public float startDistance;

        protected override bool NewTask()
        {
            robotGoal.SetActive(true);
            // Reported in the SceneInfo message
            sean.pedestrianBehavior.SetScenarioName("JoinGroup");

            Vector3 goalPosition;
            Quaternion goalRotation;
            bool result = GetRandomGroupMembershipTransform(out goalPosition, out goalRotation);
            if (!result)
            {
                Debug.LogWarning("Cannot find any groups, waiting for groups to start new task.");
                return false;
            }
            goalPosition.y = 0.5f;
            robotGoal.transform.position = goalPosition;
            robotGoal.transform.rotation = goalRotation;
            SetTargetFlags(robotGoal);

            Vector3 startPosition;
            if (startDistance <= 0)
            {
                startPosition = Util.Navmesh.RandomHit().position;
            }
            else
            {
                startPosition = Util.Navmesh.RandomHit(goalPosition, startDistance).position;

                // could give invalid location, if so retry three times
                int count = 0;
                while ((double.IsInfinity(startPosition.x) || double.IsInfinity(startPosition.x)) && count < 3)
                {
                    startPosition = Util.Navmesh.RandomHit(goalPosition, startDistance).position;
                    count++;
                }
                if (count == 3)
                {
                    startPosition = Util.Navmesh.RandomHit().position;
                }

            }
            startPosition.y = 0.75f;
            robotStart.transform.position = startPosition;
            robotStart.transform.rotation = Util.Navmesh.RandomRotation();

            return true;
        }

    }
}
