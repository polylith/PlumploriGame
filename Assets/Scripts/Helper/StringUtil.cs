using UnityEngine;

/// <summary>
/// Class with usefull string functions
/// </summary>
public static class StringUtil
{
    /// <summary>
    /// Fill a given string upto a given length
    /// </summary>
    /// <param name="s">string to fill</param>
    /// <param name="n">length</param>
    /// <returns></returns>
    public static string Fill(string s, int n)
    {
        if (null == s)
            s = "";

        if (n < 1)
            n = 1;

        string str = s;

        while (str.Length < n)
            str = " " + str;

        return str;
    }

    /// <summary>
    /// Turn a string into leet speak.
    /// </summary>
    /// <param name="sIn">string to translate</param>
    /// <returns>string in leet speak</returns>
    public static string ToLeet(string sIn)
    {
        if (null == sIn)
            return "? ? ?";

        string sOut = "";        
        
        for (int i = 0; i < sIn.Length; i++)
        {
            string c = sIn.Substring(i, 1).ToLower();
            float zz = Random.value;

            if ("a".Equals(c))
                sOut += "@";
            else if ("b".Equals(c))
                sOut += "8";
            else if ("e".Equals(c))
                sOut += "3";
            else if ("l".Equals(c))
                sOut += zz > 0.5f ? "1" : "!";
            else if ("h".Equals(c))
                sOut += "#";
            else if ("s".Equals(c))
                sOut += zz > 0.5f ? "5" : "$";
            else if ("t".Equals(c))
                sOut += "7";
            else if ("z".Equals(c))
                sOut += "2";
            else if (zz > 0.85f)
                sOut += c.ToUpper();
            else if (zz > 0.75f)
                sOut += c;
            else if (zz > 0.5f)
                sOut += "_";
            else if (zz > 0.3f)
                sOut += "+";
            else
                sOut += "*";
        }

        return sOut;
    }
}
