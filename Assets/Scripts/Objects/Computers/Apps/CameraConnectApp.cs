using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Language;
using UnityEngine.UI;

public class CameraConnectApp : PCApp
{
    public RectTransform photoGrid;
    public UIIconButton searchButton;
    public TextMeshProUGUI textDisplay;
    public Image progressBar;
    public UITextButton buttonPrefab;
    public Image uiImagePrefab;

    private FotoCamera fotoCamera;
    private readonly List<FotoCamera> cameraList = new List<FotoCamera>();
    private IEnumerator ieSearch;

    public override void StoreCurrentState(EntityData entityData)
    {
        // TODO
        base.StoreCurrentState(entityData);
    }

    public override void RestoreCurrentState(EntityData entityData)
    {
        // TODO
        base.RestoreCurrentState(entityData);
    }

    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        return null;
    }

    public override List<Formula> GetGoals()
    {
        return null;
    }

    protected override void Effect()
    {
        // TODO
    }

    protected override void Init()
    {
        ClearPhotoGrid();
        textDisplay.SetText("");
        searchButton.SetAction(SearchFotoCameras);
        searchButton.IsEnabled = true;
        base.Init();
    }

    public override void ResetApp()
    {
        ClearSearch();
        ClearPhotoGrid();
        base.ResetApp();
    }

    protected override void PreCall()
    {
    }

    private void ClearPhotoGrid()
    {
        foreach (Transform trans in photoGrid)
        {
            Destroy(trans.gameObject);
        }
    }

    private void ClearSearch()
    {
        textDisplay.SetText("");
        progressBar.fillAmount = 0f;
        computer.StopPCNoise();
        searchButton.IsEnabled = true;

        if (null != ieSearch)
            StopCoroutine(ieSearch);

        ieSearch = null;
    }

    private void ShowPhotos()
    {
        ClearPhotoGrid();
        textDisplay.SetText("");

        UIFoto[] uiFotos = fotoCamera.GetFotos();
        int fotoCount = 0;

        for (int i = 0; i < uiFotos.Length; i++)
        {
            if (!uiFotos[i].IsEmpty())
            {
                Image image = Instantiate(uiImagePrefab);
                Texture2D texture = uiFotos[i].GetTexture();
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                image.sprite = Sprite.Create(texture, rect, Vector2.zero);
                image.rectTransform.SetParent(photoGrid, false);
                fotoCount++;
            }
        }

        if (fotoCount == 0)
        {
            textDisplay.SetText(
                LanguageManager.GetText(
                    LangKey.NotAvailable,
                    LanguageManager.GetText(LangKey.Image)
                )
            );
            return;
        }
    }

    private void SearchFotoCameras()
    {
        ClearPhotoGrid();
        searchButton.IsEnabled = false;
        GameManager gameManager = GameManager.GetInstance();

        Room room = gameManager.CurrentRoom;
        FotoCamera[] fotoCameras = room.transform.GetComponentsInChildren<FotoCamera>();
        // TODO also search for cameras in openables
        Player player = gameManager.CurrentPlayer;
        List<Collectable> collectedItems = player.GetInventoryItems();
        cameraList.Clear();

        if (fotoCameras.Length > 0)
        {
            cameraList.AddRange(fotoCameras);
        }

        if (null != collectedItems)
        {
            foreach (Collectable collectable in collectedItems)
            {
                if (collectable is FotoCamera fotoCamera)
                {
                    cameraList.Add(fotoCamera);
                }
            }
        }

        ClearSearch();
        ieSearch = IESearch();
        StartCoroutine(ieSearch);
    }

    private IEnumerator IESearch()
    {
        computer.PCNoise();
        progressBar.fillAmount = 0f;

        yield return new WaitForSecondsRealtime(0.5f);

        textDisplay.SetText(
            LanguageManager.GetText(
                LangKey.Scanning,
                AppName
            )
        );

        float duration = Math.Max(3f, Math.Min(5f, cameraList.Count * 2f));
        float f = 0f;

        while (duration > 0f)
        {
            progressBar.fillAmount = f;
            f += 0.1f;

            if (f > 1f)
                f = 0f;

            duration -= 0.1f;

            yield return new WaitForSecondsRealtime(0.1f);
        }

        textDisplay.SetText("");

        yield return null;

        ieSearch = null;
        ClearSearch();
        ShowFotoCameras();
    }

    private void ShowFotoCameras()
    {
        textDisplay.SetText("");
        ClearPhotoGrid();

        if (cameraList.Count == 0)
        {
            textDisplay.SetText(
                LanguageManager.GetText(
                    LangKey.NotAvailable,
                    LanguageManager.GetText(
                        LangKey.Camera
                    )
                )
            );
            return;
        }

        int i = 0;

        foreach (FotoCamera fotoCamera in cameraList)
        {
            UITextButton button = Instantiate(buttonPrefab);
            button.transform.name = "  Camera (" + i + ")  ";
            button.SetText(
                LanguageManager.GetText(LangKey.Camera)
                + " " + (i + 1).ToString()
            );
            button.transform.SetParent(photoGrid, false);
            button.SetAction(() => {
                SetCurrentFotoCamera(fotoCamera);
            });
            i++;
        }
    }

    private void SetCurrentFotoCamera(FotoCamera fotoCamera)
    {
        this.fotoCamera = fotoCamera;
        ShowPhotos();
    }
}
