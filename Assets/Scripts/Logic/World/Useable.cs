using UnityEngine;
using Action;

/// <summary>
/// <para>
/// Useables are special collectables that can be used
/// either on their own or in combination with a required
/// type.
/// </para>
/// <para>
/// All objects that require another interactable to be
/// used MUST be collectable, e.g. a key is collectable
/// and useable with an object of type lockable, but the
/// lockable is not collectable.
/// </para>
/// <para>
/// Objects that should respond to the use action but are
/// not collectable MUST NOT be useables, and instead MUST
/// inherit from the more general class Interactable.
/// </para>
/// </summary>
public abstract class Useable : Collectable
{
    /// <summary>
    /// Type name of the interactable to interact with.
    /// This is given as a string. This way is unfortunately
    /// sensitive to typos.
    /// </summary>
    public string requiredTypeName;
    /// <summary>
    /// The system type of the required interactle that
    /// is retrieved at runtime.
    /// </summary>
    private System.Type type;

    private void Awake()
    {
        SetRequiredType(requiredTypeName);
    }

    public override void Restore()
    {
        if (!IsCollected)
            base.Restore();

        gameObject.SetActive(true);
    }

    public override int IsInteractionEnabled()
    {
        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(UseAction)) 
            && !actionController.IsCurrentAction(typeof(PointerAction)))
            return base.IsInteractionEnabled();

        if (actionController.IsCurrentAction(typeof(PointerAction)))
            return 1;

        return actionController.CheckCurrentActionState();
    }

    public abstract int IsUseable(Interactable inter);

    public virtual int IseUseable(Interactable inter)
    {
        if (null == inter)
            return 0;

        if (!CheckRequiredType(inter))
            return -1;

        return IsUseable(inter);
    }

    public virtual bool RequiresType()
    {
        return null != type;
    }

    public virtual bool CheckRequiredType(Interactable inter)
    {
        if (null == type)
            return true;

        System.Type type2 = inter.GetType();
        bool res = type2.Equals(type) || type2.IsSubclassOf(type);

        if (res)
            return true;

        Component[] comps = inter.transform.GetComponents<Component>();

        if (null == comps || comps.Length == 0)
            return false;

        foreach (Component comp in comps)
        {
            type2 = comp.GetType();
            res = type2.Equals(type) || type2.IsSubclassOf(type);

            if (res)
                return true;
        }

        return false;
    }

    public void SetRequiredType(string sType)
    {
        type = System.Type.GetType(sType, false);
        requiredTypeName = null != type ? sType : null;
    }
}