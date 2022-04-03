
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class IPv4Config : AbstractData
{
    public static string GetMacAddress()
    {
        Random random = new Random();
        byte[] buffer = new byte[6];
        random.NextBytes(buffer);
        string result = String.Concat(buffer.Select(
            x => string.Format("{0}:", x.ToString("X2"))).ToArray()
        );
        return result.TrimEnd(':');
    }

    public static IPv4Config GetIPv4Config(
        string ipString,
        string maskString,
        string gatewayIPString = "",
        string mac = null
    )
    {
        if (!IsValidIPv4Address(ipString))
            throw new Exception(ipString);

        if (!IsValidIPv4Address(maskString))
            throw new Exception(maskString);

        bool isGatewaySet = !"".Equals(gatewayIPString);

        if (isGatewaySet && !IsValidIPv4Address(gatewayIPString))
            throw new Exception(gatewayIPString);

        return new IPv4Config()
        {
            ip = ParseDotDecimalString(ipString),
            mask = ParseDotDecimalString(maskString),
            gateway = isGatewaySet ? ParseDotDecimalString(gatewayIPString) : 0,
            mac = mac
        };
    }

    private static Regex validIpV4AddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexOptions.Singleline);

    public static bool IsValidIPv4Address(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
            return false;

        return validIpV4AddressRegex.IsMatch(ipString.Trim());
    }

    public static uint ParseDotDecimalString(string ipV4String)
    {
        string[] octets = ipV4String.Split('.');
        uint ipV4Addr = 0;
        int factor = 1;

        for (int i = octets.Length - 1; i >= 0; i--)
        {
            ipV4Addr += (uint)(int.Parse(octets[i]) * factor);
            // left shift by 8 bits
            factor <<= 8;
        }

        return ipV4Addr;
    }

    public static string GetDotDecimal(uint addr, bool isMask = false)
    {
        if (!isMask && addr == 0)
            return "";

        List<string> octets = new List<string>();

        while (octets.Count < 4)
        {
            octets.Add("" + (addr % 256));
            // right shift to next octet
            addr >>= 8;
        }

        octets.Reverse();
        return string.Join(".", octets);
    }

    public static bool InSameSubnet(
        string ipV4Addr1String,
        string ipV4Addr2String,
        string subnetMaskString
    )
    {
        if (!IsValidIPv4Address(ipV4Addr1String)
            || !IsValidIPv4Address(ipV4Addr2String)
            || !IsValidIPv4Address(subnetMaskString)
        )
            return false;

        uint ipV4Addr1 = ParseDotDecimalString(ipV4Addr1String);
        uint ipV4Addr2 = ParseDotDecimalString(ipV4Addr2String);
        uint subnetMaskValue = ParseDotDecimalString(subnetMaskString);
        return InSameSubnet(ipV4Addr1, ipV4Addr2, subnetMaskValue);
    }

    public static bool InSameSubnet(
        uint ipV4Addr1,
        uint ipV4Addr2,
        uint subnetMaskValue
    )
    {
        uint baseIpV4Addr1 = ipV4Addr1 & subnetMaskValue;
        uint baseIpV4Addr2 = ipV4Addr2 & subnetMaskValue;
        return baseIpV4Addr1 == baseIpV4Addr2;
    }

    public static bool CheckOffset(uint mask, int offset)
    {
        return (~mask >> 1) > offset;
    }

    public bool IsConnected { get => ip > 0; }
    public string IP { get => GetDotDecimal(ip, false); }
    public string Mask { get => GetDotDecimal(mask, true); }
    public string Gateway { get => GetDotDecimal(gateway, false); }
    public string Mac { get => GetMac(); }
    public uint Range { get => ~mask; }

    public uint ip { get; private set; } = 0;
    public uint mask { get; private set; } = 0;

    private uint gateway = 0;
    private string mac = null;

    public void CheckOffset(int offset)
    {
        if (!CheckOffset(mask, offset))
            throw new Exception(offset.ToString());
    }

    public void Reset()
    {
        Set(0, 0, 0);
    }

    public void Set(uint ip, uint mask, uint gateway)
    {
        this.ip = ip;
        this.mask = mask;
        this.gateway = gateway;
    }

    private string GetMac()
    {
        if (null == mac)
            mac = GetMacAddress();

        return mac;
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
