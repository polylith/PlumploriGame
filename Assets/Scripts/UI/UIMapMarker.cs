using UnityEngine;
using UnityEngine.UI;

public class UIMapMarker : MonoBehaviour
{
    public bool IsCurrent { get => isCurrent; set => SetCurrent(value); }
    public bool IsVisible { get => isVisible; set => SetVisible(value); }

    public Image position;
    public Image marker;

    private bool isCurrent;
    private bool isVisible;
    
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
