using UnityEngine;
using System;

/// <summary>
/// This class is used to produce random noise and other
/// audio data dynamically. The volume must be damped down,
/// because otherwise the output is much too loud.
/// </summary>
public class ProceduralAudio : MonoBehaviour
{
    public bool NoiseRatio { get => noiseRatio == 1f; set => SetNoiseRatio(value); }
    public double frequency = 440;

    [Range(0f,1f)]
    public double gain = 0.05;

    [Range(0f,1f)]
    public float offset = 0f;

    [Range(0f,1f)]
    public float noiseRatio = 0.5f;

    private System.Random RandomNumber = new System.Random();
    private double increment;
    private double phase;
    private double sampling_frequency = 48000;

    private void SetNoiseRatio(bool mode)
    {
        noiseRatio = (mode ? UnityEngine.Random.value : 1f);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        increment = frequency * 2 * Math.PI / sampling_frequency;

        for (var i = 0; i < data.Length; i = i + channels)
        {
            phase = phase + increment;
            float tonalPart = (float)(gain * Math.Sin(phase));
            float noisePart = offset - 1.0f + (float)RandomNumber.NextDouble() * 2.0f;
            data[i] = noiseRatio * noisePart + (1f - noiseRatio) * tonalPart;

            if (channels == 2)
            {
                data[i + 1] = data[i];
                i++;
            }

            if (phase > 2 * Math.PI) phase = 0;
        }
    }
}