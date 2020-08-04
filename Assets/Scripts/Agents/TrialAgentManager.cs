using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialAgentManager : AgentManager
{
    public RosSharp.RosBridgeClient.PoseArrayPublisher poseArrayPublisher;

    public void GenerateAgents(List<Vector3> peoplePositions, List<Quaternion> peopleRotations) {
        for(int i = 0; i < peoplePositions.Count; i++) {
            var randPos = peoplePositions[i] + Vector3.up;

            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "Agent_" + i;
            agent.transform.parent = agentParent.transform;
            // position = peoplePositions[i];
            agent = agent.transform.GetChild(0).gameObject;

            var agentScript = agent.GetComponent<Agent>();
            agentScript.agentManager = this;
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 150;
            agentScript.perceptionRadius = PerceptionRadius;

            agents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
            agentsDests.Add(agent, SpawnLocation());
        }

        StartCoroutine(CoroutineRunner());
    }
    
    public override Vector3 SpawnLocation() {
        int idx = Random.Range(0, poseArrayPublisher.possiblePositions.Count-1);
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(poseArrayPublisher.possiblePositions[idx].position, out hit, 10, UnityEngine.AI.NavMesh.AllAreas);
        return hit.position;
    }
}
