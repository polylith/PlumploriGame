using System.Collections.Generic;
using UnityEngine;

public class ProfiBrainColorCode : MonoBehaviour
{
    public bool OrderedEval { get; set; }
    public int CodeLength { get => codeLength; set => SetCodeLength(value); }
    public int NumberOfColors { get; set; }

    public ProfiBrainColorDot[] profiBrainColorDots;
    private int codeLength;
    private int[] code;

    private void SetCodeLength(int codeLength)
    {
        if (this.codeLength == codeLength)
            return;

        this.codeLength = Mathf.Min(codeLength, profiBrainColorDots.Length);
        int i = 0;

        while (i < this.codeLength)
        {
            profiBrainColorDots[i].gameObject.SetActive(true);
            i++;
        }

        while (i < profiBrainColorDots.Length)
        {
            profiBrainColorDots[i].gameObject.SetActive(false);
            i++;
        }
    } 

    public int[] GenerateCode()
    {
        ProfiBrainColorDot.ShuffleColors();
        int n = Mathf.Min(NumberOfColors, codeLength);
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

        if (code.Length < codeLength)
        {
            List<int> codeList = new List<int>(code);

            while (codeList.Count < codeLength)
            {
                codeList.Add(code[Random.Range(0, code.Length)]);
            }

            code = codeList.ToArray();
            ArrayHelper.ShuffleArray(code);
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
        
        List<int> indexList = new List<int>(ArrayHelper.GetArray(0, n));

        for (int i = 0; i < n; i++)
        {
            if (this.code[i] == code[i])
            {
                result[0] += 1;
                indexList.Remove(i);
            }
        }

        List<int> foundList = new List<int>();

        foreach (int i in indexList)
        {
            int colorIndex = this.code[i];

            foreach (int j in indexList)
            {
                if (i != j && !foundList.Contains(j)
                    && colorIndex == code[j])
                {
                    result[1] += 1;
                    foundList.Add(j);
                    break;
                }
            }
        }

        return result;
    }
}
