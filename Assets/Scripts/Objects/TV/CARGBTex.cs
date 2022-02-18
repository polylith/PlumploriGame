using System.Collections;
using UnityEngine;

/// <summary>
/// This class draws patterns of random one-dimensional
/// cellular automata as a texture. This script simply
/// needs to be applied to a GameObject that has a mesh
/// and a material. The mesh must also have a uv-mapping.
/// </summary>
public class CARGBTex : MonoBehaviour
{
    private static int[] powers = new int[] { 1, 2, 4 };
    public int width = 256;
    public int height = 256;

    [Range(0.1f,1f)]
    public float evolveTime = 0.5f;

    public int[] code = new int[3] { 55, 33, 127 };

    private Material mat;
    private Texture2D tex;
    private Color[] pix;
    public int[] counter = new int[2];

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;
        tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
        tex.filterMode = FilterMode.Bilinear;// FilterMode.Point;
        tex.anisoLevel = (int)AnisotropicFiltering.Disable;
        pix = new Color[tex.width * tex.height];
        mat.mainTexture = tex;
        mat.EnableKeyword("_EMISSION");
        mat.SetTexture("_EmissionMap", tex);
        SetPixels();
        Evolve();
    }

    private void SetPixels()
    {
        code[0] = Random.Range(0, 256);
        code[1] = Random.Range(0, 256);
        code[2] = Random.Range(0, 256);

        counter[0] = 0;
        counter[1] = (code[0] + code[1] + code[2]) / 3;
        int x = width - 1;

        for (int y = 0; y < height; y++)
            pix[x * width + y] = new Color(Random.Range(0, 100) % 2, Random.Range(0, 100) % 2, Random.Range(0, 100) % 2);
    }

    private void Evolve()
    {
        int changes = 0;
        Color[] tmp = new Color[height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tmp[y] = Evolve(x - 1, y);
                changes += tmp[y].Equals(pix[x * width + y]) ? 0 : 1;
            }

            for (int y = 0; y < height; y++)
                pix[x * width + y] = tmp[y];
        }

        counter[0]++;

        if (counter[0] == counter[1])
            SetPixels();

        ApplyTex();

        if (changes == 0)
            StartCoroutine(IEWait());
        else
            StartCoroutine(IENext());
    }

    private Color Evolve(int x, int y)
    {
        x += width;
        x %= width;
        int[] count = new int[3] { 0, 0, 0 };

        for (int i = 0; i < 3; i++)
        {
            int y2 = (y + i - 1 + height) % height;

            if (y2 >= 0 && y2 < height)
            {
                Color state = pix[x * width + y2];

                if (state.r > 0f)
                    count[0] += powers[i];

                if (state.g > 0f)
                    count[1] += powers[i];

                if (state.b > 0f)
                    count[2] += powers[i];
            }
        }

        return new Color(
            count[0] > 0 && code[0] % count[0] > 0 ? 1f : 0f,
            count[1] > 0 && code[1] % count[1] > 0 ? 1f : 0f,
            count[2] > 0 && code[2] % count[2] > 0 ? 1f : 0f
            );
    }

    private IEnumerator IEWait()
    {
        yield return new WaitForSecondsRealtime(2.5f + Random.Range(0f, 2.5f));

        SetPixels();
        Evolve();
    }

    private IEnumerator IENext()
    {
        yield return new WaitForSecondsRealtime(evolveTime);

        Evolve();
    }

    private void ApplyTex()
    {
        tex.SetPixels(pix);
        tex.Apply();
    }
}
