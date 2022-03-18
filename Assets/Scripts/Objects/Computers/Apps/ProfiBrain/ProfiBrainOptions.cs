using UnityEngine;
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

    public UIOnOffButton emptyInputsCheck;
    public UIOnOffButton onlyUsedColorsCheck;
    public UIOnOffButton orderedEvalCheck;

    private bool isVisible;
    private readonly ProfiBrainSettings settings = new ProfiBrainSettings();
    private System.Action newGameAction;

    public void Init(System.Action newGameAction)
    {
        this.newGameAction = newGameAction;

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

    private void ApplySettings()
    {
        settings.emptyInputs = emptyInputsCheck.IsOn;
        settings.onlyUsedColors = onlyUsedColorsCheck.IsOn;
        settings.orderedEval = orderedEvalCheck.IsOn;
        newGameAction?.Invoke();
    }

    public void ResetInputs()
    {
        emptyInputsCheck.SetIsOn(settings.emptyInputs, true);
        onlyUsedColorsCheck.SetIsOn(settings.onlyUsedColors, true);
        orderedEvalCheck.SetIsOn(settings.orderedEval, true);
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
