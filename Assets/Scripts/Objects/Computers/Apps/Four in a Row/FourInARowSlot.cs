using UnityEngine;
using UnityEngine.UI;

public class FourInARowSlot : MonoBehaviour
{
    public bool IsEmpty { get => null == coin; }
    public int PlayerId { get => null == coin ? -1 : coin.PlayerId; }

    private Image back;

    private FourInARowCoin coin;

    private void Awake()
    {
        back = GetComponent<Image>();
    }

    public void ResetSlot()
    {
        if (null != coin)
        {
            Destroy(coin.gameObject);
        }

        back.color = Color.gray;
        coin = null;
    }

    public void Highlight(int colorIndex)
    {
        Color color = Color.gray;

        if (colorIndex > -1 && colorIndex < FourInARowCoin.Colors.Length)
        {
            color = FourInARowCoin.Colors[colorIndex];
        }

        back.color = color;
    }

    public void Highlight()
    {
        Color color = Color.gray;

        if (null != coin)
        {
            color = coin.Color;
        }

        back.color = color;
    }

    public void SetCoin(FourInARowCoin coin)
    {
        this.coin = coin;
    }
}
