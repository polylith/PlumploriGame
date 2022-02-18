using UnityEngine;

public class GroundObject : MonoBehaviour
{
    private void Start()
    {
        RaycastHit hit = Calc.GetPointOnGround(transform.position);
        transform.position = hit.point;
    }
}
