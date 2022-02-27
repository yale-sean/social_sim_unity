// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class SocialForce
    {
        public Vector3 force;
        public bool anyAgentInFront;
        public bool anyAgentApproaching;

        public SocialForce()
        {
            force = Vector3.zero;
            anyAgentInFront = false;
            anyAgentApproaching = false;
        }

        public bool pauseable { get { return anyAgentInFront || anyAgentApproaching; } }
    }
}
