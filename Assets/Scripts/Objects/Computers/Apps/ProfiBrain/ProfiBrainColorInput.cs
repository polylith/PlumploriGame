using UnityEngine;
using UnityEngine.EventSystems;

public class ProfiBrainColorInput : ProfiBrainColorDot, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static int CurrentSelectedColorIndex { get => null != currentSelectedColorInput ? currentSelectedColorInput.colorIndex : -1; }
    private static ProfiBrainColorInput currentSelectedColorInput;
        
    public bool IsEnabled { get; set; }

    public string soundIdToken = "click2";

    public System.Action OnColorInput;

    public void Select()
    {
        SetColorIndex();
    }

    protected virtual void SetColorIndex()
    {
        if (null != currentSelectedColorInput)
        {
            ProfiBrainColorInput tmpColorInput = currentSelectedColorInput;
            currentSelectedColorInput = null;
            tmpColorInput.Hightlight(false);
        }

        currentSelectedColorInput = this;
        activeScale = Vector3.one;
    }

    protected override void SetColorIndex(int colorIndex)
    {
        base.SetColorIndex(colorIndex);
        Hightlight(false);
    }

    private void Hightlight(bool mode)
    {
        if (currentSelectedColorInput == this)
            return;

        activeScale = mode ? Vector3.one : currentScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsEnabled)
            return;

        SetColorIndex();

        if (null != soundIdToken)
        {
            AudioManager.GetInstance().PlaySound(soundIdToken);
        }

        OnColorInput?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEnabled)
            return;

        Hightlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsEnabled)
            return;

        Hightlight(false);
    }
}
