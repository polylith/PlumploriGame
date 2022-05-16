using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskMgrApp : PCApp
{
    public RectTransform list;
    public UITextButton killButton;
    public TextMeshProUGUI status;

    private PCAppInfo current;
    private IEnumerator ieUpdate;

    public override void StoreCurrentState(EntityData entityData)
    {

        base.StoreCurrentState(entityData);  
    }

    public override void RestoreCurrentState(EntityData entityData)
    {

        base.RestoreCurrentState(entityData);
    }

    private void Awake()
    {
        Init();
    }

    protected override void Effect()
    {
        if (!isInfected || !IsActive)
            return;

        try
        {
            int n = UnityEngine.Random.Range(0, list.childCount);
            Transform child = list.GetChild(n);
            PCAppInfo appInfo = child.GetComponent<PCAppInfo>();

            if (null != appInfo)
            {
                if (UnityEngine.Random.value > 0.75f
                    || appInfo.Id > 25 && appInfo.Id < 55)
                {
                    appInfo.Click();
                    GameEvent.GetInstance().Execute(Kill, 1f);
                }
                else
                {
                    appInfo.SetState(UnityEngine.Random.Range(0, 3));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        GameEvent.GetInstance().Execute(Effect, UnityEngine.Random.Range(5f, 10f));
    }

    protected override void Init()
    {
        ClearList();
        ClearStatus();
        SetCurrent(null);
        killButton.SetAction(Kill);
        base.Init();
    }

    public override void SetInfected(bool isInfected)
    {
        this.isInfected = isInfected && UnityEngine.Random.value >= 0.5f;

        if (this.isInfected)
        {
            GameEvent.GetInstance().Execute(Effect, UnityEngine.Random.Range(5f, 10f));
        }
    }

    public override void ResetApp()
    {
        computer.OnAppStart -= AddAppInfo;
        ClearStatus();
    }

    protected override void PreCall()
    {
        killButton.SetText(Language.LanguageManager.GetText(Language.LangKey.KillProcess));
        computer.OnAppStart += AddAppInfo;
        UpdateList();
    }

    public void AddAppInfo(PCAppInfo appInfo)
    {
        if (null == appInfo)
            return;

        appInfo.transform.SetParent(list, false);

        if (null == appInfo.onKill)
        {
            appInfo.Highlight();
            appInfo.onKill = SetCurrent;
        }        
    }

    private void UpdateList()
    {
        if (!IsActive)
            return;

        Dictionary<int, PCAppInfo> infos = computer.Infos;

        if (null != infos && infos.Count > 0)
        {
            float sum = 0f;
            List<PCAppInfo> appList = new List<PCAppInfo>();
            int count = 0;

            foreach (int id in infos.Keys)
            {
                PCAppInfo appInfo = infos[id];

                if (null != appInfo)
                {
                    if (null == appInfo.onKill)
                        AddAppInfo(appInfo);

                    appList.Add(appInfo);
                    float value = appInfo.NextValue();
                    sum += value;
                    count += value > 0f ? 1 : 0;
                }
            }

            float total = sum + UnityEngine.Random.Range(0, 100);

            foreach (PCAppInfo appInfo in appList)
            {
                if (null != appInfo)
                {
                    float value = (sum > 0f ? appInfo.MemValue / total : 0f) * 100f;
                    appInfo.SetMemValue(value);
                    appInfo.UpdateValues();
                }
            }

            ShowStatus(sum, total, count, appList.Count);

            if (sum > 0f)
            {
                appList.Sort((PCAppInfo info1, PCAppInfo info2) =>
                {
                    float v1 = null != info1 ? info1.MemValue : 0f;
                    float v2 = null != info2 ? info2.MemValue : 0f;
                    return v1 > v2 ? -1 : 1;
                });

                foreach (PCAppInfo info in appList)
                    info?.transform.SetAsLastSibling();
            }
        }
        else
            ClearStatus();

        ieUpdate = IEUpdate();
        StartCoroutine(ieUpdate);
    }

    private void ClearStatus()
    {
        status.SetText("");
    }

    private void ShowStatus(float p, float sum, int active, int count)
    {
        int value = Mathf.RoundToInt((sum > 0f ? p / sum : 0f) * 100f);
        string color = value > 80 ? (value > 90 ? "#ff0000" : "#ff8800" ): "#000000";
        string sactive = active.ToString();
        string svalue = value.ToString();

        if (isInfected && UnityEngine.Random.value > .35f)
        {
            sactive = StringUtil.ToLeet(sactive);
            svalue = StringUtil.ToLeet(svalue);
        }

        string text = "<color=#000000>Active " + sactive + " / " + count + "</color><br><color=" + color + ">CPU " + svalue + " %</color>";
        status.SetText(text);
    }

    private void ClearList()
    {
        foreach (Transform trans in list)
            trans.SetParent(null);
    }

    private IEnumerator IEUpdate()
    {
        yield return new WaitForSecondsRealtime(2f);

        ieUpdate = null;
        UpdateList();
    }

    private void SetCurrent(PCAppInfo appInfo = null)
    {
        bool b = null != appInfo;
        killButton.IsEnabled = b;
        killButton.SetState(b ? 1 : 0);

        if (current == appInfo)
            return;

        if (null != current)
            current.SetState(0);

        current = appInfo;
    }

    private void Kill()
    {
        current?.Kill();
        SetCurrent(null);
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
            "TaskMgrApp.IsEnabled"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        Dictionary<string, Action<bool>> dict = new Dictionary<string, Action<bool>>();
        dict.Add("TaskMgrApp.IsEnabled", SetEnabled);
        return dict;
    }

    public override List<Formula> GetGoals()
    {
        List<Formula> list = new List<Formula>();
        list.Add(new Implication(null, WorldDB.Get("TaskMgrApp.IsEnabled")));
        return list;
    }

}