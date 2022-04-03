using System;
using System.Collections.Generic;
using Language;
using UnityEngine;

public class PCAppShopApp : PCApp
{
    public UITextButton categoryButtonPrefab;
    public PCAppDisplay pcAppDisplayPrefab;

    public RectTransform pcAppCategoriesParent;
    public RectTransform pcAppsListDisplayParent;

    private string currentCategoryName = "";
    private List<PCApp.Category> categories;
    private List<UITextButton> categoryButtons;
    private UITextButton activeCategoryButton;
    private Dictionary<string, List<PCAppDisplay>> categoryMap;

    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        return null;
    }

    public override List<Formula> GetGoals()
    {
        return null;
    }

    protected override void Effect()
    {
        // not here!!!
    }

    public override void SetInfected(bool isInfected)
    {
        // never infect this app!!!
        this.isInfected = false;
    }

    public override void ResetApp()
    {
        UnsetActiveCategory();
        ClearParent(pcAppCategoriesParent);
        ClearParent(pcAppsListDisplayParent);
    }

    protected override void PreCall()
    {
        ResetApp();

        categoryMap = new Dictionary<string, List<PCAppDisplay>>();
        categoryButtons = new List<UITextButton>();
        categories = AppShop.GetInstance().Categories;

        foreach (PCApp.Category category in categories)
        {
            UITextButton categoryButton = GameObject.Instantiate(categoryButtonPrefab);
            categoryButton.localScales = new Vector3[] { Vector3.one, Vector3.one, Vector3.one };
            categoryButton.hoverScale = false;
            categoryButton.changeColor = false;
            categoryButton.SetText(
                LanguageManager.GetText(category.ToString())
            );
            categoryButton.SetAction(() => {
                ShowCategory(category, categoryButton);
            });
            categoryButton.isExclusive = true;
            categoryButton.transform.SetParent(pcAppCategoriesParent);
            categoryButtons.Add(categoryButton);
        }

        ShowCategory(categories[0], categoryButtons[0]);
    }

    private void ClearParent(RectTransform rectTransform)
    {
        foreach (Transform trans in rectTransform)
        {
            Destroy(trans.gameObject);
        }
    }

    private void UnsetActiveCategory()
    {
        if (null != activeCategoryButton)
        {
            activeCategoryButton.SetState(0);
        }

        if (!"".Equals(currentCategoryName)
            && categoryMap.ContainsKey(currentCategoryName))
        {
            foreach (PCAppDisplay pcAppDisplay in categoryMap[currentCategoryName])
                pcAppDisplay.gameObject.SetActive(false);
        }

        currentCategoryName = "";
        activeCategoryButton = null;
    }

    private void ShowCategory(PCApp.Category category, UITextButton categoryButton)
    {
        if (currentCategoryName.Equals(category.ToString()))
            return;

        UnsetActiveCategory();
        activeCategoryButton = categoryButton;
        activeCategoryButton.SetState(2);
        currentCategoryName = category.ToString();

        if (!categoryMap.ContainsKey(currentCategoryName))
        {
            List<PCApp> appList = AppShop.GetInstance().GetPCApps(category);
            List<PCAppDisplay> displayList = new List<PCAppDisplay>();

            foreach (PCApp pcApp in appList)
            {
                PCAppDisplay pcAppDisplay = pcAppDisplayPrefab.Instanciate(
                    pcApp.appName,
                    pcApp.icon
                );

                bool isInstalled = computer.installedAppNames.Contains(pcApp.appName);
                pcAppDisplay.IsInstalled = isInstalled;

                if (!isInstalled)
                {
                    pcAppDisplay.installButton.SetAction(() =>
                    {
                        InstallApp(pcAppDisplay);
                    });
                }

                pcAppDisplay.transform.SetParent(pcAppsListDisplayParent);
                pcAppDisplay.transform.localScale = Vector3.one;
                pcAppDisplay.gameObject.SetActive(false);
                displayList.Add(pcAppDisplay);
            }

            categoryMap.Add(currentCategoryName, displayList);
        }


        foreach (PCAppDisplay pcAppDisplay in categoryMap[currentCategoryName])
            pcAppDisplay.gameObject.SetActive(true);
    }

    private void InstallApp(PCAppDisplay pcAppDisplay)
    {
        pcAppDisplay.Install(computer);
    }
}
