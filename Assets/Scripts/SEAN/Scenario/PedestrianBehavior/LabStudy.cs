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
    public class LabStudy : Base
    {

        public GameObject AgentPrefab;

        private Dictionary<string, GameObject> _positions = new Dictionary<string, GameObject>();
        Trajectory.TrackedAgent _agent;
        private SEAN sean;

        override public string scenario_name { get { return "LabStudy"; } }

        public void Awake()
        {
            sean = SEAN.instance;
            _agent = new Trajectory.TrackedAgent();
            GameObject positions = transform.Cast<Transform>().ToArray()[0].gameObject;
            foreach (Transform position in positions.transform)
            {
                _positions.Add(position.name, position.gameObject);
            }
        }

        public Dictionary<string, GameObject> positions
        {
            get
            {
                return _positions;
            }
        }

        public void AgentStart(Transform start)
        {
            GameObject agent = Instantiate(AgentPrefab, Vector3.up, Quaternion.identity);
            agent.name = "Player";
            agent.transform.parent = transform;
            // agent = agent.transform.GetChild(0).gameObject;
        }

        public void AgentGoal(Transform goalTransform)
        {
            // goal.transform.position = goalTransform;
            // TODO
            // goal.transform.rotation = GetRandomRotation();
        }

        public override Trajectory.TrackedGroup[] groups
        {
            get
            {
                return new Trajectory.TrackedGroup[0];
            }
        }

        public override Trajectory.TrackedAgent[] agents
        {
            get
            {
                if (!_agent)
                {
                    return new Trajectory.TrackedAgent[] { };
                }
                return new Trajectory.TrackedAgent[] { _agent };
            }
        }
    }
}