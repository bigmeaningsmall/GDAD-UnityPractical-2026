using System.Collections;
using UnityEngine;

// This IS a MonoBehaviour, because Unity needs to access it
// it sits on the Player GameObject so that items can find it on collision.
// It is a SEPARATE component from the TwinStickController. - following good practice to sperate classes and components (one handles movement, one handes stats - each script has oine responsibility - See SOLID)
// Now implements IDamagable - the same interface the crates and enemies use
public class PlayerStats : MonoBehaviour, IDamagable
{
    // An INSTANCE of our plain PlayerData class - runtime state, this player's own numbers
    [SerializeField] private PlayerData data = new PlayerData();

    // A public way for other scripts to read the data (but not replace it)
    public PlayerData Data
    {
        get { return data; }
    }

    // Required by IDamagable - our health lives inside PlayerData
    public int Health
    {
        get { return data.currentHealth; }
    }
    
    [SerializeField]
    private Material mat;
    private Color originalColor;

    private void Awake()
    {
        // Tell the GameManager we exist, so anything that needs the player can find us through it
        GameManager.Instance.RegisterPlayer(this);
    }
    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;

        // A new player has appeared - tell the UI what our stats are
        UIEventManager.HealthChanged(data.currentHealth, data.maxHealth);
        UIEventManager.ManaChanged(data.currentMana, data.maxMana);
    }

    // Add health, but never go above maxHealth
    public void RestoreHealth(int amount)
    {
        data.currentHealth = data.currentHealth + amount;
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);

        UIEventManager.HealthChanged(data.currentHealth, data.maxHealth);
    }

    // Add mana, but never go above maxMana
    public void RestoreMana(int amount)
    {
        data.currentMana = data.currentMana + amount;
        data.currentMana = Mathf.Clamp(data.currentMana, 0, data.maxMana);

        UIEventManager.ManaChanged(data.currentMana, data.maxMana);
    }

    // Required by IDamagable - take damage the same way anything else in the game does
    public void TakeDamage(int damage)
    {
        data.currentHealth = data.currentHealth - damage;
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);
        
        UIEventManager.HealthChanged(data.currentHealth, data.maxHealth);

        // Broadcast it, exactly like the crates and enemies do.
        // The event logger picks this up without us doing anything else.
        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(gameObject.name, data.currentHealth);
        }

        if (data.currentHealth <= 0)
        {
            Die();
        }
    }

    // Required by IDamagable
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
        if (HealthEventManager.OnObjectDestroyed != null)
        {
            HealthEventManager.OnObjectDestroyed(gameObject.name, data.currentHealth);
        }

        Debug.Log("Player died!");

        // TODO (Week 5) - the GameManager will handle game over and restarting
        
        // Tell the GameManager - it deals with lives and respawning
        GameManager.Instance.PlayerDied();

        Destroy(gameObject);
    }
}