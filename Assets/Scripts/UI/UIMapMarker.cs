using UnityEngine;
using UnityEngine.UI;

public class UIMapMarker : MonoBehaviour
{
    public bool IsCurrent { get => isCurrent; set => SetCurrent(value); }
    public bool IsVisible { get => isVisible; set => SetVisible(value); }

    public RectTransform rectTransform;
    public Image position;
    public Image marker;

    private bool isCurrent;
    private bool isVisible;

    public void SetPosition(Vector3 position)
    {
        rectTransform.position = position;
        rectTransform.localScale = Vector3.one;
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetCurrent(bool isCurrent)
    {
        if (this.isCurrent == isCurrent)
            return;

        this.isCurrent = isCurrent;
        UpdateIcons();
    }

    private void SetVisible(bool isVisible)
    {
        this.isVisible = isVisible;
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        position.gameObject.SetActive(IsVisible && !IsCurrent);
        marker.gameObject.SetActive(IsVisible && IsCurrent);
    }

}
