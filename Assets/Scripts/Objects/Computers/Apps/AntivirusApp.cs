using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Language;

public class AntivirusApp : PCApp
{
    private static int scancount;

    public UITextButton scanButton;
    public UITextButton cleanButton;
    public Image reportBg;
    public ScrollRect scrollRect;
    public TextMeshProUGUI report;
    public Transform statusPanel;
    public Image statusbar1;
    public Image statusbar2;
    public TextMeshProUGUI statusText;

    private bool isScanning;
    private List<string> cleanList;
    private Dictionary<string, List<PCAppInfo>> scanResult;
    private int counted;
    private IEnumerator ieScan;

    private bool isCleaning;
    private IEnumerator ieClean;

    private void Awake()
    {
        Init();
    }

    protected override void Effect()
    {
        // no virus effects here!!!
    }

    protected override void Init()
    {
        ShowStatus(false);
        scanResult = new Dictionary<string, List<PCAppInfo>>();
        isScanning = false;
        ClearReport();
        cleanButton.SetAction(Clean);
        scanButton.SetAction(Scan);
        base.Init();
    }

    public override void SetInfected(bool isInfected)
    {
        // never infect this app!!!
        this.isInfected = false;
    }

    private void ShowStatus(bool visible)
    {
        statusText.SetText("");
        statusPanel.gameObject.SetActive(visible);
    }

    private void SetStatusText(string s, string color = "#000000")
    {
        statusText.SetText("<color=" + color + ">" + s + "</color>");
    }

    public override void ResetApp()
    {
        StopScan();
        StopClean();
        scanResult.Clear();
        ClearReport();
        base.ResetApp();
    }

    protected override void PreCall()
    {
        scanButton.SetText(
            LanguageManager.GetText(
                LangKey.Scan,
                LanguageManager.GetText(LangKey.PC)
            )
        );
        cleanButton.SetText(
            LanguageManager.GetText(
                LangKey.Clean,
                LanguageManager.GetText(LangKey.PC)
            )
        );
    }

    private void Scan()
    {
        if (isScanning)
            return;

        scancount++;
        ClearReport();
        scanResult.Clear();
        cleanList?.Clear();
        isScanning = true;
        scanButton.Block(true);
        cleanButton.SetEnabled(false);
        counted = 0;

        foreach (PCAppInfo appInfo in computer.GetAppInfos())
        {
            counted++;

            if (null != appInfo && appInfo.IsVirus)
            {
                if (!scanResult.ContainsKey(appInfo.AppName))
                    scanResult.Add(appInfo.AppName, new List<PCAppInfo>());

                scanResult[appInfo.AppName].Add(appInfo);
            }
        }

        ieScan = IEScan();
        StartCoroutine(ieScan);
    }

    private IEnumerator IEScan()
    {
        statusbar1.color = Color.yellow;
        statusbar2.color = Color.cyan;
        statusbar1.fillAmount = 0f;
        statusbar2.fillAmount = 0f;
        ShowStatus(true);
        AudioManager.GetInstance().PlaySound("pc.noise", computer.gameObject);
        float f1 = 0f;

        SetStatusText("Scanning: 0 %");

        while (f1 <= 1f)
        {
            float zz = UnityEngine.Random.value * 2f + 0.5f;

            yield return new WaitForSecondsRealtime(zz);

            AudioManager.GetInstance().Restart("pc.noise", computer.gameObject, 1f + 0.2f * UnityEngine.Random.value);
            float f2 = 0f;

            while (f2 <= 1f)
            {
                statusbar2.fillAmount = f2;

                yield return null;

                f2 += Time.deltaTime * UnityEngine.Random.Range(1f, 5f);
            }

            statusbar2.fillAmount = 1f;
            statusbar1.fillAmount = f1;

            float percentValue = Mathf.Min(100, Mathf.RoundToInt(f1 * 100f));
            f1 += Time.deltaTime * UnityEngine.Random.Range(1f, 5f);
            SetStatusText("Scanning: " + percentValue + " %");
        }

        statusbar1.fillAmount = 1f;

        yield return new WaitForSecondsRealtime(1f);

        StopScan();
    }

    private void OnDisable()
    {
        StopScan();
        StopClean();
    }

    private void StopScan()
    {
        if (null != ieScan)
        {
            StopCoroutine(ieScan);
            ShowReport();
            scanButton.Block(false);
            AudioManager.GetInstance().StopSound("pc.noise", computer.gameObject);
        }

        ShowStatus(false);
        isScanning = false;
        ieScan = null;
    }

    private void ShowReport()
    {
        string text = "<color=#000000><u>" + LanguageManager.GetText(LangKey.ScanReport) + "</u><br><br>";

        if (null != cleanList && cleanList.Count > 0)
        {
            string list = "";

            for (int i = 0; i < cleanList.Count; i++)
            {
                int j = i + 1;
                string name = cleanList[i];
                list += " " + (j < 10 ? " " : "") + j + "| " + name + "<br>";
            }

            text += LanguageManager.GetText(LangKey.ObjectsRemoved, cleanList.Count.ToString()) + "<br>";
            text += list + "<br>";
        }

        if (scanResult.Count > 0)
        {
            text += LanguageManager.GetText(LangKey.ObjectsFound, scanResult.Count.ToString() + " / " + counted.ToString()) + "<br>";
            string list = "";
            int i = 1;
            int[] count = new int[4];

            foreach (string name in scanResult.Keys)
            {
                list += " " + (i < 10 ? " " : "" ) + i + "| " + name + "<br>";
                List<PCAppInfo> appList = scanResult[name];
                i++;

                foreach (PCAppInfo appInfo in appList)
                {
                    if (null != appInfo)
                    {
                        int index = 0;

                        if (appInfo.Id >= 10)
                            index++;

                        if (appInfo.Id >= 50)
                            index++;

                        count[index]++;
                        count[3]++;
                    }
                }
            }

            text += LanguageManager.GetText(LangKey.ScanResult, count[3].ToString()) + "<br>";
            text += LanguageManager.GetText(LangKey.System) + ": " + count[0].ToString() + "<br>";
            text += LanguageManager.GetText(LangKey.Malware) + ": " + count[1].ToString() + "<br>";
            text += LanguageManager.GetText(LangKey.Programs) + ": " + count[2].ToString() + "<br><br>";
            text += list;

            AudioManager.GetInstance().PlaySound("alarm", computer.gameObject);
        }
        else
        {
            text += "<i>" + LanguageManager.GetText(LangKey.ScanResult, "0") + "</i>";
            AudioManager.GetInstance().PlaySound("notify", computer.gameObject);
        }

        text += "<br><br><br></color>";
        report.SetText(text);
        reportBg.color = Color.white;
        cleanButton.SetEnabled(scanResult.Count > 0);

        if (cleanButton.IsEnabled)
            cleanButton.SetState(1);

        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    private void Clean()
    {
        if (scanResult.Count == 0 || isCleaning)
            return;

        isCleaning = true;
        ieClean = IEClean();
        cleanButton.Block(true);
        StartCoroutine(ieClean);
    }

    private IEnumerator IEClean()
    {
        statusbar1.color = Color.blue;
        statusbar1.fillAmount = 0f;
        ShowStatus(true);
        AudioManager.GetInstance().PlaySound("pc.noise", computer.gameObject);
        Queue<string> queue = new Queue<string>();
        cleanList = new List<string>();

        foreach (string name in scanResult.Keys)
            queue.Enqueue(name);

        float f = 0f;
        int i = 0;
        int n = queue.Count;
        SetStatusText("Status: " + i + " / " + n);

        while (queue.Count > 0)
        {
            string name = queue.Dequeue();
            List<PCAppInfo> appList = scanResult[name];

            foreach (PCAppInfo appInfo in appList)
            {
                SetStatusText("Status: " + i + " / " + n + " (" + appInfo.AppName + ")");
                
                float duration = 0.25f + UnityEngine.Random.value * 0.25f;

                if (null != appInfo)
                {
                    if (!cleanList.Contains(appInfo.AppName))
                        cleanList.Add(appInfo.AppName);

                    bool force = scancount > 1
                        || appInfo.Id <= 25
                        || UnityEngine.Random.value < 0.5f;
                    appInfo.Kill(force);

                    if (appInfo.HasApp)
                        duration += UnityEngine.Random.Range(0.25f, 0.75f);
                }

                statusbar2.color = Color.cyan;
                statusbar2.fillAmount = 0f;
                f = 0f;

                while (f <= 1f)
                {
                    statusbar2.fillAmount = f;
                    statusbar2.color = Color.Lerp(statusbar2.color, Color.green, f);

                    yield return null;

                    f += Time.deltaTime;
                    duration -= Time.deltaTime;
                }

                statusbar2.fillAmount = 1f;

                if (duration > 0f)
                    yield return new WaitForSecondsRealtime(duration);
            }

            i++;

            f = 0f;
            float fillAmount = (float)i / (float)n;

            while (f <= 1f)
            {
                statusbar1.fillAmount = Mathf.Lerp(statusbar1.fillAmount, fillAmount, Time.deltaTime);

                yield return null;

                f += Time.deltaTime;
            }

            SetStatusText("Status: " + i + " / " + n);
            scanResult.Remove(name);
        }

        f = 0f;

        while (f <= 1f)
        {
            statusbar1.fillAmount = Mathf.Lerp(statusbar1.fillAmount, 1f, f);

            yield return null;

            f += Time.deltaTime;
        }

        yield return new WaitForSecondsRealtime(1f);

        AudioManager.GetInstance().PlaySound("notify", computer.gameObject);
        ClearReport();

        List<PCAppInfo> list = computer.GetAppInfos();
        bool hasVirus = false;

        foreach (PCAppInfo info in list)
        {
            if (info.IsVirus)
            {
                hasVirus = true;
                break;
            }
        }

        if (!hasVirus)
        {
            scancount = 0;
            computer.ClearVirus();
        }

        isCleaning = false;
        StopClean();
    }

    private void StopClean()
    {
        if (null != ieClean)
        {
            StopCoroutine(ieClean);
            AudioManager.GetInstance().StopSound("pc.noise", computer.gameObject);

            if (!isCleaning)
                ShowReport();
        }

        ShowStatus(false);
        ieClean = null;
        isCleaning = false;
    }

    private void ClearReport()
    {
        report.SetText("");
        reportBg.color = new Color(0.75f, 0.75f, 0.75f);
        scanButton.Block(false);
        cleanButton.Block(false);
        cleanButton.SetEnabled(false);
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
            "AntivirusApp.IsEnabled"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        Dictionary<string, Action<bool>> dict = new Dictionary<string, Action<bool>>();
        dict.Add("AntivirusApp.IsEnabled", SetEnabled);
        return dict;
    }

    public override List<Formula> GetGoals()
    {
        List<Formula> list = new List<Formula>();
        list.Add(new Implication(null, WorldDB.Get("AntivirusApp.IsEnabled")));
        return list;
    }
}