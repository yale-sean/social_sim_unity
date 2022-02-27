// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class RandomABNavAgentManager : BaseAgentManager
    {
        public int numberOfAgents = 65;
        public float WAYPOINT_DIST = 1.5f;

        public List<IVI.INavigable> agents;
        public List<Trajectory.TrackedGroup> groups;

        public GameObject agentPrefab;

        private GameObject agentsGO;

        public string scenario_name
        {
            get
            {
                return "Random";
            }
        }

        void Update()
        {
            foreach (var agent in agents)
            {
                if (agent.CloseEnough())
                {
                    // Set the next goal
                    agent.InitDest(Util.Navmesh.RandomPose().position);
                }
            }
        }

        public void Restart()
        {
            // get agents game object
            foreach (Transform transform in transform)
            {
                if (transform.gameObject.name == "Agents")
                {
                    agentsGO = transform.gameObject;
                }
            }
            Clear();
            for (int i = 0; i < numberOfAgents; i++)
            {
                SpawnAgent("Agent_" + i, Util.Navmesh.RandomPose());
            }
        }

        void Clear()
        {
            if (!agentsGO) { return; }
            agents = new List<IVI.INavigable>();
            groups = new List<Trajectory.TrackedGroup>();
            foreach (Transform child in agentsGO.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        IVI.INavigable SpawnAgent(string name, Pose pose)
        {
            var sfRandom = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);
            IVI.INavigable agent = sfRandom.GetComponentInChildren<IVI.INavigable>();
            agent.name = name;
            agent.transform.position = pose.position;
            agent.transform.rotation = pose.rotation;
            agent.transform.parent = agentsGO.transform;
            agents.Add(agent);
            Vector3 pos = Util.Navmesh.RandomPose().position;
            //print(name + ": " + pos);
            agent.InitDest(pos);
            return agent;
        }
    }
}