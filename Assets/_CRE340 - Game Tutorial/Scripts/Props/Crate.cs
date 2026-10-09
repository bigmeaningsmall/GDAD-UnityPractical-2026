using System.Collections;
using UnityEngine;

// Note the TWO things after the colon:
// MonoBehaviour  - the class we INHERIT from (we get all the Unity behaviour)
// IDamagable     - the interface we IMPLEMENT (we promise to have its members)
public class Crate : MonoBehaviour, IDamagable
{
    public int health = 10;

    // Required by IDamagable - exposes our health field for reading.
    // Note the casing: lowercase 'health' is the actual variable, uppercase 'Health' is the public way in.
    public int Health
    {
        get { return health; }
    }

    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    // Required by IDamagable - the compiler will not let us build without it
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Now passing TWO things - the name of this object, and the health left
        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(gameObject.name, health);
        }

        if (health <= 0)
        {
            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(gameObject.name, health);
            }

            Destroy(gameObject);
        }
    }

    // Also required by IDamagable
    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.green)); // this crate flashes green
    }

    // Coroutine - same technique as the pulse effect in Week 2 Part 2
    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f); // wait without freezing the game

        mat.color = originalColor;
    }
}