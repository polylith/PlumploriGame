using UnityEngine;
using TMPro;

public class UIIconTextButton : UIIconButton
{
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

    public override void SetState(int state)
    {
        base.SetState(state);
        textMesh.color = State > -1 && IsEnabled ? UITextButton.textColors[State] : Color.gray;
    }
}