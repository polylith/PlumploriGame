using System.Collections.Generic;
using UnityEngine;

public class ProfiBrainColorCode : MonoBehaviour
{
    public bool OrderedEval { get; set; }

    public ProfiBrainColorDot[] profiBrainColorDots;
    private int[] code;

    public int[] GenerateCode() {
        ProfiBrainColorDot.ShuffleColors();
        int n = profiBrainColorDots.Length;
        code = ArrayHelper.GetArray(0, n);
        ArrayHelper.ShuffleArray(code);

        foreach (ProfiBrainColorDot profiBrainColorDot in profiBrainColorDots)
        {
            profiBrainColorDot.ColorIndex = -1;
            profiBrainColorDot.gameObject.SetActive(false);
        }

        List<int> usedColors = new List<int>();

        foreach (int colorIndex in code)
        {
            if (!usedColors.Contains(colorIndex))
            {
                usedColors.Add(colorIndex);
            }
        }

        ArrayHelper.Shuffle(usedColors);
        return usedColors.ToArray();
    }

    public void ShowCode()
    {
        for (int i = 0; i < code.Length; i++)
        {
            profiBrainColorDots[i].gameObject.SetActive(true);
            profiBrainColorDots[i].ColorIndex = code[i];
        }
    }

    public int[] Evaluate(int[] code)
    {
        int[] result;
        int n = Mathf.Min(code.Length, this.code.Length);

        if (OrderedEval)
        {
            result = new int[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = this.code[i] == code[i] ? 1 : 0;
            }

            return result;
        }

        result = new int[] { 0, 0 };
        
        List<int> list = new List<int>(ArrayHelper.GetArray(0, n));

        for (int i = 0; i < n; i++)
        {
            if (this.code[i] == code[i])
            {
                result[0] += 1;
                list.Remove(i);
            }
        }

        foreach (int i in list)
        {
            int colorIndex = this.code[i];

            foreach (int j in list)
            {
                if (i != j && colorIndex == code[j])
                {
                    result[1] += 1;
                    break;
                }
            }
        }

        return result;
    }
}
