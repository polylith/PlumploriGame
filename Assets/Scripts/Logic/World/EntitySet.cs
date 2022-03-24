using UnityEngine;

/// <summary>
/// This abstract class defines all functions and
/// methods for different subtypes of entity collections.
/// </summary>
public abstract class EntitySet : MonoBehaviour
{
    public abstract int Count { get; }
    public abstract bool IsReady { get; }
    public abstract Entity Instantiate(EntityData entityData);
    public abstract void Init();
}