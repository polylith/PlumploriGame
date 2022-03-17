using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PCAppInfo : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler
{
    private Color[] colors = new Color[] { Color.white, Color.yellow, new Color(1f, 0.4f, 0f, 1f) };

    public Image back;
    public TextMeshProUGUI[] texts;
    public System.Action<PCAppInfo> onKill;

    public string AppName { get => appName; }
    public bool IsVirus { get => isVirus; }
    public bool HasApp { get => null != app; }
    public bool IsAppActive { get => HasApp && app.IsActive; }
    public int Id { get => id; }
    public float MemValue { get => memValue; }
    public float TimeValue { get => timeValue; }

    private bool isVirus;
    private Computer computer;
    private PCApp app;
    private int state;
    private int id = -1;
    private float memValue;
    private int timeValue;
    private string username = "SYSTEM";
    private string appName;
    private string sysName;
    private string color;

    public string GetName()
    {
        return sysName;
    }

    public void SetVirus(bool isVirus)
    {
        if (this.isVirus == isVirus)
            return;

        this.isVirus = isVirus;

        if (isVirus)
        {
            colors = new Color[] {
                CATex.RandomColor(),
                CATex.RandomColor(),
                CATex.RandomColor(),
                CATex.RandomColor()
            };
        }
        else
        {
            colors = new Color[] {
                Color.white,
                Color.yellow,
                new Color(1f, 0.4f, 0f, 1f)
            };
        }

        if (null != app)
        {
            app.SetInfected(isVirus);
            this.isVirus = app.IsInfected;
        }
    }

    public void SetUser(string username)
    {
        this.username = username;
    }

    public void SetId(int id = -1, string name = null)
    {
        this.id = id;
        appName = name;
        sysName = name;
        string username = this.username;

        if (id < 10)
            username = "SYSTEM";

        if (isVirus)
        {
            if (id > -1 && id < 10)
                name = Computer.SysApps[id % Computer.SysApps.Length];

            name = StringUtil.ToLeet(name);
            sysName = name;
            username = StringUtil.ToLeet(username);
        }

        color = id >= 50 ? "#0000bf" : (id < 25 ? "#ff8800" : "#aaaaaa");
        texts[0].SetText("<color=" + color + ">" + id.ToString() + "</color>");
        texts[1].SetText("<color=" + color + ">" + username + "</color>");
        texts[2].SetText("<color=" + color + ">" + name + "</color>");
    }

    public void SetPC(Computer computer)
    {
        this.computer = computer;
    }

    public void SetApp(PCApp app)
    {
        this.app = app;

        if (null != app)
            SetId(app.Id, app.GetAppName());
    }

    public float NextValue()
    {
        memValue = 0f;

        if (!HasApp || IsAppActive)
        {
            float zz = Random.Range(0f, 100f);
            memValue = isVirus ? zz : (zz < 25f ? zz : 0f);
        }

        return memValue;
    }

    public void SetMemValue(float memValue)
    {
        this.memValue = memValue;
    }

    public void UpdateValues()
    {
        string state = memValue > 0f ? "running" : "sleeping";
        texts[3].SetText("<color=" + color + ">" + state + "</color>");

        if (memValue > 0f)
            timeValue++; 

        int s = (int)Mathf.Floor(timeValue % 60f);
        int m = (int)Mathf.Floor(timeValue / 60f);
        int h = (int)Mathf.Floor(timeValue / 3600f);
        string stime = (h < 10 ? "0" : "") + h + ":"
            + (m < 10 ? "0" : "") + m + ":"
            + (s < 10 ? "0" : "") + s;

        texts[4].SetText("<color=" + color + ">" + stime + "</color>");

        string svalue = Mathf.RoundToInt(memValue).ToString() + " %";
        texts[5].SetText("<color=" + color + ">" + svalue + "</color>");
    }

    public void SetState(int state)
    {
        if (this.state == state)
            return;

        state = Mathf.Min(state, colors.Length);
        this.state = state;
        back.color = colors[state];
    }
 
    public void Kill(bool force = false)
    {
        if (isVirus && !force && !computer.Crashed)
        {
            int n = Random.Range(2, 10);

            for (int j = 0; j < n; j++)
                computer.StartApp(appName, -1, true, app);
        }

        computer.Kill(this, force);
    }

    public void Destroy()
    {
        Destroy(gameObject);
        CloseApp();
    }

    public void CloseApp()
    {
        if (null != app)
        {
            app.Close(true);
            return;
        }

        computer.RemoveAppId(id);
    }

    public void Highlight(float duration = 1f)
    {
        try
        {
            if (null != gameObject.transform.parent)
            {
                duration = Mathf.Max(1f, duration);
                back.color = Color.blue;
                back.DOColor(colors[state], duration);
            }
        }
        catch (System.Exception) { }
    }

    public void Click()
    {
        SetState(2);
        onKill?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (state == 2)
            return;

        Click();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (state == 2)
            return;

        SetState(1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (state == 2)
            return;

        SetState(0);
    }
}