using System;
using System.Linq;
using System.Text.RegularExpressions;

public static class PromptInutParser
{
    private static string pattern = @"^(?:""([^""]*)""\s*|([^""\s]+)\s*)+";

    public static PromptCommand Parse(string str)
    {
        try
        {
            Regex parseDir = new Regex(pattern, RegexOptions.IgnoreCase);

            if (parseDir.IsMatch(str))
            {
                Match dir = parseDir.Match(str);
                var captures = dir.Groups[1].Captures.Cast<Capture>().Concat(
                   dir.Groups[2].Captures.Cast<Capture>()).
                   OrderBy(x => x.Index).
                   ToArray();
                string code = captures[0].Value;
                string[] args = null;

                if (captures.Length > 1)
                {
                    args = new string[captures.Length - 1];

                    for (int i = 1; i < captures.Length; i++)
                    {
                        args[i - 1] = captures[i].Value;
                    }
                }

                return new PromptCommand(code, args);
            }
        }
        catch(Exception) { }

        throw new Exception();
    }
}