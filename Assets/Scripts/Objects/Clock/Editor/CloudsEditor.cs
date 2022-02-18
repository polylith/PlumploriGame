using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CloudsSimulation))]
public class CloudsEditor : Editor
{
    private CloudsSimulation cloudSim;
    private CloudSettings[] settings;
    private float[] changeTime;
    private readonly bool[] showSettings = new bool[2] { true, true };

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        cloudSim = FindObjectOfType<CloudsSimulation>();
        settings = cloudSim.GetSettings();
        changeTime = new float[2] { 0f, 0f };
    }

    private void ApplySettings(int i)
    {
        cloudSim.ChangeClouds(settings[i], changeTime[i]);
    }

    private void ShowSettings(int i)
    {
        settings[i].color = EditorGUILayout.ColorField("Color", settings[i].color);
        settings[i].density = EditorGUILayout.FloatField("Density", settings[i].density);
        settings[i].speed = EditorGUILayout.FloatField("Speed", settings[i].speed);
        changeTime[i] = EditorGUILayout.FloatField("Duration", changeTime[i]);

        if (EditorApplication.isPlaying)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply", GUILayout.Width(100), GUILayout.Height(35)))
                ApplySettings(i);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    public override void OnInspectorGUI()
    {
        int i = 0;
        showSettings[i] = EditorGUILayout.Foldout(showSettings[i], "Cloud Settings (" + i + ")");

        if (showSettings[i])
            ShowSettings(i);

        EditorGUILayout.Space();

        i++;
        showSettings[i] = EditorGUILayout.Foldout(showSettings[i], "Cloud Settings (" + i + ")");

        if (showSettings[i])
            ShowSettings(i);

        EditorGUILayout.Space();
    }
}