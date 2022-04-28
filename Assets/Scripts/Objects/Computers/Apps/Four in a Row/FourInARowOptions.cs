using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Language;

public class FourInARowOptions : MonoBehaviour
{
    public FourInARowSettings Settings { get => settings; }

    public RectTransform rectTrans;
    public TextMeshProUGUI optionsTitle;
    public TextMeshProUGUI[] labels;

    [SerializeField]
    public List<DropdownOption> playerModes;

    public FourInARowPlayerConfig[] playerConfigs;

    public UITextButton newGameButton;
    public UITextButton hideButton;

    public UISpinner numberOfPlayersSpinner;

    private bool isVisible;
    private readonly FourInARowSettings settings = new FourInARowSettings();
    private System.Action newGameAction;
    private System.Action hideAction;
    private bool isHideActionEnabled;

    public void Init(System.Action newGameAction, System.Action hideAction)
    {
        this.newGameAction = newGameAction;
        this.hideAction = hideAction;

        numberOfPlayersSpinner.Value = settings.numberOfPlayers;
        numberOfPlayersSpinner.minValue = 2;
        numberOfPlayersSpinner.maxValue = 5;
        numberOfPlayersSpinner.OnValueChanged += UpdatePlayerConfigs;

        InitTexts();
        newGameButton.SetAction(ApplySettings);
        hideButton.SetAction(Hide);

        foreach (FourInARowPlayerConfig playerConfig in playerConfigs)
            playerConfig.Init(playerModes);

        isVisible = true;
        SetVisible(false, true);
    }

    private void InitTexts()
    {
        optionsTitle.SetText(
            LanguageManager.GetText(LangKey.Options)
        );
        labels[0].SetText(
            LanguageManager.GetText(LangKey.NumberOfPlayers)
        );
        newGameButton.SetText(
            LanguageManager.GetText(LangKey.NewGame)
        );
        hideButton.SetText(
            LanguageManager.GetText(LangKey.Cancel)
        );
    }

    private void UpdatePlayerConfigs()
    {
        int numberOfPlayers = numberOfPlayersSpinner.Value;

        foreach (FourInARowPlayerConfig playerConfig in playerConfigs)
        {
            playerConfig.IsEnabled = playerConfig.index < numberOfPlayers;
        }
    }

    private void ApplySettings()
    {
        isHideActionEnabled = false;
        settings.numberOfPlayers = numberOfPlayersSpinner.Value;
        settings.playerMode0 = playerConfigs[0].SelectedIndex;
        settings.playerMode1 = playerConfigs[1].SelectedIndex;
        settings.playerMode2 = playerConfigs[2].SelectedIndex;
        settings.playerMode3 = playerConfigs[3].SelectedIndex;

        foreach (FourInARowPlayerConfig playerConfig in playerConfigs)
        {
            playerConfig.IsEnabled = false;
        }

        newGameAction?.Invoke();
    }

    public void ResetInputs()
    {
        int numberOfPlayers = settings.numberOfPlayers;
        int[] playerModes = new int[] {
            settings.playerMode0,
            settings.playerMode1,
            settings.playerMode2,
            settings.playerMode3
        };

        for (int i = 0; i < playerConfigs.Length; i++)
        {
            playerConfigs[i].SelectedIndex = playerModes[i];
            playerConfigs[i].IsEnabled = i < numberOfPlayers;
        }
    }

    public void Show(bool isHideActionEnabled = false)
    {
        ResetInputs();
        hideButton.IsEnabled = isHideActionEnabled;
        this.isHideActionEnabled = isHideActionEnabled;
        SetVisible(true);
    }

    public void Hide()
    {
        FourInARowPlayerConfig.HideActiveDropdownInput();
        SetVisible(false);

        if (!isHideActionEnabled)
            return;

        hideAction.Invoke();
        isHideActionEnabled = false;
    }

    private void SetVisible(bool isVisible, bool instant = false)
    {
        if (this.isVisible == isVisible)
            return;

        this.isVisible = isVisible;
        Vector3 scale = isVisible ? Vector3.one : new Vector3(1f, 0f, 1f);

        if (instant)
        {
            rectTrans.localScale = scale;
            return;
        }

        rectTrans.DOScale(scale, 0.35f);
    }
}
