using UnityEngine;

/// <summary>
/// This abstract class defines all functions and
/// methods for different sbutypes of entity collections.
/// </summary>
public abstract class EntitySet : MonoBehaviour
{
    public abstract Entity Instantiate(EntityData entityData);
    public abstract void Init();
}