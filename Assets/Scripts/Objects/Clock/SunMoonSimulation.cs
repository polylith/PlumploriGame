using System;
using UnityEngine;

public class SunMoonSimulation : TimeSimulation
{
    public static SunMoonSimulation GetInstance()
    {
        return ins;
    }

    public GameObject moonCenter;
    public GameObject moon;

    public static float moonDistance = 100f;
    private static Color[] colors = new Color[] { Color.black, new Color(1f, 0.95f, 0.85f), new Color(0.06f, 0.66f, 1f) };
    private static SunMoonSimulation ins;

    private float sunx;    
    
    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        TimeUpdate();
    }

    private void Init()
    {
        if (null != moon)
            moon.transform.localPosition = new Vector3(moonDistance, 0f, 0f);

        secondsPerHour = secondsPerMinute * 60.0f;
        secondsPerDay = secondsPerHour * 24.0f;
        secondsPerHalfDay = secondsPerDay * 0.5f;
        Reset();
    }

    public override void Set(int currentDay, int currentMonth, int currentYear, int hour, int min, int sec)
    {
        daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
        timeOfDay = hour * 60f;
        timeOfDay += min;
        timeOfDay *= 60f;
        timeOfDay += sec;
        Propagate();
    }

    public void TimeUpdate()
    {
        SunUpdate();

        timeOfDay += timeStep;

        if (timeOfDay >= secondsPerDay)
        {
            currentDay++;

            if (currentDay > daysInMonth)
            {
                currentDay = 1;
                currentMonth++;

                if (currentMonth > 12)
                {
                    currentYear++;
                    currentMonth = 1;
                }

                daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            }

            timeOfDay = 0f;
        }

        Propagate();
        GameEvent.GetInstance()?.Execute(TimeUpdate, timeScale);
    }

    private void SunUpdate()
    {
        float hour = (timeOfDay / 60f / 60f) % 24f;
        bool isActive = hour >= 4f && hour <= 22f;
        sunLight.gameObject.SetActive(isActive);

        float v = ((secondsPerHalfDay - timeOfDay) / secondsPerDay);
        RenderSettings.skybox.SetFloat("_Exposure", 1f - 2f * Mathf.Abs(v));
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1f + 5f * Mathf.Abs(v));
        sunx = 90f - v * 90f;
        transform.eulerAngles = new Vector3(sunx, -90f, 0f);

        //Debug.Log("H " + hour + " v " + v + " sun x " + sunx);

        v = Mathf.Abs(v);
        Color color1 = Color.Lerp(colors[0], colors[1], v);
        Color color2 = Color.Lerp(colors[0], colors[2], v);
        Color color3 = Color.Lerp(color2, color1, v);
        RenderSettings.ambientIntensity = v;

        v = 1f - v;

        if (isActive)
            sunLight.intensity = v * 1.25f;

        RenderSettings.haloStrength = v;
        RenderSettings.ambientEquatorColor = color1;
        RenderSettings.ambientSkyColor = color2;
        RenderSettings.fogColor = color3;
        
    }
}