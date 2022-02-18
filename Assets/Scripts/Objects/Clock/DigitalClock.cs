using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Action;

public class DigitalClock : Clock
{
    public Text display;
    public bool showSec;
    public Canvas canvas;
    public Image alarmImg;
    public Image alarmActive;

    private bool alarmState;
    private List<DigitalClockAlarmData> alarms = new List<DigitalClockAlarmData>();
    private string displayText = "";
    private int alarmCount;
    private DigitalClockAlarmData currentAlarmData;

    private void Awake()
    {
        alarmImg.gameObject.SetActive(false);
        alarmActive.gameObject.SetActive(false);
        InitInteractableUI(true);
    }
    
    private void OnBecameVisible()
    {
        canvas.worldCamera = Camera.main;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override List<string> GetAttributes()
    {
        List<string> list = new List<string>();

        string[] attributes = new string[alarms.Count];

        for (int i = 0; i < alarms.Count; i++)
            list.Add(alarms[i].ToString());

        return list;
    }

    protected override void RegisterAtoms()
    {
        if (alarms.Count == 0)
            return;

        RegisterAtoms(GetAttributes());
    }

    public override void RegisterGoals()
    {
        Debug.Log("!!! TODO !!!");
    }

    public List<DigitalClockAlarmData> GetAlarms()
    {
        return alarms;
    }

    public List<DigitalClockAlarmData> SetAlarms(List<DigitalClockAlarmData> list)
    {
        ClearAlarms();

        foreach (DigitalClockAlarmData alarmData in list)
        {
            AddAlarm(alarmData);
        }

        return alarms;
    }

    public void SetCurrentTime(int h, int m)
    {
        SunMoonSimulation ins = SunMoonSimulation.GetInstance();

        if (null != ins)
            ins.OnTimeChange -= SetTime;

        SetTime(h, m, 0);
        RunClock();
    }

    public override void SetTime(float fh, int m, int s)
    {
        hour = (int)fh;
        min = m;
        sec = s;
        UpdateDisplay(fh, m, s);
        CheckAlarms(fh, m);
    }

    public void ClearAlarms()
    {
        alarms.Clear();
        alarmImg.gameObject.SetActive(false);
    }

    public void AddAlarm(DigitalClockAlarmData alarmData)
    {
        int alarmtime = alarmData.Time;

        if (alarmData.Time < 0)
            return;

        if (!alarms.Contains(alarmData))
            alarms.Add(alarmData);

        DigitalClockAlarmData activeAlarmData = alarms.Find(data => data.IsActive == true);
        alarmImg.gameObject.SetActive(null != activeAlarmData);
    }

    private void CheckAlarms(float fh, int m)
    {
        if (alarms.Count == 0 || alarmState)
            return;

        int h = (int)fh % 24;
        int alarmtime = m + h * 60;
        DigitalClockAlarmData activeAlarmData = alarms.Find(data => data.Time == alarmtime);

        if (null == activeAlarmData)
            return;

        currentAlarmData = activeAlarmData;
        currentAlarmData.IsOn = true;
        ((DigitalClockUI)InteractableUI).UpdateIsOnAlarmData(currentAlarmData);
        Fire(currentAlarmData.ToString(), true);
        alarmState = true;
        alarmCount = 0;
        Alarm();
    }

    public void StopAlarm()
    {
        alarmState = false;
        alarmActive.gameObject.SetActive(false);
        display.text = displayText;
        Fire(currentAlarmData.ToString(), false);

        if (null != currentAlarmData)
            currentAlarmData.IsOn = false;

        currentAlarmData = null;        
    }

    public override bool Interact(Interactable interactable)
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsCurrentAction(typeof(LookAction)))
        {
            ShowDescription();
            return true;
        }

        //if (!actionController.IsCurrentAction(typeof(PointerAction)))
            return base.Interact(interactable);

        /*
        bool isEnabled = !UIGame.GetInstance().IsCursorOverUI && alarmState;

        if (!isEnabled)
            return false;

        AudioManager.GetInstance()?.PlaySound("light.switch", gameObject);
        StopAlarm();
        return true;
        */
    }

    private void Alarm()
    {
        if (!alarmState)
            return;

        bool bAlarm = alarmCount % 2 == 0;
        display.text = bAlarm ? "" : displayText;
        alarmActive.gameObject.SetActive(bAlarm);

        if (bAlarm)
            AudioManager.GetInstance()?.PlaySound("alarm.clock", gameObject);

        alarmCount++;

        if (alarmCount == 60)
        {
            alarmCount = 0;
            display.text = displayText;
            alarmActive.gameObject.SetActive(false);
            GameEvent.GetInstance()?.Execute(Alarm, 60f);
        }
        else
            GameEvent.GetInstance()?.Execute(Alarm, 0.5f);
    }

    private void UpdateDisplay(float h, int m, int s)
    {
        displayText = (h < 10f ? "0" : "") + (int)h
            + ":" + (m < 10 ? "0" : "") + m
            + (showSec ? ":" + (s < 10 ? "0" : "") + s : "");
        display.text = displayText;
    }

    protected override void UpdateTime()
    {
        deltaTime = showSec ? 1 : 60;
        GameEvent.GetInstance()?.Execute(RunClock, showSec ? 1f : 60f);
    }
}