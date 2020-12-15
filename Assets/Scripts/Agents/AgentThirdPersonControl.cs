//#define DEBUGGING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.UI;

public class AgentThirdPersonControl : MonoBehaviour
{
    public string SpawnTag = "Spawn";
    public GameObject targetObject;

    public GameObject inGameUI;
    public GameObject afterGameUI;
    public GameObject tokenTextbox;

    public GameObject afterGameUIROS;
    public GameObject tokenTextboxROS;

    public Text timeDigits;
    public Text gameEndMsg;

    public float timeLimit = 240;

    public float tolerance = 0.3f;

    public Transform robotTransform;

    private List<Transform> possiblePositions = new List<Transform>();
    private GameObject avatarObject;
    private RosSharp.RosBridgeClient.OnePoseStampedPublisher posePublisher;
    private GameObject robotGoalObject;

    private string token = "";
    private string tokenTimeout = "";
    private string personposition = "";
    private string robotposition = "";
    private string windowmode = "";
    private int seed;

    public bool reachedTarget = false;
    public bool gameEnded = false;

    private float remTime;

    public string GoalStatusTopic = "/actor_goal_status";
    private RosSharp.RosBridgeClient.BoolPublisher reachedGoalPublisher;

    void Awake()
    {
        string[] args = System.Environment.GetCommandLineArgs ();
        for (int i = 0; i < args.Length; i++)
        {
            if (args [i] == "-token")
            {
                token = args [i + 1];
                Debug.Log("Token: " + token);
            }
            else if (args [i] == "-token_timeout")
            {
                tokenTimeout = args [i + 1];
                Debug.Log("token_timeout: " + tokenTimeout);
            }
            else if (args [i] == "-person_position")
            {
                personposition = args [i + 1];
                Debug.Log("person_position: " + personposition);
            }
            else if (args [i] == "-robot_position")
            {
                robotposition = args [i + 1];
                Debug.Log("robot_position: " + robotposition); //person's target is robot spawn position
            }
            else if (args[i] == "-avatar_seed")
            {
                seed = System.Int32.Parse(args[i + 1]);
                Random.seed = seed;
                Debug.Log("avatar_seed: " + seed);
            }
            else if (args[i] == "-window_mode")
            {
                windowmode = args[i + 1];
                Debug.Log("window_mode: " + windowmode);
            }
        }
    }

    void Start()
    {
        if (windowmode == "exclusive")
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }

        Cursor.visible = false;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(SpawnTag))
        {
            possiblePositions.Add(obj.transform);
        }

#if DEBUGGING
// Lab
//        personposition = "SpawnLocation_Lab7";
//        robotposition = "SpawnLocation_Lab6";

// Warehouse
        personposition = "SpawnLocation6";
        robotposition = "SpawnLocation7";
#endif

        avatarObject = transform.GetChild(1).gameObject;
        var cameraParentTransform = transform.GetChild(0);
        cameraParentTransform.parent = avatarObject.transform;
        avatarObject.AddComponent<ThirdPersonUserControl>();

        // Move avatar and target to random locations
        if (personposition == "" || robotposition == "")
        {
            Debug.Log("Randomly Spawning: " + personposition);
            avatarObject.transform.position = SpawnLocation();
            targetObject.transform.position = SpawnLocation();
            //robotTransform.position = targetObject.transform.position + Vector3.up;

            // Make sure the start and target positions are different
            while (avatarObject.transform.position == targetObject.transform.position)
            {
                avatarObject.transform.position = SpawnLocation();
            }
            
        }

        //Move avatar and target to preset locations
        else
        {
            Debug.Log("Spawning person at position: '" + personposition +"', robot at position: '" + robotposition+ "'");
            GameObject personGoalObject = GameObject.Find(personposition);
            robotGoalObject = GameObject.Find(robotposition);

            // Removing the "Spawn" tag from those locations
            personGoalObject.tag = "Untagged";
            robotGoalObject.tag = "Untagged";
            
            Debug.Log("personGoalObject: " + personGoalObject +", robotGoalObject: " + robotGoalObject);
            Debug.Log("personGoalObject.transform: " + personGoalObject.transform +", robotGoalObject.transform: " + robotGoalObject.transform);
            // person
            avatarObject.transform.position = robotGoalObject.transform.position;
            avatarObject.transform.LookAt(personGoalObject.transform.position, Vector3.up);
            // send the robot to this position
            robotTransform.position = personGoalObject.transform.position;
            // DON'T do this! Breaks the sync between ros and unity frames
            //robotTransform.LookAt(robotGoalObject.transform.position, Vector3.up);
            targetObject.transform.position = personGoalObject.transform.position;

            // set the  robot goal
            posePublisher = gameObject.AddComponent<RosSharp.RosBridgeClient.OnePoseStampedPublisher>() as RosSharp.RosBridgeClient.OnePoseStampedPublisher;
            posePublisher.SendOnce(robotGoalObject.transform);
            posePublisher.FrameId = "map";
            posePublisher.Topic = "/move_base_simple/goal";
        }
    
        // Adjust camera position
        cameraParentTransform.localPosition = new Vector3(0, 1.8f, 0);
        #if (DEBUGGING)
            personposition = "SpawnLocation_Lab6";
            robotposition = "SpawnLocation_Lab4";
        #endif

        // so we can stop recording once the trial is over
        reachedGoalPublisher = gameObject.AddComponent<RosSharp.RosBridgeClient.BoolPublisher>();
        reachedGoalPublisher.Topic = GoalStatusTopic;
    }

    void Update()
    {
        // Rotate the target
        targetObject.transform.Rotate(0.0f, 2.0f, 0.0f, Space.World);
        remTime = timeLimit - Time.realtimeSinceStartup;

        if (!gameEnded) {
            Vector3 diff = avatarObject.transform.position - targetObject.transform.position;
            diff.y = 0;
            float dist = diff.magnitude;
            if (dist < tolerance && !reachedTarget)
            {
                reachedTarget = true;
                reachedGoalPublisher.UpdateInfo(true);
                //avatarObject.GetComponent<ThirdPersonUserControl>().enabled = false;
                tokenTextbox.GetComponent<InputField>().text = token;
                tokenTextboxROS.GetComponent<InputField>().text = token;
                inGameUI.SetActive(false);
                gameEndMsg.text = "You made it!";
                afterGameUI.SetActive(true);
                afterGameUIROS.SetActive(true);
                Cursor.visible = true;
                GameObject.FindWithTag("Actor").GetComponent<ThirdPersonCharacter>().pause(true);
                gameEnded = true;
            }
            else if (remTime <= 0)
            {
                tokenTextbox.GetComponent<InputField>().text = tokenTimeout;
                tokenTextboxROS.GetComponent<InputField>().text = tokenTimeout;
                inGameUI.SetActive(false);
                gameEndMsg.text = "Time's up!";
                afterGameUI.SetActive(true);
                afterGameUIROS.SetActive(true);
                Cursor.visible = true;
                GameObject.FindWithTag("Actor").GetComponent<ThirdPersonCharacter>().pause(true);
                gameEnded = true;
            }
        }
        int remTimeMins = (int) remTime / 60;
        int remTimeSecs = (int) remTime - 60 * remTimeMins;
        timeDigits.text = remTimeMins.ToString("00") + ":" + remTimeSecs.ToString("00");
    }

    private Vector3 SpawnLocation() {
        int idx = Random.Range(0, possiblePositions.Count-1);
        possiblePositions[idx].tag = "Untagged";
        //Debug.Log("Untagging " + possiblePositions[idx].name);
        return possiblePositions[idx].position;
    }
}
