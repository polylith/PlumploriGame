using UnityEngine;

/// <summary>
/// This class holds all the data for the GameManager
/// to dynamically load a room as a scene.
/// </summary>
[System.Serializable]
public class RoomConfig : MonoBehaviour
{
    public Room Room { get; set; }

    public bool unloadOnLeave = true;
    public int arrayIndex; 
    public int sceneIndex;
    public Material skyBoxMaterial;
    public RoomTransition[] roomTransitions;
}
