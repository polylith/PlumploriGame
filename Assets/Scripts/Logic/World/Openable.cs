using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action;

/// <summary>
/// This class is for objects that can be opened and closed,
/// but do not require a key for interaction like lockables.
/// </summary>
public class Openable : Interactable
{
    public bool HasContent { get => content.Count > 0; }
    public List<Collectable> Content { get => content; }

    public string soundToken = "cabinette";
    public float soundDelay = 1f;
    public bool rotate = true;
    public Vector3 v = new Vector3(0f, 100f, 0f);
    public Transform panel;
    public AudioSource audioSource;

    public bool IsOpen { get => isOpen; }

    protected bool isOpen = false;
    private IEnumerator ieAnim;
    private float speed = 0.1f;
    private readonly List<Collectable> content = new List<Collectable>();

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
                "IsOpen"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
        SetDelegate("IsOpen", SetOpen);
    }

    public override void RegisterGoals()
    {
        Formula f = WorldDB.Get(Prefix + "IsOpen");
        WorldDB.RegisterFormula(new Implication(f, null));
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override int IsInteractionEnabled()
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsCurrentAction(typeof(UseAction)))
            return -1;

        if (!actionController.IsCurrentAction(typeof(OpenAction)))
            return base.IsInteractionEnabled();

        if (null != interactableUIPrefab && null == InteractableUI)
        {
            InitInteractableUI(true);
        }

        return 1;
    }

    public override bool Interact(Interactable interactable)
    {
        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(OpenAction)))
            return base.Interact(interactable);

        Toggle();
        return true;
    }

    private void Toggle()
    {
        SetOpen(!isOpen, false);
    }

    public void SetOpen(bool isOpen)
    {
        SetOpen(isOpen, true);
    }

    public virtual void SetOpen(bool isOpen, bool instant = false)
    {
        if (this.isOpen == isOpen)
            return;

        this.isOpen = isOpen;
        Fire("IsOpen", isOpen);

        if (null != InteractableUI && !instant)
        {
            if (IsOpen)
            {
                InteractableUI.SetInteractable(this);
                InteractableUI.Show();
            }
            else
            {
                InteractableUI.Hide();
            }
        }

        StopAnimate();

        if (rotate)
        {
            Quaternion rot = Quaternion.Euler(isOpen ? v : Vector3.zero);

            if (instant)
                panel.transform.localRotation = rot;
            else
                Animate(rot);
        }
        else
        {
            Vector3 position = panel.transform.localPosition + v * (isOpen ? 1 : -1f);

            if (instant)
                panel.transform.localPosition = position;
            else
                Animate(position);
        }
    }

    private void StopAnimate()
    {
        if (null != ieAnim)
            StopCoroutine(ieAnim);

        ieAnim = null;
    }

    private void Animate(Vector3 position)
    {
        ieAnim = IEAnimate(position);
        StartCoroutine(ieAnim);
    }

    private void Animate(Quaternion rot)
    {
        ieAnim = IEAnimate(rot);
        StartCoroutine(ieAnim);
    }

    private IEnumerator IEAnimate(Vector3 position)
    {
        if (soundDelay > 0f)
            yield return new WaitForSecondsRealtime(soundDelay);

        AudioManager.GetInstance().PlaySound(soundToken + "." + (isOpen ? "open" : "close"), gameObject, 1f, audioSource);

        float f = 0f;

        while (f <= 1f)
        {
            panel.transform.localPosition = Vector3.Lerp(panel.transform.localPosition, position, f);
            yield return null;
            f += speed;
        }

        panel.transform.localPosition = position;
        ieAnim = null;
    }

    private IEnumerator IEAnimate(Quaternion rot)
    {
        AudioManager.GetInstance().PlaySound(soundToken + "." + (isOpen ? "open" : "close"), gameObject, 1f, audioSource);

        if (soundDelay > 0f)
            yield return new WaitForSecondsRealtime(soundDelay);

        float f = 0f;

        while (f <= 1f)
        {
            panel.transform.localRotation = Quaternion.Lerp(panel.transform.localRotation, rot, f);
            yield return null;
            f += speed;
        }

        panel.transform.localRotation = rot;
        ieAnim = null;
    }
}