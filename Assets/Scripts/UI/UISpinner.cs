using UnityEngine;
using TMPro;

public class UISpinner : MonoBehaviour
{
    public delegate void ValueChanged();
    public event ValueChanged OnValueChanged; 

    public int Value { get => value; set => SetValue(value); }

    public TextMeshProUGUI[] digits;
    public UIIconButton upBtn;
    public UIIconButton downBtn;

    public int maxValue = 99;

    private int value;

    private void Awake()
    {
        upBtn.SetAction(Up);
        downBtn.SetAction(Down);
    }

    public void SetValue(int value)
    {
        this.value = value + maxValue;
        this.value %= maxValue;

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
