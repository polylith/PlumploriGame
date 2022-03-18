using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Language;

public class ProfiBrainOptions : MonoBehaviour
{
    public ProfiBrainSettings Settings { get => settings; }

    public RectTransform rectTrans;
    public TextMeshProUGUI optionsTitle;
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

        optionsTitle.SetText(
            LanguageManager.GetText(LangKey.Options)
        );
        newGameButton.SetText(
            LanguageManager.GetText(LangKey.NewGame)
        );
        hideButton.SetText(
            LanguageManager.GetText(LangKey.Cancel)
        );
        newGameButton.SetAction(ApplySettings);
        hideButton.SetAction(Hide);

        isVisible = true;
        SetVisible(false, true);
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
        int level = Mathf.Min(numberOfColors, codeLength) - 1;

        if (!emptyInputsCheck.IsOn)
            level++;

        if (!onlyUsedColorsCheck.IsOn)
            level++;

        if (!orderedEvalCheck.IsOn)
            level++;

        ShowLevel(level);
    }

    private void ShowLevel(int level)
    {
        int n = Mathf.Min(level, stars.Length);
        int i = 0;

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
