using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Language;

public class DigitalClockAlarmEntryDisplay : MonoBehaviour
{
    public static readonly float Height = 55f;

    public DigitalClockAlarmEntryDisplay Instantiate(DigitalClockUI ui, DigitalClockAlarmData alarmdata)
    {
        DigitalClockAlarmEntryDisplay display = Instantiate< DigitalClockAlarmEntryDisplay>(this);
        display.Init(ui, alarmdata);
        return display;
    }

    public DigitalClockAlarmData Alarmdata { get; private set; }

    public TextMeshProUGUI timeDisplay;
    public Image activeIcon;
    public Image isOnIcon;
    public UIIconButton isOnBtn;
    public UIIconButton activateBtn;
    public UIIconButton deleteBtn;

    private DigitalClockUI ui;

    private void Init(DigitalClockUI ui, DigitalClockAlarmData alarmdata)
    {
        Alarmdata = alarmdata;
        this.ui = ui;

        ShowAlarmTime();
        isOnBtn.SetAction(StopAlarm);
        isOnBtn.SetActiveColor(Color.yellow);
        activateBtn.SetAction(ToggleActive);
        deleteBtn.SetAction(Delete);

        isOnBtn.SetToolTip(
            LanguageManager.GetText(
                LangKey.Stopp,
                LanguageManager.GetText(LangKey.Alarm)
            )
        );

        deleteBtn.SetToolTip(
            LanguageManager.GetText(
                LangKey.Delete,
                LanguageManager.GetText(LangKey.Alarm)
            )
        );

        SetActive(Alarmdata.IsActive);
        UpdateIsOnButton();
    }

    public void UpdateIsOnButton()
    {
        isOnBtn.SetEnabled(Alarmdata.IsOn);
        isOnBtn.SetState(Alarmdata.IsOn ? 0 : -1);
        isOnIcon.gameObject.SetActive(Alarmdata.IsOn);
    }

    private void StopAlarm()
    {
        ui.StopAlarm(this);
    }

    public void ShowAlarmTime()
    {
        timeDisplay.SetText(Alarmdata.TimeString);
    }

    public void SetActive(bool isActive)
    {
        Alarmdata.IsActive = isActive;
        activeIcon.color = isActive ? Color.red : Color.gray;

        activateBtn.SetToolTip(
            LanguageManager.GetText(
                isActive ? LangKey.Deactivate : LangKey.Activate,
                LanguageManager.GetText(LangKey.Alarm)
            )
        );
        activateBtn.SetActiveColor(isActive ? Color.gray : Color.red);

        if (!isActive && Alarmdata.IsOn)
        {
            StopAlarm();
        }
    }

    private void ToggleActive()
    {
        SetActive(!Alarmdata.IsActive);
    }

    private void Delete()
    {
        ui.Delete(this);
    }
}
