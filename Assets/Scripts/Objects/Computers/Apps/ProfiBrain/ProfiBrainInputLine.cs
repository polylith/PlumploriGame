using UnityEngine;
using UnityEngine.UI;

public class ProfiBrainInputLine : MonoBehaviour
{
    public ProfiBrainUserInput[] profiBrainUserInputs;
    public Image[] profiBrainOutputs;
    public GridLayoutGroup gridLayoutGroup;

    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }
    public bool IsComplete { get => CheckComplete(); }
    public int[] Code { get => GetCode(); }
    public bool OrderedEval { get => orderedEval; set => SetOrderedEval(value); }
    public bool EmptyInputs { get => emptyInputs; set => SetEmptyInputs(value); }

    public Image back;

    private bool isEnabled;
    private bool orderedEval;
    private bool emptyInputs;

    private void SetEmptyInputs(bool emptyInputs)
    {
        if (this.emptyInputs == emptyInputs)
            return;

        this.emptyInputs = emptyInputs;

        foreach (ProfiBrainUserInput profiBrainUserInput in profiBrainUserInputs)
        {
            profiBrainUserInput.EmptyInputs = emptyInputs;
        }
    }

    private void SetOrderedEval(bool orderedEval)
    {
        if (this.orderedEval == orderedEval)
            return;

        this.orderedEval = orderedEval;
        gridLayoutGroup.constraintCount = orderedEval ? 4 : 2;
    }

    public void SetAction(System.Action callBack)
    {
        foreach (ProfiBrainUserInput profiBrainUserInput in profiBrainUserInputs)
        {
            profiBrainUserInput.OnColorInput = callBack;
        }
    }

    public void ResetLine()
    {
        foreach (ProfiBrainUserInput profiBrainUserInput in profiBrainUserInputs)
        {
            profiBrainUserInput.ColorIndex = -1;
        }

        foreach (Image profiBrainOutput in profiBrainOutputs)
        {
            profiBrainOutput.rectTransform.localScale = new Vector3(0.25f, 0.25f, 1f);
            profiBrainOutput.color = Color.gray;
        }
    }

    private void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;

        foreach (ProfiBrainUserInput profiBrainUserInput in profiBrainUserInputs)
        {
            profiBrainUserInput.IsEnabled = isEnabled;
        }

        back.color = isEnabled ? new Color(0.8f, 0.93f, 1f) : new Color(0.8f, 0.8f, 0.8f);
    }

    private bool CheckComplete()
    {
        if (EmptyInputs)
        {
            foreach (ProfiBrainUserInput profiBrainUserInput in profiBrainUserInputs)
            {
                if (profiBrainUserInput.ColorIndex > -1)
                    return true;
            }

            return false;
        }

        foreach (ProfiBrainUserInput profiBrainUserInput in profiBrainUserInputs)
        {
            if (profiBrainUserInput.ColorIndex < 0)
                return false;
        }

        return true;
    }

    private int[] GetCode()
    {
        int n = profiBrainUserInputs.Length;
        int[] code = new int[n];

        for (int i = 0; i < n; i++)
        {
            code[i] = profiBrainUserInputs[i].ColorIndex;
        }

        return code;
    }

    public bool SetResult(int[] result)
    {
        int k = 0;
        
        if (OrderedEval)
        {
            int n = Mathf.Min(result.Length, profiBrainOutputs.Length);
            int i = 0;

            while (k < n)
            {
                if (result[k] > 0)
                {
                    profiBrainOutputs[k].color = Color.white;
                    profiBrainOutputs[k].rectTransform.localScale = Vector3.one;
                    i++;
                }

                k++;
            }

            return i == n;
        }

        bool isSolved = result[0] == profiBrainUserInputs.Length;

        for (int i = 0; i < result.Length; i++)
        {
            for (int j = 0; j < result[i]; j++)
            {
                profiBrainOutputs[k].color = i == 0 ? Color.white : Color.black;
                profiBrainOutputs[k].rectTransform.localScale = Vector3.one;
                k++;

                if (k >= profiBrainOutputs.Length)
                    return isSolved;
            }
        }

        return isSolved;
    }
}
