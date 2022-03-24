﻿using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// <para>
/// The abstract class PCApp provides all generic attributes,
/// functions and methods for a computer program with a graphical UI.
/// </para>
/// <para>
/// The concrete programs inherit from this class and implement their
/// specific functionality.
/// </para>
/// <para>
/// The procedure of creating new programs is similar to the procedure
/// for designing interactable UIs. 
/// </para>
/// <para>
/// In the Prefabs UI asset folder there is a template for a PCApp.
/// This template can be reused. The elements in it may be removed or
/// changed as well. However, it is important not to apply these changes
/// to the original prefab, otherwise they will be applied to all
/// existing PCApps.
/// </para>
/// <para>
/// Also important is that the new PCApp must be saved as a prefab variant.
/// </para>
/// </summary>
public abstract class PCApp : MonoBehaviour
{
    public bool IsInfected { get => isInfected; }
    public bool IsEnabled { get => isEnabled; }
    public bool IsActive { get => isActive; }
    public bool IsReady { get => isReady; }
    public bool HasId { get => id > -1; }
    public int Id { get => id; set => SetId(value); }

    public Transform appContent;
    public string appName = "";
    public Sprite icon;
    public PCIcon pcIcon;
    public TextMeshProUGUI appTitle;
    public UIIconButton closeButton;
    public UIIconButton hideButton;

    protected bool isInfected;
    private bool isActive = true;
    private IEnumerator ieScale;
    private bool iconInited;
    protected Computer computer;
    private bool isReady;
    private bool isEnabled = true;
    private int id = -1;

    private void Awake()
    {
        Init();
    }

    public abstract List<Formula> GetGoals();

    /// <summary>
    /// This method specifies an incorrect behavior of
    /// an app when the computer is infected with a virus.
    /// </summary>
    protected abstract void Effect();

    public virtual void SetInfected(bool isInfected)
    {
        if (this.isInfected == isInfected)
            return;

        this.isInfected = isInfected;
        Vector3 rot = Vector3.zero;

        if (isInfected)
        {
            ShowAppTitle();
            rot = new Vector3(0f, (Random.Range(0, 100) % 2) * 180f, 
                (Random.Range(0, 100) % 2) * 180f);
            Effect();
        }

        appContent.localRotation = Quaternion.Euler(rot);
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    protected virtual void Init()
    {
        pcIcon.SetApp(this);
        pcIcon.SetIcon(icon);

        if (null != closeButton)
            closeButton.SetAction(Close);

        if (null != hideButton)
        {
            hideButton.SetAction(Hide);
            hideButton.SetActiveColor(Color.black);
        }

        SetActive(false, true);
    }

    public abstract List<string> GetAttributes();
    public abstract Dictionary<string, System.Action<bool>> GetDelegates();

    public void SetEnabled(bool isEnabled)
    {
        if (this.isEnabled == isEnabled)
            return;

        this.isEnabled = isEnabled;
        computer?.SetAppEnabled(this, isEnabled);
    }

    public void SetPC(Computer computer)
    {
        this.computer = computer;
    }

    protected void ShowAppTitle()
    {
        string text = GetAppName();

        if (IsInfected)
            text = StringUtil.ToLeet(text);

        appTitle.SetText(text);
    }

    public string GetAppName()
    {
        return Language.LanguageManager.GetText(appName);
    }

    public PCIcon GetAppIcon()
    {
        if (!iconInited)
        {
            pcIcon = Instantiate(pcIcon) as PCIcon;
            pcIcon.SetIcon(icon);
            pcIcon.SetApp(this);
            pcIcon.name = GetAppName() + " (Desktop Icon)";
            iconInited = true;
        }

        return pcIcon;
    }

    private void ShowIconInTaskBar(bool mode)
    {
        PCIcon icon = GetAppIcon();

        if (null != icon)
            icon.ShowInTaskBar(mode);
    }

    public void Hide()
    {
        Hide(false);
    }

    public void Hide(bool instant)
    {
        if (!isActive)
            return;

        SetActive(false, instant);
        computer?.SetCurrentApp(null);        
    }

    public void Show()
    {
        computer?.SetCurrentApp(this);
        SetActive(true);
        ShowIconInTaskBar(true);
    }

    public void Close()
    {
        Close("");
    }

    public void Close(string arg)
    {
        Close(false);

        if (IsInfected)
            computer.StartVirus();
    }

    public virtual void Close(bool instant)
    {
        Hide(instant);
        ShowIconInTaskBar(false);
        computer?.RemoveAppId(id);
        SetId(-1);
        ResetApp();
    }

    public virtual void ResetApp()
    {
        // nothing to do here
    }

    public void SetActive(bool isActive, bool instant = false)
    {
        if (this.isActive == isActive)
            return;

        isReady = false;
        this.isActive = isActive;
        Vector3 scale = isActive ? Vector3.one : Vector3.zero;

        if (isActive)
        {
            ShowAppTitle();
            PreCall();
        }

        if (instant)
        {
            transform.localScale = scale;
            isReady = true;
        }
        else
        {
            if (null != ieScale)
                StopCoroutine(ieScale);

            if (isActive && isInfected)
                Effect();

            ieScale = IEScale(scale);
            StartCoroutine(ieScale);
        }
    }

    /// <summary>
    /// This method can be used to setup a start state
    /// of a specific app.
    /// </summary>
    protected virtual void PreCall()
    {
        // nothing to do here
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0f;

        while (f <= 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, f);
            yield return null;
            f += Time.deltaTime;
        }

        transform.localScale = scale;
        isReady = true;
    }
}