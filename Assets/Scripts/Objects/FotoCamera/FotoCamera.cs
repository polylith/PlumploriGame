using System.Collections.Generic;
using UnityEngine;
using Action;

public class FotoCamera : Useable
{
    private static int maxFotos = 5;

    public UIFoto fotoPrefab;

    private bool isEnabled = true;
    private UIFoto[] fotos;
    private int fotoCount;
    private int currentFoto;
    private float zoom = 55f;
    private float focalLength = 15f;
    private float hueShift = 0f;
    private float saturation = 0f;
    private float brightness = 5f;
    private float contrast = 20f;
        
    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
                "IsEnabled"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
        SetDelegate("IsEnabled", SetEnabled);
    }

    public override void RegisterGoals()
    {

        Debug.Log("!!! TODO !!!");
    }

    public float[] GetColorGrading()
    {
        return new float[] { hueShift, saturation, brightness, contrast };
    }

    public void SetColorGrading(float[] values)
    {
        hueShift = values[0];
        saturation = values[1];
        brightness = values[2];
        contrast = values[3];
    }

    public float GetZoom()
    {
        return zoom;
    }

    public void SetZoom(float zoom)
    {
        this.zoom = zoom;
    }

    public float GetFocalLength()
    {
        return focalLength;
    }

    public void SetFocalLength(float focalLength)
    {
        this.focalLength = focalLength;
    }

    public int GetCurrentFoto()
    {
        return currentFoto;
    }

    public void SetCurrentFoto(int current)
    {
        currentFoto = current;
    }

    public int GetFotoCount()
    {
        return fotoCount;
    }

    public void SetFotoCount(int count)
    {
        fotoCount = count;
        fotoCount %= maxFotos;
    }

    public bool IsEmpty(int index)
    {
        return fotos[index].IsEmpty();
    }

    public UIFoto[] GetFotos()
    {
        if (null == fotos)
        {
            currentFoto = -1;
            fotoCount = 0;
            fotos = new UIFoto[maxFotos];

            for (int i = 0; i < fotos.Length; i++)
            {
                fotos[i] = Instantiate(fotoPrefab) as UIFoto;
                fotos[i].name = "Foto (" + i + ")";
                fotos[i].SetIndex(i);
                fotos[i].SetImage(null);
                fotos[i].gameObject.SetActive(false);
            }
        }

        return fotos;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override int IsInteractionEnabled()
    {
        if (ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
            return isEnabled ? 1 : -1;

        return base.IsInteractionEnabled();
    }

    public override int IseUseable(Interactable inter)
    {
        return IsInteractionEnabled();
    }

    public override int IsUseable(Interactable inter)
    {
        return IsInteractionEnabled();
    }

    public void SetEnabled(bool isEnabled)
    {
        if (this.isEnabled == isEnabled)
            return;

        this.isEnabled = isEnabled;
    }

    public override bool RequiresType()
    {
        return false;
    }
}