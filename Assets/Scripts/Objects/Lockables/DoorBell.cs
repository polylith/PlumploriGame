using System.Collections.Generic;
using UnityEngine;
using Action;

public class DoorBell : Interactable
{
    public Door door;
    public AudioSource audioSource;

    public void SetDoor(Door door)
    {
        this.door = door;

        if (null != door)
            SetPrefix(door.Prefix + "Bell");
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override List<string> GetAttributes()
    {
        List<string> list = new List<string>
        {
            "IsPressed"
        };
        return list;
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
    }

    public override void RegisterGoals()
    {
        Formula f = new Negation(WorldDB.Get(Prefix + "IsPressed"));
        WorldDB.RegisterFormula(new Implication(f, null));
    }

    public override int IsInteractionEnabled()
    {
        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(UseAction)))
            return base.IsInteractionEnabled();

        return ShouldBeEnabled() ? 1 : -1;
    }

    public override bool Interact(Interactable interactable)
    {
        ActionController actionController = ActionController.GetInstance();
                
        if (!actionController.IsCurrentAction(typeof(UseAction)))
            return base.Interact(interactable);

        Press();
        return true;
    }

    public void Press()
    {
        if (audioSource.isPlaying)
            return;

        string soundId = (null != door ? door.doortype : "door" ) + ".bell.1";
        AudioManager.GetInstance()?.PlaySound(soundId, gameObject, 1f, audioSource);
        GameEvent.GetInstance()?.Execute(SendCommand, Random.Range(1f, 2f));
        Fire("IsPressed", true);
    }

    private void SendCommand()
    {
        if (null == door)
            return;

        bool isLocked = door.IsLocked;

        if (isLocked)
        {
            door.SetRelock();
        }

        Player player = GameManager.GetInstance().CurrentPlayer;
        door.SetOpener(player, player.IsNPC);
        door.Interact();
    }

    protected override bool ShouldBeEnabled()
    {
        return null != door && !door.IsOpen;
    }
}