// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 
using UnityEngine;

namespace SEAN.Metrics
{
    public class CountCollisions : MonoBehaviour
    {
        private bool ShowDebug = false;

        protected SEAN sean;

        private const float DebounceTime = 1f;
        private float debounceFallSeconds = 0f;

        protected float CollisionDistance = 0f;

        public void Start()
        {
            sean = SEAN.instance;
            // Setup Colliders
            if (GetComponents<CapsuleCollider>().Length != 1)
            {
                throw new System.Exception("CountCollisions can only be attached to objects with exactly one existing CapsuleCollider");
            }
            CapsuleCollider rigidCollider = GetComponent<CapsuleCollider>();
            // Personal Space
            CapsuleCollider personalSpaceCollider = gameObject.AddComponent<CapsuleCollider>();
            personalSpaceCollider.isTrigger = true;
            personalSpaceCollider.center = rigidCollider.center;
            personalSpaceCollider.radius = sean.metrics.PersonalDistance;
            // Intimate Space
            CapsuleCollider intimateSpaceCollider = gameObject.AddComponent<CapsuleCollider>();
            intimateSpaceCollider.isTrigger = true;
            intimateSpaceCollider.center = rigidCollider.center;
            intimateSpaceCollider.radius = sean.metrics.IntimateDistance;
            // Collision Space
            CapsuleCollider collisionSpaceCollider = gameObject.AddComponent<CapsuleCollider>();
            collisionSpaceCollider.isTrigger = true;
            collisionSpaceCollider.center = rigidCollider.center;
            collisionSpaceCollider.radius = rigidCollider.radius + 0.01f;
            CollisionDistance = collisionSpaceCollider.radius + 0.05f;
        }

        private void FixedUpdate()
        {
            if (debounceFallSeconds > 0f)
            {
                debounceFallSeconds -= Time.deltaTime;
                //print("debounceFallSeconds: " + debounceFallSeconds);
                return;
            }
        }

        /// <summary>
        /// Counts the number of collisions with other objects
        ///   hit parameter is the other collider
        /// Does not count a collision if the robot is not moving towards the object
        /// </summary>
        private void OnTriggerEnter(Collider hit)
        {
            // Ignore collisions w/ other triggers
            if (hit.isTrigger || debounceFallSeconds > 0f)
            {
                return;
            }
            Vector3 v = gameObject.GetComponent<Rigidbody>().velocity;
            // Handle a robot reset, don't count collisions while falling or w/in DebounceTime seconds
            if (System.Math.Round(v.y, 3) != 0f)
            {
                //print("Fall detected , pausing collision counts for " + DebounceTime + " seconds");
                debounceFallSeconds = DebounceTime;
                return;
            }

            // If the robot isn't moving, it can't be at fault.
            float vel = (float)System.Math.Round(v.magnitude, 3);
            // If the robot is oriented towards the other collider (this indicates they hit the robot)
            // positive dot product indicates an acute angle
            bool orientedTowardsOther = Vector3.Dot(v, hit.transform.position - gameObject.transform.position) > 0;

            bool isRobotAtFault = (vel != 0) && (orientedTowardsOther);

            if (!(hit.gameObject.tag.Equals(SEAN.AgentTag) || hit.gameObject.tag.Equals(SEAN.GroupTag)))
            {
                if (ShowDebug)
                {
                    print("At vel " + v + ", " + vel + " ObjectCollision with " + hit.gameObject.name + " is a trigger? " + hit.isTrigger);
                }
                sean.metrics.IncrementObjectCollisions();
                return;
            }

            double dist = Util.Geometry.GroundPlaneDist(hit.transform.position, gameObject.transform.position) - IVI.SFAgent.RADIUS;
            if (dist > sean.metrics.PersonalDistance)
            {
                return;
            }
            else if (dist > sean.metrics.IntimateDistance)
            {
                if (ShowDebug)
                {
                    print("At vel " + v + ", " + vel + " Personal Space Violation of " + dist + " meters with " + hit.gameObject.name + " is a trigger? " + hit.isTrigger);
                }
                sean.metrics.IncrementPersonalDistViolations(isRobotAtFault);
            }
            else if (dist > CollisionDistance)
            {
                if (ShowDebug)
                {
                    print("At vel " + v + ", " + vel + " Intimate Space Violation of " + dist + " meters with " + hit.gameObject.name + " is a trigger? " + hit.isTrigger);
                }
                sean.metrics.IncrementIntimateSpaceViolations(isRobotAtFault);
            }
            else
            {
                if (ShowDebug)
                {
                    print("At vel " + v + ", " + vel + " Collision Space Violation of " + dist + " meters with " + hit.gameObject.name + " is a trigger? " + hit.isTrigger);
                }
                sean.metrics.IncrementPeopleCollisions(isRobotAtFault);
            }
        }
    }
}
