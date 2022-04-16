using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FourInARowPlayerDisplay : MonoBehaviour
{
    public Image back;
    public TextMeshProUGUI playerDisplay;
    public TextMeshProUGUI scoreDisplay;

    public bool IsCurrent { get => isCurrent; set => SetCurrent(value); }
    public int PlayerId { get => playerId; set => SetPlayerId(value); }

    private int playerId;
    private int score;
    private bool isCurrent;

    private void SetCurrent(bool isCurrent)
    {
        if (this.isCurrent == isCurrent)
            return;

        this.isCurrent = isCurrent;
        back.transform.localScale = isCurrent
            ? Vector3.one : new Vector3(0.7f, 0.7f, 1f);
    }

    private void SetPlayerId(int playerId)
    {
        if (this.playerId != playerId)
        {
            score = 0;
        }

        IsCurrent = false;
        this.playerId = playerId;
        UpdateScoreDisplay();

        if (playerId > -1)
        {
            Color color = FourInARowCoin.Colors[playerId];
            back.color = color;
            gameObject.SetActive(true);
            UpdatePlayerDisplay();
            return;
        }

        gameObject.SetActive(false);
    }

    public void AddScore(int score)
    {
        this.score += score;
        UpdateScoreDisplay();
    }

    private void UpdatePlayerDisplay()
    {
        playerDisplay.SetText(
            Language.LanguageManager.GetText(
                Language.LangKey.Player,
                (playerId + 1).ToString()
            )
        );
    }

    private void UpdateScoreDisplay()
    {
        scoreDisplay.SetText(score.ToString());
    }

}
