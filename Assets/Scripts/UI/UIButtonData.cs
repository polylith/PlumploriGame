using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UIButtonData
{
    public string text;
    public System.Action action;

    public UIButtonData(string text, System.Action action)
    {
        this.text = text;
        this.action = action;
    }
}