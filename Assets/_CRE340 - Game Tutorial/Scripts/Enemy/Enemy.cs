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
        // An enemy placed in the scene by hand already has its data, so set up now. means we can have enemies in the scene already or instantiate them
        // Enemies made by the factory arrive and get Initialise called instead.
        if (enemyData != null)
        {
            Initialise(enemyData);
        }
    }

    // Everything that used to be in Awake. The factory calls this after creating us.
    public void Initialise(EnemyData data)
    {
        enemyData = data;

        health = enemyData.startingHealth;
        gameObject.name = enemyData.enemyName;

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
        
        // todo - call sound for enemy spawn
    }

    private void Update()
    {
        // Ask the GameManager where the player is.
        // There might not be one - not spawned yet, or dead - so check before using it.
        if (GameManager.Instance.Player == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            GameManager.Instance.Player.transform.position,
            enemyData.moveSpeed * Time.deltaTime
        );
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
        
        //todo call sound here or earlier? - sound will trigger if hit but object destroyed if dead
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

    // private void Die()
    // {
    //     // TODO - WE ADDED A SINGLETON MANAGER SO NOW WE CABN CALL IT EASILY FROM HERE
    //     GameManager.Instance.AddXP(enemyData.xpValue);
    //     
    //     Destroy(gameObject);
    // }
    
    private void Die()
    {
        // Describe what happened
        EnemyKilled message = new EnemyKilled();
        message.enemyName = enemyData.enemyName;
        message.xpValue = enemyData.xpValue;
        message.position = transform.position;

        // Announce it. We have no idea whats listening. 
        EventBus<EnemyKilled>.Publish(message);
        
        //todo call the death sound here

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