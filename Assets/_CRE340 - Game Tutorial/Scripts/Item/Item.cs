using System.Collections;
using UnityEngine;

// Base class Item - every item in our game derives from this - It Inherits from Monobehaviour so any derived class gets Mono and Item
public abstract class Item : MonoBehaviour  
{
    // An INSTANCE of our plain ItemData class, holding this item's identity.
    // 'protected' means derived classes can reach it, but unrelated scripts can't.
    [SerializeField] protected ItemData data;
    
    [Header("Item Appearance")]
    [SerializeField] protected Color itemColor = Color.white; // set per-prefab in the Inspector
    
    [Header("Item Movement")]
    [SerializeField] protected float rotationSpeed = 100f; // degrees per second
    
    // ---------- SETUP ----------

    // 'virtual' so derived classes can override it and set their own identity.
    // We use Awake (not Start) because Awake runs the INSTANT an object is created,
    // so our data is ready before anything else tries to read it. - See the 'Unity Lifecycle'
    protected virtual void Awake()
    {
        Renderer itemRenderer = GetComponent<Renderer>(); // get the renderer component 

        if (itemRenderer != null)
        {
            itemRenderer.material.color = itemColor; // access the material and set the colour
        }
    }
    
    // Every item rotates - defined ONCE here, inherited by every item type - notice the protected keyword so derived classes can still reach it, but unrelated scripts can't.
    protected virtual void Update()
    {
        // Rotate slowly around the Y-axis 
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime); // not the most efficient way to rotate or use Update(), but its simple and works for this example
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

    // OLD USE METHOD BEFORE ABSTRACTION - now we have an abstract method below so every derived class MUST implement its own Use() method
    // // 'virtual' means: here is a default, but derived classes are allowed to replace it 
    // public virtual void Use(PlayerStats player)
    // {
    //     Debug.Log("Used a generic item - it did nothing.");
    // }
    
    // 'abstract' method - no body at all. Every derived class MUST provide its own Use().
    public abstract void Use(PlayerStats player);
    
    // ---------- More SPECIALISED BEHAVIOUR (Click example - OK to put the bool here as its just an example - normally I would keep the variables at the top of the class) ----------
    
    // To prevent multiple clicks during the pulse effect
    private bool canClick = true;

    // OnMouseDown is a built-in Unity message - it fires when this object's collider is clicked
    protected virtual void OnMouseDown()
    {
        if (canClick)
        {
            canClick = false;
            StartCoroutine(PulseEffect());
        }
    }

    // Coroutine to handle the pulse effect - runs across several frames
    private IEnumerator PulseEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;    // scale up
        
        yield return new WaitForSeconds(0.2f);          // wait, without freezing the game
        
        transform.localScale = originalScale;           // scale back down
        canClick = true;                                // re-enable clicking
    }
}