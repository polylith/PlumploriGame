using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMap : MonoBehaviour
{
    public UIIconButton closeButton;
    public Image mapImage;
    public Image positionMarker;
    public TextMeshProUGUI textMesh;

    private bool isVisible = true;
    private IEnumerator ieScale;
    private bool inited;
    private Plan plan;
    private IEnumerator ieUpdateMap;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (inited)
            return;

        SetText("");
        ApplyTexture(null);
        inited = true;
    }

    public void UpdateMap(System.Action<bool> action = null, bool instant = false)
    {
        if (!isVisible || null != ieUpdateMap)
            return;
        /* TODO
        if (GameManager.GetInstance().CurrentWorld.PlanChanged())
        {
            ieUpdateMap = IEUpdateMap(action, instant);
            StartCoroutine(ieUpdateMap);
        }
        else
        */
        {
            action?.Invoke(instant);
        }        
    }

    /* TODO
    private IEnumerator IEUpdateMap(System.Action<bool> action = null, bool instant = false)
    {
        UIProgress.GetInstance().Run();

        yield return new WaitForSecondsRealtime(2f);

        GameManager.GetInstance().CurrentWorld.GetPlan(SetMap, action, instant);

        yield return new WaitForSecondsRealtime(2f);

        ieUpdateMap = null;
    }
    */

    private void SetMap(Plan plan)
    {
        this.plan = plan;

        if (null == plan)
        {
            SetText(Language.LanguageManager.GetText(Language.LangKey.NotAvailable, Language.LanguageManager.GetText(Language.LangKey.Map)));
            ApplyTexture(null);
        }
        else
        {
            ApplyTexture(plan.GetTexture());
            SetText(plan.GetText());
        }

        UIProgress.GetInstance().Stop();
    }

    private void UpdateMarker()
    {
        if (null == plan || !isVisible)
        {
            positionMarker.transform.gameObject.SetActive(false);
            return;
        }

        Vector4 pos = plan.UpdatePositionMarker();
        pos.x = mapImage.rectTransform.rect.x + pos.x * mapImage.rectTransform.rect.width + positionMarker.rectTransform.rect.center.x;
        pos.y = mapImage.rectTransform.rect.y + pos.y * mapImage.rectTransform.rect.height - positionMarker.rectTransform.rect.center.y;
        positionMarker.transform.gameObject.SetActive(pos.z >= 0f);
        positionMarker.rectTransform.anchoredPosition = new Vector2(pos.x * 0.5f, pos.y);
        positionMarker.rectTransform.localRotation = Quaternion.Euler(0f, 0f, pos.w);
    }

    private void ApplyTexture(Texture2D tex)
    {
        if (null != tex)
        {
            mapImage.color = Color.white;
            mapImage.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
        }
        else
        {
            mapImage.color = Color.clear;
            mapImage.sprite = null;
        }
    }

    private void SetText(string text)
    {
        textMesh.text = text;
    }
        
    public void Show(bool isVisible, bool instant = false)
    {
        if (this.isVisible == isVisible)
            return;

        this.isVisible = isVisible;
        
        if (isVisible)
        {
            closeButton.SetToolTip(Language.LanguageManager.GetText(Language.LangKey.Hide, Language.LanguageManager.GetText(Language.LangKey.Map)));
            gameObject.SetActive(true);
            UpdateMap(StartScale,instant);
        }
        else
        {
            StartScale(instant);
        }
        
        if (!instant)
            AudioManager.GetInstance().PlaySound(isVisible ? "max" : "min", gameObject);
    }

    private void StartScale(bool instant = false)
    {
        Vector3 scale = isVisible ? Vector3.one : Vector3.zero;
        StopScale();

        if (instant)
        {
            transform.localScale = scale;
            UpdateMarker();

            if (!isVisible)
                gameObject.SetActive(false);
        }
        else
        {
            ieScale = IEScale(scale);
            StartCoroutine(ieScale);
        }
    }

    private void StopScale()
    {
        if (null == ieScale)
            return;

        StopCoroutine(ieScale);
        ieScale = null;
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0;

        while (f <= 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, f);

            yield return null;

            f += Time.deltaTime;
        }

        transform.localScale = scale;

        if (!isVisible)
            gameObject.SetActive(false);

        ieScale = null;
    }

    private void Update()
    {
        if (!isVisible)
            return;

        UpdateMarker();
    }
}