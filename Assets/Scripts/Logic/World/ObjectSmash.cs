using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script should be used to smash object.
/// It will probably not be used anymore or reshaped.
/// </summary>
public class ObjectSmash : MonoBehaviour
{
    private Collider col;
    public PhysicMaterial bouncyMat;
    private List<GameObject> smashObjects;
    
    private void Awake()
    {
        smashObjects = new List<GameObject>();
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        CleanUp();
    }

    private void OnTriggerEnter(Collider other)
    {
        Smash(other.transform.gameObject);
    }

    public void Smash(GameObject hitObj)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material[] materials = renderer.materials;

        CleanUp();
        smashObjects = Split(hitObj, mesh, materials);

        if (smashObjects.Count == 0)
            return;

        foreach (GameObject obj in smashObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            //rb.AddExplosionForce(Random.Range(5f, 10f), hitObj.transform.position, 2f);
            rb.useGravity = true;
            //rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        transform.gameObject.SetActive(false);
    }

    private List<GameObject> Split(GameObject hitObj, Mesh mesh, Material[] materials)
    {
        List<GameObject> objects = new List<GameObject>();
        List<MeshData> meshData = Split(hitObj, hitObj.transform.position - transform.position, mesh, transform.localScale);

        if (null == meshData || meshData.Count == 0)
            return objects;

        for (int i = 0; i < meshData.Count; i++)
        {
            try
            {
                GameObject obj = new GameObject("Smashed " + transform.name + " " + i);
                obj.transform.SetParent(transform.parent.transform, true);
                obj.AddComponent<MeshCollider>();
                obj.AddComponent<Rigidbody>();
                obj.AddComponent<MeshFilter>();
                obj.AddComponent<MeshRenderer>();

                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                MeshCollider meshCol = obj.GetComponent<MeshCollider>();
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

                meshData[i].InitMesh(meshFilter);                
                meshCol.convex = true;
                meshCol.material = Instantiate(bouncyMat) as PhysicMaterial;
                meshCol.material.bounciness = Random.value * 0.4f + 0.5f;
                meshCol.sharedMesh = meshFilter.mesh;                
                renderer.materials = materials;
                objects.Add(obj);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        return objects;
    }

    private static List<MeshData> Split(GameObject hitObj, Vector3 point, Mesh mesh, Vector3 scale)
    {
        List<MeshData> list = new List<MeshData>();
        Bounds bounds = mesh.bounds;
        int n = 24;
        int rings = 10;
        float alpha = Mathf.PI * 2f / (float)n;
        float rx = bounds.extents.x * scale.x / (float)rings;
        float ry = bounds.extents.y * scale.y / (float)rings;
        float rz = bounds.extents.z * scale.z / (float)rings;
        
        List<Vector3> vertices = new List<Vector3>();

        for (int j = 0; j < rings; j++)
        {
            float angle = 0f;

            for (int i = 0; i < n; i++)
            {
                float x = rx * Random.Range(0.9f, 1.1f) * Mathf.Cos(angle) * Mathf.Sin(j) + point.x;
                float y = ry * Random.Range(0.9f, 1.1f) * Mathf.Sin(angle) * Mathf.Sin(j) + point.y;
                float z = rz * Random.Range(0.9f, 1.1f) * Mathf.Cos(j) + point.z;

                x = Mathf.Clamp(x, bounds.min.x, bounds.max.x);
                y = Mathf.Clamp(y, bounds.min.y, bounds.max.y);
                z = Mathf.Clamp(z, bounds.min.z, bounds.max.z);
                
                vertices.Add(new Vector3(x, y, z));
                angle += alpha;
            }

            rx += rx;
            ry += ry;
            rz += rz;
        }

        MeshData meshData1 = null;
        MeshData meshData2 = null;
        int v = 0;

        while (v < vertices.Count)
        {
            if (v % 3 == 0)
            {
                if (null != meshData1)
                    list.Add(meshData1);

                if (null != meshData2)
                    list.Add(meshData2);

                meshData1 = new MeshData();
                meshData2 = new MeshData();
            }

            if (v + 1 < vertices.Count && v + n < vertices.Count)
            {
                meshData1.vertices.Add(vertices[v]);
                meshData1.vertices.Add(vertices[v + 1]);
                meshData1.vertices.Add(vertices[v + n]);

                if (v + n + 1 < vertices.Count)
                {
                    meshData2.vertices.Add(vertices[v + n]);
                    meshData2.vertices.Add(vertices[v + 1]);
                    meshData2.vertices.Add(vertices[v + n + 1]);
                }
            }

            v++;
        }

        if (null != meshData1)
            list.Add(meshData1);

        if (null != meshData2)
            list.Add(meshData2);

        return list;
    }

    private void CleanUp()
    {
        if (null != smashObjects && smashObjects.Count > 0)
        {
            while (smashObjects.Count > 0)
            {
                GameObject obj = smashObjects[0];
                smashObjects.RemoveAt(0);
                Destroy(obj);
            }
        }
    }

    private class MeshData
    {
        public List<Vector3> vertices;
        public List<int> faces;

        public MeshData()
        {
            vertices = new List<Vector3>();
        }

        public bool Check()
        {
            return true;// IsNotCoplanar(vertices);
        }

        public void InitMesh(MeshFilter meshFilter)
        {
            faces = new List<int>();

            for (int i = 0; i < vertices.Count; i++)
            {
                faces.Add(i);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(faces,0);
            meshFilter.mesh = mesh;            
            meshFilter.mesh.RecalculateNormals();
        }

        private static bool IsNotCoplanar(List<Vector3> list)
        {
            Matrix4x4 m = new Matrix4x4(
                new Vector4(list[0].x, list[0].y, list[0].z, 0f),
                new Vector4(list[1].x, list[1].y, list[1].z, 0f),
                new Vector4(list[2].x, list[2].y, list[2].z, 0f),
                new Vector4(0f, 0f, 0f, 1f)
                );

            float d = m.determinant;
            bool res = d != 0f;
            Debug.Log("Is coplanar " + res + " " + d);
            return res;
        }
    }
}
