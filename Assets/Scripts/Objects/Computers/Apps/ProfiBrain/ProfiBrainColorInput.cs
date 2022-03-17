using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ProfiBrainColorInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static int CurrentSelectedColorIndex { get => null != currentSelectedColorInput ? currentSelectedColorInput.colorIndex : -1; }
    private static ProfiBrainColorInput currentSelectedColorInput;

    public static readonly Color[] colors = new Color[] {
        Color.red, Color.blue, Color.yellow, Color.green
    };
    public static readonly Color emptyColor = new Color(0.2f, 0.2f, 0.2f);

    public int ColorIndex { get => colorIndex; set => SetColorIndex(value); }

    public int index;
    public Image image;

    private int colorIndex = -1;
    private Vector3 currentScale = new Vector3(0.5f, 0.5f, 1f);
    private Vector3 activeScale = new Vector3(0.5f, 0.5f, 1f);

    private void Start()
    {
        SetColorIndex(index);
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

protected void SetColorIndex(int colorIndex)
    {
        colorIndex = Mathf.Max(-1, colorIndex);

        if (this.colorIndex == colorIndex)
            return;

        this.colorIndex = colorIndex;
        Color color = this.colorIndex < 0 ? emptyColor : colors[this.colorIndex];
        image.color = color;
        currentScale = this.colorIndex < 0
            ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0.8f, 0.8f, 1f);
        Hightlight(false);
    }

    private void Hightlight(bool mode)
    {
        if (currentSelectedColorInput == this)
            return;

        activeScale = mode ? Vector3.one : currentScale;
    }

    private void Update()
    {
        image.transform.localScale = Vector3.Lerp(image.transform.localScale, activeScale, 0.1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetColorIndex();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hightlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hightlight(false);
    }
}
