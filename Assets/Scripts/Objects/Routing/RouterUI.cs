using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Language;

public class RouterUI : InteractableUI
{
    public TextMeshProUGUI heading0;
    public TextMeshProUGUI connectionCount;
    public TextMeshProUGUI internMACDisplay;
    public TMP_InputField ipInput;
    public TMP_InputField maskInput;
    public TMP_InputField offsetInput;
    public UIIconButton applyButton;

    public TextMeshProUGUI heading1;
    public TextMeshProUGUI externMACDisplay;
    public TextMeshProUGUI externIPDisplay;
    public TextMeshProUGUI externGatewayDisplay;
    public UIIconButton connectButton;

    public Image[] lamps;

    protected override void BeforeHide()
    {
        if (null == interactable || !(interactable is Router router))
            return;

        router.OnLampStateChange -= SetLampState;
        router.OnConnectionCountChange(OnConnectionCountChange, false);
    }

    protected override void Initialize()
    {
        if (null == interactable || !(interactable is Router router))
            return;

        ipInput.text = router.ipString;
        maskInput.text = router.maskString;
        offsetInput.text = router.rangeOffset.ToString();

        UpdateConnectionInfo(router);
        applyButton.SetAction(() => { ApplyConfig(router); });
        connectButton.SetAction(() => { Connect(router); });

        router.OnLampStateChange += SetLampState;
        router.OnConnectionCountChange(OnConnectionCountChange, true);
        OnConnectionCountChange(router.ConnectionCount);
    }

    private void OnConnectionCountChange(int count)
    {
        string text = "Connections: " + Mathf.Max(0, count - 1);
        connectionCount.SetText(text);
    }

    private void ApplyConfig(Router router)
    {
        IPv4Config config = null;
        int offset = 0;
        string ipString = ipInput.text;
        string maskString = ipInput.text;
        string offsetString = ipInput.text;
        string macString = router.InternIPv4Config.Mac;

        try
        {
            config = IPv4Config.GetIPv4Config(
                ipString,
                maskString,
                "",
                macString
            );
        }
        catch (System.Exception ex)
        {
            // TODO ????????
            string msg = LanguageManager.GetText(
                LangKey.Error,
                ex.Message
            );
            ShowErrorStatus(msg);
            return;
        }

        try
        {
            offset = int.Parse(offsetString);
            config.CheckOffset(offset);
        }
        catch (System.Exception)
        {
            string msg = LanguageManager.GetText(
                LangKey.Error,
                offsetString
            );
            ShowErrorStatus(msg);
            return;
        }



    }

    private void SetLampState(int index, Color color)
    {
        index = Mathf.Clamp(index, 0, lamps.Length);
        lamps[index].color = color;
    }

    private void Connect(Router router)
    {
        router.Connect(() => { UpdateConnectionInfo(router); });
    }

    private void UpdateConnectionInfo(Router router)
    {
        string macString = "--:--:--:--:--:--";
        string ipString = "---.---.---.---";
        string gatewayString = "---.---.---.---";
        bool isConnected = false;

        if (null != router)
        {
            IPv4Config ipV4Config = router.ExternIPv4Config;
            macString = ipV4Config.Mac;

            if (ipV4Config.IsConnected)
            {
                ipString = ipV4Config.IP;
                gatewayString = ipV4Config.Gateway;
                isConnected = true;
                AudioManager.GetInstance().PlaySound("beep3x", router.gameObject);
            }
        }

        externMACDisplay.SetText(macString);
        externIPDisplay.SetText(ipString);
        externGatewayDisplay.SetText(gatewayString);

        connectButton.SetIcon(isConnected ? 0 : 1);
        connectButton.SetActiveColor(Color.yellow);
        router.IsConnected = isConnected;
    }

    public static void PointerEnter()
    {
        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetCursorEnabled(true, !uiGame.IsUIExclusive);
    }

    public static void PointerExit()
    {
        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetCursorEnabled(false, !uiGame.IsUIExclusive);
    }
}
