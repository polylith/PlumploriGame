using UnityEngine;

namespace Creation
{
    /// <summary>
    /// <para>
    /// This class is a concrete implementation of the abstract
    /// MeshBuilder. The script can be placed on a GameObject,
    /// which is used at runtime to combine complex GameObjects
    /// with many meshes into a single mesh.
    /// </para>
    /// <para>
    /// IMPORTANT
    /// For blender imported model/meshes Read/Write flag must be
    /// set to true on Enabled in Model Tab.
    /// </para>
    /// </summary>
    public class ObjectMeshBuilder : MeshBuilder
    {
        /// <summary>
        /// Clone a complex GameObject.
        /// </summary>
        /// <param name="obj">GameObject to place the single mesh</param>
        /// <param name="copy">GameObjject to clone</param>
        /// <param name="shader">optional shade to use for materials instead of the original</param>
        /// <returns>the bounds of the resulting single mesh</returns>
        public Bounds CloneObject(GameObject obj, GameObject copy, Shader shader = null)
        {
            GameObject clone = Instantiate(obj) as GameObject;
            clone.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            clone.transform.localPosition = Vector3.zero;
            Init(copy.transform);
            AddMeshes(clone.transform, true);
            Build();
            Destroy(clone.gameObject);

            if (null != shader)
                RemapShader(copy, shader);

            return GetLocalBounds();
        }

        private void RemapShader(GameObject obj, Shader shader)
        {
            Renderer renderer = obj.GetComponent<Renderer>();

            if (null == renderer)
                return;

            foreach (Material mat in renderer.materials)
                mat.shader = shader;
        }
    }
}
