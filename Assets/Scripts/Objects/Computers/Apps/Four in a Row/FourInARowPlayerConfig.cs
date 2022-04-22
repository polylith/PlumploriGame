using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FourInARowPlayerConfig : MonoBehaviour
{
    private static UIDropdownInput activeDropdownInput;

    public static void HideActiveDropdownInput()
    {
        if (null == activeDropdownInput)
            return;

        activeDropdownInput.HideList();
        activeDropdownInput = null;
    }

    private static void UpdateActiveDropdownInput(UIDropdownInput uiDropdownInput)
    {
        if (activeDropdownInput == uiDropdownInput)
        {
            if (uiDropdownInput.IsListVisible)
            {
                return;
            }

            activeDropdownInput = null;
            return;
        }

        HideActiveDropdownInput();

        if (uiDropdownInput.IsListVisible)
        {
            activeDropdownInput = uiDropdownInput;
        }
    }

    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }
    public int SelectedIndex {
        get => playerModeInput.SelectedIndex;
        set => playerModeInput.SelectedIndex = value;
    }

    public int index;
    public TextMeshProUGUI label;
    public UIDropdownInput playerModeInput;

    private bool isEnabled = true;

    private void Start()
    {
        label.SetText(
            Language.LanguageManager.GetText(
                Language.LangKey.Player,
                (index + 1).ToString()
            )
        );
    }

    public void Init(List<DropdownOption> options)
    {
        playerModeInput.options = options;
        playerModeInput.Init(true);
    }

    private void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        label.color = isEnabled ? Color.black : Color.gray;
        playerModeInput.IsEnabled = isEnabled;

        if (isEnabled)
        {
            playerModeInput.OnListShow += UpdateActiveDropdownInput;
        }
        else
        {
            playerModeInput.OnListShow -= UpdateActiveDropdownInput;
        }
    }
}
