using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Action;
using Language;

/// <summary>
/// The inventory box contains all the collected objects
/// of the current player. Since the player can be switched
/// within the game, the contents of the box also change.
///
/// The inventory box consists of several slots, that may
/// hold exactly one collectable in each.
/// 
/// The inventory box is displayed in the UI and therefore
/// consists of 2D images only. For this, each interactable
/// has a 2D texture that represents the object's appearance.
/// </summary>
public class Inventorybox : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsOpen { get => isOpen; }

    public Sprite[] sprites = new Sprite[2];
    public Inventory inventory;

    private Image image;

    private bool isOpen;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// Get the current position of the inventory box
    /// in 3D world.
    /// TODO fix it!!!
    /// Doesn't seem to work in this changed setup.
    /// </summary>
    /// <returns>position in 3D World</returns>
    public Vector3 GetWorldPosition()
    {
        //Vector3 position = UIGame.GetInstance().uiCamera.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        Vector3 position = Camera.main.ScreenToWorldPoint(transform.position + Vector3.up * 0.5f);
        return position;
    }

    /// <summary>
    /// Hightlight box image on mouse over
    /// </summary>
    /// <param name="mode">true = hightlight, false = unhightlight</param>
    private void Highlight(bool mode)
    {
        image.color = mode ? Color.yellow : Color.white;
    }

    /// <summary>
    /// Update the image of the box when it's opened
    /// or closed.
    /// </summary>
    private void UpdateImage()
    {
        image.sprite = sprites[isOpen ? 1 : 0];
    }

    /// <summary>
    /// Toggle open state of the box
    /// </summary>
    private void Toggle()
    {
        isOpen = !isOpen;
        UpdateImage();
        inventory.Show(isOpen);
    }

    /// <summary>
    /// Open the inventory box and its UI
    /// </summary>
    public void Open()
    {
        Open(false);
    }

    /// <summary>
    /// Open the inventory box
    /// </summary>
    /// <param name="boxOnly">only open the image in action bar</param>
    public void Open(bool boxOnly)
    {
        if (isOpen)
            return;

        isOpen = true;
        UpdateImage();

        if (boxOnly)
            return;

        inventory.Show(isOpen);
    }

    /// <summary>
    /// Close the inventory box
    /// </summary>
    public void Close()
    {
        Close(false);
    }

    /// <summary>
    /// Close the inventory box
    /// </summary>
    /// <param name="boxOnly">only close the image in action bar</param>
    public void Close(bool boxOnly)
    {
        if (!isOpen)
            return;

        isOpen = false;
        UpdateImage();

        if (boxOnly)
            return;

        inventory.Show(isOpen);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ActionController.GetInstance().HandleCursor();
        UIToolTip.GetInstance().SetText(
            LanguageManager.GetText(isOpen 
            ? LangKey.Close : LangKey.Open, 
            LanguageManager.GetText(LangKey.Inventorybox)),
            1
        );
        UIToolTip.GetInstance().Show(transform);
        UIGame.GetInstance().SetCursorEnabled(true, false);
        Highlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ActionController.GetInstance().HighlightCurrent();// UpdateToolTip();
        Highlight(false);
        UIGame.GetInstance().SetCursorEnabled(false, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
        ActionController.GetInstance().HighlightCurrent();// UpdateToolTip();
    }
}