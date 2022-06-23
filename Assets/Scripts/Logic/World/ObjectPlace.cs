using UnityEngine;

public abstract class ObjectPlace : MonoBehaviour
{
    public bool IsAvailable { get => null == collectable; }
    public Collectable Collectable { get => collectable; }

    protected Collectable collectable;

    public virtual void SetCollectable(Collectable collectable)
    {
        this.collectable = collectable;

        if (null == collectable)
            return;

        // TODO strange walk behaviour of current player on use action
        collectable.transform.SetParent(transform, true);
    }

    public abstract Vector3 GetWalkPosition(Collectable collectable);
    public abstract Vector3 GetLookAtPosition();
}
