using System;
using System.Collections.Generic;

namespace Language
{
    public static class NameDB
    {
        private static string[] consonants =
        {
            "b", "c", "d", "f", "g", "h", "j", "k",
            "l", "m", "n", "p", "ph", "r", "s",
            "sh", "t", "v", "w", "x", "z"
        };
        private static string[] vowels =
        {
            "a", "e", "i", "o", "u", "y"
        };

        public static string GetName(int len = 0)
        {
            string name = GenerateName(len);
            name = name.Substring(0, 1).ToUpper() + (name.Length > 1 ? name.Substring(1) : "");
            return name;
        }

        private static string GenerateName(int len = 0)
        {
            Random r = new Random(UnityEngine.Random.Range(0, 1000));

            if (len < 2)
                len = UnityEngine.Random.Range(3, 5);

            string name = "";

            if (UnityEngine.Random.value > 0.35f)
                name += consonants[r.Next(consonants.Length)];

            name += vowels[r.Next(vowels.Length)];

            while (name.Length < len)
            {
                name += consonants[r.Next(consonants.Length)];
                name += vowels[r.Next(vowels.Length)];
            }

            return name;
        }
    }
}