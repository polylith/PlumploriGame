using System.Collections.Generic;
using UnityEngine;

public static class PhoneDirectory
{
    private static bool isInited;
    private static readonly string[] numbers = new string[] {
        "434565", "321555", "999573", "765123"
    };

    public static void Init()
    {
        if (isInited)
            return;

        for (int i = 0; i < numbers.Length; i++)
        {
            string name = Language.NameDB.GetName();
            string number = numbers[i];
            TelephoneDevice device = new TelephoneDevice(name, number);
            Register(device);
        }

        isInited = true;
    }
    
    public static ITelephoneDevice GetPhone(string number)
    {
        if (null == number || !dict.ContainsKey(number))
            return null;

        return dict[number];
    }

    public static void Remove(string number)
    {
        if (null == number || !dict.ContainsKey(number))
            return;

        dict.Remove(number);
    }

    private static Dictionary<string, ITelephoneDevice> dict = new Dictionary<string, ITelephoneDevice>();
    private static Dictionary<string, PhoneBook> phonebooks = new Dictionary<string, PhoneBook>();

    public static PhoneBook GetPhoneList(ITelephoneDevice phoneDevice)
    {
        if (!phonebooks.ContainsKey(phoneDevice.Number))
        {
            PhoneBook phoneBook = new PhoneBook();

            foreach (string number in dict.Keys)
            {
                if (!number.Equals(phoneDevice.Number))
                {
                    string name = phoneDevice.Name;
                    PhoneBookEntry entry = new PhoneBookEntry()
                    { Name = name, Number = number };
                    phoneBook.Add(entry);
                }
            }

            phonebooks.Add(phoneDevice.Number, phoneBook);
        }

        return phonebooks[phoneDevice.Number];
    }

    public static string Register(ITelephoneDevice phoneDevice)
    {
        string number = phoneDevice.Number;

        if (string.IsNullOrEmpty(number))
            number = GetNumber();

        if (!dict.ContainsKey(number))
            dict.Add(number, phoneDevice);

        return number;
    }

    private static string GetNumber()
    {
        string number = Random.Range(12345, 99999).ToString();

        do
        {
            number += (Random.Range(0, 100) % 10).ToString();
        }
        while (dict.ContainsKey(number));

        return number;
    }
}