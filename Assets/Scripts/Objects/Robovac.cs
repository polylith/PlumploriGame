using System.Collections.Generic;
using Movement;
using UnityEngine;
using UnityEngine.AI;

public class Robovac : Interactable
{
    public GameObject stationObject;
    public AudioSource audioSource;
    public TestTarget targetObject;
    public Vector3 moveDiff;
    public float distance;
    public float minDistance = 2.55f;
    private NavMeshAgent agent;
    public bool IsMoving { get => CheckDistance(); }
    private Vector3 lastPosition;

    public override List<string> GetAttributes()
    {
        List<string> attributes = new List<string>();
        return attributes;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override void RegisterGoals()
    {
        // TODO
    }

    protected override void RegisterAtoms()
    {
        // TODO
    }

    public override int IsInteractionEnabled()
    {
        return base.IsInteractionEnabled();
    }

    public override bool Interact(Interactable interactable = null)
    {
        return base.Interact(interactable);
    }

    private bool CheckDistance()
    {
        moveDiff = transform.position - lastPosition;
        distance = Vector3.Distance(transform.position, targetObject.transform.position);
        return !agent.isStopped && !moveDiff.Equals(Vector3.zero) && distance > minDistance;
    }

    private void SetTarget()
    {
        Vector3 target = NavMeshMover.GetRandomPointOnNavMesh();
        MovePathInfo info = NavMeshMover.CalculatePath(transform.position + Vector3.up, target);
        targetObject.target = info.LastPoint;
        agent.SetDestination(targetObject.target);

        if (!audioSource.isPlaying)
        {
            AudioManager.GetInstance().PlaySound("vacuumcleaner.cleaning", gameObject, 1f, audioSource);
        }
    }

    private void Start()
    {
        if (null == agent)
        {
            agent = GetComponent<NavMeshAgent>();
            minDistance = agent.stoppingDistance * 1.01f;
            SetTarget();
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        bool isMoving = IsMoving;
        Debug.Log(transform.name + " is moving " + isMoving);

        if (!isMoving)
        {
            SetTarget();
        }

        lastPosition = transform.position;
    }
}
