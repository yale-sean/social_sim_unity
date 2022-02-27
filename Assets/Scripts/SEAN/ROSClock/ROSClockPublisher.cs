// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.ROSClock
{
    public class ROSClockPublisher : MonoBehaviour
    {
        ROSConnection ros;
        public string topicName = "clock";

        private static ROSClockPublisher _instance;
        public static ROSClockPublisher instance { get { return _instance; } }

        public static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public double StartMillis { get; private set; }


        public double LastMSecs { get; private set; }
        private RosMessageTypes.Std.MTime lastPublishedTime;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                //print("TIMESCALE: " + Time.timeScale);
                _instance = this;
                StartMillis = (DateTime.Now.ToUniversalTime() - UNIX_EPOCH).TotalMilliseconds;
                //print("startMillis: " + startMillis);
                lastPublishedTime = this.Now();
            }
        }

        void Start()
        {
            ros = ROSConnection.instance;
        }

        void FixedUpdate()
        {
            instance.Publish();
        }

        public RosMessageTypes.Std.MTime Now()
        {
            return Util.Time.MTime(Util.Time.Milliseconds(StartMillis));
        }

        public RosMessageTypes.Std.MTime LastPublishedTime()
        {
            return lastPublishedTime;
        }

        public void Publish()
        {
            double msecs = Util.Time.Milliseconds(StartMillis);
            if (LastMSecs == msecs)
            {
                return;
            }
            LastMSecs = msecs;
            RosMessageTypes.Std.MTime t = instance.Now();
            RosMessageTypes.Rosgraph.MClock message = new RosMessageTypes.Rosgraph.MClock();
            message.clock.secs = t.secs;
            message.clock.nsecs = t.nsecs;
            //print("clock: " + t.secs + "." + t.nsecs);
            ros.Send(topicName, message);
            lastPublishedTime = t;
        }

        public void UpdateMHeader(RosMessageTypes.Std.MHeader header)
        {
            UpdateMHeaders(new List<RosMessageTypes.Std.MHeader>() { header });
        }

        public void UpdateMHeaders(List<RosMessageTypes.Std.MHeader> headers)
        {
            foreach (RosMessageTypes.Std.MHeader header in headers)
            {
                header.seq++;
                header.stamp.secs = lastPublishedTime.secs;
                header.stamp.nsecs = lastPublishedTime.nsecs;
            }
        }
    }
}