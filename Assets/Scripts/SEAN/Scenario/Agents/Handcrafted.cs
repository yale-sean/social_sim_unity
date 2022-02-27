// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class Handcrafted : BaseAgentManager
    {
        public float SPAWN_HEIGHT = 0;
        public float WAYPOINT_DIST = 1.5f;

        public List<IVI.INavigable> agents;
        public List<Trajectory.TrackedGroup> groups;

        public GameObject agentPrefab;

        private PedestrianBehavior.SocialSituation current = PedestrianBehavior.SocialSituation.Empty;
        private GameObject agentsGO;
        private List<Pose> spawnLocations;
        private Dictionary<IVI.INavigable, List<Pose>> agentGoals;

        public Pose openGroupLocation = Pose.identity;

        public string scenario_name
        {
            get
            {
                return "Handcrafted_" + current;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            agentGoals = new Dictionary<IVI.INavigable, List<Pose>>();
        }

        void Update()
        {
            // no need to plan if we only have static groups
            if (current == PedestrianBehavior.SocialSituation.DownPath || current == PedestrianBehavior.SocialSituation.CrossPath)
            {
                foreach (var agent in agents)
                {
                    if (agent.CloseEnough())
                    {
                        // Set the next goal
                        agent.InitDest(agentGoals[agent][1].position);
                        Pose currentGoal = agentGoals[agent][0];
                        agentGoals[agent].RemoveAt(0);
                        agentGoals[agent].Add(currentGoal);
                    }
                }
            }
        }

        public void NewScenario(PedestrianBehavior.SocialSituation situation, GameObject socialSituationEnv, GameObject spawnLocations)
        {
            current = situation;
            // Must clear before registering the new group
            Clear();
            //print("new scenario: " + current);
            this.spawnLocations = new List<Pose>();
            foreach (Transform s in spawnLocations.transform)
            {
                if (current == PedestrianBehavior.SocialSituation.JoinGroup || current == PedestrianBehavior.SocialSituation.LeaveGroup)
                {
                    groups.Add(Trajectory.TrackedGroup.GetOrAttach(s.gameObject));
                }
                // Put spawn positions on the ground plane
                Vector3 p = s.position;
                p.y = 0;
                this.spawnLocations.Add(new Pose(p, s.rotation));
            }
            // get agents game object
            foreach (Transform transform in socialSituationEnv.transform)
            {
                if (transform.gameObject.name == "Agents")
                {
                    agentsGO = transform.gameObject;
                }
            }
            SpawnAgents();
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
            openGroupLocation = Pose.identity;
            agentGoals.Clear();
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
            return agent;
        }

        void SpawnGroup(Pose groupCenter)
        {
            IVI.GroupDataLoader.GroupData group = IVI.GroupDataLoader.groupData[Random.Range(0, IVI.GroupDataLoader.groupData.Count)];
            int openGroupIdx = Random.Range(0, group.pos.Count);
            for (int i = 0; i < group.pos.Count; i++)
            {

                float angle = Mathf.Atan(group.pos[i].x / group.pos[i].z);
                if (group.pos[i].z > 0)
                {
                    angle += Mathf.PI;
                }
                Quaternion rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
                Pose pose = new Pose(groupCenter.position + group.pos[i], rotation);
                if (i == openGroupIdx)
                {
                    openGroupLocation = pose;
                }
                else
                {
                    SpawnAgent("Agent_" + i, pose);
                }
            }
        }

        void SpawnAgents()
        {
            if (current == PedestrianBehavior.SocialSituation.Empty)
            {
                return;
            }
            if (current == PedestrianBehavior.SocialSituation.DownPath || current == PedestrianBehavior.SocialSituation.CrossPath)
            {
                for (int i = 0; i < spawnLocations.Count; i++)
                {
                    Pose spawnPose = spawnLocations[i];
                    List<Pose> trajectoryPoints = new List<Pose>();
                    for (int j = 0; j < spawnLocations.Count; j++)
                    {
                        trajectoryPoints.Add(spawnLocations[(i + j) % spawnLocations.Count]);
                    }
                    IVI.INavigable agent = SpawnAgent("Agent_" + i++, spawnPose);
                    agentGoals.Add(agent, trajectoryPoints);
                    agent.InitDest(spawnPose.position);
                }
                return;
            }
            if (current == PedestrianBehavior.SocialSituation.JoinGroup || current == PedestrianBehavior.SocialSituation.LeaveGroup)
            {
                foreach (Pose pose in spawnLocations)
                {
                    SpawnGroup(pose);
                }
                return;
            }
        }
    }
}