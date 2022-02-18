using System.Collections.Generic;

/// <summary>
/// Class for usefull array and list operations.
/// </summary>
public static class ArrayHelper
{
    private static readonly System.Random rnd = new System.Random();

    public static T[][] GetMatrix<T>(int size1, int size2)
    {
        T[][] M = new T[size1][];

        for (int i = 0; i < M.Length; i++)
            M[i] = new T[size2];

        return M;
    }

    public static int[] Copy(int[] arr)
    {
        if (null == arr)
            return null;

        int[] arr2 = new int[arr.Length];

        for (int i = 0; i < arr.Length; i++)
            arr2[i] = arr[i];

        return arr2;
    }
        
    public static List<T> Intersect<T>(List<T> list1, List<T> list2)
    {
        List<T> res = new List<T>();
        List<T> l1 = list1;
        List<T> l2 = list2;

        if (list2.Count < list1.Count)
        {
            l1 = list2;
            l2 = list1;
        }

        for (int i = 0; i < l1.Count; i++)
        {
            for (int j = 0; j < l2.Count; j++)
            {
                if (l1[i].Equals(l2[j]) && !res.Contains(l1[i]))
                {
                    res.Add(l1[i]);
                    break;
                }
            }
        }

        return res;
    }

    public static List<int> Union(List<int> list1, List<int> list2)
    {
        List<int> list3 = new List<int>();
        list3.AddRange(list1);

        for (int i = 0; i < list2.Count; i++)
        {
            int v = list2[i];

            if (!list3.Contains(v))
                list3.Add(v);
        }

        return list3;
    }

    public static bool IsSubset(List<int> list1, List<int> list2)
    {
        for (int i = 0; i < list1.Count; i++)
        {
            int v = list1[i];

            if (!list2.Contains(v))
                return false;
        }

        return true;
    }

    public static int[] Intersect(int[] arr1, int[] arr2)
    {
        List<int> list = new List<int>();
        List<int> list1 = new List<int>();
        list1.AddRange(arr1);

        for (int i = 0; i < arr2.Length; i++)
        {
            if (list1.Contains(arr2[i]) && !list.Contains(arr2[i]))
                list.Add(arr2[i]);
        }

        return list.ToArray();
    }

    public static int[] GetArray(int offset, int length, int step = 1)
    {
        int[] arr = new int[length];
        int j = offset;

        for (int i = 0; i < length; i++)
        {
            arr[i] = j;
            j += step;
        }

        return arr;
    }

    public static void Shuffle<T>(List<T> list)
    {
        if (null == list || list.Count < 2)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            int j = i;

            while (j == i)
                j = rnd.Next(1, 100) % list.Count;

            T h = list[i];
            list[i] = list[j];
            list[j] = h;
        }
    }

    public static void ShuffleArray(int[] arr, bool full = true)
    {
        if (null == arr || arr.Length < 2)
        {
            return;
        }

        for (int i = 0; i < arr.Length; i++)
        {
            int j = i;

            while (j == i || arr[i] == arr[j] && full)
                j = rnd.Next(1, 100) % arr.Length;

            Swap(arr, i, j);
        }
    }

    public static T[] Shuffle<T>(T[] arr, bool full = true)
    {
        if (null == arr || arr.Length < 2)
        {
            return arr;
        }

        for (int i = 0; i < arr.Length; i++)
        {
            int j = i;

            while (j == i || arr[i].Equals(arr[j]) && full)
                j = rnd.Next(1, 100) % arr.Length;

            Swap(arr, i, j);
        }

        return arr;
    }

    public static void Swap<T>(T[] arr, int i, int j)
    {
        if (i < 0 || i >= arr.Length || j < 0 || j >= arr.Length || i == j)
            return;

        T h = arr[i];
        arr[i] = arr[j];
        arr[j] = h;
    }
}