using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace IVI
{
    [ExecuteInEditMode]
    public class NavNode : MonoBehaviour
    {
        public bool createNode = false;
        public bool createGroupNode = false;
        public GameObject createConnection = null;
        public float radius;
        public int spawnCount = 0;

        private Dictionary<NavNode, NavEdge> neighbors = new Dictionary<NavNode, NavEdge>();
        [HideInInspector]
        public MeshRenderer render;

        public void Start()
        {
            render = GetComponent<MeshRenderer>();
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                #region Editor

                if (render == null)
                    render = GetComponent<MeshRenderer>();

                render.enabled = true;
                transform.localScale = Vector3.one * radius * 2;

                if (createNode)
                {
                    createNode = false;

                    var GO = NavManager.inst.CreateNode(gameObject);
                    Selection.activeGameObject = GO;
                    createConnection = GO;
                }
                if (createGroupNode)
                {
                    createGroupNode = false;

                    var GO = NavManager.inst.CreateGroupNode(gameObject);
                    Selection.activeGameObject = GO;
                    createConnection = GO;
                }
                if (createConnection != null)
                {
                    var otherNode = createConnection.GetComponent<NavNode>();
                    if (otherNode != null && otherNode != this)
                    {
                        var navEdge = NavManager.inst.CreateEdge(this, otherNode);
                    }

                    createConnection = null;
                }

                #region Visualization

                //for (int i = 0; i < neighbors.Count; i++)
                //{
                //    var n = neighbors[i];
                //    if (n == null)
                //    {
                //        neighbors.RemoveAt(i);
                //        i--;
                //    }
                //    else
                //    {
                //        Debug.DrawLine(transform.position + Vector3.up * NavManager.NODE_RADIUS, n.transform.position + Vector3.up * NavManager.NODE_RADIUS, Color.green);
                //    }
                //}

                #endregion

                #endregion
            }
#endif

            //if (Application.isPlaying || !NavManager.VISUALIZE)
            //{
            //    render.enabled = false;
            //}
            if (NavManager.inst != null)
            {
                render.enabled = NavManager.inst.VISUALIZE;

                var pos = transform.position;
                pos.y = NavManager.inst.SPAWN_HEIGHT;
                transform.position = pos;
            }
        }

        public void OnDestroy()
        {
            foreach (var go in GameObject.FindObjectsOfType<NavEdge>())
            {
                if (go.node1 == this || go.node2 == this)
                {
                    DestroyImmediate(go.gameObject);
                }
            }
        }

        #region Public Functions

        public Dictionary<NavNode, NavEdge> GetNeighbors()
        {
            return neighbors;
        }

        #endregion
    }
}
