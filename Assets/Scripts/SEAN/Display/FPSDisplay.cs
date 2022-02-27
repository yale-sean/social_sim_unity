// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Display
{
    public class FPSDisplay : MonoBehaviour
    {
        void OnGUI()
        {
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.LowerLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            //print("GUI Publishing: " + situations.empty.name + ": " + situations.empty.val);
            //print("GUI Publishing: " + situations.leaveGroup.name + ": " + situations.leaveGroup.val);
            string text = string.Format("fps: {0}", (int)(1.0f / Time.smoothDeltaTime));
            GUI.Label(rect, text, style);
        }
    }
}