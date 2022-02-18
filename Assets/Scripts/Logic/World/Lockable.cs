using System.Collections.Generic;
using Creation;

/// <summary>
/// <para>
/// This abstract class is the base class for
/// lockable objects. They are only useable with
/// a key in use interaction.
/// </para>
/// <para>
/// For more details on keys and lockables, read
/// the comments in class Key.
/// </para>
/// </summary>
public abstract class Lockable : Interactable
{
    public int InitId = -1;
    public int Id { get => id; }
    /// <summary>
    /// If the Id if < 0 this door is not lockable
    /// </summary>
    public bool IsLockable { get => id >= 0; }
    public bool IsLocked { get => isLocked; }
    public bool IsAutoOpening { get => isAutoOpening; }
    public bool IsOpen { get => isOpen; }
    public bool isLocked = false;

    private int id = int.MinValue;
    
    protected bool isOpen = false;
    protected bool isAutoOpening;

    private void Awake()
    {
        if (InitId > 0)
        {
            SetId(InitId);
        }
    }

    public void SetId(int id)
    {
        this.id = id;
    }
        
    public abstract void SetAutoOpening(bool isAutoOpening);
    protected abstract void Open();
    public abstract void SetLocked(bool isLocked);
    public abstract void Close();
    public abstract bool Unlock(Key key);
    public abstract bool Lock(Key key);
    public abstract bool Interact();

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
                "IsLocked"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
        SetDelegate("IsLocked", SetLocked);
    }

    public override void RegisterGoals()
    {
        Formula f = new Negation(WorldDB.Get(Prefix + "IsLocked"));
        WorldDB.RegisterFormula(new Implication(f, null));
    }

    public override void RegisterCurrentState()
    {
        WorldDB.RegisterCurrentState(Prefix + "IsLocked", isLocked);
    }

    public override bool Interact(Interactable interactable)
    {
        if (null == interactable)
            return Interact();

        if (!(interactable is Key))
            return false;

        Key key = (Key)interactable;
        bool res = isLocked ? Unlock(key) : Lock(key);

        if (!res)
            return false;

        Fire("IsLocked", isLocked);
        return true;
    }
}