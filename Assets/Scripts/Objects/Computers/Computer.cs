using System.Collections.Generic;
using UnityEngine;

public class Computer : Interactable
{
    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override void RegisterGoals()
    {
        // TODO
    }

    protected override void RegisterAtoms()
    {
        // TODO
    }

    private void Start()
    {
        InitInteractableUI(true);
    }
}
