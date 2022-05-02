using System.Collections;
using System.Collections.Generic;
using Language;
using UnityEngine;

public class Router : Interactable, IIPv4Device
{
    private static Color[][] colors = new Color[][] {
        new Color[] { Color.black, Color.green, Color.red },
        new Color[] { Color.black, Color.yellow, Color.red },
        new Color[] { Color.black, Color.yellow, Color.red },
    };
        
    public delegate void OnLampStateChangeEvent(int index, Color color);
    public event OnLampStateChangeEvent OnLampStateChange;

    public int ConnectionCount { get => configMap.Count; }
    public bool IsConnected { get => isConnected; set => SetIsConnected(value); }
    public bool HasError { get => hasError; private set => SetHasError(value); }
    public bool IsTransmitting { get => isTransmitting; set => SetIsTransmitting(value); }

    public IPv4Config IPv4Config { get => ExternIPv4Config; }

    public IPv4Config InternIPv4Config { get => GetInternIPv4Config(); }
    public IPv4Config ExternIPv4Config { get => GetExternIPv4Config(); }

    public GameObject[] lamps;

    public string ipString = "192.168.0.1";
    public string maskString = "255.255.255.0";
    public int rangeOffset = 10;

    private bool isConnected;
    private bool hasError;
    private bool isTransmitting;

    private Material[] lampMaterials;
    private IEnumerator ieTransmit;
    private IEnumerator ieBlink;

    private readonly IPv4ConfigMap configMap = new IPv4ConfigMap();
    private IPv4Config internIPvpV4Config;
    private IPv4Config externIPv4Config;

    public override List<string> GetAttributes()
    {
        return new List<string>()
        {
            "IsConnected",
            "HasError"
        };
    }

    public override string GetDescription()
    {
        return LanguageManager.GetText(
            IsConnected
                ? Language.LangKey.IsConnected
                : Language.LangKey.IsDisconnected,
            GetText()
        );
    }

    public override void RegisterGoals()
    {
        // TODO
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
    }

    private void Start()
    {
        InitLamps();
        InitConfig();
        InitInteractableUI(true);
        Connect(UpdateConnectionInfo);
        Transmit();
        RandomDeviceConnection.Register(this);
    }

    private void OnDestroy()
    {
        RandomDeviceConnection.Unregister(this);
    }

    private void Transmit()
    {
        IsTransmitting = !IsTransmitting;
        GameEvent.GetInstance().Execute(Transmit, Random.Range(5f, 30f));
    }

    public void Reconfig()
    {
        InitConfig();
    }

    private void InitConfig()
    {
        internIPvpV4Config = IPv4Config.GetIPv4Config(ipString, maskString);
        configMap.Init(internIPvpV4Config);
    }

    public void OnConnectionCountChange(IPv4ConfigMap.OnCountChangeEvent callBack, bool b)
    {
        if (b)
            configMap.OnCountChange += callBack;
        else
            configMap.OnCountChange -= callBack;
    }
    
    public void Connect(System.Action callBack = null)
    {
        Transmit();
        ExtraNet.GetInstance().Connect(this, callBack);
    }

    public void Connect(IIPv4Device ipV4Device, System.Action callBack)
    {
        if (null == ipV4Device)
            return;

        Transmit();
        IPv4Config ipV4Config = ipV4Device.IPv4Config;

        if (ipV4Config.IsConnected)
        {
            // disconnect
            configMap.Remove(ipV4Config);
            ipV4Config.Reset();
        }
        else
        {
            try
            {
                // connect
                configMap.Register(ipV4Device, ipV4Config, internIPvpV4Config.ip);
                ipV4Config.Router = this;
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
                HasError = true;
                ipV4Config.Reset();
            }
        }

        if (null != callBack)
            GameEvent.GetInstance().Execute(callBack, 1f + Random.value);
    }

    private void UpdateConnectionInfo()
    {
        IsConnected = ExternIPv4Config.IsConnected;
    }

    private void InitLamps()
    {
        int n = lamps.Length;
        lampMaterials = new Material[n];

        for (int i = 0; i < n; i++)
        {
            lampMaterials[i] = lamps[i].GetComponent<Renderer>().material;
            SetLampState(i, 0);
        }
    }

    private void SetLampState(int index, int state)
    {
        Material mat = lampMaterials[index];
        Color color = colors[index][state];
        mat.SetColor("_Color", color);
        mat.SetColor("_EmissionColor", new Vector4(color.r, color.g, color.b, 15f));

        if (state > 0)
        {
            mat.EnableKeyword("_EMISSION");
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }

        OnLampStateChange?.Invoke(index, color);
    }

    private void SetIsConnected(bool isConnected)
    {
        this.isConnected = isConnected;
        SetLampState(0, isConnected ? 1 : 0);

        for (int i = 1; i < lampMaterials.Length; i++)
        {
            SetLampState(i, 0);
        }

        Fire("IsConnected", IsConnected);
    }

    private void SetIsTransmitting(bool isTransmitting)
    {
        if (this.isTransmitting == isTransmitting)
            return;

        this.isTransmitting = isTransmitting;

        if (null != ieTransmit)
            StopCoroutine(ieTransmit);

        SetLampState(1, 0);

        if (!isTransmitting || !gameObject.activeInHierarchy)
            return;

        ieTransmit = IETransmit();
        StartCoroutine(ieTransmit);
    }

    private IEnumerator IETransmit()
    {
        while (IsTransmitting)
        {
            SetLampState(1, 1);

            yield return new WaitForSecondsRealtime(Random.value);

            SetLampState(1, 0);

            yield return new WaitForSecondsRealtime(Random.value);
        }

        yield return null;

        ieTransmit = null;
    }

    private void SetHasError(bool hasError)
    {
        if (this.hasError == hasError)
            return;

        this.hasError = hasError;

        if (null != ieBlink)
            StopCoroutine(ieBlink);

        SetLampState(2, 0);
        Fire("HasError", HasError);

        if (!hasError)
            return;

        ieBlink = IEBlink();
        StartCoroutine(ieBlink);
    }

    private IEnumerator IEBlink()
    {
        while (HasError)
        {
            SetLampState(2, 2);

            yield return new WaitForSecondsRealtime(1f);

            SetLampState(2, 0);

            yield return new WaitForSecondsRealtime(1f);
        }

        yield return null;

        ieBlink = null;
    }

    private IPv4Config GetInternIPv4Config()
    {
        if (null == internIPvpV4Config)
            internIPvpV4Config = new IPv4Config();

        return externIPv4Config;
    }

    private IPv4Config GetExternIPv4Config()
    {
        if (null == externIPv4Config)
            externIPv4Config = new IPv4Config();

        return externIPv4Config;
    }

    public void Send()
    {

    }

    public void Receive()
    {

    }
}
