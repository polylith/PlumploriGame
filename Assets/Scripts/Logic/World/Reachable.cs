using UnityEngine;

/// <summary>
/// This class was an intermediate class for objects that
/// should only be able to interact from a certain distance.
/// This class was used in the previous version, which included
/// a first person player mode. Therefore, this class is no
/// longer used in the current version.
/// </summary>
[System.Obsolete("Not used any more", true)]
public abstract class Reachable : Interactable
{
    public float minDistance = 3f;

    private bool isVisible = true;

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (null == col)
            col = GetComponent<Collider>();

        Player currentPlayer = GameManager.GetInstance().CurrentPlayer;


        if (null == currentPlayer || col.isTrigger)
        {
            col.enabled = false;
            return;
        }

        float distance = Vector3.Distance(currentPlayer.transform.position, transform.position);

        if (distance > minDistance)
        {
            //col.enabled = false;
            
            return;
        }

        //col.enabled = ShouldBeEnabled();
    }
}