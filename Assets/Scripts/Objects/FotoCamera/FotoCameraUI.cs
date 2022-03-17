using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using Language;

/// <summary>
/// This is the UI for a photo camera.
/// </summary>
/// TODO reshape to an implentation of the InteractableUI
public class FotoCameraUI : InteractableUI
{
    private static FotoCameraUI ins;

    public static FotoCameraUI GetInstance()
    {
        return ins;
    }

    public Text colorGradingInfo;
    public PostProcessVolume volume;
    public AudioSource audioSource;
    public UIIconButton[] buttons;
    public Image back;
    public Transform colorGradingPanel;

    public Transform zoomTrans;
    public Transform FocalLengthTrans;
    public Picture picturePrefab;
    public Transform fotoTrans;
    public UISnapshot uISnapshot;
    public SnapshotCamera cam;
    public Slider zoomSlider;
    public Slider focalLengthSlider;
    public Slider[] sliders;

    private ColorGrading colorGrading;
    private DepthOfField dofLayer;
    private int lastIndex;
    private bool isSaving;
    private float[] colorValues;
    private IEnumerator ieShowWait;
    private Collectable currentItem;
    private bool isPPVolumeInited;

    protected override void Initialize()
    {
        if (null == ins)
        {
            ins = this;
        }

        if (!isPPVolumeInited)
        {
            volume.profile.TryGetSettings<ColorGrading>(out colorGrading);
            volume.profile.TryGetSettings<DepthOfField>(out dofLayer);
            isPPVolumeInited = true;
        }

        HideColorGrandingPanel();
        InitSettings();
        InitButtons();
        uISnapshot.Activate(cam.cam, true);
    }

    private void Clear()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        UIFoto[] fotos = fotoCam.GetFotos();

        for (int i = 0; i < fotos.Length; i++)
        {
            fotos[i].transform.SetParent(fotoCam.transform, false);
            fotos[i].gameObject.SetActive(false);
        }
    }

    private void InitColorGrading()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        colorValues = fotoCam.GetColorGrading();

        if (null != colorGrading)
        {
            colorGrading.hueShift.value = colorValues[0];
            colorGrading.saturation.value = colorValues[1];
            colorGrading.brightness.value = colorValues[2];
            colorGrading.contrast.value = colorValues[3];
        }

        for (int i = 0; i < sliders.Length; i++)
            sliders[i].value = colorValues[i];
    }

    private void InitSettings()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        lastIndex = -1;
        isSaving = false;
        float zoom = fotoCam.GetZoom();
        zoomSlider.value = zoom;
        cam.cam.focalLength = zoom;

        float focalLength = fotoCam.GetFocalLength();
        focalLengthSlider.value = focalLength;

        if (null != dofLayer)
            dofLayer.focalLength.value = focalLength;

        UIFoto[] fotos = fotoCam.GetFotos();

        for (int i = 0; i < fotos.Length; i++)
        {
            fotos[i].transform.SetParent(fotoTrans, false);
            fotos[i].gameObject.SetActive(true);
        }

        InitColorGrading();
        InitButtons();

        int currentIndex = fotoCam.GetCurrentFoto();

        if (currentIndex == -1)
        {
            SetFotoMode();
            int count = fotoCam.GetFotoCount();
            HighLight(count);
        }
        else
            View(currentIndex);
    }

    private void ClearColorGradingInfo()
    {
        colorGradingInfo.text = "";
        colorGradingInfo.gameObject.SetActive(false);
    }

    private void ShowColorGradingInfo(string s1, int value)
    {
        string s2 = value.ToString();

        while (s2.Length < 8)
            s2 = " " + s2;

        colorGradingInfo.text = s1 + s2;
        colorGradingInfo.gameObject.SetActive(true);
    }

    private void ShowColorGrandingPanel()
    {
        colorGradingPanel.gameObject.SetActive(true);
    }

    private void HideColorGrandingPanel()
    {
        ClearColorGradingInfo();
        colorGradingPanel.gameObject.SetActive(false);
    }

    private void InitButtons()
    {
        buttons[1].SetAction(Hide);
        buttons[1].SetToolTip(LanguageManager.GetText(LangKey.SwitchOff, LanguageManager.GetText(LangKey.Camera)));

        buttons[3].SetAction(ExportPicture);
        buttons[3].SetToolTip(LanguageManager.GetText(LangKey.Develop, LanguageManager.GetText(LangKey.Image)));
        buttons[3].gameObject.SetActive(false);

        buttons[4].SetAction(HideColorGrandingPanel);
        buttons[4].SetToolTip(LanguageManager.GetText(LangKey.Back));

        SetFotoMode();

        
    }

    private void ShowUIArrows(bool isVisible)
    {
        UIArrows uiArrows = UIArrows.GetInstance();

        if (!isVisible)
        {
            uiArrows.Hide(this);
            return;
        }

        if (!uiArrows.IsCurrentlyInUse)
        {
            uiArrows.Show(
                this,
                () => MoveCamera(0, -1),
                () => MoveCamera(0, 1),
                () => MoveCamera(-1, 0),
                () => MoveCamera(1, 0)
            );
        }
    }

    private void MoveCamera(int dirX, int dirY)
    {
        if (dirX != 0)
        {
            GameManager.GetInstance().CurrentPlayer.RotateY(15f * dirX);
        }

        if (dirY != 0)
        {
            Vector3 rot = cam.transform.localEulerAngles;
            rot.x += dirY * 15f;
            cam.transform.DOLocalRotate(rot, 0.5f);
        }
    }

    private void ExportPicture()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        int currentIndex = fotoCam.GetCurrentFoto();
        UIFoto[] fotos = fotoCam.GetFotos();
        UIFoto foto = fotos[currentIndex];
        Picture pic = picturePrefab.Instantiate(foto.GetTexture());
        Player player = GameManager.GetInstance().CurrentPlayer;
        pic.gameObject.layer = player.gameObject.layer;

        if (pic.AddInventory(player))
        {
            AudioManager.GetInstance().PlaySound("dingdong");
            ShowObject(pic, true);
            foto.Delete();
            SetFotoMode();
        }
        else
        {
            AudioManager.GetInstance().PlaySound("buzzer");
        }
    }

    public void Zoom()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        float zoom = zoomSlider.value;
        zoomSlider.interactable = false;
        cam.cam.focalLength = zoom;
        zoomSlider.interactable = true;
        fotoCam.SetZoom(zoom);
        ShowColorGradingInfo("Zoom", (int)zoomSlider.value);

        if (!audioSource.isPlaying)
            AudioManager.GetInstance().PlaySound("camera.zoom", gameObject, 1f, audioSource);
    }

    public void SetFocalLength()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        float focalLength = focalLengthSlider.value;
        focalLengthSlider.interactable = false;
        dofLayer.focalLength.value = focalLength;
        focalLengthSlider.interactable = true;
        fotoCam.SetFocalLength(focalLength);
        ShowColorGradingInfo("Focal Length", (int)focalLengthSlider.value);
    }

    public void SetHueShift()
    {
        if (null == colorValues || colorValues.Length == 0)
            return;

        colorValues[0] = sliders[0].value;
        colorGrading.hueShift.value = colorValues[0];
        UpdateColorGrading();
        ShowColorGradingInfo("Hue Shift", (int)sliders[0].value);
    }

    public void SetSaturation()
    {
        if (null == colorValues || colorValues.Length == 0)
            return;

        colorValues[1] = sliders[1].value;
        colorGrading.saturation.value = colorValues[1];
        UpdateColorGrading();
        ShowColorGradingInfo("Saturation", (int)sliders[1].value);
    }

    public void SetBrightness()
    {
        if (null == colorValues || colorValues.Length == 0)
            return;

        colorValues[2] = sliders[2].value;
        colorGrading.brightness.value = colorValues[2];
        UpdateColorGrading();
        ShowColorGradingInfo("Brightness", (int)sliders[2].value);
    }

    public void SetContrast()
    {
        if (null == colorValues || colorValues.Length == 0)
            return;

        colorValues[3] = sliders[3].value;
        colorGrading.contrast.value = colorValues[3];
        UpdateColorGrading();
        ShowColorGradingInfo("Contrast", (int)sliders[3].value);
    }

    private void UpdateColorGrading()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        fotoCam.SetColorGrading(colorValues);
    }

    private void Save()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        int currentIndex = fotoCam.GetCurrentFoto();

        if (currentIndex < 0 || isSaving)
            return;

        isSaving = true;
        StartCoroutine(SaveImage());
    }

    private IEnumerator SaveImage()
    {
        if (interactable is FotoCamera fotoCam && null != fotoCam)
        {
            UIProgress uiProgress = UIProgress.GetInstance();
            uiProgress.Run();

            yield return new WaitForSecondsRealtime(1f);

            int currentIndex = fotoCam.GetCurrentFoto();
            UIFoto[] fotos = fotoCam.GetFotos();
            UIMessage msg = fotos[currentIndex].Save();

            yield return new WaitForSecondsRealtime(1f);

            AudioManager.GetInstance().PlaySound(msg.isError ? "buzzer" : "dingdong");
            ShowText(msg, 5f);

            if (!msg.isError)
            {
                uiProgress.ProgressReady();
            }

            yield return new WaitForSecondsRealtime(1f);

            uiProgress.Stop();

            yield return new WaitForSecondsRealtime(1f);

            isSaving = false;
        }

        yield return null;
    }

    private void ShowText(UIMessage msg, float clearTime = 0f)
    {
        if (null == msg)
            return;

        UIToolTip uiToolTip = UIToolTip.GetInstance();

        uiToolTip.SetText(msg.text, 1);
        uiToolTip.SetColor(msg.isError ? Color.red : Color.yellow);

        if (clearTime > 0f)
            GameEvent.GetInstance().Execute(uiToolTip.ClearText, clearTime);
    }
    
    private void SwitchAction()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        int currentIndex = fotoCam.GetCurrentFoto();

        if (currentIndex == -1)
        {
            HideColorGrandingPanel();
            GetSnapshot();
            return;
        }

        SetFotoMode();
    }

    private void SetFotoMode()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;


        ShowUIArrows(true);
        HideColorGrandingPanel();
        ShowObject(fotoCam);

        buttons[0].soundID = "camera.focus";
        buttons[0].SetAction(SwitchAction);
        buttons[0].SetToolTip(LanguageManager.GetText(LangKey.TakePhoto));
        buttons[0].SetIcon(0);
        zoomTrans.gameObject.SetActive(true);
        FocalLengthTrans.gameObject.SetActive(true);

        buttons[2].SetIcon(0);
        buttons[2].SetAction(ShowColorGrandingPanel);
        buttons[2].SetToolTip(LanguageManager.GetText(LangKey.ColorGrading));

        buttons[3].gameObject.SetActive(false);
        fotoCam.SetCurrentFoto(-1);
        uISnapshot.Activate(cam.cam, false);
        int count = fotoCam.GetFotoCount();
        HighLight(count);
    }

    private void ShowObject(Collectable item, bool instant = false)
    {
        UIGame uiGame = UIGame.GetInstance();

        if (null != ieShowWait)
            StopCoroutine(ieShowWait);

        ieShowWait = null;

        if (currentItem == item)
            return;

        if (!uiGame.IsObjectVisible || instant)
        {
            currentItem = item;
            uiGame.ShowObject(item);
        }
        else
        {
            ieShowWait = IEShowWait(item);

            if (gameObject.activeInHierarchy)
                StartCoroutine(ieShowWait);
            else
                ieShowWait = null;
        }        
    }

    private IEnumerator IEShowWait(Collectable item)
    {
        yield return new WaitForSecondsRealtime(4f);

        currentItem = item;
        UIGame.GetInstance().ShowObject(item);
        ieShowWait = null;
    }

    private void StopShow()
    {
        if (null != ieShowWait)
            StopCoroutine(ieShowWait);

        ieShowWait = null;
        UIGame.GetInstance().HideObject();
    }

    public void View(int index)
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        ShowUIArrows(false);
        HideColorGrandingPanel();
        Player player = GameManager.GetInstance().CurrentPlayer;
        buttons[0].SetToolTip(LanguageManager.GetText(LangKey.Back));
        buttons[0].soundID = "click2";
        buttons[0].SetIcon(1);

        buttons[2].SetIcon(1);
        buttons[2].SetAction(Save);
        buttons[2].SetToolTip(LanguageManager.GetText(LangKey.Save, LanguageManager.GetText(LangKey.Image)));

        fotoCam.SetCurrentFoto(index);
        HighLight(index);
        UIFoto[] fotos = fotoCam.GetFotos();
        uISnapshot.SetImage(fotos[index]);
        
        buttons[3].gameObject.SetActive(true);
        buttons[3].SetEnabled(player.HasInventoryCapacity());
        zoomTrans.gameObject.SetActive(false);
        FocalLengthTrans.gameObject.SetActive(false);
    }

    public void SetIndex(int index)
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        fotoCam.SetFotoCount(index);

        if (fotoCam.IsEmpty(index))
            SetFotoMode();
        else
            View(index);
    }

    protected override void BeforeHide()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        Clear();
        StopShow();
        ShowUIArrows(false);
    }

    private void GetSnapshot()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        AudioManager.GetInstance().PlaySound("camera.shutter", gameObject);
        int count = fotoCam.GetFotoCount();
        UIFoto[] fotos = fotoCam.GetFotos();
        fotos[count].Shoot(cam);
        Next();
    }

    private void Next()
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        int count = fotoCam.GetFotoCount();
        count++;
        fotoCam.SetFotoCount(count);
        count = fotoCam.GetFotoCount();
        //HighLight(count);
        if (fotoCam.IsEmpty(count))
            SetFotoMode();
        else
            View(count);
    }

    private void HighLight(int index)
    {
        if (!(interactable is FotoCamera fotoCam) || null == fotoCam)
            return;

        UIFoto[] fotos = fotoCam.GetFotos();

        if (lastIndex > -1)
            fotos[lastIndex].RestoreColor();

        fotos[index].SetState(2);
        lastIndex = index;
    }

    public void PointerEnter()
    {
        UIGame.GetInstance().SetCursorEnabled(true, false);
    }

    public void PointerExit()
    {
        UIGame uiGame = UIGame.GetInstance();
        bool mode = !uiGame.IsCursorOverUI && !uiGame.IsUIExclusive;
        uiGame.SetCursorEnabled(false, mode);
    }
}