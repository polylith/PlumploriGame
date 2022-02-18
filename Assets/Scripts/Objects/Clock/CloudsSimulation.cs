using System.Collections;
using UnityEngine;

public class CloudsSimulation : MonoBehaviour
{
    private readonly Material[] mats = new Material[2];
    private readonly Color[] colors = new Color[2] { new Color(1f, 1f, 1f, 0.86f), new Color(1f, 1f, 1f, 0.19f) };
    private readonly float[] density = new float[2] { 1.8f, 1.4f };
    private readonly float[] speed = new float[2] { 1.5f, -0.25f };

    private void Awake()
    {
        Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
        mats[0] = renderer.material;
        renderer = transform.GetChild(1).GetComponent<Renderer>();
        mats[1] = renderer.material;
    }

    public CloudSettings[] GetSettings()
    {
        return new CloudSettings[2] 
        {
            new CloudSettings()
            {
                color = colors[0],
                density = density[0],
                speed = speed[0]
            },
            new CloudSettings()
            {
                color = colors[1],
                density = density[1],
                speed = speed[1]
            }
        };
    }

    public void ChangeClouds(CloudSettings settings, float changeTime = 0f)
    {
        if (changeTime > 0)
        {
            StartCoroutine(IEChange(settings, changeTime));
        }
        else
        {
            int i = settings.i;
            colors[i] = settings.color;
            density[i] = settings.density;
            speed[i] = settings.speed;

            mats[i].SetColor("_CloudColor", colors[i]);
            mats[i].SetFloat("_Density", density[i]);
            mats[i].SetFloat("_Speed", speed[i]);
        }
    }

    private IEnumerator IEChange(CloudSettings settings, float changeTime)
    {
        float df = changeTime / Time.deltaTime;
        float f = 0f;
        int i = settings.i;

        while (f <= 1f)
        {
            colors[i] = Color.Lerp(colors[i],settings.color, f);
            density[i] = Mathf.Lerp(density[i], settings.density, f);
            speed[i] = Mathf.Lerp(speed[i], settings.speed, f);

            mats[i].SetColor("_CloudColor", colors[i]);
            mats[i].SetFloat("_Density", density[i]);
            mats[i].SetFloat("_Speed", speed[i]);

            yield return new WaitForSeconds(Time.deltaTime);

            f += df;
        }

        colors[i] = settings.color;
        density[i] = settings.density;
        speed[i] = settings.speed;

        mats[i].SetColor("_CloudColor", colors[i]);
        mats[i].SetFloat("_Density", density[i]);
        mats[i].SetFloat("_Speed", speed[i]);
    }
}