using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AgentManager : MonoBehaviour
{
    public int agentCount = 10;
    public GameObject agentPrefab;
    public Dictionary<GameObject, Agent> agentsObjs = new Dictionary<GameObject, Agent>();
    public Dictionary<GameObject, Vector3> agentsDests = new Dictionary<GameObject, Vector3>();
    public bool autoStart = false;
    public string AgentTag = "Agents";
    // Tag for objects that can be robot and agent spawn locations
    public string SpawnTag = "Spawn";

    private List<Agent> agents = new List<Agent>();
    private GameObject agentParent;

    private const int PATHFINDING_FRAME_SKIP = 25;
    private List<Transform> possiblePositions = new List<Transform>();

    #region Unity Functions

    void Awake() {
        //Random.InitState(0);
        agentParent = GameObject.Find(AgentTag);
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

            //Debug.Log(agent + " " + agent.transform.childCount);

            agent = agent.transform.GetChild(0).gameObject;

            var agentScript = agent.GetComponent<Agent>();
            agentScript.agentManager = this;
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 150;
            agentScript.perceptionRadius = 2;

            agents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
            agentsDests.Add(agent, SpawnLocation());
        }

        StartCoroutine(Run());
    }

    void OnDrawGizmos() {
        int num_colors = agentsDests.Count;
        int c = 0;
        foreach (KeyValuePair<GameObject, Vector3> kv in agentsDests) {
            GameObject agent = kv.Key;
            Vector3 dest = kv.Value;
            Gizmos.color = Color.HSVToRGB((c++*(360.0f/num_colors)/360),1,1);
            Gizmos.DrawWireSphere(agent.transform.position, 0.25f);
            Gizmos.DrawWireCube(dest, new Vector3(0.25f, 0.25f, 0.25f));
        }
    }

    IEnumerator Run() {
        yield return null;

        for (int frames = 0; ; frames++)
        {
            if (frames % PATHFINDING_FRAME_SKIP == 0)
            {
                SetAgentDestinations();
            }

            foreach (var agent in agents)
            {
                agent.ApplyForce();
            }

            yield return null;
        }
    }

    #endregion

    #region Public Functions

    public bool IsAgent(GameObject obj) {
        return agentsObjs.ContainsKey(obj);
    }

    public void SetAgentDestinations() {
        foreach (var agent in agents)
        {
            Vector3 destination;
            agentsDests.TryGetValue(agent.gameObject, out destination);
            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, 10, NavMesh.AllAreas);
            agent.ComputePath(hit.position);
        }
    }

    public void UpdateAgentDestination(GameObject obj) {
        agentsDests.Remove(obj);
        agentsDests.Add(obj, SpawnLocation());
    }

    public void RemoveAgent(GameObject obj) {
        var agent = obj.GetComponent<Agent>();

        agents.Remove(agent);
        agentsObjs.Remove(obj);
    }

    public void DestroyAll() {
        StopCoroutine(Run());
        foreach (var agent in agents) {
            agentsObjs.Remove(agent.gameObject);
        }
        agents = new List<Agent>();
        foreach (Transform child in agentParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public Vector3 SpawnLocation() {
        // uniform sampling
        int idx = Random.Range(0, possiblePositions.Count-1);
        Transform trialSystem = GameObject.Find("TrialSystem").transform;
        NavMeshHit hit;
        NavMesh.SamplePosition(possiblePositions[idx].position, out hit, 10, NavMesh.AllAreas);
        return hit.position;
    }

    #endregion

    #region Private Functions

    #endregion

    #region Visualization Functions

    #endregion

}
