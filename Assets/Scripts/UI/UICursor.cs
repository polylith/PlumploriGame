using Action;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The UICursor displays the image according to the currently
/// selected GameAction.
///
/// The image of the cursor is set via the Unity Cursor API.
/// Another image of an object can be displayed in the UI and
/// follows the cursor with a slight delay.
/// </summary>
public class UICursor : MonoBehaviour
{
    private static Vector2 defaultPosition = new Vector2(0.15f, 0.75f);

    public bool IsActive { get => isActive; set => SetActive(value); }

    public Texture2D defaultTexture;

    private Vector2 pivot = defaultPosition;
    private Texture2D tex;
    private bool isActive = false;
    private bool isEnabled = false;
    private bool isOverUI;
    private RectTransform rectTransform;
    private Image objectIcon;

    private void Awake()
    {
        tex = defaultTexture;
        rectTransform = GetComponent<RectTransform>();
        objectIcon = GetComponent<Image>();
        ResetImage();
        HideObjectImage();
        Hide();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            return;

        SetActive(IsActive);
    }

    public bool IsEnabled()
    {
        return isEnabled;
    }
            
    private void SetActive(bool active)
    {
        isActive = active;

        if (active)
            SetEnabled(false, true);

        Cursor.visible = active;
    }

    public void ShowObjectIcon()
    {
        objectIcon.enabled = true;
    }

    public void ShowObjectIcon(Texture2D tex)
    {
        objectIcon.enabled = false;
        objectIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        objectIcon.sprite.name = tex.name;
        ShowObjectIcon();
    }

    public void HideObjectImage()
    {
        objectIcon.enabled = false;
    }

    public void Hide()
    {
        SetActive(false);
    }

    private void ResetImage()
    {
        pivot = defaultPosition;
        Vector2 cursorHotspot = new Vector2(defaultTexture.width, defaultTexture.height) * pivot;
        Cursor.SetCursor(defaultTexture, cursorHotspot, CursorMode.Auto);
    }

    public void RestoreImage()
    {
        SetImage(tex, pivot);
    }

    public void SetImage(Texture2D tex, Vector2 pivot)
    {
        Vector2 cursorHotspot = new Vector2(tex.width, tex.height) * pivot;
        Cursor.SetCursor(tex, cursorHotspot, CursorMode.Auto);

        this.tex = tex;
        this.pivot = pivot;
    }
        
    public void SetDisabled(bool mode)
    {
        isEnabled = false;
        GameAction gameAction = mode
            ? ActionController.GetInstance().Current
            : ActionController.GetInstance().defaultAction;
        Texture2D tex = gameAction.GetDisabled();
        Vector2 pivot = gameAction.GetPivots(-1);
        SetImage(tex, pivot);
    }

    public void SetEnabled(bool enabled, bool mode)
    {
        GameAction gameAction = mode
            ? ActionController.GetInstance().Current
            : ActionController.GetInstance().defaultAction;
        Texture2D tex = gameAction.GetEnabled(enabled);
        Vector2 pivot = gameAction.GetPivots(enabled ? 1 : 0);
        isEnabled = enabled;
        SetImage(tex, pivot);
    }

    /// <summary>
    /// Important method to manually suspend interaction with
    /// the 3D world when only UI elements should be interactive.
    /// </summary>
    /// <param name="mode">true = block 3D world, otherwise false</param>
    public void SetOverUI(bool mode)
    {
        isOverUI = mode;

        if (!mode)
            SetEnabled(false, true);
    }

    public bool IsOverUI()
    {
        return isOverUI;
    }

    /// <summary>
    /// Called by the Unity engine every single frame.
    /// If an object image is to be displayed at the
    /// cursor position, the position of the image is
    /// updated with the mouse position.
    /// </summary>
    private void Update()
    {
        if (objectIcon.enabled)
            rectTransform.position = Input.mousePosition;
    }
}