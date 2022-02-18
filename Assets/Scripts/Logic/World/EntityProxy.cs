using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class might be removed. There is another class
/// for this purpose: the InteractableProxy.
/// </summary>
public class EntityProxy : Entity
{
    public Entity entity;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (null != rb)
            rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (null == rb)
            return;

        Vector3 rot = transform.rotation.eulerAngles;
        RigidbodyConstraints constraints = rb.constraints;
        bool freezeX = ((constraints & RigidbodyConstraints.FreezeRotationX) == RigidbodyConstraints.FreezeRotationX);
        bool freezeY = ((constraints & RigidbodyConstraints.FreezeRotationY) == RigidbodyConstraints.FreezeRotationY);
        bool freezeZ = ((constraints & RigidbodyConstraints.FreezeRotationZ) == RigidbodyConstraints.FreezeRotationZ);

        if (freezeX)
            rot.x = 0f;

        if (freezeY)
            rot.y = 0f;

        if (freezeZ)
            rot.z = 0f;

        transform.rotation = Quaternion.Euler(rot);
    }

    private void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        if (null != rb)
            rb.useGravity = true;

        if (null != entity)
            entity.Initialize();
    }

    public override string GetDescription()
    {
        return null != entity ? entity.GetDescription() : "";
    }

    public override List<string> GetAttributes()
    {
        if (null == entity)
            return new List<string>();

        return entity.GetAttributes();
    }

    protected override void RegisterAtoms()
    {
        if (null == entity)
            return;

        entity.Register();
    }

    public override void RegisterGoals()
    {
        if (null == entity)
            return;

        entity.RegisterGoals();
    }
}