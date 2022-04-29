// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System;
using SEAN.Scenario.Agents.Playback;
using UnityEngine;
using System.Linq;

namespace SEAN.Scenario.PedestrianBehavior
{
    public class Playback : Base
    {

        GameObject playback;
        LoadAllAvatar navManager;

        override public string scenario_name { get { return "Playback"; } }

        public void Start()
        {
            base.Start();
            foreach (Transform transform in pedestrianControl.transform)
            {
                if (transform.name == "Playback")
                {
                    playback = transform.gameObject;
                    break;
                }
            }
            if (playback == null)
            {
                throw new Exception("Could not find Playback GameObject in the PedestrianControl game object.");
            }
            playback.SetActive(true);
            navManager = playback.GetComponent<LoadAllAvatar>();
            if (navManager == null)
            {
                throw new Exception("Could not find the navManager on the Playback GameObject.");
            }
        }

        public override Trajectory.TrackedGroup[] groups
        {
            get
            {
                // TODO:
                return new Trajectory.TrackedGroup[0];
            }
        }

        public override Trajectory.TrackedAgent[] agents
        {
            get
            {
                if (navManager == null)
                {
                    return new Trajectory.TrackedAgent[0];
                }
                //Debug.Log("size of agent list:"+navManager.agentsList.Count);
                return navManager.agents.Values.ToArray();
            }
        }

    }
}
