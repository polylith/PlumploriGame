using System.Collections.Generic;
using Movement;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Television;
using Language;

public class Robovac : Interactable
{
    public enum State
    {
        Off = -1,
        On,
        Charging,
        Working,
        Resting,
        Starting,
        Scanning,
        Returning
    };

    public delegate void OnChargeUpdateEvent(float value);
    public event OnChargeUpdateEvent OnChargeUpdate;

    public delegate void OnProgressUpdateEvent(float value);
    public event OnProgressUpdateEvent OnProgressUpdate;

    public delegate void OnAutoScanEvent();
    public event OnAutoScanEvent OnAutoScan;

    public delegate void OnStateChangeEvent();
    public event OnStateChangeEvent OnStateChange;

    public State CurrentState {
        get => currentState;
        private set => SetCurrentState(value);
    }

    public float Progress { get => GetProgress(); }
    public bool HasTargets { get => targets.Count > 0; }
    public float RestTime { get; set; } = 30f;
    public bool IsMoving { get => CheckDistance(); }
    public float ChargeState { get => chargeState; }

    public Vector3 StartPosition { get => GetStartPosition(); }

    public GameObject stationObject;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AutoCameraRegisterer autoCameraRegisterer;
    public GameObject rays;

    private Vector3 moveDiff;
    private float distance;

    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private int currentTargetIndex = -1;
    private Vector3 currentTargetPosition;
    private List<Vector3> targets = new List<Vector3>();
    private float chargeState = 0.98f;
    private State currentState = State.Off;

    private Sequence moveSeq;
    private Sequence raysSeq;


    public override List<string> GetAttributes()
    {
        List<string> attributes = new List<string>();
        return attributes;
    }

    public override string GetDescription()
    {
        string text = LanguageManager.GetText(
            CurrentState.ToString(),
            GetText()
        ) + ".";

        if (CurrentState != State.Scanning && CurrentState != State.Working
            && targets.Count == 0)
        {
            text += "\n" + LanguageManager.GetText(
                Language.LangKey.NotAvailable,
                LanguageManager.GetText(Language.LangKey.Data)
            ) + "!";
        }

        return text;
    }

    public override void RegisterGoals()
    {
        // TODO
    }

    protected override void RegisterAtoms()
    {
        // TODO
    }

    public override Vector3 GetInteractionPosition()
    {
        return StartPosition;
    }

    private Vector3 GetStartPosition()
    {
        Vector3 target = stationObject.transform.position + stationObject.transform.forward + Vector3.up;
        return NavMeshMover.GetWalkAblePoint(target);
    }

    private float GetProgress()
    {
        float value = 0f;

        if (targets.Count > 0)
        {
            float f = Mathf.Max(0f, currentTargetIndex);
            value = f/targets.Count;
        }

        return value;
    }

    private void SetCurrentState(State state)
    {
        if (state == currentState)
            return;

        currentState = state;
        OnStateChange?.Invoke();
    }

    public void SetTargets(List<Vector3> targets)
    {
        this.targets.Clear();
        this.targets.AddRange(targets);
        currentTargetIndex = this.targets.Count;
        CurrentState = State.Off;
        SwitchState();
    }

    public void StartScanning()
    {
        if (CurrentState == State.Off)
            StateOn();

        CurrentState = State.Starting;
        agent.enabled = false;
        targets = NavMeshView.GetInstance().GetPoints(stationObject.transform, StartPosition);
        LeaveStation(State.Scanning, () => {
            OnAutoScan?.Invoke();
        });
        rays.transform.localScale = Vector3.zero;
        rays.SetActive(true);
        raysSeq = DOTween.Sequence().
            SetAutoKill(false).
            SetLoops(-1).
            Append(rays.transform.DOLocalRotate(Vector3.up * 180f, 1f)).
            Join(rays.transform.DOScale(Vector3.one, 1f)).
            Append(rays.transform.DOLocalRotate(Vector3.up * -180f, 1f)).
            Join(rays.transform.DOScale(new Vector3(1f, 0f, 1f), 1f))
            .Play();
    }

    public void StopScanning()
    {
        if (targets.Count > 6)
        {
            for (int i = 0; i < 6; i++)
            {
                targets.RemoveAt(0);
            }
        }

        GotoStation();
        rays.SetActive(false);
        OnAutoScan?.Invoke();
        ClearRaysSequence();
    }

    public void Scan()
    {
        if (IsMoving)
            return;

        OnProgressUpdate?.Invoke(Progress);

        if (currentTargetIndex < targets.Count)
        {
            if (currentTargetIndex > 0 && currentTargetIndex % 6 == 0)
                Signal(4f);

            Vector3 target = targets[currentTargetIndex];
            target = NavMeshMover.GetWalkAblePoint(target);
            currentTargetIndex++;
            SetDestination(target);
        }
        else
        {
            StopScanning();
        }

    }

    private void SwitchState()
    {
        bool isMoving = IsMoving;

        switch (CurrentState)
        {
            case State.Off:
                // nothing to do
                break;
            case State.On:
                StateOn();
                break;
            case State.Charging:
                CheckCharge();
                break;
            case State.Resting:
                // nothing to do
                break;
            case State.Starting:
                // nothing to do
                break;
            case State.Scanning:
                if (!isMoving)
                {
                    Scan();
                }
                break;
            case State.Working:
                if (!isMoving)
                {
                    NextTarget();
                }
                break;
            case State.Returning:
                if (!isMoving)
                {
                    EnterStation();
                }
                break;
        }
    }

    public void SwitchOn()
    {
        CurrentState = State.On;
        SwitchState();
    }

    private void SwitchCamera(bool isOn)
    {
        if (null != autoCameraRegisterer)
            autoCameraRegisterer.enabled = isOn;
    }

    private void StateOn()
    {
        Signal(2f);

        if (CurrentState == State.On)
        {
            CurrentState = State.Charging;
        }

        GameEvent.GetInstance().Execute(SwitchState, 2f);
    }

    public void SwitchOff()
    {
        CurrentState = State.Off;
        currentTargetIndex = -1;
        SwitchCamera(false);
        agent.enabled = false;
        Signal(1.75f);
    }

    private void Signal(float pitch = 2.5f)
    {
        AudioManager.GetInstance().PlaySound("vacuumcleaner.signal", gameObject, pitch, audioSource2);
    }

    private void CheckCharge()
    {
        UpdateCharge();

        if (chargeState < 1f)
        {
            GameEvent.GetInstance().Execute(SwitchState, 1f);
            return;
        }

        Signal(2.5f);

        if (targets.Count == 0)
        {
            GameEvent.GetInstance().Execute(SwitchOff, 1f);
            return;
        }

        CurrentState = State.Resting;
        GameEvent.GetInstance().Execute(StartWorking, RestTime);
    }

    private void StartWorking()
    {
        LeaveStation(State.Working, NextTarget);
    }

    private void EnterStation()
    {
        agent.enabled = true;

        if (null == moveSeq)
        {
            lastPosition = transform.position;
            Vector3 target = stationObject.transform.position;
            float duration = Vector3.Distance(transform.position, target) / agent.speed;
            moveSeq = DOTween.Sequence().
                SetAutoKill(false).
                Append(transform.DOMove(target, duration)).
                Join(transform.DOLocalRotate(-Vector3.up * 180, duration)).
                OnComplete(() => {
                    agent.enabled = false;
                    CurrentState = State.Charging;
                    audioSource1.Pause();
                    Signal(2f);
                    SwitchState();
                    SwitchCamera(false);
                    ClearMoveSequence();
                }).
                Play();
        }
    }

    private void LeaveStation(State nextState, System.Action callBack = null)
    {
        if (null == moveSeq && targets.Count > 0)
        {
            currentTargetIndex = 0;
            CurrentState = State.Starting;
            Vector3 target = StartPosition;
            float duration = Vector3.Distance(transform.position, target) / agent.speed;

            Signal(2.25f);

            moveSeq = DOTween.Sequence().
                SetAutoKill(false).
                SetDelay(0.5f).
                OnPlay(() => {
                    AudioManager.GetInstance().PlaySound("vacuumcleaner." + CurrentState.ToString().ToLower(), gameObject, 1f, audioSource1);
                }).
                Append(transform.DOMove(target, duration)).
                Join(transform.DOLocalRotate(Vector3.up * 180, duration)).
                OnComplete(() => {
                    CurrentState = nextState;
                    lastPosition = transform.position;
                    currentTargetIndex = 0;
                    agent.enabled = true;
                    callBack?.Invoke();
                    SwitchCamera(true);
                    ClearMoveSequence();
                }).
                Play();
        }
    }

    private void ClearMoveSequence()
    {
        if (null != moveSeq)
        {
            moveSeq.Pause();
            moveSeq.Kill(false);
        }

        moveSeq = null;
    }

    private void ClearRaysSequence()
    {
        if (null != raysSeq)
        {
            raysSeq.Pause();
            raysSeq.Kill(false);
        }

        raysSeq = null;
    }

    private void UpdateCharge()
    {
        if (CurrentState == State.Charging)
        {
            chargeState += 0.01f;
        }
        else if (CurrentState > State.Resting)
        {
            chargeState -= 0.0001f;

            if (chargeState < 0.1f)
            {
                CurrentState = State.Returning;
            }
        }

        chargeState = Mathf.Clamp01(chargeState);
        OnChargeUpdate?.Invoke(chargeState);
    }

    public void GotoStation()
    {
        CurrentState = State.Returning;
        SetDestination(StartPosition);
    }

    private bool CheckDistance()
    {
        if (!agent.enabled)
            return true;

        moveDiff = transform.position - lastPosition;
        distance = Vector3.Distance(transform.position, currentTargetPosition);
        return !agent.isStopped && !moveDiff.Equals(Vector3.zero) && distance > agent.stoppingDistance;
    }

    private void NextTarget()
    {
        if (null != InteractableUI && InteractableUI.IsVisible
            || CurrentState != State.Working)
            return;

        OnProgressUpdate?.Invoke(Progress);

        if (currentTargetIndex < targets.Count)
        {
            Vector3 target = targets[currentTargetIndex];
            target = NavMeshMover.GetWalkAblePoint(target);
            currentTargetIndex++;
            SetDestination(target);
        }
        else
        {
            GotoStation();
        }
    }

    private void SetDestination(Vector3 destination)
    {
        currentTargetPosition = destination;
        agent.SetDestination(destination);

        if (!agent.isStopped && !audioSource1.isPlaying)
        {
            AudioManager.GetInstance().PlaySound("vacuumcleaner." + CurrentState.ToString().ToLower(), gameObject, 1f, audioSource1);
        }
    }

    private void SetPosition(Vector3 position)
    {
        lastPosition = transform.position;
        position = NavMeshMover.GetWalkAblePoint(position);
        transform.position = position;
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        InitInteractableUI(true);

        /*
        if (targets.Count == 0)
        {
            targets = NavMeshView.GetInstance().GetPoints(stationObject.transform, StartPosition);
        }
        */
        if (currentTargetIndex > -1 && currentTargetIndex < targets.Count)
        {
            CurrentState = State.Working;
            SetPosition(targets[currentTargetIndex]);
            currentTargetIndex++;
        }
        else if (targets.Count > 0)
        {
            CurrentState = State.On;
        }

        GameEvent.GetInstance().Execute(SwitchState, 5f);
        lastPosition = transform.position;
    }

    private void Update()
    {
        // ShowState();

        if (!agent.enabled)
            return;

        SwitchState();
        UpdateCharge();
        lastPosition = transform.position;
    }

    private void ShowState()
    {
        string s = transform.name + " " + CurrentState.ToString() + " " + Mathf.Round(ChargeState * 100) + "%";

        if (CurrentState == State.Working || CurrentState == State.Scanning)
        {
            s += " | " + currentTargetIndex + " / " + targets.Count;
        }

        Debug.Log(s);
    }
}
