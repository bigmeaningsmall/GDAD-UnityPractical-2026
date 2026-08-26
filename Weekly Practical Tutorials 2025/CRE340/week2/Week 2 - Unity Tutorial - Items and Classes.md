### Simple Unity OOP Demo Concept

1. **Scene Setup:**
   - Create a Unity scene with a simple plane and two interactable 3D objects (e.g., cubes or spheres) representing the items.

2. **Classes and Inheritance:**
   - **Base Class**: `Item` (inherits from `MonoBehaviour`)
     - Contains common properties like `ItemName` and `Description`.
     - Includes a `DisplayInfo()` method that logs information to the console.

   - **Derived Classes**:
     - **HealthPotion**: Inherits from `Item`
       - Additional property: `HealthRestoreAmount`.
       - Overrides `DisplayInfo()` to show specific health-related information.
       
     - **ManaPotion**: Inherits from `Item`
       - Additional property: `ManaRestoreAmount`.
       - Overrides `DisplayInfo()` to show specific mana-related information.

3. **Player Interaction:**
   - Implement a `PlayerInteraction` script to detect when the player clicks on an item using a raycast.
   - Call the `DisplayInfo()` method of the clicked item, printing the details to the console.

4. **Visual Representation:**
   - Attach `HealthPotion` and `ManaPotion` scripts to the 3D objects in the scene.
   - Label the objects with their respective names (e.g., "Health Potion", "Mana Potion").

- **Objectives:**
   1. Demonstrate OOP concepts like **inheritance** and **polymorphism** by allowing interaction with the derived classes (`HealthPotion` and `ManaPotion`), which all share a common interface through the `Item` base class.
   2. This exercise is a simple example to show how you might use Classes and Objects in OOP 
      - `Item : HealthPotion`
      - `Item : ManaPotion`
      - Consider how you might extend this or use a similar structure in a game


---

# Documentation for Item, HealthPotion, and ManaPotion Classes

This tutorial will guide you through setting up a simple class hierarchy in Unity using inheritance and polymorphism. The goal is to create a base class called `Item` and derive `HealthPotion` and `ManaPotion` classes from it. We will use these classes to demonstrate the use of constructors, method overriding, and adding randomised properties for each potion type.

---

## Step 1: Create the Base Class `Item`

**Instructions:**
1. Create a new C# script called `Item.cs`.
2. Replace the default code with the following `Item` class definition:

```csharp
using UnityEngine;

// Base class Item with virtual DisplayInfo method
public class Item : MonoBehaviour
{
    protected string itemName;      // Name of the item (accessible to derived classes)
    protected string description;   // Description of the item (accessible to derived classes)

    // Default constructor for Item, called when an Item object is created
    public Item()
    {
        itemName = "Generic Item";
        description = "A generic item.";
        Debug.Log("1st Item Constructor Called");
    }

    // Constructor with parameters, allows setting name and description during instantiation
    public Item(string newItemName, string newDescription)
    {
        itemName = newItemName;
        description = newDescription;
        Debug.Log("2nd Item Constructor Called");
    }

    // Virtual method to be overridden in derived classes
    public virtual void DisplayInfo()
    {
        Debug.Log($"{itemName}: {description}");
    }

    // A simple method to greet
    public void SayHello()
    {
        Debug.Log("Hello, I am an item.");
    }
}
```

**Explanation:**
- This `Item` class is the base class for all items in your game.
- It contains:
  - Two protected 'fields' (*or variables*): `itemName` and `description`, which can be accessed and modified by derived classes.
  - A default constructor that assigns generic values to these fields.
  - A parameterised constructor to customise `itemName` and `description` upon creation.
  - A `DisplayInfo()` method marked as `virtual`, which allows derived classes to override it with specific implementations.
  - A simple `SayHello()` method for demonstration purposes.
    - We just debug to the console

---

## Step 2: Create the `HealthPotion` Class

**Instructions:**
1. Create a new C# script called `HealthPotion.cs`.
2. Replace the default code with the following class definition:

```csharp
using UnityEngine;

// Derived class HealthPotion that inherits from Item
public class HealthPotion : Item
{
    public int healthRestoreAmount;      // Amount of health this potion restores
    public int minRestoreAmount = 30;    // Minimum restore amount for random range
    public int maxRestoreAmount = 70;    // Maximum restore amount for random range

    // Default constructor, sets the name and description of the health potion
    public HealthPotion()
    {
        itemName = "Health Potion";
        description = "A potion that restores health.";
    }

    // Called when the object is instantiated
    private void Start()
    {
        // Assign a random value for healthRestoreAmount within the specified range
        healthRestoreAmount = Random.Range(minRestoreAmount, maxRestoreAmount);
        Debug.Log($"HealthPotion: Random restore amount set to {healthRestoreAmount}.");
    }

    // Override method to display specific health potion info
    public override void DisplayInfo()
    {
        Debug.Log($"{itemName}: Restores {healthRestoreAmount} health points.");
    }
}
```

**Explanation:**
- This class inherits from `Item` and represents a specific type of item, the health potion.
- **Fields**: `healthRestoreAmount`, `minRestoreAmount`, and `maxRestoreAmount` define the restore amount for the potion.
- **Constructors**: The default constructor assigns default values for `itemName` and `description`.
- **Start Method**: Called when the object is instantiated in Unity, it assigns a random `healthRestoreAmount` using `Random.Range(minRestoreAmount, maxRestoreAmount)`.
- **Override Method**: `DisplayInfo()` is overridden to provide specific information about the health potion, including the randomised restore amount.

---

## Step 3: Create the `ManaPotion` Class

**Instructions:**
1. Create a new C# script called `ManaPotion.cs`.
2. Replace the default code with the following class definition:

```csharp
using UnityEngine;

// Derived class ManaPotion that inherits from Item
public class ManaPotion : Item
{
    public int manaRestoreAmount;       // Amount of mana this potion restores
    public int minRestoreAmount = 20;   // Minimum restore amount for random range
    public int maxRestoreAmount = 50;   // Maximum restore amount for random range

    // Default constructor, sets the name and description of the mana potion
    public ManaPotion()
    {
        itemName = "Mana Potion";
        description = "A potion that restores mana.";
    }

    // Called when the object is instantiated
    private void Start()
    {
        // Assign a random value for manaRestoreAmount within the specified range
        manaRestoreAmount = Random.Range(minRestoreAmount, maxRestoreAmount);
        Debug.Log($"ManaPotion: Random restore amount set to {manaRestoreAmount}.");
    }

    // Override method to display specific mana potion info
    public override void DisplayInfo()
    {
        Debug.Log($"{itemName}: Restores {manaRestoreAmount} mana points.");
    }
}
```

**Explanation:**
- This class is similar to `HealthPotion` but represents a mana potion.
- **Fields**: `manaRestoreAmount`, `minRestoreAmount`, and `maxRestoreAmount` define the restore amount for mana.
- **Constructors and Methods**: Functions similarly to `HealthPotion`, with a randomised restore amount set during the `Start()` method.

---

## Summary
1. **Item Base Class**:
   - Acts as a generic template for all items.
   - Contains a `DisplayInfo()` method that can be overridden by derived classes.

2. **HealthPotion Derived Class**:
   - Inherits from `Item`.
   - Includes a randomised `healthRestoreAmount` that is displayed using the overridden `DisplayInfo()` method.

3. **ManaPotion Derived Class**:
   - Inherits from `Item`.
   - Includes a randomised `manaRestoreAmount` that is displayed using the overridden `DisplayInfo()` method.

---



## We will now 'Instantiate' the items using an 'ItemSpawner' script 

This tutorial will guide you through setting up a basic `ItemSpawner` and `ItemClick` system in Unity. The goal is to create a spawner that places items in the scene and a script that lets you interact with them by clicking. These scripts will allow you to instantiate `HealthPotion` and `ManaPotion` prefabs and display their details when clicked.

---

## Step 1: Create the `ItemSpawner` Script

**Instructions:**
1. Create a new C# script called `ItemSpawner.cs`.
2. Replace the default code with the following `ItemSpawner` class definition:

```csharp
using UnityEngine;

// The ItemSpawner script is responsible for spawning Health and Mana Potions in the scene
public class ItemSpawner : MonoBehaviour
{
    // References to the potion prefabs, to be set in the Unity Inspector
    public GameObject healthPotionPrefab;  // Drag HealthPotion prefab here in the Inspector
    public GameObject manaPotionPrefab;    // Drag ManaPotion prefab here in the Inspector

    public int numberOfItemsEachSide = 3;  // Number of items on each side of the origin (total of 6 items)
    public float spacing = 2.0f;           // Distance between each item along the X-axis

    // This method is called when the scene starts
    void Start()
    {
        SpawnHealthPotions();  // Spawn the health potions along the X-axis
        SpawnManaPotions();    // Spawn the mana potions along the X-axis
    }

    // Spawns HealthPotion instances along the X-axis, centered around the origin
    void SpawnHealthPotions()
    {
        for (int i = -numberOfItemsEachSide; i <= numberOfItemsEachSide; i++)
        {
            // Calculate the position of each potion along the X-axis with Y = -0.5 and Z = 0
            Vector3 position = new Vector3(i * spacing, -0.5f, 0);

            // Instantiate the HealthPotion prefab at the calculated position
            GameObject newHealthPotion = Instantiate(healthPotionPrefab, position, Quaternion.identity);

            // Check if the HealthPotion script is correctly attached to the instantiated prefab
            HealthPotion healthPotionItem = newHealthPotion.GetComponent<HealthPotion>();
            if (healthPotionItem != null)
            {
                // Display information about the health potion in the Console
                healthPotionItem.DisplayInfo();
            }
            else
            {
                Debug.LogWarning("The instantiated health potion does not have the HealthPotion component!");
            }
        }
    }

    // Spawns ManaPotion instances along the X-axis, centered around the origin
    void SpawnManaPotions()
    {
        for (int i = -numberOfItemsEachSide; i <= numberOfItemsEachSide; i++)
        {
            // Calculate the position of each potion along the X-axis with Y = -0.5 and Z = -4.0 (to separate them visually)
            Vector3 position = new Vector3(i * spacing, -0.5f, -4.0f);

            // Instantiate the ManaPotion prefab at the calculated position
            GameObject newManaPotion = Instantiate(manaPotionPrefab, position, Quaternion.identity);

            // Check if the ManaPotion script is correctly attached to the instantiated prefab
            ManaPotion manaPotionItem = newManaPotion.GetComponent<ManaPotion>();
            if (manaPotionItem != null)
            {
                // Display information about the mana potion in the Console
                manaPotionItem.DisplayInfo();
            }
            else
            {
                Debug.LogWarning("The instantiated mana potion does not have the ManaPotion component!");
            }
        }
    }
}
```

**Explanation:**
- **References to Prefabs**: The script requires references to the `HealthPotion` and `ManaPotion` prefabs. These references must be assigned in the Unity Inspector.
- **Spawning Logic**: The script spawns `numberOfItemsEachSide` items on both the positive and negative X-axis, centered around the origin `(0, -0.5, 0)`.
  - `SpawnHealthPotions()`: Places health potions along the X-axis at Z = 0.
  - `SpawnManaPotions()`: Places mana potions along the X-axis at Z = -4.0 to separate them visually.

### Step 2: Setting Up the Prefabs in Unity
1. **Create Prefabs**: Make sure you have `HealthPotion` and `ManaPotion` prefabs with their respective scripts (`HealthPotion` and `ManaPotion`) attached.
2. **Assign Prefabs**: In the Unity Inspector, assign these prefabs to the `healthPotionPrefab` and `manaPotionPrefab` fields in the `ItemSpawner` script.

---

## Step 3: Create the `ItemClick` Script

**Instructions:**
1. Create a new C# script called `ItemClick.cs`.
2. Replace the default code with the following `ItemClick` class definition:

```csharp
using UnityEngine;

// The ItemClick script allows interaction with items by clicking on them
public class ItemClick : MonoBehaviour
{
    // Update is called once per frame
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
                // Attempt to get the Item component from the hit object
                Item clickedItem = hit.transform.GetComponent<Item>();

                // If an Item component is found, interact with it
                if (clickedItem != null)
                {
                    // Call the DisplayInfo method of the clicked item
                    clickedItem.DisplayInfo();
                    
                    // Call the SayHello method of the clicked item
                    clickedItem.SayHello();
                }
            }
        }
    }
}
```

**Explanation:**
- **Raycasting**: When the player clicks, the script sends a ray from the camera to the point where the mouse is clicked.
- **Component Detection**: The script checks if the object hit by the ray has an `Item` component attached.
- **Interacting with the Item**:
  - Calls the `DisplayInfo()` method, which shows specific information based on whether it’s a `HealthPotion` or `ManaPotion`.
  - Calls the `SayHello()` method, which is a generic method for all items.

### Step 4: Setting Up the Scene in Unity
1. **Attach the `ItemClick` Script**:
   - Attach `ItemClick` to the **Main Camera** or an empty GameObject in your scene.
2. **Test the Setup**:
   - Click on any of the spawned potions in the scene, and check the console for the output.
   - You should see the specific information for each item based on the overridden `DisplayInfo()` method in `HealthPotion` or `ManaPotion`.

---

## Summary - OOP and Inheritance

- **ItemSpawner**: Automatically places `HealthPotion` and `ManaPotion` prefabs into the scene.
- **ItemClick**: Lets the player click on items to reveal their details and run shared or overridden methods.

This exercise shows how a simple Unity project can demonstrate key OOP ideas:

- **Constructors** – set up default values for your objects.
- **Inheritance** – share common fields and methods in a base class (`Item`).
- **Polymorphism** – treat all items the same (`Item`) while letting each behave differently (`HealthPotion`, `ManaPotion`).
- **Method Overriding** – replace base behaviour with a more specific version in a subclass.

Together, this gives you a small but practical example of how **object-oriented structure** supports flexible and reusable systems in game development.


---
---

# BONUS STUFF! - Extending the base functionality


## Extension: Adding Visual Behaviours to Items

So far, our items demonstrate OOP concepts through inheritance, constructors, and method overriding, with feedback appearing in the Console. To make the example feel more like a game, we can also give our items some **visual behaviours**.

This shows how:
	Shared code in the base class (`Item`) can drive common behaviours, while derived classes (`HealthPotion`, `ManaPotion`) can still customise or override them. 
	To connect OOP design with in-game feedback.

### 1: Spinning Items (Base Class Behaviour)

Add this to the `Item` class:

```csharp
protected float rotationSpeed = 100f;

private void Update()
{
    // Rotate every item slowly around the Y-axis
    transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
}
```

- All items will now rotate.
- You can override the behaviour in a subclass to change rotation speed or direction.

---
### Option 2: Pulse Effect on Click (Interaction Feedback on Base Class)

Inside `Item`, add some functionality for mouseclick :

```csharp
// To prevent multiple clicks during the pulse effect  
private bool canClick = true;  
  
// Simple visual feedback for all items  
protected virtual void OnMouseDown()  
{  
    // Pulse scale effect when clicked    
	if (canClick)
	{  
        canClick = false;  
        StartCoroutine(PulseEffect());    
    }  
}  
// Coroutine to handle the pulse effect  
private IEnumerator PulseEffect()  
{  
    Vector3 originalScale = transform.localScale;  
    transform.localScale = originalScale * 1.2f;  
    yield return new WaitForSeconds(0.2f);  
    transform.localScale = originalScale;  
    canClick = true; // Re-enable clicking after the effect  
}
```

- When clicked, the item briefly scales up and down.
- Demonstrates adding interaction-driven feedback to the base item class 

---

### Option 3: Visual Differentiation by Subclass - Custom Colours

Give each subclass a unique look that is derived from a base colour field in `Start()`:

- Add this protected serialised field to the base `Item` class

```C#
[SerializeField] protected Color itemColor = Color.white; // Color of the item, set in Inspector on the interited classes
```

- Notice it can now be set in the Item prefabs (`HealthPotion`, `ManaPotion`)  that derive from the base `Item` class

- Add this to `Start` method of each class to se the colour

```csharp
// In HealthPotion.cs

// set the color of the mana potion to use the itemColor from the base class  
GetComponent<Renderer>().material.color = itemColor;

// In ManaPotion.cs

// set the color of the mana potion to use the itemColor from the base class  
GetComponent<Renderer>().material.color = itemColor;
```

- Each derived class defines its own visual style.
- Reinforces polymorphism: same base class, different results.

---

### Extending the Example

- **Connects OOP to gameplay**: we can affect changes to gameplay and behaviour via the base class that translates across all items or gameobjects
- **Shows flexibility**: base class behaviours can be shared or overridden easily.
- **Experimentation**: think about how you might use this kind of design to support gameplay or a game framework

---

#### ⚠️ **Note**: This example is based on _inheritance_. It’s a powerful tool, but Unity also leans heavily on **composition** (attaching components to GameObjects) and other patterns like **ScriptableObjects** for data-driven design. There are often multiple valid ways to structure a system, and we’ll explore these alternatives as the module progresses.