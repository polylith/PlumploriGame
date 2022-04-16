using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Movement;
using Creation;
using Action;

/// <summary>
/// <para>
/// This abstract class encapsulates all attributes, method
/// and functions of a game character.
/// </para>
/// <para>
/// This class still needs to be cleaned up. Much is still
/// from the original version, which is now no longer needed.
/// </para>
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public abstract class Character : MonoBehaviour
{
    protected static string[] walkToken = new string[] { "walk", "stairwalk" };
    
    public Canvas canvas;
    public Rigidbody rb;
    public CapsuleCollider col;
    public Animator animator;
    public Transform headBone;
    public Transform handL;
    public Transform handR;
    public Transform fotoCameraPosition;
    public AudioSource[] audioSource;
    public AudioListener audioListener;

    public bool IsNPC { get => isNPC; }
    public bool IsJumping{ get => isJumping; }
    public bool IsMoving { get => null != moveSequence && moveSequence.IsPlaying(); }
    public bool IsRotating { get => null != rotationSequence && rotationSequence.IsPlaying(); }
    public bool IsWalking { get => isWalking; }

    protected float moveSpeed = 1.25f;
    protected float rotateSpeed = 180f;
    protected float jumpSpeed = 8f;

    protected float fallMultiplier = 2.5f;
    protected float lowJumpMultiplier = 2f;

    protected bool stairWalk = false;

    private bool isWalking = false;
    private bool isJumping = false;
    private float speed = 1f;

    protected Vector3 currentPosition;
    public Vector3 CurrentTarget { get; protected set; }

    private InventoryContent inventoryContent;

    private bool isNPC;
    private string currentTriggerName = "";
    private string stoken = "indoor";

    private Sequence moveSequence;
    private Sequence rotationSequence;
    private List<Vector3> targets;
    private Queue<CharacterSequenceData> dataQueue;
    private IEnumerator ieResume;
    private bool inited = false;
    public string characterName;
    private bool isHighlighted;

    public Key GetKeyFor(Lockable lockable)
    {
        return inventoryContent.GetKeyFor(lockable);
    }

    public void Highlight(bool mode)
    {
        if (isHighlighted == mode)
            return;

        isHighlighted = mode;

        if (mode)
        {
            UIGame.GetInstance().SetCursorEnabled(true, true);
            canvas.gameObject.SetActive(true);
            UIToolTip.GetInstance().SetText(GetName(), 1);
            LookAt(GameManager.GetInstance().CurrentPlayer.transform.position);
        }
        else
        {
            canvas.gameObject.SetActive(false);
            UIGame.GetInstance().SetCursorEnabled(false, true);
            UIToolTip.GetInstance().ClearText();
        }
    }

    public string GetName()
    {
        return characterName;
    }
     
    public void SetName(string name)
    {
        characterName = name;
    }

    // for NPC
    public bool HasTargets()
    {
        return null != targets && targets.Count > 0;
    }

    // for NPC
    public void SetNextTarget()
    {
        if (!IsNPC || HasTargets())
            return;

        Debug.Log(transform.name + " Set Next Target");

        if (dataQueue.Count == 0)
        {
            // TODO get position in current room to go to
            // GameManager.GetInstance().CurrentWorld.GetNextTarget(this);
        }
        else
        {
            CharacterSequenceData data = dataQueue.Dequeue();
            SetTargets(data.data, data.duration, data.action);
        }
    }

    // for NPC
    public void Move()
    {
        ContinueSequence();
    }

    // for NPC
    protected void ContinueSequence()
    {
        if (!IsNPC || isJumping)
            return;

        if (isHighlighted)
        {
            GameEvent.GetInstance().Execute(ContinueSequence, 5f);
            return;
        }

        Debug.Log(transform.name + " continue seq " + (null == moveSequence ? "NULL" : moveSequence.stringId));
        
        if (null != moveSequence)
            PlaySeq();
        else
            SetNextTarget();
    }

    private void PlaySeq(bool play = true)
    {
        if (null != moveSequence)
        {
            if (play)
            {
                moveSequence.Play();
            }
            else
            {
                moveSequence.Pause();
            }
        }        
    }

    private void FinishSequence(System.Action action = null)
    {
        ClearMoveSequence();
        action?.Invoke();        
    }

    // for Player
    private void ClearMoveSequence()
    {
        PlaySeq(false);

        if (null != moveSequence)
        {
            moveSequence.Kill(false);
        }
        
        moveSequence = null;
        targets?.Clear();
    }

    // for Player
    private void ClearRotationSequence()
    {
        if (null != rotationSequence)
        {
            rotationSequence.Pause();
            rotationSequence.Kill(false);
        }

        rotationSequence = null;
    }

    // for NPC
    public void CancelSequences()
    {
        dataQueue?.Clear();
        targets?.Clear();

        if (null == moveSequence && null == rotationSequence)
            return;

        PlaySeq(false);
        moveSequence?.Kill(false);
        rotationSequence?.Kill(false);
        moveSequence = null;
        rotationSequence = null;
    }

    // for NPC
    public void StopMove(float waitTime = 0f)
    {
        Debug.Log(transform.name + " stop move " + waitTime);

        PlaySeq(false);

        if (waitTime <= 0f)
            return;

        if (null != ieResume)
            StopCoroutine(ieResume);

        ieResume = IEResume(waitTime);
        StartCoroutine(ieResume);
    }

    // for NPC
    private IEnumerator IEResume(float waitTime)
    {
        yield return null;

        if (waitTime > 0f)
            yield return new WaitForSecondsRealtime(waitTime);

        ContinueSequence();
    }

    // for NPC            
    public Vector3 GetLastTarget()
    {
        Vector3 p = currentPosition;

        if (null != targets && targets.Count > 0)
            p = targets[targets.Count - 1];

        return p;
    }

    // for NPC
    public void SetTargets(List<Vector3> targets, float duration, System.Action action)
    {
        duration /= moveSpeed;

        if (IsMoving)
        {
            dataQueue.Enqueue(new CharacterSequenceData() {
                data = targets,
                duration = duration,
                action = action
            });
            return;
        }

        float speed = Random.Range(0.9f, 1.1f);
        this.targets.AddRange(targets);
        moveSequence = SequenceManager.GetInstance().GetSequence();
        moveSequence.Append(
            transform.DOPath(targets.ToArray(),
            duration,
            PathType.Linear,
            PathMode.Ignore,
            10,
            Color.red
        )
        .SetEase(Ease.Linear)
        .OnWaypointChange(WayPointCallback));
        moveSequence.OnComplete(() => {
                FinishSequence(action);
            });

        if (isHighlighted || isJumping)
            return;

        PlaySeq();
    }

    // for Player
    public float Goto(Vector3 target, Vector3 lookat, System.Action action = null,
        bool bCheckNavMesh = true)
    {
        if (IsMoving)
        {
            ClearMoveSequence();
        }

        float duration;
        Vector3[] path = new Vector3[0];
        RaycastHit hit = Calc.GetPointOnGround(target);
        target = hit.point;

        if (bCheckNavMesh)
        {
            float distance = Vector3.Distance(transform.position, target);

            Debug.Log("Distance " + transform.position + " -> " + target + " = " + distance);

            if (distance > 0.5f)
            {
                Vector3 direction = (target - transform.position).normalized;
                Vector3 start = transform.position + direction;
                MovePathInfo info = NavMeshMover.CalculatePath(start, target);
                path = info.points;
            }

            if (path.Length == 0)
            {
                if (null != action)
                {
                    if (!target.Equals(lookat))
                    {
                        LookAt(lookat, action);
                    }
                    else
                    {
                        Interact(action);
                    }
                }
                else
                {
                    SetWalking(false);
                }
                return 0f;
            }
        }
        else
        {
            path = new Vector3[] { transform.position, target };
        }

        CurrentTarget = path.Length > 0 ? path[path.Length - 1] : target;

        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetOverUI(true);
        uiGame.SetCursorVisible(false);

        UIDropPoint uiDropPoint = UIDropPoint.GetInstance();
        uiDropPoint.ShowPointer(target, hit.normal, 2);
        uiDropPoint.IsFreezed = true;

        duration = Calc.CalcPathDuration(path, moveSpeed);
        targets = new List<Vector3>(path);
        moveSequence = DOTween.Sequence().
            SetAutoKill(false).
            OnPlay(() => {
                SetWalking(true);
            }).
            Append(
                transform.DOPath(
                    targets.ToArray(),
                    duration,
                    PathType.Linear,
                    PathMode.Ignore,
                    10,
                    Color.red
                ).
                SetEase(Ease.Linear).
                OnWaypointChange(WayPointCallback)
            ).
            OnComplete(() => {
                FinishSequence();
                SetWalking(false);
                if (!target.Equals(lookat))
                {
                    LookAt(lookat, action);
                }
                else if (null != action)
                {
                    Interact(action);
                }

                uiDropPoint.ResetPointer();
                uiDropPoint.HidePointer(0.75f);
            
                uiGame.HideCursor(0.5f);
                uiGame.SetOverUI(false);
                GameManager.GetInstance().UnHighlight();
            }
        );
        PlaySeq();
        return duration;
    }

    private void WayPointCallback(int currentWayPointIndex)
    {
        // when NPC is highlighted, don't move or rotate
        if (isHighlighted)
        {
            StopMove(5f);
            return;
        }

        int nextWayPointIndex = currentWayPointIndex + 1;
        currentPosition = transform.position;

        if (nextWayPointIndex >= targets.Count)
            return;

        if (!isWalking && IsMoving)
            SetWalking(true);

        try
        {
            Vector3 p = targets[nextWayPointIndex];
            LookAt(p);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void InstantLookAt(Vector3 p)
    {
        Vector3 relativePosition = (p - transform.position);
        float yRot = Quaternion.LookRotation(relativePosition, Vector3.up).eulerAngles.y;
        transform.localRotation = Quaternion.Euler(new Vector3(0f, yRot, 0f));
    }

    public void LookAt(Vector3 p, System.Action callBack = null)
    {
        Vector3 relativePosition = (p - transform.position);

        if (Mathf.Approximately(relativePosition.x, 0f) && Mathf.Approximately(relativePosition.z, 0f))
        {
            relativePosition = new Vector3(0.01f, relativePosition.y, relativePosition.z);
        }

        float currentYRotation = transform.localRotation.eulerAngles.y;
        float yRot = Quaternion.LookRotation(relativePosition, Vector3.up).eulerAngles.y;
        Vector3 rot = new Vector3(0f, yRot, 0f);
        float angleDiff = currentYRotation != 0f
            ? Mathf.Abs(Quaternion.Angle(Quaternion.Euler(rot), transform.localRotation))
            : yRot;
        bool isNoRotation = Mathf.Approximately(angleDiff, 0f);
        float duration = Mathf.Min(0.9f, Mathf.Max(angleDiff / rotateSpeed, 0.5f));
        
        if (isNoRotation)
        {
            if (null != callBack)
            {
                Interact(callBack);
            }
            
            return;
        }

        if (IsRotating)
        {
            ClearRotationSequence();
        }

        rotationSequence = DOTween.Sequence().
            SetAutoKill(false)
            .OnPlay(() => SetWalking(true))
            .Append(
                transform.DOLocalRotate(
                    rot,
                    duration,
                    RotateMode.Fast
                )
                .SetEase(Ease.Linear)
            )
            .OnComplete(() => {
                if (!IsMoving && IsWalking)
                {
                    SetWalking(false);
                }
                if (null != callBack)
                {
                    Interact(callBack);
                }

                ClearRotationSequence();
            })
            .Play();
    }

    public void RotateY(float deltaY, System.Action callBack = null)
    {
        if (IsMoving)
            return;

        if (IsRotating)
        {
            ClearRotationSequence();
        }

        float duration = Mathf.Min(0.9f, Mathf.Max(deltaY / rotateSpeed, 0.5f));
        Vector3 rot = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y + deltaY,
            transform.localEulerAngles.z
        );
        rotationSequence = DOTween.Sequence().
            SetAutoKill(false)
            .OnPlay(() => SetWalking(true))
            .Append(
                transform.DOLocalRotate(
                    rot,
                    duration,
                    RotateMode.Fast
                )
                .SetEase(Ease.Linear)
            )
            .OnComplete(() => {
                if (!IsMoving && IsWalking)
                {
                    SetWalking(false);
                }
                if (null != callBack)
                {
                    callBack.Invoke();
                }

                ClearRotationSequence();
            })
            .Play();
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        currentPosition = position;
    }

    public void SetRotation(Quaternion rot)
    {
        transform.localRotation = rot;
    }

    public void SetAnimationTrigger(string triggerName)
    {
        if (null == animator || null == triggerName || ("").Equals(triggerName)
            || triggerName.Equals(currentTriggerName))
            return;

        Debug.Log("Set Animation Trigger " + triggerName);

        currentTriggerName = triggerName;
        animator.SetTrigger(triggerName);
        SetAnimationSpeed(1f);
    }

    protected void SetAnimationSpeed(float speed)
    {
        this.speed = speed;

        if (null == animator)
            return;

        animator.SetFloat("walkdirection", this.speed);
    }

    public bool SetGroundToken(string stoken)
    {
        if (null == stoken || this.stoken.Equals(stoken) || "untagged".Equals(stoken))
            return false;

        this.stoken = stoken;
        
        if (audioSource[0].isPlaying)
            audioSource[0].Stop();

        SetWalking(isWalking);
        return true;
    }

    // for Player
    public void Interact(System.Action callBack)
    {
        ActionController actionController = ActionController.GetInstance();
        GameAction currentAction = actionController.Current;

        if (currentAction is PointerAction)
        {
            callBack?.Invoke();

            if (IsWalking)
                SetWalking(false);

            return;
        }

        string actionName = currentAction.langKey.ToString().ToLower();
        StartCoroutine(IEInteract(callBack, actionName));
    }

    // for Player
    private IEnumerator IEInteract(System.Action callBack, string actionName)
    { 
        /*if (!IsWalking)
        {
            SetWalking(true);

            yield return new WaitForSecondsRealtime(0.25f);
        }*/

        SetAnimationTrigger(actionName);

        yield return new WaitForSecondsRealtime(0.5f);

        callBack?.Invoke();

        yield return new WaitForSecondsRealtime(0.35f);

        SetWalking(false);
    }

    protected void SetWalking(bool isWalking)
    {
        AudioManager audioManager = AudioManager.GetInstance();
        this.isWalking = isWalking;

        if (this.isWalking || stairWalk)
        {
            SetAnimationTrigger(walkToken[stairWalk ? 1 : 0]);

            if (!audioSource[0].isPlaying)
                audioManager?.PlaySound("footsteps." + stoken.ToLower(), gameObject, speed, audioSource[0]);
        }
        else 
        {
            SetAnimationTrigger("idle");
            SetAnimationSpeed(Random.Range(0.5f, 1f));

            if (audioSource[0].isPlaying)
                audioSource[0].Pause();
        }
    }

    public List<Collectable> GetInventoryItems()
    {
        if (null == inventoryContent)
            return null;

        return inventoryContent.GetItems();
    }

    public bool HasInventoryCapacity()
    {
        return null == inventoryContent || inventoryContent.HasCapacity();
    }

    public int GetInventoryContentCount()
    {
        if (null == inventoryContent)
            return 0;

        return inventoryContent.Count;
    }

    public bool AddInventoryItem(Collectable collectable)
    {
        if (null == inventoryContent)
            inventoryContent = new InventoryContent();

        return inventoryContent.Add(collectable);
    }

    public bool RemoveInventoryItem(Collectable collectable)
    {
        if (null == inventoryContent)
            inventoryContent = new InventoryContent();

        return inventoryContent.Remove(collectable);
    }

    public void SetStairWalk(bool mode)
    {
        if (stairWalk == mode)
            return;

        stairWalk = mode;
        SetAnimationTrigger(mode ? "stairwalk" : "idle");
    }

    protected abstract void CollisionOnEnter(Collision collision);
}