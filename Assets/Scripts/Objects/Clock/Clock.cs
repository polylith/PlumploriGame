using System;

/// <summary>
/// A clock can show time in several ways.
/// The time can be 
/// <list type="bullet">
/// <item><description>controlled by a SunMoonSimulation if present as a singleton.</description></item>
/// <item><description>initialized with the current time of the user.</description></item>
/// <item><description>set to a fixed start value.</description></item>
/// </list>
/// 
/// The time is updated using a deltaTime.
/// The default value is one second.
/// With larger values the expiration of the time is accelerated.
///
/// This abstract class must be implemented in specific subclasses.
/// </summary>
public abstract class Clock : Interactable
{
    public int hour;
    public int min;
    public int sec;
    public bool currentTime;

    protected int deltaTime = 1;

    private void Start()
    {
        SunMoonSimulation ins = SunMoonSimulation.GetInstance();

        if (null != ins)
            ins.OnTimeChange += SetTime;
        else
            InitClock();
    }

    protected override bool ShouldBeEnabled()
    {
        return true;
    }

    private void InitClock()
    {
        if (currentTime)
        {
            DateTime now = DateTime.Now;
            hour = now.Hour;
            min = now.Minute;
            sec = now.Second;
        }

        SetTime(hour, min, sec);
        UpdateTime();
    }

    /// <summary>
    /// Is used to run the clock if there is no SunMoonSimulation
    /// </summary>
    protected void RunClock()
    {
        sec += deltaTime;

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

    /// <summary>
    /// Updating the time display must be implemented in each
    /// specific subclass.
    /// </summary>
    protected abstract void UpdateTime();

    /// <summary>
    /// Setting the current time must be implemented in each
    /// specific subclass.
    /// </summary>
    public abstract void SetTime(float h, int m, int s);
}