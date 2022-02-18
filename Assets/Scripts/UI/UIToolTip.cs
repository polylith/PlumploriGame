using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Action;

public class UIToolTip : MonoBehaviour
{
    public static TextMeshProUGUI TmpTextMesh { get; set; }

    public static Color[] colors = new Color[] { Color.red, Color.gray, Color.white };
    private static Vector3[] scales = new Vector3[] { Vector3.zero, new Vector3(0.55f, 0.55f, 1f) };
    private static Vector3 offset = new Vector3(10f, 5f, 0f);
    private static float delay = 0.75f;

    private static UIToolTip ins;

    public static UIToolTip GetInstance()
    {
        return ins;
    }

    public RectTransform rectTransform;
    public TextMeshProUGUI textMesh;
    public Vector3 size;
    public Image back;

    private bool isVisible;
    private bool isHidden;
    private IEnumerator ieShow;
    private bool follow;
    
    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            isVisible = true;
            Hide(true);
            ResetColor();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text, int colorIndex)
    {
        textMesh.SetText(text);
        textMesh.color = colors[colorIndex + 1];

        //if (!isVisible && !isHidden)
          //  Show();
    }

    public void ClearText()
    {
        SetText("", 1);
    }
        
    public void ResetColor()
    {
        textMesh.color = Color.white;
    }
        
    public void SetColor(Color color)
    {
        textMesh.color = color;
    }

    public void Restore()
    {
        ClearText();
        ResetColor();
        ActionController.GetInstance().HighlightCurrent();
    }

    public void Show(Transform trans)
    {
        if (!isVisible)
            transform.localScale = scales[0];

        isHidden = false;
        Vector3 pos = Input.mousePosition;
        Vector2 pivot = Vector2.zero;

        if (null != trans)
        {
            if (trans.gameObject.layer != (int)Layers.GameUI)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(trans.position);
                pivot = new Vector2(0.5f, 0.5f);
                pos.x = screenPos.x;
                pos.y = Mathf.Max(pos.y + 80f, screenPos.y);
                textMesh.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                RectTransform rectTransform = trans.GetComponent<RectTransform>();
                pivot = new Vector2(0.125f, 1f);
                pos.x = rectTransform.position.x;
                pos.y = rectTransform.position.y + rectTransform.rect.height;
                textMesh.alignment = TextAlignmentOptions.BaselineLeft;
            }
        }

        follow = null == trans;
        float[] values = Calc.CalculatePositionOnScreen(pos, pivot, offset, size);

        pos.x = values[0];
        pos.y = values[1];
        pivot.x = values[2];
        pivot.y = values[3];
        float rotY = values[4];
        float rotZ = values[5];

        back.rectTransform.localRotation = Quaternion.Euler(0f, rotY, rotZ);
        rectTransform.pivot = pivot;
        transform.position = pos;

        if (!isVisible)
        {
            if (null != ieShow)
                StopCoroutine(ieShow);

            gameObject.SetActive(true);
            isVisible = true;
            ieShow = IEShow();
            StartCoroutine(ieShow);
        }
    }

    public void Hide()
    {
        Hide(false);
    }

    public void Hide(bool instant)
    {
        if (!isVisible)
            return;

        isVisible = false;
        isHidden = true;

        if (instant)
        {
            transform.localScale = scales[0];
            ClearText();
        }
        else
        {
            if (null != ieShow)
                StopCoroutine(ieShow);

            ieShow = IEShow();
            StartCoroutine(ieShow);
        }
    }

    private IEnumerator IEShow()
    {
        if (isVisible)
            yield return new WaitForSecondsRealtime(delay);

        float f = 0f;
        float df = Time.deltaTime * 3f;
        Vector3 scale = scales[isVisible ? 1 : 0];
        
        while (f <= 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, f);
            yield return null;
            f += df;
        }

        transform.localScale = scale;

        if (!isVisible)
        {
            gameObject.SetActive(false);
            ClearText();
        }

        ieShow = null;
    }

    private void Update()
    {
        if (!isVisible || !follow)
            return;

        Vector3 pos = Input.mousePosition;
        pos = new Vector3(pos.x, pos.y, 0f) + offset;
        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 3f);
    }
}
