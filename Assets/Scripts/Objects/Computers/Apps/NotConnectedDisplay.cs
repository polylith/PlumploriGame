using UnityEngine;
using TMPro;
using Language;

public class NotConnectedDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UITextButton retryButton;

    public void Show(System.Action retryAction)
    {
        text.SetText(
            LanguageManager.GetText(
                LangKey.NotAvailable,
                LanguageManager.GetText(LangKey.NetworkConnection)
            )
        );
        retryButton.SetText(LanguageManager.GetText(LangKey.Retry));
        retryButton.SetAction(retryAction);
        retryButton.IsEnabled = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        retryButton.SetAction(null);
        retryButton.IsEnabled = false;
        gameObject.SetActive(false);
    }
}
