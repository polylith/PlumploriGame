using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// <para>
/// Although a computer clock is also a clock and there is
/// already an abstract superclass for clocks that provides
/// exactly the same functions, the computer clock must not
/// inherit from this class because otherwise it would become
/// an interactive object in the 3D world.
/// </para>
/// </summary>
public class PCClock : MonoBehaviour, IPointerClickHandler
{
    public delegate void OnTimeUpdateEvent(int hour, int min, int sec);
    public event OnTimeUpdateEvent OnTimeUpdate;

    public bool IsAutoSync { get => currentTime || isSunMoonControlled; }
    public bool IsInfected { get => isInfected; set => SetInfected(value); }

    public TextMeshProUGUI display;
    public int hour;
    public int min;
    public int sec;
    public bool currentTime;
    public PCClockConfig pcClockConfig;

    private bool isSunMoonControlled;
    private bool isInfected;
    private IEnumerator ieRunClock;

    private void Start()
    {
        Init();
    }

    private void SetInfected(bool isInfected)
    {
        if (this.isInfected == isInfected)
            return;

        this.isInfected = isInfected;
        pcClockConfig.SetInfected(isInfected);
    }

    public void Apply(int h, int m, int s, bool isAutoSync)
    {
        ClearAutoSync();

        if (isAutoSync)
        {
            SunMoonSimulation ins = SunMoonSimulation.GetInstance();

            if (null != ins)
            {
                ins.OnTimeChange += SetTime;
                isSunMoonControlled = true;
                currentTime = false;
                return;
            }
            else
            {
                currentTime = true;
            }
        }
        else
        {
            currentTime = false;
            hour = h;
            min = m;
            sec = s;
        }

        isSunMoonControlled = false;
        InitClock();
    }

    private void ClearRunClock()
    {
        if (null != ieRunClock)
        {
            StopCoroutine(ieRunClock);
            ieRunClock = null;
        }
    }

    private void ClearAutoSync()
    {
        ClearRunClock();
        SunMoonSimulation ins = SunMoonSimulation.GetInstance();

        if (null != ins)
        {
            ins.OnTimeChange -= SetTime;
            isSunMoonControlled = false;
        }
    }

    public void ResetTime()
    {
        ClearAutoSync();
        GameEvent.GetInstance().Execute(Init, 2f);
    }

    private void Init()
    {
        SunMoonSimulation ins = SunMoonSimulation.GetInstance();

        if (null != ins)
        {
            ins.OnTimeChange += SetTime;
            isSunMoonControlled = true;
            currentTime = false;
        }
        else
        {
            isSunMoonControlled = false;
            InitClock();
        }

        pcClockConfig.Init();
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

    private IEnumerator IERunClock()
    {
        while (true)
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

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private void UpdateTime()
    {
        ClearRunClock();
        ieRunClock = IERunClock();
        StartCoroutine(ieRunClock);
    }

    public void SetTime(float h, int m, int s)
    {
        hour = (int)h;
        min = m;
        sec = s;
        OnTimeUpdate?.Invoke(hour, min, sec);
        UpdateDisplay(h, m, s);        
    }

    private void UpdateDisplay(float h, int m, int s)
    {
        string displayText = (h < 10f ? "0" : "") + (int)h
            + ":" + (m < 10 ? "0" : "") + m
            + ":" + (s < 10 ? "0" : "") + s;
        display.SetText(displayText);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        pcClockConfig.ToggleVisibility();
    }
}