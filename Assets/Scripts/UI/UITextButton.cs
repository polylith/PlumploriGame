using UnityEngine;
using TMPro;

public class UITextButton : UIButton
{
    public static Color[] textColors = new Color[] {
        Color.black,
        Color.blue,
        Color.yellow
    };

    public TextMeshProUGUI textMesh;

    protected override void SetEnabled(bool isEnabled)
    {
        base.SetEnabled(isEnabled);
        SetState(0);
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public override void SetState(int state)
    {
        base.SetState(state);
        textMesh.color = IsEnabled ? textColors[State] : Color.gray; 
    }

    public UITextButton Instantiate(UIButtonData data)
    {
        UITextButton button = Instantiate(this) as UITextButton;
        button.SetText(data.text);
        button.SetAction(data.action);
        return button;
    }
}