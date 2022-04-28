using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FourInARowColumnSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public delegate void OnColumnSelectEvent(int index);
    public event OnColumnSelectEvent OnColumnSelect;

    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }

    public Image arrow;
    public Image back;
    public Outline outline;
    public int columnIndex;

    private bool isEnabled = true;

    private void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        SetState(false);
        back.raycastTarget = isEnabled;
    }

    private void SetState(bool state)
    {
        arrow.color = state ? new Color(1f, 0.8f, 0f) : Color.clear;
        back.color = state ? new Color(1f, 1f, 0f, 0.25f) : Color.clear;
        outline.effectColor = state ? new Color(1f, 0f, 0f, 0.25f) : Color.clear;
    }

    private void Start()
    {
        SetEnabled(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEnabled)
            return;

        SetState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsEnabled)
            return;

        SetState(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsEnabled)
            return;

        OnColumnSelect?.Invoke(columnIndex);
        SetState(false);
    }
}
