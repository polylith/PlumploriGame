using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// This class is the UI representation of a slot
/// in the inventory. Each slot can hold exactly
/// one collectable.
///
/// The slot consists of two images. One image is
/// for the 2D texture of the collectable and the
/// other is a background image for the slot container.
/// The background image is colored on mouse interaction.
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool HasObject { get => null != collectable; }

    public Image objectIcon;

    private Image background;
    private Collectable collectable;

    private void Awake()
    {
        objectIcon.gameObject.SetActive(false);
        background = GetComponent<Image>();
        UpdateBackground(false, true);
    }

    public void HideObject()
    {
        if (null == collectable || collectable.IsDropping)
            return;

        collectable.gameObject.SetActive(false);
        objectIcon.gameObject.SetActive(false);
    }

    public void ResetObjectInPosition()
    {
        if (null == collectable)
            return;

        objectIcon.gameObject.SetActive(true);
    }

    public void SetObject(Collectable collectable)
    {
        if (this.collectable == collectable.transform.gameObject)
            return;

        if (null != this.collectable)
            RemoveObject();

        this.collectable = collectable;

        if (null != collectable)
        {
            Texture2D tex = collectable.GetObjectIcon();
            objectIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            objectIcon.sprite.name = tex.name;
            objectIcon.gameObject.SetActive(true);
            collectable.gameObject.SetActive(false);
            UpdateBackground(false, false);
        }
    }

    public void RemoveObject()
    {
        if (null == collectable)
            return;

        objectIcon.gameObject.SetActive(false);
        collectable.gameObject.SetActive(true);
        collectable = null;
        UpdateBackground(false, false);
    }

    private void UpdateBackground(bool mode, bool instant)
    {
        Color color = null != collectable ? Color.white : Color.grey;

        if (mode)
        {
            color = Color.yellow;
        }

        if (instant)
            background.color = color;
        else
            background.DOColor(color, 0.5f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIDropPoint.GetInstance().HidePointer();

        if (null == collectable)
        {
            UIGame.GetInstance().SetCursorEnabled(false, true);
            return;
        }

        collectable.MouseOver();
        UpdateBackground(true, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIDropPoint.GetInstance().HidePointer();

        if (null == collectable)
        {
            UIGame.GetInstance().SetCursorEnabled(false, true);
            return;
        }

        if (collectable.IsDropping)
            return;

        collectable.MouseExit();
        UpdateBackground(false, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (null == collectable)
            return;

        collectable.MouseClick();
        UpdateBackground(false, false);
    }
}