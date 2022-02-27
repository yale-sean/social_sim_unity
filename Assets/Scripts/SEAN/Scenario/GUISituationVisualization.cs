// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario
{
    [RequireComponent(typeof(Classifier.SituationClassifier))]
    public class GUISituationVisualization : MonoBehaviour
    {
        public bool showSituations = true;
        private Classifier.SituationClassifier situations;
        void Start()
        {
            situations = GetComponent<Classifier.SituationClassifier>();
        }

        void OnGUI()
        {
            if (!showSituations) { return; }
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(45, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.LowerLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            //print("GUI Publishing: " + situations.empty.name + ": " + situations.empty.val);
            //print("GUI Publishing: " + situations.leaveGroup.name + ": " + situations.leaveGroup.val);
            string text = string.Format("({0:0.00}) E:{1}|C:{2}|D:{3}|J:{4}|L:{5}",
                situations.lastUpdateTime,
                situations.empty.val,
                situations.crossPath.val,
                situations.downPath.val,
                situations.joinGroup.val,
                situations.leaveGroup.val
            );
            GUI.Label(rect, text, style);
        }
    }
}