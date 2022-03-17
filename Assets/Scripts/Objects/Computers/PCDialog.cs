using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Language;
using System.Collections.Generic;

public class PCDialog : MonoBehaviour
{
    public enum DialogType
    {
        Error = 0,
        Info,
        Warning,
        Question,
        Prompt
    }
    public enum ButtonLabels
    {
        None = -1,
        YesNoCancel,
        OkCancel,
        Ok,
        Custom
    }

    public int OwnerId { get => ownerId; }
    public bool IsVisible { get => isVisible; }

    public RectTransform rectTransform;
    public Transform appContent;
    public TextMeshProUGUI appTitle;
    public UIIconButton closeButton;
    public UIIconButton hideButton;

    public Sprite[] icons;
    public Color[] iconColors;
    public Image titleIcon;
    public TextMeshProUGUI message;
    public UITextButton button0;
    public UITextButton button1;
    public UITextButton button2;

    private bool isVisible = false;
    private IEnumerator ieScale;
    private int ownerId = -1;

    private void Awake()
    {
        isVisible = true;
        SetVisible(false, true);
    }

    public void SetOwnerId(int ownerId)
    {
        this.ownerId = ownerId;
    }

    public void SetType(DialogType dialogType)
    {
        int iType = (int)dialogType;
        titleIcon.sprite = icons[iType];
        titleIcon.color = iconColors[iType];
        appTitle.SetText(
            LanguageManager.GetText(dialogType.ToString())
        );
    }

    public void SetMessage(string text)
    {
        message.SetText(text);
    }

    public void SetButtonActions(
        ButtonLabels buttonLabels,
        List<System.Action> buttonActions,
        string[] buttonTexts = null
    )
    {
        if (buttonLabels == ButtonLabels.None
            || null == buttonActions || buttonActions.Count == 0)
        {
            button0.gameObject.SetActive(false);
            button1.gameObject.SetActive(false);
            button2.gameObject.SetActive(false);
            return;
        }

        button0.SetAction(buttonActions[0]);
        button0.gameObject.SetActive(null != buttonActions[0]);

        if (buttonLabels == ButtonLabels.Custom
            && null != buttonTexts && buttonTexts.Length > 0)
        {
            button0.SetText(buttonTexts[0]);
        }

        if (buttonActions.Count > 1)
        {
            button1.SetAction(buttonActions[1]);
            button1.gameObject.SetActive(null != buttonActions[1]);

            if (buttonLabels == ButtonLabels.Custom
                && null != buttonTexts && buttonTexts.Length > 1)
            {
                button1.SetText(buttonTexts[1]);
            }
        }
        else
        {
            button1.gameObject.SetActive(false);
        }

        if (buttonActions.Count > 2)
        {
            button2.SetAction(buttonActions[2]);
            button2.gameObject.SetActive(null != buttonActions[2]);

            if (buttonLabels == ButtonLabels.Custom
                && null != buttonTexts && buttonTexts.Length > 2)
            {
                button2.SetText(buttonTexts[2]);
            }
        }
        else
        {
            button2.gameObject.SetActive(false);
        }

        switch (buttonLabels)
        {
            case ButtonLabels.Custom:
                // already done
                break;
            case ButtonLabels.YesNoCancel:
                button0.SetText(LanguageManager.GetText(LangKey.Yes));
                button1.SetText(LanguageManager.GetText(LangKey.No));
                button2.SetText(LanguageManager.GetText(LangKey.Cancel));
                break;
            case ButtonLabels.OkCancel:
                button0.SetText(LanguageManager.GetText(LangKey.Ok));
                button1.SetText(LanguageManager.GetText(LangKey.Cancel));

                button2.gameObject.SetActive(false);
                break;
            case ButtonLabels.Ok:
                button0.SetText(LanguageManager.GetText(LangKey.Ok));

                button1.gameObject.SetActive(false);
                button2.gameObject.SetActive(false);
                break;
        }   
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool isVisible, bool instant = false)
    {
        if (this.isVisible == isVisible)
            return;

        this.isVisible = isVisible;
        Vector3 scale = isVisible ? Vector3.one : Vector3.zero;

        if (instant)
        {
            transform.localScale = scale;
            return;
        }

        if (null != ieScale)
            StopCoroutine(ieScale);

        ieScale = IEScale(scale);
        StartCoroutine(ieScale);
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0f;

        while (f <= 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, f);
            yield return null;
            f += Time.deltaTime;
        }

        transform.localScale = scale;
    }

}
