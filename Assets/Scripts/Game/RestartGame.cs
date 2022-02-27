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

public class RestartGame : MonoBehaviour
{
    public bool scenariosFinished = false;
    public float timeLimit = 240;

    public List<string> scenarios = new List<string>();
    public List<int> robotLocations = new List<int>();
    public List<int> targetLocations = new List<int>();

    public bool started = false;

    public string token = "";
    public string tokenTimeout = "";

    public string windowmode = "";

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        started = true;
    }
}
