// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario
{
    public class Robot : MonoBehaviour
    {
        public float radius = 0.16f;
        public GameObject base_link;
        public Camera camera_first;
        public Camera camera_third;
        public Camera camera_overhead;

        public Trajectory.TrackedTrajectory trajectory { get; private set; }
        private void GetOrAttachTrajectory()
        {
            if (trajectory != null) { return; }
            trajectory = gameObject.GetComponent<Trajectory.TrackedTrajectory>();
            if (trajectory == null)
            {
                trajectory = gameObject.AddComponent(typeof(Trajectory.TrackedTrajectory)) as Trajectory.TrackedTrajectory;
                trajectory.mainGameObject = base_link;
            }
        }
        public void Start()
        {
            GetOrAttachTrajectory();
            if (camera_first == null)
            {
                throw new System.ArgumentException("A first person camera must be assigned to the robot " + name);
            }
            if (camera_third == null)
            {
                throw new System.ArgumentException("A third person camera must be assigned to the robot " + name);
            }
            if (camera_overhead == null)
            {
                throw new System.ArgumentException("A overhead camera must be assigned to the robot " + name);
            }
        }
        public new Transform transform
        {
            get
            {
                return base_link.transform;
            }
        }
        public Vector3 position
        {
            get
            {
                return transform.position;
            }
        }
        public Quaternion rotation
        {
            get
            {
                return transform.rotation;
            }
        }
        public override string ToString()
        {
            return gameObject.name;
        }
    }
}