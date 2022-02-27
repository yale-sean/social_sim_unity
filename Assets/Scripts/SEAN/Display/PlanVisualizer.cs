// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;
using SEAN.Display.VolumetricLine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using System.Linq;

namespace SEAN.Display
{
    public class PlanVisualizer : MonoBehaviour
    {
        private SEAN sean;

        public string Topic;
        public Color LineColor;
        public float waitSec = 0.25f;
        public float pThresh = 0.5f;
        private ulong stamp;
        private ulong prevStamp;

        private RosMessageTypes.Nav.MPath message;
        private bool started = false;

        // settings for rendering path
        public Material LightSaberMaterial;

        public int SampledPath = 25; // no matter the length of the path, only sample [pathLength] points
        private int pathLength;

        private List<Vector3> pathPositions;
        private Vector3[] renderPathPositions;

        VolumetricLineStripBehavior lineStripBehavior;

        void Awake()
        {
            // Make sure we catch the first global plan message
            ROSConnection.instance.Subscribe<RosMessageTypes.Nav.MPath>(Topic, ReceiveMessage);
        }

        void Start()
        {
            pathPositions = new List<Vector3>();
            sean = SEAN.instance;
            lineStripBehavior = gameObject.GetComponent<VolumetricLineStripBehavior>();
            if (lineStripBehavior == null)
            {
                lineStripBehavior = gameObject.AddComponent<VolumetricLineStripBehavior>();
                // TemplateMaterial must be set first!
                lineStripBehavior.TemplateMaterial = LightSaberMaterial;
                lineStripBehavior.LightSaberFactor = 1;
                lineStripBehavior.LineWidth = 0.2f;
                lineStripBehavior.LineColor = LineColor;
            }
            started = true;
            ProcessMessage();
        }

        void ReceiveMessage(RosMessageTypes.Nav.MPath message)
        {
            this.message = message;
            ProcessMessage();
        }

        void EnableLineStrip(bool enable)
        {
            lineStripBehavior.enabled = enable;
        }

        void ProcessMessage()
        {
            if (!started)
            {
                return;
            }
            if (message == null)
            {
                return;
            }
            stamp = message.header.stamp.secs;
            if (prevStamp == null || stamp - prevStamp < waitSec)
            {
                return;
            }
            pathPositions.Clear();
            if (message.poses.Length > 2)
            {
                Vector3 lastP = Vector3.zero;
                for (int i = 0; i < message.poses.Length - 1; i++)
                {
                    Vector3 p = message.poses[i].pose.position.From<FLU>();
                    p.y = sean.robot.position.y;
                    double dist = Vector3.Distance(lastP, p);
                    if (dist > pThresh)
                    {
                        pathPositions.Add(p);
                        lastP = p;
                    }
                    if (pathPositions.Count == SampledPath)
                    {
                        break;
                    }
                }
                for (int i = pathPositions.Count; i < SampledPath; i++)
                {
                    pathPositions.Add(lastP);
                }
                renderPathPositions = pathPositions.ToArray<Vector3>();
                lineStripBehavior.UpdateLineVertices(renderPathPositions);
                EnableLineStrip(true);
            }
            else
            {
                EnableLineStrip(false);
            }
            prevStamp = stamp;
        }
    }
}
