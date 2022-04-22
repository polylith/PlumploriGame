using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIPhonebookEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image back;
    public TextMeshProUGUI entryNameDisplay;
    public TextMeshProUGUI entryNumberDisplay;

    private PhoneBookEntry pbEntry;
    private TelephoneUI telephoneUI;

    public UIPhonebookEntry Instantiate(PhoneBookEntry entry, TelephoneUI telephoneUI, int i)
    {
        UIPhonebookEntry uiEntry = Instantiate(this);
        uiEntry.telephoneUI = telephoneUI;
        uiEntry.name = "Entry " + i;
        uiEntry.SetEntry(entry);
        return uiEntry;
    }

    public void SetEntry(PhoneBookEntry entry)
    {
        pbEntry = entry;
        UpdateDisplays();
    }
        
    private void UpdateDisplays()
    {
        entryNameDisplay.SetText(pbEntry.Name);
        entryNumberDisplay.SetText(pbEntry.Number);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (telephoneUI.InUse)
            return;

        telephoneUI.InputNumber(pbEntry.Number);
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