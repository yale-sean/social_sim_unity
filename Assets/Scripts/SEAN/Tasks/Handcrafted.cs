// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Tasks
{
    public class Handcrafted : Base
    {
        public Scenario.PedestrianBehavior.SocialSituation socialSituation = Scenario.PedestrianBehavior.SocialSituation.Empty;

        protected override bool NewTask()
        {
            Scenario.PedestrianBehavior.Handcrafted scenario = (Scenario.PedestrianBehavior.Handcrafted)sean.pedestrianBehavior;
            // starts the scenario by spawning people and setting their destinations
            scenario.NewScenario(socialSituation);

            robotGoal.transform.position = scenario.goal.position;
            robotGoal.transform.rotation = scenario.goal.rotation;

            robotStart.transform.position = scenario.start.position;
            robotStart.transform.rotation = scenario.start.rotation;

            return true;
        }

    }
}
