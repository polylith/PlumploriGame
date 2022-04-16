using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIPhonebookEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image back;
    public TextMeshProUGUI entryNameDisplay;
    public TextMeshProUGUI entryNumberDisplay;

    private string entryName = "???";
    private string entryNumber;
    private TelephoneUI telephoneUI;

    public UIPhonebookEntry Instantiate(string name, string number, TelephoneUI telephoneUI, int i)
    {
        UIPhonebookEntry entry = Instantiate(this) as UIPhonebookEntry;
        entry.telephoneUI = telephoneUI;
        entry.name = "Entry " + i;
        entry.SetName(name);
        entry.SetNumber(number);
        return entry;
    }

    private void Awake()
    {
        entryNameDisplay.SetText("");
        entryNumberDisplay.SetText("");
    }

    public void SetName(string name)
    {
        entryName = name;
        entryNameDisplay.SetText(name);
    }

    public void SetNumber(string number)
    {
        entryNumber = number;
        entryNumberDisplay.SetText(number);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (telephoneUI.InUse)
            return;

        telephoneUI.InputNumber(entryNumber);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (telephoneUI.InUse)
            return;

        UIGame.GetInstance().SetCursorEnabled(true, false);
        back.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (telephoneUI.InUse)
            return;

        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetCursorEnabled(false, !uiGame.IsUIExclusive);
        back.color = Color.white;
    }
}