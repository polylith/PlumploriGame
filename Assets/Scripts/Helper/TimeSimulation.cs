using UnityEngine;

/// <summary>
/// Abstract class to simulate daytime in Unity.
/// </summary>
public abstract class TimeSimulation : MonoBehaviour
{
    public delegate void OnTimeChangeEvent(float h, int m, int s);
    public event OnTimeChangeEvent OnTimeChange;

    public Light sunLight;
    protected float timeOfDay = 0.0f;

    protected float secondsPerMinute = 60.0f;
    protected float secondsPerHour;
    protected float secondsPerDay;
    protected float secondsPerHalfDay;

    protected int currentDay = 1;
    protected int currentMonth = 1;
    protected int daysInMonth = 30;
    protected int currentYear = 1;

    public float timeStep = 1f;
    public float timeScale = 1f;

    public void Reset()
    {
        System.DateTime now = System.DateTime.Now;
        Set(now.Day, now.Month, now.Year, now.Hour, now.Minute, now.Second);
    }

    public abstract void Set(int currentDay, int currentMonth, int currentYear, int hour, int min, int sec);

    protected void Propagate()
    {
        if (null == OnTimeChange)
            return;

        int s = (int)timeOfDay % 60;
        int m = (int)timeOfDay / 60 % 60;
        float h = timeOfDay / 60f / 60f;
        OnTimeChange.Invoke(h, m, s);
    }
}
