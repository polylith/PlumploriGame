using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script allows to take screenshots as a Texture2D
/// and transfer existing images from a UIFoto to display.
/// </summary>
public class UISnapshot : MonoBehaviour
{
    private RawImage rawimage;
    
    private void Awake()
    {
        rawimage = GetComponent<RawImage>();
    }

    public void SetImage(UIFoto foto)
    {
        rawimage.texture = foto.GetTexture();
    }

    public void Activate(Camera cam, bool reset)
    {
        if (reset)
        {
            Player currentPlayer = GameManager.GetInstance().CurrentPlayer;
            cam.transform.SetParent(currentPlayer.fotoCameraPosition, false);
            // cam.transform.SetParent(Camera.main.transform, false);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cam.transform.gameObject.layer = Camera.main.gameObject.layer;
        }

        Rect rect = rawimage.rectTransform.rect;
        RenderTexture renderTexture = new RenderTexture((int)rect.width, (int)rect.height, 16);
        cam.targetTexture = renderTexture;
        rawimage.texture = renderTexture;
    }
}