using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This script shows a place for a dropable collectable object.
/// All object places are arranged in a room at a separate transform.
/// </summary>
public class ObjectPlace : MonoBehaviour
{
    public DropOrientation dropOrientation;
    public Transform parentPosition;
    public bool IsVisible { get => render.enabled; }
    public bool IsAvailable { get => null == collectable; }

    private Collider col;
    private Renderer render;
    private Collectable collectable;

    private void Awake()
    {
        render = GetComponentInChildren<Renderer>();
        col = GetComponent<Collider>();
        Hide();
    }

    public void SetCollectable(Collectable collectable)
    {
        this.collectable = collectable;

        if (null == collectable)
            return;

        collectable.transform.SetParent(transform, true);
    }

    public Vector3 GetWalkPosition(Collectable collectable)
    {
        Vector3 relPosition = (collectable.GetInteractionPosition() - collectable.transform.position);
        return CalculateWalkPosition() + relPosition;
    }

    public Bounds GetBounds()
    {
        return render.bounds;
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void SetVisible(bool isVisible)
    {
        col.enabled = isVisible;
        render.enabled = isVisible;
    }

    private void OnMouseOver()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && transform == hit.transform)
        {
            Vector3 position = hit.point;
            UIDropPoint uiDropPoint = UIDropPoint.GetInstance();
            uiDropPoint.SetObjectPlace(this);
            /*
            uiDropPoint.SetPosition(
                position,
                CalculateWalkPosition(),
                GetRotation()
            );
            */
            uiDropPoint.IsLegal = true;
            UIGame.GetInstance().SetCursorEnabled(true, true);
        }
    }

    public Vector3 GetRotation()
    {
        if (dropOrientation == DropOrientation.Horizontal)
            return Vector3.zero;

        return transform.localRotation.eulerAngles;
    }

    public Vector3 CalculateWalkPosition()
    {
        if (dropOrientation == DropOrientation.Vertical)
        {
            return transform.position + (transform.forward.normalized * 1.5f);
        }

        Vector3 pos1 = transform.position + (transform.forward + transform.right).normalized;
        Vector3 pos2 = transform.position + (transform.forward - transform.right).normalized;

        NavMeshHit myNavHit;

        if (NavMesh.SamplePosition(pos1, out myNavHit, 2, NavMesh.AllAreas))
        {
            return myNavHit.position;
        }

        if (NavMesh.SamplePosition(pos2, out myNavHit, 2, NavMesh.AllAreas))
        {
            return myNavHit.position;
        }

        return transform.position + transform.forward;
    }

    private void OnMouseExit()
    {
        UIDropPoint uiDropPoint = UIDropPoint.GetInstance();
        uiDropPoint.IsLegal = false;
        UIGame.GetInstance().SetCursorEnabled(false, true);
    }

    private void OnMouseUpAsButton()
    {
        if (UIGame.GetInstance().IsCursorOverUI || !Input.GetMouseButtonUp(0))
            return;

        UIDropPoint.GetInstance().InvokeDrop();
    }
}