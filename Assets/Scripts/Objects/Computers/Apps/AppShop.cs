using System.Collections.Generic;
using UnityEngine;

public class AppShop : MonoBehaviour
{
    private static AppShop ins;

    public static AppShop GetInstance()
    {
        return ins;
    }

    public List<PCApp.Category> Categories { get => GetCategories(); }

    public PCApp[] pcApps;
    private readonly Dictionary<string, PCApp> appDict = new Dictionary<string, PCApp>();
    private readonly Dictionary<PCApp.Category, List<PCApp>> categoryDict = new Dictionary<PCApp.Category, List<PCApp>>();

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        foreach (PCApp pcApp in pcApps)
        {
            if (!appDict.ContainsKey(pcApp.appName))
            {
                appDict.Add(pcApp.appName, pcApp);
            }

            if (!categoryDict.ContainsKey(pcApp.category))
            {
                categoryDict.Add(pcApp.category, new List<PCApp>());
            }

            List<PCApp> appList = categoryDict[pcApp.category];

            if (!appList.Find(app => app.appName.Equals(pcApp.appName)))
            {
                appList.Add(pcApp);
            }            
        }
    }

    private List<PCApp.Category> GetCategories()
    {
        List<PCApp.Category> categories = new List<PCApp.Category>();

        foreach (PCApp.Category category in categoryDict.Keys)
        {
            categories.Add(category);
        }

        return categories;
    }

    public List<PCApp> GetPCApps(PCApp.Category category)
    {
        if (categoryDict.ContainsKey(category))
            return categoryDict[category];

        return new List<PCApp>();
    }

    public PCApp GetPCApp(string appName)
    {
        if (!appDict.ContainsKey(appName))
            return null;

        PCApp pcApp = Instantiate<PCApp>(appDict[appName]);
        pcApp.transform.name = appName;
        return pcApp;
    } 
}
