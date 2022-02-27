// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario.PedestrianBehavior
{
    public enum SocialSituation
    {
        Empty,
        JoinGroup,
        LeaveGroup,
        DownPath,
        CrossPath,
    }

    public abstract class Base : MonoBehaviour
    {
        protected GameObject pedestrianControl;

        protected void Start()
        {
            pedestrianControl = GameObject.Find("/Environment/PedestrianControl");
        }

        protected string _name;
        public virtual string scenario_name
        {
            get
            {
                return _name;
            }
        }

        public void SetScenarioName(string name)
        {
            _name = name;
        }

        public abstract Trajectory.TrackedGroup[] groups { get; }
        public abstract Trajectory.TrackedAgent[] agents { get; }
    }
}