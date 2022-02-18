using Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// Window handle and window are separate interactables.
/// The window handle defines with the states 0, 1, 2, 3
/// how the window can be opened.
/// </para>
/// <para>
/// In state 0 the window is not interactive. In this case,
/// the collider of the window is deactivated.
/// </para>
/// <para>
/// In states 1, 2 and 3 the collider of the window is
/// activated. In these states, the window can be opened
/// and closed with the open action.
/// </para>
/// <para>
/// With the use action, the window handle can be interacted
/// with. If the window is open, the window handle cannot be
/// interacted with.
/// </para>
/// </summary>
public class WindowHandle : Interactable
{
    public GameObject handleObject;
    public GameObject panel;
    public AudioSource audioSource;
    public WindowTrigger trigger;

    public int State { get => state; }
    public bool IsOpen { get => isOpen;  }

    private static float[] handleRotation = new float[] { 0f, -90f, -180f, -90f };
    private static float[] panelRotation = new float[] { 0f, -97.5f, -10.5f };
    private int state = -1;
    private bool isOpen;
    private IEnumerator ieRot;
    private IEnumerator ieRot2;
    private bool isEnabled = true;

    private void Awake()
    {
        SetState(0);
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[] {
            "IsEnabled",
            "IsOpen1",
            "IsOpen2"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
        SetDelegate("IsEnabled", SetEnabled);
    }

    public override void RegisterGoals()
    {
        Formula f = WorldDB.Get(Prefix + "IsEnabled");
        WorldDB.RegisterFormula(new Implication(null, f));

        Conjunction con = new Conjunction();
        con.AddFormula(f);
        con.AddFormula(WorldDB.Get(Prefix + "IsOpen1"));
        WorldDB.RegisterFormula(new Implication(con, null));

        con = new Conjunction();
        con.AddFormula(f);
        con.AddFormula(WorldDB.Get(Prefix + "IsOpen2"));
        WorldDB.RegisterFormula(new Implication(con, null));
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    private void SetEnabled(bool isEnabled)
    {
        if (this.isEnabled == isEnabled)
            return;

        this.isEnabled = isEnabled;
    }

    public void SetState(int state)
    {
        if (this.state == state || isOpen)
            return;

        if (null != ieRot)
        {
            StopCoroutine(ieRot);
            ieRot = null;
            handleObject.transform.localRotation = Quaternion.Euler(0f, handleRotation[this.state], 0f);
        }

        state = Mathf.Max(0, state);
        state %= handleRotation.Length;
        this.state = state;
        trigger.SetActive(state > 0);
        ieRot = IERot();
        StartCoroutine(ieRot);
    }

    private IEnumerator IERot()
    {
        Quaternion handleRot = Quaternion.Euler(0f, handleRotation[state], 0f);
        float f = 0f;

        while (f <= 1f)
        {
            handleObject.transform.localRotation = Quaternion.Lerp(handleObject.transform.localRotation, handleRot, f);
            yield return null;
            f += Time.deltaTime;
        }

        ieRot = null;
        handleObject.transform.localRotation = handleRot;
    }

    public override bool Interact(Interactable interactable)
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsCurrentAction(typeof(LookAction)))
        {
            ShowDescription();
            actionController.UnsetActionState(this);
            return true;
        }

        if (!actionController.IsCurrentAction(typeof(UseAction)))
            return false;

        if (isOpen)
            return false;

        AudioManager.GetInstance().PlaySound("window.handle", gameObject, 1f, audioSource);
        SetState(state + 1);
        return true;
    }

    public void Open(bool isOpen)
    {
        this.isOpen = isOpen;
        Fire("IsOpen" + state, isOpen);
        AudioManager.GetInstance().PlaySound("window." + (isOpen ? "open" : "close"), gameObject, 1f, audioSource);
        SetRotation();
    }

    private void SetRotation(bool instant = false)
    {
        Quaternion panelRot = Quaternion.Euler(Vector3.zero);

        if (state == 2)
        {
            float xRot =  isOpen ? -panelRotation[2] : 0f;
            panelRot = Quaternion.Euler(xRot, 0f, 0f);
        }
        else
        {
            float zRot = isOpen ? -panelRotation[1] : 0f;
            panelRot = Quaternion.Euler(0f, 0f, zRot);
        }

        if (instant)
        {
            panel.transform.localRotation = panelRot;
        }
        else
        {
            if (null != ieRot2)
                StopCoroutine(ieRot2);

            ieRot2 = IERot2(panelRot);
            StartCoroutine(ieRot2);
        }
    }

    private IEnumerator IERot2(Quaternion rot)
    {
        float f = 0f;

        while (f <= 1f)
        {
            panel.transform.localRotation = Quaternion.Lerp(panel.transform.localRotation, rot, f);

            yield return null;

            f += Time.deltaTime;
        }

        panel.transform.localRotation = rot;
        ieRot2 = null;
    }

    public override int IsInteractionEnabled()
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsCurrentAction(typeof(LookAction)))
            return base.IsInteractionEnabled();

        if (!ShouldBeEnabled()
            || !actionController.IsCurrentAction(typeof(UseAction)))
            return -1;

        if (actionController.IsCurrentAction(typeof(UseAction)) && isOpen)
            return -1;

        return 1;
    }

    protected override bool ShouldBeEnabled()
    {
        return isEnabled;
    }
}