// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.PedestrianBehavior
{

    public class GraphNav : Base
    {

        GameObject graph;

        IVI.NavManager navManager;

        public override string scenario_name
        {
            get
            {
                return "Graph_" + _name;
            }
        }

        public void Start()
        {
            base.Start();
            foreach (Transform transform in pedestrianControl.transform)
            {
                if (transform.name == "Graph")
                {
                    graph = transform.gameObject;
                    break;
                }
            }
            if (graph == null)
            {
                Debug.LogError("Could not find graph game object");
                return;
            }
            graph.SetActive(true);
            navManager = graph.GetComponent<IVI.NavManager>();
            if (navManager == null)
            {
                Debug.LogError("Could not find nav manager on graph");
                return;
            }
        }

        public override Trajectory.TrackedGroup[] groups
        {
            get
            {
                if (navManager == null)
                {
                    return new Trajectory.TrackedGroup[0];
                }
                List<Trajectory.TrackedGroup> groups = new List<Trajectory.TrackedGroup>();
                foreach (IVI.NavNode node in navManager.allNavNodes)
                {
                    if (node is Trajectory.ITrackedGroup)
                    {
                        groups.Add(((Trajectory.ITrackedGroup)node).group);
                    }
                }
                return groups.ToArray();
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
                return navManager.allAgents;
            }
        }
    }
}