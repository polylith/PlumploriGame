using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Language;
using System.Collections.Generic;
using Movement;

/// <summary>
/// This is the interactive UI for a Robovac.
/// Here the Robovac can be switched on and off,
/// the charge state and the progress in tracking
/// the targets can be monitored. If there are no
/// targets yet, an automatic scan of the current
/// room can be started.
/// </summary>
public class RobovacUI : InteractableUI
{
    public UIIconButton onOffButton;

    public UIIconButton autoScanButton;
    public Image warningIcon;
    public Image okayIcon;

    public Image alertIcon;

    public TextMeshProUGUI chargeStateLabel;
    public Image chargeDisplay;
    public Image chargeIcon;
    public TextMeshProUGUI chargeTextDisplay;

    public TextMeshProUGUI currentStateLabel;
    public TextMeshProUGUI currentStateDisplay;

    public TextMeshProUGUI progressLabel;
    public Image ProgressDisplay;
    public TextMeshProUGUI progressTextDisplay;

    public RectTransform mapPanel;
    public UIMapMarker uiMapMarkerPrefab;
    public RawImage mapImage;
    
    private float chargeValue = -1f;
    private int chargeColorIndex = -1;
    private static Color[] chargeColors = new Color[] {
        Color.green,
        Color.cyan,
        Color.yellow,
        Color.red
    };
    private bool isMapPanelVisible;
    private List<UIMapMarker> uiMapMarkers;

    protected override void Initialize()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        robovac.OnChargeUpdate += UpdateChargeState;
        robovac.OnAutoScan += UpdateAutoScanButton;
        robovac.OnStateChange += UpdateCurrentStateDisplay;
        robovac.OnProgressUpdate += UpdateProgress;

        onOffButton.SetAction(ToggleOnOff);
        isMapPanelVisible = true;
        ShowMapPanel(false, true);
        UpdateUI();
    }

    protected override void BeforeHide()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        robovac.OnChargeUpdate -= UpdateChargeState;
        robovac.OnAutoScan -= UpdateAutoScanButton;
        robovac.OnStateChange -= UpdateCurrentStateDisplay;
        robovac.OnProgressUpdate -= UpdateProgress;
    }

    private void UpdateChargeState(float value)
    {
        if (chargeValue == value)
            return;

        chargeValue = value;
        int index = 0;
        
        if (value < 0.15f)
        {
            index = 3;
        }
        else if (value < 0.5f)
        {
            index = 2;
        }
        else if (value < 0.75f)
        {
            index = 1;
        }

        if (chargeColorIndex != index)
        {
            chargeColorIndex = index;
            Color color = chargeColors[chargeColorIndex];
            chargeIcon.color = color;
            chargeDisplay.color = color;
        }

        chargeIcon.fillAmount = value;
        chargeDisplay.fillAmount = value;
        string text = Mathf.Round(value * 100f) + " %";
        chargeTextDisplay.SetText(text);
    }

    private void UpdateProgress(float value)
    {
        ProgressDisplay.fillAmount = value;
        string text = Mathf.Round(value * 100f) + " %";
        progressTextDisplay.SetText(text);
    }
    
    private void AutoScan()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        autoScanButton.SetEnabled(false);

        if (robovac.CurrentState != Robovac.State.Scanning)
        {
            robovac.StartScanning();
        }
        else
        {
            robovac.StopScanning();
        }
    }

    private void ToggleOnOff()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        bool isOn = robovac.CurrentState > Robovac.State.Off;

        if (isOn)
        {
            robovac.SwitchOff();
        }
        else
        {
            robovac.SwitchOn();
        }
    }

    private void UpdateUI()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        chargeStateLabel.SetText(
            LanguageManager.GetText(LangKey.ChargeState)
        );
        currentStateLabel.SetText(
            LanguageManager.GetText(LangKey.CurrentState)
        );
        progressLabel.SetText(
            LanguageManager.GetText(LangKey.Progress)
        );
        UpdateChargeState(robovac.ChargeState);
        UpdateProgress(robovac.Progress);
        UpdateAutoScanButton();
        UpdateOnOffButton();
        UpdateCurrentStateDisplay();
    }

    private void UpdateOnOffButton()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        bool isOn = robovac.CurrentState > Robovac.State.Off;
        bool isEnabled = robovac.CurrentState < Robovac.State.Working;

        onOffButton.SetActiveColor(isOn ? Color.red : Color.green);
        onOffButton.SetIcon(isOn ? 0 : 1);
        onOffButton.SetToolTip(
            LanguageManager.GetText(
                isOn ? LangKey.SwitchOff : LangKey.SwitchOn,
                robovac.GetText()
            )
        );

        onOffButton.SetEnabled(isEnabled);
    }

    private void UpdateAutoScanButton()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        string toolTip = LanguageManager.GetText(
            LangKey.Scan,
            LanguageManager.GetText(LangKey.Environment)
        );
        bool isScanning = robovac.CurrentState == Robovac.State.Scanning;
        Color color = Color.yellow;
        int state = 0;
        System.Action action = AutoScan;

        if (isScanning)
        {
            state = 1;
            action = null;
            toolTip = null;
        }
        else if (robovac.HasTargets)
        {
            state = 2;
            color = Color.green;

            if (isMapPanelVisible)
                action = HideMapPanel;
            else
                action = ShowMapPanel;

            toolTip = LanguageManager.GetText(
                isMapPanelVisible ? LangKey.Hide : LangKey.Show,
                LanguageManager.GetText(LangKey.Data)
            );
        }
        
        warningIcon.gameObject.SetActive(state == 0);
        okayIcon.gameObject.SetActive(state == 2);
        autoScanButton.SetActiveColor(color);

        autoScanButton.SetToolTip(toolTip);
        autoScanButton.SetAction(action);
        autoScanButton.SetEnabled(!isScanning);        
    }

    private void UpdateCurrentStateDisplay()
    {
        if (null == interactable || !(interactable is Robovac robovac))
            return;

        bool hasTargets = robovac.HasTargets;
        alertIcon.gameObject.SetActive(!hasTargets);
        currentStateDisplay.SetText(robovac.GetDescription());
        currentStateDisplay.color = hasTargets ? Color.white : Color.yellow;
        UpdateOnOffButton();
    }

    private void ShowMapPanel(bool isVisible, bool instant = false,
        System.Action callBack = null)
    {
        if (isMapPanelVisible == isVisible)
        {
            return;
        }

        isMapPanelVisible = isVisible;

        Vector3 scale = isVisible ? Vector3.one : new Vector3(0f, 1f, 1f);

        if (instant)
        {
            mapPanel.localScale = scale;
            callBack?.Invoke();
        }
        else
        {
            mapPanel.DOScale(scale, 0.25f);

            if (null != callBack)
            {
                GameEvent.GetInstance().Execute(callBack, 0.5f);
            }
        }
    }

    private void ShowMapPanel()
    {
        ShowMapPanel(true, false, ShowTargets);
    }

    private void HideMapPanel()
    {
        ShowMapPanel(false, false);
    }

    private void ShowTargets()
    {
        UpdateAutoScanButton();

        if (!isMapPanelVisible)
            return;

        if (null == interactable || !(interactable is Robovac robovac))
            return;

        if (null != uiMapMarkers)
        {
            foreach (UIMapMarker marker in uiMapMarkers)
            {
                Destroy(marker.gameObject);
            }

            uiMapMarkers = null;
        }

        NavMeshPointsData targets = robovac.Targets;

        if (null == targets || targets.Points.Count == 0)
            return;

        uiMapMarkers = new List<UIMapMarker>();

        float x0 = targets.Rect.min.x;
        float y0 = targets.Rect.min.y;
        Rect mapRect = mapImage.rectTransform.rect;
        Debug.Log(mapRect + "\n" + targets.Rect);
        float fx = Mathf.Abs(mapRect.width / targets.Rect.width);
        float fy = Mathf.Abs(mapRect.height / targets.Rect.height);

        for (int i = 0; i < targets.Points.Count; i++)
        {
            Vector3 point = targets.Points[i];
            float x = Mathf.Abs(point.x - x0) * fx;
            // map z to y in 2D UI
            float y = Mathf.Abs(point.z - y0) * fy;
            UIMapMarker marker = Instantiate(uiMapMarkerPrefab) as UIMapMarker;
            marker.transform.name = "UI Map Marker (" + i + ")";
            marker.transform.SetParent(mapImage.transform);
            marker.SetPosition(new Vector3(x, y, 0f));
            marker.Show();
            uiMapMarkers.Add(marker);
        }
    }
}
