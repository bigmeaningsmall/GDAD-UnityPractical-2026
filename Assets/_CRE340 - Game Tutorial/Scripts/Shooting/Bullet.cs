using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1; // how much damage this bullet deals - used in Part 2

    // --- NEW ---
    public float lifeTime = 5f; // seconds before it gives up and goes away

    private Rigidbody rb;

    // --- NEW ---
    private BulletPool pool;    // null if we were instantiated rather than pooled
    private float timeAlive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // cache the rigidbody
    }

    // --- NEW - the pool calls this when it hands us out, so we know where to go back to ---
    public void SetPool(BulletPool owner)
    {
        pool = owner;
    }

    // --- NEW ---
    // OnEnable runs every time the object is switched on, so this is where we reset.
    // A pooled bullet arrives in whatever state the last shot left it in.
    private void OnEnable()
    {
        timeAlive = 0f;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // --- NEW - Shoot calls this once the bullet is in position ---
    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    // --- NEW - this replaces what ObjectDestruction was doing for us ---
    private void Update()
    {
        timeAlive = timeAlive + Time.deltaTime;

        if (timeAlive >= lifeTime)
        {
            Finish();
        }
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
            
            //todo sound - we can call the sound here so its the same for all or... make sound dependant on object hit?
            //todo sound - inside the IDamagable check
            AudioEvent.PlaySFX(SoundId.Slap_Heavy, transform.position);
        }

        // --- NEW - go back to the pool instead of waiting to be destroyed ---
        Finish();
    }

    // --- NEW - back to the pool if we came from one, destroyed if we didn't ---
    private void Finish()
    {
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}