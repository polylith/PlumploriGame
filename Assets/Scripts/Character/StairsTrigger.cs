using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StairsTrigger : MonoBehaviour
{
    public Collider col;

    private void Awake()
    {
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Character chctrl = other.GetComponent<Character>();

        if (null == chctrl)
            return;

        chctrl.SetStairWalk(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Character chctrl = other.GetComponent<Character>();

        if (null == chctrl)
            return;

        chctrl.SetStairWalk(false);
    }
}
