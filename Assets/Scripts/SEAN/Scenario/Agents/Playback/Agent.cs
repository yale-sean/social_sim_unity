using System;
using UnityEngine;

namespace SEAN.Scenario.Agents.Playback
{
    public class Agent : Base
    {

        override protected void Start()
        {
            base.Start();
            collisionCapsule.radius = 0.05f;
        }

        public void UpdateVelocity(Pose pose)
        {
            //agents[id].transform.rotation = pose.rotation;
            //agents[id].transform.position = pose.position;
            // TODO: assign velocity
            velocity = pose.position - gameObject.transform.position;
            //destPos = pose.position;
            //Debug.Log(gameObject.name + ": " + velocity + ", goal: " + destPos);
        }

        /// <summary>
        /// Returns the velocity computed by the overloaded UpdateVelocity(Pose pose) method
        /// </summary>
        /// <returns></returns>
        protected override Vector3 UpdateVelocity()
        {
            return velocity;
        }

        protected override void StopNavigation()
        {
            // Do nothing
        }
    }
}
