using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Language;
using System.Collections.Generic;

/// <summary>
/// <para>
/// This class is the interactive UI for a computer.
/// This UI is displayed in full screen and hides the
/// action bar. The pointer cursor is used as the
/// cursor for the computer.
/// </para>
/// </summary>
public class ComputerUI : InteractableUI
{
    public PCDialogs pcDialogs;
    public PCCursor pcCursor;
    public Transform desktop;
    public Transform desktopGrid;
    public Transform appParent;
    public Transform taskBar;
    public BlueScreen blueScreen;
    public LoginScreen loginScreen;

    public RectTransform bootMenu;
    public UIIconTextButton shutdownButton;
    public UIIconTextButton rebootButton;
    public UIIconTextButton logoffButton;

    public Image desktopBackground;
    public Image foreground;

    public Transform bootTrans;
    public Image bootImage;
    public Image bootBar;
    public TextMeshProUGUI bootPercentDisplay;

    public IPv4ConfigDisplay ipV4ConfigDisplay;

    public PCClock pcClock;

    private IEnumerator ieAnim;
    private bool isBootMenuVisible;

    private Sequence flashSeq;

    protected override void Initialize()
    {
        InteractableUI.CloseOtherActiveUIs(this);
        loginScreen.gameObject.SetActive(true);
        closeButton.SetAction(ToggleBootMenu);

        shutdownButton.SetAction(Shutdown);
        shutdownButton.SetText(
            LanguageManager.GetText(LangKey.Shutdown)
        );

        rebootButton.SetAction(Reboot);
        rebootButton.SetText(
            LanguageManager.GetText(LangKey.Reboot)
        );

        logoffButton.SetAction(LogOff);
        logoffButton.SetText(
            LanguageManager.GetText(LangKey.LogOff)
        );

        OnVisibilityChange += SwitchState;
        bootTrans.gameObject.SetActive(false);

        loginScreen.shutdownButton.SetAction(Shutdown);
        loginScreen.OnValidLogin += OnValidLogin;
        ClearAppParent();
        HideDesktop();
    }

    public bool ShowDialog(
        GameObject owner,
        bool isBlocking,
        PCDialog.DialogType dialogType,
        string messageText,
        PCDialog.ButtonLabels buttonLabels,
        List<System.Action> buttonActions = null,
        System.Action closeAction = null,
        string[] buttonTexts = null
    )
    {
        return pcDialogs.ShowDialog(
            owner,
            isBlocking,
            dialogType,
            messageText,
            buttonLabels,
            buttonActions,
            closeAction,
            buttonTexts
        );
    }

    private void ClearAppParent()
    {
        foreach (Transform trans in appParent)
        {
            Destroy(trans.gameObject);
        }

        foreach (Transform trans in desktopGrid)
        {
            Destroy(trans.gameObject);
        }
    }

    private void ToggleBootMenu()
    {
        ShowBootMenu(!isBootMenuVisible);
    }

    public void HideBootMenue()
    {
        if (!isBootMenuVisible)
            return;

        ShowBootMenu(false);
    }

    private void ShowBootMenu(bool isVisible)
    {
        if (isBootMenuVisible == isVisible)
            return;

        isBootMenuVisible = isVisible;
        Vector3 scale = isBootMenuVisible ? Vector3.one : new Vector3(1f, 0f, 1f);
        bootMenu.DOScale(scale, 0.25f);
    }

    protected override void BeforeHide()
    {
        OnVisibilityChange -= SwitchState;
        loginScreen.OnValidLogin -= OnValidLogin;

        if (IsSuspended)
        {
            return;
        }

        desktop.localScale = new Vector3(1f, 0f, 1f);
        bootTrans.gameObject.SetActive(false);
        blueScreen.Hide();

        if (null != interactable && interactable is Computer computer)
        {
            computer.StopPCNoise();
            computer.CurrentState = Computer.State.Off;
            computer.UpdateScreen();
        }
    }

    private void SwitchState()
    {
        if (IsSuspended || null == interactable || !(interactable is Computer computer)
            || computer.CurrentState == Computer.State.ShuttingDown)
            return;

        RestoreCurrentState();
        LoginData loginData = computer.CurrentUser;

        if (null == loginData && computer.CurrentState == Computer.State.Off
            || computer.CurrentState == Computer.State.LogIn)
        {
            ShowLogin(computer);
            return;
        }

        if (null != loginData && computer.CurrentState == Computer.State.Off)
        {
            Boot(computer);
        }
    }

    public void SetAppParent(PCApp app)
    {
        if (null == app)
            return;

        HideBootMenue();
        app.transform.SetParent(appParent, false);
        app.transform.SetAsLastSibling();
    }

    private void Recover()
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        Reboot(computer);
    }

    private void HideDesktop()
    {
        pcClock.pcClockConfig.Hide();
        ipV4ConfigDisplay.Hide();
        HideBootMenue();
        bootMenu.localScale = new Vector3(1f, 0f, 1f);
        desktopGrid.gameObject.SetActive(false);
        desktop.localScale = new Vector3(1f, 0f, 1f);
        desktopBackground.color = Color.clear;
        loginScreen.SetVisible(false, true);
        blueScreen.Hide();
    }

    public void UpdateConnectButton()
    {
        ipV4ConfigDisplay.UpdateConnectionInfo(false);
    }

    protected override void StoreCurrentState()
    {
        if (null == interactable)
            return;

        EntityData entityData = interactable.EntityData;
        entityData.SetAttribute("uiSuspended", IsSuspended ? "1" : "");

        if (!IsSuspended || null == interactable || !(interactable is Computer computer))
            return;

        pcClock.pcClockConfig.StoreCurrentState(entityData);
        ipV4ConfigDisplay.StoreCurrentState(entityData);
        List<string> appNames = new List<string>();
        List<string> virusNames = new List<string>();

        foreach (int id in computer.Infos.Keys)
        {
            PCAppInfo info = computer.Infos[id];
            PCApp app = info.App;
            Debug.Log(info);

            if (null != app)
            {
                app.StoreCurrentState(entityData);
                appNames.Add(app.appName);
            }
            else if (info.IsVirus)
            {
                virusNames.Add(info.AppName);
            }
        }

        entityData.SetAttribute("activeAppNames", string.Join(";", appNames));
        entityData.SetAttribute("activeVirusNames", string.Join(";", virusNames));
    }

    protected override void RestoreCurrentState()
    {
        if (null == interactable)
            return;

        EntityData entityData = interactable.EntityData;
        bool isSuspended = entityData.GetAttribute("uiSuspended").Equals("1");

        if (!isSuspended
            || null == interactable
            || !(interactable is Computer computer))
            return;

        computer.InitAppInfos(false);
        bootTrans.gameObject.SetActive(false);
        desktopGrid.gameObject.SetActive(true);
        desktop.localScale = new Vector3(1f, 1f, 1f);
        desktopBackground.color = Color.white;
        loginScreen.SetVisible(false, true);
        blueScreen.Hide();
        HideBootMenue();
        pcCursor.SetVisible(true);
        suspendButton.SetState(0);
        UpdateConnectButton();
        computer.ShowAppIcons(desktopGrid, taskBar);

        pcClock.pcClockConfig.RestoreCurrentState(entityData);
        ipV4ConfigDisplay.RestoreCurrentState(entityData);

        string activeAppNamesStr = entityData.GetAttribute("activeAppNames");
        string activeVirusNamesStr = entityData.GetAttribute("activeVirusNames");

        if (!string.IsNullOrEmpty(activeAppNamesStr))
        {
            string[] appNames = activeAppNamesStr.Split(';');
            computer.RestoreApps(appNames);
        }

        if (!string.IsNullOrEmpty(activeVirusNamesStr))
        {
            string[] virusNames = activeVirusNamesStr.Split(';');
            computer.RestoreViruses(virusNames);
        }

        foreground.DOColor(Color.clear, 1f);

        entityData.SetAttribute("uiSuspended");
        computer.CurrentState = Computer.State.Running;
    }

    protected override void Suspend()
    {
        Computer computer = (Computer)interactable;
        computer?.UpdateScreen();
        base.Suspend();
        GameEvent.GetInstance().Execute(() =>
        {
            computer?.CloseAllApps();
            pcCursor.SetVisible(false);
            HideBootMenue();
            UIGame.GetInstance().HideCursor(false);
        }, 0.5f);
    }

    private void LogOff()
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        HideDesktop();
        computer.LogOff();
        SwitchState();
    }

    private void ShowLogin(Computer computer)
    {
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlaySound("pc.beep", computer.gameObject);
        computer.PCNoise();

        DOTween.Sequence().
            SetAutoKill(true).
            SetDelay(2f).
            OnComplete(() =>
            {
                computer.StopPCNoise();
                pcCursor.SetVisible(true);
                loginScreen.SetVisible(true);
            }).
            Play();
    }

    private void OnValidLogin(LoginData loginData)
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        computer.LogIn(loginData);
        pcCursor.SetVisible(false);
        GameEvent.GetInstance().Execute(SwitchState, 1f);
    }

    private void Reboot()
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        Reboot(computer);
    }

    public void Reboot(Computer computer)
    {
        computer.CurrentState = Computer.State.Rebooting;
        computer.CloseAllApps();
        computer.PCNoise();
        HideDesktop();
        GameEvent.GetInstance().Execute<Computer>(Boot, computer, 3f);
    }

    private void Boot(Computer computer)
    {
        computer.CurrentState = Computer.State.Booting;

        if (null != ieAnim)
            return;

        computer.InitAppInfos(computer.HasVirus);
        ieAnim = IEBoot(computer);
        StartCoroutine(ieAnim);
    }

    public void Shutdown()
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        computer.CurrentState = Computer.State.ShuttingDown;
        ieAnim = IEShutdown(computer);
        StartCoroutine(ieAnim);
    }

    private IEnumerator IEBoot(Computer computer)
    {
        suspendButton.SetState(-1);
        HideDesktop();
        ipV4ConfigDisplay.Init(computer);

        yield return null;

        bootTrans.gameObject.SetActive(false);
        bootPercentDisplay.SetText("");
        bootBar.color = Color.blue;

        string percent;
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlaySound("pc.beep", computer.gameObject);

        yield return new WaitForSecondsRealtime(0.5f);

        computer.PCNoise();

        yield return new WaitForSecondsRealtime(0.5f);

        float zz;
        float f = 0f;
        float maxValue = 1f;

        if (computer.HasVirus)
        {
            maxValue = Random.Range(0.8f, 2f);
            SetVirus(computer);
        }

        bootBar.rectTransform.localScale = new Vector3(0f, 1f, 1f);
        bootImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        bootTrans.gameObject.SetActive(true);

        while (f <= maxValue)
        {
            zz = Mathf.Max(0.2f, Random.value * (1f - f));

            if (zz > 0f)
            {
                DOTween.Sequence().
                    SetAutoKill(true).
                    Append(bootImage.rectTransform.DOLocalRotate(new Vector3(0f, 0f, -30f), zz * 0.4f).SetEase(Ease.Linear)).
                    Append(bootImage.rectTransform.DOLocalRotate(new Vector3(0f, 0f, 30f), zz * 0.6f).SetEase(Ease.Linear)).
                    Play();

                yield return new WaitForSecondsRealtime(zz);

                computer.PCNoise();
            }

            bootBar.rectTransform.DOScale(new Vector3(f, 1f, 1f), 0.5f);
            percent = Mathf.Round(Mathf.Min(f, maxValue) * 100f) + " %";
            bootPercentDisplay.SetText(percent);
            f += Time.deltaTime * Random.Range(0.5f, 2f);

            yield return null;

            if (computer.HasVirus && f < 1f && Random.value > 0.75f)
            {
                f *= Random.Range(0.85f, 1.25f);

                if (Random.value > 0.75f)
                {
                    bootBar.color = Random.ColorHSV();
                }
            }
        }

        percent = Mathf.Round(maxValue * 100f) + " %";
        bootPercentDisplay.SetText(percent);
        bootBar.rectTransform.localScale = new Vector3(maxValue, 1f, 1f);

        zz = Random.value + 0.25f;
        bootImage.rectTransform.DOLocalRotate(Vector3.zero, zz);

        yield return new WaitForSecondsRealtime(zz);

        float duration = audioManager.GetSoundLength("pc.boot");
        audioManager.PlaySound("pc.boot", computer.gameObject);
        desktop.localScale = Vector3.one;
        desktopBackground.DOColor(Color.white, duration);

        yield return new WaitForSecondsRealtime(duration);

        desktopGrid.gameObject.SetActive(true);
        computer.ShowAppIcons(desktopGrid, taskBar);

        bootTrans.gameObject.SetActive(false);
        computer.StopPCNoise();

        yield return null;

        UpdateConnectButton();
        pcCursor.SetVisible(true);
        computer.CurrentState = Computer.State.Running;
        ieAnim = null;
        computer.UpdateScreen();
        suspendButton.SetState(0);
    }

    public void SetVirus(Computer computer)
    {
        pcCursor.SetInfected(computer.HasVirus);
        pcClock.IsInfected = computer.HasVirus;

        if (computer.HasVirus)
        {
            GameEvent.GetInstance().Execute<Computer>(Flash, computer, Random.Range(1f, 5f));
        }
        else
        {
            pcClock.ResetTime();
        }
    }

    private void Flash(Computer computer)
    {
        if (!computer.HasVirus || computer.CurrentState == Computer.State.Off)
          return;

        float zz = 0.25f * (1f + Random.value);
        AudioManager.GetInstance().PlaySound("electric", computer.gameObject, 2f + zz);
        foreground.sprite = CATex.GetSprites(Random.value > 0.75f);
        foreground.color = Random.value > 0.5f ? Color.white : Random.ColorHSV();
        foreground.DOColor(Color.clear, zz);
        desktopBackground.color = Random.value > 0.5f ? Color.gray : Random.ColorHSV();
        desktopBackground.DOColor(Color.white, Random.Range(2f, 4f) * zz);
        pcClock.SetTime(Random.Range(0,100), Random.Range(0, 100), Random.Range(0, 100));

        if (Random.value > 0.9f && computer.IsRunning)
        {
            desktop.localScale = new Vector3(
                Random.Range(1.1f, 1.25f),
                Random.Range(1.1f, 1.25f),
                1f
            );
            desktop.localRotation = Quaternion.Euler(
                new Vector3(0f, 0f, Random.Range(-15f, 15f))
            );
            zz = Random.Range(0.25f, 1f);
            flashSeq = DOTween.Sequence().
                SetAutoKill(false).
                Append(
                    desktop.DOScale(Vector3.one, zz)
                ).
                Join(
                    desktop.DOLocalRotate(Vector3.zero, zz).
                    SetEase(Ease.Flash)
                ).
                OnComplete(() => {
                    ClearFlashSequence();
                }).
                Play();
            
        }

        zz = Random.Range(1f, 15f);
        pcCursor.SetMoveable(false, Random.value > 0.75f, zz);
        GameEvent.GetInstance().Execute<Computer>(Flash, computer, zz);
    }

    private void ClearFlashSequence()
    {
        if (null != flashSeq)
        {
            flashSeq.Pause();
            flashSeq.Kill(false);
        }

        flashSeq = null;
    }

    private IEnumerator IEShutdown(Computer computer)
    {
        pcCursor.SetVisible(false);
        suspendButton.SetState(-1);

        AudioManager audioManager = AudioManager.GetInstance();
        computer.PCNoise();

        yield return new WaitForSecondsRealtime(1f);

        if (null != computer.CurrentApp)
        {
            PCApp app = computer.CurrentApp;
            app.Hide();

            while (!app.IsReady)
                yield return new WaitForSecondsRealtime(0.25f);
        }

        computer.CloseAllApps();

        yield return null;
        
        desktop.DOScale(new Vector3(1f, 0f, 1f), 0.5f);

        yield return new WaitForSecondsRealtime(1.5f);

        Hide();

        yield return new WaitForSecondsRealtime(0.5f);

        computer.ResetScreen();
        computer.StopPCNoise();

        yield return new WaitForSecondsRealtime(0.5f);

        audioManager.PlaySound("electric", computer.gameObject, 1f + Random.value * 0.125f);

        yield return new WaitForSecondsRealtime(1f);

        computer.CurrentState = Computer.State.Off;
        ieAnim = null;
    }

    public void ShowBlueScreen(bool quickDown, bool isInteractive)
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        computer.PCNoise();
        GameEvent.GetInstance().Execute<bool>(
            CrashDown,
            isInteractive,
            quickDown ? 2f : 5f
        );
    }

    private void CrashDown(bool isInteractive)
    {
        if (null == interactable || !(interactable is Computer computer))
            return;

        Flash(computer);
        computer.Beep();
        computer.CloseAllApps();
        ClearAppParent();
        pcCursor.SetVisible(false);
        blueScreen.Show(isInteractive);
        computer.UpdateScreen();

        if (isInteractive)
            return;

        computer.Electric();

        Shutdown();
        computer.Crashed = false;
    }

    private void Update()
    {
        if (!blueScreen.IsInteractive)
            return;

        if (Input.anyKey)
            Recover();
    }
}
