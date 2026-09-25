using UnityEngine;

// Base class Item - every item in our game derives from this
public class Item : MonoBehaviour
{
    // An INSTANCE of our plain ItemData class, holding this item's identity.
    // 'protected' means derived classes can reach it, but unrelated scripts can't.
    [SerializeField] protected ItemData data;

    // ---------- SETUP ----------

    // 'virtual' so derived classes can override it and set their own identity.
    // We use Awake (not Start) because Awake runs the INSTANT an object is created,
    // so our data is ready before anything else tries to read it. - See the 'Unity Lifecycle'
    protected virtual void Awake()
    {
        // Nothing shared here yet - each derived class sets its own identity.
        // TODO - We come back to this in Part 2 and add behaviour that EVERY item gets.
    }

    // ---------- SHARED BEHAVIOUR (inherited as-is, never overridden) ----------

    // Every item detects the player the same way, so is here ONCE.
    // HealthPotion and ManaPotion get this for free - they don't need ontriggers as its in the base class
    private void OnTriggerEnter(Collider other)
    {
        // Only react to the Player (we've already tagged the player) 
        if (other.CompareTag("Player"))
        {
            // Find the stats component on whatever just touched us
            PlayerStats playerStats = other.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                // THIS is the polymorphic call. - see 'polymorphism' meaning 'many forms'
                // We are on a plain 'Item' here, but the version of Use() that runs is the one belonging to the ACTUAL type (HealthPotion, ManaPotion, ...)
                Use(playerStats);

                // Destroy this item now it's been picked up - later we can chain this to eeffects and things..
                Destroy(this.gameObject);
            }
        }
    }

    // A shared method every item has and none of them change - this is  just an example using a simple debugging function
    public void DisplayInfo()
    {
        Debug.Log(data.itemName + " : " + data.description);
    }

    // ---------- SPECIALISED BEHAVIOUR (overridden by each derived class) ----------

    // 'virtual' means: here is a default, but derived classes are allowed to replace it 
    public virtual void Use(PlayerStats player)
    {
        Debug.Log("Used a generic item - it did nothing.");
    }
}