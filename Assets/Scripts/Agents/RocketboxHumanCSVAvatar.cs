//TODO: remove
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;
//using UnityStandardAssets.Characters.ThirdPerson;

//public class RocketboxHumanCSVAvatar : MonoBehaviour
//{
//    public float groundCheckDistance = 0.3f;
//    public float mass = 100000;

//    public  GameObject[] avatars;
//    static private List<GameObject> avatarsList;

//    private GameObject avatarPrefab;
//    private GameObject avatarObject;
//    private GameObject virtualColliderObject;
//    private Rigidbody rb;

//    void Awake()
//    {
//        // Copy the avatars list at the beginning of the game
//        // or if avatarsList becomes empty
//        if (avatarsList is null || avatarsList.Count == 0)
//        {
//            avatarsList = new List<GameObject>(avatars);
//        }

//        // Load a random avatar from the remaining avatars in the list
//        // then remove that avatar from the list, and if the list becomes empty
//        // then it copies the avatars array to avatarsList
//        if (avatarPrefab is null)
//        {
//            int randomIndex = Random.Range(0, avatarsList.Count);
//            avatarPrefab = avatarsList[randomIndex];
//            avatarsList.RemoveAt(randomIndex);
//        }

//        avatarObject = Instantiate(avatarPrefab, transform.position, transform.rotation);
//        avatarObject.transform.parent = transform;
//        avatarObject.tag = "Actor";

//        rb = avatarObject.GetComponent<Rigidbody>();

//        // makes sure people can't tip over
//        rb.inertiaTensor = new Vector3(0.01f, 0.01f, 0.01f);
//        rb.centerOfMass = Vector3.zero;
//        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
//	    rb.mass = 1E5f;

//        var thirdComp = avatarObject.AddComponent<ThirdPersonCharacter>();
//        thirdComp.m_GroundCheckDistance = groundCheckDistance;
//        //thirdComp.m_MoveSpeedMultiplier = 1.0f;
//        //thirdComp.m_AnimSpeedMultiplier = 1.0f;
//        thirdComp.m_MovingTurnSpeed = 180;
//        thirdComp.m_StationaryTurnSpeed = 90;

//        var agentComp = avatarObject.AddComponent<PeoplePlaybackController>();
//    }

//    void FixedUpdate() {
//        //Debug.Break();
//        // make sure the person doesnt tip over
//        avatarObject.transform.eulerAngles = new Vector3(0, avatarObject.transform.eulerAngles.y, 0);
//    }
//}
