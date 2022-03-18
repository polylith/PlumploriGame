using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Language;
using UnityEngine.UI;
using System.Collections;

public class DigitalClockUI : InteractableUI
{
    public DigitalClockAlarmEntryDisplay displayPrefab;

    public ScrollRect scrollRect;
    public RectTransform content;

    public UISpinner newAlarmHours;
    public UISpinner newAlarmMinutes;
    public UIButton addNewAlarmBtn;

    public UISpinner setHours;
    public UISpinner setMinutes;
    public UIButton setTimeBtn;

    public TextMeshProUGUI alarmListHeader;
    public TextMeshProUGUI newAlarmHeader;
    public TextMeshProUGUI currentTimeHeader;

    private List<DigitalClockAlarmData> alarms;
    private bool doUpdateNewAlarmTime = true;
    private bool doUpdateSetTime = true;

    protected override void BeforeHide()
    {
        doUpdateNewAlarmTime = false;
        doUpdateSetTime = false;
    }

    protected override void Initialize()
    {
        if (!(interactable is DigitalClock digitalClock))
            return;

        alarms = digitalClock.GetAlarms();
        ShowAlarms();
        SetHeaders();
        InitNewAlarmInputs();
        InitCurrentTimeInputs();
        ScrollToTop();
    }

    private void SetHeaders()
    {
        alarmListHeader.SetText(
            LanguageManager.GetText(LangKey.Alarms)
        );
        newAlarmHeader.SetText(
            LanguageManager.GetText(
                LangKey.Add,
                LanguageManager.GetText(LangKey.Alarm)
            )
        );
        currentTimeHeader.SetText(
            LanguageManager.GetText(
                LangKey.Set,
                LanguageManager.GetText(LangKey.CurrentTime)
            )
        );
    }

    private void InitNewAlarmInputs()
    {
        addNewAlarmBtn.SetAction(AddNewAlarm);
        newAlarmHours.OnValueChanged += OnNewAlarmInputChanged;
        newAlarmMinutes.OnValueChanged += OnNewAlarmInputChanged;
        UpdateNewAlarmInputs();
        DoUpdateNewAlarmTime();
    }

    private void OnNewAlarmInputChanged()
    {
        doUpdateNewAlarmTime = false;
    }

    private void UpdateNewAlarmInputs()
    {
        UpdateInputs(newAlarmHours, newAlarmMinutes);
    }

    private void InitCurrentTimeInputs()
    {
        setTimeBtn.SetAction(SetCurrentTime);
        setHours.OnValueChanged += OnCurrentTimeInputChanged;
        setMinutes.OnValueChanged += OnCurrentTimeInputChanged;
        UpdateCurrentTimeInputs();
        DoUpdateSetTime();
    }

    private void OnCurrentTimeInputChanged()
    {
        doUpdateSetTime = false;
    }

    private void UpdateCurrentTimeInputs()
    {
        UpdateInputs(setHours, setMinutes);
    }

    private void UpdateInputs(UISpinner hoursInput, UISpinner minutesInput)
    {
        if (!(interactable is DigitalClock digitalClock))
            return;

        hoursInput.Value = digitalClock.hour;
        minutesInput.Value = digitalClock.min;
    }

    public void Delete(DigitalClockAlarmEntryDisplay display)
    {
        DigitalClockAlarmData alarmData = display.Alarmdata;

        if (alarmData.IsOn)
        {
            StopAlarm(display);
        }

        alarms.Remove(alarmData);
        Destroy(display.gameObject);
    }

    private void ShowAlarms()
    {
        foreach (Transform trans in content.transform)
        {
            Destroy(trans.gameObject);
        }

        if (alarms.Count > 0)
        {
            foreach (DigitalClockAlarmData alarmData in alarms)
            {
                DigitalClockAlarmEntryDisplay display = displayPrefab.Instantiate(this, alarmData);
                display.transform.SetParent(content, false);
            }
        }
        else
        {
            TextMeshProUGUI textMesh = Instantiate<TextMeshProUGUI>(statusLine);
            textMesh.color = Color.black;
            textMesh.SetText(
                LanguageManager.GetText(
                    LangKey.NotAvailable,
                    LanguageManager.GetText(
                        LangKey.Alarms
                    )
                )
            );
            textMesh.transform.SetParent(content, false);
        }
        

        content.sizeDelta = new Vector2(content.sizeDelta.x, ((alarms.Count * DigitalClockAlarmEntryDisplay.Height + 2.5f) + 30f) * 0.5f);
    }

    public void ScrollToTop()
    {
        StartCoroutine(IEScrollToTop());
    }

    private IEnumerator IEScrollToTop()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    private void AddNewAlarm()
    {
        int h = newAlarmHours.Value;
        int m = newAlarmMinutes.Value;
        int alarmtime = h * 60 + m;
        DigitalClockAlarmData alarmData = new DigitalClockAlarmData(alarmtime);

        if (alarms.Contains(alarmData))
            return;

        alarms.Add(alarmData);
        alarms.Sort((dat1, dat2) => {
            return dat1.Time - dat2.Time;
        });
        ShowAlarms();
    }

    public void UpdateIsOnAlarmData(DigitalClockAlarmData data)
    {
        DigitalClockAlarmEntryDisplay[] displays = content.GetComponentsInChildren<DigitalClockAlarmEntryDisplay>();

        foreach (DigitalClockAlarmEntryDisplay display in displays)
        {
            if (display.Alarmdata == data)
            {
                display.UpdateIsOnButton();
                break;
            }
        }
    }

    public void StopAlarm(DigitalClockAlarmEntryDisplay display)
    {
        if (!(interactable is DigitalClock digitalClock))
            return;

        digitalClock.StopAlarm();
        display.UpdateIsOnButton();
    }

    private void SetCurrentTime()
    {
        if (!(interactable is DigitalClock digitalClock))
            return;

        int h = setHours.Value;
        int m = setMinutes.Value;
        digitalClock.SetCurrentTime(h, m);
        doUpdateSetTime = true;
        DoUpdateSetTime();
    }

    public static void PointerEnter()
    {
        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetCursorEnabled(true, !uiGame.IsUIExclusive);
    }

    public static void PointerExit()
    {
        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetCursorEnabled(false, !uiGame.IsUIExclusive);
    }

    private void DoUpdateNewAlarmTime()
    {
        if (!doUpdateNewAlarmTime)
            return;

        UpdateNewAlarmInputs();

        GameEvent gameEvent = GameEvent.GetInstance();

        if (doUpdateNewAlarmTime && gameEvent)
            gameEvent.Execute(DoUpdateNewAlarmTime, 60f);
    }

    private void DoUpdateSetTime()
    {
        if (!doUpdateSetTime)
            return;

        UpdateCurrentTimeInputs();

        GameEvent gameEvent = GameEvent.GetInstance();

        if (doUpdateSetTime && gameEvent)
            gameEvent.Execute(DoUpdateSetTime, 60f);
    }
}
