using System.Collections.Generic;
using Action;
using Language;
using UnityEngine;

public class Computer : Interactable
{
    public static string[] SysApps = new string[]
    {
        "SYSTEM", "SERVICES", "SVCHOST", "LOGON",
        "KERNEL", "WINDOWSERVER", "MDS", "PIDINFO"
    };

    private static string[] virus = new string[]
    {
        "brain", "bitfire", "blaster", "tequila",
        "code-red", "iloveu", "sobig", "wannacry",
        "my doom", "zeus", "sasser", "melissa",
        "storm trojan", "stuxnet", "shamoon"
    };

    public enum State
    {
        Off = -1,
        LogIn,
        Booting,
        Running,
        ShuttingDown,
        Rebooting
    }

    public delegate void OnAppStartEvent(PCAppInfo info);
    public event OnAppStartEvent OnAppStart;

    public Dictionary<int, PCAppInfo> Infos { get => infos; }
    public PCApp CurrentApp { get; private set; }
    public State CurrentState { get; set; } = State.Off;
    public bool IsRunning { get => CurrentState >= State.Running && CurrentState < State.ShuttingDown; }
    public bool HasVirus { get => hasVirus; }
    public bool Crashed { get; set; }

    public PCAppInfo pcAppInfoPrefab;

    public AudioSource audioSource;

    public Transform appsParent;

    public LoginData CurrentUser;
    
    private List<int> appIds = new List<int>();
    private Dictionary<int, PCAppInfo> infos = new Dictionary<int, PCAppInfo>();
    private List<PCApp> apps;
    private bool hasVirus = false;

    public override List<string> GetAttributes()
    {
        List<string> attributes = new List<string>() {
            "IsEnabled",
            "HasVirus"
        };

        // TODO add apps attributes

        return attributes;
    }

    public override string GetDescription()
    {
        string text = LanguageManager.GetText(
            CurrentState.ToString(),
            GetText()
        ) + ".";

        return text;
    }

    public void AppFire(string token, bool isDesignated)
    {
        Fire(token, isDesignated);
    }

    public override void RegisterGoals()
    {
        // TODO
    }

    protected override void RegisterAtoms()
    {
        // TODO
    }

    public override bool Interact(Interactable interactable)
    {
        if (ActionController.GetInstance().IsCurrentAction(typeof(UseAction))
            && IsInteractionEnabled() == 1)
        {
            CheckCurrentLoginData();
            UIGame.GetInstance().HideCursor(true);
        }

        return base.Interact(interactable);
    }

    private void CheckCurrentLoginData()
    {
        if (null == CurrentUser)
            return;

        if (!CurrentUser.keepLogin
            || CurrentUser.userGroup == LoginData.UserGroup.Guest)
            CurrentUser = null;
    }

    private void Start()
    {
        InitInteractableUI(true);
    }

    private void InitSystem()
    {
        apps = new List<PCApp>();
        PCApp[] arr = appsParent.GetComponentsInChildren<PCApp>();

        foreach (PCApp pcAppPrefab in arr)
        {
            PCApp pcApp = Instantiate<PCApp>(pcAppPrefab);
            apps.Add(pcApp);
        }

        for (int i = 0; i < SysApps.Length; i++)
        {
            int id = i + 1;
            StartApp(SysApps[i], id, false, null);
        }
    }

    public void InitAppInfos()
    {
        InitSystem();

        foreach (PCApp app in apps)
        {
            if (app.IsEnabled && app.IsReady && app.IsActive)
                RegisterApp(app);
        }

        if (hasVirus)
        {
            hasVirus = false;
            SetVirus(true);
        }
    }

    private int GetAppId(bool hasApp)
    {
        int id = Random.Range(0, 50) + (hasApp ? 50 : 0);
        int count = 0;

        while (appIds.Contains(id))
        {
            id = Random.Range(0, 50) + (hasApp ? 50 : 0);
            count++;

            if (count > 50)
            {
                Crashed = true;
                appIds.Clear();
                infos.Clear();
                ((ComputerUI)InteractableUI).ShowBlueScreen(true, true);
                return -1;
            }
        }

        appIds.Add(id);
        return id;
    }

    public List<PCAppInfo> GetAppInfos()
    {
        List<PCAppInfo> list = new List<PCAppInfo>();

        foreach (int id in infos.Keys)
            list.Add(infos[id]);

        return list;
    }

    public void ShowAppIcons(Transform desktopGrid, Transform taskBar)
    {
        for (int i = 0; i < apps.Count; i++)
        {
            ShowAppIcon(desktopGrid, taskBar, apps[i]);
        }
    }

    public void ShowAppIcon(Transform desktopGrid, Transform taskBar, PCApp app)
    {
        app.gameObject.SetActive(true);
        app.SetPC(this);
        PCIcon pcIcon = app.GetAppIcon();
        pcIcon.SetParents(desktopGrid, taskBar);
    }

    public void KillProcess(string sid = "")
    {
        Kill(sid, false);
    }

    public bool Kill(string sid, bool force = false)
    {
        if (null == sid)
            return false;

        try
        {
            int id = int.Parse(sid);

            if (!infos.ContainsKey(id))
                return false;

            infos[id].Kill(force);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public void Kill(PCAppInfo appInfo, bool force = false)
    {
        if (null == appInfo)
            return;

        int id = appInfo.Id;
        RemoveAppId(id);
        appInfo.CloseApp();
        appInfo.Destroy();

        if (!force && id < 25)
        {
            ((ComputerUI)InteractableUI).ShowBlueScreen(id < 10, id >= 10);
        }
    }

    public void RemoveAppId(int id)
    {
        if (infos.ContainsKey(id))
        {
            PCAppInfo appInfo = infos[id];
            infos.Remove(id);
            appInfo.SetId();
            Destroy(appInfo.gameObject);
        }

        if (!appIds.Contains(id))
            return;

        appIds.Remove(id);
    }

    private void RegisterApp(PCApp app)
    {
        if (null == app)
            return;

        int id = app.Id;

        if (!app.HasId)
        {
            id = GetAppId(true);

            if (id < 0)
            {
                return;
            }

            app.SetId(id);
        }

        if (HasVirus && id >= 50 && Random.value > 0.8f)
        {
            app.SetEnabled(false);

            System.Action closeAction = () =>
            {
                app.Close();
                app.GetAppIcon().ShowInTaskBar(false);
                app.SetEnabled(true);
            };
            ((ComputerUI)InteractableUI).ShowDialog(
                app.gameObject,
                true,
                PCDialog.DialogType.Error,
                LanguageManager.GetText(
                    Language.LangKey.AppLaunchError,
                    app.GetAppName()
                ),
                PCDialog.ButtonLabels.Ok,
                new List<System.Action>() {
                    closeAction
                },
                closeAction
            );
            return;
        }

        StartApp(app.GetAppName(), id, HasVirus, app);
    }

    public void StartApp(string name, int id, bool isVirus, PCApp app)
    {
        if (Crashed)
            return;

        if (id < 0)
            id = GetAppId(null != app);

        PCAppInfo appInfo;

        if (!infos.ContainsKey(id))
        {
            appInfo = Instantiate(pcAppInfoPrefab) as PCAppInfo;
            infos.Add(id, appInfo);
        }
        else
        {
            appInfo = infos[id];
        }

        ((ComputerUI)InteractableUI).SetAppParent(app);
        appInfo.SetPC(this);
        appInfo.SetApp(app);
        appInfo.SetVirus(isVirus);

        string username = "ANONYM";

        if (null != CurrentUser)
        {
            username = CurrentUser.username;
        }

        appInfo.SetUser(username);
        appInfo.SetId(id, name);
        appIds.Add(id);

        appInfo.name = "App Info" + id;
        OnAppStart?.Invoke(appInfo);
    }

    public void SetAppEnabled(PCApp app, bool isEnabled)
    {
        app.GetAppIcon().gameObject.SetActive(isEnabled);

        if (!isEnabled)
            app.Close();
    }

    public void SetCurrentApp(PCApp app)
    {
        if (CurrentApp == app)
            return;

        if (null != CurrentApp)
        {
            CurrentApp.GetAppIcon().ResetState();
            CurrentApp.Hide();
        }

        if (null != app && !app.HasId)
            RegisterApp(app);

        CurrentApp = app;
    }

    public void CloseAllApps()
    {
        List<int> list = new List<int>();
        list.AddRange(infos.Keys);

        foreach (int id in list)
        {
            if (infos.ContainsKey(id))
                infos[id].Kill(true);
        }

        infos.Clear();
        appIds.Clear();
        CurrentApp = null;
    }
        
    public void Beep(string arg)
    {
        ((ComputerUI)InteractableUI).Beep(this);
    }

    private void SetVirus(bool hasVirus)
    {
        if (this.hasVirus == hasVirus)
            return;

        this.hasVirus = hasVirus;
        Fire("HasVirus", hasVirus);

        ((ComputerUI)InteractableUI).SetVirus(this);

        if (!hasVirus)
        {
            List<PCAppInfo> list = GetAppInfos();

            foreach (PCAppInfo info in list)
            {
                info.SetVirus(false);
            }

            return;
        }

        StartVirus();
    }

    public void StartVirus()
    {
        int modul = Random.Range(3, 6);
        int numberOfVirusInstances = Random.Range(1, 10);
        int virusIndex = Random.Range(0, 100) % virus.Length;

        for (int index = 0; index < numberOfVirusInstances; index++)
        {
            if (index % modul == 0)
                virusIndex = Random.Range(0, 100) % virus.Length;

            StartApp(virus[virusIndex], -1, true, null);
        }
    }

    public void ClearVirus()
    {
        if (!hasVirus)
            return;

        SetVirus(false);
    }

    public void Shutdown(string arg = "")
    {
        ((ComputerUI)InteractableUI).Shutdown();
    }

    public void Reboot(string arg = "")
    {
        ((ComputerUI)InteractableUI).Reboot(this);
    }

    public void LogOff()
    {
        CurrentUser = null;
        CurrentState = State.LogIn;
    }

    public LoginData LogIn(LoginData loginData)
    {
        if (null == CurrentUser)
            CurrentUser = loginData;

        if (null != loginData)
            CurrentState = State.Off;

        return CurrentUser;
    }
}
