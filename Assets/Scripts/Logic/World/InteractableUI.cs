using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// <para>
/// Some interactive objects might require their own UI
/// to improve interaction with these objects, because
/// these object may be too small or require specific
/// types of interaction that are not covered by the
/// standard action management.
/// </para>
/// <para>
/// This class provides all the generic attributes,
/// functions and methods for the UI of an Interactable
/// to show and hide, switch the current interactable and
/// handling of the pointer behaviour.
/// All the specific functionality of the UI needs to be
/// implemented in a deriving class.
/// </para>
/// <para>
/// An InteractableUI must always be set up as a prefab.
/// At runtime of the game it will be checked if a concrete
/// instance already exists to reuse it and avoid multiple
/// copies of the same UI.
/// </para>
/// <para>
/// See the InteractableUI source code for further remarks.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// The best way to create a specific InteractableUI
/// would go like this:
/// </para>
/// <list type="number">
/// <item>Create an empty GameObject
/// in the canvas of the UIGame in Tmp GUI Parent and
/// name it according to the interactable for which
/// the UI should be.
/// e. g. Digital Clock UI</item>
/// <item>Set RectTransform to stretch in both directions
/// and set left, top, right, bottom to 0.
/// The pivot values should be chosen according to the
/// appearance of the UI panel. e. g. (0.5, 0.0)</item>
/// <item>In this empty parent element the
/// actual UI can be designed "easily" in position and
/// size.</item>
/// <item>Next, the script for the specific UI can be
/// created, it has to inherit from InteractableUI and
/// implement all the requirements of this abstract
/// class.</item>
/// <item>This script is placed as a component on the
/// empty parent element.</item>
/// <item>Now all required components need to be created
/// in the UI and linked in the inspector on the parent
/// element.</item>
/// <item>Afterwards, the UI can be created as a prefab.
/// This is done by dragging the parent element into a
/// directory in the Unity UI.</item>
/// <item>In the inspector of the specific interactable,
/// the prefab must be linked in the interactableUIPrefab
/// field.
/// IMPORTANT:
/// <list type="bullet">
/// <item>Link the prefab object in the directory,
/// not the one in the object tree, because this must be
/// removed at the end to keep the UI compact.</item>
/// <item>If the specific interactable is already a prefab,
/// the link must be added to its prefab.</item>
/// </list>
/// </item>
/// <item>After that, the functionality of the specific UI
/// can be developed step by step and tested in play mode.</item>
/// </list>
/// <para>
/// If the interactable belongs to an own namespace,
/// this namespace should also be used for the specific
/// UI class.
/// </para>
/// <para>
/// The script for the specific UI should be placed in
/// the same asset folder in Unity as the script for the
/// specific Interactable class.
/// For example:
/// </para>
/// <list type="bullet">
/// <item>Assets/Scripts/Objects/Clock/DigitalClock.cs</item>
/// <item>Assets/Scripts/Objects/Clock/DigitalClockUI.cs</item>
/// </list>
///
/// <para>
/// The UI prefab should be placed in the according prefab folder.
/// For example:
/// </para>
/// <list type="bullet">
/// <item>Assets/Prefabs/UI/Interactables/Digital Clock UI.prefab</item>
/// </list>
///
/// <para>
/// Also consider adding some documentation and code commenting!
/// </para>
/// </remarks>
public abstract class InteractableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly Dictionary<string, InteractableUI> uiMap = new Dictionary<string, InteractableUI>();

    public static InteractableUI GetInstance(InteractableUI interactableUIPrefab)
    {
        string type = interactableUIPrefab.GetType().ToString();

        if (!uiMap.ContainsKey(type))
        {
            uiMap.Add(type, Instantiate<InteractableUI>(interactableUIPrefab));
        }

        return uiMap[type];
    }

    /// <summary>
    /// Checks if there are any open interactive uis for
    /// and hide all other except the calling ui.
    /// </summary>
    /// <param name="excludeUI">ui to exclude from closing</param>
    public static void CloseOtherActiveUIs(InteractableUI excludeUI)
    {
        foreach (InteractableUI ui in uiMap.Values)
        {
            if (ui != excludeUI)
            {
                // TODO maybe even remove
                ui.Hide();
            }
        }
    }

    /// <summary>
    /// Checks if there are any open interactive uis for
    /// non-collectable interactables and hide them.
    /// </summary>
    public static void CloseActiveUIs()
    {
        foreach (InteractableUI ui in uiMap.Values)
        {
            if (!ui.IsCollectable && ui.IsVisible)
            {
                // TODO maybe even remove
                ui.Hide();
            }
        }
    }

    /// <summary>
    /// Checks if there are any open interactive uis for
    /// non-collectable interactables and hide them.
    /// </summary>
    public static void CloseAllUIs()
    {
        foreach (InteractableUI ui in uiMap.Values)
        {
            // TODO maybe even remove
            ui.Hide();
        }
    }

    public string soundId = "";
    public bool uiExclusiveMode;
    public RectTransform uiParent;
    public TextMeshProUGUI headLine;
    public TextMeshProUGUI statusLine;
    public UIIconButton closeButton;

    public delegate void OnVisibilityChangeEvent();
    public event OnVisibilityChangeEvent OnVisibilityChange;

    public bool IsCollectable { get => null != interactable && interactable is Collectable; }
    public bool IsVisible { get => isVisible; }
    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }

    protected Interactable interactable;
    protected bool isEnabled = true;
    protected IEnumerator ieScale;

    private bool isVisible = true;

    private void Awake()
    {
        if (null != statusLine)
        {
            statusLine.SetText("");
        }

        Scale(false, true);
        closeButton.SetAction(Hide);
    }

    protected abstract void Initialize();
    protected abstract void BeforeHide();

    public void SetInteractable(Interactable interactable)
    {
        this.interactable = interactable;

        if (null == interactable)
            return;

        SetHeadLine(interactable.GetText());
        Initialize();
    }

    public void SetHeadLine(string title)
    {
        if (null == headLine)
            return;

        headLine.SetText(title);
    }

    public void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;
    }

    public void Show()
    {
        UIGame.GetInstance().AddTmpGUI(uiParent.gameObject);

        if (null != statusLine)
        {
            UIToolTip.TmpTextMesh = statusLine;
        }

        Initialize();
        Scale(true);
    }

    public void Hide()
    {
        BeforeHide();

        if (null != statusLine)
        {
            UIToolTip.TmpTextMesh = null;
        }

        Scale(false);
    }

    private void Scale(bool isVisible, bool instant = false)
    {
        if (this.isVisible == isVisible)
            return;

        UIGame uiGame = UIGame.GetInstance();

        if (null != uiGame)
        {
            uiGame.IsUIExclusive = uiExclusiveMode && isVisible;

            if (uiExclusiveMode)
            {
                uiGame.SetCursorEnabled(false, false);
                uiGame.SetOverUI(isVisible);
            }
            else
            {
                uiGame.SetCursorEnabled(false, true);
            }
        }

        this.isVisible = isVisible;
        Vector3 scale = isVisible ? Vector3.one : new Vector3(1f, 0f, 1f);

        if (instant)
        {
            uiParent.localScale = scale;
        }
        else
        {
            if (null != ieScale)
                StopCoroutine(ieScale);

            if (isVisible && null != soundId && soundId.Length > 0)
                AudioManager.GetInstance().PlaySound(soundId);

            ieScale = IEScale(scale);
            StartCoroutine(ieScale);
        }
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0f;

        while (f <= 1f)
        {
            uiParent.localScale = Vector3.Lerp(uiParent.localScale, scale, f);
            f += Time.deltaTime;
            yield return null;
        }

        yield return null;

        uiParent.localScale = scale;
        OnVisibilityChange?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIGame.GetInstance().SetCursorEnabled(false, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIGame.GetInstance().SetCursorEnabled(false, !uiExclusiveMode);
    }
}
