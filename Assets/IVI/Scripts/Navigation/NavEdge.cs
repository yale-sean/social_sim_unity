using UnityEngine;

namespace IVI
{
    [ExecuteInEditMode]
    public class NavEdge : MonoBehaviour
    {
        public NavNode node1, node2;
        public float width;
        public Constraint constraint = Constraint.NONE;

        private MeshRenderer render;

        #region Enums

        public enum Constraint
        {
            NONE,
            NO_FLOW,
            HIGH_FLOW,
            FORWARD_FLOW,
            BACKWARD_FLOW,
        }

        #endregion

        void Start()
        {
            render = GetComponent<MeshRenderer>();

            width = NavManager.EDGE_WIDTH;
        }

        void Update()
        {
            var dir = (node2.transform.position - node1.transform.position).normalized;
            var pos = (node1.transform.position + dir * node1.radius + node2.transform.position - dir * node2.radius) / 2;
            pos.y = NavManager.EDGE_HEIGHT / 2f;
            transform.position = pos;
            transform.LookAt(node1.transform, Vector3.up);
            var dist = Vector3.Distance(node1.transform.position, node2.transform.position) - node1.radius - node2.radius;
            transform.localScale = new Vector3(width, NavManager.EDGE_HEIGHT, dist);

            var eulerAngles = transform.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            transform.eulerAngles = eulerAngles;

            #region Appearance

            var angle = Mathf.Atan(NavManager.EDGE_HEIGHT / (dist / 2f)) * Mathf.Rad2Deg;
            if (constraint == Constraint.BACKWARD_FLOW)
            {
                eulerAngles.x = angle;
                transform.eulerAngles = eulerAngles;
                transform.position += Vector3.up * NavManager.EDGE_HEIGHT;
            }
            if (constraint == Constraint.FORWARD_FLOW)
            {
                eulerAngles.x = -angle;
                transform.eulerAngles = eulerAngles;
                transform.position += Vector3.up * NavManager.EDGE_HEIGHT;
            }
            if (constraint == Constraint.HIGH_FLOW)
            {
                var temp = transform.localScale;
                temp.y = NavManager.EDGE_HEIGHT * 2;
                transform.localScale = temp;
                temp = transform.position;
                temp.y = NavManager.EDGE_HEIGHT;
                transform.position = temp;
            }
            if (constraint == Constraint.NO_FLOW)
            {
                var temp = transform.localScale;
                temp.y = NavManager.EDGE_HEIGHT / 2;
                transform.localScale = temp;
                temp = transform.position;
                temp.y = NavManager.EDGE_HEIGHT / 4;
                transform.position = temp;
            }

            #endregion

            //if (Application.isPlaying || !NavManager.VISUALIZE)
            //{
            //    render.enabled = false;
            //}
            if (NavManager.inst != null)
            {
                render.enabled = NavManager.inst.VISUALIZE;

                var tempPos = transform.position;
                tempPos.y = NavManager.inst.SPAWN_HEIGHT;
                transform.position = tempPos;
            }
        }
    }
}
