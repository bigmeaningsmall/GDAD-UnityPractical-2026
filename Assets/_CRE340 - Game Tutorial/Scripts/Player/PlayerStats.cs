using UnityEngine;

// This IS a MonoBehaviour, because Unity needs to access it
// it sits on the Player GameObject so that items can find it on collision.
// It is a SEPARATE component from the TwinStickController. - following good practice to sperate classes and components (one handles movement, one handes stats - each script has oine responsibility - See SOLID)
public class PlayerStats : MonoBehaviour
{
    // Here we create an INSTANCE of our plain PlayerData class.
    // 'new PlayerData()' calls the default constructor we wrote in Step 3.
    [SerializeField] private PlayerData data = new PlayerData();

    // A public way for other scripts to read the data (but not replace it)
    public PlayerData Data
    {
        get { return data; }
    }

    // Add health, but never go above maxHealth
    public void RestoreHealth(int amount)
    {
        data.currentHealth = data.currentHealth + amount;

        // Mathf.Clamp keeps a value between a minimum and a maximum
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);

        Debug.Log("Player health is now " + data.currentHealth + " / " + data.maxHealth);
    }

    // Add mana, but never go above maxMana
    public void RestoreMana(int amount)
    {
        data.currentMana = data.currentMana + amount;
        data.currentMana = Mathf.Clamp(data.currentMana, 0, data.maxMana);

        Debug.Log("Player mana is now " + data.currentMana + " / " + data.maxMana);
    }
}