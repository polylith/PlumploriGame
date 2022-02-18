using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// The class generates a dynamic texture that
/// can be used for materials and UI images.
/// This class is used in the class TV.
/// </para>
/// <para>
/// The texture is created by using cellular
/// automata that are randomly recombined using
/// an index as code.Some codes create only
/// random noise, others shift the texture in
/// several directions.
/// </para>
/// <para>
/// This class can be used statically.
/// There is a singleton for this purpose,
/// but also object instances may be used.
/// </para>
/// </summary>
public class CATex
{
    private static CATex ins;

    /// <summary>
    /// This function can be used in the static version
    /// to fetch the texture of the next generation.
    /// This texture can be used as a single image or
    /// can be animated with a timer. The parameter
    /// <paramref name="reinitTexture">reinitTexture</paramref>
    /// resets the state of the automaton and brings up
    /// a new pattern.
    /// </summary>
    /// <param name="reinitTexture">reinit the texture</param>
    /// <returns>texture to use on material or image</returns>
    public static Sprite GetSprites(bool reinitTexture)
    {
        if (null == ins)
            ins = new CATex(null, 100, 100);

        if (reinitTexture)
            ins.Init();

        ins.Evolve(Random.Range(10,20));
        return Sprite.Create(ins.tex, new Rect(0f, 0f, ins.tex.width, ins.tex.height), Vector2.one * 0.5f);
    }

    private static int[] powers = new int[] { 1, 2, 4 };
    private static Color color0 = new Color(0f, 0f, 0f, 0f);
    public static Color[] allColors = new Color[] {
        new Color(    1f,     0f,     0f, 1f), // 0 Rot
        new Color(    1f,   0.4f,     0f, 1f), // 1 Orange
        new Color(    1f,     1f,     0f, 1f), // 2 Gelb
        new Color(    0f,     1f,     0f, 1f), // 3 Grün
        new Color( 0.06f,  0.66f,     1f, 1f), // 4 Hellblau
        new Color(    0f,     0f,     1f, 1f), // 5 Blau
        new Color(    1f,     0f,     1f, 1f),  // 6 Violett
        Color.white,  // 7 weiss
        Color.gray  // 8 grau
    };

    /// <summary>
    /// Get a random color defined in the static color array.
    /// </summary>
    /// <returns>color of the static array</returns>
    public static Color RandomColor()
    {
        return allColors[Random.Range(0, allColors.Length - 2)];
    }

    public float EvolveTime { get => evolveTime; }
    public Color[] ColorStates { get => pix; }

    private Material mat;
    private Texture2D tex;
    protected Color[] pix;

    private Color[] colors = new Color[] { Color.gray, Color.white };
    private int[][][] states;
    private int state;    
    private List<int> codes;
    private int codeCounter;
    private int code;
    private int code2;
    private int callCount;
    private int maxCount;
    private bool stop;
    private float evolveTime;
    private bool wrapMode;
    private List<int> codeList;

    public CATex(Material mat, int width, int height)
    {
        this.mat = mat;
        tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
        tex.filterMode = FilterMode.Point;
        tex.anisoLevel = (int)AnisotropicFiltering.Disable;
        pix = new Color[tex.width * tex.height];

        if (null != mat)
        {
            mat.mainTexture = tex;
            mat.SetTexture("_EmissionMap", tex);
        }

        Init();
    }

    /// <summary>
    /// Get the current generated texture for rendering.
    /// </summary>
    /// <returns>evolved texture</returns>
    public Texture GetRenderTexture()
    {
        return tex;
    }

    /// <summary>
    /// Set automaton to random noise mode
    /// </summary>
    public void Noise()
    {
        stop = true;
        maxCount = 0;
        colors = new Color[] {
            Color.black,
            new Color(1f, 1f, 1f, 0.65f)
        };
        codeCounter = 0;
        stop = false;
        code = 1;
        Evolve();
    }

    /// <summary>
    /// Stop the evolution.
    /// </summary>
    public void Stop()
    {
        stop = true;
    }

    /// <summary>
    /// Restarts the evolution.
    /// </summary>
    public void Restart()
    {
        stop = false;
        InitCodes();
        SetCode(0);
        Evolve();
    }

    protected virtual void Init()
    {
        state = 0;
        states = new int[2][][];
        states[0] = new int[tex.width][];
        states[1] = new int[states[0].Length][];

        for (int x = 0; x < states[0].Length; x++)
        {
            states[0][x] = new int[tex.height];
            states[1][x] = new int[states[0][x].Length];

            for (int y = 0; y < states[0][x].Length; y++)
            {
                pix[x * tex.width + y] = color0;
            }
        }

        InitCodes();
        SetCode(0);
        Evolve();
    }
       
    private void InitCodes()
    {
        codes = new List<int>();
        codeCounter = 0;
        int n = Random.Range(2, 5) * Random.Range(2, 5);

        while (n > 0)
        {
            codes.Add(Random.Range(-48, 256));
            n--;
        }
    }

    private void InitColors()
    {
        int[] arr = ArrayHelper.GetArray(0, allColors.Length);
        ArrayHelper.ShuffleArray(arr);
        int n = (code > 1 ? Mathf.Max(1, code % allColors.Length) : Random.Range(1, allColors.Length));
        colors = new Color[n];

        for (int i = 0; i < n; i++)
        {
            colors[i] = allColors[arr[i]];
        }
    }

    private void SetCode(int changeCount)
    {
        code = codes[codeCounter];
        codeCounter++;
        codeCounter %= codes.Count;
        code2 = Random.Range(0, 100);
        codeList = new List<int>();

        if (code > 0)
        {
            int k = 0;
            int c = code;

            while (c > 0)
            {
                if (c % 2 == 1)
                    codeList.Add(k);

                c >>= 1;
                k++;
            }
        }

        if (changeCount == 0)
        {
            wrapMode = Random.value > 0.5f;
            evolveTime = Random.value * 0.25f + 0.05f;
            InitColors();

            for (int x = 0; x < states[0].Length; x++)
            {
                for (int y = 0; y < states[0][x].Length; y++)
                {
                    int zz = Random.Range(0, 100) % 2;
                    states[state][x][y] = zz;
                }
            }
        }

        maxCount = Random.Range(1, 11) * 5;
        callCount = 0;
    }

    protected virtual int GetState(int x, int y)
    {
        if (wrapMode)
        {
            x += states[state].Length;
            x %= states[state].Length;
            y += states[state][x].Length;
            y %= states[state][x].Length;
        }
        else if (x < 0 || x >= states[0].Length || y < 0 || y >= states[0][0].Length)
            return 0;

        return states[state][x][y];
    }

    protected virtual void SetState(int state, int x, int y, int i)
    {
        if (wrapMode)
        {
            x += states[state].Length;
            x %= states[state].Length;
            y += states[state][x].Length;
            y %= states[state][x].Length;
        }
        else if (x < 0 || x >= states[0].Length || y < 0 || y >= states[0][0].Length)
            return;

        states[state][x][y] = i;
    }

    private void Evolve(int count)
    {
        while (count > 0)
        {
            Evolve();
            count--;
        }
    }

    protected virtual int Evolve(int x, int y)
    {
        int count = 0;

        if (code < -16)
        {
            count += GetState(x, y - 1);
            count += GetState(x - 1, y);
            count += GetState(x + 1, y);
            count += GetState(x, y + 1);
            count += GetState(x - 1, y - 1);
            count += GetState(x + 1, y - 1);
            count += GetState(x - 1, y + 1);
            count += GetState(x + 1, y + 1);

            if (states[state][x][y] == 0)
            {
                if (count == 3)
                    return 1;
            }
            else if (count < 2 || count > 3)
            {
                return 0;
            }

            return states[state][x][y];
        }
        else if (code <= 0)
        {
            count += GetState(x, y - 1);
            count += GetState(x - 1, y);
            count += GetState(x + 1, y);
            count += GetState(x, y + 1);
            int nextState = (state + 1) % 2;

            if (count == 4)
            {
                int i = 1 - states[state][x][y];
                SetState(nextState, x, y - 1, i);
                SetState(nextState, x - 1, y, i);
                SetState(nextState, x + 1, y, i);
                SetState(nextState, x, y + 1, i);
                return i;
            }

            return states[state][x][y];
        }

        int x0 = x;
        int y0 = y;
        int c2 = code2 % 4;

        if (c2 < 2)
        {
            for (int j = 0; j < powers.Length; j++)
            {
                switch (code % 4)
                {
                    case 0:
                        if (GetState(x0 + j - 1, y0) > 0)
                            count += powers[j];
                        break;
                    case 1:
                        if (GetState(x0, y0 + j - 1) > 0)
                            count += powers[j];
                        break;
                }
            }
        }
        else if (c2 == 2)
        {
            count += GetState(x - 1, y - 1) > 0 ? powers[0] : 0;
            count += GetState(x, y) > 0 ? powers[1] : 0;
            count += GetState(x + 1, y + 1) > 0 ? powers[2] : 0;
        }
        else
        {
            count += GetState(x - 1, y + 1) > 0 ? powers[0] : 0;
            count += GetState(x, y) > 0 ? powers[1] : 0;
            count += GetState(x + 1, y - 1) > 0 ? powers[2] : 0;
        }

        return codeList.Contains(count) ? 1 : 0;
    }

    private Color GetRandRGB()
    {
        if (code < 0 || code % 4 == 1)
            return colors[Mathf.Abs(code) % colors.Length];

        int zz = Random.Range(0, 100);

        if (zz > maxCount)
            return colors[callCount % colors.Length];

        return colors[zz % colors.Length];
    }

    /// <summary>
    /// Evolve the next generation.
    /// </summary>
    public virtual void Evolve()
    {
        int changeCount = 0;
        int newState = (state + 1) % 2;
        callCount++;

        for (int x = 0; x < states[newState].Length; x++)
        {
            for (int y = 0; y < states[newState][x].Length; y++)
            {
                if (maxCount > 0)
                {
                    int nextState = Evolve(x, y);
                    changeCount += states[state][x][y] == nextState ? 0 : 1;
                    states[newState][x][y] = nextState;
                    pix[x * tex.width + y] = nextState == 0 ? color0 : GetRandRGB();
                }
                else
                {
                    pix[x * tex.width + y] = colors[Random.value > 0.5f ? 0 : 1];
                }
            }

            if (code > 0)
                states[state][x] = states[newState][x];
        }

        state = newState;
        ApplyTex();

        if (maxCount > 0 && (changeCount == 0 || callCount % maxCount == 0))
            SetCode(changeCount);

        if (!stop && null != mat)
            GameEvent.GetInstance()?.Execute(Evolve, evolveTime);        
    }

    protected void ApplyTex()
    {
        tex.SetPixels(pix);
        tex.Apply();
    }
}