using UnityEngine;

/// <summary>
/// This class handles the playback of audio on collision.
/// </summary>
[RequireComponent(typeof(AudioSource), typeof(Collider))]
public class SoundTrigger : MonoBehaviour
{
    public string soundID;
    public float pitch = 1f;
    public float r = 0f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Trigger()
    {
        AudioManager.GetInstance()?.PlaySound(soundID, gameObject, pitch + r * Random.value);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!audioSource.isPlaying)
            Trigger();
    }
}
