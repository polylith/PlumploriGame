using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Language;

public class CalcApp : PCApp
{
    private static string digits = "0123456789.,/+*X-=CB";
    public TextMeshProUGUI resultDisplay;

    private string sInput = "";
    private float result = 0f;
    private float param = 0f;
    private System.Action<float, float> op;
    private System.Action<float, float> lastOp;

    public override void SetInfected(bool isInfected)
    {
        base.SetInfected(isInfected);

        digits = "0123456789.,/+*X-=CB"
            + (isInfected ? "AbcdEfGhIjKlmNoPqRsTuVwXyZ?!§$%&@" : "");
    }

    protected override void Effect()
    {
        if (!isInfected || !IsActive)
            return;

        int n = Random.Range(1, 5);

        while (n > 0)
        {
            int zz = Random.Range(0, 100) % digits.Length;
            DoInput(digits.Substring(zz, 1));
            n--;
        }

        GameEvent.GetInstance().Execute(Effect, Random.Range(5f, 10f));
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
            "CalcApp.IsEnabled",
            "CalcApp.HasError"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    public override Dictionary<string, System.Action<bool>> GetDelegates()
    {
        Dictionary<string, System.Action<bool>> dict = new Dictionary<string, System.Action<bool>>();
        dict.Add("CalcApp.IsEnabled", SetEnabled);
        return dict;
    }

    public override List<Formula> GetGoals()
    {
        List<Formula> list = new List<Formula>();
        list.Add(new Implication(null, WorldDB.Get("CalcApp.IsEnabled")));

        Conjunction con = new Conjunction();
        con.AddFormula(WorldDB.Get("CalcApp.IsEnabled"));
        con.AddFormula(WorldDB.Get("CalcApp.HasError"));
        list.Add(new Implication(con, null));
        return list;
    }

    public override void ResetApp()
    {
        ClearResult();
    }

    private void ClearResult()
    {
        Clear();
        Show(result);
    }

    private void Clear()
    {
        sInput = "0";
        result = 0f;
        op = null;
        lastOp = null;
    }

    private void ShowError()
    {
        AudioManager.GetInstance().PlaySound("buzzer", computer.gameObject);
        string text = "<color=#ff0000>" + LanguageManager.GetText(LangKey.Error) + "</color>";
        ShowText(text);
        computer?.AppFire("HasError", true);
    }

    private void Show(float f)
    {
        string color = "#000000";
        sInput = LanguageManager.ToLangString(f);
        string text = sInput;

        if (float.IsNaN(f) || float.IsNegativeInfinity(f) || float.IsPositiveInfinity(f))
        {
            Clear();
            color = "#0000ff";
        }

        text = "<color=" + color + ">" + text + "</color>";
        ShowText(text);
    }

    private void ShowText(string text)
    {
        resultDisplay.SetText(text);
    }

    public void Input9()
    {
        DoInput("9");
    }

    public void Input8()
    {
        DoInput("8");
    }

    public void Input7()
    {
        DoInput("7");
    }

    public void Input6()
    {
        DoInput("6");
    }

    public void Input5()
    {
        DoInput("5");
    }

    public void Input4()
    {
        DoInput("4");
    }

    public void Input3()
    {
        DoInput("3");
    }

    public void Input2()
    {
        DoInput("2");
    }

    public void Input1()
    {
        DoInput("1");
    }

    public void Input0()
    {
        DoInput("0");
    }

    public void InputDP()
    {
        DoInput(".");
    }

    public void InputDiv()
    {
        DoInput("/");
    }

    public void InputMul()
    {
        DoInput("X");
    }

    public void InputAdd()
    {
        DoInput("+");
    }

    public void InputSub()
    {
        DoInput("-");
    }

    public void InputC()
    {
        DoInput("C");
    }

    public void InputRes()
    {
        DoInput("=");
    }

    public void InputRec()
    {
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        bool error = false;

        if (null == op)
        {
            param = GetInput();
            error = param == 0f;

            if (!error)
            {
                param = 1f / param;
                Show(param);
            }
        }
        else
        {
            error = result == 0f;

            if (!error)
            {
                result = 1f / result;
                Show(result);
            }
        }

        if (!error)
            return;

        Clear();
        ShowError();
    }

    public void InputInv()
    {
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);

        if (null == op)
        {
            param = GetInput() * -1f;
            Show(param);
        }
        else
        {
            result *= -1f;
            Show(result);
        }
    }

    public void InputBack()
    {
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        sInput = sInput.Length > 0 ? sInput.Substring(0, sInput.Length - 1) : "";

        if ("".Equals(sInput))
            sInput = "0";

        string text = "<color=#000000>" + sInput + "</color>";
        ShowText(text);
    }

    private void DoInput(string s)
    {
        if (null == s || !digits.Contains(s))
            return;

        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);

        if ("C".Equals(s))
            ResetApp();
        else if ("B".Equals(s))
            InputBack();
        else if ("X".Equals(s) || "*".Equals(s))
        {
            SetOp(Mul, "*");
        }
        else if ("+".Equals(s))
        {
            SetOp(Add, "+");
        }
        else if ("/".Equals(s))
        {
            SetOp(Div, "/");
        }
        else if ("-".Equals(s))
        {
            SetOp(Sub, "-");
        }
        else if ("=".Equals(s))
        {
            DoOp();
        }
        else
        {
            AddInput(s);
        }
    }

    private float GetInput()
    {
        try
        {
            float f = float.Parse(sInput);
            return f;
        }
        catch (System.Exception)
        {
            return 0f;
        }
    }

    private void SetOp(System.Action<float, float> op, string s)
    {
        if (null != this.op && !DoOp())
            return;

        result = GetInput();
        this.op = op;
        string text = "<color=#000000>" + sInput + " " + s + " </color>";
        ShowText(text);
        sInput = "0";
    }

    private void Mul(float f1, float f2)
    {
        result = f1 * f2;
    }

    private void Div(float f1, float f2)
    {
        if (f2 == 0f)
            throw new System.Exception();

        result = f1 / f2;
    }

    private void Add(float f1, float f2)
    {
        result = f1 + f2;
    }

    private void Sub(float f1, float f2)
    {
        result = f1 - f2;
    }

    private bool DoOp()
    {
        try
        {
            if (null != op)
            {
                param = GetInput();
                op.Invoke(result, param);
                Show(result);
                lastOp = op;
                op = null;
            }
            else if (null != lastOp)
            {
                lastOp.Invoke(result, param);
                Show(result);
            }

            return true;
        }
        catch (System.Exception)
        {
            ClearResult();
            ShowError();
        }

        return false;
    }

    private void AddInput(string s)
    {
        if (null == s || !digits.Contains(s))
            return;

        if (",".Equals(s))
            s = ".";

        if (".".Equals(s))
        {
            if ("".Equals(sInput))
                sInput = "0";
            else if (sInput.Contains("."))
                return;            
        }

        if ("0".Equals(sInput) && ("0".Equals(s) || !".".Equals(s)))
            sInput = "";

        sInput += s;
        string text = "<color=#000000>" + sInput + "</color>";
        ShowText(text);
    }

    private void Update()
    {
        if (!IsActive)
            return;

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            InputRes();
        else if (Input.GetKeyDown(KeyCode.Delete))
            ClearResult();
        else if (Input.GetKeyDown(KeyCode.Backspace))
            InputBack();
    }

    private void OnGUI()
    {
        if (!IsActive)
            return;

        Event e = Event.current;

        if (null != e && e.isKey)
            DoInput(e.character.ToString().ToUpper());
    }
}