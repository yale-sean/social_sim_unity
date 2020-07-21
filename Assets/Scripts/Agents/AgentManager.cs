using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AgentManager : MonoBehaviour
{
    public int agentCount = 10;
    public float agentSpawnRadius = 20;
    public GameObject agentPrefab;
    public static Dictionary<GameObject, Agent> agentsObjs = new Dictionary<GameObject, Agent>();
    public static Dictionary<GameObject, Vector3> agentsDests = new Dictionary<GameObject, Vector3>();
    public bool autoStart = false;

    private static List<Agent> agents = new List<Agent>();
    private GameObject agentParent;
    public static AgentManager instance;

    private const int PATHFINDING_FRAME_SKIP = 25;
    private const float randomNavmeshRadius = 8f;

    #region Unity Functions

    void Awake()
    {
        instance = this;
        Random.InitState(0);
        agentParent = GameObject.Find("Agents");
    }

    void Start()
    {
        if (autoStart)
            GenerateAgents();
    }

    public void GenerateAgents()
    {
        for (int i = 0; i < agentCount; i++)
        {
            var randPos = RandomNavmeshLocation(randomNavmeshRadius);
            //NavMeshHit hit;
            //NavMesh.SamplePosition(randPos, out hit, 10, NavMesh.AllAreas);
            randPos = randPos + Vector3.up;

            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "Agent " + i;
            agent.transform.parent = agentParent.transform;

            Debug.Log(agent + " " + agent.transform.childCount);

            agent = agent.transform.GetChild(0).gameObject;

            var agentScript = agent.GetComponent<Agent>();
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 150;
            agentScript.perceptionRadius = 2;

            agents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
            agentsDests.Add(agent, RandomNavmeshLocation(randomNavmeshRadius));
        }

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
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

    public static bool IsAgent(GameObject obj)
    {
        return agentsObjs.ContainsKey(obj);
    }

    public static void SetAgentDestinations()
    {
        foreach (var agent in agents)
        {
            Vector3 destination;
            agentsDests.TryGetValue(agent.gameObject, out destination);
            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, 10, NavMesh.AllAreas);
            agent.ComputePath(hit.position);
        }
    }

    public static void UpdateAgentDestination(GameObject obj)
    {
        agentsDests.Remove(obj);
        agentsDests.Add(obj, RandomNavmeshLocation(randomNavmeshRadius));
    }

    public static void RemoveAgent(GameObject obj)
    {
        var agent = obj.GetComponent<Agent>();

        agents.Remove(agent);
        agentsObjs.Remove(obj);
    }

    public void DestroyAll()
    {
        StopCoroutine(Run());
        foreach (var agent in agents)
        {
            agentsObjs.Remove(agent.gameObject);
        }
        agents = new List<Agent>();
        foreach (Transform child in agentParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public static Vector3 RandomNavmeshLocation(float radius) {
         Vector3 randomDirection = Random.insideUnitSphere * radius;
         NavMeshHit hit;
         Vector3 finalPosition = Vector3.zero;
         if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
             finalPosition = hit.position;            
         }
         return finalPosition;
     }

    #endregion

    #region Private Functions

    #endregion

    #region Visualization Functions

    #endregion

}
