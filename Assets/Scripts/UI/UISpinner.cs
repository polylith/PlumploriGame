using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This input field provides a discrete input by buttons.
/// One button increases the value by one, the other
/// decreases the value by one.
/// </summary>
public class UISpinner : MonoBehaviour
{
    public delegate void ValueChanged();
    public event ValueChanged OnValueChanged; 

    public bool IsEnabled { get => isEnabled; }
    public int Value { get => value; set => SetValue(value); }

    public Image background;
    public TextMeshProUGUI[] digits;
    public Color enabledDisplayColor = Color.red;
    public Color disabledDisplayColor = Color.gray;
    public Color enabledBackgroundColor = Color.black;
    public Color disabledBackgroundColor = new Color(0.8f, 0.8f, 0.8f);
    public UIIconButton upBtn;
    public UIIconButton downBtn;

    public int maxValue = 99;
    public int minValue = 0;

    private int value;
    private bool isEnabled = true;

    private void Awake()
    {
        upBtn.SetAction(Up);
        downBtn.SetAction(Down);
        SetEnabled(isEnabled, true);
    }

    public void SetEnabled(bool isEnabled, bool force = false)
    {
        if (this.isEnabled == isEnabled && !force)
            return;

        this.isEnabled = isEnabled;

        foreach (TextMeshProUGUI textMesh in digits)
            textMesh.color = isEnabled ? enabledDisplayColor : disabledDisplayColor;

        background.color = isEnabled ? enabledBackgroundColor : disabledBackgroundColor;

        upBtn.IsEnabled = isEnabled;
        downBtn.IsEnabled = isEnabled;
    }

    private void SetValue(int value)
    {
        this.value = value + maxValue;
        this.value %= maxValue;

        if (minValue > 0)
        {
            this.value = Mathf.Clamp(this.value, minValue, maxValue - 1);
        }
        
        value = this.value;

        for (int i = 0; i < digits.Length; i++)
        {
            int digitValue = value % 10;
            digits[i].SetText(digitValue.ToString());
            value /= 10;
        }
    }

    private void Up()
    {
        SetValue(value + 1);
        OnValueChanged?.Invoke();
    }

    private void Down()
    {
        SetValue(value - 1);
        OnValueChanged?.Invoke();
    }
}
