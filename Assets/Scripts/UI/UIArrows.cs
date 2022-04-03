using UnityEngine;
using DG.Tweening;

/// <summary>
/// This class provides clickable arrows. The buttons don't have
/// any actions to perform on click. On show the caller needs to
/// provide the specific actions to perform for each button.
/// </summary>
public class UIArrows : MonoBehaviour
{
    private static UIArrows ins;

    public static UIArrows GetInstance()
    {
        return ins;
    }

    public bool IsCurrentlyInUse { get => isVisible; }

    public UIButton upButton;
    public UIButton downButton;
    public UIButton leftButton;
    public UIButton rightButton;

    private bool isVisible;
    private object owner;

    private bool isUpButtonActive;
    private bool isDownButtonActive;
    private bool isLeftButtonActive;
    private bool isRightButtonActive;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            isVisible = true;
            Hide(null, true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetActions(
        System.Action upButtonAction = null,
        System.Action downButtonAction = null,
        System.Action leftButtonAction = null,
        System.Action rightButtonAction = null
    )
    {
        upButton.SetAction(upButtonAction);
        downButton.SetAction(downButtonAction);
        leftButton.SetAction(leftButtonAction);
        rightButton.SetAction(rightButtonAction);

        isUpButtonActive = null != upButtonAction;
        isDownButtonActive = null != downButtonAction;
        isLeftButtonActive = null != leftButtonAction;
        isRightButtonActive = null != rightButtonAction;
    }

    private void SetVisible(bool isVisible, bool instant)
    {
        Vector3 scale = isVisible ? Vector3.one : Vector3.zero;
        this.isVisible = isVisible;

        if (instant)
        {
            transform.localScale = scale;
            return;
        }

        transform.DOScale(scale, 0.35f);
    }

    /// <summary>
    /// <para>
    /// To show the arrows a specific action needs to be specified for each
    /// arrow button. Providing null for a button action will hide this button
    /// if it is not needed.
    /// </para>
    /// <para>
    /// When ui arrows are already in use, nothing will happend.
    /// </para>
    /// <para>
    /// Optionally each arrow button may get a specific tooltip.
    /// </para>
    /// <para>
    /// Providing null as tooltip will not show any tooltip for a specific
    /// arrow button.
    /// </para>
    /// </summary>
    /// <returns>true when arrows are currently not in use</returns>
    public bool Show(
        object owner,
        System.Action upButtonAction, System.Action downButtonAction,
        System.Action leftButtonAction, System.Action rightButtonAction,
        string upButtonToolTip = null, string downButtonToolTip = null,
        string leftButtonToolTip = null, string rightButtonToolTip = null
    )
    {
        if (isVisible)
            return false;

        this.owner = owner;
        SetActions(
            upButtonAction,
            downButtonAction,
            leftButtonAction,
            rightButtonAction
        );
        SetToolTips(
            upButtonToolTip,
            downButtonToolTip,
            leftButtonToolTip,
            rightButtonToolTip
        );
        SetVisible(true, false);
        return true;
    }

    public void Hide(object owner, bool instant = false)
    {
        if (!isVisible || this.owner != owner)
            return;

        SetVisible(false, instant);
        SetActions();
        SetToolTips(null, null, null, null);
        this.owner = null;
    }

    /// <summary>
    /// Each arrow button may get a specific tooltip.
    /// Providing null as tooltip will not show any tooltip for
    /// a specific arrow button.
    /// </summary>
    private void SetToolTips(
        string upButtonToolTip,
        string downButtonToolTip,
        string leftButtonToolTip,
        string rightButtonToolTip)
    {
        upButton.SetToolTip(upButtonToolTip);
        downButton.SetToolTip(downButtonToolTip);
        leftButton.SetToolTip(leftButtonToolTip);
        rightButton.SetToolTip(rightButtonToolTip);
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (isUpButtonActive && Input.GetKeyDown(KeyCode.UpArrow))
        {
            upButton.Click();
            return;
        }

        if (isDownButtonActive && Input.GetKeyDown(KeyCode.DownArrow))
        {
            downButton.Click();
            return;
        }

        if (isLeftButtonActive && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            leftButton.Click();
            return;
        }

        if (isRightButtonActive && Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightButton.Click();
            return;
        }
    }
}
