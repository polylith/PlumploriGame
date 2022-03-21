using System;
using System.Collections.Generic;

public class ProfiBrainApp : PCApp
{
    public int CurrentLevel { get => profiBrainOptions.CurrentLevel; }
    public int MaxLevel { get => profiBrainOptions.MaxLevel; }

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
        List<string>attributes = new List<string>()
        {
            "ProfiBrainApp.IsEnabled"
            
        };

        for (int level = 1; level <= MaxLevel; level++)
        {
            attributes.Add("ProfiBrainApp.Level" + level + "Solved");
        }

        return attributes;
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        Dictionary<string, Action<bool>> dict = new Dictionary<string, Action<bool>>();
        dict.Add("ProfiBrainApp.IsEnabled", SetEnabled);
        return dict;
    }

    public override List<Formula> GetGoals()
    {
        List<Formula> list = new List<Formula>();
        list.Add(new Implication(null, WorldDB.Get("ProfiBrainApp.IsEnabled")));
        List<Formula> conjunctionList = new List<Formula>();

        for (int level = 1; level <= MaxLevel; level++)
        {
            list.Add(new Implication(WorldDB.Get("ProfiBrainApp.Level" + level + "Solved"), null));
            conjunctionList.Add(WorldDB.Get("ProfiBrainApp.Level" + level + "Solved"));
        }

        list.Add(new Implication(new Conjunction(conjunctionList), null));
        return list;
    }

    protected override void Effect()
    {
        if (!isInfected || !IsActive)
            return;

        if (null != currentInputLine)
        {
            currentInputLine.RandomInput();

            if (UnityEngine.Random.value > 0.5f)
                profiBrainColorSelection.RandomInput();
        }

        GameEvent.GetInstance().Execute(Effect, UnityEngine.Random.Range(5f, 10f));
    }

    public override void SetInfected(bool isInfected)
    {
        if (this.isInfected == isInfected)
            return;

        this.isInfected = isInfected;
        ShowAppTitle();

        if (isInfected)
            GameEvent.GetInstance().Execute(Effect, UnityEngine.Random.Range(1f, 5f));
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
        SetCurrentLineIndex(-1);
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

        currentInputLine = null;
        currentLineIndex = index;

        if (index >= profiBrainInputLines.Length)
        {
            AudioManager.GetInstance().PlaySound("lose", computer.gameObject);
            FinishGame();
            return;
        }

        if (index < 0)
            return;

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
            computer?.AppFire("ProfiBrainApp.Level" + CurrentLevel + "Solved", true);
            return;
        }

        SetCurrentLineIndex(currentLineIndex + 1);
    }
}
