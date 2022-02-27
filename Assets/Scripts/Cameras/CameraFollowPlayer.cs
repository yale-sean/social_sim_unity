using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour
{
    // Use this for initialization
    private Transform player;
    private string cameraName = "PlayerCamera";

    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;
    public float smooth = 10.0f;
    Vector3 playerDir;
    public float distance;

    void Start()
    {
        foreach (Transform child in gameObject.transform.parent.transform)
        {
            // get the SF Avatar player
            if (child != gameObject.transform)
            {
                player = child;
            }
        }

        // transform of the camera
        foreach (Camera c in Camera.allCameras)
        {
            if (c.gameObject.name == cameraName)
            {
                c.gameObject.SetActive(true);
                if (player)
                {
                    c.transform.parent = player;
                }
            }
            else
            {
                c.gameObject.SetActive(false);
            }
        }

        playerDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredCameraPos = transform.parent.TransformPoint(playerDir * maxDistance);
        Vector3 playerPos = transform.parent.position;

        RaycastHit hit;

        if (Physics.Linecast(playerPos, desiredCameraPos, out hit))
        {
            distance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, playerDir * distance, Time.deltaTime * smooth);

    }
}
