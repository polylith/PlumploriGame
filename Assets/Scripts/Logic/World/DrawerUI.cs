using System.Collections.Generic;
using UnityEngine;

public class DrawerUI : InteractableUI
{
    public DrawerContentSlotUI[] drawerContentSlotUIs;

    protected override void BeforeHide()
    {
        UIDropPoint.GetInstance().HidePointer();

        if (null == interactable || !(interactable is Openable openable))
            return;

        openable.SetOpen(false, false);
    }

    protected override void Initialize()
    {
        if (null != closeButton)
        {
            closeButton.SetAction(CloseUI);
        }

        ClearContent();
        UIDropPoint.GetInstance().HidePointer();

        if (null == interactable
            || !(interactable is Openable openable)
            || !openable.HasContent)
            return;

        List<Collectable> content = openable.Content;

        foreach (Collectable collectable in content)
        {
            ObjectPlace objectPlace = collectable.ObjectPlace;

            if (null != objectPlace && objectPlace is DrawerContentSlotUI drawerContentSlotUI)
            {
                drawerContentSlotUI.SetCollectable(collectable);
            }
        }
    }

    private void ClearContent()
    {
        foreach (DrawerContentSlotUI drawerContentSlotUI in drawerContentSlotUIs)
        {
            drawerContentSlotUI.Clear();
        }
    }

    private void CloseUI()
    {
        Action.ActionController.GetInstance().CancelCurrentAction();
        Hide();
    }

    public Vector3 GetWalkPosition()
    {
        if (null == interactable)
        {
            return GameManager.GetInstance().CurrentPlayer.transform.position;
        }            

        return interactable.GetInteractionPosition();
    }

    public Vector3 GetLookAtPosition()
    {
        if (null == interactable)
        {
            return GameManager.GetInstance().CurrentPlayer.transform.position;
        }

        return interactable.transform.position;
    }

    public void AddObject(Collectable collectable)
    {
        if (null == interactable
            || !(interactable is Openable openable))
            return;

        List<Collectable> content = openable.Content;

        if (!content.Contains(collectable))
        {
            content.Add(collectable);
        }
    }
}
