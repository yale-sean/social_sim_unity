// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using System.Collections.Generic;

namespace SEAN.TF
{
    public class BaseTransformPublisher : MonoBehaviour
    {
        private ROSConnection ros;
        private RosMessageTypes.Std.MTime LastHeader = new RosMessageTypes.Std.MTime();

        protected void Start()
        {
            ros = ROSConnection.instance;
        }

        public class NamedTransform
        {
            public string name;
            public RosMessageTypes.Geometry.MPoseStamped pose;

            public NamedTransform(string name, RosMessageTypes.Geometry.MPoseStamped pose)
            {
                this.name = name;
                this.pose = pose;
            }
        }

        protected void PublishIfNew(NamedTransform transform)
        {
            List<NamedTransform> transforms = new List<NamedTransform>();
            transforms.Add(transform);
            PublishIfNew(transforms);
        }

        protected void PublishIfNew(List<NamedTransform> transforms)
        {
            foreach (NamedTransform transform in transforms)
            {
                SEAN.instance.clock.UpdateMHeader(transform.pose.header);
                if (LastHeader.secs >= transform.pose.header.stamp.secs && LastHeader.nsecs >= transform.pose.header.stamp.nsecs)
                {
                    return;
                }
                string name = transform.name;
                if (!name.StartsWith("/"))
                {
                    name = "/" + name;
                }
                ros.Send(name, transform.pose);
            }
            LastHeader.secs = transforms[0].pose.header.stamp.secs;
            LastHeader.nsecs = transforms[0].pose.header.stamp.nsecs;
        }
    }
}
