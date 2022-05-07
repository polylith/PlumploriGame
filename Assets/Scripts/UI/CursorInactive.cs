using Action;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This Script just resets highlighted objects and hides pointer
/// when the cursor is over an UI container.
/// </summary>
public class CursorInactive : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
            return;

        ActionController.GetInstance().HandleCursor();
    }
}
