using UnityEngine;

public class OutDoor : Door
{
    public DoorBell doorBell;

    private void Start()
    {
        Init();
    }

    public void RemoveBell()
    {
        if (null == doorBell)
            return;

        Destroy(doorBell.gameObject);
        doorBell = null;
    }

    public void InitBell()
    {
        if (null == doorBell)
            return;

        doorBell.SetDoor(this);
        doorBell.Register();
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    private void OnTriggerExit(Collider other)
    {
        Close();
    }
}