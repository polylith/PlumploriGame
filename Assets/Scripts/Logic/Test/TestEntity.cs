using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TestEntity : Entity, IPointerClickHandler
{
    public Image image;
    public TextMeshProUGUI textMesh;

    private List<string> attributes;
    private Dictionary<string, System.Action<bool>> delegateMap;
    private RectTransform rectTransform;

    private bool isEnabled = true;
    private bool isRotating = true;
    private bool isMovingX = true;
    private bool isMovingY = true;
    private float zRot;
    private Vector3 move;
    private float acceleration;

    public override void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        SetEnabled(false);
        SetRotating(false);
        SetMovingX(false);
        SetMovingY(false);
        InitAttributes();
        SwitchState();
    }

    private void InitAttributes()
    {
        attributes = new List<string>();
        attributes.Add("IsEnabled");
        attributes.Add("IsRotating");
        attributes.Add("IsMovingX");
        attributes.Add("IsMovingY");

        delegateMap = new Dictionary<string, System.Action<bool>>();

        if (Random.value > Random.value)
            delegateMap.Add("IsEnabled", SetEnabled);

        delegateMap.Add("IsRotating", SetRotating);
        delegateMap.Add("IsMovingX", SetMovingX);
        delegateMap.Add("IsMovingY", SetMovingY);
    }

    public void ResetEnabled()
    {
        if (delegateMap.ContainsKey("IsEnabled"))
            delegateMap.Remove("IsEnabled");
    }

    public override Bounds GetBounds()
    {
        return new Bounds(Vector3.zero, new Vector3(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y, 1f));
    }

    public override List<string> GetAttributes()
    {
        return attributes;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    protected override void RegisterAtoms()
    {
        textMesh.SetText(Prefix);
        RegisterAtoms(attributes);

        foreach (string key in delegateMap.Keys)
            SetDelegate(key, delegateMap[key]);
    }

    public override void RegisterCurrentState()
    {
        if (isEnabled || HasDelegate("IsEnabled"))
            RegisterCurrentState("IsEnabled", isEnabled);

        RegisterCurrentState("IsRotating", isRotating);
        RegisterCurrentState("IsMovingX", isMovingX);
        RegisterCurrentState("IsMovingY", isMovingY);
    }

    public override void RegisterGoals()
    {
        bool b = Random.value > Random.value;
        WorldDB.RegisterGoal(Prefix, "IsEnabled", b);
        SetEnabled(!b);

        bool c = b && Random.value > Random.value;
        WorldDB.RegisterGoal(Prefix, "IsRotating", c);
        SetRotating(b && !c);

        c = b && Random.value > Random.value;
        WorldDB.RegisterGoal(Prefix, "IsMovingX", c);
        SetMovingX(b && !c);

        c = b && Random.value > Random.value;
        WorldDB.RegisterGoal(Prefix, "IsMovingY", c);
        SetMovingY(b && !c);

        if (isEnabled)
        {
            Formula f = new Negation(WorldDB.Get(Prefix + "IsEnabled"));

            if (delegateMap.ContainsKey("IsEnabled"))
            {
                WorldDB.RegisterFormula(new Implication(null, f));
            }

            Formula Q1 = new Negation(WorldDB.Get(Prefix + "IsRotating"));
            Formula Q2 = new Negation(WorldDB.Get(Prefix + "IsMovingX"));
            Formula Q3 = new Negation(WorldDB.Get(Prefix + "IsMovingY"));

            Conjunction P2 = new Conjunction();
            P2.AddFormula(f);
            P2.AddFormula(Q1);

            Conjunction P3 = new Conjunction();
            P3.AddFormula(f);
            P3.AddFormula(Q1);
            P3.AddFormula(Q2);

            WorldDB.RegisterFormula(new Implication(f, Q1));
            WorldDB.RegisterFormula(new Implication(P2, Q2));
            WorldDB.RegisterFormula(new Implication(P3, Q3));
        }
        else
        {
            Formula f = WorldDB.Get(Prefix + "IsEnabled");

            if (delegateMap.ContainsKey("IsEnabled"))
            {
                WorldDB.RegisterFormula(new Implication(null, f));
            }

            Formula Q = WorldDB.Get(Prefix + "IsRotating");

            if (isRotating)
                Q = new Negation(Q);

            WorldDB.RegisterFormula(new Implication(f, Q));

            Q = WorldDB.Get(Prefix + "IsMovingX");

            if (isMovingX)
                Q = new Negation(Q);

            WorldDB.RegisterFormula(new Implication(f, Q));

            Q = WorldDB.Get(Prefix + "IsMovingY");

            if (isMovingY)
                Q = new Negation(Q);

            WorldDB.RegisterFormula(new Implication(f, Q));
        }
    }

    private void SwitchState()
    {
        int colorIndex = (isEnabled ? 0 : -1);

        if (colorIndex > -1)
        {
            if (isRotating)
                colorIndex += 1;

            if (isMovingX)
                colorIndex += 2;

            if (isMovingY)
                colorIndex += 4;
        }

        Color color = (colorIndex > -1 ? CATex.allColors[colorIndex] : Color.black);
        image.color = color;
    }

    public void SetEnabled(bool isEnabled)
    {
        Debug.Log(transform.name + " is enabled " + this.isEnabled + " -> " + isEnabled);

        this.isEnabled = isEnabled;
        SwitchState();
    }

    public void SetRotating(bool isRotating)
    {
        Debug.Log(transform.name + " rotating " + this.isRotating + " -> " + isRotating);

        this.isRotating = isRotating;

        if (isRotating)
            zRot = Random.Range(1f, 10f);
        else
            zRot = 0f;

        SwitchState();
    }

    public void SetMovingX(bool isMovingX)
    {
        Debug.Log(transform.name + " moving X " + this.isMovingX + " -> " + isMovingX);

        this.isMovingX = isMovingX;

        if (isMovingX)
        {
            move = new Vector3(Random.Range(0.15f, 1f) * (Random.value > 0.5f ? 1f : -1f), move.y, 0f);
            acceleration = Random.value + 0.5f;
        }
        else
        {
            move = new Vector3(0f, move.y, 0f);
        }

        SwitchState();
    }

    public void SetMovingY(bool isMovingY)
    {
        Debug.Log(transform.name + " moving Y " + this.isMovingY + " -> " + isMovingY);

        this.isMovingY = isMovingY;

        if (isMovingY)
        {
            move = new Vector3(move.x, Random.Range(0.15f, 1f) * (Random.value > 0.5f ? 1f : -1f), 0f);
            acceleration = Random.value + 0.5f;
        }
        else
        {
            move = new Vector3(move.x, 0f, 0f);
        }

        SwitchState();
    }

    private void Update()
    {
        if (!isEnabled)
            return;

        if (isRotating)
            image.rectTransform.Rotate(new Vector3(0f, 0f, zRot * acceleration));

        if (isMovingX || isMovingY)
        {
            Vector3 pos = rectTransform.position + move * acceleration;

            if (pos.x < 0f || pos.x > Screen.width)
            {
                move.x *= -1f;
                acceleration = 1f / acceleration;
                pos = rectTransform.position + move * acceleration;
            }

            if (pos.y < 0f || pos.y > Screen.height)
            {
                move.y *= -1f;
                acceleration = 1f / acceleration;
                pos = rectTransform.position + move * acceleration;
            }

            rectTransform.position = pos;
        }
    }

    private void SwitchStates()
    {
        if (!delegateMap.ContainsKey("IsEnabled"))
        {
            SetEnabled(!isEnabled);
            Fire("IsEnabled", isEnabled);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SwitchStates();
    }
}