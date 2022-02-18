using UnityEngine;

/// <summary>
/// This abstract class is the base class for a map to
/// display the current position of the active character.
/// </summary>
public abstract class Plan : MonoBehaviour
{
    public Sprite[] sprites;
    public Sprite positionMarker;
    
    public abstract Vector4 UpdatePositionMarker();
    public abstract Texture2D GetTexture();
    public abstract string GetText();
}