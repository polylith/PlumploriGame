using UnityEngine;

public class FenceDoor : Door
{
    private void Start()
    {
        Init();
        langKey = Language.LangKey.FenceDoor.ToString();
    }
    
    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override void RegisterGoals()
    {
        Debug.Log("!!! TODO !!!");
    }

    private void OnTriggerEnter(Collider other)
    {
        Character chchtrl = other.gameObject.GetComponent<Character>();

        if (null == chchtrl)
            return;

        SetAutoOpening(chchtrl.IsNPC);
        Open(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        Close();
    }
}
