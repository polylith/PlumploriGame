using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure for mapping sound ids to audio sources.
/// Used by AudioManager only.
/// </summary>
public class AudioMapper
{
    private List<AudioSource> list;
    private string id;

    public AudioMapper(string id)
    {
        this.id = id;
        list = new List<AudioSource>();
    }

    public List<AudioSource> GetList()
    {
        return list;
    }

    public void Check()
    {
        List<AudioSource> list2 = new List<AudioSource>();

        foreach (AudioSource audioSource in list)
        {
            if (null != audioSource && audioSource.isPlaying)
                list2.Add(audioSource);
        }

        if (list2.Count == 0)
        {
            AudioManager.GetInstance()?.RemoveId(id);
        }
        else
        {
            list = list2;
            GameEvent.GetInstance()?.Execute(this.Check, 2f);
        }
    }
}
