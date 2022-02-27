using UnityEngine;
using UnityEngine.AI;

namespace SEAN.Util
{
    public class Navmesh
    {

        /// <summary>
        ///  Sample a point on the navmesh.
        ///  based on: https://answers.unity.com/questions/857827/pick-random-point-on-navmesh.html
        /// </summary>
        /// <returns>Vector3 uniform(ish) random point on navmesh</returns>
        public static Vector3 RandomVector(int depth = 0, int maxDepth = 42, float nearestEdgeThreshold = 0.25f)
        {
            if (depth > maxDepth)
            {
                throw new System.Exception("reached max depth of " + maxDepth + " without finding a solution");
            }
            NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
            int maxIndices = navMeshData.indices.Length - 3;
            // Pick the first indice of a random triangle in the nav mesh
            int firstVertexSelected = Random.Range(0, maxIndices);
            int secondVertexSelected = Random.Range(0, maxIndices);
            // Spawn on Verticies
            Vector3 point = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
            Vector3 firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
            Vector3 secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];
            // Eliminate points that share a similar X or Z position to stop spawining in square grid line formations
            if ((int)firstVertexPosition.x == (int)secondVertexPosition.x || (int)firstVertexPosition.z == (int)secondVertexPosition.z)
            {
                point = RandomVector(depth, maxDepth, nearestEdgeThreshold);
            }
            else
            {
                // Select a random point on it
                point = Vector3.Lerp(
                    firstVertexPosition,
                    secondVertexPosition, //[t + 1]],
                    Random.Range(0.05f, 0.95f) // Not using Random.value as clumps form around Verticies 
                );
            }
            if (point.x == Mathf.Infinity || point.y == Mathf.Infinity || point.z == Mathf.Infinity)
            {
                return RandomVector(depth, maxDepth, nearestEdgeThreshold);
            }
            // Make sure we are at least nearestEdgeThreshold away from the closest edge
            NavMeshHit hit;
            if (NavMesh.FindClosestEdge(point, out hit, NavMesh.AllAreas))
            {
                if (hit.distance < nearestEdgeThreshold - Mathf.Epsilon)
                {
                    point -= hit.position.normalized * (nearestEdgeThreshold + Mathf.Epsilon - hit.distance);
                }
                // Maybe the navmesh in this area is too small, if so, resample
                if (NavMesh.FindClosestEdge(point, out hit, NavMesh.AllAreas) && hit.distance < nearestEdgeThreshold - Mathf.Epsilon)
                {
                    return RandomVector(depth, maxDepth, nearestEdgeThreshold);
                }
            }
            return point;
        }

        public static NavMeshHit RandomHit()
        {
            NavMeshHit hit;
            // may not be necessary:
            NavMesh.SamplePosition(RandomVector(), out hit, 10, NavMesh.AllAreas);
            return hit;
        }

        public static NavMeshHit RandomHit(Vector3 nearPosition, float distance, float maxDistance = 1)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * distance;
            nearPosition.x += randomDir.x;
            nearPosition.z += randomDir.y;
            NavMeshHit hit;
            // may not be necessary:
            NavMesh.SamplePosition(nearPosition, out hit, maxDistance, NavMesh.AllAreas);
            return hit;
        }

        /// <summary>
        ///   Get a random rotation along the ground plane (y-axis rotation)
        /// </summary>
        /// <returns>Quaternion randomly rotated aroudn the y-axis</returns>
        public static Quaternion RandomRotation()
        {
            return Quaternion.Euler(0, Random.Range(0, 360), 0);
        }


        /// <summary>
        /// Get a random pose on the navmesh
        /// </summary>
        /// <returns></returns>
        public static Pose RandomPose()
        {
            Pose pose = new Pose();
            pose.position = RandomVector();
            pose.rotation = RandomRotation();
            return pose;
        }

    }
}
