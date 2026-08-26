### Interface Implementation with Event Manager: Quick Integration Guide

This guide builds on the previous `IDamagable` interface example by integrating a simple `HealthEventManager` to broadcast events when an object is damaged or destroyed. This will demonstrate how to use delegates and events to manage game object interactions.

### Step 1: Set Up the `HealthEventManager`

1.  **Create a new C# script** called `HealthEventManager.cs`.
2.  Add the following code to define the delegate and events:

``` csharp
// HealthEventManager.cs
public static class HealthEventManager
{
    // Define a delegate that handles health-related events
    public delegate void HealthEvent(int currentHealth);

    // Called when any object implementing IDamagable takes damage
    public static HealthEvent OnObjectDamaged;

    // Called when any object implementing IDamagable is destroyed
    public static HealthEvent OnObjectDestroyed;
}
```

### Step 2: Modify `Crate`, `ExplodingCrate`, and `Enemy` Scripts

Update the `TakeDamage` method in each of these scripts to invoke the relevant events from `HealthEventManager`.

#### Crate Script Modification

Modify `Crate.cs` to include the `HealthEventManager` events:

``` csharp
// Crate.cs
using UnityEngine;

public class Crate : MonoBehaviour, IDamagable
{
    public int health = 10;
    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        // Trigger the OnObjectDamaged event
        HealthEventManager.OnObjectDamaged?.Invoke(health);

        if (health <= 0)
        {
            // Trigger the OnObjectDestroyed event
            HealthEventManager.OnObjectDestroyed?.Invoke(health);
            Destroy(gameObject);
        }
    }

    public void ShowHitEffect()
    {
        mat.color = Color.green; // Flash green to show a hit effect
        Invoke("ResetMaterial", 0.1f);
    }

    private void ResetMaterial()
    {
        mat.color = originalColor;
    }
}
```

#### ExplodingCrate Script Modification

Modify `ExplodingCrate.cs` to include the `HealthEventManager` events:

``` csharp
// ExplodingCrate.cs
using UnityEngine;

public class ExplodingCrate : MonoBehaviour, IDamagable
{
    public int health = 10;
    public GameObject explosionEffectPrefab;
    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        // Trigger the OnObjectDamaged event
        HealthEventManager.OnObjectDamaged?.Invoke(health);

        ShowHitEffect();

        if (health <= 0)
        {
            Explode();

            // Trigger the OnObjectDestroyed event
            HealthEventManager.OnObjectDestroyed?.Invoke(health);
            Destroy(gameObject);
        }
    }

    public void ShowHitEffect()
    {
        mat.color = Color.red; // Flash red to show a hit effect
        Invoke("ResetMaterial", 0.1f);
    }

    private void ResetMaterial()
    {
        mat.color = originalColor;
    }

    private void Explode()
    {
        // Instantiate explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
```

#### Enemy Script Modification

Modify `Enemy.cs` to include the `HealthEventManager` events:

``` csharp
// Enemy.cs
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    public int health = 10;
    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        // Trigger the OnObjectDamaged event
        HealthEventManager.OnObjectDamaged?.Invoke(health);

        ShowHitEffect();

        if (health <= 0)
        {
            Die();

            // Trigger the OnObjectDestroyed event
            HealthEventManager.OnObjectDestroyed?.Invoke(health);
        }
    }

    public void ShowHitEffect()
    {
        mat.color = Color.red; // Flash red to show a hit effect
        Invoke("ResetMaterial", 0.1f);
    }

    private void ResetMaterial()
    {
        mat.color = originalColor;
    }

    private void Die()
    {
        // Optional: add logic like playing an animation or dropping loot
        Destroy(gameObject); // Destroy enemy object
    }
}
```

### Step 3: Create an Event Listener to Handle Events

1.  **Create a new C# script** called `EventListener.cs`.
2.  Add the following code to listen to and respond to events:

``` csharp
// EventListener.cs
using UnityEngine;

public class EventListener : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to events
        HealthEventManager.OnObjectDamaged += HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed += HandleObjectDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from events to avoid memory leaks
        HealthEventManager.OnObjectDamaged -= HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed -= HandleObjectDestroyed;
    }

    private void HandleObjectDamaged(int remainingHealth)
    {
        Debug.Log($"An object was damaged! Remaining Health: {remainingHealth}");
    }

    private void HandleObjectDestroyed(int remainingHealth)
    {
        Debug.Log("An object was destroyed!");
    }
}
```

### Step 4: Attach the `EventListener` Script

- Attach the `EventListener` script to any GameObject in your scene to observe the events when the `Crate`, `ExplodingCrate`, or `Enemy` prefabs are interacted with.

### Step 5: Test the Scene

- Run the scene and fire bullets at the objects.
- Observe the console for event messages when the objects take damage or are destroyed.

### Summary

- The `HealthEventManager` uses delegates to define two events: `OnObjectDamaged` and `OnObjectDestroyed`.
- You modified `Crate`, `ExplodingCrate`, and `Enemy` to call these events.
- `EventListener` listens to these events and performs actions (e.g., logging messages).

This integration demonstrates how to use a simple event manager to communicate between different objects in Unity.
