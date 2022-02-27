// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Trajectory
{
    public class TrackedGroup : MonoBehaviour
    {
        public float SearchRadius = 1.7f;
        public float radius { get; private set; }
        public Vector3 center { get { return gameObject.transform.position; } }
        public static TrackedGroup GetOrAttach(GameObject go)
        {
            TrackedGroup group = go.GetComponent<TrackedGroup>();
            if (group == null)
            {
                group = go.AddComponent(typeof(TrackedGroup)) as TrackedGroup;
            }
            return group;
        }

        ///   - [Group Membership] given an o-space center location
        ///     - [Distance] Find all people within 0.7 to 1.2 (average) to 1.7m radius of the o-space center
        ///     - [Orientation] Looking at o-space center w/in 45 degrees
        ///     - [Velocity] with a velocity of less than walking (1.5m/s)
        public List<TrackedTrajectory> GroupMembers(float movingVelocity = 1.5f, float degreesEpsilon = 45.0f)
        {
            radius = 0f;
            Vector3 groupCenter = gameObject.transform.position;
            List<TrackedTrajectory> tracks = new List<TrackedTrajectory>();
            foreach (Collider collider in Physics.OverlapSphere(groupCenter, SearchRadius))
            {
                TrackedTrajectory track = collider.gameObject.GetComponent<TrackedTrajectory>();
                if (track == null)
                {
                    continue;
                }
                if (track.velocity >= movingVelocity)
                {
                    continue;
                }
                if (!track.lookingAt(groupCenter, degreesEpsilon: degreesEpsilon))
                {
                    continue;
                }
                radius = Mathf.Max(radius, Vector3.Distance(groupCenter, collider.gameObject.transform.position));
                tracks.Add(track);
            }
            return tracks;
        }

        public bool GroupMemberLocationGenerator(out Vector3 position, out Quaternion rotation, float defaultRadius = 0.5f)
        {
            position = Vector3.zero;
            List<TrackedTrajectory> tracks = GroupMembers();

            // place the new member in the largest gap
            if (tracks.Count == 0)
            {
                position = center + Vector3.Normalize(Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward) * defaultRadius;
            }
            else if (tracks.Count == 1)
            {
                position = center + Vector3.Normalize(tracks[0].position - center) * -1 * radius;
            }
            else
            {
                // Implements Yang's method
                float greatest_angle = 0;
                for (int i = 0; i < tracks.Count; i++)
                {
                    for (int j = i + 1; j < tracks.Count; j++)
                    {
                        if (i == j) { continue; }
                        Vector3 a = tracks[i].position - center;
                        a.y = 0;
                        Vector3 b = tracks[j].position - center;
                        b.y = 0;
                        if (greatest_angle < Vector3.Angle(a, b))
                        {
                            greatest_angle = Vector3.Angle(a, b);
                            Vector3 middle = Vector3.Normalize(b - a) * ((b - a).magnitude / 2);
                            position = center + Vector3.Normalize(middle - center) * radius;
                        }
                    }
                }
            }

            rotation = Quaternion.LookRotation(center - position);

            return true;
        }

        private float UpdateRadius()
        {
            GroupMembers();
            return radius;
        }

        public bool ContainsTransform(Transform transform)
        {
            float r = UpdateRadius();
            Vector3 a = center;
            a.y = 0;
            Vector3 b = transform.position;
            b.y = 0;
            //print(name + " ContainsTransform (Distance(" + a + ", " + b + ") = " + Vector3.Distance(a, b) + ") <= " + r);
            return Vector3.Distance(a, b) <= r * 2;
        }
    }
}