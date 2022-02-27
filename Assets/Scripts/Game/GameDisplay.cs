/*
    Copyright (c) 2020, Members of Yale Interactive Machines Group, Yale University,
    Mohamed Hussein
    All rights reserved.
    This source code is licensed under the BSD-style license found in the
    LICENSE file in the root directory of this source tree. 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour
{
    private Rect mainRect = new Rect(0, 0, 1, 1);
    private Rect leftMiniRect = new Rect(0.03f, 0.72f, 0.25f, 0.25f);
    private Rect rightMiniRect = new Rect(0.72f, 0.72f, 0.25f, 0.25f);

    public Camera mainDisplay;
    public bool showMiniDisplays = false;
    public Camera leftMiniDisplay;
    public Camera rightMiniDisplay;
    public GameObject miniDisplaysBackground;

    void Start()
    {
        mainDisplay.rect = mainRect;
        mainDisplay.depth = 0;

        leftMiniDisplay.rect = leftMiniRect;
        rightMiniDisplay.rect = rightMiniRect;

        if (showMiniDisplays)
        {
            leftMiniDisplay.depth = 2;
            rightMiniDisplay.depth = 2;
            miniDisplaysBackground.SetActive(true);
        }
        else
        {
            leftMiniDisplay.depth = -2;
            rightMiniDisplay.depth = -2;
            miniDisplaysBackground.SetActive(false);
        }
    }
}
