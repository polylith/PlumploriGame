using System.Collections;
using System.Collections.Generic;
using Creation;
using UnityEngine;
using Action;

public abstract class Door : Lockable
{
    public Character Opener { get => opener; }

    public Transform positionsParent;
    public Transform transitPosition;

    public GameObject panel;
    public GameObject handleObj;
    public string doortype = "door";
    
    private Quaternion rot;
    private IEnumerator ieAnim;
    private float speed = 0.1f;
    private Character opener;
    private Quaternion handleRot;
    private IEnumerator ieHandle;

    private bool doRelock;

    protected void Init()
    {
        bool isAutoOpening = this.isAutoOpening;
        this.isAutoOpening = !isAutoOpening;
        SetAutoOpening(isAutoOpening);
        langKey = Language.LangKey.Door.ToString();
        bool open = isOpen;

        if (open)
        {
            Close(true);

            if (!isLocked)
                Open(transform, true);
        }
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
                "IsEnabled"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        list.AddRange(base.GetAttributes());

        return list;
    }

    protected override void RegisterAtoms()
    {
        base.RegisterAtoms();
    }

    public override int IsInteractionEnabled()
    {
        if (ActionController.GetInstance().IsCurrentAction(typeof(LookAction)))
            return base.IsInteractionEnabled();

        if (ActionController.GetInstance().IsCurrentAction(typeof(OpenAction))
            || ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
            return null != opener ? (IsLocked ? -1 : 1) : -1;

        return -1;
    }

    public override bool Interact()
    {
        if (ActionController.GetInstance().IsCurrentAction(typeof(LookAction)))
        {
            ShowDescription();
            ActionController.GetInstance().UnsetActionState(this);
            return true;
        }

        if (!ActionController.GetInstance().IsCurrentAction(typeof(OpenAction))
            && !ActionController.GetInstance().IsCurrentAction(typeof(UseAction))
            || null == opener)
            return false;

        col.enabled = false;

        if (isOpen)
        {
            GameEvent.GetInstance().Execute(Close, 0.25f);
        }
        else
        {
            AudioManager.GetInstance().PlaySound(doortype + ".handle", gameObject);
            GameEvent.GetInstance().Execute(Open, 0.5f);
        }

        ActionController.GetInstance().UnsetActionState(this);
        col.enabled = true;

        if (!IsLocked)
            Fire("IsOpen", isOpen);

        return true;
    }

    public override bool Unlock(Key key)
    {
        if (!isLocked || null == key)
            return false;

        if (key.CheckAccess(this))
        {
            isLocked = false;
            AudioManager.GetInstance()?.PlaySound(doortype + ".unlock", gameObject);
            return true;
        }

        return false;
    }

    public override bool Lock(Key key)
    {
        if (isLocked || null == key)
            return false;

        if (key.CheckAccess(this))
        {
            if (isOpen)
            {
                Close();
                GameEvent.GetInstance().Execute(LockSound, 1f);
            }
            else
                LockSound();

            isLocked = true;            
            return true;
        }

        return false;
    }

    private void LockSound()
    {
        AudioManager.GetInstance()?.PlaySound(doortype + ".lock", gameObject);
    }

    public override void SetAutoOpening(bool isAutoOpening)
    {
        if (this.isAutoOpening == isAutoOpening)
            return;

        this.isAutoOpening = isAutoOpening;
        col.isTrigger = isAutoOpening;
    }

    public void SetRelock()
    {
        if (!isLocked)
            return;

        doRelock = isLocked;
        isLocked = false;
    }

    public void ResetDoRelock()
    {
        isLocked = true;
        doRelock = false;
    }

    public override void SetLocked(bool isLocked)
    {
        if (this.isLocked == isLocked)
            return;

        this.isLocked = isLocked;

        if (isLocked && isOpen)
            Close(true);
    }

    protected override void Open()
    {
        if (null == opener)
            return;

        if (opener.IsNPC && isLocked)
        {
            opener.StopMove();
            Key key = opener.GetKeyFor(this);

            if (null != key && Interact(key))
            {
                opener.Move();
                return;
            }

            opener.CancelSequences();
            return;
        }

        Open(opener.transform);
    }

    private IEnumerator IEHandle ()
    {
        float f = 0f;
        Quaternion handleRot = Quaternion.Euler(0f, 0f, 25f);

        while (f <= 1f)
        {
            handleObj.transform.localRotation = Quaternion.Lerp(handleObj.transform.localRotation, handleRot, Time.deltaTime);
            yield return null;
            f += 0.35f;
        }

        f = 0f;
        handleRot = Quaternion.Euler(0f, 0f, 0f);

        while (f <= 1f)
        {
            handleObj.transform.localRotation = Quaternion.Lerp(handleObj.transform.localRotation, handleRot, Time.deltaTime);
            yield return null;
            f += 0.25f;
        }

        handleObj.transform.localRotation = handleRot;
        ieHandle = null;
    }

    public override Vector3 GetInteractionPosition()
    {
        if (null == opener)
            return base.GetInteractionPosition();

        Vector3 dir = opener.transform.position - transform.position;
        Vector3 pos = interactPos.position - positionsParent.transform.position;
        pos = positionsParent.transform.position + new Vector3(
            pos.x * (dir.x < 0f ? 1f : -1f),
            pos.y,
            pos.z
        );
        return pos;
    }

    public Vector3 GetTransitPosition()
    {
        if (null == opener)
            return base.GetInteractionPosition();

        Vector3 dir = opener.transform.position - transform.position;
        Vector3 pos = transitPosition.position - positionsParent.transform.position;
        pos = positionsParent.transform.position + new Vector3(
            pos.x * (dir.x < 0f ? 1f : -1f),
            pos.y,
            pos.z
        );
        return pos;
    }

    public virtual void Open(Transform trans, bool instant = false)
    {
        col.isTrigger = true;

        if (isOpen)
            return;

        GameManager.GetInstance().UnHighlight();

        if (isLocked)
        {
            AudioManager.GetInstance()?.PlaySound(doortype + ".locked", gameObject);
            col.isTrigger = false;
            return;
        }

        if (null != handleObj)
        {
            if (null != ieHandle)
                StopCoroutine(ieHandle);

            ieHandle = IEHandle();
            StartCoroutine(ieHandle);
        }

        isOpen = true;
        Vector3 dir = trans.position - transform.position;
        float angle = transform.localRotation.eulerAngles.x;
        float zRot = -95f;

        if (angle < 90f) // West
        {
            zRot *= (dir.z < 0f ? -1 : 1);
        }
        else if (angle < 180f)
        {
            zRot *= (dir.x < 0f ? -1 : 1);
        }
        else if (angle < 270f)
        {
            zRot *= (dir.z < 0f ? 1 : -1);
        }
        else
        {
            zRot *= (dir.x < 0f ? 1 : -1);
        }

        rot = Quaternion.Euler(0f, 0f, zRot);

        //Debug.Log("Angle " + angle + " dir " + dir + " rot " + yRot);

        if (instant)
            panel.transform.localRotation = rot;
        else
            Animate();
    }

    public void SetOpener(Character character, bool isAutoOpening)
    {
        opener = character;
        SetAutoOpening(isAutoOpening);
    }

    private void ResetTrigger()
    {
        col.isTrigger = isAutoOpening;
        SetOpener(null, false);
    }

    public override void Close()
    {
        Close(false);
        ResetTrigger();
    }
        
    public virtual void Close(bool instant)
    {
        if (!isOpen)
            return;

        isAutoOpening = false;
        isOpen = false;
        rot = Quaternion.Euler(0f, 0f, 0f);

        if (instant)
            panel.transform.localRotation = rot;
        else
            Animate();
    }

    protected virtual void Animate()
    {
        if (null != ieAnim)
            StopCoroutine(ieAnim);

        ieAnim = IEAnimate();
        StartCoroutine(ieAnim);
    }

    protected virtual IEnumerator IEAnimate()
    {
        if (!isOpen)
            yield return new WaitForSeconds(0.5f);

        float f = 0f;
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager?.PlaySound(doortype + ".open", gameObject);

        while (f <= 1f)
        {
            panel.transform.localRotation = Quaternion.Lerp(panel.transform.localRotation, rot, f);
            yield return null;
            f += speed;
        }

        if (!isOpen)
            audioManager?.PlaySound(doortype + ".close", gameObject);

        panel.transform.localRotation = rot;
        ieAnim = null;

        if (null != opener && !opener.IsNPC)
        {
            GameManager.GetInstance().OnBeforeRoomLeave(this);

            Vector3 goal = GetTransitPosition();
            col.enabled = false;
            opener.Goto(
                goal,
                goal,
                OnTransitReached,
                true //false
            );
        }

        if (!isOpen && doRelock)
        {
            ResetDoRelock();
        }
    }

    private void OnTransitReached()
    {
        col.enabled = true;
        GameManager.GetInstance().OnRoomLeft(this);
        opener.LookAt(Camera.main.transform.position);
        ResetTrigger();
        Close();
    }

    private void OnCollisionEnter(Collision other)
    {
        Character character = other.gameObject.GetComponent<Character>();

        if (null == character)
            return;

        SetOpener(character, character.IsNPC);
    }
}