using UnityEngine;

/// <summary>
/// This class draws a Mandelbrot set as texture.
/// This script simply needs to be applied to a
/// GameObject that has a mesh and a material.
/// The mesh must also have a uv-mapping.
/// </summary>
public class MandelbrotTex : MonoBehaviour
{
    private static Color[] colors = {
        Color.black,
        Color.gray,
        Color.white,
        new Color(    1f,     0f,     0f, 1f), // Rot
        new Color(    1f,   0.4f,     0f, 1f), // Orange
        new Color(    1f,     1f,     0f, 1f), // Gelb
        new Color(    0f,     1f,     0f, 1f), // Grün
        new Color( 0.06f,  0.66f,     1f, 1f), // Hellblau
        new Color(    0f,     0f,     1f, 1f), // Blau
        new Color(    1f,     0f,     1f, 1f)  // Violett        
    };

    private static Color[] generatePixels(int w, int h, Rect loc)
    {
        float xmin = loc.x;
        float ymin = loc.y;
        float xmax = loc.x + loc.width;
        float ymax = loc.y + loc.height;
        Color[] pix = new Color[w * h];
        int pIx = 0;
        float[] p = new float[w];
        float q = ymin;
        float dp = (xmax - xmin) / w;
        float dq = (ymax - ymin) / h;

        p[0] = xmin;

        for (int i = 1; i < w; i++)
            p[i] = p[i - 1] + dp;

        for (int r = 0; r < h; r++)
        {
            for (int c = 0; c < w; c++)
            {
                int n = 0;
                float x = 0.0f;
                float y = 0.0f;
                float xsqr = 0.0f;
                float ysqr = 0.0f;

                do
                {
                    xsqr = x * x;
                    ysqr = y * y;
                    y = 2 * x * y + q;
                    x = xsqr - ysqr + p[c];
                    n++;
                }
                while (n < 512 && xsqr + ysqr < 4);

                pix[pIx++] = colors[(n % colors.Length)];
            }

            q += dq;
        }

        return pix;
    }

    public Rect loc = new Rect(-2.0f, -1.2f, 3.2f, 2.4f);
    public int width = 256;
    public int height = 256;

    private Material mat;
    private Texture2D tex;
    private Color[] pix;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;
        tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
        tex.filterMode = FilterMode.Point;
        tex.anisoLevel = (int)AnisotropicFiltering.Disable;
        pix = new Color[tex.width * tex.height];
        mat.mainTexture = tex;
        mat.SetTexture("_EmissionMap", tex);
        Paint();
    }

    private void Paint()
    {
        pix = generatePixels(tex.width, tex.height, loc);
        ApplyTex();
    }

    private void ApplyTex()
    {
        tex.SetPixels(pix);
        tex.Apply();
    }
}