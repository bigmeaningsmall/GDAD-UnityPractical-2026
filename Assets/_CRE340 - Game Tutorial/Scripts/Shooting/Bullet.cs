using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1; // how much damage this bullet deals - used in Part 2

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // cache the rigidbody
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop the bullet dead and let it drop - a cheap 'impact' effect
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;
        
        // Ask the thing we hit: do you implement IDamagable?
        // TryGetComponent gives us a bool AND the component in one call
        if (collision.gameObject.TryGetComponent<IDamagable>(out IDamagable damageable))
        {
            // We have NO IDEA what we just hit. Crate? Enemy? Something we'll add in the future?
            // Doesn't matter - it promised it has these members, so we can use them.
            damageable.TakeDamage(damage);
            damageable.ShowHitEffect();

            // The Health property lets us ASK the target a question without knowing what it is 
            Debug.Log("Hit something - " + damageable.Health + " health remaining");
        }
    }
}