using Action;
using Language;
using UnityEngine;

/// <summary>
/// The Walkable Ground is used to navigate in the current scene.
/// It allows the interaction Go to to make the current player
/// character go to any position in the current room.
/// The walkable area is bounded by the collider(s).
/// This area is independent of the static navigation mesh,
/// but must be aligned with it.
/// </summary>
public class WalkableGround : MonoBehaviour
{
    /// <summary>
    /// The type of Walkable Ground determines the audio used for footsteps.
    /// When a character touches the collider of the Walkable Ground,
    /// its token applied for the footsteps' audio.
    /// There are these types: 
    /// - indoor
    /// - outdoor(streets, pavement, ...)
    /// - way
    /// - grass
    /// </summary>
    public enum TypeToken {
        INDOOR = 0,
        OUTDOOR,
        WAY,
        GRASS
    }
    public TypeToken groundType = TypeToken.INDOOR;
    private Vector3 position;

    private void OnMouseOver()
    {
        if (UIGame.GetInstance().IsCursorOverUI
            || ActionController.GetInstance().IsDropActionActive())
        {
            return;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && transform == hit.transform)
        {
            UIGame.GetInstance().SetCursorEnabled(true, false);
            ActionController.GetInstance().ShowTempText(
                LanguageManager.GetText(LangKey.GoHere)
            );
            position = hit.point;
            UIDropPoint.GetInstance().ShowPointer(hit.point, hit.normal, 1);
        }
    }

    private void OnMouseExit()
    {
        if (UIGame.GetInstance().IsCursorOverUI
            || ActionController.GetInstance().IsDropActionActive())
        {
            return;
        }

        ActionController.GetInstance().UpdateToolTip();
        UIGame.GetInstance().SetCursorEnabled(false, true);
        UIDropPoint.GetInstance().HidePointer();
    }

    private void OnMouseUpAsButton()
    {
        if (UIGame.GetInstance().IsCursorOverUI
            || ActionController.GetInstance().IsDropActionActive()
            || !Input.GetMouseButtonUp(0))
        {
            return;
        }

        GameManager.GetInstance().GotoPosition(position);
    }
}
