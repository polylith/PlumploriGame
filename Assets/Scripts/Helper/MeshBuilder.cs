using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Creation
{
    /// <summary>
    /// This abstract class unifies all meshes of a complex
    /// GameObject and merges them into a single mesh.
    /// In the process, the materials are preserved
    /// and mapped to the new mesh.
    /// </summary>
    public abstract class MeshBuilder : MonoBehaviour
    {
        private MeshFilter combinedFilter;
        private MeshRenderer combinedRenderer;
        private MaterialMap matMap;
        private ArrayList combineInstanceArrays;

        protected Bounds GetLocalBounds()
        {
            return combinedFilter.mesh.bounds;
        }

        protected Bounds GetBounds()
        {
            return combinedRenderer.bounds;
        }

        private void GetCheck(Transform trans)
        {
            combinedFilter = trans.gameObject.GetComponent<MeshFilter>();
            combinedRenderer = trans.gameObject.GetComponent<MeshRenderer>();

            if (null == combinedFilter)
            {
                trans.gameObject.AddComponent<MeshFilter>();
                combinedFilter = trans.gameObject.GetComponent<MeshFilter>();
            }

            if (null == combinedRenderer)
            {
                trans.gameObject.AddComponent<MeshRenderer>();
                combinedRenderer = trans.gameObject.GetComponent<MeshRenderer>();
            }

            transform.gameObject.isStatic = true;
        }
        
        public virtual void Init(Transform trans)
        {
            GetCheck(trans);
            matMap = new MaterialMap();
            combineInstanceArrays = new ArrayList();
        }

        public virtual void AddMeshes(Transform trans, Regex regex)
        {
            foreach (Transform child in trans)
            {
                if (child.gameObject.activeSelf)
                {
                    Match match = regex.Match(child.name);

                    if (child.childCount > 0)
                        AddMeshes(child, regex);

                    if (match.Success || child.gameObject.isStatic)
                        MergeMeshes(child);
                }
            }
        }

        public void AddMeshes(Transform trans, bool destroy = false)
        {
            foreach (Transform child in trans)
            {
                if (child.gameObject.activeSelf)
                {
                    if (child.childCount > 0)
                        AddMeshes(child);

                    MergeMeshes(child, destroy);
                }
            }

            if (trans.gameObject.activeSelf)
                MergeMeshes(trans, destroy);
        }

        public void MergeMeshes(Transform trans, bool destroy = false)
        {
            MeshFilter meshFilter = trans.gameObject.GetComponent<MeshFilter>();

            if (null == meshFilter)
                return;

            Renderer meshRenderer = meshFilter.GetComponent<Renderer>();

            if (null == meshRenderer || null == meshFilter.sharedMesh 
                || meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
                return;

            Mesh mesh = meshFilter.sharedMesh;

            if (meshRenderer is SkinnedMeshRenderer)
            {
                mesh = new Mesh();
                ((SkinnedMeshRenderer)meshRenderer).BakeMesh(mesh);
            }

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int materialArrayIndex = matMap.CheckAddMaterial(meshRenderer.materials[i]);
                combineInstanceArrays.Add(new ArrayList());
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = i;
                combineInstance.mesh = mesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }

            Destroy(meshRenderer);
            Destroy(meshFilter);

            if (destroy)
                Destroy(trans.gameObject);
            else
                RemoveIfEmpty(trans);
        }

        private static void RemoveIfEmpty(Transform trans)
        {
            Component[] comp = trans.GetComponents<MonoBehaviour>();
            Collider[] col = trans.GetComponents<Collider>();

            if (trans.childCount == 0 && (null == comp || comp.Length == 0) && (null == col || col.Length == 0))
                Destroy(trans.gameObject);
        }

        public void Build()
        {
            Mesh[] meshes = new Mesh[matMap.Count];
            CombineInstance[] combineInstances = new CombineInstance[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                CombineInstance[] combineInstanceArray = (combineInstanceArrays[i] as ArrayList)
                    .ToArray(typeof(CombineInstance)) as CombineInstance[];
                meshes[i] = new Mesh();
                meshes[i].MarkDynamic();
                meshes[i].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshes[i].CombineMeshes(combineInstanceArray, true, true);

                combineInstances[i] = new CombineInstance();
                combineInstances[i].mesh = meshes[i];
                combineInstances[i].subMeshIndex = 0;
            }

            combinedFilter.sharedMesh = new Mesh();
            combinedFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedFilter.sharedMesh.CombineMeshes(combineInstances, false, false);

            foreach (Mesh oldMesh in meshes)
            {
                oldMesh.Clear();
                DestroyImmediate(oldMesh);
            }

            Material[] mats = matMap.ToArray();

            for (int i = 0; i < mats.Length; i++)
                mats[i] = Material.Instantiate(mats[i]) as Material;

            combinedRenderer.materials = mats;
        }
    }
}