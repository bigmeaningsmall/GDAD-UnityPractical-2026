using System.Collections;
using UnityEngine;

//TODO - We may refactor this class eventually to use a base class and inheritance - for now its just a monobehaviour with an interface
public class Enemy : MonoBehaviour, IDamagable
{
    public int health = 10;

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
            Die();
        }
    }

    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.red));
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Die()
    {
        // TODO - later weeks: drop loot, award XP, play an animation
        Destroy(gameObject);
    }
}