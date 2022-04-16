using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TelephoneUI : InteractableUI
{
    private static string digits = "0123456789*#";

    public bool InUse { get => inUse; }

    public UIPhonebookEntry entryPrefab;

    public Image callImg;
    public Image back;
    public TextMeshProUGUI display;
    public UIIconButton callButton;
    public UIIconButton finishButton;
    public UIIconButton backButton;
    public Image phoneBookTrans;
    public ScrollRect scrollRect;
    public RectTransform list;

    private bool isActive;
    private string sInput;
    private bool isPhonebookVisible;
    private bool inUse;
    
    protected override void Initialize()
    {
        InitPhonebook();
        InitButtons();

        isActive = false;
        inUse = false;
        callImg.gameObject.SetActive(false);
        ClearDisplay();
        isPhonebookVisible = false;
        phoneBookTrans.rectTransform.localScale = new Vector3(0f, 1f, 1f);
    }

    protected override void BeforeHide()
    {
        if (null == interactable || !(interactable is Telephone phone))
            return;

        if (inUse && null != phone)
        {
            phone.FinishOutCall();
            return;
        }

        if (isPhonebookVisible)
            ShowPhonebook(false);

        callImg.gameObject.SetActive(false);
        ClearPhonebook();
        ClearDisplay();
        isActive = false;
        inUse = false;

        if (null != phone)
        {
            phone.FinishCall();
            phone.audioSource.loop = false;
        }
    }

    private void InitButtons()
    {
        callButton.SetAction(Call);
        backButton.SetAction(InputBack);
        finishButton.SetAction(Hide);

        callButton.IsEnabled = false;
        backButton.IsEnabled = false;
        finishButton.IsEnabled = true;
    }

    private void UpdateButtons()
    {
        bool hasInput = sInput.Length > 0 && !inUse;
        callButton.IsEnabled = hasInput;
        backButton.IsEnabled = hasInput;
    }

    private static bool IsServiceCode(string s)
    {
        return s.Contains("*") || s.Contains("#");
    }

    private void HandleServiceCode(string s)
    {
        if (inUse || null == interactable || !(interactable is Telephone phone))
            return;

        if ("*135#".Equals(s))
        {
            ShowText(phone.Number);
        }
        else if ("*#0*#".Equals(s))
        {
            inUse = true;
        }
        else
        {
            AudioManager.GetInstance().PlaySound("action.denied", gameObject);
            GameEvent.GetInstance().Execute(ClearDisplay, 3f);
        }

        sInput = "";
        UpdateButtons();
    }

    private void Call()
    {
        if(null == interactable || !(interactable is Telephone phone))
            return;
        
        if ("".Equals(sInput))
            return;

        if (IsServiceCode(sInput))
        {
            HandleServiceCode(sInput);
            return;
        }

        inUse = true;
        callImg.gameObject.SetActive(true);
        UpdateButtons();
        phone.Call(sInput);
    }

    public void CancelCall()
    {
        if (null == interactable || !(interactable is Telephone phone))
            return;

        callImg.gameObject.SetActive(false);
        AudioManager.GetInstance().PlaySound("click1", gameObject, 1.25f);
        inUse = false;
        ClearDisplay();
        phone.ClearCurrentNumber();
        UpdateButtons();
    }

    public void Input1()
    {
        AddInput("1");
    }

    public void Input2()
    {
        AddInput("2");
    }

    public void Input3()
    {
        AddInput("3");
    }

    public void Input4()
    {
        AddInput("4");
    }

    public void Input5()
    {
        AddInput("5");
    }

    public void Input6()
    {
        AddInput("6");
    }

    public void Input7()
    {
        AddInput("7");
    }

    public void Input8()
    {
        AddInput("8");
    }

    public void Input9()
    {
        AddInput("9");
    }

    public void Input0()
    {
        AddInput("0");
    }

    public void InputStar()
    {
        AddInput("*");
    }

    public void InputHash()
    {
        AddInput("#");
    }

    public void InputBack()
    {
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        sInput = sInput.Length > 0 ? sInput.Substring(0, sInput.Length - 1) : "";
        ShowText(sInput);
    }

    private void AddInput(string s)
    {
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);

        if (null == s || !digits.Contains(s) || sInput.Length >= 15 || inUse)
            return;

        AudioManager.GetInstance().PlaySound("dtmf" + s, gameObject);
        sInput += s;
        ShowText(sInput);
        UpdateButtons();
    }

    public void InputNumber(string number)
    {
        AudioManager.GetInstance().PlaySound("click2", gameObject);
        ClearDisplay();
        sInput = number;
        ShowText(sInput);
        UpdateButtons();
    }

    private void ClearDisplay()
    {
        sInput = "";
        display.SetText("");
    }

    private void ShowText(string s, string color = "#000000")
    {
        string text = "<color=" + color + ">" + s + "</color>";
        display.SetText(text);
    }
        
    private void Update()
    {
        if (!isActive)
            return;

        if (Input.GetKeyDown(KeyCode.Backspace))
            InputBack();
        else if (Input.GetKeyDown(KeyCode.Delete))
            ClearDisplay();
        else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Call();
    }

    private void OnGUI()
    {
        if (!isActive)
            return;

        Event e = Event.current;

        if (null != e && e.isKey)
            AddInput(e.character.ToString().ToUpper());
    }

    public void ShowPhonebook()
    {
        ShowPhonebook(!isPhonebookVisible);
    }

    private void ShowPhonebook(bool isVisible)
    {
        if (isPhonebookVisible == isVisible)
            return;

        isPhonebookVisible = isVisible;
        Vector3 scale = new Vector3(isVisible ? 1f : 0f, 1f, 1f);
        phoneBookTrans.rectTransform.DOScale(scale, 0.25f);
    }

    private void ClearPhonebook()
    {
        foreach (Transform trans in list)
            Destroy(trans.gameObject);
    }

    private void InitPhonebook()
    {
        if (null == interactable || !(interactable is Telephone phone))
            return;

        ClearPhonebook();

        PhoneBook phoneBook = PhoneDirectory.GetPhoneList(phone);
        List<PhoneBookEntry> phonelist = phoneBook.Entries;

        for (int i = 0; i < phonelist.Count; i++)
        {
            string name = phonelist[i].Name;
            string number = phonelist[i].Number;
            UIPhonebookEntry entry = entryPrefab.Instantiate(name, number, this, i);
            entry.transform.SetParent(list, false);
        }

        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public void PointerEnter()
    {
        if (inUse)
            return;

        UIGame.GetInstance().SetCursorEnabled(true, false);
    }

    public void PointerExit()
    {
        if (inUse)
            return;

        UIGame uiGame = UIGame.GetInstance();
        bool mode = !uiGame.IsCursorOverUI && !uiGame.IsUIExclusive;
        uiGame.SetCursorEnabled(false, mode);
    }
}