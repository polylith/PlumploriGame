using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Action;

public class DrawerContentSlotUI : ObjectPlace,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image background;
    public Image objectIcon;
    public DrawerUI drawerUI;

    private void Awake()
    {
        objectIcon.gameObject.SetActive(false);
        background = GetComponent<Image>();
        UpdateBackground(false, true);
    }

    public override Vector3 GetWalkPosition(Collectable collectable)
    {
        return drawerUI.GetWalkPosition();
    }

    public override Vector3 GetLookAtPosition()
    {
        return drawerUI.GetLookAtPosition();
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

    public void AddObject(Collectable collectable)
    {
        drawerUI.AddObject(collectable);
        SetCollectable(collectable);
    }

    public override void SetCollectable(Collectable collectable)
    {
        if (null != this.collectable && null != collectable
            && this.collectable.gameObject == collectable.transform.gameObject)
            return;

        if (null != this.collectable)
            RemoveObject();

        this.collectable = collectable;

        if (null == collectable)
        {
            return;
        }

        Texture2D tex = collectable.GetObjectIcon();
        objectIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        objectIcon.sprite.name = tex.name;
        objectIcon.gameObject.SetActive(true);
        collectable.gameObject.SetActive(false);
        UpdateBackground(false, false);
    }

    public void Clear()
    {
        collectable = null;
        objectIcon.gameObject.SetActive(false);
        UpdateBackground(false, false);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!UIGame.GetInstance().IsCursorOverUI || !Input.GetMouseButtonUp(0))
            return;

        ActionController actionController = ActionController.GetInstance();

        if (null == collectable)
        {
            if (actionController.IsCurrentAction(typeof(DropAction))
                && actionController.IsCurrentActionActive())
            {
                UIDropPoint uiDropPoint = UIDropPoint.GetInstance();
                uiDropPoint.SetObjectPlace(this);
                uiDropPoint.IsLegal = true;
                uiDropPoint.InvokeDrop();
            }

            return;
        }

        collectable.MouseClick();
        UpdateBackground(false, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ActionController actionController = ActionController.GetInstance();
        UIDropPoint.GetInstance().HidePointer();

        if (actionController.IsCurrentAction(typeof(DropAction))
                && actionController.IsCurrentActionActive())
        {
            UIGame.GetInstance().SetCursorEnabled(false, true);
            UpdateBackground(false, false);
            return;
        }

        if (null == collectable)
        {
            UIGame.GetInstance().SetCursorEnabled(false, false);
            return;
        }

        if (collectable.IsDropping)
            return;

        collectable.MouseExit();
        UpdateBackground(false, false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ActionController actionController = ActionController.GetInstance();
        UIDropPoint.GetInstance().HidePointer();

        if (null == collectable)
        {
            if (actionController.IsCurrentAction(typeof(DropAction))
                && actionController.IsCurrentActionActive())
            {
                UIGame.GetInstance().SetCursorEnabled(true, true);
                UpdateBackground(true, false);
                UIDropPoint.GetInstance().SetObjectPlace(this);
                return;
            }

            UIGame.GetInstance().SetCursorEnabled(false, false);
            return;
        }

        if (actionController.IsCurrentAction(typeof(DropAction))
                && actionController.IsCurrentActionActive())
        {
            UIGame.GetInstance().SetCursorDisabled(true);
            return;
        }

        collectable.MouseOver();
        UpdateBackground(true, false);
    }
}
