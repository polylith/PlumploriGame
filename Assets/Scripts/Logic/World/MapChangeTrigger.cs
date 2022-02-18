using Action;
using UnityEngine;

/// <summary>
/// This script can be attached to a GameObject and requires
/// a collider as a trigger. When the active character exits
/// the trigger, the map is updated.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MapChangeTrigger : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();

        if (null == character || character.IsNPC)
            return;

        ActionController.GetInstance().UpdateMap();
    }
}
