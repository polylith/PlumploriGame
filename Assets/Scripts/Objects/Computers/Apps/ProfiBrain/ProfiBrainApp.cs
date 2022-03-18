using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfiBrainApp : PCApp
{
    public UIIconButton optionsButton;
    public UIIconButton newGameButton;

    public ProfiBrainOptions profiBrainOptions;

    public ProfiBrainColorSelection profiBrainColorSelection;
    public ProfiBrainInputLine[] profiBrainInputLines;
    public ProfiBrainColorCode profiBrainColorCode;

    private int currentLineIndex;
    private ProfiBrainInputLine currentInputLine;
    private bool onlyUsedColors;
    
    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        return null;
    }

    public override List<Formula> GetGoals()
    {
        throw new NotImplementedException();
    }

    protected override void Effect()
    {
        // TODO
    }

    protected override void Init()
    {
        base.Init();

        newGameButton.SetAction(NewGame);
        optionsButton.SetAction(profiBrainOptions.Show);
        profiBrainOptions.Init(NewGame);
        NewGame();
    }

    private void SetupOptions()
    {
        ProfiBrainSettings settings = profiBrainOptions.Settings;

        foreach (ProfiBrainInputLine profiBrainInputLine in profiBrainInputLines)
        {
            profiBrainInputLine.CodeLength = settings.codeLength;
            profiBrainInputLine.OrderedEval = settings.orderedEval;
            profiBrainInputLine.EmptyInputs = settings.emptyInputs;
        }

        profiBrainColorCode.CodeLength = settings.codeLength;
        profiBrainColorCode.NumberOfColors = settings.numberOfColors;
        profiBrainColorCode.OrderedEval = settings.orderedEval;
        onlyUsedColors = settings.onlyUsedColors;
    }

    private void NewGame()
    {
        SetupOptions();
        optionsButton.SetEnabled(false);
        profiBrainOptions.Hide();

        foreach (ProfiBrainInputLine profiBrainInputLine in profiBrainInputLines)
        {
            profiBrainInputLine.IsEnabled = false;
            profiBrainInputLine.ResetLine();
        }

        int[] usedColors = profiBrainColorCode.GenerateCode();
        profiBrainColorSelection.ShowColors(onlyUsedColors, usedColors);
        profiBrainColorSelection.SetCheckAction(Evaluate);
        profiBrainColorSelection.SetCheckButtonEnabled(false);
        SetCurrentLineIndex(0);
    }

    private void FinishGame()
    {
        profiBrainColorCode.ShowCode();
        profiBrainColorSelection.SetCheckAction(Evaluate);
        profiBrainColorSelection.SetCheckButtonEnabled(false);
        optionsButton.SetEnabled(true);
    }

    private void SetCurrentLineIndex(int index)
    {
        if (null != currentInputLine)
        {
            currentInputLine.IsEnabled = false;
        }

        if (index >= profiBrainInputLines.Length)
        {
            AudioManager.GetInstance().PlaySound("lose", computer.gameObject);
            FinishGame();
            return;
        }

        currentLineIndex = index;
        currentInputLine = profiBrainInputLines[index];
        currentInputLine.SetAction(OnColorInput);
        currentInputLine.IsEnabled = true;
        profiBrainColorSelection.IsEnabled = true;
        optionsButton.SetEnabled(currentLineIndex < 1);
    }

    private void OnColorInput()
    {
        profiBrainColorSelection.SetCheckButtonEnabled(currentInputLine.IsComplete);
    }

    private void Evaluate()
    {
        profiBrainColorSelection.SetCheckButtonEnabled(false);
        currentInputLine.IsEnabled = false;
        int[] code = currentInputLine.Code;
        int[] result = profiBrainColorCode.Evaluate(code);
        bool isSolved = currentInputLine.SetResult(result);

        if (isSolved)
        {
            AudioManager.GetInstance().PlaySound("win", computer.gameObject);
            FinishGame();
            return;
        }

        SetCurrentLineIndex(currentLineIndex + 1);
    }
}
