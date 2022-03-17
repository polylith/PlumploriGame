using UnityEngine;
using TMPro;

public class BlueScreen : MonoBehaviour
{
    public bool IsInteractive { get; private set; } = false;
    public TextMeshProUGUI text0;
    public TextMeshProUGUI text1;

    public void Show(bool isInteractive)
    {
        text1.enabled = false;
        gameObject.SetActive(true);
        IsInteractive = isInteractive;

        if (!IsInteractive)
            return;

        GameEvent.GetInstance().Execute<int>(Blink, 0, 0.5f);
    }

    public void Hide()
    {
        IsInteractive = false;
        text1.enabled = false;
        gameObject.SetActive(false);
    }

    private void Blink(int count)
    {
        if (!IsInteractive)
            return;

        text1.enabled = count % 2 == 0;
        GameEvent.GetInstance().Execute<int>(Blink, count + 1, 0.5f);
    }
}
