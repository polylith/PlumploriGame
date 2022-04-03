using System.Collections;
using UnityEngine;
using TMPro;
using Language;

/// <summary>
/// This config is to connect or disconnect to a router
/// or to the network directly
/// </summary>
public class IPv4ConfigDisplay : MonoBehaviour
{
    public Computer Computer { get; private set; }

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI macDislay;
    public TextMeshProUGUI ipDislay;
    public TextMeshProUGUI maskDislay;
    public TextMeshProUGUI gatewayDislay;

    public UIIconButton showHideButton;
    public UIIconButton connectButton;
    public UIIconButton cancelButton;

    private bool isVisible;
    private IEnumerator ieScale;

    public void Init(Computer computer)
    {
        Computer = computer;
        showHideButton.SetAction(ToggleVisibility);
        connectButton.SetAction(Connect);
        cancelButton.SetAction(Hide);
        titleText.SetText(
            LanguageManager.GetText(LangKey.NetworkConnection)
        );
        isVisible = true;
        SetVisible(false, true);
    }

    private Router FindRouter()
    {
        Room room = GameManager.GetInstance().CurrentRoom;
        Router[] routers = room.transform.GetComponentsInChildren<Router>();

        if (null == routers || routers.Length == 0)
            return null;

        Router router = routers[0];

        if (routers.Length > 1)
        {
            float minDistance = float.MaxValue;

            foreach (Router r in routers)
            {
                float distance = Vector3.Distance(Computer.transform.position, r.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    router = r;
                }
            }
        }

        return router;
    }

    private void Connect()
    {
        if (null == Computer)
            return;

        Computer.PCNoise();
        Router router = FindRouter();

        if (null == router)
        {
            // direct dis/connect
            ExtraNet.GetInstance().Connect(Computer, UpdateConnectionInfo);
        }
        else
        {
            // dis/connect router
            router.Connect(Computer, UpdateConnectionInfo);
        }
    }

    public void UpdateConnectionInfo()
    {
        string macString = "--:--:--:--:--:--";
        string ipString = "---.---.---.---";
        string maskString = "---.---.---.---";
        string gatewayString = "---.---.---.---";
        bool isConnected = false;

        if (null != Computer)
        {
            Computer.StopPCNoise();
            IPv4Config ipV4Config = Computer.IPv4Config;
            macString = ipV4Config.Mac;

            if (ipV4Config.IsConnected)
            {
                ipString = ipV4Config.IP;
                maskString = ipV4Config.Mask;
                gatewayString = ipV4Config.Gateway;
                isConnected = true;
                AudioManager.GetInstance().PlaySound("beep3x", Computer.gameObject);
            }
        }

        macDislay.SetText(macString);
        ipDislay.SetText(ipString);
        maskDislay.SetText(maskString);
        gatewayDislay.SetText(gatewayString);

        showHideButton.SetIcon(isConnected ? 1 : 0);
        showHideButton.SetActiveColor(isConnected ? Color.green : Color.red);

        connectButton.SetIcon(isConnected ? 0 : 1);
        connectButton.SetActiveColor(Color.yellow);
    }

    public void ToggleVisibility()
    {
        SetVisible(!isVisible);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool isVisible, bool instant = false)
    {
        if (this.isVisible == isVisible)
            return;

        this.isVisible = isVisible;
        Vector3 scale = isVisible ? Vector3.one : new Vector3(0f, 1f, 1f);

        if (isVisible)
            UpdateConnectionInfo();

        if (instant)
        {
            transform.localScale = scale;
            return;
        }

        if (null != ieScale)
            StopCoroutine(ieScale);

        ieScale = IEScale(scale);
        StartCoroutine(ieScale);
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0f;

        while (f <= 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, f);

            yield return null;

            f += 0.2f;
        }

        yield return null;

        ieScale = null;
    }
}
