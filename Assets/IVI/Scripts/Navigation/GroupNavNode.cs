using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace IVI
{
    public class GroupNavNode : NavNode, SEAN.Scenario.Trajectory.ITrackedGroup
    {
        public int groupSize = 5;
        public float minTime = 5;
        public float maxTime = 10;
        public static string MemberTag = SEAN.SEAN.GroupTag;

        public Dictionary<INavigable, int> members;
        private Queue<int> indices;
        private List<(float, Vector3)> positions;

        public SEAN.Scenario.Trajectory.TrackedGroup group { get { return SEAN.Scenario.Trajectory.TrackedGroup.GetOrAttach(gameObject); } }

        new void Start()
        {
            base.Start();
#if UnityEditor
        if (!EditorApplication.isPlaying)
            return;
#endif

            members = new Dictionary<INavigable, int>();
            indices = new Queue<int>();
            positions = new List<(float, Vector3)>();
            if (groupSize <= 6 && GroupDataLoader.groupData != null)
            {
                GroupDataLoader.GroupData group = GroupDataLoader.groupData[Random.Range(0, GroupDataLoader.groupData.Count)];
                //print("Loading Group: " + group);
                float minDist = group.pos.Min(p => p.magnitude);
                //radius = minDist;
                for (int i = 0; i < groupSize; i++)
                {
                    indices.Enqueue(i);
                    //print("dataset enqueued count: " + indices.Count);

                    //float angle = 360f * i / groupSize;
                    //Vector3 pos = transform.position + (group.pos[i] / minDist * radius);

                    float angle = Mathf.Atan(group.pos[i].x / group.pos[i].z);
                    if (group.pos[i].z > 0)
                    {
                        angle += Mathf.PI;
                    }
                    Vector3 pos = transform.position + group.pos[i];

                    positions.Add((angle, pos));
                }
            }
            else
            {
                for (int i = 0; i < groupSize; i++)
                {
                    indices.Enqueue(i);
                    //print("datagen enqueued count: " + indices.Count);

                    var angle = 360f * i / groupSize;
                    var pos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * radius;

                    positions.Add((angle * Mathf.Deg2Rad, pos));
                }
            }

            var obst = GetComponent<NavMeshObstacle>();
            if (obst != null)
            {
                DestroyImmediate(obst);
            }

            obst = gameObject.AddComponent<NavMeshObstacle>();
            obst.carving = true;
            obst.shape = NavMeshObstacleShape.Capsule;
            var navMeshAgentRadius = 0.5f;
            obst.radius = 0.5f * (1 - navMeshAgentRadius / radius);

        }

        new void Update()
        {
            base.Update();

            //if (members != null)
            //{
            //    foreach (var member in members)
            //    {
            //        //Debug.DrawLine(transform.position, member.Key.transform.position, Color.green);
            //        //Debug.DrawLine(positions[member.Value] + transform.position, member.Key.transform.position, Color.green);
            //    }
            //}
        }

        public float GetTime()
        {
            return Random.value * (maxTime - minTime) + minTime;
        }

        public bool CanAddMember
        {
            get
            {
                return indices.Count > 0;
            }
        }

        public (float, Vector3) AddMember(INavigable agent)
        {
            if (members.ContainsKey(agent))
                return positions[members[agent]];

            if (!CanAddMember)
            {
                return (0, Vector3.zero);
            }
            var ind = indices.Dequeue();
            members.Add(agent, ind);
            agent.tag = MemberTag;
            return positions[ind];
        }

        public void RemoveMember(INavigable agent)
        {
            agent.tag = SEAN.SEAN.AgentTag;
            if (!members.ContainsKey(agent))
            {
                return;
            }
            var ind = members[agent];
            members.Remove(agent);
            indices.Enqueue(ind);
        }
    }
}