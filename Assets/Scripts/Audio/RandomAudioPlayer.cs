using System.Collections;
using UnityEngine;

/// <summary>
/// This class is used to randomly play sound data as soon
/// as the object where this script is located comes into view.
/// Possible applications might be bird chirping on trees for instance.
/// </summary>
public class RandomAudioPlayer : MonoBehaviour
{
    /// <summary>
    /// Soundtokens to choose randomly
    /// </summary>
    public string[] soundTokens;
    /// <summary>
    /// Absolute probability distribution of the sound tokens
    /// </summary>
    public int[] tokenCount;
    public float minWait = 2f;
    public float maxWait = 5f;
    public AudioSource audioSource;

    private int index1 = -1;
    private int index2 = -1;
    private bool isPlaying;
    
    private void OnBecameVisible()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        index1 = Random.Range(0, 100) % soundTokens.Length;
        StartCoroutine(IEAudio());
    }

    private void OnBecameInvisible()
    {
        isPlaying = false;
    }

    private IEnumerator IEAudio()
    {
        yield return new WaitForSecondsRealtime(Random.Range(0f, minWait) + 0.5f * minWait);

        while (isPlaying)
        {
            if (!audioSource.isPlaying)
            {
                index2 = Random.Range(0, 100) % tokenCount[index1];
                index2++;
                string soundID = soundTokens[index1] + "." + (index2 < 10 ? "0" : "") + index2.ToString();
                AudioManager.GetInstance().PlaySound(soundID, gameObject, 1f + 0.075f * Random.value, audioSource);
                float time = AudioManager.GetInstance().GetSoundLength(soundID) + 1f;
                yield return new WaitForSecondsRealtime(time);
            }

            yield return new WaitForSecondsRealtime(Random.Range(0f, maxWait) + minWait);
        }
    }
}