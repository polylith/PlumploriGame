using System.Collections;
using UnityEngine;
using TMPro;
using Language;

public class PCClockConfig : MonoBehaviour
{
    public PCClock pcClock;

    public TextMeshProUGUI titleText;

    public UISpinner hourSpinner;
    public UISpinner minSpinner;
    public UISpinner secSpinner;

    public UIOnOffButton autoTimeSyncButton;

    public UIIconButton okButton;
    public UIIconButton cancelButton;

    private bool isVisible;
    private IEnumerator ieScale;
    private bool isInfected;

    private void Start()
    {
        isVisible = true;
        SetVisible(false, true);
    }

    public void Init()
    {
        titleText.SetText(
            LanguageManager.GetText(
                LangKey.Clock
            )
        );
        UpdateTime(pcClock.hour, pcClock.min, pcClock.sec);

        okButton.SetAction(Apply);
        cancelButton.SetAction(Hide);

        autoTimeSyncButton.initialIsOn = pcClock.IsAutoSync;
        AutoTimeSyncButtonChanged(pcClock.IsAutoSync);
        autoTimeSyncButton.SetIsOn(pcClock.IsAutoSync, true);
        autoTimeSyncButton.SetLabelText(
            LanguageManager.GetText(
                LangKey.AutoSync
            )
        );
        autoTimeSyncButton.OnStateChange += AutoTimeSyncButtonChanged;
    }

    public void SetInfected(bool isInfected)
    {
        if (this.isInfected == isInfected)
            return;

        this.isInfected = isInfected;

        if (isInfected)
        {
            hourSpinner.maxValue = 99;
            minSpinner.maxValue = 99;
            secSpinner.maxValue = 99;
            return;
        }

        hourSpinner.maxValue = 24;
        minSpinner.maxValue = 60;
        secSpinner.maxValue = 60;
    }

    private void AutoTimeSyncButtonChanged(bool value)
    {
        hourSpinner.SetEnabled(!value);
        minSpinner.SetEnabled(!value);
        secSpinner.SetEnabled(!value);
    }

    private void SpinnerChanged()
    {
        pcClock.OnTimeUpdate -= UpdateTime;
    }

    private void UpdateTime(int h, int m, int s)
    {
        hourSpinner.Value = h;
        minSpinner.Value = m;
        secSpinner.Value = s;
    }

    private void Apply()
    {
        int h = hourSpinner.Value;
        int m = minSpinner.Value;
        int s = secSpinner.Value;
        bool isAutoSync = autoTimeSyncButton.IsOn;
        pcClock.Apply(h, m, s, isAutoSync);
        Hide();
    }

    public void ToggleVisibility()
    {
        SetVisible(!isVisible);
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

        if (isVisible)
        {
            pcClock.OnTimeUpdate += UpdateTime;

            hourSpinner.OnValueChanged += SpinnerChanged;
            minSpinner.OnValueChanged += SpinnerChanged;
            secSpinner.OnValueChanged += SpinnerChanged;            
        }
        else
        {
            pcClock.OnTimeUpdate -= UpdateTime;

            hourSpinner.OnValueChanged -= SpinnerChanged;
            minSpinner.OnValueChanged -= SpinnerChanged;
            secSpinner.OnValueChanged -= SpinnerChanged;            
        }

        Vector3 scale = isVisible ? Vector3.one : new Vector3(0f, 1f, 1f);

        if (instant)
        {
            transform.localScale = scale;
            return;
        }

        if (null != ieScale)
            StopCoroutine(ieScale);

        ieScale = IEScale(scale);
        StartCoroutine(ieScale);
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0f;

        while (f <= 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, f);

            yield return null;

            f += 0.2f;
        }

        yield return null;

        ieScale = null;
    }
}
