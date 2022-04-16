using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Language;
using System.Collections;

public class PCAppDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsInstalled { get => isInstalled; set => SetIsInstalled(value); }

    public Image back;
    public Outline outline;
    public TextMeshProUGUI appNameDisplay;
    public Image icon;
    public UIIconButton installButton;
    public Image progressBar;
    public TextMeshProUGUI progressText;

    private bool isInstalled;
    private string appName;
    private int state = -1;
    private IEnumerator ieInstall;

    public PCAppDisplay Instanciate(string appName, Sprite sprite)
    {
        PCAppDisplay pcAppDisplay = GameObject.Instantiate(this);
        pcAppDisplay.transform.name = "PC App Display " + appName;
        pcAppDisplay.appName = appName;
        pcAppDisplay.appNameDisplay.SetText(LanguageManager.GetText(appName));
        pcAppDisplay.icon.sprite = sprite;
        return pcAppDisplay;
    }

    public void Install(Computer computer)
    {
        if (null != ieInstall)
            return;

        ieInstall = IEInstall(computer);
        StartCoroutine(ieInstall);
    }

    private IEnumerator IEInstall(Computer computer)
    {
        float f = 0f;
        progressText.color = Color.yellow;
        computer.PCNoise();

        while (f <= 1f)
        {
            progressBar.fillAmount = f;
            progressText.SetText(Mathf.Round(f * 100) + " %");

            yield return null;

            f += Random.value * 0.01f;
        }

        progressBar.fillAmount = 1f;
        progressText.SetText("100 %");

        yield return null;

        PCApp pcApp = AppShop.GetInstance().GetPCApp(appName);
        computer.InstallApp(pcApp);
        computer.StopPCNoise();
        AudioManager.GetInstance().PlaySound("notify", computer.gameObject);
        IsInstalled = true;
        ieInstall = null;
    }

    private void SetIsInstalled(bool isInstalled)
    {
        this.isInstalled = isInstalled;
        installButton.IsEnabled = !isInstalled;
        progressText.SetText(
            LanguageManager.GetText(
                isInstalled ? LangKey.Installed : LangKey.Install
            )
        );
        progressText.color = Color.black;
        icon.color = isInstalled ? Color.yellow : new Color(0.7f, 0.7f, 0.7f);
        progressBar.fillAmount = 0f;
        SetState(isInstalled ? 2 : 0);
    }

    private void SetState(int state)
    {
        if (this.state == state)
            return;

        this.state = state;

        switch (state)
        {
            case 0:
                back.color = Color.white;
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, 1f);
                break;
            case 1:
                back.color = Color.white;
                outline.effectColor = Color.blue;
                outline.effectDistance = new Vector2(5f, -5f);
                break;
            case 2:
                back.color = new Color(0.8f, 0.8f, 0.8f);
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(5f, 5f);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsInstalled)
            return;

        SetState(1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsInstalled)
            return;

        SetState(0);
    }
}
