/*
© Siemens AG, 2018
Author: Berkay Alp Cakal (berkay_alp.cakal.ct@siemens.com)
Modified and incorporated into SEAN by Nathan Tsoi and the Yale Interactive Machines Group

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Sensors
{
    [RequireComponent(typeof(LaserScanner))]
    public class LaserScanPublisher : MonoBehaviour
    {
        ROSConnection ros;
        public string Topic = "/laser";
        private LaserScanner laserScanner;
        public string FrameId = "laser";

        private RosMessageTypes.Sensor.MLaserScan message;
        private float scanPeriod;
        private float previousScanTime = 0;

        void Start()
        {
            ros = ROSConnection.instance;
            laserScanner = GetComponent<LaserScanner>();
            message = laserScanner.InitializeMessage(FrameId);
            scanPeriod = laserScanner.ScanPeriod();
        }

        private void FixedUpdate()
        {
            if (Time.realtimeSinceStartup >= previousScanTime + scanPeriod)
            {
                UpdateMessage();
                previousScanTime = Time.realtimeSinceStartup;
            }
        }

        private void UpdateMessage()
        {
            SEAN.instance.clock.UpdateMHeader(message.header);
            message.ranges = laserScanner.Scan();
            ros.Send(Topic, message);
        }
    }

}