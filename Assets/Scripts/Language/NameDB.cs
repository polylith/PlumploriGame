using System;
using System.Collections.Generic;

namespace Language
{
    public static class NameDB
    {
        private static string[][] consonants = {
            new string[]{
                "b", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "s",
                "dr", "gr", "kr", "kl", "gl", "fr", "sl", "shl", "shr", "sh",
                "st", "sk", "t", "w", "z" },
            new string[]{
                "rk", "lk", "lg", "rg", "ng", "nk", "rt", "ck", "ch", "ll",
                "rr", "ss", "bb", "tt", "nn", "mm" }
        };
        private static string[][] vowels = {
            new string[] { "a", "i", "e" },
            new string[] { "a", "u", "e", "o", "y", "ie" }
        };
        private static List<string> list = new List<string>();

        public static string GetName(int len = 0)
        {
            string name = GenerateName(len);
            name = name.Substring(0, 1).ToUpper() + (name.Length > 1 ? name.Substring(1) : "");
            return name;
        }

        private static string GenerateName(int len = 0)
        {
            Random r = new Random(UnityEngine.Random.Range(0, 1000));
            
            if (len <= 2)
                len = r.Next(5) + r.Next(5) + 2;

            string name = "";

            int j = 0;
            int i = 0;

            if (r.Next(0,100) > 50)
            {
                i = r.Next(0, 100) % vowels[j].Length;
                name += vowels[j][i];
                j = len % 2;
            }

            i = r.Next(0, 100) % consonants[j].Length;
            name += consonants[j][i];
            j = len % 2;
            i = r.Next(0, 100) % vowels[j].Length;
            name += vowels[j][i];

            while (name.Length < len)
            {
                j = r.Next(0, 100) % 2;
                i = r.Next(0, 100) % consonants[j].Length;
                name += consonants[j][i];

                if (name.Length >= len || j == 0 && consonants[j][i].Length > 1)
                    break;

                j = len % 2;
                i = r.Next(0, 100) % vowels[j].Length;
                name += vowels[j][i];
            }

            if (list.Contains(name))
                name = GenerateName(len);
            else
                list.Add(name);

            return name;
        }
    }
}