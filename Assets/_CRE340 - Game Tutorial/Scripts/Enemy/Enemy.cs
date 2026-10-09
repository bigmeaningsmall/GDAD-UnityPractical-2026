using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Enemy Data")]
    // The AUTHORED data - shared by every enemy using this asset.
    // Drag an EnemyData asset onto this in the Inspector.
    public EnemyData enemyData;

    // RUNTIME state - this enemy's own health, copied from the data when it spawns.
    // It is NOT public, because nothing should be setting it except TakeDamage.
    private int health;
    
    [Header("Contact Damage")]
    public float damageCooldown = 1f;      // seconds between hits while touching
    private float lastDamageTime = -100f;  // start in the past so the first hit lands

    // Required by IDamagable
    public int Health
    {
        get { return health; }
    }

    private Material mat;
    private Color originalColor;
    private Transform playerTransform;

    private void Awake()
    {
        // Safety check - without data we can't set anything up
        if (enemyData == null)
        {
            Debug.LogError("Enemy on " + gameObject.name + " has no EnemyData assigned!");
            return;
        }

        // COPY the authored values into our own runtime state.
        // From here on we change our 'health', never 'enemyData.startingHealth'.
        health = enemyData.startingHealth;

        // Use the name from the data - this also tidies up the '(Clone)' we saw in Part 1
        gameObject.name = enemyData.enemyName;

        // Apply the colour from the data
        mat = GetComponent<Renderer>().material;
        mat.color = enemyData.enemyColor;
        originalColor = mat.color;
    }

    private void Start()
    {
        // Find the player once at the start rather than every frame.
        // We search for the COMPONENT - if something has PlayerStats, it's the player.
        PlayerStats player = FindFirstObjectByType<PlayerStats>();

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        // Very simple chase - move straight towards the player at our data's speed
        if (playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                enemyData.moveSpeed * Time.deltaTime
            );
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

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

            Die();
        }
    }

    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.white));
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Die()
    {
        // TODO (Week 5) - award enemyData.scoreValue to the GameManager that we haven't built yet
        Destroy(gameObject);
    }
    
    ///---------- Dealing Damage - ------------------
    ///     // OnCollisionStay fires every frame while two objects are touching
    private void OnCollisionStay(Collision collision)
    {
        // Cooldown first - it's the cheapest check and it fails most of the time
        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        // Is this the player? We ask for the COMPONENT rather than checking a tag.
        // If it has PlayerStats then it's the player - nothing else in the game has one.
        if (collision.gameObject.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(enemyData.contactDamage);
            playerStats.ShowHitEffect();

            lastDamageTime = Time.time;
        }
    }
}