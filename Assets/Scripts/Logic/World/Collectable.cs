using System.Collections.Generic;
using UnityEngine;
using Action;

/// <summary>
/// <para>
/// This abstracted class handles objects that the current
/// player may collect. These objects are inserted in the
/// current player's inventory, if there is free capacity
/// left.
/// </para>
/// <para>
/// Some collectables may also be dropped into the current
/// scene again. The attribute isDropAble is designed for
/// this purpose.
/// </para>
/// <para>
/// The RigidbodyConstraints and the useGravitiy attribute
/// are potentially removed. This is because the revision
/// of the drop operation is done via ObjectPlaces, which
/// are used to specify the drop position.
/// In earlier version, objects could just be dropped
/// anywhere and based on physics and gravity they found
/// the position on their own.
/// </para>
/// </summary>
public abstract class Collectable : Interactable
{
    public Transform dropPoint;
    public bool isDropAble;
    public DropOrientation dropOrientation;

    public bool IsCollected { get => collected; }
    public bool IsDropping { get => isDropping; }
    public ObjectPlace ObjectPlace { get => objectPlace; }

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;
    private int layer;

    private InventorySlot inventorySlot;
    private InventoryContent inventoryContent;
    private bool collected = false;
    private bool isDropping;
    private ObjectPlace objectPlace;

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
            "Collected",
            "Dropped"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
    }

    public override void RegisterGoals()
    {
        Formula f = WorldDB.Get(Prefix + "Collected");
        WorldDB.RegisterFormula(new Implication(f, null));

        f = WorldDB.Get(Prefix + "Dropped");
        WorldDB.RegisterFormula(new Implication(f, null));
    }

    public override Vector3 GetInteractionPosition()
    {
        if (null == objectPlace)
        {
            return base.GetInteractionPosition();
        }

        return objectPlace.GetWalkPosition(this);
    }

    public override Vector3 GetLookAtPosition()
    {
        if (null == objectPlace)
            return base.GetLookAtPosition();

        return objectPlace.GetLookAtPosition();
    }

    public override int IsInteractionEnabled()
    {
        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(GrabAction))
            && !actionController.IsCurrentAction(typeof(DropAction)))
            return base.IsInteractionEnabled();

        if (actionController.IsCurrentAction(typeof(GrabAction)) && collected 
            || actionController.IsCurrentAction(typeof(DropAction)) && !collected)
            return -1;

        if (actionController.IsCurrentAction(typeof(DropAction)))
        {
            if (collected && !isDropAble)
                return -1;

            return 1;
        }
        
        if (actionController.IsCurrentAction(typeof(GrabAction)) && !collected)
        {
            Player player = GameManager.GetInstance().CurrentPlayer;

            if (!player.HasInventoryCapacity())
                return -1;
        }

        return 1;
    }

    /// <summary>
    /// Set the inventory content that contains this collectectable.
    /// </summary>
    public void SetInventoryContent(InventoryContent inventoryContent)
    {
        this.inventoryContent = inventoryContent;
    }

    /// <summary>
    /// Get the inventory slot that contains this collectectable.
    /// </summary>
    /// <returns>inventory slot that contains this collectectable</returns>
    public InventorySlot GetInventorySlot()
    {
        return inventorySlot;
    }

    /// <summary>
    /// Remove this collectable from its intentory slot
    /// </summary>
    private void UnsetInventorySlot()
    {
        if (null == inventorySlot)
            return;

        inventorySlot.RemoveObject();
        inventorySlot = null;
    }

    /// <summary>
    /// Set the inventory slot that contains this collectectable.
    /// </summary>
    public void SetInventorySlot(InventorySlot inventorySlot)
    {
        this.inventorySlot = inventorySlot;        
    }

    /// <summary>
    /// Restore position, rotation and scale of this transform.
    /// </summary>
    public virtual void Restore()
    {
        Debug.Log(transform.name + " restore values");

        RestoreLayer();
        transform.position = position;
        transform.localRotation = rotation;
        transform.localScale = scale;
    }

    /// <summary>
    /// Restore the render layer of this GameObject.
    /// </summary>
    public void RestoreLayer()
    {
        SetLayer(layer);
    }

    /// <summary>
    /// Store position, rotation, scale and the render layer
    /// of this transform.
    /// </summary>
    public void StoreValues()
    {
        Debug.Log(transform.name + " store values");
        position = transform.position;
        rotation = transform.localRotation;
        scale = transform.localScale;
        layer = transform.gameObject.layer;
    }

    /// <summary>
    /// Collect this collectable
    /// </summary>
    /// <returns>true when it could be added to the current player's inventory</returns>
    public bool Collect()
    {
        CheckObjectIcon();
        Player player = GameManager.GetInstance().CurrentPlayer;
        bool isCollected = AddInventory(player);

        if (isCollected && null != objectPlace)
        {
            objectPlace.SetCollectable(null);
            objectPlace = null;
        }

        return isCollected;
    }

    public override bool Interact(Interactable interactable)
    {
        Debug.Log("Collectable " + this.GetType().Name + " interact " + interactable);

        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(GrabAction)) 
            && !actionController.IsCurrentAction(typeof(DropAction)))
            return base.Interact(interactable);

        Debug.Log("\tcollected " + collected);

        if (!collected && actionController.IsCurrentAction(typeof(GrabAction)))
            return Collect();

        if (collected && actionController.IsCurrentAction(typeof(DropAction)))
        {
            if (!isDropAble)
                return false;

            if (isDropping)
            {
                if (UIDropPoint.GetInstance().IsLegal)
                    StopDrop();
                else
                    return false;
            }
            else
                InitDrop();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Add this collectable to the inventory of a given player.
    /// </summary>
    /// <param name="player">player to add this collectable</param>
    /// <returns>true when it could be added to the player's inventory</returns>
    public bool AddInventory(Player player)
    {
        collected = player.AddInventoryItem(this);
        StoreValues();

        if (collected)
        {
            Inventory inventory = ActionController.GetInstance().GetInventory();
            inventory.UpdateDisplay(
                player.GetInventoryContentCount(),
                player.HasInventoryCapacity()
            );
            inventory.Insert(this);

            if (ActionController.GetInstance().IsCurrentAction(typeof(GrabAction)))
                AudioManager.GetInstance().PlaySound("grab");

            gameObject.SetActive(false);
            Fire("Collected", true);
            Fire("Dropped", false);
        }

        GameManager.GetInstance().UnHighlight();
        return collected;
    }

    /// <summary>
    /// Initialize the dropping of this collectable.
    /// </summary>
    private void InitDrop()
    {
        ActionController actionController = ActionController.GetInstance();
        GameManager gameManager = GameManager.GetInstance();
        UIGame uiGame = UIGame.GetInstance();
        UIDropPoint uiDropPoint = UIDropPoint.GetInstance();

        uiGame.SetCursorVisible(true);
        uiGame.SetCursorEnabled(false, true);
        UIToolTip.GetInstance().Hide();

        isDropping = true;
        col.enabled = false;

        actionController.CloseInventorybox();
        actionController.Show(false);

        uiGame.ShowObjectOnCursor(this);
        gameManager.ShowObjectPlaces(true, this);
        
        uiDropPoint.Show(true, this);

        uiGame.ShowEscape(true);
        uiGame.ShowObject(this);
    }

    /// <summary>
    /// Stop dropping this collectable.
    /// </summary>
    private void StopDrop()
    {
        isDropping = false;
        GameManager gameManager = GameManager.GetInstance();
        UIDropPoint uiDropPoint = UIDropPoint.GetInstance();
        UIGame uiGame = UIGame.GetInstance();

        if (IsHighlighted)
            gameManager.UnHighlight();

        Player player = gameManager.CurrentPlayer;
        // walk position is the interact position 
        objectPlace = uiDropPoint.ObjectPlace;
        Vector3 walkPosition = objectPlace.GetWalkPosition(this);
        Vector3 position = objectPlace.transform.position;

        if (objectPlace is ObjectPlace3D objectPlace3D)
        {
            Vector3 dropRotation = objectPlace3D.GetRotation();

            // rotate the walk position according to the rotation of the drop position
            // TODO check if this really works in any case
            walkPosition = Calc.RotatePointAroundPivot(
                walkPosition,
                position,
                dropRotation
            );
        }

        SetLayer((int)Layers.Invisible);
        uiGame.SetCursorVisible(false);
        uiGame.SetOverUI(true);
        gameManager.GotoAndInteract(
            walkPosition,
            position,
            InvokeDrop
        );

        uiDropPoint.Show(false);
        uiGame.ShowEscape(false);
        uiGame.HideObjectOnCursor();
        gameManager.ShowObjectPlaces(false);
    }

    /// <summary>
    /// Invoke dropping this collectable.
    /// </summary>
    private void InvokeDrop()
    {
        GameManager gameManager = GameManager.GetInstance();
        Player player = gameManager.CurrentPlayer;
        player.RemoveInventoryItem(this);
        UnsetInventorySlot();

        ActionController actionController = ActionController.GetInstance();

        Inventory inventory = actionController.GetInventory();
        inventory.UpdateDisplay(player.GetInventoryContentCount());

        SetObjectPlace(objectPlace);

        AudioManager.GetInstance().PlaySound("plopp.2");

        FinishDrop();
    }

    /// <summary>
    /// Puts this collectable to a given ObjectPlace.
    /// </summary>
    /// <param name="objectPlace">place for that collectable</param>
    public void SetObjectPlace(ObjectPlace objectPlace)
    {
        this.objectPlace = objectPlace;
        collected = false;
        Fire("Collected", false);
        Fire("Dropped", true);


        if (objectPlace is ObjectPlace3D objectPlace3D)
        {
            col.enabled = true;
            RestoreLayer();
            transform.gameObject.SetActive(true);
            transform.position = objectPlace3D.transform.position;
            transform.localRotation = Quaternion.Euler(objectPlace3D.GetRotation());
            objectPlace3D.SetCollectable(this);
        }
        else if (objectPlace is DrawerContentSlotUI drawerContentSlotUI)
        {
            drawerContentSlotUI.AddObject(this);
        }
    }

    /// <summary>
    /// Finish drop interaction and restore UI.
    /// Also used when drop action is cancelled.
    /// </summary>
    public void FinishDrop()
    {
        isDropping = false;
        col.enabled = true;

        UIDropPoint.GetInstance().Show(false);

        ActionController actionController = ActionController.GetInstance();
        actionController.Current.Finish();
        actionController.SetDefaultAction();
        actionController.UpdateToolTip();
        actionController.Show(true);

        UIGame uiGame = UIGame.GetInstance();
        uiGame.HideObject();
        uiGame.HideObjectOnCursor();
        uiGame.HideCursor(1f);
        uiGame.SetOverUI(uiGame.IsUIExclusive);
    }
        
    public override void MouseClick()
    {
        // Just handle drop action
        if (!ActionController.GetInstance().IsCurrentAction(typeof(DropAction)))
        {
            base.MouseClick();
            return;
        }

        if (!Input.GetMouseButtonUp(0))
            return;

        GameManager.GetInstance().Interact(this);
    }
}