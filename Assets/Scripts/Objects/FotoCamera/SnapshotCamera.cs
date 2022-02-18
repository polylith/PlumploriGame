using UnityEngine;

public class SnapshotCamera : MonoBehaviour
{
    public Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public Texture2D GetCameraShot()
    {
        int width = Screen.width;
        int height = Screen.height;
        cam.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        cam.Render();
        RenderTexture.active = cam.targetTexture;
        RenderTexture texture = cam.targetTexture;
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Texture2D image = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false)
        {
            name = "IMG" + System.DateTime.Now.ToString("yyyyMMddHHmmss"),
            filterMode = FilterMode.Bilinear
        };        
        image.ReadPixels(rect, 0, 0);
        image.Apply();
        return image;
    }

    public void GetSnapshot(UIFoto uiFoto)
    {
        if (null == uiFoto)
            return;

        uiFoto.SetImage(GetCameraShot());
    }
}