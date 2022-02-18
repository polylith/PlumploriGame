using UnityEngine;

public class Picture : Collectable
{
    public Texture2D texture;
    private Color color;
    private Material[] materials;

    private void Awake()
    {
        langKey = Language.LangKey.Image.ToString();
        Renderer renderer = GetComponentInChildren<Renderer>();
        materials = renderer.materials;
        color = materials[0].color;

        if (null != texture)
            SetTexture(texture);
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public void SetColor(Color color)
    {
        this.color = color;
        materials[0].color = color;
    }

    private void SetTexture(Texture2D texture)
    {
        this.texture = texture;
        materials[1].SetTexture("_MainTex", texture);
        materials[1].SetTexture("_EmissionMap", texture);
        SetObjectIcon(texture);
        Vector3 scale = transform.localScale;

        if (texture.width == texture.height)
            return;

        float f = (float)texture.width / (float)texture.height;

        if (texture.width < texture.height)
        {
            f = (float)texture.height / (float)texture.width;
            scale.z *= f;
            scale.x = 1f;
        }
        else
        {
            scale.x *= f;
            scale.z = 1f;
        }

        transform.localScale = scale;
    }

    public Picture Instantiate(Texture2D texture)
    {
        Picture pic = Instantiate(this) as Picture;
        pic.SetTexture(texture);
        pic.SetPrefix(texture.name);
        pic.Register();
        return pic;
    }
}