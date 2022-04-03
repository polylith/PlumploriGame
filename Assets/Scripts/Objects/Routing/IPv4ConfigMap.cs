using System.Collections.Generic;

public class IPv4ConfigMap
{
    public delegate void OnCountChangeEvent(int count);
    public event OnCountChangeEvent OnCountChange;

    public int Count { get => ipMap.Count; }
    public uint BaseIp { get; private set; }
    private uint mask;
    private uint offset;

    private readonly Dictionary<string, IIPv4Device> ipMap = new Dictionary<string, IIPv4Device>();
    private readonly Dictionary<string, uint> ipMem = new Dictionary<string, uint>();

    public IPv4ConfigMap(IPv4Config config = null, uint offset = 10)
    {
        if (null != config)
            Init(config, offset);
    }

    public void Init(IPv4Config config, uint offset = 10)
    {
        foreach (IIPv4Device ipV4Device in ipMap.Values)
        {
            if (null != ipV4Device)
            {
                IPv4Config ipV4config = ipV4Device.IPv4Config;
                ipV4config.Reset();
            }
        }

        this.offset = (uint)UnityEngine.Mathf.Max(1, offset);
        ipMap.Clear();
        ipMem.Clear();
        mask = config.mask;
        BaseIp = config.ip & mask;
        ipMap.Add(config.IP, null);
        OnCountChange?.Invoke(Count);
    }

    /// <summary>
    /// Register an IP for a connected device
    /// Might throw IndexOutOfRangeException when no IP is available
    /// </summary>
    /// <param name="ipV4Device">device to register</param>
    /// <param name="config">IPv4Config to set ip, mask and gateway</param>
    /// <param name="gateway">gateway to connect</param>
    public void Register(IIPv4Device ipV4Device, IPv4Config config, uint gateway)
    {
        uint ip = BaseIp + offset;

        if (ipMem.ContainsKey(config.Mac))
        {
            ip = ipMem[config.Mac];
        }

        string ipString;

        while (true)
        {
            ipString = IPv4Config.GetDotDecimal(ip);

            if (!ipMap.ContainsKey(ipString))
                break;
            else
                ip++;

            if (!IPv4Config.InSameSubnet(BaseIp, ip, mask))
                throw new System.IndexOutOfRangeException();
        }

        ipMap.Add(ipString, ipV4Device);
        config.Set(ip, mask, gateway);
        OnCountChange?.Invoke(Count);
    }

    public void Remove(IPv4Config config)
    {
        if (null == config || !ipMap.ContainsKey(config.IP))
            return;

        ipMap.Remove(config.IP);

        if (ipMem.ContainsKey(config.Mac))
        {
            ipMem[config.Mac] = config.ip;
        }
        else
        {
            ipMem.Add(config.Mac, config.ip);
        }

        OnCountChange?.Invoke(Count);
    }
}
