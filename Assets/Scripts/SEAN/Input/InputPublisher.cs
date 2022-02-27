// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Input
{
    public class InputPublisher : MonoBehaviour
    {
        private ROSConnection ros;
        public string TopicCmdVel = "/social_sim/cmd_vel";
        public string TopicTrigger = "/social_sim/trigger";

        /// <summary>
        ///  enable local joystick input
        /// </summary>
        public bool EnableJoystick = true;
        /// <summary>
        ///  enable local keyboard input
        /// </summary>
        public bool EnableKeyboard = true;

        public bool LocalInput
        {
            get
            {
                return EnableJoystick || EnableKeyboard;
            }
        }

        /// <summary>
        ///  scale joystick input by this amount
        /// </summary>
        public float JoystickScaleLinear = -0.5f;
        public float JoystickScaleAngular = -1.7f;

        /// <summary>
        ///  keyboard inputs translate directly to this output value
        /// </summary>
        public float FixedScaleLinear = -0.5f;
        public float FixedScaleAngular = 0.5f;

        private float _horizontal = 0;
        public float Horizontal { get { return _horizontal; } }
        private float _vertical = 0;
        public float Vertical { get { return _vertical; } }
        private bool _l1 = false;
        public bool L1 { get { return _l1; } }

        void Start()
        {
            ros = ROSConnection.instance;
        }

        void Update()
        {
            if (EnableJoystick && UnityEngine.Input.GetJoystickNames().Length > 0)
            {
                ReadJoystick();
            }
            else if (EnableKeyboard)
            {
                ReadKeyboard();
            }
        }

        void ReadKeyboard()
        {
            if (UnityEngine.Input.GetKey(KeyCode.Space))
            {
                _l1 = true;
            }
            else
            {
                _l1 = false;
            }
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                _horizontal = -FixedScaleAngular;
            }
            else if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                _horizontal = FixedScaleAngular;
            }
            else
            {
                _horizontal = 0;
            }
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
            {
                _vertical = -FixedScaleLinear;
            }
            else if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
            {
                _vertical = FixedScaleLinear;
            }
            else
            {
                _vertical = 0;
            }
            Send();
        }

        void ReadJoystick()
        {
            // Set in project settings:
            // Button mapping (mac?):
            // 1: left analog stick
            // 2: right analog stick
            // 3: start button
            // 4: d-pad up
            // 5: d-pad right
            // 6: d-pad down
            // 7: d-pad left
            // 8: l2 trigger
            // 9: r2 trigger
            // 10: l1 trigger
            // 11: r1 trigger
            //https://forum.unity.com/threads/ps3-button-map.89288/

            // Button mapping (linux?):
            // 4: l1
            _horizontal = UnityEngine.Input.GetAxis("RHorizontal") * JoystickScaleAngular;
            _vertical = UnityEngine.Input.GetAxis("RVertical") * JoystickScaleLinear;
            _l1 = UnityEngine.Input.GetAxis("L1") != 0;
            Send();
        }

        public RosMessageTypes.Geometry.MTwist CmdVel
        {
            get
            {
                // cmd vel
                RosMessageTypes.Geometry.MTwist twist = new RosMessageTypes.Geometry.MTwist();
                twist.linear.x = Vertical;
                twist.angular.z = Horizontal;
                return twist;
            }
        }

        private void Send()
        {
            // trigger
            RosMessageTypes.Std.MBool b = new RosMessageTypes.Std.MBool();
            b.data = L1;
            ros.Send(TopicTrigger, b);
            // cmd vel
            ros.Send(TopicCmdVel, CmdVel);
        }
    }
}
