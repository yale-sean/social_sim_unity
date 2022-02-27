// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Trajectory
{
    public class TrackedTrajectory : MonoBehaviour
    {
        /// History of poses
        private LinearTrajectory _trajectory;
        public LinearTrajectory trajectory { get { return _trajectory; } }

        // Use the default game object unless mainGameObject is set (e.g. for the Robot's base_link)
        private GameObject _mainGameObject;
        public GameObject mainGameObject
        {
            get
            {
                if (_mainGameObject)
                {
                    return _mainGameObject;
                }
                return gameObject;
            }
            set { _mainGameObject = value; }
        }

        #region Parameters

        private bool ShowDebug = false;
        public float TrajectoryDeltaSec = 0.5f;
        public int TrajectoryPoints = 30;

        #endregion

        private float TrajectoryGapTimeSec = 0f;

        public Vector3 position
        {
            get
            {
                return mainGameObject.transform.position;
            }
        }
        public Vector3 orientation
        {
            get
            {
                return mainGameObject.transform.position;
            }
        }
        public float velocity
        {
            get
            {
                return trajectory.velocity;
            }
        }
        public Vector2 vector
        {
            get
            {
                return trajectory.vector;
            }
        }

        // check if a game object is looking at a point
        public bool lookingAt(Vector3 groupCenter, float degreesEpsilon = 45.0f)
        {
            // left hand coordinates, x and z are the ground plane, effectively z = y
            float deg = Vector2.Angle(trajectory.vector, new Vector2(groupCenter.x, groupCenter.z) - trajectory.position);
            return deg < degreesEpsilon;
        }

        public bool movingTowards(Vector3 point, float minVelocity = 0.5f)
        {
            return lookingAt(point) && trajectory.velocity > minVelocity;
        }

        /// <summary>
        /// TrackedTrajectories|Agents|Robots near this game object
        ///   Must have an attached collider and script of the appropriate type
        /// </summary>
        public List<TrackedAgent> nearbyAgents(float Threshold)
        {
            List<TrackedAgent> elements = new List<TrackedAgent>();
            if (ShowDebug)
            {
                Debug.DrawLine(position, position + new Vector3(Threshold, 0, 0), Color.red, 1f);
            }

            // Ignoring triggers to 
            Collider[] colliders = Physics.OverlapSphere(position, Threshold, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider collider in colliders)
            {
                GameObject go = collider.gameObject;
                Component component;
                //if (go.TryGetComponent(typeof(TrackedAgent), out component) && !(component is IVI.RobotAgent)) {
                if (go.TryGetComponent(typeof(TrackedAgent), out component))
                {
                    //print("object: " + go.name + " is an agent with component: " + component.GetType());
                    elements.Add(component as TrackedAgent);
                }
            }
            return elements;
        }

        public float Angle(TrackedTrajectory b)
        {
            //print("(traj) a: " + vector + ", b: " + b.vector);
            return Vector2.Angle(vector, b.vector);
        }

        public float Angle(Vector2 b)
        {
            //print("(point) a: " + vector + ", b: " + b);
            return Vector2.Angle(vector, b);
        }

        public virtual void Start()
        {
            _trajectory = new LinearTrajectory(TrajectoryPoints);
        }

        public virtual void Update()
        {
            // $$$ Roll float over?
            TrajectoryGapTimeSec += Time.deltaTime;
            if (TrajectoryGapTimeSec > TrajectoryDeltaSec)
            {
                trajectory.Add(mainGameObject.transform);
                if (trajectory.hasPoses && ShowDebug)
                {
                    Vector3 debugVector = new Vector3(trajectory.vector.x, 0, trajectory.vector.y);
                    Debug.DrawLine(position, position + debugVector, Color.red);
                    //Debug.DrawRay(transform.position, forward, Color.green);
                }
            }
        }
    }
}