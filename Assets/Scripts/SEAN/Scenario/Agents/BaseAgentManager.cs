// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class BaseAgentManager : MonoBehaviour
    {
        private static BaseAgentManager _instance;
        public static BaseAgentManager instance { get { return _instance; } }

        public const int PATHFINDING_FREQ_IN_FRAMES = 10;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
    }

}
