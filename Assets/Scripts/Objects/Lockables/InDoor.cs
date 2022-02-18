using UnityEngine;

public class InDoor : Door
{
    [HideInInspector]
    public int i;
    [HideInInspector]
    public int j;
    [HideInInspector]
    public int wallIndex;

    private void Start()
    {
        Init();
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    private void OnTriggerEnter(Collider other)
    {
        Character character = other.gameObject.GetComponent<Character>();
        
        if (null == character || Opener != character)
            return;

        Open(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        Close();
    }
}