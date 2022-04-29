using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Agents.Playback
{
    // TODO: rename PlaybackAgentManager
    public class LoadAllAvatar : MonoBehaviour
    {
        private static LoadAllAvatar _instance;
        public static LoadAllAvatar instance { get { return _instance; } }

        public Dictionary<int, Agent> agents;
        public GameObject agentPrefab;
        private GameObject agentsGO;

        // Source Data: https://github.com/erichhhhho/DataExtraction
        public TextAsset csv_file;

        public int startFrame = 0;
        public bool flip_x;
        public bool flip_y;
        public float fps = 5;

        // agentIds indexed by frameId
        private Dictionary<int, HashSet<int>> agentsInFrame;
        // trajectory indexed by agentId
        private Dictionary<int, Dictionary<int, Pose>> trajectories;
        private float StartTime;
        private void FindAgentsGameObject()
        {
            foreach (Transform transform in gameObject.transform)
            {
                if (transform.gameObject.name == "Agents")
                {
                    agentsGO = transform.gameObject;
                }
            }
        }

        private void LoadScene()
        {
            agentsInFrame = new Dictionary<int, HashSet<int>>();
            trajectories = new Dictionary<int, Dictionary<int, Pose>>();
            foreach (Dictionary<int, object> row in Util.CSVReader.Read(csv_file))
            {
                int frameId = (int)row[0];
                int agentId = (int)row[1];
                float x = (float)row[2];
                float y = (float)row[3];
                if (flip_x) { x = -x; }
                if (flip_y) { y = -y; }
                bool isFirstFrame = false;
                if (!trajectories.ContainsKey(agentId))
                {
                    isFirstFrame = true;
                    trajectories[agentId] = new Dictionary<int, Pose>();
                }
                // TODO: right -> Left handed conversion
                Pose pose = new Pose();
                Pose pose1 = new Pose();
                pose.position = new Vector3(x, 0, y);
                trajectories[agentId][frameId] = pose;
                // Update every frame except for the last one twice
                // So that their values are kept to the last time updated
                // And the last frame's' value is the same as its previous frame
                if (!isFirstFrame) {
                    pose.rotation = CalculateOrientation(agentId, frameId-1);
                    trajectories[agentId][frameId] = pose;
                    pose1.position = trajectories[agentId][frameId-1].position;
                    pose1.rotation = trajectories[agentId][frameId].rotation;
                    trajectories[agentId][frameId-1] = pose1;
                }
                if (!agentsInFrame.ContainsKey(frameId)) {
                    agentsInFrame[frameId] = new HashSet<int>();
                }
                agentsInFrame[frameId].Add(agentId);
            }
            agents = new Dictionary<int, Agent>();
        }

        void Start()
        {
            FindAgentsGameObject();
            LoadScene();
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            
            StartTime = Time.time;
            int currentFrame = startFrame;
            while (true)
            {
                currentFrame = (int) Mathf.Floor((Time.time-StartTime) * fps)+startFrame;
                currentFrame %= agentsInFrame.Count;
                Debug.Log(currentFrame);
                //print("frame: " + currentFrame + ", " + frames[currentFrame]);
                try {
                    CreateOrMovePedestrians(currentFrame);
                } catch (KeyNotFoundException e) {
                    Debug.Log("Error Frame " + currentFrame.ToString());
                }
                //CreateOrMovePedestrians(currentFrame);
                yield return 0;
                // yield return new WaitForSeconds(1.0f / fps);
            }
        }

        void CreateOrMovePedestrians(int frameId)
        {
            HashSet<int> seenPedestrianIds = new HashSet<int>();
            //Debug.Log("frameId"+frameId.ToString());
            foreach (int agentId in agentsInFrame[frameId])
            {
                seenPedestrianIds.Add(agentId);
                if (!agents.ContainsKey(agentId))
                {   
                    // Debug.LogFormat("agentId: {0}, pos: {1}", agentId, trajectories[agentId][frameId]);
                    SpawnAgent(agentId, trajectories[agentId][frameId]);
                }
                else
                {
                    MoveAgent(agentId, trajectories[agentId][frameId]);
                }
            }

            // Deactivate pedestrians w/o information in this frame
            foreach (int id in agents.Keys)
            {
                if (!seenPedestrianIds.Contains(id))
                {
                    agents[id].gameObject.SetActive(false);
                }
            }
        }

        void SpawnAgent(int id, Pose pose)
        {
            var sfRandom = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);

            IVI.SFAgent sfAgentComponent;
            sfAgentComponent = sfRandom.GetComponentInChildren<IVI.SFAgent>();
            if (sfAgentComponent != null)
            {
                Destroy(sfAgentComponent);
            }

            Agent playbackAgent;
            playbackAgent = sfRandom.GetComponentInChildren<Agent>();
            if (playbackAgent == null)
            {
                playbackAgent = sfRandom.transform.GetChild(0).gameObject.AddComponent<Agent>();
            }
            
            playbackAgent.name = "Agent_" + id;
            playbackAgent.transform.position = pose.position;
            playbackAgent.transform.rotation = pose.rotation;
            playbackAgent.transform.parent = agentsGO.transform;

            // Allow intersection w/ other objects
            // foreach (Collider c in playbackAgent.GetComponents<Collider>())
            // {   
            //     if (GetComponent<Collider>().GetType() == typeof(CapsuleCollider))
            //     {
            //         Debug.Log("HERE");
            //         ((CapsuleCollider)c).radius = 0.01f;
            //     }
            // }
            agents.Add(id, playbackAgent);
        }

        void MoveAgent(int id, Pose pose)
        {
            agents[id].UpdateVelocity(pose);
        }

        Quaternion CalculateOrientation(int agentId, int frameId)
        {
            Dictionary<int, Pose> trajectory = trajectories[agentId];
            float angle = 0;
            int nextFrame = frameId + 1;
            if (trajectory.ContainsKey(nextFrame))
            {
                Pose nextPose = trajectory[nextFrame];
                Pose currPose = trajectory[frameId];

                if ((nextPose.position.x - currPose.position.x) != 0)
                {
                    angle = Mathf.Rad2Deg * Mathf.Tan((nextPose.position.z - currPose.position.z) / (nextPose.position.x - currPose.position.x));
                }
            }
            return Quaternion.Euler(0, angle, 0);
        }
    }
}
