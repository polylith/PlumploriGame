using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PCIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private static Vector3[] scales = new Vector3[]
    {
        new Vector3(0.55f, 0.55f, 1f),
        new Vector3(0.75f, 0.75f, 1f),
        Vector3.one
    };
    
    public RectTransform rectTransform;

    public string soundID = "click1";
    public Image icon;

    private PCApp app;
    private int state = -1;

    private Transform desktopGrid;
    private Transform taskBar;
    private PCIcon taskIcon;

    private void Start()
    {
        if (state < 0)
        {
            SetState(0);
            return;
        }

        int tmpState = state;
        state = -1;
        SetState(tmpState);
    }

    public void SetParents(Transform desktopGrid, Transform taskBar)
    {
        this.desktopGrid = desktopGrid;
        this.taskBar = taskBar;
        SetParent(this.desktopGrid);
    }

    public void ShowInTaskBar(bool mode)
    {
        if (mode)
        {
            if (null == taskIcon)
            {
                taskIcon = Instantiate(this) as PCIcon;
                taskIcon.app = app;
                taskIcon.soundID = soundID;
                taskIcon.taskBar = taskBar;
                taskIcon.desktopGrid = desktopGrid;
                taskIcon.icon = icon;
                taskIcon.state = state;
                taskIcon.name = app.GetAppName() + " (Task Icon)";
                SetState(1);
            }

            taskIcon.SetParent(taskBar, true);
            return;
        }

        if (null != taskIcon)
        {
            Destroy(taskIcon.gameObject);
        }

        SetState(0);
        taskIcon = null;
    }

    private void SetParent(Transform parent, bool resize = false)
    {
        transform.SetParent(parent, false);
        gameObject.SetActive(app.IsEnabled);

        if (resize)
        {
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = new Vector2(
                layoutElement.preferredWidth,
                layoutElement.preferredHeight
            ) * 0.5f;
        }
    }

    public void SetApp(PCApp app)
    {
        this.app = app;

        if (null != taskIcon)
            taskIcon.SetApp(app);
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;

        if (null != taskIcon)
            taskIcon.SetIcon(sprite);
    }

    public void ResetState()
    {
        SetState(0);
    }

    private void SetState(int state)
    {
        if (this.state == state)
            return;

        if (null != taskIcon)
        {
            if (!app.IsActive)
                taskIcon.SetState(state);

            this.state = 1;
        }
        else
        {
            this.state = state;
        }
        
        transform.localScale = scales[this.state];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (state == 2)
            return;

        SetState(1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (state == 2)
            return;

        SetState(0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.GetInstance().PlaySound(soundID);

        if (state == 2)
        {
            SetState(1);
            app?.Hide();
        }
        else
        {
            SetState(2);
            app?.Show();
        }
    }
}