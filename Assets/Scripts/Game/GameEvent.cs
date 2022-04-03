using System.Collections;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    public static GameEvent GetInstance()
    {
        return ins;
    }

    private static GameEvent ins;

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
    
    public IEnumerator Execute(System.Action action, float waitTime = 1f)
    {
        if (null == action)
            return null;

        if (waitTime > 0f)
        {
            try
            {
                IEnumerator ieNum = ExecuteWait(action, waitTime);
                StartCoroutine(ieNum);
                return ieNum;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        else
        {
            action();
            return null;
        }
    }

    private IEnumerator ExecuteWait(System.Action action, float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        action();
    }

    public IEnumerator Execute<T>(System.Action<T> action, T param, float waitTime = 1f)
    {
        if (null == action)
            return null;

        if (waitTime > 0f)
        {
            IEnumerator ieNum = ExecuteWait(action, param, waitTime);
            StartCoroutine(ieNum);
            return ieNum;
        }
        else
        {
            action(param);
            return null;
        }
    }

    private IEnumerator ExecuteWait<T>(System.Action<T> action, T param, float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        action(param);
    }
}