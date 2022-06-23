using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// <para>
/// This class is a singleton. It can be called in various ways
/// to display activity of unspecified duration in the UI.
/// </para>
/// <para>
/// For example, to visualize the progress of asynchronous load
/// and save operations.
/// </para>
/// </summary>
public class UIProgress : MonoBehaviour
{
    public static UIProgress GetInstance()
    {
        return ins;
    }

    private static UIProgress ins;

    public bool ShowPercentage { get => showPercentage; set => SetShowPercentage(value); }

    public Image[] imgs;
    public RectTransform progressParent;
    public Image readyImage;
    public Image progressBar;
    public TextMeshProUGUI percentDisplay;

    private bool showPercentage = false;
    private bool isRunning;
    private int count;
    private float[] scales;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            isRunning = false;
            gameObject.SetActive(false);
            float deltaAlpha = 1f / imgs.Length;
            scales = new float[imgs.Length];

            for (int i = 0; i < scales.Length; i++)
            {
                scales[i] = (i + 1) * deltaAlpha;
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetShowPercentage(bool b)
    {
        showPercentage = b;
        percentDisplay.SetText("0 %");
        progressBar.fillAmount = 0f;
        progressParent.gameObject.SetActive(b);
    }

    public void UpdateProgressBar(float f)
    {
        f = Mathf.Clamp01(f);
        progressBar.fillAmount = f;
        string percentText = Mathf.Round(f * 100f) + " %";
        percentDisplay.SetText(percentText);
    }

    public void ProgressReady()
    {
        readyImage.gameObject.SetActive(true);
        progressBar.fillAmount = 1f;
        percentDisplay.SetText("100 %");
        GameEvent.GetInstance()?.Execute(
            () => {
                AudioManager.GetInstance().PlaySound("blip");
            },
            0.5f
        );
    }

    public void Run(bool showPercentage = false)
    {
        if (isRunning)
            return;

        ShowPercentage = showPercentage;

        if (showPercentage)
        {
            progressParent.localScale = Vector3.zero;
        }

        readyImage.gameObject.SetActive(false);
        count = 0;
        isRunning = true;
        transform.transform.localScale = Vector3.zero;
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence()
            .SetAutoKill(true)
            .Append(transform.DOScale(Vector3.one, 0.3f));

        if (showPercentage)
        {
            seq.Append(progressParent.DOScale(Vector3.one, 0.125f));
        }

        seq.OnComplete(() => {
                StartCoroutine(IERun());
            })
            .Play();
    }

    public void Stop()
    {
        if (!isRunning)
            return;

        readyImage.gameObject.SetActive(false);

        Sequence seq = DOTween.Sequence()
            .SetAutoKill(true);

        if (ShowPercentage)
        {
            seq.Append(progressParent.DOScale(Vector3.zero, 0.125f));
        }

        seq.Append(transform.DOScale(Vector3.zero, 0.3f))
            .OnComplete(() => {
                isRunning = false;
                gameObject.SetActive(false);
                ShowPercentage = false;
            })
            .Play();
    }

    private IEnumerator IERun()
    {
        

        while (isRunning)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                int j = (i + count) % imgs.Length;
                imgs[j].transform.localScale = Vector3.one * scales[i];

                yield return new WaitForEndOfFrame();
            }

            count++;
        }
    }
}