using UnityEngine;

/// <summary>
/// A sound holds all needed settings that are used
/// to play this audio clip
/// </summary>
[System.Serializable]
public class Sound : MonoBehaviour
{
    // use to identify the Sound
    public string soundID;
    public AudioClip clip;
    [Range(0,5)]
    public float pitch = 1.0f;
    [Range(0, 1)]
    public float volume = 1.0f;
    [Range(-1,1)]
    public float panStereo = 0f;
    [Range(0f,1f)]
    public float spartialBlend = 0f;
    
    public float maxDistance = 15f;

    public AudioRolloffMode rolloffmode = AudioRolloffMode.Linear;
    public bool loop = false;
    public bool paused = false;

    /// <summary>
    /// Duration of the Audio Clip
    /// </summary>
    /// <returns>duration in seconds</returns>
    public float Length { get => null != clip ? clip.length : 0f; }
}
