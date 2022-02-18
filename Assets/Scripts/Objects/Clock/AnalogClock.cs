using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An analog clock is a clock with hands for hour,
/// minute and optionally seconds.
///
/// It is inherited from Interactable but it does not
/// provide any specific interaction.
/// </summary>
public class AnalogClock : Clock
{
    private static float[] Degrees = new float[] { 360f / 60f, 360f / 12f };

    public GameObject handHour;
    public GameObject handMin;
    public GameObject handSec;

    public override int IsInteractionEnabled()
    {
        return -1;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    protected override void RegisterAtoms()
    {
        // None
    }

    public override void RegisterGoals()
    {
        // None
    }

    public override void SetTime(float h, int m, int s)
    {
        if (null == handSec && null == handMin && null == handHour)
            return;

        float r = s * Degrees[0];

        if (null != handSec)
        {
            AudioManager.GetInstance()?.PlaySound("clock.tick", gameObject);
            handSec.transform.localEulerAngles = new Vector3(0f, 0f, r);
        }

        float fs = s / 60f;
        r = (float)(m + fs) * Degrees[0];

        if (null != handMin)
            handMin.transform.localEulerAngles = new Vector3(0f, 0f, r);

        if (null != handHour)
        {
            float fm = m / 60f;
            r = (float)((h % 12) + fm) * Degrees[1];
            handHour.transform.localEulerAngles = new Vector3(0f, 0f, r);
        }
    }

    protected override void UpdateTime()
    {
        deltaTime = null != handSec ? 1 : 60;
        GameEvent.GetInstance()?.Execute(RunClock, null != handSec ? 1f : 60f);
    }
}