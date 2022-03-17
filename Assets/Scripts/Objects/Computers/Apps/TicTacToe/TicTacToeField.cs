using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TicTacToeField : MonoBehaviour, IPointerClickHandler
{
    private static Color[] colors = new Color[]
    {
        new Color(    1f,     0f,     0f, 1f), // Rot
        new Color(    1f,   0.4f,     0f, 1f), // Orange
        new Color(    1f,     1f,     0f, 1f), // Gelb
        new Color(    0f,     1f,     0f, 1f), // Grün
        new Color( 0.06f,  0.66f,     1f, 1f), // Hellblau
        new Color(    0f,     0f,     1f, 1f), // Blau
        new Color(    1f,     0f,     1f, 1f)  // Violett
    };
    
    public static void ShuffleColors()
    {
        colors = ArrayHelper.Shuffle<Color>(colors);
    }

    public int id;
    
    public delegate void OnClick(int id);
    public event OnClick onClick;

    public int State { get => state; }
    public Image image;

    private int state = -1;
    
    public void SetState(int state, Sprite sprite = null)
    {
        this.state = state;
        image.sprite = sprite;
        image.color = state < 0 ? new Color(0.76f, 0.76f, 0.76f) : colors[state];
        image.transform.localScale = state > -1 ? new Vector3(0.8f, 0.8f, 1f) : Vector3.one;
    }

    public void Highlight()
    {
        image.transform.DOScale(Vector3.one, 0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (state == -1)
            onClick?.Invoke(id);
    }
}