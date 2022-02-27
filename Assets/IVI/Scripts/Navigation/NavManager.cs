using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IVI
{
    [ExecuteInEditMode]
    public class NavManager : SEAN.Scenario.Agents.BaseAgentManager
    {
        public static NavManager inst;
        public NavNode[] allNavNodes;
        public GroupNavNode[] allGroupNodes;
        public NavEdge[] allEdges;
        public INavigable[] allAgents;
        public Dictionary<NavNode, int> node2Index;
        public Dictionary<NavNode, int> groupNode2Index;
        public bool[,] adjMatrix;

        public int[] nodeOccupancy;
        public int[,] edgeOccupancy;
        public int[] nodeDesired;
        public int[,] edgeDesired;
        public int[] nodeDiff;

        public Dictionary<INavigable, NavNode> prevAgentNode;
        public Dictionary<INavigable, NavNode> nextAgentNode;

        public GameObject nodePrefab;
        public GameObject edgePrefab;
        public GameObject groupNodePrefab;
        public GameObject nodesGO;
        public GameObject edgesGO;
        public GameObject agentsGO;

        public const float NODE_RADIUS = 1;
        public const float EDGE_HEIGHT = 0.5f;
        public const float EDGE_WIDTH = 1f;
        public bool VISUALIZE = true;
        public float SPAWN_HEIGHT = 0;

        public GameObject agentPrefab;

        void Awake()
        {
            inst = this;
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                VISUALIZE = false;

                StartCoroutine(Run());
            }
            else
            {
                VISUALIZE = true;
            }
        }

        void Update()
        {
            inst = this;
        }

        IEnumerator Run()
        {
            yield return null;

            allNavNodes = GameObject.FindObjectsOfType<NavNode>();

            #region Spawn Agents

            int j = 0;
            foreach (var node in allNavNodes)
            {
                for (int i = 0; i < node.spawnCount; i++)
                {
                    var theta = Mathf.PI * 2 * Random.value;
                    var pos = node.transform.position + new Vector3(Mathf.Sin(theta), SPAWN_HEIGHT, Mathf.Cos(theta)) * Random.value * node.radius;
                    var sfRandom = Instantiate(agentPrefab, pos, Quaternion.identity);
                    var agent = sfRandom.GetComponentInChildren<INavigable>();
                    agent.name = "Agent_" + j++;
                    agent.transform.parent = agentsGO.transform;
                }
            }

            allGroupNodes = GameObject.FindObjectsOfType<GroupNavNode>();

            foreach (GroupNavNode node in allGroupNodes)
            {
                for (int i = 0; i < node.spawnCount; i++)
                {
                    if (!node.CanAddMember)
                    {
                        break;
                    }
                    var sfRandom = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);
                    var agent = sfRandom.GetComponentInChildren<INavigable>();
                    agent.name = "Agent_" + j++;
                    (float, Vector3) pose = node.AddMember(agent);
                    float degRot = pose.Item1 * (180 / Mathf.PI);
                    //print(agent.name + " rad: " + pose.Item1 + " : deg : " + degRot);
                    agent.transform.rotation = Quaternion.Euler(0, degRot, 0);
                    agent.transform.position = pose.Item2;
                    agent.transform.parent = agentsGO.transform;
                }
            }
            yield return null;  // Allow the agents to spawn

            #endregion

            allEdges = GameObject.FindObjectsOfType<NavEdge>();
            allAgents = GameObject.FindObjectsOfType<INavigable>();
            node2Index = new Dictionary<NavNode, int>();
            groupNode2Index = new Dictionary<NavNode, int>();
            adjMatrix = new bool[allNavNodes.Length, allNavNodes.Length];
            #region Initialize Graph

            for (int i = 0; i < allNavNodes.Length; i++)
            {
                node2Index.Add(allNavNodes[i], i);
            }
            foreach (var groupNode in GameObject.FindObjectsOfType<GroupNavNode>())
            {
                groupNode2Index.Add(groupNode, node2Index[groupNode]);
            }

            foreach (var edge in allEdges)
            {
                var a = node2Index[edge.node1];
                var b = node2Index[edge.node2];

                adjMatrix[a, b] = true;
                adjMatrix[b, a] = true;

                edge.node1.GetNeighbors().Add(edge.node2, edge);
                edge.node2.GetNeighbors().Add(edge.node1, edge);
            }

            #endregion

            nodeOccupancy = new int[allNavNodes.Length];
            edgeOccupancy = new int[allNavNodes.Length, allNavNodes.Length];
            prevAgentNode = new Dictionary<INavigable, NavNode>();
            nextAgentNode = new Dictionary<INavigable, NavNode>();

            #region Initialize Agent Nodes

            foreach (INavigable agent in allAgents)
            {
                if (agent.tag == GroupNavNode.MemberTag)
                {
                    continue;
                }
                var closestNode = allNavNodes.Aggregate((a, b) => Vector3.Distance(agent.transform.position, a.transform.position) < Vector3.Distance(agent.transform.position, b.transform.position) ? a : b);
                prevAgentNode.Add(agent, closestNode);
                nextAgentNode.Add(agent, closestNode);
                var ind = node2Index[closestNode];
                nodeOccupancy[ind]++;
                edgeOccupancy[ind, ind] += 2;
            }

            #endregion

            UpdateDesiredOccupancy();

            #region Start Navigation

            foreach (var agent in allAgents)
            {
                if (agent.tag == GroupNavNode.MemberTag)
                {
                    continue;
                }
                yield return UpdateAgentGoal(agent);
            }

            #endregion

            yield break;
        }

        #region Public Functions

        public GameObject CreateNode(GameObject example)
        {
            var GO = GameObject.Instantiate(nodePrefab);
            GO.transform.position = example.transform.position;
            GO.transform.parent = nodesGO.transform;
            GO.name = "Node " + (GameObject.FindObjectsOfType<NavNode>().Length);
            GO.GetComponent<NavNode>().radius = NODE_RADIUS;

            return GO;
        }

        public GameObject CreateGroupNode(GameObject example)
        {
            var GO = GameObject.Instantiate(groupNodePrefab);
            GO.transform.position = example.transform.position;
            GO.transform.parent = nodesGO.transform;
            GO.name = "Group Node " + (GameObject.FindObjectsOfType<NavNode>().Length);
            GO.GetComponent<NavNode>().radius = NODE_RADIUS;

            return GO;
        }

        public NavEdge CreateEdge(NavNode node1, NavNode node2)
        {
            var GO = GameObject.Instantiate(edgePrefab);
            GO.transform.parent = edgesGO.transform;
            GO.name = "Edge " + (GameObject.FindObjectsOfType<NavEdge>().Length);
            var navEdge = GO.GetComponent<NavEdge>();
            navEdge.node1 = node1;
            navEdge.node2 = node2;
            navEdge.width = EDGE_WIDTH;

            return navEdge;
        }

        public void UpdateDesiredOccupancy()
        {
            nodeDesired = new int[allNavNodes.Length];
            edgeDesired = new int[allNavNodes.Length, allNavNodes.Length];
            foreach (var node in allNavNodes)
            {
                if (node.GetType() == typeof(GroupNavNode))
                {
                    var groupNode = (GroupNavNode)node;
                    nodeDesired[node2Index[groupNode]] = groupNode.groupSize;
                }
            }
            foreach (var edge in allEdges)
            {
                var a = node2Index[edge.node1];
                var b = node2Index[edge.node2];

                //edgeDesired[a, b] = edge.size;
                //edgeDesired[b, a] = edge.size;

                var maxScale = 100;
                switch (edge.constraint)
                {
                    case NavEdge.Constraint.NONE:
                        edgeDesired[a, b] = 1;
                        edgeDesired[b, a] = 1;

                        break;
                    case NavEdge.Constraint.NO_FLOW:
                        edgeDesired[a, b] = maxScale;
                        edgeDesired[b, a] = maxScale;

                        break;
                    case NavEdge.Constraint.HIGH_FLOW:
                        edgeDesired[a, b] = -1;
                        edgeDesired[b, a] = -1;

                        break;
                    case NavEdge.Constraint.FORWARD_FLOW:
                        edgeDesired[a, b] = 1;
                        edgeDesired[b, a] = maxScale;

                        break;
                    case NavEdge.Constraint.BACKWARD_FLOW:
                        edgeDesired[a, b] = maxScale;
                        edgeDesired[b, a] = 1;

                        break;
                }
            }
        }

        public IEnumerator UpdateAgentGoal(INavigable agent)
        {
            UpdateDesiredOccupancy();

            #region Compute Node and Edge Differences

            nodeDiff = new int[allNavNodes.Length];
            for (int i = 0; i < allNavNodes.Length; i++)
            {
                nodeDiff[i] = nodeDesired[i] - nodeOccupancy[i];
            }

            #endregion

            #region Initialize Prev, Curr, and Next Nodes

            var prevNode = prevAgentNode[agent];
            var currNode = nextAgentNode[agent];
            var prevInd = node2Index[prevNode];
            var currInd = node2Index[currNode];

            var nextInd = Random.Range(0, allNavNodes.Length);
            var nextNode = allNavNodes[nextInd];
            while (nextInd == currInd || groupNode2Index.ContainsKey(nextNode))
            {
                nextInd = Random.Range(0, allNavNodes.Length);
                nextNode = allNavNodes[nextInd];
            }

            #endregion

            #region Check Group Nodes

            if (groupNode2Index.Count > 0)
            {
                var maxGroupNode = Extension.MaxBy(groupNode2Index, p => nodeDiff[p.Value]);
                if (nodeDiff[maxGroupNode.Value] > 0 && maxGroupNode.Value != currInd)
                {
                    nextInd = maxGroupNode.Value;
                    nextNode = maxGroupNode.Key;
                }
            }

            #endregion

            #region Avoid Group Nodes

            var path = Dijkstra(currNode, nextNode);
            nextNode = path[1];
            nextInd = node2Index[nextNode];
            while (nextNode.GetType() == typeof(GroupNavNode) && nodeDiff[nextInd] <= 0)
            {
                while (nextInd == currInd || groupNode2Index.ContainsKey(nextNode))
                {
                    nextInd = Random.Range(0, allNavNodes.Length);
                    nextNode = allNavNodes[nextInd];
                }
                nextNode = Dijkstra(currNode, nextNode)[1];
                nextInd = node2Index[nextNode];

                yield return null;
            }

            #endregion

            #region Update Occupancy

            edgeOccupancy[prevInd, currInd]--;
            edgeOccupancy[currInd, nextInd]++;
            nodeOccupancy[currInd]--;
            nodeOccupancy[nextInd]++;

            prevAgentNode[agent] = currNode;
            nextAgentNode[agent] = nextNode;

            #endregion

            #region Start Behavior

            if (groupNode2Index.ContainsKey(nextNode))
            {
                GroupNavNode groupNode = (GroupNavNode)nextNode;
                (float, Vector3) pose = groupNode.AddMember(agent);
                agent.InitDest(nextNode, pose.Item2);
            }
            else
            {
                float theta = Mathf.PI * 2 * Random.value;
                Vector3 position = new Vector3(Mathf.Sin(theta), 0, Mathf.Cos(theta)) * Random.value * nextNode.radius;
                agent.InitDest(nextNode, position);
            }

            #endregion

            yield break;
        }

        #endregion

        #region Utility Functions

        private int MaxIndex(int[] arr)
        {
            float max = float.MinValue;
            int max_index = -1;

            for (int i = 0; i < arr.Length; i++)
                if (arr[i] >= max)
                {
                    max = arr[i];
                    max_index = i;
                }

            return max_index;
        }

        private List<NavNode> Dijkstra(NavNode start, NavNode goal) //Agents still go back to prev
        {
            var closed = new bool[allNavNodes.Length];
            var dist = new float[allNavNodes.Length];
            var par = new int[allNavNodes.Length];

            for (int i = 0; i < dist.Length; i++)
                dist[i] = float.MaxValue;

            var startInd = node2Index[start];
            dist[startInd] = 0;
            par[startInd] = -1;
            for (int i = 0; i < dist.Length; i++)
            {
                int currInd = MinDistIndex(dist, closed);
                closed[currInd] = true;

                if (currInd == node2Index[goal])
                {
                    break;
                }

                for (int j = 0; j < dist.Length; j++)
                {
                    if (!closed[j] &&
                        adjMatrix[currInd, j])
                    {
                        //var newDist = dist[currInd] + Vector3.Distance(allNavNodes[currInd].transform.position, allNavNodes[j].transform.position) * Mathf.Exp(-1 * edgeDiff[currInd, j]);
                        //var newDist = dist[currInd] + 1 * Mathf.Exp(-1 * edgeDiff[currInd, j]);

                        var newDist = dist[currInd] + Vector3.Distance(allNavNodes[currInd].transform.position, allNavNodes[j].transform.position) * edgeDesired[currInd, j];
                        //var newDist = dist[currInd] + 1 * edgeDesired[currInd, j];
                        if (dist[currInd] != float.MaxValue &&
                            newDist < dist[j])
                        {
                            dist[j] = newDist;
                            par[j] = currInd;
                        }
                    }
                }
            }

            var path = new List<NavNode>();
            var tempInd = node2Index[goal];
            while (tempInd != -1)
            {
                path.Add(allNavNodes[tempInd]);

                tempInd = par[tempInd];
            }
            path.Reverse();

            return path;
        }

        private int MinDistIndex(float[] dist, bool[] closed)
        {
            float min = float.MaxValue;
            int min_index = -1;

            for (int i = 0; i < dist.Length; i++)
                if (closed[i] == false && dist[i] <= min)
                {
                    min = dist[i];
                    min_index = i;
                }

            return min_index;
        }

        #endregion
    }

    public static class Extension
    {

        public static TSource MaxBy<TSource, TProperty>(this IEnumerable<TSource> source, System.Func<TSource, TProperty> selector)
        {
            // check args        

            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new System.InvalidOperationException();

                var max = iterator.Current;
                var maxValue = selector(max);
                var comparer = Comparer<TProperty>.Default;

                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    var currentValue = selector(current);

                    if (comparer.Compare(currentValue, maxValue) > 0)
                    {
                        max = current;
                        maxValue = currentValue;
                    }
                }

                return max;
            }
        }

        public static TSource MinBy<TSource, TProperty>(this IEnumerable<TSource> source, System.Func<TSource, TProperty> selector)
        {
            // check args        

            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new System.InvalidOperationException();

                var min = iterator.Current;
                var minValue = selector(min);
                var comparer = Comparer<TProperty>.Default;

                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    var currentValue = selector(current);

                    if (comparer.Compare(currentValue, minValue) < 0)
                    {
                        min = current;
                        minValue = currentValue;
                    }
                }

                return min;
            }
        }

    }
}