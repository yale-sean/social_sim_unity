// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 
using UnityEngine;

namespace SEAN.Input
{
    [RequireComponent(typeof(InputPublisher))]
    public class GUIInputVisualization : MonoBehaviour
    {
        public bool showControl = true;
        private InputPublisher control;
        void Start()
        {
            control = GetComponent<InputPublisher>();
            //string[] args = System.Environment.GetCommandLineArgs();
            //for (int i = 0; i < args.Length; i++)
            //{
            //    if (args [i] == "-showjoystick")
            //    {
            //        showJoystick = args [i + 1] == "1";
            //        Debug.Log("show joystick: " + showJoystick);
            //    }
            //}
        }

        void OnGUI()
        {
            if (!showControl) { return; }
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.LowerRight;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            string text = string.Format("(L1: {0}) Horizontal: {1}, Vertical: {2}", control.L1, control.Horizontal, control.Vertical);
            GUI.Label(rect, text, style);
        }
    }
}