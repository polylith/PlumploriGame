using UnityEngine;
using TMPro;

public class PCClock : MonoBehaviour
{
    public TextMeshProUGUI display;
    public int hour;
    public int min;
    public int sec;
    public bool currentTime;

    private void Start()
    {
        SunMoonSimulation ins = SunMoonSimulation.GetInstance();

        if (null != ins)
            ins.OnTimeChange += SetTime;
        else
            InitClock();
    }

    private void InitClock()
    {
        if (currentTime)
        {
            System.DateTime now = System.DateTime.Now;
            hour = now.Hour;
            min = now.Minute;
            sec = now.Second;
        }

        SetTime(hour, min, sec);
        UpdateTime();
    }

    private void RunClock()
    {
        sec++;

        if (sec >= 60)
        {
            int t = sec / 60;
            sec %= 60;
            min += t;
        }

        if (min >= 60)
        {
            int t = min / 60;
            min %= 60;
            hour += t;
            hour %= 24;
        }

        SetTime(hour, min, sec);
        UpdateTime();
    }

    private void UpdateTime()
    {
        GameEvent.GetInstance()?.Execute(RunClock, 1f);
    }

    public void SetTime(float h, int m, int s)
    {
        UpdateDisplay(h, m, s);
    }

    private void UpdateDisplay(float h, int m, int s)
    {
        string displayText = (h < 10f ? "0" : "") + (int)h
            + ":" + (m < 10 ? "0" : "") + m
            + ":" + (s < 10 ? "0" : "") + s;
        display.SetText(displayText);
    }
}