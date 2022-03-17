using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIOnOffButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public delegate void OnStateChangeEvent(bool isOn);
    public event OnStateChangeEvent OnStateChange;

    public bool IsOn { get => isOn; }
    public bool IsEnabled { get => isEnabled; }

    public string soundId = "click2";

    public Image back;
    public Image knob;
    public RectTransform knobTransform;
    public TextMeshProUGUI label;
    public bool initialIsOn;

    [Range(0.1f, 1f)]
    public float smothTime = 0.25f;

    public Vector3[] knobPositions = new Vector3[]
    {
        new Vector3(-17.5f, 0f, 0f),
        new Vector3(17.5f, 0f, 0f)
    };
    public Color[] stateColors = new Color[]
    {
        Color.gray,
        Color.yellow
    };
    public Color[] knobColors = new Color[]
    {
        Color.black,
        new Color(1f, 0.53f, 0f)
    };
    public Color[] highlightColors = new Color[]
    {
        Color.black,
        Color.blue
    };

    private bool isOn;
    private Vector3 knobPosition;
    private int state;
    private bool isEnabled = true;

    private void Start()
    {
        Reset(true);
    }

    private void Update()
    {
        if (!isEnabled)
            return;

        knobTransform.localPosition = Vector3.Lerp(
            knobTransform.localPosition,
            knobPosition, smothTime
        );
    }

    public void SetEnabled(bool isEnabled)
    {
        if (this.isEnabled == isEnabled)
            return;

        int index = isOn ? 1 : 0;
        Color labelColor = isEnabled ? highlightColors[state] : Color.gray;
        Color stateColor = isEnabled ? stateColors[index] : Color.gray;
        Color knobColor = isEnabled ? knobColors[index] : Color.gray;

        knobPosition = knobPositions[index];
        back.color = stateColor;
        knob.color = knobColor;
        label.color = labelColor;

        this.isEnabled = isEnabled;
    }

    public void SetLabelText(string text)
    {
        label.SetText(text);
    }

    public void Reset(bool force = false)
    {
        SetIsOn(initialIsOn, force);
    }

    private void Toggle()
    {
        AudioManager.GetInstance().PlaySound(soundId);
        SetIsOn(!isOn);
    }

    public void SetIsOn(bool isOn, bool force = false)
    {
        if (this.isOn == isOn && !force)
            return;

        int index = isOn ? 1 : 0;
        knobPosition = knobPositions[index];

        if (!isEnabled)
            knobTransform.localPosition = knobPosition;

        back.color = stateColors[index];
        knob.color = knobColors[index];
        this.isOn = isOn;
        OnStateChange?.Invoke(isOn);
    }

    private void Hightlight(int state)
    {
        if (this.state == state)
            return;

        this.state = state;
        Color color = highlightColors[state];
        label.color = color;
        int index = isOn ? 1 : 0;
        knob.color = state > 0 ? color : knobColors[index];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled)
            return;

        Toggle();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEnabled)
            return;

        UIGame.GetInstance().SetCursorEnabled(true, false);
        Hightlight(1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEnabled)
            return;

        UIGame uiGame = UIGame.GetInstance();
        bool mode = !uiGame.IsCursorOverUI && !uiGame.IsUIExclusive;
        uiGame.RestoreCursor();
        uiGame.SetCursorEnabled(false, mode);
        Hightlight(0);
    }
}
