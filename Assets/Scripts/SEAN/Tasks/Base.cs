// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Tasks
{
    /// <summary>
    /// Delegates to call when a new task is started
    /// </summary>
    public delegate void OnNewTask();

    public abstract class Base : MonoBehaviour
    {

        public GameObject robotStart { get; protected set; }
        public GameObject robotGoal { get; protected set; }

        public GameObject playerStart { get; protected set; }
        public GameObject playerGoal { get; protected set; }

        public float timeoutTaskSeconds = 120f;
        private float taskStartTime = 0f;

        private GameObject cube;
        private GameObject arrow;

        public OnNewTask onNewTask;

        public bool PublishGoal = true;
        public static string Topic = "/move_base_simple/goal";
        public string FrameID = "map";

        protected SEAN sean;
        private static ROSConnection ros;

        private float debounceTime = 0f;
        public float debounceTimeoutSec = 5f;
        private float debounceStartupTime = 0f;
        protected float debounceStartupTimeoutSec = 3f;
        private static RosMessageTypes.Geometry.MPoseStamped nextGoal;

        public Transform robotStartTransform
        {
            get { return robotStart.transform; }
            set
            {
                robotStart.transform.position = value.position;
                robotStart.transform.rotation = value.rotation;
            }
        }

        public Transform robotGoalTransform
        {
            get { return robotGoal.transform; }
            set
            {
                robotGoal.transform.position = value.position;
                robotGoal.transform.rotation = value.rotation;
            }
        }

        public Transform personStartTransform
        {
            get { return playerStart.transform; }
            set
            {
                playerStart.transform.position = value.position;
                playerStart.transform.rotation = value.rotation;
            }
        }

        public Transform personGoalTransform
        {
            get { return playerGoal.transform; }
            set
            {
                playerGoal.transform.position = value.position;
                robotGoal.transform.rotation = value.rotation;
            }
        }

        public GameObject interactiveStart
        {
            get
            {
                if (sean.PlayerControl)
                {
                    return playerStart;
                }
                else
                {
                    return robotStart;
                }
            }
        }

        private GameObject interactiveGoal
        {
            get
            {
                if (sean.PlayerControl)
                {
                    return playerGoal;
                }
                else
                {
                    return robotGoal;
                }
            }
        }

        private Transform controlledAvatar
        {
            get
            {
                if (sean.PlayerControl)
                {
                    // get the avatar
                    return sean.player.transform.GetChild(0);
                }
                else
                {
                    return sean.robot.transform;
                }
            }

        }

        public float completionDistance = 1.2f;

        public int maximumNumberOfTasks = 0;

        public bool isRunning { get; private set; }
        public ushort number { get; private set; }

        // implementers should override this method
        protected abstract bool NewTask();

        private void OnNewTask()
        {
            UpdatePositions();
            UpdateCameras();
            isRunning = true;
            number++;
            taskStartTime = Time.time;
        }

        public void SetTargetFlags(GameObject goal)
        {
            foreach (Transform child in goal.transform)
            {
                if (child.name == "TargetFlagCube")
                {
                    cube = child.gameObject;
                }
                else if (child.name == "TargetFlagArrow")
                {
                    arrow = child.gameObject;
                }
            }
        }

        public void Awake()
        {
            isRunning = false;
            // Needs to happen before any callbacks are assigned
            onNewTask = OnNewTask;
        }

        public virtual void Start()
        {
            ros = ROSConnection.instance;
            sean = SEAN.instance;
            initStartAndGoal();
            number = 0;
            debounceStartupTime = Time.time;
        }

        public void Update()
        {
            CheckNewTask();
            // rotate the target
            if (cube)
            {
                cube.transform.Rotate(0.0f, 2.0f, 0.0f, Space.World);
            }
        }

        protected void Publish()
        {
            nextGoal = new RosMessageTypes.Geometry.MPoseStamped();
            nextGoal.header.frame_id = FrameID;
            sean.clock.UpdateMHeader(nextGoal.header);
            nextGoal.pose = Util.Geometry.GetMPose(interactiveGoal.transform);
            ros.Send(Topic, nextGoal);
        }

        private void initStartAndGoal()
        {
            robotStart = sean.GetStartOrGoal(Scenario.Agents.ControlledAgent.Robot, true);
            robotGoal = sean.GetStartOrGoal(Scenario.Agents.ControlledAgent.Robot, false);
            playerStart = sean.GetStartOrGoal(Scenario.Agents.ControlledAgent.Player, true);
            playerGoal = sean.GetStartOrGoal(Scenario.Agents.ControlledAgent.Player, false);
            if (sean.ControlledAgent == Scenario.Agents.ControlledAgent.Robot)
            {
                robotGoal.SetActive(true);
                playerGoal.SetActive(false);
            }
            if (sean.ControlledAgent == Scenario.Agents.ControlledAgent.Player)
            {
                robotGoal.SetActive(false);
                playerGoal.SetActive(true);
            }
            robotStart.SetActive(false);
            playerStart.SetActive(false);
        }

        private void CheckNewTask()
        {
            if (debouce())
            {
                if (maximumNumberOfTasks > 0 && number >= maximumNumberOfTasks)
                {
                    Debug.Log("Completed " + number + " of  " + maximumNumberOfTasks + " tasks, exiting");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
                if (NewTask())
                {
                    if (PublishGoal)
                    {
                        Publish();
                    }
                    onNewTask.Invoke();
                }
            }
        }

        // disable mesh on the start object
        private void disableMesh(GameObject go)
        {
            go.GetComponent<MeshRenderer>().enabled = false;
        }

        private bool debouce()
        {
            if (!isRunning)
            {
                return debounceStartup();
            } else
            {
                return debounceCompletion() || timeout();
            }
        }

        private bool debounceStartup()
        {
            return (Time.time - debounceStartupTime > debounceStartupTimeoutSec);
        }
        private bool debounceCompletion()
        {
            // Wait until the robot and goal are not at the origin
            if (controlledAvatar.position == Pose.identity.position && controlledAvatar.rotation == Pose.identity.rotation &&
                interactiveGoal.transform.position == Pose.identity.position && interactiveGoal.transform.rotation == Pose.identity.rotation)
            {
                //print("at the origin");
                return false;
            }
            float distToGoal = Vector3.Distance(controlledAvatar.position, interactiveGoal.transform.position);
            //print(debounceTime + " > " + completionDistance + ", debouceCompletion distToGoal: " + distToGoal);

            if (distToGoal > completionDistance)
            {
                debounceTime = 0;
                return false;
            }
            if (debounceTime == 0)
            {
                debounceTime = Time.time;
                return false;
            }
            if (Time.time - debounceTime > debounceTimeoutSec)
            {
                debounceTime = 0;
                return true;
            }
            return false;
        }

        private void UpdatePositions()
        {
            if (sean.PlayerControl && playerStart)
            {
                // if the avatar hasn't been created yet; this will spawn avatar at start position
                if (sean.player.transform.GetChild(0).gameObject.name == "RocketboxRandomAnimatedPlayer")
                {
                    sean.player.transform.rotation = playerStart.transform.rotation;
                    sean.player.transform.position = playerStart.transform.position;
                }
                else
                {
                    sean.player.transform.GetChild(0).rotation = playerStart.transform.rotation;
                    sean.player.transform.GetChild(0).position = playerStart.transform.position;
                }
            }
            if (robotStart)
            {
                sean.robot.base_link.transform.rotation = robotStart.transform.rotation;
                sean.robot.base_link.transform.position = robotStart.transform.position;
            }
        }

        private bool timeout()
        {
            if (taskStartTime == 0f) { return false; }
            if (taskStartTime + timeoutTaskSeconds < Time.time)
            {
                taskStartTime = 0f;
                return true;
            }
            return false;
        }

        private void UpdateCameras()
        {
            if (SEAN.instance.TopDownViewOnly) { return; }
            SEAN.instance.environment.topViewCamera.enabled = false;
            if (!sean.PlayerControl)
            {
                SEAN.instance.robot.camera_first.enabled = true;
            }
        }

        #region helpers
        /// <summary>
        ///  Sample a random group (from ground truth, if available)
        /// </summary>
        /// <returns></returns>
        protected bool GetRandomGroup(out Vector3 center)
        {
            center = Vector3.zero;
            if (sean.pedestrianBehavior.groups.Length < 1)
            {
                return false;
            }
            center = sean.pedestrianBehavior.groups[Random.Range(0, sean.pedestrianBehavior.groups.Length)].center;
            return true;
        }

        /// <summary>
        ///  Sample a random and unoccupied group member position (from ground truth, if available)
        /// </summary>
        /// <returns>true if a group center is found and returned via the output argument</returns>
        protected bool GetRandomGroupMembershipTransform(out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            if (sean.pedestrianBehavior.groups.Length < 1)
            {
                return false;
            }
            Scenario.Trajectory.TrackedGroup[] groups = sean.pedestrianBehavior.groups;
            if (groups[Random.Range(0, groups.Length)].GroupMemberLocationGenerator(out position, out rotation))
            {
                // may not be necessary:
                UnityEngine.AI.NavMeshHit hit = Util.Navmesh.RandomHit(position, 0.5f);
                position = hit.position;
                return true;
            }
            return false;
        }
        #endregion helpers

        void OnGUI()
        {
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(45, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            string text = string.Format("Interaction ends in: {0:0.00}", timeoutTaskSeconds - (Time.time - taskStartTime));
            GUI.Label(rect, text, style);
        }


    }
}
