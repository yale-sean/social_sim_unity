// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using System;
using System.Linq;
using MathNet.Numerics;

namespace SEAN.Scenario.Trajectory
{
    public class LinearTrajectory
    {
        /// <summary>
        ///  Constant Params
        /// </summary>
        private float MinMagnitude = 0.05f;
        private int MinNumPoses = 3;

        private Util.CappedQueue<Util.PoseStamped> _poses;
        public Util.PoseStamped[] poses { get { return _poses.ToArray(); } }
        // Need at least 3 poses
        public bool hasPoses { get { return _poses.Count > MinNumPoses; } }
        private bool _needs_recalc = true;
        private float _delta_t;
        private float _velocity;
        public float velocity
        {
            get
            {
                if (_needs_recalc) { Recalc(); }
                return _velocity;
            }
        }
        private Vector2 _vector;
        public Vector2 vector
        {
            get
            {
                if (_needs_recalc) { Recalc(); }
                return _vector;
            }
        }
        public Vector2 position
        {
            get;
            private set;
        }
        public LinearTrajectory(int TrajectoryPoints)
        {
            _poses = new Util.CappedQueue<Util.PoseStamped>(TrajectoryPoints);
        }
        /// <summary>Add a point to the fixed size queue. Call this method on some regular interval for a path composed of points evenly spaced in time.</summary>
        public void Add(Transform transform)
        {
            _needs_recalc = true;
            position = new Vector2(transform.position.x, transform.position.z);
            Util.PoseStamped p = new Util.PoseStamped();
            p.time = Time.time;
            p.position.x = transform.position.x;
            p.position.y = transform.position.y;
            p.position.z = transform.position.z;
            p.rotation.w = transform.rotation.w;
            p.rotation.x = transform.rotation.x;
            p.rotation.y = transform.rotation.y;
            p.rotation.z = transform.rotation.z;
            _poses.Enqueue(p);
        }

        /// <summary>
        ///  Ordinary least squares fit recent points
        /// </summary>
        private bool OLS()
        {
            // TODO: (per sam) use average of recent velocities instead
            if (!hasPoses)
            {
                _velocity = 0;
                _vector = Vector2.zero;
                return false;
            }
            // left hand coordinates, x and z are the ground plane, effectively y = z
            // NOTE: first element is the oldest!
            Util.PoseStamped[] poses = _poses.ToArray();
            double[] x = new double[poses.Length];
            double[] y = new double[poses.Length];
            float[] t = new float[poses.Length];
            for (int i = 0; i < poses.Length; i++)
            {
                t[i] = poses[i].time;
                x[i] = poses[i].position.x;
                y[i] = poses[i].position.z;
            }
            Tuple<double, double> p = Fit.Line(x, y);
            // slope intercept form: y = mx + b
            // intercept
            double b = p.Item1;
            // slope
            double m = p.Item2;
            float x_0 = (float)x.Min();
            float x_1 = (float)x.Max();
            float y_0 = (float)(m * x.Min() + b);
            float y_1 = (float)(m * x.Max() + b);
            Vector2 v = new Vector2(x_1 - x_0, y_1 - y_0);
            if (float.IsNaN(v.magnitude) || v.magnitude < MinMagnitude)
            {
                _velocity = 0;
                _vector = Vector2.zero;
                return false;
            }
            // find the points on our line corresponding to the first and last pose
            // first pose is the last element of the array
            if (poses[poses.Length - 1].position.x > poses[0].position.x)
            {
                v.x *= -1;
            }
            if (poses[poses.Length - 1].position.y > poses[0].position.y)
            {
                v.y *= -1;
            }
            // compute delta time
            _delta_t = t.Max() - t.Min();
            //Debug.Log("magnitude: " + v.magnitude + ", delta_t: " + _delta_t);
            _velocity = (float)(v.magnitude / _delta_t);
            _vector = v;
            return true;
        }

        private void Recalc()
        {
            if (!_needs_recalc) { return; }
            if (OLS())
            {
                _needs_recalc = false;
            }
            // Try instead computing average velocity of recent points
            //AverageVelocity();
        }
    }
}