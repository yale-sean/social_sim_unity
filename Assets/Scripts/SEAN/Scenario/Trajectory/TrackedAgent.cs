// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario.Trajectory
{
    public class TrackedAgent : MonoBehaviour
    {
        public TrackedTrajectory trajectory { get; private set; }

        private void GetOrAttachTrajectory()
        {
            if (trajectory != null) { return; }
            trajectory = gameObject.GetComponent<TrackedTrajectory>();
            if (trajectory == null)
            {
                trajectory = gameObject.AddComponent(typeof(TrackedTrajectory)) as TrackedTrajectory;
            }
        }

        protected virtual void Start()
        {
            GetOrAttachTrajectory();
        }
    }
}