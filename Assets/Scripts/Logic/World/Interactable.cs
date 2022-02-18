using Action;
using Movement;
using UnityEngine;

/// <summary>
/// Interactable is the abstract superclass for all objects with interaction.
/// Each interactive object needs at least one collider to be detected
/// by raycasting in the 3D world.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class Interactable : Entity
{
    /*
     * This property indicates whether this object is currently 
     * highlighted. It is just a property that is not displayed 
     * in the Unity Inspector.
     */
    public bool IsHighlighted { get; set; }

    /*
     * This property indicates whether this object has an
     * interactable UI. It is just a property that is not 
     * displayed in the Unity Inspector.
     */
    public bool HasInteractableUI { get => null != interactableUIPrefab; }

    /*
     * An object-specific UI can provide the interaction. 
     * To activate this UI, UseAction first has to be selected.
     * This UI must be a separate prefab that has been designed 
     * in the GameUI before. It will only be instantiated at 
     * runtime, if no instance already exists, to avoid multiple 
     * copies of the same UI.
     */
    public InteractableUI interactableUIPrefab;
    public InteractableUI InteractableUI { get; private set; }

    /*
     * The Look Position is used to create a Texture2D with 
     * the ObjectCamera at runtime. 
     * This position is just an empty GameObject. 
     * The relative position and rotation should be such that 
     * the object can be seen at a good angle.
     * Not every object needs this position. 
     * Collectable objects in particular need it because they 
     * are displayed in the Inventory using this Texture2D.
     */
    public Transform lookPos;

    /*
     * The interaction position defines how the character 
     * should be positioned to interact with this object.
     * The position should be chosen so that it is on the 
     * navigation mesh if possible, or at least not too 
     * far away. The character should not get into the mesh
     * of the object, especially if there are parts of the 
     * object moving by animations.
     * The interaction position should never be accessed 
     * directly, even if it is a public attribute.
     */
    public Transform interactPos;

    // The main collider, there might be additional colliders.
    public Collider col;

    // This Texture2D is dynamically created at runtime
    private Texture2D objectIcon;

    /// <summary>
    /// This method instantiates and initializes the UI prefab
    /// when available. The parameter
    /// <paramref name="uiExclusiveMode">uiExclusiveMode</paramref>
    /// disables the ineraction with the 3D world while the UI
    /// is displayed.
    /// </summary>
    /// <param name="uiExclusiveMode">true = disable 3D world</param>
    protected void InitInteractableUI(bool uiExclusiveMode)
    {
        if (null == InteractableUI)
        {
            InteractableUI = InteractableUI.GetInstance(interactableUIPrefab);
        }

        InteractableUI.SetInteractable(this);
        InteractableUI.uiExclusiveMode = uiExclusiveMode;
    }

    /// <summary>
    /// This function returns the interaction position if exists.
    /// The interaction position should never be accessed directly,
    /// even if it is a public attribute.
    /// </summary>
    /// <returns>interaction position as Vector3</returns>
    public virtual Vector3 GetInteractionPosition()
    {
        return null != interactPos
            ? interactPos.position
            : transform.position;
    }

    /// <summary>
    /// Just set a Texture2D. This method is only called
    /// by the ObjectCamera if there is no image of the
    /// object yet or its appearance has changed.
    /// </summary>
    /// <param name="tex">image of this object</param>
    public void SetObjectIcon(Texture2D tex)
    {
        objectIcon = tex;
    }

    /// <summary>
    /// This function checks if an image of the object
    /// already exists and returns it.
    /// </summary>
    /// <returns>image of this object as Texture2D</returns>
    public Texture2D GetObjectIcon()
    {
        CheckObjectIcon();
        return objectIcon;
    }

    /// <summary>
    /// This function checks if an image of the object
    /// already exists. If there is no image yet, the
    /// ObjectCamera is called to create an one. 
    /// </summary>
    public void CheckObjectIcon()
    {
        if (null == objectIcon)
        {
            objectIcon = ObjectCamera.GetInstance()
                .CreateObjectIcon(gameObject, 250, 200, lookPos);
        }
    }

    /// <summary>
    /// This method displays a cross in the 3D world at the
    /// interaction position. The method is called with the
    /// parameter <paramref name="visible">visible</paramref>
    /// to show or hide this position.
    /// </summary>
    /// <param name="visible">true = show or false = hide</param>
    private void ShowInteractPosition(bool visible)
    {
        UIDropPoint uiDropPoint = UIDropPoint.GetInstance();

        if (visible)
        {
            Vector3 pos = GetInteractionPosition();
            pos = NavMeshMover.GetWalkAblePoint(pos);
            RaycastHit hit = Calc.GetPointOnGround(pos);
            uiDropPoint.ShowPointer(hit.point, hit.normal, 1);
            uiDropPoint.IsFreezed = true;
            return;
        }

        uiDropPoint.ResetPointer();
    }

    /// <summary>
    /// This function determines whether the currently selected
    /// action is applicable to this interactive object before
    /// it is executed. This function is called on MouseOver on
    /// this object.
    /// 
    /// This function must be overridden in specific subclasses.
    /// Overriding this function in subclasses should only go on
    /// its specific actions, that are intended for this specific
    /// interactable. Any handling of other actions is done in the
    /// respective superclass.
    /// 
    /// The return value is an integer with the interpretation
    /// -1 if the interaction is not possible,
    /// otherwise 1.
    /// 0 is generally not used as a return value.
    /// It would describe an indeterminate or unknown state.
    /// </summary>
    /// <returns>integer representing the interaction state</returns>
    public virtual int IsInteractionEnabled()
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsCurrentAction(typeof(LookAction))
            || actionController.IsCurrentAction(typeof(PointerAction)))
            return 1;

        if (actionController.IsCurrentAction(typeof(UseAction))
            && null != InteractableUI)
            return InteractableUI.IsEnabled ? 1 : -1;

        return -1;
    }

    /// <summary>
    /// This function is called when the interaction is actually applied.
    /// For most of the interactions the parameter
    /// <paramref name="interactable">interactable</paramref> is null.
    /// It is just used in the case of the UseAction when a second object
    /// is required.
    /// 
    /// This function must be overridden in specific subclasses.
    /// Overriding this function in subclasses should only go on
    /// its specific actions, that are intended for this specific
    /// interactable. Any handling of other actions is done in the
    /// respective superclass.
    /// 
    /// Returns a simple binary value indicating whether the interaction
    /// has been applied.
    /// </summary>
    /// <param name="interactable">other object to interact</param>
    /// <returns>true when interaction was applied successfully</returns>
    public virtual bool Interact(Interactable interactable = null)
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsCurrentAction(typeof(UseAction))
            && null != interactableUIPrefab)
        {
            if (this is Useable useable && !useable.IsCollected)
            {
                useable.Collect();
            }

            InteractableUI = InteractableUI.GetInstance(interactableUIPrefab);
            InteractableUI.SetInteractable(this);
            InteractableUI.Show();
            return InteractableUI.IsEnabled;
        }

        if (!actionController.IsCurrentAction(typeof(LookAction)))
        {
            return false;
        }

        ShowDescription();
        return true;
    }

    /// <summary>
    /// This function might no longer be necessary and
    /// will be removed in future.
    /// </summary>
    /// <returns>the enabled state</returns>
    protected virtual bool ShouldBeEnabled()
    {
        return false;
    }

    /// <summary>
    /// This method handles the behavior when the cursor
    /// is hovering over the interactable. This method was
    /// originally intended to be overrideable, but it turned
    /// out that a generic handling was already fine.
    /// </summary>
    public void MouseOver()
    {
        UIGame uiGame = UIGame.GetInstance();
        ActionController actionController = ActionController.GetInstance();
        GameManager gameManager = GameManager.GetInstance();

        // doors need to know the opener
        if (this is Door door)
        {
            door.SetOpener(gameManager.CurrentPlayer, true);
        }

        int enabledState = actionController.SetActionState(this);
        gameManager.UnHighlight();

        if (!(this is Collectable collectable) || !collectable.IsCollected)
        {
            gameManager.Highlight(this, enabledState);

            if (!actionController.IsDropActionActive())
                ShowInteractPosition(true);
        }

        if (enabledState > -1)
        {
            uiGame.SetCursorEnabled(enabledState == 1, true);
        }
        else
        {
            uiGame.SetCursorDisabled();
        }
        
        actionController.Current.UpdateToolTip();
    }

    /// <summary>
    /// This method handles the behavior when the cursor is
    /// no longer over the interactable.
    /// </summary>
    public void MouseExit()
    {
        UIGame.GetInstance().SetCursorEnabled(false, true);
        GameManager.GetInstance().UnHighlight();
        ActionController actionConstroller = ActionController.GetInstance();
        actionConstroller.UpdateToolTip();
        ShowInteractPosition(false);

        if (actionConstroller.IsCurrentActionActive())
            return;

        actionConstroller.UnsetActionState(this);
    }

    /// <summary>
    /// This method handles the behavior when the left mouse button is
    /// pressed while the cursor is over this interactable to invoke
    /// the current selected action.
    ///
    /// This method must be overridden in specific subclasses.
    /// Overriding this method in subclasses should only go on its specific
    /// action, that is intended for this specific interactable. Any handling
    /// of other actions is done in the respective superclass.
    /// </summary>
    public virtual void MouseClick()
    {
        /*
         * Nothing should happen if not the left mouse button 
         * has been pressed.
         */
        if (!Input.GetMouseButtonUp(0))
        {
            return;
        }

        GameManager.GetInstance().Interact(this);
    }

    /// <summary>
    /// Unity method to handle mouse interaction
    /// </summary>
    private void OnMouseEnter()
    {
        /* 
         * If UI is in exclusive mode the interaction with the 3D world
         * is disabled. Same if the object is already highlighted.
         */
        if (IsHighlighted || UIGame.GetInstance().IsCursorOverUI)
        {
            return;
        }

        MouseOver();
    }

    /// <summary>
    /// Unity method to handle mouse interaction
    /// </summary>
    private void OnMouseExit()
    {
        /* 
         * if UI is in exclusive mode the interaction with the 3D world
         * is disabled.
         */
        if (UIGame.GetInstance().IsCursorOverUI)
        {
            return;
        }

        MouseExit();
    }

    /// <summary>
    /// Unity method to handle mouse interaction
    /// </summary>
    private void OnMouseUpAsButton()
    {
        /* 
         * If UI is in exclusive mode the interaction with the 3D world
         * is disabled. Only react on left mouse button.
         */
        if (UIGame.GetInstance().IsCursorOverUI || !Input.GetMouseButtonUp(0))
        {
            return;
        }

        MouseClick();
    }
}
