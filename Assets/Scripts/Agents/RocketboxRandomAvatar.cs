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

    private GameObject[] avatars;
    private GameObject avatarPrefab;
    private GameObject avatarObject;
    private GameObject virtualColliderObject;

    void Awake()
    {
        avatars = Resources.LoadAll<GameObject>("Prefabs/Rocketbox");
        avatarPrefab = avatars[Random.Range(0, avatars.Length)];
        avatarObject = Instantiate(avatarPrefab, transform.position, transform.rotation);
        avatarObject.transform.parent = transform;
        avatarObject.tag = "Actor";

        Rigidbody rb = avatarObject.GetComponent<Rigidbody>();

        rb.inertiaTensor = new Vector3(0.01f, 0.01f, 0.01f);
        rb.centerOfMass = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;

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
}
