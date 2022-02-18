using UnityEngine;

public class TestTarget : MonoBehaviour
{
    public Vector3 target;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime);
    }
}
