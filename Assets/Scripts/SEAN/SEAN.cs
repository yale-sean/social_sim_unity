// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using System.Collections.Generic;
using System.Linq;

namespace SEAN
{

    [ExecuteAlways]
    public class SEAN : MonoBehaviour
    {
        private static SEAN _instance;
        public static SEAN instance { get { return _instance; } }

        public ROSClock.ROSClockPublisher clock { get { return ROSClock.ROSClockPublisher.instance; } }

        public const string AgentTag = "NavAgent";
        public const string GroupTag = "GroupMember";

        public Scenario.Agents.LowLevelControl AgentController;
        public Scenario.Agents.ControlledAgent ControlledAgent;

        public bool TopDownViewOnly = false;
        public bool PlayerControl = false;
        public bool EvaluationMode = false;

        private GameObject _SEAN, _Environment, _RobotTasks, _PedestrianBehaviors, _Robots, _Players, _Controllers, _StartAndGoal;
        public Environment.Environment environment { get; private set; }
        private Scenario.PedestrianBehavior.Base _pedestrianBehavior = null;
        private Tasks.Base _task = null;
        private Scenario.Robot _robot = null;
        private Scenario.Player _player = null;
        private Input.InputPublisher _input = null;
        public Input.InputPublisher input { get { return _input; } }
        private Control.ControlSubscriber _controller = null;
        private Metrics.Metrics _metrics = null;
        public Metrics.Metrics metrics { get { return _metrics; } }

        #region arguments
        public int RosConnectionPort = 10000;

        // TODO: configure other command line options
        private void ParseCommandLineArgs()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                string value = args[i + 1];
                if (args[i] == "-ros-tcp-port")
                {
                    RosConnectionPort = Int32.Parse(value);
                }
                else if (args[i] == "-evaluation-mode")
                {
                    EvaluationMode = true;
                }
                else if (args[i] == "-scenario")
                {
                    SetPedestrianBehavior(value);
                }
                else if (args[i] == "-task")
                {
                    SetTask(value);
                }
                else if (args[i] == "-task-social-situation")
                {
                    taskSocialSituation.socialSituation = (Scenario.PedestrianBehavior.SocialSituation)Enum.Parse(typeof(Scenario.PedestrianBehavior.SocialSituation), value);
                }
                else if (args[i] == "-taskID")
                {
                    // Assume -task arg comes first 
                    if (_task.name != "LabStudy")
                    {
                        throw new ArgumentException("-taskID flag can only be used with LabStudy task.");
                    }
                    robotTask.taskID = Int32.Parse(value);
                }
                else if (args[i] == "-completion-distance")
                {
                    robotTask.completionDistance = float.Parse(value);
                }
                else if (args[i] == "-max-num-tasks")
                {
                    robotTask.maximumNumberOfTasks = Int32.Parse(value);
                }
                else if (args[i] == "-task-timeout-seconds")
                {
                    robotTask.timeoutTaskSeconds = float.Parse(value);
                }
            }

        }
        #endregion arguments

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
            _SEAN = gameObject;
            _Environment = GameObject.Find("/Environment");
            if (_Environment == null)
            {
                throw new ArgumentException("Cannot find /Environment. Please make sure the top-level Environment object exists");
            }
            environment = _Environment.GetComponent<Environment.Environment>();
            if (environment == null)
            {
                throw new ArgumentException("No Environment script is attached to the /Environment object. Please attach this script.");
            }

            foreach (Transform child in _SEAN.transform)
            {
                if (child.name == "PedestrianBehaviors")
                {
                    _PedestrianBehaviors = child.gameObject;
                }
                else if (child.name == "RobotTasks")
                {
                    _RobotTasks = child.gameObject;
                }
                else if (child.name == "Robots")
                {
                    _Robots = child.gameObject;
                }
                else if (child.name == "Players")
                {
                    _Players = child.gameObject;
                }
                else if (child.name == "Controllers")
                {
                    _Controllers = child.gameObject;
                }
                else if (child.name == "Input")
                {
                    _input = child.gameObject.GetComponent<Input.InputPublisher>();
                }
                else if (child.name == "Metrics")
                {
                    _metrics = child.gameObject.GetComponent<Metrics.Metrics>();
                }
                else if (child.name == "StartAndGoal")
                {
                    _StartAndGoal = child.gameObject;
                }
            }

            if (ControlledAgent == Scenario.Agents.ControlledAgent.Player)
            {
                PlayerControl = true;
                player.gameObject.SetActive(true);
            }
            else
            {
                // deactive player by default
                player.gameObject.SetActive(false);
            }

            ParseCommandLineArgs();

#if !UNITY_EDITOR
            ROSConnection.instance.RosPort = RosConnectionPort;
#endif
        }

        public List<Scenario.PedestrianBehavior.Base> pedestrianBehaviors
        {
            get
            {
                List<Scenario.PedestrianBehavior.Base> _behaviors = new List<Scenario.PedestrianBehavior.Base>();
                foreach (Transform child in _PedestrianBehaviors.transform)
                {
                    Scenario.PedestrianBehavior.Base behavior = child.gameObject.GetComponent<Scenario.PedestrianBehavior.Base>();
                    if (behavior == null)
                    {
                        throw new Exception("All children of the PedstrianBehaviors object must have a PedestrianBeahvior.Basescript attached. " + child.gameObject.name + " does not.");
                    }
                    _behaviors.Add(behavior);
                }
                return _behaviors;
            }
        }


        public void SetPedestrianBehavior(string name)
        {
            bool found = false;
            foreach (Scenario.PedestrianBehavior.Base behavior in pedestrianBehaviors)
            {
                if (behavior.name == name)
                {
                    _pedestrianBehavior = behavior;
                    behavior.gameObject.SetActive(true);
                    found = true;
                }
                else
                {
                    behavior.gameObject.SetActive(false);
                }
            }
            if (!found)
            {
                throw new ArgumentException("Could not find scenario with name " + name + ", valid options are " + (from s in pedestrianBehaviors select s.name));
            }
        }

        public Scenario.PedestrianBehavior.Base pedestrianBehavior
        {
            get
            {
                if (_pedestrianBehavior != null)
                {
                    return _pedestrianBehavior;
                }
                int activeCount = 0;
                foreach (Scenario.PedestrianBehavior.Base scenario in pedestrianBehaviors)
                {
                    if (scenario.gameObject.activeSelf)
                    {
                        _pedestrianBehavior = scenario;
                        activeCount++;
                    }
                }
                if (activeCount != 1)
                {
                    Debug.LogWarning("More than 1 Scenario is active, using: " + _pedestrianBehavior.name);
                }
                if (activeCount == 0)
                {
                    Debug.LogWarning("No Scenario is active");
                }
                return _pedestrianBehavior;
            }
        }

        public List<Tasks.Base> robotTasks
        {
            get
            {
                List<Tasks.Base> _tasks = new List<Tasks.Base>();
                foreach (Transform child in _RobotTasks.transform)
                {
                    _task = child.gameObject.GetComponent<Tasks.Base>();
                    if (_task == null)
                    {
                        throw new Exception("All children of the RobotTasks object must have a Tasks.Base script attached. " + child.gameObject.name + " does not.");
                    }
                    _tasks.Add(_task);
                }
                return _tasks;
            }
        }

        public void SetTask(string name)
        {
            bool found = false;
            foreach (Tasks.Base task in robotTasks)
            {
                if (task.name == name)
                {
                    _task = task;
                    task.gameObject.SetActive(true);
                    found = true;
                }
                else
                {
                    task.gameObject.SetActive(false);
                }
            }
            if (!found)
            {
                throw new ArgumentException("Could not find task with name " + name + ", valid options are " + (from t in robotTasks select t.name));
            }
        }

        public Tasks.Base robotTask
        {
            get
            {
                if (_task != null)
                {
                    return _task;
                }
                int activeCount = 0;
                foreach (Tasks.Base task in robotTasks)
                {
                    if (task.gameObject.activeSelf)
                    {
                        _task = task;
                        activeCount++;
                    }
                }
                if (activeCount != 1)
                {
                    Debug.LogWarning("More than 1 Task is active, using: " + _task.name);
                }
                if (activeCount == 0)
                {
                    Debug.LogWarning("No Task is active");
                }
                return _task;
            }
        }

        public Tasks.Handcrafted taskSocialSituation
        {
            get
            {
                return GameObject.Find("Tasks/HandcraftedSocialSituation").GetComponent<Tasks.Handcrafted>();
            }
        }

        public Scenario.Robot robot
        {
            get
            {
                if (_robot != null)
                {
                    return _robot;
                }
                int activeCount = 0;
                foreach (Transform child in _Robots.transform)
                {
                    if (child.gameObject.activeSelf)
                    {
                        _robot = child.gameObject.GetComponent<Scenario.Robot>();
                        if (_robot == null)
                        {
                            throw new Exception("All children of the Robots object must have a Robot script attached. " + child.gameObject.name + " does not.");
                        }
                        activeCount++;
                    }
                }
                if (activeCount != 1)
                {
                    throw new Exception("Exactly 1 Robot must be active. " + activeCount + " are active.");
                }
                return _robot;
            }
        }

        public Scenario.Player player
        {
            get
            {
                if (_player != null)
                {
                    return _player;
                }
                int playerCount = 0;
                foreach (Transform child in _Players.transform)
                {
                    _player = child.gameObject.GetComponent<Scenario.Player>();
                    if (_player == null)
                    {
                        throw new Exception("All children of the Players object must have a Player script attached. " + child.gameObject.name + " does not.");
                    }
                    playerCount++;
                }
                if (playerCount > 1)
                {
                    throw new Exception("Only 1 Player can exist. " + playerCount + " exist.");
                }
                return _player;
            }
        }

        public Control.ControlSubscriber controller
        {
            get
            {
                if (_controller != null) { return _controller; }
                int activeCount = 0;
                foreach (Transform child in _Controllers.transform)
                {
                    if (child.gameObject.activeSelf)
                    {
                        _controller = child.gameObject.GetComponent<Control.ControlSubscriber>();
                        if (_controller == null)
                        {
                            throw new Exception("All children of the Controllers object must have a ControlSubscriber script attached. " + child.gameObject.name + " does not.");
                        }
                        activeCount++;
                    }
                }
                if (activeCount != 1)
                {
                    throw new Exception("Exactly 1 Controller must be active. " + activeCount + " are active.");
                }
                return _controller;
            }
        }

        public GameObject GetStartOrGoal(Scenario.Agents.ControlledAgent agent, bool start)
        {
            foreach (Transform child in _StartAndGoal.transform)
            {
                if (child.gameObject.name == agent.ToString())
                {
                    foreach (Transform startOrGoal in child.gameObject.transform)
                    {
                        if (start && startOrGoal.gameObject.name == "Start")
                        {
                            return startOrGoal.gameObject;
                        }
                        if (!start && startOrGoal.gameObject.name == "Target")
                        {
                            return startOrGoal.gameObject;
                        }
                    }
                }
            }
            return null;
        }


        #region UI

        /// <summary>
        ///  Get PedestrianBehaviors available for selection in the UI
        /// </summary>
        /// <param name="uiPedestrianBehaviors"></param>
        /// <param name="selectedId"></param>
        public void UIGetPedestrianBehaviors(out List<string> uiPedestrianBehaviors, out int selectedId)
        {
            int i = 0;
            selectedId = 0;
            uiPedestrianBehaviors = new List<string>();
            foreach (Scenario.PedestrianBehavior.Base behavior in pedestrianBehaviors)
            {
                uiPedestrianBehaviors.Add(behavior.name);
                if (behavior.gameObject.activeSelf) { selectedId = i; }
                i++;
            }
        }

        /// <summary>
        ///  Get Tasks available for selection in the UI
        /// </summary>
        /// <param name="uiTasks"></param>
        /// <param name="selectedId"></param>
        public void UIGetTasks(out List<string> uiTasks, out int selectedId)
        {
            int i = 0;
            selectedId = 0;
            uiTasks = new List<string>();
            foreach (Tasks.Base task in robotTasks)
            {
                // Can't use handcrafted task w/ anything except the handcrafted scenario
                if (pedestrianBehavior.name != "HandcraftedSocialSituation" && task.name == "HandcraftedSocialSituation")
                {
                    if (task.gameObject.activeSelf) { selectedId = -1; }
                    continue;
                }
                // Can't use labstudy task w/ anything except the labstudy scenario
                if (pedestrianBehavior.name != "LabStudy" && task.name == "LabStudy")
                {
                    if (task.gameObject.activeSelf) { selectedId = -1; }
                    continue;
                }
                uiTasks.Add(task.name);
                if (task.gameObject.activeSelf) { selectedId = i; }
                i++;
            }
        }

        #endregion

    }
}
