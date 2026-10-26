using UnityEngine;
using Unity.Cinemachine;
using PrimeTween;

// Builds camera noise as the player's health drops.
// This is a STATE, not an event - it stays on until health changes again.
public class LowHealthCamera : MonoBehaviour
{
    public CinemachineBasicMultiChannelPerlin noise;

    [Header("Tuning")]
    public float threshold = 0.3f;      // below this fraction we start shaking
    public float maxAmplitude = 0.5f;   // how strong it gets at zero health
    public float blendTime = 0.4f;      // how long it takes to reach the new level

    private void Awake()
    {
        if (noise == null)
        {
            noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void OnEnable()
    {
        UIEventManager.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        UIEventManager.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (noise == null || max <= 0)
        {
            return;
        }

        float fraction = (float)current / (float)max;
        float target = 0f;

        if (fraction < threshold)
        {
            // The further below the threshold we are, the stronger it gets
            float amount = 1f - (fraction / threshold);
            target = maxAmplitude * amount;
        }

        // Tween to the new level rather than snapping to it
        Tween.Custom(noise.AmplitudeGain, target, duration: blendTime,
            onValueChange: value => noise.AmplitudeGain = value);
    }
}