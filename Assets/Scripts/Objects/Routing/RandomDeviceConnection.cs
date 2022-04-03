using System.Collections.Generic;
using UnityEngine;

public class RandomDeviceConnection : IIPv4Device
{
    private static readonly Dictionary<string, Router> routerDict = new Dictionary<string, Router>();
    private static readonly Dictionary<string, List<RandomDeviceConnection>> deviceDict = new Dictionary<string, List<RandomDeviceConnection>>();

    public static void Register(Router router)
    {
        if (null == router)
            return;

        IPv4Config config = router.IPv4Config;
        string mac = config.Mac;

        if (routerDict.ContainsKey(mac))
            return;

        routerDict.Add(mac, router);
        StartConnections(router);
    }

    public static void Unregister(Router router)
    {
        if (null == router)
            return;

        IPv4Config config = router.IPv4Config;
        string mac = config.Mac;

        if (!routerDict.ContainsKey(mac))
            return;

        routerDict.Remove(mac);
        FinishConnections(mac, router);
    }

    private static void StartConnections(Router router)
    {
        IPv4Config config = router.IPv4Config;
        uint range = config.Range >> 1;

        if (range < 4)
            return;

        string mac = config.Mac;

        if (!deviceDict.ContainsKey(mac))
            deviceDict.Add(mac, new List<RandomDeviceConnection>());

        range = (uint)Mathf.Min(32, range);
        List<RandomDeviceConnection> list = deviceDict[mac];
        int n = (int)Random.Range(1, range);

        for (int i = 0; i < n; i++)
        {
            RandomDeviceConnection rdc = new RandomDeviceConnection(router);
            list.Add(rdc);
        }
    }

    private static void FinishConnections(string mac, Router router)
    {
        if (!deviceDict.ContainsKey(mac))
            return;

        List<RandomDeviceConnection> list = deviceDict[mac];

        foreach(RandomDeviceConnection rdc in list)
        {
            rdc.Disconnect();
        }

        deviceDict.Remove(mac);
    }

    public IPv4Config IPv4Config { get => GetExternIPv4Config(); }

    private IPv4Config ipV4Config;
    private readonly Router router;

    private RandomDeviceConnection(Router router)
    {
        this.router = router;
        GameEvent.GetInstance().Execute(Connect, Random.Range(30f, 60f));
    }

    private IPv4Config GetExternIPv4Config()
    {
        if (null == ipV4Config)
            ipV4Config = new IPv4Config();

        return ipV4Config;
    }

    private void Connect()
    {
        router.Connect(this, null);
    }

    private void Disconnect()
    {
        if (null == router)
            return;

        router.Connect(this, null);
    }

    public void Send()
    {
        // Nothing to do
    }

    public void Receive()
    {
        // Nothing to do
    }
}
