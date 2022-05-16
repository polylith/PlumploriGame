using UnityEngine;
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
    public enum Category {
        System,
        App,
        Game
    }

    public string AppName { get => GetAppName(); }
    public bool IsReady { get; private set; }
    public bool IsInfected { get => isInfected; }
    public bool IsEnabled { get => isEnabled; }
    public bool IsActive { get => isActive; }
    public bool HasId { get => id > -1; }
    public int Id { get => id; set => SetId(value); }

    public Category category;
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

    /// <summary>
    /// <para>
    /// Stores the current state of the pc app.
    /// This methods needs to be overriden.
    /// </para>
    /// <para>
    /// The base method need to be called after all
    /// specific data are stored.
    /// </para>
    /// </summary>
    /// <param name="entityData"></param>
    public virtual void StoreCurrentState(EntityData entityData)
    {
        entityData.SetAttribute(appName + ".isActive", isActive ? "1" : "");
        entityData.SetAttribute(appName + ".isInfected", isInfected ? "1" : "");
    }

    /// <para>
    /// Restores the current state of the pc app.
    /// This methods needs to be overriden.
    /// </para>
    /// <para>
    /// The base method need to be called after all
    /// specific data are restored.
    /// </para>
    public virtual void RestoreCurrentState(EntityData entityData)
    {
        bool isActive = entityData.GetAttribute(appName + ".isActive").Equals("1");
        SetActive(isActive);

        bool isInfected = entityData.GetAttribute(appName + ".isInfected").Equals("1");
        SetInfected(isInfected);

        entityData.Clear(appName);
    }

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

    private string GetAppName()
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

    /// <summary>
    /// This method is called on closing an app.
    /// The specific application should clean up here.
    /// Calling the base method is not needed because
    /// nothing will happen there.
    /// </summary>
    public virtual void ResetApp()
    {
        // nothing to do here
    }

    public void SetActive(bool isActive, bool instant = false)
    {
        if (this.isActive == isActive)
            return;

        this.isActive = isActive;
        IsReady = false;
        Vector3 scale = isActive ? Vector3.one : Vector3.zero;

        if (isActive)
        {
            ShowAppTitle();
            PreCall();
        }

        if (instant)
        {
            transform.localScale = scale;
            IsReady = true;
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
    /// of a specific app. Doesn't need to be called
    /// in overridden versions.
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
        IsReady = true;
    }
}