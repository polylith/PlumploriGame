using System.Collections.Generic;
using Movement;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Television;

public class Robovac : Interactable
{
    public enum State
    {
        Off = -1,
        On,
        Charging,
        Resting,
        Starting,
        Working,
        Returning
    };

    public delegate void OnChargeUpdateEvent(float value);
    public event OnChargeUpdateEvent OnChargeUpdate;

    public State CurrentState { get; private set; } = State.Off;
    public float RestTime { get; set; } = 30f;
    public bool IsMoving { get => CheckDistance(); }
    public float ChargeState { get => chargeState; }

    public GameObject stationObject;
    public AudioSource audioSource;
    public TestTarget targetObject;
    public AutoCameraRegisterer autoCameraRegisterer;

    private Vector3 moveDiff;
    private float distance;
    private float minDistance = 2.55f;

    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private int currentTargetIndex = -1;
    private readonly List<Vector3> targets = new List<Vector3>();
    private float chargeState = 0.98f;
    private Sequence seq;
    
    public override List<string> GetAttributes()
    {
        List<string> attributes = new List<string>();
        return attributes;
    }

    public override string GetDescription()
    {
        return Language.LanguageManager.GetText(
            CurrentState.ToString(),
            GetText()
        );
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
        if (CurrentState < State.Working)
            return transform.position - transform.forward;

        return GameManager.GetInstance().CurrentPlayer.transform.position;
    }

    public override int IsInteractionEnabled()
    {
        return base.IsInteractionEnabled();
    }

    public override bool Interact(Interactable interactable = null)
    {
        return base.Interact(interactable);
    }

    public void SetTargets(List<Vector3> targets)
    {
        this.targets.Clear();
        this.targets.AddRange(targets);
        currentTargetIndex = this.targets.Count;
        CurrentState = State.Off;
        SwitchState();
    }

    private void SetRandomTargets()
    {
        int n = 5;

        while (n > 0)
        {
            Vector3 target = NavMeshMover.GetRandomPointOnNavMesh();
            targets.Add(target);
            n--;
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
                SwitchOn();
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
        CurrentState = State.Charging;
        Signal(2f);

        if (null != autoCameraRegisterer)
            autoCameraRegisterer.enabled = true;

        GameEvent.GetInstance().Execute(SwitchState, 2f);
    }

    public void SwitchOff()
    {
        CurrentState = State.Off;

        if (null != autoCameraRegisterer)
            autoCameraRegisterer.enabled = false;

        agent.enabled = false;
        Signal(1.75f);
    }

    private void Signal(float pitch = 2.5f)
    {
        AudioManager.GetInstance().PlaySound("blip", gameObject, pitch, audioSource);
    }

    private void CheckCharge()
    {
        UpdateCharge();

        if (chargeState < 1f)
        {
            GameEvent.GetInstance().Execute(SwitchState, 1f);
        }

        Signal(2.5f);

        if (targets.Count == 0)
        {
            GameEvent.GetInstance().Execute(SwitchOff, 1f);
        }
        
        CurrentState = State.Resting;
        GameEvent.GetInstance().Execute(StartWorking, RestTime);
    }

    private void EnterStation()
    {
        agent.enabled = true;

        if (null == seq)
        {
            Vector3 target = stationObject.transform.position;
            seq = DOTween.Sequence().
                SetAutoKill(false).
                Append(transform.DOMove(target, 2f)).
                Join(transform.DOLocalRotate(-Vector3.up * 180, 2f)).
                OnComplete(() => {
                    agent.enabled = false;
                    CurrentState = State.Charging;
                    audioSource.Pause();
                    Signal(2f);
                    SwitchState();
                    ClearSequence();
                }).
                Play();
        }
    }

    private void StartWorking()
    {
        if (null == seq && targets.Count > 0)
        {
            currentTargetIndex = 0;
            CurrentState = State.Starting;
            Vector3 target = stationObject.transform.position + stationObject.transform.forward + Vector3.up;
            target = NavMeshMover.GetWalkAblePoint(target);

            Signal(2.25f);

            seq = DOTween.Sequence().
                SetAutoKill(false).
                SetDelay(0.5f).
                OnPlay(() => {
                    AudioManager.GetInstance().PlaySound("vacuumcleaner.cleaning", gameObject, 1f, audioSource);
                }).
                Append(transform.DOMove(target, 2f)).
                Join(transform.DOLocalRotate(Vector3.up * 180, 2f)).
                OnComplete(() => {
                    CurrentState = State.Working;
                    currentTargetIndex = 0;
                    agent.enabled = true;
                    SwitchState();
                    ClearSequence();
                }).
                Play();
        }
    }

    private void ClearSequence()
    {
        if (null != seq)
        {
            seq.Pause();
            seq.Kill(false);
        }

        seq = null;
    }

    private void UpdateCharge()
    {
        if (CurrentState == State.Charging)
        {
            chargeState += 0.01f;
        }
        else if (CurrentState == State.Working)
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
        currentTargetIndex = -1;
        CurrentState = State.Returning;
        Vector3 target = stationObject.transform.position + stationObject.transform.forward;
        SetDestination(target);
        targetObject.target = target;
    }

    private bool CheckDistance()
    {
        if (!agent.enabled)
            return true;

        moveDiff = transform.position - lastPosition;
        distance = Vector3.Distance(transform.position, targetObject.transform.position);
        return !agent.isStopped && !moveDiff.Equals(Vector3.zero) && distance > minDistance;
    }

    private void NextTarget()
    {
        if (null != InteractableUI && InteractableUI.IsVisible
            || CurrentState != State.Working || IsMoving)
            return;
        
        if (currentTargetIndex < targets.Count)
        {
            Vector3 target = targets[currentTargetIndex];
            MovePathInfo info = NavMeshMover.CalculatePath(transform.position + Vector3.up, target);
            target = info.LastPoint;
            currentTargetIndex++;
            targetObject.target = target;
            SetDestination(targetObject.target);
        }
        else
        {
            GotoStation();
        }
    }

    private void SetDestination(Vector3 destination)
    {
        agent.SetDestination(destination);

        if (!agent.isStopped && !audioSource.isPlaying)
        {
            AudioManager.GetInstance().PlaySound("vacuumcleaner.cleaning", gameObject, 1f, audioSource);
        }
    }

    private void SetPosition(Vector3 position)
    {
        position = NavMeshMover.GetWalkAblePoint(position);
        transform.position = position;
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        minDistance = agent.stoppingDistance * 1.01f;

        if (targets.Count == 0)
        {
            SetRandomTargets();
        }

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
        ShowState();
        if (!agent.enabled)
            return;

        SwitchState();
        lastPosition = transform.position;
        UpdateCharge();
    }

    private void ShowState()
    {
        string s = transform.name + " " + CurrentState.ToString() + " " + Mathf.Round(ChargeState * 100) + "%";

        if (CurrentState == State.Working)
        {
            s += " | " + currentTargetIndex + " / " + targets.Count;
        }

        Debug.Log(s);
    }
}
