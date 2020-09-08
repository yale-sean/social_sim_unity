using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class AgentThirdPersonControl : MonoBehaviour
{
    public string SpawnTag = "Spawn";
    public GameObject targetObject;

    private List<Transform> possiblePositions = new List<Transform>();
    private GameObject avatarObject;

    void Awake() {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(SpawnTag)) {
            possiblePositions.Add(obj.transform);
        }
    }

    void Start()
    {
        avatarObject = transform.GetChild(1).gameObject;
        transform.GetChild(0).parent = avatarObject.transform;
        avatarObject.AddComponent<ThirdPersonUserControl>();

        // Move avatar and target to random locations
        transform.position = SpawnLocation();
        targetObject.transform.position = SpawnLocation();
    }

    void Update()
    {
        // Rotate the target
        targetObject.transform.Rotate(0.0f, 2.0f, 0.0f, Space.World);
    }

    private Vector3 SpawnLocation() {
        int idx = Random.Range(0, possiblePositions.Count-1);
        return possiblePositions[idx].position;
    }
}
