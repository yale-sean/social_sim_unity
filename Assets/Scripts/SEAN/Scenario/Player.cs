using UnityEngine;

namespace SEAN.Scenario
{
    public class Player : MonoBehaviour
    {
        public Camera camera_first;

        private void DisableRobotCameras()
        {
            foreach (Camera c in Camera.allCameras)
            {
                if (c.gameObject.name == camera_first.name)
                {
                    c.gameObject.SetActive(true);
                    c.gameObject.transform.Rotate(Vector3.right * 15.0f);
                }
                else
                {
                    c.gameObject.SetActive(false);
                }
            }
        }

        public void Start()
        {
            if (camera_first == null)
            {
                throw new System.ArgumentException("A first person camera must be assigned to the player " + name);
            }
            DisableRobotCameras();
        }

        public new Transform transform
        {
            get
            {
                return gameObject.transform;
            }
        }
        public Vector3 position
        {
            get
            {
                return transform.position;
            }
        }
        public Quaternion rotation
        {
            get
            {
                return transform.rotation;
            }
        }
        public override string ToString()
        {
            return gameObject.name;
        }
    }
}
