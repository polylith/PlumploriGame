using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Action;
using UnityEngine.EventSystems;

/// <summary>
/// The UIGame is a singleton. It handles the display
/// of the UICursor, shows or hides a Shade to hide the
/// screen and displays temporary UI elements.  
/// </summary>
public class UIGame : MonoBehaviour
{
    public static UIGame GetInstance()
    {
        return ins;
    }

    public static readonly float minFoV = 60f / (9f / 16f);

    private static UIGame ins;
    
    public bool IsUIExclusive { get; set; }
    public bool IsObjectVisible { get => showObject.gameObject.activeSelf; }

    public Transform tmpGUIParent;
    public Transform showObject;
    public Image objectIcon;
    public Image shade;
    public GameObject escape;
    public CanvasScaler canvasScaler;
    public UICursor uiCursor;
    public Camera uiCamera;
    public Material uiSkybox;
    public Light uiSun;

    private bool shadeVisible;
    private bool isObjectOnCursorVisible;
    private Material currentSkybox;
    
    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        AdjustSize();

        Inventory inventory = Inventory.GetInstance();
        inventory.UpdatePositions();
        inventory.Show(false, true);

        showObject.gameObject.SetActive(false);
        uiCursor.SetActive(true);
        shadeVisible = true;
        shade.enabled = true;
    }

    private void Start()
    {
        SetCursorVisible(false);
    }

    public void ShowShade()
    {
        if (shadeVisible)
            return;

        FadeShade(0f);
    }

    public void HideShade()
    {
        if (!shadeVisible)
            return;

        shadeVisible = false;
        shade.CrossFadeAlpha(0f, 2f, true);
    }

    public void FadeShade(float duration = 2f)
    {
        if (duration > 0f && shadeVisible)
            return;

        if (!shadeVisible)
        {
            shadeVisible = true;
            shade.CrossFadeAlpha(1f, 2f, true);

            if (duration > 0f)
                duration += 2f;
        }

        if (duration > 0f)
            GameEvent.GetInstance().Execute(HideShade, duration);
    }

    private void AdjustSize()
    {
        canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

        if (null == uiCamera)
        {
            return;
        }

        float f = (float)Screen.height / (float)Screen.width;
        float fov = minFoV * f;
        uiCamera.fieldOfView = fov;
    }

    public void ShowObject(Interactable inter)
    {
        if (null == inter)
            return;

        Texture2D tex = inter.GetObjectIcon();
        objectIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        objectIcon.sprite.name = tex.name;
        showObject.gameObject.SetActive(true);
        AudioManager.GetInstance().PlaySound("plopp.1");
    }

    public void HideObject()
    {
        if (!showObject.gameObject.activeSelf)
            return;

        showObject.gameObject.SetActive(false);
        AudioManager.GetInstance().PlaySound("plopp.3");
    }

    public void ShowObjectOnCursor(Interactable interactable)
    {
        isObjectOnCursorVisible = true;
        uiCursor.ShowObjectIcon(interactable.GetObjectIcon());
    }

    public void HideObjectOnCursor()
    {
        uiCursor.HideObjectImage();
        isObjectOnCursorVisible = false;
    }

    public void ShowContext(object obj)
    {
        bool isOverUI = false;

        /*
        if (obj is Player)
        {
            Player player = (Player)obj;
            isOverUI = true;
            UIContext.GetInstance().SetContext(new UIButtonData[] {
                new UIButtonData(Language.LanguageManager.GetText(Language.LangKey.SwitchPlayer), player.SwitchPlayer)
            });
        }
        */
        SetOverUI(isOverUI);
        UIContext.GetInstance().Show(obj);
    }

    public bool IsCursorOverUI { get => uiCursor.IsOverUI() || CheckPointerOverUI(); }

    private bool CheckPointerOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.layer == (int)Layers.GameUI
                || results[i].gameObject.layer == (int)Layers.UI)
            {
                return true;
            }
        }

        return false;
    }

    public void SetOverUI(bool mode)
    {
        uiCursor.SetOverUI(mode);
    }

    public void ApplySkybox(Material skybox, Light sun)
    {
        if (currentSkybox == skybox)
            return;

        CameraClearFlags clearFlags = CameraClearFlags.Skybox;

        if (null != skybox)
        {
            currentSkybox = skybox;
            clearFlags = CameraClearFlags.Depth;
        }
        else
        {
            sun = uiSun;
            currentSkybox = uiSkybox;
        }

        RenderSettings.sun = sun;
        RenderSettings.skybox = currentSkybox;
        uiCamera.clearFlags = clearFlags;
    }

    public void ShowEscape(bool visible)
    {
        escape.SetActive(visible);
    }

    public void HideCursor(float duration)
    {
        SetCursorVisible(false);
        GameEvent.GetInstance().Execute(
            () =>
                {
                    SetCursorVisible(true);
                },
            duration
        );
    }

    public void SetCursorVisible(bool visible)
    {
        uiCursor.SetActive(visible);
        RestoreObjectIcon(visible);
    }

    private void RestoreObjectIcon(bool visible)
    {
        if (visible && isObjectOnCursorVisible)
            uiCursor.ShowObjectIcon();
        else
            uiCursor.HideObjectImage();
    }

    public void RestoreCursor()
    {
        uiCursor.RestoreImage();
        RestoreObjectIcon(true);
    }

    public void SetCursorEnabled(bool enabled, bool mode)
    {
        uiCursor.SetEnabled(enabled, mode);
    }

    public void SetCursorDisabled(bool mode = true)
    {
        uiCursor.SetDisabled(mode);
    }

    public void SetCursor(GameAction action)
    {
        bool isEnabled = action.isActiveAndEnabled;
        Texture2D tex = action.GetEnabled(isEnabled);
        Vector2 pivot = action.GetPivots(isEnabled ? 1 : 0);
        uiCursor.SetImage(tex, pivot);
    }

    public void AddTmpGUI(GameObject obj)
    {
        if (obj.transform.parent != tmpGUIParent)
            obj.transform.SetParent(tmpGUIParent, false);

        obj.transform.SetAsLastSibling();
    }
}