using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Moves the mixer between snapshots based on what is happening to the player.
// Subscribes to the bus - nothing in the game knows this exists.
public class DynamicAudioEffects : MonoBehaviour
{
    [Header("Snapshots")]
    public AudioMixerSnapshot normal;
    public AudioMixerSnapshot hit;
    public AudioMixerSnapshot lowHealth;

    [Header("Hit Duck")]
    public float duckInTime = 0.05f;      // fast on the way in so it reads as an impact
    public float duckOutTime = 0.6f;      // slower on the way out

    [Header("Low Health")]
    [Range(0f, 1f)]
    public float lowHealthFraction = 0.3f;   // below this and the filter stays on
    public float stateChangeTime = 1f;       // how long low health takes to come on or off

    private bool isLowHealth;

    private void OnEnable()
    {
        EventBus<PlayerHealthChanged>.Subscribe(HandleHealthChanged);
    }

    private void OnDisable()
    {
        EventBus<PlayerHealthChanged>.Unsubscribe(HandleHealthChanged);
    }

    private void Start()
    {
        normal.TransitionTo(0f);
    }

    private void HandleHealthChanged(PlayerHealthChanged message)
    {
        // Guard against a divide by zero if max health hasn't been set up
        if (message.maxHealth <= 0)
        {
            return;
        }

        float fraction = (float)message.currentHealth / message.maxHealth;

        isLowHealth = fraction <= lowHealthFraction && message.currentHealth > 0;

        if (message.tookDamage)
        {
            // Stop any duck already running so a second hit restarts it
            StopAllCoroutines();
            StartCoroutine(HitDuck());
        }
        else
        {
            // Healed, so just move to whichever state we should be resting in
            GoToRestingSnapshot(stateChangeTime);
        }
    }

    // Drop into the hit snapshot, then come back to wherever we should be
    private IEnumerator HitDuck()
    {
        hit.TransitionTo(duckInTime);

        yield return new WaitForSeconds(duckInTime);

        GoToRestingSnapshot(duckOutTime);
    }

    // Low health and normal are the two states we sit in. The hit is temporary.
    private void GoToRestingSnapshot(float time)
    {
        if (isLowHealth)
        {
            lowHealth.TransitionTo(time);
        }
        else
        {
            normal.TransitionTo(time);
        }
    }
}