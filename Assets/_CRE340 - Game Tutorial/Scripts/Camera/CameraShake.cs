using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3.x - use 'using Cinemachine;' on 2.x

// Listens for gameplay events and shakes the camera.
// Nothing in the game knows this exists.
public class CameraShake : MonoBehaviour
{
    [Header("Impulse")]
    public CinemachineImpulseSource impulseSource;

    [Header("Strength")]
    public float fireShake = 0.1f;
    public float killShake = 0.5f;

    private void Awake()
    {
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
    }

    private void OnEnable()
    {
        EventBus<WeaponFired>.Subscribe(HandleWeaponFired);
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<WeaponFired>.Unsubscribe(HandleWeaponFired);
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }

    // private void HandleWeaponFired(WeaponFired message)
    // {
    //     Shake(message.position, fireShake);
    // }
    private void HandleWeaponFired(WeaponFired message)
    {
        if (impulseSource == null)
        {
            return;
        }

        impulseSource.transform.position = message.position;

        // Push the camera back against the direction we fired
        Vector3 kick = -message.direction * fireShake;

        impulseSource.GenerateImpulse(kick);
    }

    private void HandleEnemyKilled(EnemyKilled message)
    {
        Shake(message.position, killShake);
    }

    private void Shake(Vector3 position, float strength)
    {
        if (impulseSource == null)
        {
            return;
        }

        // Move the source to where it happened, so things further away shake less
        impulseSource.transform.position = position;

        impulseSource.GenerateImpulse(strength);
    }
}