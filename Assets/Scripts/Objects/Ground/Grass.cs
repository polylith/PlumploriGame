using UnityEngine;

public class Grass : MonoBehaviour
{
    public Vector3 size;
    public int min = 5;
    public int max = 10;
    public GameObject grassPrefab;
    public Texture[] textures;

    private bool inited = false;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (inited)
            return;

        int rnd = Random.Range(0, 100) % textures.Length;
        Texture tex = textures[rnd];
        int n = Random.Range(min, max);

        for (int i = 0; i < n; i++)
        {
            GameObject obj = Instantiate(grassPrefab, transform);
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();


            foreach (Renderer renderer in renderers)
            {
                Material mat = renderer.materials[0];
                mat.SetTexture("_MainTex", tex);
            }

            Vector3 position = Random.insideUnitSphere;
            float s = (position.x + position.y + 1f) * 0.3f;
            position.x *= size.x;
            position.z *= size.z;
            position.y = 0f;
            obj.transform.localRotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
            obj.transform.localPosition = position;
            obj.transform.localScale = new Vector3(s, s, 1f);
        }

        inited = true;
        Destroy(GetComponent<Grass>());
    }
}
