using System.Collections;
using UnityEngine;
using TMPro;
using Language;
using System;

/// <summary>
/// This config is for setting up the pc clock
/// </summary>
public class PCClockConfig : MonoBehaviour
{
    public bool IsVisible { get => isVisible; }

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

    public void StoreCurrentState(EntityData entityData)
    {
        entityData.SetAttribute("pcClockConfig.IsVisible", IsVisible ? "1" : "");
        entityData.SetAttribute("pcClockConfig.IsAutoSync", pcClock.IsAutoSync ? "1" : "");

        string currentTimeStr = null;
        string updatedTimestamp = null;

        if (!pcClock.IsAutoSync)
        {
            int h = hourSpinner.Value;
            int m = minSpinner.Value;
            int s = secSpinner.Value;
            currentTimeStr = h + ":" + m + ":" + s;

            DateTime now = DateTime.Now;
            updatedTimestamp = ((DateTimeOffset)now).ToUnixTimeSeconds().ToString();
        }

        entityData.SetAttribute("pcClockConfig.currentTime", currentTimeStr);
        entityData.SetAttribute("pcClockConfig.updatedTimestamp", updatedTimestamp);
    }

    public void RestoreCurrentState(EntityData entityData)
    {
        bool isVisible = entityData.GetAttribute("pcClockConfig.IsVisible").Equals("1");
        SetVisible(isVisible, true);

        bool isAutoSync = entityData.GetAttribute("pcClockConfig.IsAutoSync").Equals("1");
        int h = 0;
        int m = 0;
        int s = 0;

        if (!isAutoSync)
        {
            string currentTimeStr = entityData.GetAttribute("pcClockConfig.currentTime");
            string[] timeArray = currentTimeStr.Split(':');
            int currentTime = int.Parse(timeArray[0]) * 60 * 60;
            currentTime += int.Parse(timeArray[1]) * 60;
            currentTime += int.Parse(timeArray[2]);

            string updatedTimestampStr = entityData.GetAttribute("pcClockConfig.updatedTimestamp");

            DateTime now = DateTime.Now;
            long currentTimestamp = ((DateTimeOffset)now).ToUnixTimeSeconds();
            long updatedTimestamp = long.Parse(updatedTimestampStr);
            currentTimestamp -= updatedTimestamp;
            currentTime += (int)currentTimestamp;

            s = currentTime % 60;
            currentTime /= 60;
            m = currentTime % 60;
            currentTime /= 60;
            h = currentTime % 24;
            UpdateTime(h, m, s);
        }
        // TODO
        pcClock.Apply(h, m, s, isAutoSync);
        AutoTimeSyncButtonChanged(isAutoSync);
        Apply(false);
    }

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

        okButton.SetAction(() => {
            Apply(true);
        });
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

    private void Apply(bool hide)
    {
        int h = hourSpinner.Value;
        int m = minSpinner.Value;
        int s = secSpinner.Value;
        bool isAutoSync = autoTimeSyncButton.IsOn;
        pcClock.Apply(h, m, s, isAutoSync);

        if (hide)
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

    public void SetVisible(bool isVisible, bool instant = false)
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
