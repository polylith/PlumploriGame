using System.Collections.Generic;
using Action;
using UnityEngine;

/// <summary>
/// This class allows indirect interaction with the actual
/// Interactable. The proxy does not provide any interaction
/// itself, but just passes the interaction to the referenced
/// Interactables.
/// </summary>
/// <remarks>
/// For example, with a cabinet, that consists of a box and a door.
/// The door is an Interactable of type Openable.
/// The actual Interactable is the door.
/// However, the cabinet as a whole should enable interaction
/// so that it can be opened and closed again.
/// If the cabinet also contains other interactable objects,
/// a problem comes up since the box collider of the cabinet
/// would cover the access to the objects inside.
/// The box of the cabinet consists of more than one collider,
/// so that the objects inside the cabinet are not hidden
/// when the door is opened.
/// </remarks>
public class InteractableProxy : Interactable
{
    public Interactable[] interactables;
    public Collider[] additionalColliders;

    private void Start()
    {
        foreach (Interactable interactable in interactables)
        {
            interactable.col.enabled = false;
        }

        foreach (Collider other in additionalColliders)
        {
            other.enabled = false;
        }
    }

    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    public override string GetDescription()
    {
        string description = "";

        foreach (Interactable interactable in interactables)
        {
            // TODO line breaks???
            description += interactable.GetDescription();
        }

        return description;
    }

    public override void RegisterGoals()
    {
        // Nothing to do
    }

    protected override void RegisterAtoms()
    {
        // Nothing to do
    }

    public override Vector3 GetInteractionPosition()
    {
        if (null == interactables || interactables.Length == 0)
            return base.GetInteractionPosition();

        return interactables[0].GetInteractionPosition();
    }

    public override int IsInteractionEnabled()
    {
        if (null != interactables && interactables.Length > 0)
            return interactables[0].IsInteractionEnabled();

        return base.IsInteractionEnabled();
    }

    public override bool Interact(Interactable interactable)
    {
        if (null == interactables || interactables.Length == 0)
            return base.Interact(interactable);

        if (ActionController.GetInstance().IsCurrentAction(typeof(PointerAction)))
        {
            interactables[0].Interact(interactable);
            return true;
        }

        bool res = false;

        foreach (Interactable proxyInteractable in interactables)
        {
            res |= proxyInteractable.Interact(interactable);
        }

        if (res)
        {
            ToggleColliders();
        }            

        return res;
    }

    private void ToggleColliders()
    {
        bool isEnabled = !col.enabled;
        col.enabled = isEnabled;
        isEnabled = !isEnabled;

        foreach (Collider other in additionalColliders)
        {
            other.enabled = isEnabled;
        }
    }
}
