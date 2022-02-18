using UnityEngine;
using DG.Tweening;

/// <summary>
/// This script lets a camera track an arbitrary target
/// at a given distance. The position remains fixed.
/// Just the angle of the camera is changed to follow
/// the target object.
/// As a singleton it can be accessed from everywhere.
/// </summary>
public class CameraFollowTarget : MonoBehaviour
{
    private static CameraFollowTarget ins;

    public static CameraFollowTarget GetInstance()
    {
        return ins;
    }

    public Transform target;
    public Camera cam;
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    public bool IsActive { get; private set; } = false;

    private Sequence moveSequence;

    /// <summary>
    /// Called by the Unity Engine on scricpt loading
    /// </summary>
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

    /// <summary>
    /// Called by the Unity Engine every single frame
    /// </summary>
    private void Update()
    {
        if (!IsActive || null == target)
        {
            return;
        }

        cam.transform.LookAt(target.position + offset);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    /// <summary>
    /// Clear the active move sequence.
    /// </summary>
    private void ClearMoveSequence()
    {
        if (null != moveSequence)
        {
            moveSequence.Pause();
            moveSequence.Kill(false);
        }

        moveSequence = null;
    }

    /// <summary>
    /// Changes the position of the camera with an animation
    /// and performs an optional callback when the animation
    /// is finished.
    /// </summary>
    /// <param name="goal">position to go</param>
    /// <param name="callBack">optional callback on arrival</param>
    public void Goto(Vector3 goal, System.Action callBack = null)
    {
        float duration = Vector3.Distance(goal, transform.position);
        duration = Mathf.Max(0.75f, duration);
        moveSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(transform.DOMove(goal, duration))
            .OnComplete(() => {
                callBack?.Invoke();
                ClearMoveSequence();
            })
            .Play();
    }

    /// <summary>
    /// Changes the position of the camera instantaneously.
    /// </summary>
    /// <param name="location">position to set</param>
    public void SetPosition(Transform location)
    {
        ClearMoveSequence();
        transform.position = location.position;
    }

    /// <summary>
    /// Changes the position of the camera instantaneously
    /// looking at a given point.
    /// </summary>
    /// <param name="position">position to set</param>
    /// <param name="lookAt">position to look at</param>
    public void SetPosition(Vector3 position, Vector3 lookAt)
    {
        ClearMoveSequence();
        transform.position = position;
        cam.transform.LookAt(lookAt + offset);
    }

    /// <summary>
    /// Sets the object to be followed.
    /// </summary>
    /// <param name="target">object to follow</param>
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
