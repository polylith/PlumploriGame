using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropdownInput : MonoBehaviour
{
    public delegate void OnListShowEvent(UIDropdownInput uiDropdownInput);
    public event OnListShowEvent OnListShow;

    public delegate void OnSelectionChangeEvent(DropdownOption aelectedOptionData);
    public event OnSelectionChangeEvent OnSelectionChange;

    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }
    public DropdownOption SelectedOptionData { get; private set; }
    public int SelectedIndex { get => selectedIndex; set => SetSelectedIndex(value); }
    public bool IsListVisible { get => isListVisible; }

    public UIIconTextButton selectedOption;
    public RectTransform optionsList;
    public RectTransform content;
    [SerializeField]
    public List<DropdownOption> options;
    public int selectedIndex = -1;

    private List<UIIconTextButton> optionButtons;
    private bool isInited;
    private bool isEnabled = true;

    private IEnumerator ieScale;
    private bool isListVisible = true;

    private void Start()
    {
        Init();
        SetListVisible(false, true);

        if (selectedIndex < 0)
        {
            Select(options[0]);
        }
    }
        
    public void Init(bool force = false)
    {
        if (isInited && !force)
            return;

        foreach (Transform trans in content)
            Destroy(trans.gameObject);

        LayoutElement layoutElement = content.GetComponent<LayoutElement>();
        float width = 350f;
        float height = Mathf.Min(3, options.Count) * 85f + 10f;
        optionsList.sizeDelta = new Vector2(width + 25f, height);

        height = options.Count * 85f + 10f;
        content.sizeDelta = new Vector2(width, height);
        layoutElement.preferredHeight = height;
        optionButtons = new List<UIIconTextButton>();

        for (int i = 0; i < options.Count; i++)
        {
            DropdownOption dropdownOption = options[i];
            dropdownOption.index = i;
            UIIconTextButton optionButton = Instantiate(selectedOption);
            optionButtons.Add(optionButton);
            optionButton.transform.name = "Option (" + i + ")";
            optionButton.transform.SetParent(content);
            optionButton.SetText(
                Language.LanguageManager.GetText(dropdownOption.langKey)
            );
            optionButton.sprites = new Sprite[] { dropdownOption.icon };
            optionButton.SetIcon(0);
            optionButton.SetAction(() => { Select(dropdownOption); });
        }
        
        selectedOption.SetAction(ToggleListVisibility);

        isInited = true;
    }

    private void SetEnabled(bool isEnabled)
    {
        if (this.isEnabled == isEnabled)
            return;

        this.isEnabled = isEnabled;
        selectedOption.IsEnabled = isEnabled;

        if (isEnabled)
            return;

        HideList();
    }

    private void SetSelectedIndex(int index)
    {
        if (index < 0 || index >= options.Count || selectedIndex == index)
            return;

        DropdownOption dropdownOption = options[index];
        Select(dropdownOption);
    }

    private void Select(DropdownOption dropdownOption)
    {
        int index = dropdownOption.index;

        if (selectedIndex != index)
        {
            if (selectedIndex > -1)
            {
                optionButtons[selectedIndex].SetState(0);
            }

            selectedIndex = index;
            optionButtons[selectedIndex].SetState(2);
            SelectedOptionData = dropdownOption;
            selectedOption.SetText(
                Language.LanguageManager.GetText(dropdownOption.langKey)
            );
            selectedOption.sprites = new Sprite[] { SelectedOptionData.icon };
            selectedOption.SetIcon(0);
            OnSelectionChange?.Invoke(SelectedOptionData);
        }

        HideList();
    }

    public void ToggleListVisibility()
    {
        SetListVisible(!isListVisible);
        OnListShow?.Invoke(this);
    }

    public void ShowList()
    {
        if (isListVisible)
            return;

        SetListVisible(true);
    }

    public void HideList()
    {
        if (!isListVisible)
            return;

        SetListVisible(false);
    }

    private void SetListVisible(bool isVisible, bool instant = false)
    {
        if (isListVisible == isVisible)
            return;

        isListVisible = isVisible;
        Vector3 scale = new Vector3(1f, isVisible ? 1f : 0f, 1f);

        if (instant)
        {
            optionsList.localScale = scale;
            return;
        }

        if (null != ieScale)
            StopCoroutine(ieScale);

        ieScale = IEScale(scale);
        StartCoroutine(ieScale);
    }

    private IEnumerator IEScale(Vector3 scale)
    {
        float f = 0f;

        while (f <= 1f)
        {
            optionsList.localScale = Vector3.Lerp(optionsList.localScale, scale, f);

            yield return null;

            f += 0.2f;
        }

        optionsList.localScale = scale;

        yield return null;

        ieScale = null;
    }
}
