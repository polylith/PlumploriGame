using System.Collections;
using UnityEngine;

/// <summary>
/// A cuckoo clock is a special type of analog clock
/// featuring an animation for the cuckoo on every
/// full hour and a gong every quarter hour.
///
/// It is inherited from Interactable but it does not
/// provide any specific interaction.
/// </summary>
public class CuckooClock : AnalogClock
{
    private static Quaternion[] rotations = new Quaternion[] {
            Quaternion.Euler(0f, -125f, 0f),
            Quaternion.Euler(0f, 125f, 0f),
            Quaternion.Euler(0f, 0f, 0f)
        };

    public GameObject[] doors;
    public Animator anim;
    public AudioSource audioSource;
    private IEnumerator iePlay;

    public override void SetTime(float h, int m, int s)
    {
        // Don't start animation if it's already active
        if (null == iePlay)
        {
            // every quarter hour
            if (m % 15 == 0)
            {
                int count = m / 15;

                // 15, 30, 45
                if (count > 0)
                    PlayGong(count);
                else // full hour
                    PlayCuckoo((int)h % 12);
            }
        }
        
        base.SetTime(h, m, s);
    }

    private void UnsetPlay()
    {
        iePlay = null;
    }

    public void PlayGong(int count)
    {
        if (null != iePlay)
            return;

        iePlay = IEGong(count);
        StartCoroutine(iePlay);
    }

    private IEnumerator IEGong(int count)
    {
        int i = 0;
        string soundID = "clock.gong";
        AudioManager audioManager = AudioManager.GetInstance();

        while (i < count)
        {
            audioManager.PlaySound(
                soundID,
                gameObject,
                1f,
                audioSource
            );

            while (audioSource.isPlaying)
                yield return null;

            i++;
        }

        yield return null;

        /* 
         * Wait more than one minute to unset playing, 
         * otherwise it will be played again! 
         */
        GameEvent.GetInstance()?.Execute(UnsetPlay, 65f);
    }

    public void PlayCuckoo(int h)
    {
        if (null != iePlay)
            return;

        if (h == 0)
            h = 12;

        iePlay = IECuckoo(h);
        StartCoroutine(iePlay);
    }

    private IEnumerator IECuckoo(int h)
    {
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlaySound("cuckoo.start", gameObject, 1f, audioSource);

        float f = 0f;
 
        while (f <= 1f)
        {
            doors[0].transform.localRotation = Quaternion.Lerp(doors[0].transform.localRotation, rotations[0], f);
            doors[1].transform.localRotation = Quaternion.Lerp(doors[1].transform.localRotation, rotations[1], f);

            yield return null;

            f += Time.deltaTime;
        }

        doors[0].transform.localRotation = rotations[0];
        doors[1].transform.localRotation = rotations[1];

        while (audioSource.isPlaying)
            yield return null;

        int i = 0;
        string soundID = "cuckoo";

        while (i < h)
        {
            anim.SetTrigger("cuckoo");

            audioManager.PlaySound(soundID, gameObject, 1f, audioSource);

            while (audioSource.isPlaying)
                yield return null;

            i++;
        }
        
        anim.SetTrigger("stop");

        yield return new WaitForSecondsRealtime(0.1f);

        audioManager.PlaySound("cuckoo.start", gameObject, 1f, audioSource);

        f = 0f;
        
        while (f <= 1f)
        {
            doors[0].transform.localRotation = Quaternion.Lerp(doors[0].transform.localRotation, rotations[2], f);
            doors[1].transform.localRotation = Quaternion.Lerp(doors[1].transform.localRotation, rotations[2], f);

            yield return null;

            f += Time.deltaTime;
        }

        doors[0].transform.localRotation = rotations[2];
        doors[1].transform.localRotation = rotations[2];

        while (audioSource.isPlaying)
            yield return null;

        /* 
         * Wait more than one minute to unset playing, 
         * otherwise it will be played again! 
         */
        GameEvent.GetInstance()?.Execute(UnsetPlay, 65f);
    }
}