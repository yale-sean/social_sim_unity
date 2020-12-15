using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class RocketboxRandomAvatar : MonoBehaviour
{
    public float forceMultiplier = 0.4f;
    public float goalFactor = 1;
    public float agentFactor = 1;
    public float wallFactor = 1;

    public float groundCheckDistance = 0.3f;
    public float perceptionRadius = 1.5f;

    public bool useAvatarParam = false;

    public  GameObject[] avatars;
    static private List<GameObject> avatarsList;

    private GameObject avatarPrefab;
    private GameObject avatarObject;
    private GameObject virtualColliderObject;
    private Rigidbody rb;

    void Awake()
    {
        //avatars = Resources.LoadAll<GameObject>("Prefabs/Rocketbox");
        string[] args = System.Environment.GetCommandLineArgs ();
        string avatar = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (args [i] == "-avatar")
            {
                avatar = args [i + 1];
                Debug.Log("Avatar: " + avatar);
                break;
            }
        }

        // Load the avatar that is specified in the command line parameter
        if (useAvatarParam && avatar != "")
        {
            avatarPrefab = Resources.Load("Prefabs/Rocketbox/" + avatar) as GameObject;
        }

        // Copy the avatars list at the beginning of the game
        // or if avatarsList becomes empty
        if (avatarsList is null || avatarsList.Count == 0)
        {
            avatarsList = new List<GameObject>(avatars);
        }

        // Load a random avatar from the remaining avatars in the list
        // then remove that avatar from the list, and if the list becomes empty
        // then it copies the avatars array to avatarsList
        if (avatarPrefab is null)
        {
            int randomIndex = Random.Range(0, avatarsList.Count);
            avatarPrefab = avatarsList[randomIndex];
            avatarsList.RemoveAt(randomIndex);
        }

        avatarObject = Instantiate(avatarPrefab, transform.position, transform.rotation);
        avatarObject.transform.parent = transform;
        avatarObject.tag = "Actor";

        rb = avatarObject.GetComponent<Rigidbody>();

        rb.inertiaTensor = new Vector3(0.01f, 0.01f, 0.01f);
        rb.centerOfMass = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
	rb.mass = 1E5f;

        virtualColliderObject = new GameObject();
        virtualColliderObject.transform.position = transform.position;
        virtualColliderObject.transform.rotation = transform.rotation;
        virtualColliderObject.transform.parent = avatarObject.transform;
        virtualColliderObject.transform.SetSiblingIndex(0);
        virtualColliderObject.layer = 2;    // Ignore Raycast layer
        virtualColliderObject.name = "SocialForcesCollider";
        var sphereComp = virtualColliderObject.AddComponent<SphereCollider>();
        sphereComp.isTrigger = true;
        sphereComp.radius = perceptionRadius;

        var thirdComp = avatarObject.AddComponent<ThirdPersonCharacter>();
        thirdComp.m_GroundCheckDistance = groundCheckDistance;
        //thirdComp.m_MoveSpeedMultiplier = 1.0f;
        //thirdComp.m_AnimSpeedMultiplier = 1.0f;
        thirdComp.m_MovingTurnSpeed = 90;
        thirdComp.m_StationaryTurnSpeed = 45;

        var agentComp = avatarObject.AddComponent<Agent>();
        agentComp.enabled = false;
        agentComp.FORCE_MULTIPLIER = forceMultiplier;
        agentComp.GOAL_FACTOR = goalFactor;
        agentComp.AGENT_FACTOR = agentFactor;
        agentComp.WALL_FACTOR = wallFactor;

        var navComp = avatarObject.AddComponent<NavMeshAgent>();
        navComp.enabled = false;
        navComp.baseOffset = 1;
        navComp.speed = 3.5f;
        navComp.angularSpeed = 120;
        navComp.acceleration = 8;
        navComp.stoppingDistance = 0;
        navComp.autoBraking = true;
        navComp.radius = 1;
        navComp.height = 2;
    }

    void FixedUpdate() {
        avatarObject.transform.eulerAngles = new Vector3(0, avatarObject.transform.eulerAngles.y, 0);
    }
}
