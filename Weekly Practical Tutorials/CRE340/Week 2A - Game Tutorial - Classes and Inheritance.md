---
Order: 2-A
---
# **Week 2 - Part 1 : Items - Classes & Inheritance**

### Overview
#### **What we are building**:

- We'll build an **item system** using **inheritance** and drop it into the twin stick game scene.
- We make a base **`Item`** class and two derived types - **`HealthPotion`** and **`ManaPotion`**.
- We give the **Player** some stats (health and mana) and let it **pick items up** using a trigger.
- We also add a **mouse click** interaction as a second way to click on the same items for debugging.

#### **Why we are doing this**:

- Week 1 was an engine and code refresher. From here we start building an **actual game**, and items are the first system.
- The point of inheritance here is **one method, many behaviours**. We call `Use()` on an `Item`, and what happens depends on _which kind_ of item it actually is. We don't have to check
- This exact `Item` hierarchy is what we grow into an **inventory system** in Week 6 and **save to disk** in Week 9. 
	- By building our class structure early we are anticipating how to extend it later and sticking to an **inheritance** structure with a common base class.

#### **Learning Objectives**:

- **Inheritance**: Share common fields and behaviour in a base class (`Item`).
- **Polymorphism**: Treat every item as an `Item`, but let each type behave differently.
- **Method Overriding**: Replace base behaviour with a specific version using `virtual` and `override`.
- **Composition**: Layer new behaviour onto the Player (stats) without touching the controller we wrote in Week 1.


---
---

# **Part 1: Building the Item Inheritance Structure**

This carries on from Week 1. You should already have a **Player** prefab that moves, aims (rotates), and dashes using the **InputManager**.

We'll start with a new scene, build the class hierarchy, then wire it into the Player.

---

### **Step 1: Scene Setup**

- Import the package from blackboard with the new scene - *Scene-Week2_Items-and-Classes*
- Open the scene - press play to make sure you can move around

---

### **Step 2: MonoBehaviour or Plain Class? (read this before you write any code)**

Everything we've written so far has been a **MonoBehaviour** - a class that inherits from Unity's `MonoBehaviour` and gets attached to a GameObject. That's not the only kind of class you can write, and this week we use both.

The rule of thumb:

- **MonoBehaviour** when **Unity needs to talk to it** - it needs `Update()`, it needs trigger messages, it needs to sit on a GameObject so other scripts can find it.
- **Plain C# class** when it's **just data or logic** - it doesn't need to exist in the scene, it just needs to exist.

For our player: `PlayerStats` needs to be on the GameObject so items can find it when they collide. But the actual numbers don't need to be anywhere in particular. They just need to exist. That can be a plain class.

This also gets us **constructors**, which are the normal way to set a class up in C# but which don't work properly on MonoBehaviours (Unity builds those for us). 

On a plain class, constructors work exactly expect in a programming language.

** ***Note - This is a concept we'll revisit in later weeks, particularly in week 9 when we save the game to a file.***

---

### **Step 3: Create the `PlayerData` Class (a plain C# class)**

First the data - just the numbers, no Unity involved.

1. **Create the PlayerData Script**:
    - In _Assets / _CRE340 / Game Tutorial / Scripts_, create a C# script called `PlayerData`.
    - Replace the code with the following:


```csharp

// NOTE - we dont need 'using UnityEngine;' needed and NO MonoBehaviour.
// This is a plain C# class. It holds data. It never sits on a GameObject by itself.

// [System.Serializable] lets Unity display this class in the Inspector
// (and later, in Week 9, lets us save it to a file)
// This is very handy as it means we can use the regular classes but make it easy to work with in the editor

[System.Serializable] // Take note of this!!! It means the whole class can be exposed in the editor and serialised in a save system later - It'll make make sense later or comment it to see the difference in the editor
public class PlayerData
{
    public int currentHealth;
    public int maxHealth;
    public int currentMana;
    public int maxMana;

    // DEFAULT CONSTRUCTOR - runs when we write 'new PlayerData()'
    // A brand new player, with default starting values
    public PlayerData()
    {
        currentHealth = 50;
        maxHealth = 100;
        currentMana = 20;
        maxMana = 100;
    }

    // PARAMETERISED CONSTRUCTOR - runs when we write 'new PlayerData(80, 40)'
    // Lets us create a player set up with specific values
    public PlayerData(int startingHealth, int startingMana)
    {
        currentHealth = startingHealth;
        maxHealth = 100;
        currentMana = startingMana;
        maxMana = 100;
    }
}

```

**Explanation**:

- A **class** is the template. An **instance** (or object) is an actual thing built from that template. `PlayerData` is the template; the moment we write `new PlayerData()` we get an instance.
- A **constructor** is the method that runs when an instance is created. It's named exactly the same as the class and has no return type.
- We have **two** constructors here. C# picks the right one based on what you pass in - no arguments gets the first, two `ints` gets the second. 
	- This is called **overloading**. We can have multiple functions with the same name that accept different parameters.  

---

### **Step 4: Create the `PlayerStats` Component (a MonoBehaviour)**

Now the component that lives on the Player and owns an instance of that data.

1. **Create the PlayerStats Script**:
    - Create a C# script called `PlayerStats`.
    - Replace the code with the following:

```csharp

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

```

1. **Attach it to the Player**:
    - Select the **Player** and `Add Component -> Player Stats`.
    - In the Inspector you'll see a **Data** dropdown. Open it and you'll find the four values from `PlayerData`.
2. **Apply to the Prefab**:
    - Click **Overrides -> Apply All** at the top of the Inspector so the Player prefab keeps the stats.
    - Alternatively you might open the prefab directly.

**Explanation**:

- **Two classes, two jobs.** 
	- `PlayerStats` is the component Unity creates for us when we add it to a GameObject. 
	- `PlayerData` is an object **we** create with the keyword `new`.  An instance of the template class `PlayerData`
- The data is nested inside the component in the Inspector, so we can see it the same way we would if we just had variables
- When we make a save system in later weeks (Week 9) we'll just serialise yhe `PlayerData` object straight to the save system. That's one of the main reasons we are using a plain class.


> [!Notes on Composition]
> **This (Step 4)  is COMPOSITION.**
> 
> We didn't add health and mana INSIDE the TwinStickController - we added a separate component beside it. 
> 
> **Inheritance** *decides what a thing IS ('a HealthPotion is an Item').* 
> 
> **Composition** *decides what a GameObject HAS ('the Player has a controller AND stats AND a collider').* 
> 
> Unity uses both, and knowing which is best for different systems is something to consider**


---

### **Step 5: Why Inheritance? (the items need a different approach)**

We're making two kinds of potion. We _could_ write two completely separate scripts with no relationship to each other. 

Why not make two scripts? :
- Every item in our game shares the same **core idea**: it has a name, it sits in the world, the player can pick it up, and something happens when they do. Only that last part changes between types.

Inheritance lets us write the shared parts **once** in a base class, and let each specific type fill in only what makes it different:

- `Item` - *the base. What EVERY item is and does.*
- `HealthPotion : Item` - *an Item, plus it restores health.*
- `ManaPotion : Item` - *an Item, plus it restores mana.*

The payoff for using inheritance here: 
- Other scripts only ever need to know about **`Item`**. They call `Use()` and the right thing happens, whether it's a health potion, a mana potion, or something we haven't invented yet. 
- We can add a third, forth, fifth etc... item type later and none of the existing code changes. 

> [!note]
> **Note - Two different structuring tools in Step 4 and Step 5.** 
> 
> *The Player got* **COMPOSITION** (*a separate stats component bolted on*). 
> 
> *The items get* **INHERITANCE** (*a family of types sharing a base*). 
> 
> Neither is 'the right one' - they solve different problems, and a real project uses both. 
> 
> Keep asking: is this a KIND of thing, or does this thing HAVE a part?


---

### **Step 6: Create the `ItemData` Class (a plain C# class)**

Same pattern as the player. The identity of an item is just data, so it gets a plain class.

1. **Create the ItemData Script**:
    - Create a C# script called `ItemData`.
    - Replace the code with the following:

```csharp
// Another plain C# class - no MonoBehaviour, no Unity overhead 
// This holds the shared identity that EVERY item has. - Its the template for an item
[System.Serializable]
public class ItemData
{
    public string itemName;
    public string description;

    // Default constructor - a generic, unnamed item
    public ItemData()
    {
        itemName = "Generic Item";
        description = "A generic item that does things.";
    }

    // Parameterised constructor - each item type stamps its own identity
    public ItemData(string newItemName, string newDescription)
    {
        itemName = newItemName;
        description = newDescription;
    }
}
```

**Explanation**:

- Exactly the same idea as `PlayerData` - a template for data, with two constructors.
- Note we now have a **real use** for the parameterised constructor. 
	- `HealthPotion` will call `new ItemData("Health Potion", "A potion that restores health.")` and get its identity in one line.
- In Week 6 we'll look at making adding this to **inventory** stores, and in Week 9 we'll maybe add it to a save/load file system

---

### **Step 7: Create the Base Class `Item`**

Now the base class - the template every item is built from.

1. **Create the Item Script**:
    - Create a C# script called `Item.cs`.
    - Replace the code with the following:

```csharp
using UnityEngine;

// Base class Item - every item in our game derives from this - It Inherits from Monobehaviour so any derived class gets Mono and Item
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

```

**Explanation**:

- **`protected`** fields can be read and changed by derived classes, but not by unrelated scripts. Halfway between `private` and `public`. Its `protected` within the inheritance structure.
- **`Awake()`** is `virtual` and currently empty. That's deliberate - it gives derived classes something to hook into, and gives us somewhere to add shared behaviour in Part 2. *We could do this now but I've split it to a second tutorial to show how shared behaviour works*.
- **`OnTriggerEnter`** lives in the base class, so **every item type inherits the pickup behaviour** without rewriting it. This is "inherited as-is".
- **`Use()`** is marked **`virtual`**, meaning derived classes can **override** it with their own version. This is "inherited and specialised".

> [!note on Constructors in Monobehaviour]
> 
> **Note - Notice the Item class itself has no constructor.** 
> 
> Unity creates MonoBehaviours for us when we attach them to a GameObject, so constructors on a MonoBehaviour never do what you expect. 
> 
> That's why the constructors went on ItemData and PlayerData instead - plain classes, where 'new' actually runs our code. 
> 
> This is one of the places Unity C# and 'textbook' C# part ways, and it's worth knowing why


---

### **Step 8: Create the `HealthPotion` Class**

1. **Create the HealthPotion Script**:
    - Create a C# script called `HealthPotion.cs`.
    - Replace the code with the following:


```csharp
using UnityEngine;

// Derived class HealthPotion - it IS an Item, plus it restores health
public class HealthPotion : Item // NOTICE WE ARE INHERITING FROM OUR ITEM CLASS
{
    [Header("Health Potion")]
    public int healthRestoreAmount;      // Amount of health this potion restores
    public int minRestoreAmount = 30;    // Minimum restore amount for the random range
    public int maxRestoreAmount = 70;    // Maximum restore amount for the random range

    // 'override' our base class Awake to set up this specific item type
    protected override void Awake()
    {
        base.Awake();   // ALWAYS call the base version first, so shared setup still runs - its a bit of boilerplate overhead to keep things in order

        // Create our identity using ItemData's parameterised constructor
        data = new ItemData("Health Potion", "A potion that restores health.");

        // Roll a random restore amount within the range set in the Inspector
        healthRestoreAmount = Random.Range(minRestoreAmount, maxRestoreAmount);

        Debug.Log("HealthPotion: random restore amount set to " + healthRestoreAmount);
    }

    // 'override' replaces the base class version of Use() with this one
    public override void Use(PlayerStats player)
    {
        Debug.Log("Health Potion used - restoring " + healthRestoreAmount + " health.");
        player.RestoreHealth(healthRestoreAmount);
    }
}
```

**Explanation**:

- `public class HealthPotion : Item` is the inheritance. The `: Item` part means "this IS an Item".
- We set `data` even though we never declared it here - it's **inherited** from `Item`.
- **`base.Awake()`** calls the base class version before the derived version does things. 
	- In Part 2 the base `Awake` will add functionality, and any derived class that doesn't this call will miss the functionality.
- We **never wrote `OnTriggerEnter`** in this class. It came from the base class.
	- Handy stuff! - means we can modify how triggering works in the base without having to change the derived classes.

---

### **Step 9: Create the `ManaPotion` Class**

1. **Create the ManaPotion Script**:
    - Create a C# script called `ManaPotion.cs`.
    - Replace the code with the following:


```csharp
using UnityEngine;

// Derived class ManaPotion - it IS an Item, plus it restores mana
public class ManaPotion : Item // again NOTICE its derived or inherited from 'Item' 
{
    [Header("Mana Potion")]
    public int manaRestoreAmount;       // Amount of mana this potion restores
    public int minRestoreAmount = 20;   // Minimum restore amount for the random range
    public int maxRestoreAmount = 50;   // Maximum restore amount for the random range

    protected override void Awake()
    {
        base.Awake();   // always call the base version first

        // Same constructor, different identity
        data = new ItemData("Mana Potion", "A potion that restores mana.");

        manaRestoreAmount = Random.Range(minRestoreAmount, maxRestoreAmount);

        Debug.Log("ManaPotion: random restore amount set to " + manaRestoreAmount);
    }

    // Same method name, same signature, completely different behaviour
    public override void Use(PlayerStats player)
    {
        Debug.Log("Mana Potion used - restoring " + manaRestoreAmount + " mana.");
        player.RestoreMana(manaRestoreAmount);
    }
}
```

**Explanation**:

- Structurally identical to `HealthPotion`, but `Use()` does something different.
- **This is the payoff! It seems long-winded to setup but there is good reason for it!** 
	- The base `Item` class calls `Use(playerStats)` and has no idea whether it's talking to a health potion or a mana potion. It doesn't need to. Each type knows what it does.
- No `if (item is HealthPotion) ... else if (item is ManaPotion) ...` anywhere. We are completely cutting out the need for conditional logic. 
	- This is what polymorphism avoids. If we had different classes for every item has to have specific logic in how it it uses. Here our functions are pretty much agnostic and generic.

---

### **Step 10: Make the Item Prefabs**

1. **Build the Health Potion**:
    - `GameObject -> 3D Object -> Cube` (or Sphere / or use any 3D object you want). Rename it **HealthPotion**.
    - Scale it down to something pickup-sized (e.g. 0.5, 0.5, 0.5).
    - On its **Collider**, tick **Is Trigger** - this is what makes `OnTriggerEnter` fire instead of the player bumping into it.
    - `Add Component -> Health Potion`.
    - Drag it into _Assets / _CRE340 - Game Tutorial / Prefabs_ to make the prefab, then delete it from the Hierarchy.
2. **Build the Mana Potion**:
    - Same process, named **ManaPotion**, with the **Mana Potion** script attached. Trigger ticked, prefab made, deleted from the scene.

** **Note - Give them different coloured materials so you can tell them apart. We'll automate this properly in Part 2 using the base class.**

---

### **Step 11: Create the `ItemSpawner`**

We'll scatter some items to collect to test. Similar spawner as last weeks example but I want to show an example of uniform arrangement with loops and offset/spacing.

1. **Create the ItemSpawner Script**:
    - Create an empty GameObject called **ItemSpawner** at the origin and attach this script.
    - Create a C# script called `ItemSpawner.cs` and replace the code with the following:


```csharp
using UnityEngine;
using System.Collections.Generic;

// The ItemSpawner places Health and Mana Potions in the scene
public class ItemSpawner : MonoBehaviour
{
    // References to the potion prefabs, to be set in the Unity Inspector
    public GameObject healthPotionPrefab;  // Drag the HealthPotion prefab here
    public GameObject manaPotionPrefab;    // Drag the ManaPotion prefab here

    public int numberOfItemsEachSide = 3;  // Number of items each side of the origin
    public float spacing = 2.0f;           // Distance between each item along the X-axis

    // A list that holds EVERY item we spawn - as the base type 'Item'.
    // A HealthPotion and a ManaPotion can sit side by side in here because both ARE Items.
    private List<Item> allItems = new List<Item>();

    void Start()
    {
        SpawnRow(healthPotionPrefab, 0f);     // health potions along Z = 0
        SpawnRow(manaPotionPrefab, -4.0f);    // mana potions along Z = -4 to separate them visually

        // Just to test the item inheritance - loop through every item, whatever type it is, and call the same method
        DisplayAllItems();
    }

    // One spawn method handles BOTH prefabs, because both are Items
    void SpawnRow(GameObject itemPrefab, float zPosition)
    {
        for (int i = -numberOfItemsEachSide; i <= numberOfItemsEachSide; i++)
        {
            // Work out the position along the X-axis
            Vector3 position = new Vector3(i * spacing, 0.5f, zPosition);

            // Instantiate takes 3 parameters (GameObject, position, rotation)
            GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);

            // Grab the Item component - this works for ANY item type
            Item itemComponent = newItem.GetComponent<Item>();

            if (itemComponent != null)
            {
                allItems.Add(itemComponent); // store it in our list of Items - we don't need to save a list but I want to show what a list is.
            }
            else
            {
                Debug.LogWarning("The spawned object does not have an Item component!");
            }
        }
    }

    // Loop through the list and call the same method on every item - this is just to test calling the item function to show it doesn't care what the item is as long as its base class is item with the DisplayInfo function
    void DisplayAllItems()
    {
        Debug.Log("--- Listing all spawned items ---");

        foreach (Item item in allItems)
        {
            item.DisplayInfo();
        }
    }
}
```

2. **Set it up in the Inspector**:
    - Drag the **HealthPotion** prefab into `Health Potion Prefab`, and **ManaPotion** into `Mana Potion Prefab`.

**Explanation**:

- The spawner holds everything in a **`List<Item>`**. It doesn't keep separate lists per type, and it doesn't care what's in there.
- `SpawnRow()` takes any prefab with an `Item` on it - one method, both potion types.

> [!note]
> 
> **Note - This is why the item identity is set in Awake and not Start.** 
> 
> Awake runs the moment Instantiate() creates the object, so by the time DisplayAllItems() runs, every item already knows its name. 
> 
> Had we used Start, it wouldn't have run yet and we'd be reading data that doesn't exist. *Execution order matters*, and this is a very common source of null reference errors. (*See Unity Lifecycle for order of functions*)




---

### **Step 12: Add a Click Interaction (a second way to interact)**

Triggers are how the player _collects_ items. Let's also add clicking as a way to _inspect_ them - useful for debugging, and it shows the same class hierarchy being used from a completely different direction.

1. **Create the ItemClick Script**:
    - Create a C# script called `ItemClick.cs` and attach it to the **Main Camera**.
    - Replace the code with the following:


```csharp
using UnityEngine;

// The ItemClick script lets us inspect items by clicking on them
public class ItemClick : MonoBehaviour
{
    void Update()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits any collider in the scene
            if (Physics.Raycast(ray, out hit))
            {
                // Try to get an Item component from whatever we hit.
                // Note we ask for 'Item', not 'HealthPotion' - so this works for every item type.
                Item clickedItem = hit.transform.GetComponent<Item>();

                if (clickedItem != null)
                {
                    clickedItem.DisplayInfo();
                }
            }
        }
    }
}
```

**Explanation**:

- **Raycasting**: we fire a ray from the camera through the mouse position and see what it hits.
- We *get* the **base type** `Item`, so a single script handles every item type we will ever make. Again this is an advantage of using base classes
- Two different interactions - moving into an item, clicking an item - both work off the same hierarchy without either knowing about the other.

> [!note]
> **Note - This uses the OLD input system (`Input.GetMouseButtonDown`) rather than our InputManager.**
> 
> *It's only here as a quick demo of raycasting and will be depreciated in the future years. The gameplay interaction that matters is the trigger.*


---

### **Step 13: Test It**

1. **Press Play**:
    - Watch the Console - each potion logs its random restore amount, then the spawner lists them all.
    - Move the Player into a **Health Potion**. The Console reports health restored, the potion disappears.
    - Move into a **Mana Potion**. Mana goes up instead. **Same code path for different functionality!**
    - Click on an item with the mouse to print its info without picking it up.
2. **Watch the numbers**:
    - Select the **Player** while playing and open the **Data** dropdown in the **Player Stats** component.
    - See `currentHealth` and `currentMana` increase as you collect.
    - Collect past the max and confirm the `Mathf.Clamp` holds them at 100.

---

### **What we've set up**:

- Two **plain C# classes** - `PlayerData` and `ItemData` - with constructors, created with `new`.
- Two **MonoBehaviours** that own them - `PlayerStats` (composition, on the Player) and `Item` (a base class for inheritance).
- Two derived types, **`HealthPotion`** and **`ManaPotion`**, each overriding `Use()` with its own behaviour.
- A spawner holding everything as a **`List<Item>`** and treating them all the same.

### **The ideas to take away**:

1. **Class vs instance.** `PlayerData` is a template. `new PlayerData()` is a thing. 
	- Unity makes our MonoBehaviour instances for us.
	- We make the plain ones ourselves.
2. **MonoBehaviour vs plain class.** 
	- If Unity needs to talk to it, it's a MonoBehaviour. 
	- If it's just data, it doesn't need MonoBehaviour.
3. **Polymorphism.** 
	- `Item` was called. `HealthPotion` answered. 
		- Nothing in the base class, the spawner, or the click script needed to know the difference.

In **Part 2** we go back into the base class and add new behaviour there. You'll see its very powerful to be able to modify a base without changing individual classes. You can affect huge amounts of gameobjects with base script changes. Great stuff!!!

### This was a long tutorial but its important to start to get an idea about how and why you might use base classes or inheritance. Its a bit of a long-winded setup but its pays off long term if we have a lot of items or variation of types in our game