using UnityEngine;

public class PCCursor : MonoBehaviour
{
    private static float rotOffset = -135f;

    public RectTransform mainRectTransform;
    public RectTransform ownRectTransform;
    public Transform clickImg;
    
    private bool isVisible = true;
    private bool isMoveable;
    private bool isInfected;
    private Vector3 tmpPosition;

    private void Awake()
    {
        SetVisible(false);
        clickImg.gameObject.SetActive(false);
    }

    public void SetInfected(bool isInfected)
    {
        this.isInfected = isInfected;
    }

    public void SetMoveable(bool isMoveable)
    {
        this.isMoveable = isMoveable;
    }

    public void SetMoveable(bool isMoveable, bool jump, float duration = 0f)
    {
        this.isMoveable = isMoveable;

        if (jump)
        {
            Vector2 position = new Vector2(
                mainRectTransform.rect.width * Random.value,
                mainRectTransform.rect.height * Random.value
            );
            tmpPosition += new Vector3(position.x, position.y, 0f);
            tmpPosition *= 0.5f;
            duration = duration < 2f ? 0.5f : Random.Range(1f, 3f);
        }
        else if (duration > 0f)
            duration = 0.5f;

        if (duration > 0f)
            GameEvent.GetInstance().Execute<bool>(SetMoveable, true, duration);
    }

    public void SetVisible(bool isVisible, bool isMoveable = true)
    {
        SetMoveable(isMoveable);

        if (this.isVisible == isVisible)
            return;

        this.isVisible = isVisible;
        gameObject.SetActive(isVisible);
    }

    private void SetPosition(Vector2 position)
    {
        ownRectTransform.position = new Vector3(position.x, position.y, 0f);
        Vector2 relPoint = position - mainRectTransform.rect.center;
        float zRot = (Mathf.Atan2(relPoint.y, relPoint.x) * 180f / Mathf.PI) + rotOffset;
        ownRectTransform.localRotation = Quaternion.Euler(0f, 0f, zRot);
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (!isMoveable)
        {
            Vector3 p = Vector3.Lerp(transform.position, tmpPosition, Time.deltaTime);
            SetPosition(p);
            return;
        }

        Vector2 position = Input.mousePosition;

        if (isInfected)
        {
            position = Vector3.Lerp(transform.position, position, 0.25f);
        }

        SetPosition(position);

        if (Input.GetMouseButtonDown(0))
            OnClick();
    }

    public void OnClick(bool sound = true)
    {
        if (clickImg.gameObject.activeSelf)
            return;

        if (sound)
            AudioManager.GetInstance().PlaySound("mouseclick", gameObject);

        clickImg.gameObject.SetActive(true);        
        GameEvent.GetInstance().Execute<bool>(clickImg.gameObject.SetActive, false, 0.25f);
    }
}
