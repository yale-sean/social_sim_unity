#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace IVI
{
    [ExecuteInEditMode]
    public class SamplePoints : MonoBehaviour
    {
        public bool activate = false;
        public float stepSize = 1f;
        private int padding = 0;

        #region Enums

        private enum Activity
        {
            NONE,
            NAV_NODE,
            GROUP_NAV_NODE,
            UNI_EDGE,
            BI_EDGE,
            EMPTY_EDGE,
            HIGH_EDGE,
        }

        #endregion

        void Update()
        {
            if (EditorApplication.isPlaying)
                return;

            if (activate)
            {
                activate = false;

                var nodes = GameObject.FindObjectsOfType<NavNode>();
                var edges = GameObject.FindObjectsOfType<NavEdge>();

                Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                foreach (var node in nodes)
                {
                    min.x = Mathf.Min(min.x, node.transform.position.x - node.radius);
                    min.y = Mathf.Min(min.y, node.transform.position.y);
                    min.z = Mathf.Min(min.z, node.transform.position.z - node.radius);

                    max.x = Mathf.Max(max.x, node.transform.position.x + node.radius);
                    max.y = Mathf.Max(max.y, node.transform.position.y);
                    max.z = Mathf.Max(max.z, node.transform.position.z + node.radius);
                }
                min.x = Mathf.Floor(min.x) - padding;
                min.y = Mathf.Floor(min.y);
                min.z = Mathf.Floor(min.z) - padding;
                max.x = Mathf.Ceil(max.x) + padding;
                max.y = Mathf.Ceil(max.y);
                max.z = Mathf.Ceil(max.z) + padding;

                var size = max - min;
                print(size);
                print(nodes.Length + " nodes");

                var positions = new Dictionary<Vector3, Activity>();

                for (float z = min.z; z < max.z; z += stepSize)
                    for (float x = min.x; x < max.x; x += stepSize)
                    {
                        var pos = new Vector3(x, min.y, z);
                        NavMeshHit navMeshHit;
                        NavMesh.SamplePosition(pos, out navMeshHit, float.MaxValue, LayerMask.GetMask("Default"));
                        pos.y = navMeshHit.position.y;

                        if (Vector3.Distance(navMeshHit.position, pos) < 1)
                        {
                            positions.Add(pos, Activity.NONE);

                            //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.green, 1);
                        }
                    }
                print(positions.Count + " positions");

                var edgePolygons = new Dictionary<List<Vector3>, NavEdge.Constraint>();
                foreach (var edge in edges)
                {
                    var pos1 = edge.node1.transform.position;
                    var pos2 = edge.node2.transform.position;
                    var rad1 = edge.node1.radius;
                    var rad2 = edge.node2.radius;
                    var tan = Vector3.Cross(pos1 - pos2, Vector3.up).normalized;

                    var polygon = new List<Vector3>();
                    polygon.Add(pos1 + tan * rad1);
                    polygon.Add(pos2 + tan * rad2);
                    polygon.Add(pos2 - tan * rad2);
                    polygon.Add(pos1 - tan * rad1);
                    edgePolygons.Add(polygon, edge.constraint);
                }

                foreach (var pos in positions.Keys.ToList())
                {
                    foreach (var node in nodes)
                    {
                        var nodePos = node.transform.position;
                        nodePos.y = pos.y;
                        if (Vector3.Distance(pos, nodePos) <= node.radius)
                        {
                            if (node.GetType() == typeof(GroupNavNode))
                            {
                                positions[pos] = Activity.GROUP_NAV_NODE;
                                //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.blue, 1);
                            }
                            else if (node.GetType() == typeof(NavNode))
                            {
                                positions[pos] = Activity.NAV_NODE;
                                //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.red, 1);
                            }
                            goto Outer;
                        }
                    }

                    foreach (var edge in edgePolygons)
                    {
                        if (PolygonContains(edge.Key, pos))
                        {
                            if (edge.Value == NavEdge.Constraint.BACKWARD_FLOW || edge.Value == NavEdge.Constraint.FORWARD_FLOW)
                            {
                                positions[pos] = Activity.UNI_EDGE;
                                //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.cyan, 1);
                            }
                            else if (edge.Value == NavEdge.Constraint.NO_FLOW)
                            {
                                positions[pos] = Activity.EMPTY_EDGE;
                                //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.black, 1);
                            }
                            else if (edge.Value == NavEdge.Constraint.HIGH_FLOW)
                            {
                                positions[pos] = Activity.HIGH_EDGE;
                                //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.red, 1);
                            }
                            else
                            {
                                positions[pos] = Activity.BI_EDGE;
                                //Debug.DrawLine(pos, pos + Vector3.up * 3, Color.green, 1);
                            }
                            goto Outer;
                        }
                    }
                Outer:;
                }

                var root = Application.dataPath + "/../Output/";
                var savePath = root + SceneManager.GetActiveScene().name + ".csv";
                Directory.CreateDirectory(root);
                File.WriteAllText(savePath, "x, y, z, Activity\n");
                File.AppendAllLines(savePath, positions.Select(p => p.Key.x + "," + p.Key.y + "," + p.Key.z + "," + p.Value));
            }
        }

        private bool PolygonContains(List<Vector3> polygon, Vector3 point)
        {
            point.y = polygon.First().y;
            for (int i = 0; i < polygon.Count; i++)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % polygon.Count];

                if (Vector3.Dot(Vector3.Cross(b - a, Vector3.up), point - a) < 0)
                    return false;
            }

            return true;
        }
    }
}
#endif