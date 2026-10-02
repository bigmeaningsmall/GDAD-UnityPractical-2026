using System.Collections;
using UnityEngine;

public class ExplodingCrate : MonoBehaviour, IDamagable
{
    public int health = 10;
    public GameObject explosionEffectPrefab; // optional - drag a particle effect here

    // Same property, same job - this class stores its health exactly like the Crate does
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        
        // Broadcast that something was damaged - we have no idea who (if anyone) is listening.
        // We MUST check for null first - see the explanation below.
        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(health);
        }

        if (health <= 0)
        {
            Explode();              // the thing that makes this crate different
            Destroy(gameObject);
        }
    }

    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.red)); // this one flashes red - we could also make this an exposed variable and set it in the editor to any colour
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Explode()
    {
        // Spawn an explosion effect if one has been assigned
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}