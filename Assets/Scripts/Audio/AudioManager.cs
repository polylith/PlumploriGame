using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// The AudioManager is a singleton that takes care of everything
/// related to the playback of sounds.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager GetInstance()
    {
        return ins;
    }

    // Pitch change constants based on an Ionian scale
    public static float[] tunes = new float[] {
        0f,
        Mathf.Pow(2f, 2f / 12f),
        Mathf.Pow(2f, 4f / 12f),
        Mathf.Pow(2f, 5f / 12f),
        Mathf.Pow(2f, 7f / 12f),
        Mathf.Pow(2f, 9f / 12f),
        Mathf.Pow(2f, 11f / 12f)
    };

    private static readonly float defaultVolume = 0.9f;
    private static AudioManager ins;
    private static bool isAudioOn = true;
    private static float mainVolume = defaultVolume;

    /// <summary>
    /// Default audio listener when there is no other
    /// audio listener active
    /// </summary>
    public AudioListener defaultAudioListener;
    private AudioListener currentActiveAudioListener;

    private AudioSource[] sources;
    private Dictionary<int,Sound> soundDict;
    private Dictionary<string, Sound> dict;
    private Dictionary<string, AudioMapper> media;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            CheckAudioListener();
            InitAudioPrefs();
            InitSoundDict();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Checks whether there is an active audio listener
    /// </summary>
    private void CheckAudioListener()
    {
        if (null == currentActiveAudioListener)
        {
            currentActiveAudioListener = defaultAudioListener;
        }

        currentActiveAudioListener.enabled = true;
    }

    /// <summary>
    /// Activate an audio listener
    /// </summary>
    /// <param name="audioListener">AudioListener to activate</param>
    public void SwitchAudioListener(AudioListener audioListener)
    {
        if (null != currentActiveAudioListener)
        {
            if (currentActiveAudioListener == audioListener)
                return;

            currentActiveAudioListener.enabled = false;
        }

        currentActiveAudioListener = audioListener;
        CheckAudioListener();
    }

    /// <summary>
    /// At the transform, where the AudioManager as a GameObject
    /// is located, there can be several child transforms with
    /// sound objects, defining the audios. These data are loaded
    /// and stored in a dictionary.
    /// These sounds can be grouped logically into further transforms,
    /// and do not need to be located at a single transform.
    /// </summary>
    private void InitSoundDict()
    {
        soundDict = new Dictionary<int, Sound>();
        media = new Dictionary<string, AudioMapper>();
        sources = GetComponents<AudioSource>();
        dict = new Dictionary<string, Sound>();
        Sound[] sounds = GetComponentsInChildren<Sound>();

        if (null != sounds)
        {
            foreach (Sound sound in sounds)
            {
                AddSound(sound);
            }
        }
    }

    /// <summary>
    /// This method can be used to load sounds from a transform
    /// at a later stage. For example, this could be the case
    /// when a scene is loaded that has its own sounds that are
    /// not used elsewhere.
    /// These sounds can be grouped logically into further transforms,
    /// and do not need to be located at a single transform.
    /// </summary>
    /// <param name="trans">transform holding sound objects to load</param>
    public void LoadAudios(Transform trans)
    {
        if (null == trans)
            return;

        Sound[] sounds = trans.GetComponents<Sound>();

        if (null != sounds)
        {
            foreach (Sound sound in sounds)
            {
                AddSound(sound);
            }
        }

        // continue on child transforms
        foreach (Transform child in trans)
            LoadAudios(child);
    }

    /// <summary>
    /// This method can be used to unload sounds from a transform
    /// at a later stage. For example, this could be the case
    /// when a scene is loaded that has its own sounds that are
    /// not used elsewhere.
    /// </summary>
    /// <param name="trans">transform holding sound objects to unload</param>
    public void UnloadAudios(Transform trans)
    {
        if (null == trans)
            return;

        Sound[] sounds = trans.GetComponents<Sound>();

        if (null != sounds)
        {
            foreach (Sound sound in sounds)
            {
                if (dict.ContainsKey(sound.soundID))
                    dict.Remove(sound.soundID);
                else
                    Debug.LogError("Sound " + sound.soundID + " not existing!");
            }
        }

        foreach (Transform child in trans)
            UnloadAudios(child);
    }

    /// <summary>
    /// This method inserts a single sound object into the dictionary.
    /// The ids of the sounds must be unique. A duplicate id will not
    /// throw any exceptions, but will just be logged as an error.
    /// </summary>
    /// <param name="sound">Sound to be added to dictionary</param>
    private void AddSound(Sound sound)
    {
        if (null == sound)
            return;

        if (!dict.ContainsKey(sound.soundID))
            dict.Add(sound.soundID, sound);
        else
            Debug.LogError("Sound " + sound.soundID + " already exists!");
    }

    /// <summary>
    /// This method initializes the audio settings (e. g. volume, audio on/off),
    /// which can be individually changed by the user via an UI.
    ///
    /// These values are loaded from the Unity PlayerPrefs if there are any values,
    /// otherwise the default values are used.
    /// </summary>
    private void InitAudioPrefs()
    {
        if (PlayerPrefs.HasKey(Prefs.Keys.AUDIOON.ToString()))
        {
            try
            {
                int value = PlayerPrefs.GetInt(Prefs.Keys.AUDIOON.ToString());
                isAudioOn = (value == 0 ? false : true);
            }
            catch (System.Exception)
            {
                isAudioOn = true;
            }
        }

        if (PlayerPrefs.HasKey(Prefs.Keys.AUDIOVOLUME.ToString()))
        {
            try
            {
                mainVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefs.Keys.AUDIOVOLUME.ToString()));
            }
            catch (System.Exception)
            {
                mainVolume = defaultVolume;
            }
        }
    }

    /// <summary>
    /// Property to check and set whether audio should be played
    /// </summary>
    public bool IsAudioOn { get => isAudioOn; set => SetAudioOn(value); }

    /// <summary>
    /// Turns audio on or off and saves this value to Unity PlayerPrefs.
    /// </summary>
    /// <param name="isOn">true = on, false = off</param>
    private void SetAudioOn(bool isOn)
    {
        if (isAudioOn == isOn)
            return;

        isAudioOn = isOn;
        PlayerPrefs.SetInt(Prefs.Keys.AUDIOON.ToString(), (isAudioOn ? 1 : 0));
        UpdateMusic();
    }

    public void ToggleAudioOn()
    {
        SetAudioOn(!isAudioOn);        
    }

    /// <summary>
    /// If background music is played as audio, all currently playing
    /// music needs to be updated upon change.
    /// </summary>
    private void UpdateMusic()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            int id = sources[i].GetInstanceID();
            
            if (soundDict.ContainsKey(id))
            {
                Sound sound = soundDict[id];

                if (isAudioOn)
                {
                    sources[i].volume = mainVolume * sound.volume;

                    if (sound.paused && sound.loop)
                    {
                        sources[i].Play();
                        sound.paused = false;
                    }                    
                }
                else
                {
                    if (sound.loop)
                    {
                        sound.paused = true;
                    }

                    StartCoroutine(FadeSource(sources[i],1f));
                }
            }
        }
    }

    /// <summary>
    /// Property to get and set main audio volume
    /// </summary>
    public float MainVolume { get => mainVolume; set => SetMainVolume(value); }

    /// <summary>
    /// Sets the main audio volume and saves this value to Unity PlayerPrefs.
    /// </summary>
    /// <param name="volume">audio volume</param>
    private void SetMainVolume(float volume)
    {
        mainVolume = Mathf.Clamp01(volume);

        if (mainVolume == 0f)
        {
            isAudioOn = false;
            PlayerPrefs.SetInt(Prefs.Keys.AUDIOON.ToString(), 0);
            mainVolume = defaultVolume;
        }
        else
        {
            isAudioOn = true;
            PlayerPrefs.SetInt(Prefs.Keys.AUDIOON.ToString(), 1);
            mainVolume = volume;
        }

        UpdateMusic();
        PlayerPrefs.SetFloat(Prefs.Keys.AUDIOVOLUME.ToString(), mainVolume);
    }

    /// <summary>
    /// Provides the duration of a sound
    /// </summary>
    /// <param name="soundID">id of the sound</param>
    /// <returns>duration in seconds</returns>
    public float GetSoundLength(string soundID)
    {
        if (!dict.ContainsKey(soundID))
            return 0f;

        return dict[soundID].Length;
    }

    /// <summary>
    /// Checks whether a sound is currently being played on a given GameObject.
    /// If audio is off, this function always returns false
    /// </summary>
    /// <param name="soundID">id of the sound</param>
    /// <param name="obj">GameObject to check</param>
    /// <returns>true, if audio is on and sound is beeing player, otherwise false</returns>
    public bool IsPlaying(string soundID, GameObject obj)
    {
        if (!dict.ContainsKey(soundID) || !isAudioOn)
            return false;

        AudioSource audioSource = GetAudioSources(obj);
        return null != audioSource && audioSource.isPlaying;
    }

    /// <summary>
    /// Fades out the volume of an audio source if the audio source
    /// is currently playing, or fades in the audio source if it is
    /// not yet playing.
    /// </summary>
    /// <param name="audioSource">Audio Source to fade in/out</param>
    public void FadeStereoPan(AudioSource audioSource)
    {
        if (null == audioSource)
            return;

        StartCoroutine(IEFadeStereoPan(audioSource));
    }

    /// <summary>
    /// Coroutine fading the Audio Source in/out
    /// </summary>
    /// <param name="audioSource">Audio Source to fade in/out</param>
    private IEnumerator IEFadeStereoPan(AudioSource audioSource)
    {
        float value = audioSource.panStereo;
        float df = Time.deltaTime * (Random.value > 0.5f ? -1f : 1f);

        do
        {
            float f = 0f;

            while (f < 1f)
            {
                yield return null;

                audioSource.panStereo = value;
                value += df;
                value = Mathf.Clamp(value, -1f, 1f);
                f += Time.deltaTime;
            }

            df *= -1f;
        }
        while (audioSource.isPlaying);
    }

    /// <summary>
    /// Fades in a sound without specifying an audio source.
    /// </summary>
    /// <param name="soundID">sound to be faded</param>
    /// <param name="pitch">optional pitch</param>
    /// <param name="duration">optional duration of fading</param>
    /// <returns>the Audio Source used</returns>
    public AudioSource FadeSound(string soundID, float pitch = 1f, float duration = 1f)
    {
        return FadeSound(soundID, null, pitch, duration);
    }

    /// <summary>
    /// Fades in a sound on a given GameObject without specifying an audio source.
    /// </summary>
    /// <param name="soundID">sound to be faded</param>
    /// <param name="obj">GameObject to use an Audio Source</param>
    /// <param name="pitch">optional pitch</param>
    /// <param name="duration">optional duration of fading</param>
    /// <returns>the Audio Source used for fading</returns>
    public AudioSource FadeSound(string soundID, GameObject obj = null, float pitch = 1f, float duration = 1f)
    {
        if (!dict.ContainsKey(soundID) || !isAudioOn)
            return null;

        if (null == obj)
            obj = null != UIGame.GetInstance() ? UIGame.GetInstance().gameObject : gameObject;

        Sound sound = dict[soundID];

        sound.volume = 0f;
        AudioSource audioSource = Play(sound, obj, 0f, pitch);
        audioSource.Pause();

        Sequence seq = DOTween.Sequence()
            .OnPlay(() => audioSource.Play())
            .Append(audioSource.DOFade(mainVolume, duration))
            .Play();

        return audioSource;
    }

    /// <summary>
    /// Plays a sound without specifying an audio source.
    /// </summary>
    /// <param name="soundID">sound to be player</param>
    /// <param name="pitch">optional pitch</param>
    public void PlaySound(string soundID, float pitch = 1f)
    {
        GameObject obj = null != UIGame.GetInstance() ? UIGame.GetInstance().gameObject : gameObject;
        PlaySound(soundID, obj, pitch);
    }

    /// <summary>
    /// Plays a sound an a given GameObject with a specified audio source.
    /// </summary>
    /// <param name="soundID">sound to be played</param>
    /// <param name="obj">GameObject to use an Audio Source</param>
    /// <param name="pitch">pitch of the sound</param>
    /// <param name="audioSource">Audio Source to play</param>
    /// <returns>the Audio Source actually used</returns>
    /// TODO remove gameobjeect, not used any more
    public AudioSource PlaySound(string soundID, GameObject obj, float pitch, AudioSource audioSource)
    {
        if (!dict.ContainsKey(soundID) || !isAudioOn || null == audioSource || !audioSource.gameObject.activeInHierarchy)
            return null;

        try
        {
            Sound sound = dict[soundID];
            SetupAudioSource(audioSource, sound);

            if (pitch != 1f)
                audioSource.pitch *= pitch;

            if (!audioSource.isPlaying)
            {
                audioSource.time = 0f;
                audioSource.Play();
            }
        }
        catch (System.Exception) { }

        return audioSource;
    }

    public void Restart(string soundID, GameObject obj, float pitch = 1f)
    {
        if (!dict.ContainsKey(soundID) || !isAudioOn)
            return;

        string id = obj.GetInstanceID().ToString() + "/" + soundID;

        if (!media.ContainsKey(id))
        {
            PlaySound(soundID, obj, pitch);
            return;
        }

        AudioMapper map = media[id];
        List<AudioSource> list = map.GetList();

        foreach (AudioSource audioSource in list)
        {
            audioSource.pitch = pitch;

            if (!audioSource.loop || !audioSource.isPlaying)
            {
                audioSource.time = 0f;
                audioSource.Play();
            }
        }
    }

    public void PlaySound(string soundID, GameObject obj, float pitch = 1f, float volume = 1f)
    {
        if (!dict.ContainsKey(soundID) || !isAudioOn)
            return;

        Sound sound = dict[soundID];

        Play(sound,obj,volume,pitch);
    }

    public void PlayMusic(string soundID, float pitch = 1f)
    {
        if (!dict.ContainsKey(soundID) || !isAudioOn)
            return;

        Sound sound = dict[soundID];
        PlayMusic(sound,pitch);
    }

    private void PlayMusic(Sound sound,float pitch = 1f)
    {
        if (null == sound || !isAudioOn)
            return;

        AudioSource[] sources = GetSources();

        if (null == sources)
            return;

        SetupAudioSource(sources[0], sound);
        sources[0].pitch = pitch;
        int id = sources[0].GetInstanceID();
        soundDict[id] = sound;

        if (sources[1].isPlaying)
        {
            MixAudio(sources);
        }
        else
        {
            sources[0].Play();
        }
    }

    /// <summary>
    /// Sets up an audio source according to the specifications of a sound.
    /// </summary>
    /// <param name="audioSource">Audio Source to setup</param>
    /// <param name="sound">Sound defining the setup</param>
    private void SetupAudioSource(AudioSource audioSource, Sound sound)
    {
        /*
         * if current active audio listener is the default audio listener
         * or the audio source is not placed in 3D world, the sound should
         * not be spatial.
         */
        bool isSpatialSound = currentActiveAudioListener != defaultAudioListener
            && audioSource.gameObject.layer == 0;
        audioSource.clip = sound.clip;
        audioSource.loop = sound.loop;
        audioSource.pitch = sound.pitch;
        audioSource.volume = sound.volume * mainVolume;
        audioSource.panStereo = sound.panStereo;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = isSpatialSound ? 1f : 0f;
        audioSource.rolloffMode = sound.rolloffmode;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = sound.maxDistance;

        if (audioSource.spatialBlend > 0f && null != Camera.main)
        {
            RaycastHit[] results = new RaycastHit[10];
            int count = Physics.RaycastNonAlloc(Camera.main.transform.position, audioSource.gameObject.transform.position - Camera.main.transform.position,
                results, Vector3.Distance(Camera.main.transform.position, audioSource.gameObject.transform.position), 0);

            if (count > 0)
            {
                Debug.Log("Count " + count);
                audioSource.volume *= Mathf.Pow(0.5f, count);
            }
        }
    }

    public void StopSound(string soundID, GameObject obj = null, bool instant = false)
    {
        if (null == obj)
            obj = null != UIGame.GetInstance() ? UIGame.GetInstance().gameObject : gameObject;

        string id = obj.GetInstanceID().ToString() + "/" + soundID;

        if (!media.ContainsKey(id))
            return;

        AudioMapper map = media[id];
        List<AudioSource> list = map.GetList();

        foreach (AudioSource audioSource in list)
        {
            if (instant)
                audioSource.Stop();
            else
                StartCoroutine(FadeSource(audioSource, 1f));
        }

        media.Remove(id);
    }

    public AudioSource Play(Sound sound, GameObject obj, float volume = 1f, float pitch = 1f)
    {
        if (null == sound || !isAudioOn)
            return null;
                
        AudioSource audioSrc = GetAudioSources(obj);

        if (null == audioSrc)
            return null;

        if (audioSrc.isPlaying)
            audioSrc.Stop();

        string id = obj.GetInstanceID().ToString() + "/" + sound.soundID;

        AudioMapper map = null;

        if (media.ContainsKey(id))
        {
            map = media[id];
        }
        else
        {
            map = new AudioMapper(id);
            media.Add(id, map);
            GameEvent.GetInstance()?.Execute(map.Check, 2f);
        }

        List<AudioSource> list = map.GetList();
        list.Add(audioSrc);
        SetupAudioSource(audioSrc, sound);

        if (pitch != 1f)
            audioSrc.pitch *= pitch;

        if (volume < 1f)
            audioSrc.volume *= volume;

        if (sound.loop)
            audioSrc.Play();
        else
            audioSrc.PlayOneShot(sound.clip);

        return audioSrc;
    }

    public void RemoveId(string id)
    {
        if (media.ContainsKey(id))
            media.Remove(id);
    }

    private AudioSource GetAudioSources(GameObject obj)
    {
        if (null == obj)
            return null;

        AudioSource[] audioSrcs = obj.GetComponents<AudioSource>();
        AudioSource selectedSource = null;

        int i = 0;

        if (null != audioSrcs)
        {
            while (i < audioSrcs.Length)
            {
                if (!audioSrcs[i].isPlaying)
                {
                    selectedSource = audioSrcs[i];
                    break;
                }
                else
                    i++;
            }
        }

        if (null == selectedSource)
        {
            obj.AddComponent<AudioSource>();
            audioSrcs = obj.GetComponents<AudioSource>();
            i = audioSrcs.Length - 1;
            selectedSource = audioSrcs[i];
        }

        return selectedSource;
    }

    private void MixAudio(AudioSource[] sources)
    {
        float volume0 = sources[0].volume;
        float volume1 = sources[1].volume;
        float time = Mathf.Min(volume0, volume1);
        sources[0].Play();

        if (sources[1].isPlaying)
            StartCoroutine(FadeSources(sources, time));
    }

    private IEnumerator FadeSource(AudioSource source, float time = 2f)
    {
        if (source.isPlaying)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                if (null != source)
                {
                    if (source.volume > 0f)
                        source.volume -= t / time;

                    yield return null;
                }
                else
                    break;
            }

            source?.Stop();
        }

        yield return null;
    }

    private IEnumerator FadeSources(AudioSource[] sources, float time = 2f)
    {
        int id = sources[0].GetInstanceID();
        Sound sound = soundDict[id];
        float v0 = sound.volume * mainVolume;
        sources[0].volume = 0f;

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            if ( sources[0].volume < v0)
                sources[0].volume += t / time;

            if (sources[1].volume > 0f)
                sources[1].volume -= t / time;

            yield return null;
        }

        sources[1].Stop();

        if (sources[1].loop)
        {
            time = sources[0].clip.length;
            yield return new WaitForSeconds(time);
            sources[1].Play();
            sources[0].Stop();
        }
    }

    private AudioSource[] GetSources()
    {
        AudioSource[] arr = new AudioSource[2];
        arr[0] = sources[0];
        arr[1] = sources[1];

        if (sources[0].isPlaying || sources[0].loop )//&& !sources[1].isPlaying)
        {
            arr[0] = sources[1];
            arr[1] = sources[0];
        }
        
        return arr;
    }
}
