using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIPhonebookEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image back;
    public TextMeshProUGUI textMesh;

    private string number;
    private TelephoneUI telephoneUI;

    public UIPhonebookEntry Instantiate(string number, TelephoneUI telephoneUI, int i)
    {
        UIPhonebookEntry entry = Instantiate(this) as UIPhonebookEntry;
        entry.telephoneUI = telephoneUI;
        entry.name = "Entry " + i;
        entry.SetNumber(number);
        return entry;
    }

    private void Awake()
    {
        textMesh.SetText("");
    }

    public void SetNumber(string number)
    {
        this.number = number;
        textMesh.SetText(number);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (telephoneUI.InUse)
            return;

        telephoneUI.InputNumber(number);
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