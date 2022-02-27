// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Display
{
    public class DisableRosPanel : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            ROSConnection.instance.ShowHud = false;
        }

    }
}
