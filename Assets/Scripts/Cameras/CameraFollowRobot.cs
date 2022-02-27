using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowRobot : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float tiltSpeed;
    private Vector3 height = Vector3.up;

    private void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();
    }

    private void HandleTranslation()
    {
        var targetPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            height += tiltSpeed * Vector3.up;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            height -= tiltSpeed * Vector3.up;
        }
        var direction = target.position - transform.position + height;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}
