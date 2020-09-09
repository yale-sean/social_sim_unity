using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class StandaloneAgentManager : AgentManager
{
    public int agentCount = 10;
    public bool autoStart = false;
    public string SpawnTag = "Spawn";
    private List<Transform> possiblePositions = new List<Transform>();

    void Awake() {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(SpawnTag)) {
            possiblePositions.Add(obj.transform);
        }
    }

    void Start() {
        if (autoStart)
            GenerateAgents();
    }

    public void GenerateAgents() {
        for (int i = 0; i < agentCount; i++)
        {
            var randPos = SpawnLocation() + Vector3.up;

            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "Agent " + i;
            agent.transform.parent = agentParent.transform;

            agent = agent.transform.GetChild(0).gameObject;

            var agentScript = agent.GetComponent<Agent>();
            agentScript.enabled = true;
            agentScript.agentManager = this;
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 60;
            agentScript.perceptionRadius = PerceptionRadius;

            agents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
            agentsDests.Add(agent, SpawnLocation());
        }

        StartCoroutine(CoroutineRunner());
    } 
    public override Vector3 SpawnLocation() {
        int idx = Random.Range(0, possiblePositions.Count-1);
        NavMeshHit hit;
        NavMesh.SamplePosition(possiblePositions[idx].position, out hit, 10, NavMesh.AllAreas);
        return hit.position;
    }
}