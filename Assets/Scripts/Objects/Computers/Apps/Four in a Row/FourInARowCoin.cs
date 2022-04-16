using UnityEngine;
using UnityEngine.UI;

public class FourInARowCoin : MonoBehaviour
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

    public static void ShuffleColors()
    {
        Colors = ArrayHelper.Shuffle(FourInARowCoin.AllColors);
    }

    public FourInARowCoin Instantiate(int playerId, int number)
    {
        FourInARowCoin coin = Instantiate(this) as FourInARowCoin;
        coin.PlayerId = playerId;
        coin.transform.name = "Coin " + number;
        return coin;
    }

    public Color Color { get => playerId > -1 ? Colors[playerId] : Color.gray; }
    public int PlayerId { get => playerId; private set => SetPlayerId(value); }

    public Image back;
    public Image icon;

    private int playerId;

    private void SetPlayerId(int playerId)
    {
        this.playerId = playerId;
        back.color = Colors[playerId];
    }
}
