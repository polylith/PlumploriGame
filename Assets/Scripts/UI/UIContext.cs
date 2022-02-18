using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIContext : MonoBehaviour, IPointerExitHandler
{
    public static UIContext GetInstance()
    {
        return ins;
    }

    private static UIContext ins;

    public UITextButton buttonPrefab;

    private object refObj;
    private bool isVisble = false;
    private RectTransform rectTrans;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            rectTrans = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetContext(UIButtonData[] buttonData)
    {
        Clear();

        if (null == buttonData || buttonData.Length == 0)
            return;

        for (int i = 0; i < buttonData.Length; i++)
        {
            UIButtonData data = buttonData[i];
            UITextButton button = buttonPrefab.Instantiate(data);
            button.name = "UI Button (" + i + ")";
            button.OnClick = Hide;
            button.transform.SetParent(transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseExit();
    }

    public void Clear()
    {
        foreach (Transform trans in transform)
            Destroy(trans.gameObject);
    }

    private void MouseExit()
    {
        Hide();
    }

    public void Show(object obj)
    {
        if (isVisble)
            return;

        refObj = obj;
        isVisble = true;
        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;
        float width = 1.125f * rectTrans.rect.width;
        float height = 1.125f * rectTrans.rect.height;
        Vector2 pivot = rectTrans.pivot;

        pivot.x = x + width > Screen.width ? 1f : 0f;
        pivot.y = y + height > Screen.height ? 1f : 0f;

        rectTrans.pivot = pivot;
        transform.position = new Vector2(x,y);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!isVisble)
            return;

        if (null != refObj)
        {
            if (refObj is Player)
                ((Player)refObj).Highlight(false);
        }

        refObj = null;
        isVisble = false;
        gameObject.SetActive(false);
        UIGame.GetInstance().SetOverUI(false);
    }
}