using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public abstract class UIButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static Color[] colors = new Color[] {
        Color.white,
        Color.yellow,
        Color.blue
    };

    public Vector3[] localScales;
    public bool hoverScale = true;

    public bool HasClickAction { get => null != clickAction; }
    public bool IsBlocked { get => isBlocked; }
    public int State { get => state; }
    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }
    public System.Action OnClick;
    public EventTrigger.TriggerEvent action;
    public Image background;
    public string soundID = "click";
    public bool changeColor = true;

    private System.Action clickAction;
    private int state = 1;
    public bool isExclusive;

    private bool isBlocked;
    private bool isEnabled = true;
    private string tooltip;

    private void Start()
    {
        if (state > 1)
            SetState(2);
        else if (state > -1)
            SetState(0);
    }

    public void Block(bool isBlocked)
    {
        this.isBlocked = isBlocked;
    }

    public void SetToolTip(string s)
    {
        tooltip = s;
    }

    public void SetAction(System.Action action)
    {
        if (null != this.action)
        {
            this.action.RemoveAllListeners();
        }

        if (null != action)
        {
            this.action.AddListener((eventData) => { action(); });
        }
    }

    public void SetClickAction(System.Action newAction)
    {
        if (null != newAction)
            clickAction = newAction;
        else
            SetState(-1);
    }

    public virtual void SetState(int state)
    {
        if (this.state == state)
            return;

        this.state = state;
        Vector3 scale1 = Vector3.one;
        Vector3 scale2 = Vector3.one * 1.25f;

        if (state > -1)
        {
            if (changeColor || !IsEnabled)
                background.color = IsEnabled ? colors[this.state] : new Color(0.75f, 0.75f, 0.75f);

            if (state < localScales.Length)
            {
                scale1 = localScales[state];
                scale2 = scale1 * 1.25f;
            }
        }
        else
            scale1 = Vector3.zero;

        if (gameObject.activeSelf && hoverScale)
        {
            DOTween.Sequence()
                .SetAutoKill(true)
                .Append(transform.DOScale(scale2, 0.125f))
                .Append(transform.DOScale(scale1, 0.25f))
                .Play();
        }
        else
            transform.localScale = scale1;
    }

    protected virtual void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        SetState(0);
    }

    private void ShowToolTip(bool visible)
    {
        if (null == tooltip)
        {
            return;
        }

        TextMeshProUGUI tmpTextMesh = UIToolTip.TmpTextMesh;

        if (null != tmpTextMesh)
        {
            tmpTextMesh.SetText(visible ? tooltip : "");
            return;
        }

        UIToolTip uiToolTip = UIToolTip.GetInstance();

        if (visible)
        {
            uiToolTip.SetText(tooltip, 1);
            uiToolTip.Show(transform);
        }
        else
        {
            uiToolTip.Hide();
            uiToolTip.ClearText();
            uiToolTip.ResetColor();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Click())
            AudioManager.GetInstance().PlaySound(soundID);
    }

    public bool Click()
    { 
        if (isExclusive && state == 2 || isBlocked || !isEnabled)
            return false;

        SetState(2);

        if (null != clickAction)
            clickAction.Invoke();
        else if (null != action)
            GameEvent.GetInstance().Execute(TriggerEvent, 0.5f);        

        if (!isExclusive)
            SetState(0);

        if (null != OnClick)
            GameEvent.GetInstance().Execute(OnClick, 0.75f);

        UIGame.GetInstance().SetCursorEnabled(false, false);
        ShowToolTip(false);
        return true;
    }

    private void TriggerEvent()
    {
        action?.Invoke(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isExclusive && state == 2 || isBlocked || !isEnabled)
            return;

        UIGame.GetInstance().SetCursorEnabled(true, false);
        SetState(1);
        ShowToolTip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isExclusive && state == 2 || isBlocked || !isEnabled)
            return;

        UIGame uiGame = UIGame.GetInstance();
        bool mode = !uiGame.IsCursorOverUI && !uiGame.IsUIExclusive;
        uiGame.RestoreCursor();
        uiGame.SetCursorEnabled(false, mode);
        SetState(0);
        ShowToolTip(false);
    }
}