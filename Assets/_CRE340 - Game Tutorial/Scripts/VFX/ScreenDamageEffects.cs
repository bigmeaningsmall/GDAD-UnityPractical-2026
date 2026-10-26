using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;   // Vignette, ChromaticAberration, ColorAdjustments
using PrimeTween;

// Flashes the screen when the player takes damage.
// Listens to the UI health event - nothing in the game knows this exists.
public class ScreenDamageEffects : MonoBehaviour
{
    public Volume volume;

    [Header("Damage Flash")]
    public float flashStrength = 0.45f;
    public float flashUp = 0.05f;      // fast in
    public float flashDown = 0.35f;    // slower out

    private Vignette vignette;
    private int lastHealth = -1;
    
    [Header("Aberration")]
    public float aberrationStrength = 0.6f;

    private ChromaticAberration aberration;
    
    [Header("Game Over")]
    public float desaturateTime = 1.2f;

    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (volume != null)
        {
            // Dig the override out of the profile once and keep it
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out aberration);
            volume.profile.TryGet(out colorAdjustments);
        }
    }

    private void OnEnable()
    {
        UIEventManager.OnHealthChanged += HandleHealthChanged;
        UIEventManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        UIEventManager.OnHealthChanged -= HandleHealthChanged;
        UIEventManager.OnGameOver -= HandleGameOver;
    }

    private void HandleHealthChanged(int current, int max)
    {
        // The event fires for damage AND healing. Only flash when it went down.
        if (lastHealth >= 0 && current < lastHealth)
        {
            FlashDamage();
        }

        lastHealth = current;
    }
    
    private void HandleGameOver()
    {
        if (colorAdjustments == null)
        {
            return;
        }

        // Drain the colour out of the scene. -100 is fully grey.
        Tween.Custom(0f, -100f, duration: desaturateTime,
            onValueChange: value => colorAdjustments.saturation.value = value);
    }
    
    private void FlashDamage()
    {
        if (vignette == null)
        {
            return;
        }

        // Snap up fast, fade out slower - that shape is what reads as a hit
        Sequence.Create()
            .Chain(Tween.Custom(0f, flashStrength, duration: flashUp,
                onValueChange: value => vignette.intensity.value = value))
            .Chain(Tween.Custom(flashStrength, 0f, duration: flashDown,
                onValueChange: value => vignette.intensity.value = value));
        
        if (aberration != null)
        {
            Sequence.Create()
                .Chain(Tween.Custom(0f, aberrationStrength, duration: flashUp,
                    onValueChange: value => aberration.intensity.value = value))
                .Chain(Tween.Custom(aberrationStrength, 0f, duration: flashDown * 1.5f,
                    onValueChange: value => aberration.intensity.value = value));
        }
    }
}