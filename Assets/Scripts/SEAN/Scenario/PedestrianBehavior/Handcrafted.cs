// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SEAN.Scenario.PedestrianBehavior
{
    public class Handcrafted : Base
    {
        Agents.Handcrafted agentManager;
        // Top level situation in the environment
        GameObject socialSituationEnv;
        // Top level of handcrafted situations, named with the same name as the enum
        GameObject socialSituationHandcrafted;
        Dictionary<SocialSituation, GameObject> socialSituations;
        private GameObject situationInstance;
        private GameObject _spawnLocations;
        private GameObject _startLocations;
        private GameObject _targetLocations;

        public Pose start = Pose.identity;
        public Pose goal = Pose.identity;

        SocialSituation current = SocialSituation.Empty;

        public override string scenario_name
        {
            get
            {
                return "Handcrafted_" + current;
            }
        }

        public void Start()
        {
            base.Start();
            foreach (Transform transform in pedestrianControl.transform)
            {
                if (transform.name == "HandcraftedSocialSituations")
                {
                    socialSituationEnv = transform.gameObject;
                    break;
                }
            }
            if (socialSituationEnv == null)
            {
                throw new System.Exception("Could not find social situations game object in environment");
            }
            socialSituationEnv.SetActive(true);

            foreach (Transform t1 in pedestrianControl.transform)
            {
                if (t1.name == "HandcraftedSocialSituations")
                {
                    foreach (Transform t2 in t1.gameObject.transform)
                    {
                        if (t2.name == "Handcrafted")
                        {
                            socialSituationHandcrafted = t2.gameObject;
                            break;
                        }
                    }
                }
            }
            if (socialSituationHandcrafted == null)
            {
                throw new System.Exception("Could not find handcrafted game object in social situations game object");
            }
            socialSituationHandcrafted.SetActive(true);
        }

        private Dictionary<SocialSituation, GameObject> SocialSituations
        {
            get
            {
                if (socialSituations == null)
                {
                    socialSituations = new Dictionary<SocialSituation, GameObject>();
                    foreach (Transform transform in socialSituationHandcrafted.transform)
                    {
                        socialSituations.Add((SocialSituation)System.Enum.Parse(typeof(SocialSituation), transform.name), transform.gameObject);
                    }
                }
                return socialSituations;
            }
        }

        public bool NewScenario(SocialSituation socialSituation)
        {
            current = socialSituation;
            foreach (GameObject situation in SocialSituations.Values)
            {
                situation.SetActive(false);
            }
            GameObject handcraftedSituation = SocialSituations[socialSituation];
            handcraftedSituation.SetActive(true);
            Transform[] allInstances = handcraftedSituation.transform.Cast<Transform>().ToArray();
            int index = UnityEngine.Random.Range(0, allInstances.Length);
            situationInstance = allInstances[index].gameObject;
            for (int i = 0; i < allInstances.Length; i++)
            {
                if (i == index)
                {
                    allInstances[i].gameObject.SetActive(true);
                }
                else
                {
                    allInstances[i].gameObject.SetActive(false);
                }
            }

            _spawnLocations = null;
            _startLocations = null;
            _targetLocations = null;
            foreach (Transform child in situationInstance.transform)
            {
                if (child.name == "SpawnLocations")
                {
                    _spawnLocations = child.gameObject;
                }
                else if (child.name == "RobotStart")
                {
                    _startLocations = child.gameObject;
                }
                else if (child.name == "RobotTarget")
                {
                    _targetLocations = child.gameObject;
                }
            }

            if (_spawnLocations == null)
            {
                throw new System.Exception("No spawn location found in Handcrafted Social Situation " + situationInstance.name);
            }
            if (_startLocations == null)
            {
                throw new System.Exception("No start location found in Handcrafted Social Situation " + situationInstance.name);
            }
            if (_targetLocations == null)
            {
                throw new System.Exception("No target location found in Handcrafted Social Situation " + situationInstance.name);
            }

            agentManager = (Agents.Handcrafted)Agents.BaseAgentManager.instance;
            agentManager.NewScenario(socialSituation, socialSituationEnv, _spawnLocations);

            // Set the robot start and goal location
            // Special case leavegroup at a membership position
            if (socialSituation == SocialSituation.LeaveGroup && agentManager.openGroupLocation != Pose.identity)
            {
                start.position = agentManager.openGroupLocation.position;
                start.rotation = agentManager.openGroupLocation.rotation;
            }
            else
            {
                Transform[] startTransform = _startLocations.transform.Cast<Transform>().ToArray();
                if (startTransform.Length > 0)
                {
                    Transform startT = startTransform[UnityEngine.Random.Range(0, startTransform.Length)].gameObject.transform;
                    start.position = startT.position;
                    start.rotation = startT.rotation;
                }
                else
                {
                    start.position = _spawnLocations.transform.position;
                    start.rotation = _spawnLocations.transform.rotation;
                }
            }

            // Special case join group at a membership position
            if (socialSituation == SocialSituation.JoinGroup && agentManager.openGroupLocation != Pose.identity)
            {
                goal.position = agentManager.openGroupLocation.position;
                goal.position.y = 0.5f;
                goal.rotation = agentManager.openGroupLocation.rotation;
            }
            else
            {
                Transform[] targetTransform = _targetLocations.transform.Cast<Transform>().ToArray();
                if (targetTransform.Length > 0)
                {
                    Transform targetT = targetTransform[UnityEngine.Random.Range(0, targetTransform.Length)].gameObject.transform;
                    goal.position = targetT.position;
                    goal.position.y = 0.5f;
                    goal.rotation = targetT.rotation;
                }
                else
                {
                    goal.position = _targetLocations.transform.position;
                    goal.position.y = 0.5f;
                    goal.rotation = _targetLocations.transform.rotation;
                }
            }

            return true;
        }

        public override Trajectory.TrackedGroup[] groups
        {
            get
            {
                if (agentManager == null)
                {
                    return new Trajectory.TrackedGroup[0];
                }
                return agentManager.groups.ToArray();
            }
        }

        public override Trajectory.TrackedAgent[] agents
        {
            get
            {
                if (agentManager == null)
                {
                    return new Trajectory.TrackedAgent[0];
                }
                return agentManager.agents.ToArray();
            }
        }
    }
}