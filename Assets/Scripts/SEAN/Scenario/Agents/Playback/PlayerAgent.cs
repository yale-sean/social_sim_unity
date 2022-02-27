using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class PlayerAgent : Base
    {

        public Trajectory.TrackedTrajectory trajectory { get; private set; }

        private SEAN sean;

        private void GetOrAttachTrajectory()
        {
            if (trajectory != null) { return; }
            trajectory = gameObject.GetComponent<Trajectory.TrackedTrajectory>();
            if (trajectory == null)
            {
                trajectory = gameObject.AddComponent(typeof(Trajectory.TrackedTrajectory)) as Trajectory.TrackedTrajectory;
            }
        }
        /// <summary>
        ///  Remove the rocketbox random avatar gameobject from the hierarchy
        /// </summary>
        private void BuildPlayerObject()
        {
            GameObject rocketBoxRandom = gameObject.transform.parent.gameObject;
            gameObject.transform.parent = gameObject.transform.parent.parent;
            Destroy(rocketBoxRandom);
        }
        private void AttachCameras()
        {
            GameObject camerasContainer = null;
            List<GameObject> cameras = new List<GameObject>();
            foreach (Transform child in gameObject.transform.parent)
            {
                if (child.name == "Cameras")
                {
                    camerasContainer = child.gameObject;
                    foreach (Transform camera in child)
                    {
                        cameras.Add(camera.gameObject);
                    }
                }
            }
            foreach (GameObject camera in cameras)
            {
                camera.transform.parent = gameObject.transform;
            }
            if (camerasContainer != null)
            {
                Destroy(camerasContainer);
            }
        }
        protected override void Start()
        {
            sean = SEAN.instance;
            GetOrAttachTrajectory();
            BuildPlayerObject();
            AttachCameras();
            base.Start();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        public new Transform transform
        {
            get
            {
                return gameObject.transform;
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

        // Called once per game loop. Updates the pedestrian's desired velocity
        protected override Vector3 UpdateVelocity()
        {
            Vector3 vel = Vector3.zero;
            // If input is enabled (spacebar or L1 trigger)
            if (sean.input.L1)
            {
                vel = sean.input.Vertical * transform.forward;
                vel.y = -sean.input.Horizontal;
            }
            return vel;
        }

        protected override void StopNavigation()
        {
            // do nothing :)
        }
    }
}
