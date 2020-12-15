//#define DEBUGGING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class IntroGame : MonoBehaviour
{
    public Transform mainCamera;
    public Transform target;
    public Transform robot;
    public Transform player;
    public Transform spotlight;

    public Text textMessage;

    public float moveStep = 1.0f;
    public float rotateStep = 10.0f;

    public int StayInStateSeconds = 5;

    private Vector3 originalCameraLocalPosition;
    private Vector3 originalCameraLocalAngles;
    private Vector3 originalPlayerPosition;
    private Vector3 originalPlayerAngles;

    private ThirdPersonUserControl userControl;
    private CameraCollision cameraCollision;
    private TiltRotateCamera tiltCamera;
    private AgentThirdPersonControl agentControl;

    private bool introEnded = false;

    private float stateDuration = 0;
    private float stateTimeStart = 0;

#if DEBUGGING
    private bool showIntro = true;
#else
    private bool showIntro = false;
#endif
    private int state = 1;

    private const float DISTANCE_THRESHOLD = 4.0f;
    private const int CLICK_TO_START_STATE = 5;
    private const int START_STATE = 10;
    private const int FINAL_STATE = 12;
    private const int INF_TIME = 600;

    void Start()
    {

        originalCameraLocalPosition = mainCamera.localPosition;
        originalCameraLocalAngles = mainCamera.localEulerAngles;

        agentControl = player.GetComponent<AgentThirdPersonControl>();
        agentControl.reachedTarget = true;

        cameraCollision = mainCamera.GetChild(0).GetComponent<CameraCollision>();
        //cameraCollision.enabled = false;

        tiltCamera = mainCamera.GetComponent<TiltRotateCamera>();
        tiltCamera.enabled = false;

        getPlayer();

#if !DEBUGGING
        string intro = System.Environment.GetEnvironmentVariable("INTRO");
        showIntro = !string.IsNullOrEmpty(intro) && (intro.ToLower() == "true" || intro.ToLower() == "1");
#endif
        if (showIntro) {
            Vector3 midpoint = sampleNavmesh(Vector3.Lerp(robot.position, player.position, 0.5f));
            robot.position = midpoint;
        }
        else {
            state = CLICK_TO_START_STATE;
        }
        controls(false);
        stateTimeStart = Time.realtimeSinceStartup;
    }

    private void controls(bool enabled) {
        if (!player) {
            Debug.Log("No player, cannot pause("+enabled+") controls");
            return;
        }
        player.GetComponent<ThirdPersonCharacter>().pause(!enabled);
    }

    private void getPlayer() {
        GameObject playerObj = GameObject.FindWithTag("Actor");
        if (playerObj) {
            player = playerObj.transform;
        }
    }

    private Vector3 sampleNavmesh(Vector3 position, int maxDistance = 10) {
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(position, out hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas);
        return hit.position;
    }

    private void transition() {
        state++;
        stateTimeStart = Time.realtimeSinceStartup;
    }

    private bool IsVisibleByCamera(Camera camera, Vector3 point)
    {
        Vector3 vp = camera.WorldToViewportPoint(point);
        return vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1 && vp.z > 0;
    }

    void Update() {
        if (state > FINAL_STATE) { return; }
        if (!player) {
            getPlayer();
            return;
        }

        switch(state) {
            case 1:
                stateDuration = 15;
                controls(false);
                textMessage.text = "This is your avatar";
                spotlight.gameObject.active = true;
                spotlight.position = new Vector3(player.position.x, spotlight.position.y, player.position.z);
                spotlight.gameObject.active = true;
                spotlight.GetComponent<Light>().spotAngle = 30;
                mainCamera.Rotate(new Vector3(0, rotateStep * Time.deltaTime, 0));
                break;
            case 2:
                stateDuration = 15;
                textMessage.text = "Your goal is to walk to this location";
                spotlight.position = new Vector3(target.position.x, spotlight.position.y, target.position.z);
                spotlight.GetComponent<Light>().spotAngle = 20;
                mainCamera.Rotate(new Vector3(0, -rotateStep * Time.deltaTime, 0));
                mainCamera.position = Vector3.MoveTowards(mainCamera.position, target.position + Vector3.up, moveStep * Time.deltaTime);
                break;
            case 3:
                stateDuration = 15;
                textMessage.text = "Look for the robot along your way";
                spotlight.position = new Vector3(robot.position.x, spotlight.position.y, robot.position.z);
                mainCamera.Rotate(new Vector3(0, rotateStep * Time.deltaTime, 0));
                mainCamera.position = Vector3.MoveTowards(mainCamera.position, robot.position + Vector3.up, moveStep * Time.deltaTime);
                break;
            case 4:
                target.gameObject.SetActive(false);
                spotlight.gameObject.active = false;
                textMessage.text = "Now we'll try out the controls";
                originalPlayerPosition = player.position;
                originalPlayerAngles = player.eulerAngles;
                mainCamera.localPosition = Vector3.MoveTowards(mainCamera.localPosition, originalCameraLocalPosition, moveStep * Time.deltaTime);
                mainCamera.localEulerAngles = Vector3.MoveTowards(mainCamera.localEulerAngles, originalCameraLocalAngles, rotateStep * 2 * Time.deltaTime);
                if ((mainCamera.localPosition == originalCameraLocalPosition
                  && mainCamera.localEulerAngles == originalCameraLocalAngles)) {
                    transition();
                }
                break;
            case CLICK_TO_START_STATE:
                stateDuration = 0;
                target.gameObject.SetActive(false);
                Cursor.visible = true;
                textMessage.text = "Click anywhere in the game window to begin";
                if (Input.GetMouseButtonDown(0)){
                    Cursor.visible = false;
                    controls(true);
                    tiltCamera.enabled = true;
                    if (showIntro) {
                        transition();
                    } else {
                        state = START_STATE;
                    }
                }
                break;
            case 6:
                stateDuration = 15;
                textMessage.text = "Use the W, A, S, D keys to move the avatar";
                break;
            case 7:
                stateDuration = 15;
                textMessage.text = "Hold Left Shift to move faster";
                break;
            case 8:
                stateDuration = 15;
                textMessage.text = "The Up, Down arrow keys to tilt the camera";
                break;
            case 9:
                stateDuration = 5;
                textMessage.text = "You'll teleport back to the start shortly";
                break;
            case START_STATE:
                stateDuration = INF_TIME;
                textMessage.text = "Find the robot";
                if (showIntro) {
                    if (player) {
                        player.position = originalPlayerPosition;
                        player.eulerAngles = originalPlayerAngles;
                    }
                    mainCamera.localEulerAngles = originalCameraLocalAngles;
                }
                if (Vector3.Distance(player.position, robot.position) < DISTANCE_THRESHOLD) {
                    if (IsVisibleByCamera(mainCamera.GetChild(0).GetComponent<Camera>(), robot.position)) {
                        transition();
                    }
                }
                break;
            case (START_STATE + 1):
                stateDuration = 45;
                textMessage.text = "Follow the robot and observe its movement";
                break;
            case FINAL_STATE:
                textMessage.text = "Move the player to the yellow target";
                target.gameObject.SetActive(true);
                agentControl.reachedTarget = false;
                transition();
                break;
        }
        if (stateDuration > 0 && (Time.realtimeSinceStartup - stateTimeStart)  > stateDuration) {
            transition();
        }
    }
}
