using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IVI
{
    public abstract class INavigable : SEAN.Scenario.Trajectory.TrackedAgent
    {
        protected bool ShowDebug = true;

        public int plannerFPS = 5;

        private NavNode destNode;
        protected Vector3 destPos;
        private bool navigating = false;


        protected override void Start()
        {
            base.Start();
            if (tag == "Untagged")
            {
                tag = SEAN.SEAN.AgentTag;
            }
            StartCoroutine(Coroutine());
        }

        #region Abstract Behaviors

        protected abstract bool PlanNavigation();

        protected abstract void StopNavigation();

        protected abstract void StartGroup(GroupNavNode group);

        protected abstract void StopGroup(GroupNavNode group);

        #endregion

        protected virtual IEnumerator Coroutine()
        {
            while (true)
            {
                //print(name + " Coroutine");
                if (CloseEnough())
                {
                    StopNavigation();

                    navigating = false;

                    // Graph specific code
                    if (NavManager.inst && NavManager.inst.node2Index != null && destNode)
                    {
                        if (AtGroupNode())
                        {
                            StopNavigation();

                            NavManager.inst.prevAgentNode[this] = NavManager.inst.nextAgentNode[this];
                            var groupNode = (GroupNavNode)destNode;
                            var time = groupNode.GetTime();

                            var lookAtPos = groupNode.transform.position;
                            lookAtPos.y = transform.position.y;
                            transform.LookAt(lookAtPos, Vector3.up);
                            //StartGroup(groupNode);
                            //var initPos = transform.position;
                            //var initTime = Time.time;
                            //while (initTime + time <= Time.time)
                            //{
                            //    transform.position = initPos;
                            //    yield return null;
                            //}
                            yield return new WaitForSeconds(time);
                            StopGroup(groupNode);
                        }
                        yield return NavManager.inst.UpdateAgentGoal(this);
                    }
                }
                else
                {
                    PlanNavigation();
                }

                yield return new WaitForSeconds(1 / plannerFPS);
            }
        }

        private Vector3 SampledGoalPosition(Vector3 position, float dist = 0.25f)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(position, out hit, dist, NavMesh.AllAreas);
            Vector3 pos = hit.position;
            if (pos.x == Mathf.Infinity || pos.y == Mathf.Infinity || pos.z == Mathf.Infinity)
            {
                return SampledGoalPosition(position, dist + 0.25f);
            }
            return pos;
        }

        /// <summary>
        /// Assign the goal 
        /// </summary>
        /// <param name="position"></param>
        public void InitDest(Vector3 destPos)
        {
            this.destPos = SampledGoalPosition(destPos);
            navigating = true;
            PlanNavigation();
        }

        /// <summary>
        /// Assign the goal desitination node and position 
        /// </summary>
        /// <param name="position"></param>
        public void InitDest(NavNode destNode, Vector3 destPos)
        {
            this.destNode = destNode;
            Vector3 unsampledPose = this.destNode.transform.position + destPos;
            this.destPos = SampledGoalPosition(unsampledPose);
            navigating = true;
            PlanNavigation();
        }

        private bool AtGroupNode()
        {
            return NavManager.inst.groupNode2Index.ContainsKey(destNode);
        }

        public bool CloseEnough()
        {
            //print("closeenough " + name + " dest is " + destNode);
            var closeness = SEAN.Util.Geometry.GroundPlaneDist(destPos, transform.position);
            bool closeEnough = closeness <= Parameters.CLOSE_ENOUGH_MIN_DIST;
            //print("dist: " + dist + ", close: " + close);
            return closeEnough;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!ShowDebug) { return; }
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(destPos, new Vector3(0.25f, 0.25f, 0.25f));
            //Gizmos.DrawSphere(transform.position, 0.25f);
        }

    }
}
