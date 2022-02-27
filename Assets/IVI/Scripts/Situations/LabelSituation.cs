//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace IVI {
//    public class LabelSituation : MonoBehaviour
//    {

//        public float UPDATE_RATE = 0.5f;
//        public float PERCEPTION_RANGE = 10;
//        public float AGENT_HEIGHT = 1.6f;
//        public float GROUP_THRESH = 3;
//        public RobotMovement robotMovement;
//        public Transform goal;
//        public SituationChoice situationChoice;
//        public bool publishToRos;
//        public string rosTopicName = "/social_sim/situation_label";


//        private Camera cam;
//        private Vector3 centerOffset;
//        private StringPublisher stringPublisher;

//        #region Enums

//        public enum RobotMovement
//        {
//            FORWARD,
//            GOAL
//        }

//        public enum Situation
//        {
//            CROSS_PATH,
//            PARALLEL_PATH,
//            EMPTY,
//            JOIN_GROUP,
//            LEAVE_GROUP,
//            NONE
//        }

//        public enum SituationType
//        {
//            PATH,
//            GROUP,
//            NONE
//        }

//        public enum SituationChoice
//        {
//            STRICT,
//            CLOSEST,
//            DISTANCE_WEIGHTED
//        }

//        #endregion

//        void Start()
//        {
//            cam = GetComponent<Camera>();
//            centerOffset = Vector3.up * AGENT_HEIGHT / 2;

//            if (goal != null)
//                robotMovement = RobotMovement.GOAL;

//            if (publishToRos)
//            {
//                stringPublisher = gameObject.AddComponent<StringPublisher>() as StringPublisher;
//                stringPublisher.topicName = rosTopicName;
//            }

//            if (cam != null && NavManager.inst != null)
//                StartCoroutine(Run());
//        }

//        private IEnumerator Run()
//        {
//            while (true)
//            {
//                Situation situation = PollSituation();
//                if (publishToRos)
//                {
//                    stringPublisher.Send(situation.ToString());
//                }
//                else
//                {
//                    print(situation);
//                }
//                yield return new WaitForSeconds(UPDATE_RATE);
//            }

//            yield break;
//        }

//        public Situation PollSituation()
//        {
//            var agentSituations = new Dictionary<SituationType, List<INavigable>>();
//            agentSituations.Add(SituationType.PATH, new List<INavigable>());
//            agentSituations.Add(SituationType.GROUP, new List<INavigable>());
//            agentSituations.Add(SituationType.NONE, new List<INavigable>());

//            // Looking at all people w/ in some perception range
//            foreach (var agent in NavManager.inst.allAgents.Where(a => Vector3.SqrMagnitude(a.transform.position - transform.position) < PERCEPTION_RANGE * PERCEPTION_RANGE))
//            {
//                var dist = Vector3.SqrMagnitude(agent.transform.position - transform.position);

//                var visible = true;
//                Vector3 screenPoint = cam.WorldToViewportPoint(agent.transform.position);
//                // Checking people visible
//                if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
//                {
//                    foreach (var hit in Physics.RaycastAll(transform.position, agent.transform.position + centerOffset - transform.position, PERCEPTION_RANGE).Where(g => Vector3.SqrMagnitude(g.transform.position - transform.position) < dist))
//                        if (hit.transform.tag != INavigable.AgentTag)
//                            visible = false;
//                }
//                else
//                {
//                    visible = false;
//                }

//                // Combine distance (360 in closeEnoughToRobot) and visbility (given camera FOV)
//                var closeEnoughToRobot = dist <= GROUP_THRESH;
//                if (visible || closeEnoughToRobot)
//                {
//                    //Debug.DrawLine(agent.transform.position, agent.transform.position + Vector3.up * 3, Color.green, UPDATE_RATE);

//                    var prevNode = NavManager.inst.prevAgentNode[agent];
//                    var nextNode = NavManager.inst.nextAgentNode[agent];
//                    var agentFarEnoughFromDest =
//                        Vector3.Distance(agent.transform.position, prevNode.transform.position) >= prevNode.radius &&
//                        Vector3.Distance(agent.transform.position, nextNode.transform.position) >= nextNode.radius;

//                    // TODO: currently observes a person leaving or joining a group
//                    //   should be the robot joining or leaving the group
//                    var agentInGroup = IsGroup(prevNode) && IsGroup(nextNode);
//                    var agentOnPath = !IsGroup(prevNode) && !IsGroup(nextNode) && agentFarEnoughFromDest;
//                    // Grouping based on their current task
//                    if (agentInGroup)
//                        agentSituations[SituationType.GROUP].Add(agent);
//                    else if (agentOnPath)
//                        agentSituations[SituationType.PATH].Add(agent);
//                    else
//                        agentSituations[SituationType.NONE].Add(agent);
//                }
//            }

//            var groupCount = agentSituations[SituationType.GROUP].Count;
//            var pathCount = agentSituations[SituationType.PATH].Count;
//            var noneCount = agentSituations[SituationType.NONE].Count;

//            Vector3 robotDir = Vector3.zero;
//            if (robotMovement == RobotMovement.FORWARD)
//                robotDir = transform.forward;
//            else if (robotMovement == RobotMovement.GOAL)
//                robotDir = goal.transform.position - transform.position;

//            Situation situation = Situation.NONE;
//            if (situationChoice == SituationChoice.STRICT)
//            {
//                #region STRICT

//                if (groupCount == 0 && pathCount == 0 && noneCount == 0)
//                {
//                    situation = Situation.EMPTY;
//                }
//                else if (groupCount > 0 && pathCount == 0)
//                {
//                    var closestGroup = agentSituations[SituationType.GROUP].Select(a => a.destNode).MinBy(d => Vector3.Distance(transform.position, d.transform.position));
//                    var groupDir = closestGroup.transform.position - transform.position;
//                    groupDir.y = 0;
//                    if (Vector3.Angle(robotDir, groupDir) < 90)
//                        situation = Situation.JOIN_GROUP;
//                    else
//                        situation = Situation.LEAVE_GROUP;
//                }
//                else if (pathCount > 0 && groupCount == 0)
//                {
//                    var closestAgent = agentSituations[SituationType.PATH].MinBy(a => Vector3.Distance(transform.position, a.transform.position));

//                    var pathDir1 = NavManager.inst.prevAgentNode[closestAgent].transform.position - NavManager.inst.nextAgentNode[closestAgent].transform.position;
//                    var pathDir2 = -pathDir1;

//                    if (Mathf.Min(Vector3.Angle(robotDir, pathDir1), Vector3.Angle(robotDir, pathDir2)) < 45)
//                        situation = Situation.PARALLEL_PATH;
//                    else
//                        situation = Situation.CROSS_PATH;
//                }
//                else
//                {
//                    situation = Situation.NONE;
//                }

//                #endregion
//            }
//            else if (situationChoice == SituationChoice.CLOSEST)
//            {
//                #region CLOSEST

//                if (groupCount > 0 && pathCount == 0)
//                {
//                    var closestGroup = agentSituations[SituationType.GROUP].Select(a => a.destNode).MinBy(d => Vector3.Distance(transform.position, d.transform.position));
//                    var groupDir = closestGroup.transform.position - transform.position;
//                    groupDir.y = 0;
//                    if (Vector3.Angle(robotDir, groupDir) < 90)
//                        situation = Situation.JOIN_GROUP;
//                    else
//                        situation = Situation.LEAVE_GROUP;
//                }
//                else if (pathCount > 0 && groupCount == 0)
//                {
//                    var closestAgent = agentSituations[SituationType.PATH].MinBy(a => Vector3.Distance(transform.position, a.transform.position));

//                    var pathDir1 = NavManager.inst.prevAgentNode[closestAgent].transform.position - NavManager.inst.nextAgentNode[closestAgent].transform.position;
//                    var pathDir2 = -pathDir1;

//                    if (Mathf.Min(Vector3.Angle(robotDir, pathDir1), Vector3.Angle(robotDir, pathDir2)) < 45)
//                        situation = Situation.PARALLEL_PATH;
//                    else
//                        situation = Situation.CROSS_PATH;
//                }
//                else if (groupCount > 0 && pathCount > 0)
//                {
//                    var minPathAgentDist = agentSituations[SituationType.PATH].Min(a => Vector3.Distance(transform.position, a.transform.position));
//                    var minGroupAgentDist = agentSituations[SituationType.GROUP].Min(a => Vector3.Distance(transform.position, a.transform.position));

//                    if (minGroupAgentDist < minPathAgentDist)
//                    {
//                        var closestGroup = agentSituations[SituationType.GROUP].Select(a => a.destNode).MinBy(d => Vector3.Distance(transform.position, d.transform.position));
//                        var groupDir = closestGroup.transform.position - transform.position;
//                        groupDir.y = 0;
//                        if (Vector3.Angle(robotDir, groupDir) < 90)
//                            situation = Situation.JOIN_GROUP;
//                        else
//                            situation = Situation.LEAVE_GROUP;
//                    }
//                    else
//                    {
//                        var closestAgent = agentSituations[SituationType.PATH].MinBy(a => Vector3.Distance(transform.position, a.transform.position));

//                        var pathDir1 = NavManager.inst.prevAgentNode[closestAgent].transform.position - NavManager.inst.nextAgentNode[closestAgent].transform.position;
//                        var pathDir2 = -pathDir1;

//                        if (Mathf.Min(Vector3.Angle(robotDir, pathDir1), Vector3.Angle(robotDir, pathDir2)) < 45)
//                            situation = Situation.PARALLEL_PATH;
//                        else
//                            situation = Situation.CROSS_PATH;
//                    }
//                }
//                else if (groupCount == 0 && pathCount == 0 && noneCount == 0)
//                {
//                    situation = Situation.EMPTY;
//                }
//                else
//                {
//                    situation = Situation.NONE;
//                }

//                #endregion
//            }
//            else if (situationChoice == SituationChoice.DISTANCE_WEIGHTED)
//            {
//                #region DISTANCE_WEIGHTED

//                // A robot's situation is influenced the most by the nearest agents
//                var choice = 0f;    // Positive is goal, negative is path
//                foreach (var agent in agentSituations[SituationType.GROUP])
//                    choice += 1 * Mathf.Exp(-Vector3.Distance(agent.transform.position, transform.position));
//                foreach (var agent in agentSituations[SituationType.PATH])
//                    choice += -1 * Mathf.Exp(-Vector3.Distance(agent.transform.position, transform.position));

//                // Sign indicates group or path scenario
//                // TODO: what if you are passing a group (but not joining or leaving it) on the way to join another group
//                if (choice > 0)
//                {
//                    // Then look at task related to the group and decide the sub-type
//                    var closestGroup = agentSituations[SituationType.GROUP].Select(a => a.destNode).MinBy(d => Vector3.Distance(transform.position, d.transform.position));
//                    var groupDir = closestGroup.transform.position - transform.position;
//                    groupDir.y = 0;
//                    if (Vector3.Angle(robotDir, groupDir) < 90)
//                        situation = Situation.JOIN_GROUP;
//                    else
//                        situation = Situation.LEAVE_GROUP;
//                }
//                else if (choice < 0)
//                {
//                    // Then look at paths/trajectories and decide the sub-type
//                    var closestAgent = agentSituations[SituationType.PATH].MinBy(a => Vector3.Distance(transform.position, a.transform.position));

//                    var pathDir1 = NavManager.inst.prevAgentNode[closestAgent].transform.position - NavManager.inst.nextAgentNode[closestAgent].transform.position;
//                    var pathDir2 = -pathDir1;

//                    if (Mathf.Min(Vector3.Angle(robotDir, pathDir1), Vector3.Angle(robotDir, pathDir2)) < 45)
//                        situation = Situation.PARALLEL_PATH;
//                    else
//                        situation = Situation.CROSS_PATH;
//                }
//                else
//                {
//                    situation = Situation.EMPTY;
//                }

//                #endregion
//            }

//            return situation;
//        }

//        private bool IsGroup(object node)
//        {
//            return node.GetType() == typeof(GroupNavNode);
//        }
//    }
//  }
