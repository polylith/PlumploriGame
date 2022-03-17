using System.Collections.Generic;
using Movement;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshView : MonoBehaviour
{
    private static NavMeshView ins;

    public static NavMeshView GetInstance()
    {
        return ins;
    }

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private NavMeshTriangulation triangulation;
    private List<Vector3> points;
    private Vector3 basePoint;
    private Transform parent;
    private float minX, maxX, minZ, maxZ;

    public NavMeshPointsData ExploreNavMesh(Transform parent, Vector3 basePoint)
    {
        this.basePoint = basePoint;
        this.parent = parent;
        minX = float.MaxValue;
        maxX = float.MinValue;
        minZ = float.MaxValue;
        maxZ = float.MinValue;

        triangulation = NavMesh.CalculateTriangulation();
        points = new List<Vector3>();

        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            int triangleIndex = i / 3;
            int i1 = triangulation.indices[i];
            int i2 = triangulation.indices[i + 1];
            int i3 = triangulation.indices[i + 2];
            Vector3 p1 = triangulation.vertices[i1];
            Vector3 p2 = triangulation.vertices[i2];
            Vector3 p3 = triangulation.vertices[i3];
            int areaIndex = triangulation.areas[triangleIndex];

            if (areaIndex == 0)
            {
                UpdateMinMax(p1);
                UpdateMinMax(p2);
                UpdateMinMax(p3);
            }
        }

        float midX = (minX + maxX) * 0.5f;
        float dz = Mathf.Abs(minZ - maxZ) * 0.1f;
        float z = minZ;

        List<Vector3> list = new List<Vector3>
        {
            new Vector3(minX, basePoint.y, minZ),
            new Vector3(maxX, basePoint.y, minZ),
            new Vector3(minX, basePoint.y, maxZ),
            new Vector3(maxX, basePoint.y, maxZ)
        };

        Vector3 minP = list[0];
        Vector4 p0 = basePoint;

        AddPoint(basePoint);

        while (list.Count > 1)
        {
            float minD = float.MaxValue;

            foreach (Vector3 p1 in list)
            {
                float d = Vector3.Distance(p0, p1);

                if (d < minD)
                {
                    minD = d;
                    minP = p1;
                }
            }

            list.Remove(minP);
            p0 = minP;
            AddPoint(minP);
        }

        AddPoint(list[0]);
        AddPoint(basePoint);

        while (z <= maxZ)
        {
            AddPoint(new Vector3(minX, basePoint.y, z));
            AddPoint(new Vector3(midX, basePoint.y, z));
            AddPoint(new Vector3(maxX, basePoint.y, z));
            z += dz;

            AddPoint(new Vector3(maxX, basePoint.y, z));
            AddPoint(new Vector3(midX, basePoint.y, z));
            AddPoint(new Vector3(minX, basePoint.y, z));
            z += dz;
        }

        Rect rect = new Rect(
            new Vector2(minX, minZ),
            new Vector2(Mathf.Abs(maxX - minX), Mathf.Abs(maxZ - minZ))
        );
        return new NavMeshPointsData(points, rect);
    }

    private Vector3 AddPoint(Vector3 p)
    {
        MovePathInfo info = NavMeshMover.CalculatePath(basePoint, p);

        if (info.HasPoints)
        {
            p = info.points[info.points.Length - 1];
            p.y = Mathf.Max(basePoint.y, p.y);
        }
        else
        {
            p = NavMeshMover.GetWalkAblePoint(p);
            p.y = Mathf.Max(basePoint.y, p.y);
        }

        if (!points.Contains(p))
        {
            points.Add(p);
        }

        return p;
    }

    private void UpdateMinMax(Vector3 p1)
    {
        MovePathInfo info = NavMeshMover.CalculatePath(basePoint, p1);

        if (info.HasPoints)
        {
            Vector3 p2 = info.points[info.points.Length - 1];
            p2.y = Mathf.Max(basePoint.y, p2.y);
            minX = Mathf.Min(minX, p2.x);
            maxX = Mathf.Max(maxX, p2.x);
            minZ = Mathf.Min(minZ, p2.z);
            maxZ = Mathf.Max(maxZ, p2.z);
        }
    }
}