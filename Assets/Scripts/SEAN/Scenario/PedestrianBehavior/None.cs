// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.PedestrianBehavior
{

    public class None : Base
    {
        public override Trajectory.TrackedGroup[] groups
        {
            get
            {
                return new Trajectory.TrackedGroup[] {};
            }
        }

        public override Trajectory.TrackedAgent[] agents
        {
            get
            {
                return new Trajectory.TrackedAgent[] {};
            }
        }
    }

}