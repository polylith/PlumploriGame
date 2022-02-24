using UnityEngine;
using UnityEngine.AI;

namespace Movement
{
    /// <summary>
    /// Static class providing functions on static navmesh.
    /// </summary>
    public static class NavMeshMover
    {
        public static Vector3 GetRandomPointOnNavMesh()
        {
            NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

            // Pick the first indice of a random triangle in the nav mesh
            int t = Random.Range(0, navMeshData.indices.Length - 3);

            // Select a random point on it
            Vector3 point = Vector3.Lerp(navMeshData.vertices[navMeshData.indices[t]], navMeshData.vertices[navMeshData.indices[t + 1]], Random.value);
            Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value);

            return point;
        }

        public static Vector3 GetWalkAblePoint(Vector3 target)
        {
            NavMeshHit myNavHit;

            if (NavMesh.SamplePosition(target, out myNavHit, 3, NavMesh.AllAreas))
            {
                target = myNavHit.position;
            }

            return target;
        }

        private static void LogPath(NavMeshPath path, Vector3 start, Vector3 target)
        {
            string s = start + " -> " + target + "\n";

            foreach (Vector3 p in path.corners)
            {
                s += "\t -> " + p + "\n";
            }

            s += " => " + target + "\n ==========";
            Debug.Log(s);
        }

        public static MovePathInfo CalculatePath(Vector3 start, Vector3 target)
        {
            target = Calc.GetPointOnGround(target).point;
            NavMeshPath path = new NavMeshPath();
            MovePathInfo info = new MovePathInfo
            {
                target = target
            };

            NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path);

            if (path.corners.Length == 0)
            {
                start = GetWalkAblePoint(start);
                target = GetWalkAblePoint(target);
                NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path);
            }

            // LogPath(path, start, info.target);

            Vector3[] points = new Vector3[path.corners.Length];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Calc.GetPointOnGround(path.corners[i]).point;
            }

            info.points = points;
            return info;
        }
    }
}