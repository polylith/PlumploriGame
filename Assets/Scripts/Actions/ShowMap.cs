using UnityEngine;
using Language;
using Action;

/// <summary>
/// <para>
/// This class is to display the map of the entire world
/// where the current player is currently located.
/// </para>
/// <para>
/// This is just the interactive icon in the action bar
/// to show or hide the map.
/// </para>
/// </summary>
public class ShowMap : MonoBehaviour
{
    public Sprite sprite;
    public UIMap uiMap;

    private Material mat;
    private bool inited = false;
    private bool isVisible = true;
    private bool planVisible = false;


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (inited)
            return;

        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;
        mat.mainTexture = sprite.texture;
        uiMap.Show(false, true);
        UpdateState(0);
        inited = true;
    }

    private void SetActive(bool isVisible)
    {
        if (this.isVisible == isVisible)
            return;

        // TODO
        this.isVisible = isVisible; // && GameManager.GetInstance().CurrentWorld.HasPlans();
        gameObject.SetActive(isVisible);        
    }

    public void UpdateMap()
    {
        uiMap.UpdateMap();
    }

    private void UpdateState(int i)
    {
        mat.color = UIButton.colors[i];
    }

    public void HidePlan()
    {
        ShowPlan(false);
    }

    private void ShowPlan(bool planVisible)
    {
        UIGame.GetInstance().SetOverUI(planVisible);
        uiMap.Show(planVisible);
        this.planVisible = planVisible;
    }

    private void OnMouseEnter()
    {
        UIGame.GetInstance().SetCursorEnabled(true, false);
        UIToolTip.GetInstance().SetText(LanguageManager.GetText(planVisible
            ? LangKey.Hide : LangKey.Show,
            LanguageManager.GetText(LangKey.Map)), 1);
        UIToolTip.GetInstance().ResetColor();
        UpdateState(1);
    }

    private void OnMouseExit()
    {
        UpdateState(0);
        ActionController.GetInstance().UpdateToolTip();
    }

    private void OnMouseUpAsButton()
    {
        ShowPlan(!planVisible);
        ActionController.GetInstance().UpdateToolTip();
        AudioManager.GetInstance().PlaySound("click", gameObject);
    }
}