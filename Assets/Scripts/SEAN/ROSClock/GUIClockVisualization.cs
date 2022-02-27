// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.ROSClock
{
    public class GUIClockVisualization : MonoBehaviour
    {
        private ROSClockPublisher publisher;

        private double deltaTime;

        private bool showFps = true;

        void Start()
        {
            publisher = GetComponent<ROSClockPublisher>();
            deltaTime = 0;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-showfps")
                {
                    showFps = args[i + 1] == "1";
                    Debug.Log("show fps: " + showFps);
                }
            }
        }

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            if (!showFps) { return; }
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.LowerLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            double msec = publisher.LastMSecs;
            double fps = 1.0f / deltaTime;
            string text = string.Format("clock: {0:0.0}, ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}
