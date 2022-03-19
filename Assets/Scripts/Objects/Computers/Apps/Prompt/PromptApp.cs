using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Language;

public class PromptApp : PCApp
{
    private struct CMD
    {
        public readonly string[] Patterns;
        public readonly string[] Args;
        public readonly string Description;
        public readonly Action<string> Func;

        public CMD(string[] patterns, string[] args, string description, Action<string> func)
        {
            Patterns = patterns;
            Args = args;
            Description = description;
            Func = func;
        }
    }
    private CMD[] cmds;

    public ScrollRect scrollRect;
    public RectTransform textParent;
    public TextMeshProUGUI outputPrefab;
    public TMP_InputField inputLine;

    private List<string> history;
    private int histPos;

    private void Awake()
    {
        Init();
    }

    public override void ResetApp()
    {
        history?.Clear();
        history = null;
        ClearConsole(false);
    }

    protected override void PreCall()
    {
        history = new List<string>();
        histPos = -1;
        ClearConsole();
        textParent.offsetMin = Vector2.zero;
    }

    private void ClearConsole(string arg = "")
    {
        ClearConsole(true);
    }

    private void ClearConsole(bool focus)
    {
        foreach (Transform trans in textParent)
        {
            if (trans != inputLine.transform)
                Destroy(trans.gameObject);
        }

        ClearInputLine();

        if (focus)
            FocusInput();
    }

    private void FocusInput(bool autoscroll = true)
    {
        inputLine.transform.SetAsLastSibling();
        inputLine.Select();
        inputLine.ActivateInputField();
        Canvas.ForceUpdateCanvases();
        scrollRect.content.GetComponent<VerticalLayoutGroup>().
            CalculateLayoutInputVertical();
        scrollRect.content.GetComponent<ContentSizeFitter>().
            SetLayoutVertical();

        if (autoscroll)
            scrollRect.normalizedPosition = Vector2.zero;
    }

    private void ShowHistory(int d)
    {
        ClearInputLine();

        if (history.Count == 0)
            return;

        histPos += d;

        if (histPos == history.Count)
        {
            histPos = -1;
            return;
        }

        histPos += history.Count;
        histPos %= history.Count;
        inputLine.text = history[histPos];
    }

    private void SetInput(string text)
    {
        WriteConsole(text, "|> ");
    }

    private void SetError(string text)
    {
        WriteConsole(text, "", "#ff0000");
    }

    private void SetOutput(string text)
    {
        WriteConsole(text, "");
    }

    private void WriteConsole(string text, string prefix, string color = "#ffffff")
    {
        TextMeshProUGUI textMesh = Instantiate(outputPrefab) as TextMeshProUGUI;
        textMesh.autoSizeTextContainer = true;
        textMesh.name = "Line " + textParent.childCount;
        textMesh.text = "<color=" + color + ">" + prefix + text + "</color>";
        textMesh.transform.SetParent(textParent, false);
    }

    private void ClearInputLine()
    {
        inputLine.text = "";
    }

    private void ShowHelp(string arg = "")
    {
        CMD[] cmds = GetCommands();
        string text = "";

        foreach (CMD cmd in cmds)
        {
            int i = 0;
            text += "<b>" + cmd.Patterns[i] + "</b>";
            i++;

            while (i < cmd.Patterns.Length)
            {
                text += ", <b>" + cmd.Patterns[i] + "</b>";
                i++;
            }
            
            text += "     " + cmd.Description + "<br>";
        }

        SetOutput(text);
    }

    private CMD[] GetCommands()
    {
        if (null == cmds)
        {
            cmds = new CMD[]
            {
                new CMD (
                    new string[] { "HELP" },
                    null,
                    "",
                    ShowHelp
                ),
                new CMD (
                    new string[] { "CLEAR", "CLS" },
                    null,
                    "",
                    ClearConsole
                ),
                new CMD (
                    new string[] { "BEEP" },
                    null,
                    "",
                    computer.Beep
                ),
                new CMD (
                    new string[] { "SHUTDOWN" },
                    null,
                    "",
                    computer.Shutdown
                ),
                new CMD (
                    new string[] { "REBOOT" },
                    null,
                    "",
                    computer.Reboot
                ),
                new CMD (
                    new string[] { "EXIT" },
                    null,
                    "",
                    Close
                ),
                new CMD (
                    new string[] { "TASKLIST" },
                    null,
                    "",
                    ListTasks
                ),
                new CMD (
                    new string[] { "KILL", "TASKKILL" },
                    new string[] { "PID" },
                    "",
                    computer.KillProcess
                ),
            };
        }

        return cmds;
    }

    private void HandleInput(string text)
    {
        if (histPos == -1)
            history.Add(text);

        histPos = -1;

        try
        {
            PromptCommand pcmd = PromptInutParser.Parse(text);

            if ("KILL".Equals(pcmd.code) || "TASKKILL".Equals(pcmd.code))
            {
                if (pcmd.NumArgs == 0 || !computer.Kill(pcmd.args[0], false))
                    SetError(LanguageManager.GetText(LangKey.Error)
                        + ": \"" + pcmd.code + " <i>" + "PID</i>\"");
            }
            else
            {
                CMD[] cmds = GetCommands();

                foreach (CMD cmd in cmds)
                {
                    foreach (string pattern in cmd.Patterns)
                    {
                        if (pattern.Equals(pcmd.code))
                        {
                            string arg = "";

                            if (null != pcmd.args && pcmd.args.Length > 0)
                                arg = pcmd.args[0];

                            cmd.Func.Invoke(arg);
                            return;
                        }
                    }
                }

                SetError(LanguageManager.GetText(LangKey.UnknownCommand)
                    + ": \"" + pcmd.code + "\"");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetError(LanguageManager.GetText(LangKey.UnknownCommand));
        }
    }

    private void DoInput()
    {
        string text = inputLine.text.Trim();
        ClearInputLine();

        if (!"".Equals(text))
        {
            SetInput(text);
            HandleInput(text);
        }

        FocusInput();
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
            "PromptApp.IsEnabled"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        Dictionary<string, Action<bool>> dict = new Dictionary<string, Action<bool>>();
        dict.Add("PromptApp.IsEnabled", SetEnabled);
        return dict;
    }

    public override List<Formula> GetGoals()
    {
        List<Formula> list = new List<Formula>();
        list.Add(new Implication(null, WorldDB.Get("PromptApp.IsEnabled")));

        // TODO secret code

        /*
        Conjunction con = new Conjunction();
        con.AddFormula(WorldDB.Get("CalcApp.IsEnabled"));
        con.AddFormula(WorldDB.Get("CalcApp.HasError"));
        list.Add(new Implication(con, null));
        */
        return list;
    }

    protected override void Effect()
    {
        
    }

    private void ListTasks(string arg = "")
    {
        List<PCAppInfo> list = computer.GetAppInfos();
        string text = " PID    NAME<br>================================<br>";

        for (int i = 0; i < list.Count; i++)
        {
            if (null != list[i])
                text += StringUtil.Fill(list[i].Id.ToString(), 4) + " | " + list[i].GetName() + "<br>";
        }

        WriteConsole(text, "");
    }

    private void Update()
    {
        if (!IsActive)
            return;

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            DoInput();

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ShowHistory(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            ShowHistory(-1);

        if (!inputLine.isFocused)
            FocusInput(false);

    }
}