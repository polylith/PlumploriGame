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
        ActionController actionController = ActionController.GetInstance();
        
        if (eventData.pointerEnter != gameObject
            ||(actionController.IsCurrentAction(typeof(UseAction))
            || actionController.IsCurrentAction(typeof(GrabAction)))
            && actionController.IsCurrentActionActive())
            return;

        actionController.UnsetActionState(null);
        
        UIDropPoint uiDropPoint = UIDropPoint.GetInstance();
        uiDropPoint.IsFreezed = false;
        uiDropPoint.HidePointer();
        
        GameManager.GetInstance().UnHighlight();

        UIGame.GetInstance().SetCursorEnabled(false, false);
    }
}
