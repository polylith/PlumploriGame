using UnityEngine;

public class ProfiBrainColorSelection : MonoBehaviour
{
    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }

    public UIIconButton checkButton;
    public ProfiBrainColorInput[] profiBrainColorInputs;

    private bool isEnabled;

    public void RandomInput()
    {
        int i = Random.Range(0, profiBrainColorInputs.Length);
        GameEvent.GetInstance().Execute(profiBrainColorInputs[i].Click, 0.5f);

        if (Random.value > 0.8f)
        {
            GameEvent.GetInstance().Execute(() => checkButton.Click(), 1f);
        }
    }

    public void ShowColors(bool onlyUsedColors, int[] usedColors)
    {
        if (onlyUsedColors)
        {
            int i = 0;

            while (i < usedColors.Length)
            {
                profiBrainColorInputs[i].gameObject.SetActive(true);
                profiBrainColorInputs[i].ColorIndex = -1;
                profiBrainColorInputs[i].ColorIndex = i;
                i++;
            }

            while (i < profiBrainColorInputs.Length)
            {
                profiBrainColorInputs[i].gameObject.SetActive(false);
                profiBrainColorInputs[i].ColorIndex = -1;
                i++;
            }
        }
        else
        {
            for (int i = 0; i < profiBrainColorInputs.Length; i++)
            {
                profiBrainColorInputs[i].gameObject.SetActive(true);
                profiBrainColorInputs[i].ColorIndex = -1;
                profiBrainColorInputs[i].ColorIndex = i;
            }
        }

        profiBrainColorInputs[0].Select();
    }

    private void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;

        foreach (ProfiBrainColorInput profiBrainColorInput in profiBrainColorInputs)
        {
            profiBrainColorInput.IsEnabled = isEnabled;
        }
    }

    public void SetCheckAction(System.Action action)
    {
        checkButton.SetAction(action);
    }

    public void SetCheckButtonEnabled(bool isEnabled)
    {
        checkButton.SetEnabled(isEnabled);
    } 
}
