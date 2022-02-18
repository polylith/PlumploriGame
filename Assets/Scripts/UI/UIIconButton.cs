﻿using UnityEngine;
using UnityEngine.UI;

public class UIIconButton : UIButton
{
    private static Color[] iconColors = new Color[] {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    public float[] colorActive;
    public Sprite[] sprites;
    public float[] rotations;
    public Image icon;
    public bool changeIconColor = true;

    public override void SetEnabled(bool isEnabled)
    {
        base.SetEnabled(isEnabled);
        SetState(0);
    }

    public void SetActiveColor(Color color)
    {
        colorActive = new float[] { color.r, color.g, color.b, color.a };

        if (IsEnabled && State == 0)
            icon.color = color;
    }

    public void SetIcon(int index)
    {
        if (index < 0)
            return;

        if (index < sprites.Length)
            icon.sprite = sprites[index];

        if (index < rotations.Length)
            icon.transform.localRotation = Quaternion.Euler(0f, 0f, rotations[index]);
    }

    public override void SetState(int state)
    {
        base.SetState(state);

        if (State > -1 && IsEnabled)
        {
            Color color = State == 0 && null != colorActive && colorActive.Length > 0 
                ? new Color(colorActive[0], colorActive[1], colorActive[2]) 
                : iconColors[State];

            if (!changeIconColor)
                color = null != colorActive && colorActive.Length > 0
                    ? new Color(colorActive[0], colorActive[1], colorActive[2]) 
                    : iconColors[0];

            icon.color = color;
        }
        else
            icon.color = Color.gray;
    }
}