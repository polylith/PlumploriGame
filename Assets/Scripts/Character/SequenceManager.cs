using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SequenceManager : MonoBehaviour
{
    private static SequenceManager ins;

    public static SequenceManager GetInstance()
    {
        return ins;
    }

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sequence GetSequence(bool autoKill = true)
    {
        return DOTween.Sequence()
            .SetAutoKill(autoKill);
    }
}
