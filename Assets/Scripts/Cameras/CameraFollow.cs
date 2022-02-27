using System;
using UnityEngine;


namespace UnityStandardAssets._2D
{
    public class CameraFollow : MonoBehaviour
    {
        public float xMargin = 5f; // Distance in the x axis the player can move before the camera follows.
        public float zMargin = 5f; // Distance in the z axis the player can move before the camera follows.
        public float xSmooth = 5f; // How smoothly the camera catches up with it's target movement in the x axis.
        public float zSmooth = 5f; // How smoothly the camera catches up with it's target movement in the z axis.
        public float rotSmooth = 0.5f;

        public string FrameTag = "RobotFrame";
        private Transform m_Player; // Reference to the player's transform.
        private float elapsed = 0.0f;


        private void Awake()
        {
            // Setting up the reference.
            m_Player = GameObject.FindGameObjectWithTag(FrameTag).transform;
        }


        private bool CheckXMargin()
        {
            // Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
            return Mathf.Abs(transform.position.x - m_Player.position.x) > xMargin;
        }


        private bool CheckZMargin()
        {
            // Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
            return Mathf.Abs(transform.position.z - m_Player.position.z) > zMargin;
        }


        private void Update()
        {
            TrackPlayer();
        }


        private void TrackPlayer()
        {
            // By default the target x and y coordinates of the camera are it's current x and y coordinates.
            float targetX = transform.position.x;
            float targetZ = transform.position.z;

            float d = ((xSmooth + zSmooth) / 2);
            float t = 1f;
            if (elapsed < d)
            {
                t = elapsed / d;
                elapsed += Time.deltaTime;
            }
            else
            {
                elapsed = 0;
            }

            // If the player has moved beyond the x margin...
            if (CheckXMargin())
            {
                // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
                targetX = Mathf.Lerp(transform.position.x, m_Player.position.x, t);
            }

            // If the player has moved beyond the z margin...
            if (CheckZMargin())
            {
                // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
                targetZ = Mathf.Lerp(transform.position.z, m_Player.position.z, t);
            }

            // Set the camera's position to the target position with the same z component.
            //var targetPos = new Vector3(targetX, transform.position.y, targetZ);
            var targetPos = new Vector3(m_Player.position.x, transform.position.y, m_Player.position.z);
            var targetRot = Quaternion.LookRotation(m_Player.transform.position - transform.position);

            if (CheckXMargin() || CheckZMargin())
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, t);
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSmooth * Time.deltaTime);
        }
    }
}
