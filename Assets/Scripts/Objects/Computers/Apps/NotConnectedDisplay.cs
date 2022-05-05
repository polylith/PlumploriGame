using UnityEngine;
using TMPro;
using Language;
using System.Collections;

public class NotConnectedDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UITextButton retryButton;

    private System.Action retryAction;
    private IEnumerator ieRetry;
    private Computer computer;

    public void Show(System.Action retryAction, Computer computer)
    {
        this.retryAction = retryAction;
        this.computer = computer;
        ResetText(false);
        retryButton.SetText(LanguageManager.GetText(LangKey.Retry));
        retryButton.SetAction(Retry);
        retryButton.IsEnabled = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        CancelRetry();
        retryButton.SetAction(null);
        retryButton.IsEnabled = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelRetry();
    }

    private void ResetText(bool isConnecting)
    {
        string msg = LanguageManager.GetText(
            LangKey.NotAvailable,
            LanguageManager.GetText(LangKey.NetworkConnection)
        );

        if (isConnecting)
        {
            msg = LanguageManager.GetText(
            LangKey.Retrieving,
            LanguageManager.GetText(LangKey.Data)
        );
        }

        text.SetText(msg);
    }

    private void CancelRetry()
    {
        if (null != ieRetry)
        {
            StopCoroutine(ieRetry);
            ieRetry = null;
            computer?.StopPCNoise();
        }
    }

    private void Retry()
    {
        if (null == retryAction)
            return;

        CancelRetry();

        ieRetry = IERetry();
        StartCoroutine(ieRetry);
    }

    private IEnumerator IERetry()
    {
        retryButton.IsEnabled = false;
        computer?.PCNoise();
        ResetText(true);

        yield return new WaitForSecondsRealtime(Random.Range(2f, 3f));

        retryAction.Invoke();

        yield return null;

        retryButton.IsEnabled = true;
        ResetText(false);
        CancelRetry();
    }
}
