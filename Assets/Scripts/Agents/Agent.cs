using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Agent : MonoBehaviour
{
    public AgentManager agentManager;
    public float NavPointRadius = 1.0f;

    public float FORCE_MULTIPLIER = 0.6f;
    public float GOAL_FACTOR = 1.0f;
    public float AGENT_FACTOR = 1.0f;
    public float WALL_FACTOR = 1.0f;

    public float radius;
    public float mass;
    public float perceptionRadius;

    private List<Vector3> path;
    private NavMeshAgent nma;
    private Rigidbody rb;
    private ThirdPersonCharacter character;

    private HashSet<GameObject> perceivedNeighbors = new HashSet<GameObject>();
    private HashSet<GameObject> adjacentWalls = new HashSet<GameObject>();

    private KalmanFilterVector3 kFilter = new KalmanFilterVector3();

    void Start()
    {
        path = new List<Vector3>();
        nma = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        character = GetComponent<ThirdPersonCharacter>();
        rb.mass = mass;
        transform.GetChild(0).GetComponent<SphereCollider>().radius = perceptionRadius / 2;
    }

    private void Update() {
        if (path.Count > 1 && Vector3.Distance(transform.position, path[0]) < NavPointRadius) {
            path.RemoveAt(0);
        } else if (path.Count == 1 && Vector3.Distance(transform.position, path[0]) < NavPointRadius) {
            path.RemoveAt(0);
            if (path.Count == 0) {
                agentManager.UpdateAgentDestination(gameObject);
            }
        }
    }

    #region Public Functions

    public void ComputePath(Vector3 destination)
    {
        nma.enabled = true;
        var nmPath = new NavMeshPath();
        if (nma.isOnNavMesh)
            nma.CalculatePath(destination, nmPath);
        path = nmPath.corners.Skip(1).ToList();
        //nma.SetDestination(destination);
        nma.enabled = false;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    public void ApplyForce()
    {
        var force = ComputeForce();
        force.y = 0;
        force = kFilter.Update(force.normalized);
        force.y = 0;
        character.Move(force * FORCE_MULTIPLIER, false, false);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (agentManager != null && agentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Add(other.gameObject);
        }
        else
        {
            adjacentWalls.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (perceivedNeighbors.Contains(other.gameObject))
        {
            perceivedNeighbors.Remove(other.gameObject);
        }
        if (adjacentWalls.Contains(other.gameObject))
        {
            adjacentWalls.Remove(other.gameObject);
        }
    }

    #endregion

    #region Private Functions

    private Vector3 ComputeForce()
    {
        var force = CalculateGoalForce() * GOAL_FACTOR
                  + CalculateAgentForce() * AGENT_FACTOR
                  + CalculateWallForce() * WALL_FACTOR;

        if (force != Vector3.zero)
        {
            return force.normalized * Mathf.Min(force.magnitude, Parameters.MAX_ACCEL);
        } else
        {
            return Vector3.zero;
        }
    }
    
    private Vector3 CalculateGoalForce() {
        if (path.Count == 0) {
            return Vector3.zero;
        }

        var temp = path[0] - transform.position;
        var desiredVel = temp.normalized * Parameters.DESIRED_SPEED;
        var actualVelocity = rb.velocity;
        return mass * (desiredVel - actualVelocity) / Parameters.T;
    }

    private Vector3 CalculateAgentForce()
    {
        var agentForce = Vector3.zero;

        foreach (var n in perceivedNeighbors)
        {
            if (!agentManager.IsAgent(n))
            {
                continue;
            }

            var neighbor = agentManager.agentsObjs[n];
            var dir = (transform.position - neighbor.transform.position).normalized;
            var overlap = (radius + neighbor.radius) - Vector3.Distance(transform.position, n.transform.position);

            agentForce += Parameters.A * Mathf.Exp(overlap / Parameters.B) * dir;
            agentForce += Parameters.K * (overlap > 0f ? 1 : 0) * dir;

            var tangent = Vector3.Cross(Vector3.up, dir);
            agentForce += Parameters.KAPPA * (overlap > 0f ? overlap : 0) * Vector3.Dot(rb.velocity - neighbor.GetVelocity(), tangent) * tangent;
        }

        return agentForce;
    }

    private Vector3 CalculateWallForce()
    {
        var wallForce = Vector3.zero;

        foreach (var wall in adjacentWalls)
        {
            var wallCentroid = wall.transform.position;
            var pos = transform.position;

            #region Compute Normal

            var normal = pos - wallCentroid;
            normal.y = 0;

            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.z))
            {
                normal.z = 0;
            }
            else
            {
                normal.x = 0;
            }
            normal.Normalize();

            #endregion

            var dir = (pos - wallCentroid);
            dir.y = 0;
            var agentToWallProj = Vector3.Project(dir, normal);
            var overlap = (radius + 0.5f) - agentToWallProj.magnitude;

            wallForce += Parameters.WALL_A * Mathf.Exp(overlap / Parameters.WALL_B) * normal;
            wallForce += Parameters.WALL_K * (overlap > 0f ? 1 : 0) * dir;

            var tangent = Vector3.Cross(Vector3.up, normal);
            wallForce += Parameters.WALL_KAPPA * (overlap > 0f ? overlap : 0) * Vector3.Dot(rb.velocity, tangent) * tangent;
        }

        return wallForce;
    }
    
    #endregion
}
