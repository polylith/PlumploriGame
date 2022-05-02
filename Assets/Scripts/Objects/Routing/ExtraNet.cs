public class ExtraNet : AbstractData
{
    private static ExtraNet ins;

    public static ExtraNet GetInstance()
    {
        if (null == ins)
            ins = new ExtraNet();

        return ins;
    }

    private readonly IPv4ConfigMap configMap;
    private IPv4Config ipV4Config;

    private ExtraNet()
    {
        uint ip = IPv4Config.ParseDotDecimalString("80.128.0.0");
        uint mask = IPv4Config.ParseDotDecimalString("255.255.0.0");
        ip &= mask;
        ip++;
        ipV4Config = new IPv4Config();
        ipV4Config.Set(ip, mask, ip);
        configMap = new IPv4ConfigMap(ipV4Config, 5);
    }

    public void Connect(IIPv4Device ipV4Device, System.Action callBack)
    {
        if (null == ipV4Device)
            return;

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
                ipV4Config.Reset();
                configMap.Register(ipV4Device, ipV4Config, this.ipV4Config.ip);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        GameEvent.GetInstance().Execute(callBack, 2f * (1f + UnityEngine.Random.value));
    }

    public void Load()
    {
        // TODO
    }

    public void Save()
    {
        // TODO
    }    
}
