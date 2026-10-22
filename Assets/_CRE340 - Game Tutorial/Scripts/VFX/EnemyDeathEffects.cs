using UnityEngine;

// Spawns an effect wherever an enemy died.
// No reference to Enemy, and Enemy has no reference to this.
public class EnemyDeathEffects : MonoBehaviour
{
    public GameObject deathEffectPrefab;   // a particle system, or anything you like

    private void OnEnable()
    {
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }

    private void HandleEnemyKilled(EnemyKilled message)
    {
        if (deathEffectPrefab == null)
        {
            return;
        }

        // The message told us where it happened
        Instantiate(deathEffectPrefab, message.position, Quaternion.identity);
    }
}