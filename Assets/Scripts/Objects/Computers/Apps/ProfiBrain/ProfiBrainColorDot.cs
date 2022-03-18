using UnityEngine;
using UnityEngine.UI;

public class ProfiBrainColorDot : MonoBehaviour
{
    private static readonly Color[] AllColors = new Color[] {
        new Color(    1f,     0f,     0f, 1f), // 0 Red
        new Color(    1f,   0.4f,     0f, 1f), // 1 Orange
        new Color(    1f,     1f,     0f, 1f), // 2 Yellow
        new Color(    0f,     1f,     0f, 1f), // 3 Green
        new Color( 0.06f,  0.66f,     1f, 1f), // 4 Lightblue
        new Color(    0f,     0f,     1f, 1f), // 5 Blue
        new Color(    1f,     0f,     1f, 1f)  // 6 Purple
    };
    public static Color[] Colors = AllColors;
    public static readonly Color emptyColor = new Color(0.2f, 0.2f, 0.2f);

    public static void ShuffleColors() {
        Colors = ArrayHelper.Shuffle(ProfiBrainColorDot.AllColors);
    }

    public int ColorIndex { get => colorIndex; set => SetColorIndex(value); }

    public Image image;

    protected int colorIndex = -1;
    protected Vector3 activeScale = new Vector3(0.5f, 0.5f, 1f);
    protected Vector3 currentScale = new Vector3(0.5f, 0.5f, 1f);

    protected virtual void SetColorIndex(int colorIndex)
    {
        colorIndex = Mathf.Clamp(colorIndex, -1, Colors.Length - 1);

        if (this.colorIndex == colorIndex)
            return;

        this.colorIndex = colorIndex;
        Color color = this.colorIndex < 0 ? emptyColor : Colors[this.colorIndex];
        image.color = color;
        currentScale = this.colorIndex < 0
            ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0.8f, 0.8f, 1f);
    }

    private void Update()
    {
        image.transform.localScale = Vector3.Lerp(image.transform.localScale, activeScale, 0.2f);
    }
}
