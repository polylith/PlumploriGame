using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class UIFoto : MonoBehaviour
{
    private static Color[] colors = new Color[] { Color.gray, Color.white, Color.yellow, Color.yellow };

    public Image image;
    public TextMeshProUGUI textMesh;
    public Sprite defaultSprite;
    public UIIconButton deleteButton;
    public UIIconButton viewButton;

    private Texture2D texture;
    private int index;
    private string filename;
    private int state = -1;

    private void Awake()
    {
        SetImage(null);
        SetState(0);
        deleteButton.SetAction(Delete);
        viewButton.SetAction(View);
    }

    private void InitButtons()
    {
        string text = Language.LanguageManager.GetText(Language.LangKey.Image, (index + 1).ToString());
        deleteButton.SetToolTip(Language.LanguageManager.GetText(Language.LangKey.Delete, text));
        viewButton.SetToolTip(Language.LanguageManager.GetText(Language.LangKey.Show, text));
    }

    public void PointerEnter()
    {
        if (state == 2)
            return;

        SetState(3);
        UIGame.GetInstance().SetCursorEnabled(true, false);
    }

    public void PointerExit()
    {
        if (state == 2)
            return;

        UIGame uiGame = UIGame.GetInstance();
        bool mode = !uiGame.IsCursorOverUI && !uiGame.IsUIExclusive;
        RestoreColor();
        uiGame.SetCursorEnabled(false, mode);
    }

    public void SetFoto()
    {
        FotoCameraUI.GetInstance().SetIndex(index);
    }

    private void View()
    {
        FotoCameraUI.GetInstance().View(index);
    }

    public void SetIndex(int index)
    {
        this.index = index;
        SetText((index + 1).ToString());
    }

    public bool IsEmpty()
    {
        return null == texture;
    }

    public Texture2D GetTexture()
    {
        return texture;
    }

    public UIMessage Save()
    {
        if (null == texture)
            return null;
        
        try
        {
            byte[] byteArray = texture.EncodeToJPG(100);
            string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            string path = Path.Combine(folderPath, Application.productName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, filename);
            File.WriteAllBytes(path, byteArray);

            Debug.Log("Saved in " + path);

            return new UIMessage(Language.LanguageManager.GetText(Language.LangKey.Saved,
                Language.LanguageManager.GetText(Language.LangKey.Image)));
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);

            return new UIMessage(Language.LanguageManager.GetText(Language.LangKey.Error, 
                Language.LanguageManager.GetText(Language.LangKey.NotSaved, 
                Language.LanguageManager.GetText(Language.LangKey.Image))), true);
        }
    }

    public void SetImage(Texture2D texture)
    {
        bool isImage = null != texture;
        this.texture = texture;
        
        if (isImage)
        {
            filename = texture.name + ".jpg";
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            image.sprite = Sprite.Create(texture, rect, Vector2.zero);
        }
        else
            image.sprite = defaultSprite;

        if (isImage)
            InitButtons();

        deleteButton.gameObject.SetActive(isImage);
        viewButton.gameObject.SetActive(isImage);
        RestoreColor();
    }

    public void Delete()
    {
        SetImage(null);
        RestoreColor();
        FotoCameraUI.GetInstance().SetIndex(index);
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }

    public void RestoreColor()
    {
        bool isImage = !IsEmpty();
        Color color = isImage ? Color.white : new Color(0.75f, 0.75f, 0.75f, 1f);
        int state = isImage ? 1 : 0;
        image.color = color;
        SetState(state);
    }

    public void SetState(int state)
    {
        this.state = state;
        textMesh.color = colors[state];
    }

    public void Shoot(SnapshotCamera cam)
    {
        cam.GetSnapshot(this);
    }    
}