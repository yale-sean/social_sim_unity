using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class AgentManager : MonoBehaviour
{
    public int PathfindingFrequency = 25;
    public int PerceptionRadius = 2;
    public GameObject agentPrefab;
    public GameObject agentParent;

    public Dictionary<GameObject, Agent> agentsObjs = new Dictionary<GameObject, Agent>();
    public Dictionary<GameObject, Vector3> agentsDests = new Dictionary<GameObject, Vector3>();

    protected List<Agent> agents = new List<Agent>();

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

    protected IEnumerator CoroutineRunner() {
        yield return null;

        for (int frames = 0; ; frames++)
        {
            if (frames % PathfindingFrequency == 0)
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
        agentsDests.Add(obj, DestinationLocation());
    }

    public void RemoveAgent(GameObject obj) {
        var agent = obj.GetComponent<Agent>();

        agents.Remove(agent);
        agentsObjs.Remove(obj);
    }

    public void DestroyAll() {
        StopCoroutine(CoroutineRunner());
        foreach (var agent in agents) {
            agentsObjs.Remove(agent.gameObject);
        }
        agents.Clear();
        agentsObjs.Clear();
        agentsDests.Clear();
        foreach (Transform child in agentParent.transform) {
            Debug.Log("Destroying " + child);
            Destroy(child.gameObject);
        }
    }
    
    public abstract Vector3 SpawnLocation();

    public abstract Vector3 DestinationLocation();
}