using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <para>
/// This script can be used to take a snapshot including
/// the UI and apply it to a given image. Its a singleton.
/// </para>
/// </summary>
public class CaptureUI : MonoBehaviour
{
    private static CaptureUI ins;

    public static CaptureUI GetInstance()
    {
        return ins;
    }

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UIToImage(Image image)
    {
        StartCoroutine(TakeScreenShot(image));
    }

    private static IEnumerator TakeScreenShot(Image image)
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0.0f, 0.0f, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100.0f
        );
        image.sprite = sprite;
        image.color = Color.white;
    }
}
