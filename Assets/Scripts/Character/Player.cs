using UnityEngine;

public class Player : Character
{
    private void Start()
    {
        SetWalking(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        WalkableGround walkableGround = collision.gameObject.GetComponent<WalkableGround>();

        if (null != walkableGround)
        {
            SetGroundToken(walkableGround.groundType.ToString().ToLower());
        }

    }

    protected override void CollisionOnEnter(Collision collision)
    {
        // TODO
    }
}
