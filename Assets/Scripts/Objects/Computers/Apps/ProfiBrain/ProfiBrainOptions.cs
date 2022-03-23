using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Language;

public class ProfiBrainOptions : MonoBehaviour
{
    public ProfiBrainSettings Settings { get => settings; }
    public int CurrentLevel { get => currentLevel; }
    public int MaxLevel { get => stars.Length - 1; }

    public RectTransform rectTrans;
    public TextMeshProUGUI optionsTitle;
    public TextMeshProUGUI[] labels;

    public UITextButton newGameButton;
    public UITextButton hideButton;

    public UISpinner codeLengthSpinner;
    public UISpinner numberOfColorsSpinner;

    public UIOnOffButton emptyInputsCheck;
    public UIOnOffButton onlyUsedColorsCheck;
    public UIOnOffButton orderedEvalCheck;

    public Image[] stars;

    private bool isVisible;
    private readonly ProfiBrainSettings settings = new ProfiBrainSettings();
    private System.Action newGameAction;
    private int currentLevel;

    public void Init(System.Action newGameAction)
    {
        this.newGameAction = newGameAction;

        codeLengthSpinner.maxValue = 5;
        codeLengthSpinner.minValue = 2;
        codeLengthSpinner.OnValueChanged += OnCodeLengthChange;

        numberOfColorsSpinner.minValue = 2;
        numberOfColorsSpinner.OnValueChanged += UpdateLevel;

        emptyInputsCheck.OnStateChange += UpdateLevel;
        onlyUsedColorsCheck.OnStateChange += UpdateLevel;
        orderedEvalCheck.OnStateChange += UpdateLevel;

        InitTexts();
        newGameButton.SetAction(ApplySettings);
        hideButton.SetAction(Hide);

        isVisible = true;
        SetVisible(false, true);
    }

    private void InitTexts()
    {
        optionsTitle.SetText(
            LanguageManager.GetText(LangKey.Options)
        );
        newGameButton.SetText(
            LanguageManager.GetText(LangKey.NewGame)
        );
        hideButton.SetText(
            LanguageManager.GetText(LangKey.Cancel)
        );
        labels[0].SetText(
            LanguageManager.GetText(LangKey.CodeLength)
        );
        labels[1].SetText(
            LanguageManager.GetText(LangKey.NumberOfColors)
        );
        labels[2].SetText(
            LanguageManager.GetText(LangKey.Mode)
        );
        labels[3].SetText(
            LanguageManager.GetText(LangKey.AllowEmptyInputs)
        );
        labels[4].SetText(
            LanguageManager.GetText(LangKey.ShowOnlyUsedColors)
        );
        labels[5].SetText(
            LanguageManager.GetText(LangKey.OrderedEvaluation)
        );
        labels[6].SetText(
            LanguageManager.GetText(LangKey.Level)
        );
    }

    private void OnCodeLengthChange()
    {
        int codeLength = codeLengthSpinner.Value;
        int numberOfColors = numberOfColorsSpinner.Value;
        numberOfColorsSpinner.maxValue = codeLength + 1;
        numberOfColorsSpinner.Value = Mathf.Min(codeLength, numberOfColors);
        UpdateLevel();
    }

    private void UpdateLevel(bool value)
    {
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        int codeLength = codeLengthSpinner.Value;
        int numberOfColors = numberOfColorsSpinner.Value;
        int count = Mathf.Min(numberOfColors, codeLength) - codeLengthSpinner.minValue + 1;
        int max = (codeLengthSpinner.maxValue - codeLengthSpinner.minValue);

        if (onlyUsedColorsCheck.IsOn)
            count -= 2;

        if (orderedEvalCheck.IsOn)
            count -= 2;

        float fLevel = ((float)count / (float)max) * (float)stars.Length;
        int level = Mathf.CeilToInt(fLevel);

        if (emptyInputsCheck.IsOn)
            level--;
        
        level = Mathf.Max(1, level);
        ShowLevel(level);
    }

    private void ShowLevel(int level)
    {
        int n = Mathf.Min(level, stars.Length);
        int i = 0;
        currentLevel = n;

        while (i < n)
        {
            stars[i].gameObject.SetActive(true);
            i++;
        }

        while (i < stars.Length)
        {
            stars[i].gameObject.SetActive(false);
            i++;
        }
    }

    private void ApplySettings()
    {
        settings.codeLength = codeLengthSpinner.Value;
        settings.numberOfColors = numberOfColorsSpinner.Value;
        settings.emptyInputs = emptyInputsCheck.IsOn;
        settings.onlyUsedColors = onlyUsedColorsCheck.IsOn;
        settings.orderedEval = orderedEvalCheck.IsOn;
        newGameAction?.Invoke();
    }

    public void ResetInputs()
    {
        codeLengthSpinner.Value = settings.codeLength;
        numberOfColorsSpinner.Value = settings.numberOfColors;

        emptyInputsCheck.SetIsOn(settings.emptyInputs, true);
        onlyUsedColorsCheck.SetIsOn(settings.onlyUsedColors, true);
        orderedEvalCheck.SetIsOn(settings.orderedEval, true);

        OnCodeLengthChange();
    }

    public void Show()
    {
        ResetInputs();
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool isVisible, bool instant = false)
    {
        if (this.isVisible == isVisible)
            return;

        this.isVisible = isVisible;
        Vector3 scale = isVisible ? Vector3.one : new Vector3(0f, 1f, 1f);

        if (instant)
        {
            rectTrans.localScale = scale;
            return;
        }

        rectTrans.DOScale(scale, 0.35f);
    }
}
