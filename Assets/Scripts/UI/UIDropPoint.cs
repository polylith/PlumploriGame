using UnityEngine;
using UnityEngine.UI;

public class UIDropPoint : MonoBehaviour
{
    private static Color[] colors = new Color[] { Color.red, Color.yellow, Color.green };

    public static UIDropPoint GetInstance()
    {
        return ins;
    }

    private static UIDropPoint ins;

    public bool IsLegal { get => isLegal; set => SetLegal(value); }
    public bool IsFreezed { get => isFreezed; set => SetFreezed(value); }

    public Canvas canvas;
    public Image img;
    public Image img2;

    private Collectable collectable;
    private Vector3 lastPosition;
    private Vector3 walkPosition;
    private Vector3 lastRotation;
    private bool isLegal = true;
    private bool isFreezed = false;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            Show(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetFreezed(bool freezed)
    {
        if (isFreezed == freezed)
            return;

        isFreezed = freezed;
    }

    private void SetLegal(bool isLegal)
    {
        if (this.isLegal == isLegal)
            return;

        this.isLegal = isLegal;

        if (null != collectable)
            AudioManager.GetInstance().PlaySound("wush", isLegal ? 1f : 0.75f);

        img2.color = isLegal ? colors[2] : colors[1];
        img2.gameObject.SetActive(isLegal);
    }

    public Vector3 GetPosition()
    {
        return lastPosition;
    }

    public Vector3 GetWalkPosition(Collectable collectable)
    {
        Vector3 relPosition = (collectable.GetInteractionPosition() - collectable.transform.position);

        return walkPosition + relPosition;
    }

    public Vector3 GetRotation()
    {
        return lastRotation;
    }

    public void SetPosition(Vector3 pos1, Vector3 pos2, Vector3 rotation)
    {
        lastRotation = rotation;
        lastPosition = pos1;
        walkPosition = pos2;        
        img2.transform.position = canvas.worldCamera.WorldToScreenPoint(pos1);
    }

    public void ResetPosition(Vector3 pos2)
    {
        collectable.transform.position = pos2;
    }
        
    public void Show(bool visible, Collectable collectable = null)
    {
        if (visible)
            canvas.worldCamera = Camera.main;

        this.collectable = collectable;
        img2.gameObject.SetActive(visible);
        img2.transform.position = Vector3.zero;
        SetLegal(false);
    }

    public void InvokeDrop()
    {
        if (null != collectable)
            collectable.Interact(null);
    }

    public void ResetPointer()
    {
        transform.position = Vector3.down * 100f;
        IsFreezed = false;
    }

    public void ShowPointer(Vector3 pos, Vector3 normal, int iState)
    {
        img.color = colors[iState];

        if (IsFreezed)
            return;

        transform.SetPositionAndRotation(
            pos,
            Quaternion.FromToRotation(Vector3.up, normal)
        );
        img.gameObject.SetActive(true);
    }

    public void HidePointer(float duration)
    {
        if (IsFreezed)
            return;

        img.gameObject.SetActive(false);

        GameEvent.GetInstance().Execute(
            () =>
            {
                img.gameObject.SetActive(true);
            },
            duration
        );
    }

    public void HidePointer()
    {
        if (!IsFreezed)
            img.gameObject.SetActive(false);
    }
}