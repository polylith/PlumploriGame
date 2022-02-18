using UnityEngine;

namespace Creation
{
    /// <summary>
    /// This class is used to manage keys.
    /// </summary>
    public class KeyManager
    {
        private int counter;

        public KeyManager()
        {
            counter = 2 << 4;
        }

        public int[] RegisterKeys(int keyCount)
        {
            int key0 = counter;
            int n = Mathf.CeilToInt(Mathf.Log(key0 + keyCount) / Mathf.Log(2));
            counter += n > 1 ? 2 << (n - 1) : (n == 1 ? 2 : 1);
            int key1 = key0 + keyCount;
            int mask = BuildMask(key0, n);

            //Debug.Log("Key Count " + keyCount + " -> " + n + " bits : " + key0 + " - " + key1 + " Mask " + mask + " | Total " + counter);
            //ShowKeys(key0,mask,keyCount);
            
            return new int[] { key0, mask };
        }

        private static int BuildMask(int key, int n)
        {
            int mask = int.MaxValue;

            if (n < 0)
                return mask;

            n--;
            mask -= 1;
            int i = 1;

            if (i < n)
            {
                mask -= 2;
                i++;

                while (i < n)
                {
                    mask -= 2 << (i - 1);
                    i++;
                }
            }

            return mask;
        }

        private static void ShowKeys(int key0, int mask, int keyCount)
        {
            string s = "Mask\n\t" + Int2BinStr(mask);
            int v = key0;

            for (int i = 0; i < keyCount; i++)
            {
                s += "\n" + (i + 1) + "\t" + Int2BinStr(v);
                v++;
            }

            Debug.Log(s);
        }

        public static string Int2BinStr(int value, int m = 32)
        {
            string s = "";
            int i = 0;
            int n = value;

            while (i < m)
            {
                s = (n % 2).ToString() + s;
                n >>= 1;
                i++;
            }

            return s;
        }
    }
}