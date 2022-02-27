using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class RandomAvatar : MonoBehaviour
    {
        public RuntimeAnimatorController animationController;
        public GameObject[] avatars;
        public LowLevelControl controller = LowLevelControl.SF;
        public bool isPlayer = false;
        static private List<GameObject> avatarsList;

        private GameObject avatarPrefab;
        private GameObject avatarObject;

        void Awake()
        {
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
            Animator animator = avatarObject.GetComponent<Animator>();
            animator.runtimeAnimatorController = animationController;
            if (SEAN.instance)
            {
                controller = SEAN.instance.AgentController;
            }
            if (isPlayer)
            {
                avatarObject.AddComponent<PlayerAgent>();
            }
            else
            {
                if (controller == LowLevelControl.SF)
                {
                    avatarObject.AddComponent<IVI.SFAgent>();
                }
                else if (controller == LowLevelControl.ORCA)
                {
                    avatarObject.AddComponent<ORCA.Agent>();
                }
            }
            avatarObject.transform.parent = transform;
        }
    }
}